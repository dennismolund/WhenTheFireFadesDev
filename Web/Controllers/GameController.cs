using Application.Interfaces;
using Application.Services;
using Domain.Enums;
using Domain.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Web.Helpers;
using Web.Hubs;
using Web.ViewModels;

namespace Web.Controllers;

public class GameController(
    GameOrchestrator gameOrchestrator, 
    IGameRepository gameRepository, 
    IGamePlayerRepository gamePlayerRepository,
    IRoundRepository roundRepository,
    SessionHelper sessionHelper, 
    IHubContext<GameHub> hubContext) : Controller
{

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create()
    {
        var tempUserId = sessionHelper.GetOrCreateTempUserId();
        var authenticatedUserId = sessionHelper.GetAuthenticatedUserId();
        var authenticatedName = sessionHelper.GetAuthenticatedUserName();

        var game = await gameOrchestrator.CreateGameAsync();
        await gameOrchestrator.CreateGamePlayerAsync(game, tempUserId, authenticatedName, authenticatedUserId);
        
        sessionHelper.SetCurrentGameCode(game.ConnectionCode);
        return RedirectToAction(nameof(Lobby), new { code = game.ConnectionCode });
    }
    
    [HttpGet]
    public async Task<IActionResult> Lobby(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest("Code is required.");
        }

        code = code.Trim().ToUpperInvariant();

        var game = await gameRepository.GetByCodeWithPlayersAsync(code);
        if (game == null)
        {
            return NotFound();
        }

        if (game.IsFinished())
        {
            sessionHelper.ClearCurrentGameCode();
            return RedirectToAction(nameof(Index), "Home");
        }

        var tempUserId = sessionHelper.GetOrCreateTempUserId();
        var authenticatedUserId = sessionHelper.GetAuthenticatedUserId();
        var authenticatedName = sessionHelper.GetAuthenticatedUserName();        
        
        var existingPlayer = game.Players.FindByTempUserId(tempUserId);
        
        if (existingPlayer == null)
        {
            await gameOrchestrator.CreateGamePlayerAsync(game, tempUserId, authenticatedName, authenticatedUserId);
            game = await gameRepository.GetByCodeWithPlayersAsync(code);
        }
        
        if (game == null)
        {
            return NotFound();
        }
        
        var viewModel = new LobbyViewModel
        {
            ConnectionCode = code,
            Game = game,
            CurrentPlayer = game.Players.FindByTempUserId(tempUserId)!,            
            PlayerCount = game.Players.Count,
        };

        ViewBag.TempUserId = tempUserId;
        ViewBag.PlayerNickname = sessionHelper.GetPlayerNickname();
        ViewBag.GameCode = code;

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LeaveGameLobby(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Code is required.");

        code = code.Trim().ToUpperInvariant();

        var game = await gameRepository.GetByCodeWithPlayersAsync(code);
        if (game == null)
        {
            return NotFound();
        }

        var tempUserId = sessionHelper.GetTempUserId();
        if (tempUserId == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var player = game.Players.FindByTempUserId(tempUserId);
        if (player != null)
        {
            gamePlayerRepository.RemovePlayer(player);
            await gamePlayerRepository.SaveChangesAsync();

            game = await gameRepository.GetByCodeWithPlayersAsync(code);
            if (game == null)
            {
                return NotFound();
            }
            
            await hubContext.Clients.Group(code).SendAsync("PlayerLeft", new
            {
                leftUserId = tempUserId,
                players = game.Players.Select(p => new
                {
                    p.TempUserId,
                    p.Nickname,
                    p.Seat,
                    p.IsConnected
                }).ToList(),
                totalPlayers = game.Players.Count
            });
        }
        
        sessionHelper.ClearCurrentGameCode();
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartGame(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Code is required.");

        code = code.Trim().ToUpperInvariant();

        var game = await gameRepository.GetByCodeWithPlayersAsync(code);
        if (game == null)
        {
            return NotFound();
        }

        var tempUserId = sessionHelper.GetTempUserId();
        if (tempUserId == null)
        {
            return RedirectToAction("Index", "Home");
        }

        // Start the game
        await gameOrchestrator.StartGameAsync(game);

        await hubContext.Clients.Group(code).SendAsync("GameStarted", new 
        {
            code,
            roundNumber = game.RoundCounter,
            leaderSeat = game.LeaderSeat
        });

        return RedirectToAction(nameof(Play), new { code });
    }

    [HttpGet]
    public async Task<IActionResult> Play(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Code is required.");
        code = code.Trim().ToUpperInvariant();

        var game = await gameRepository.GetByCodeWithPlayersAndRoundsAsync(code);
        if (game == null)
        {
            return NotFound();
        }
        if (game.IsInLobby())
        {
            return RedirectToAction(nameof(Lobby), new { code });
        }

        var tempUserId = sessionHelper.GetTempUserId();
        if (tempUserId == null)
        {
            return RedirectToAction("Index", "Home");
        }
        var currentPlayer = game.Players.FindByTempUserId(tempUserId);
        if (currentPlayer == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var currentRound = await roundRepository.GetCurrentRoundSnapshot(game.GameId, game.RoundCounter);

        var currentLeader = game.Players.FindBySeat(game.LeaderSeat)!;

        var viewModel = new PlayViewModel
        {
            Game = game,
            CurrentPlayer = currentPlayer,
            CurrentRound = currentRound,
            CurrentLeader = currentLeader
        };

        if (currentRound.Status is not (RoundStatus.VoteOnTeam or RoundStatus.SecretChoices)) return View(viewModel);
        

        var activeProposal = currentRound.GetActiveTeam();
        if (activeProposal == null) return View(viewModel);
        
        viewModel.ActiveTeam = activeProposal;

        // Get team members from proposal
        var proposedSeats = activeProposal.Members.Select(m => m.Seat).ToList();
        viewModel.ProposedTeamMembers = game.Players
            .Where(p => proposedSeats.Contains(p.Seat))
            .OrderBy(p => p.Seat)
            .ToList();

        // Get votes if in voting phase
        if (currentRound.IsVotingPhase())
        {
            viewModel.TeamVotes = activeProposal.Votes.ToList();
            viewModel.HasCurrentPlayerVoted = activeProposal.Votes.HasPlayerVoted(currentPlayer.Seat);

        }
        // Get mission votes if in mission phase
        else if (currentRound.IsMissionPhase())
        {
            viewModel.MissionVotes = currentRound.MissionVotes.ToList();
            viewModel.HasCurrentPlayerVoted = currentRound.MissionVotes.HasPlayerVoted(currentPlayer.Seat);
        }
        
        return View(viewModel);
    }
}
