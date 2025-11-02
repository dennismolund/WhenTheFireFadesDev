using Application.Interfaces;
using Application.Services;
using Domain.Enums;
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

        if (game.Status == GameStatus.Finished)
        {
            sessionHelper.ClearCurrentGameCode();
            return RedirectToAction(nameof(Index), "Home");
        }

        var tempUserId = sessionHelper.GetOrCreateTempUserId();
        var authenticatedUserId = sessionHelper.GetAuthenticatedUserId();
        var authenticatedName = sessionHelper.GetAuthenticatedUserName();        
        
        var existingPlayer = game.Players.SingleOrDefault(p => p.TempUserId == tempUserId);
        
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
            CurrentPlayer = game.Players.First(p => p.TempUserId == tempUserId),
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

        var player = game.Players.FirstOrDefault(p => p.TempUserId == tempUserId);
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
        if (game.Status != GameStatus.InProgress)
        {
            return RedirectToAction(nameof(Lobby), new { code });
        }

        var tempUserId = sessionHelper.GetTempUserId();
        if (tempUserId == null)
        {
            return RedirectToAction("Index", "Home");
        }
        var currentPlayer = game.Players.FirstOrDefault(p => p.TempUserId == tempUserId);
        if (currentPlayer == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var currentRound = await roundRepository.GetCurrentRoundSnapshot(game.GameId, game.RoundCounter);

        var currentLeader = game.Players.First(p => p.Seat == game.LeaderSeat);

        var viewModel = new PlayViewModel
        {
            Game = game,
            CurrentPlayer = currentPlayer,
            CurrentRound = currentRound,
            CurrentLeader = currentLeader
        };

        if (currentRound.Status == RoundStatus.VoteOnTeam || currentRound.Status == RoundStatus.SecretChoices)
        {
            var activeProposal = currentRound.Teams.FirstOrDefault(tp => tp.IsActive);
            if (activeProposal != null)
            {
                viewModel.ActiveTeam = activeProposal;

                // Get team members from proposal
                var proposedSeats = activeProposal.Members.Select(m => m.Seat).ToList();
                viewModel.ProposedTeamMembers = game.Players
                    .Where(p => proposedSeats.Contains(p.Seat))
                    .OrderBy(p => p.Seat)
                    .ToList();

                // Get votes if in voting phase
                if (currentRound.Status == RoundStatus.VoteOnTeam)
                {
                    viewModel.TeamVotes = activeProposal.Votes.ToList();
                    viewModel.HasCurrentPlayerVoted = activeProposal.Votes
                        .Any(v => v.Seat == currentPlayer.Seat);
                }
                // Get mission votes if in mission phase
                else if (currentRound.Status == RoundStatus.SecretChoices)
                {
                    viewModel.MissionVotes = currentRound.MissionVotes.ToList();
                    viewModel.HasCurrentPlayerVoted = currentRound.MissionVotes
                        .Any(v => v.Seat == currentPlayer.Seat);
                }
            }
        }
        return View(viewModel);
    }
}
