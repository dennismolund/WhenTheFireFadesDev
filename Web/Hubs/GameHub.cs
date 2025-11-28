using Domain.Extensions;
namespace Web.Hubs;

using Application.Interfaces;
using Application.Services;
using Domain.Entities;  
using Domain.Enums;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Helpers;

public class GameHub(
    IGameRepository gameRepository,
    IGamePlayerRepository gamePlayerRepository,
    IRoundRepository roundRepository,
    ITeamRepository teamRepository,
    ITeamVoteRepository teamVoteRepository,
    IMissionVoteRepository missionVoteRepository,
    GameOrchestrator gameOrchestrator,
    SessionHelper sessionHelper) : Hub
{
    public async Task JoinGameLobby(string gameCode)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);

        var game = await gameRepository.GetByCodeWithPlayersAsync(gameCode);
        if (game == null) return;
        
        await Clients.Group(gameCode).SendAsync("PlayerJoined", new
        {
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
    public async Task JoinGame(string gameCode)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
    }
    
    public async Task ProposeTeam(string gameCode, List<int> selectedSeats)
    {
        if (string.IsNullOrWhiteSpace(gameCode))
            return;

        gameCode = gameCode.Trim().ToUpperInvariant();

        var game = await gameRepository.GetByCodeWithPlayersAndRoundsAsync(gameCode);
        if (game == null)
            return;

        var tempUserId = sessionHelper.GetTempUserId();
        if (tempUserId == null)
            return;

        var leader = game.Players.FindBySeat(game.LeaderSeat);
        if (leader == null || leader.TempUserId != tempUserId)
            return;

        var currentRound = game.GetCurrentRound();
        if (currentRound == null)
            return;

        if (selectedSeats.Count != currentRound.TeamSize)
        {
            return;
        }

        var team = new Team
        {
            RoundId = currentRound.RoundId,
            IsActive = true,
            Round = currentRound,
            Members = selectedSeats.Select(seat => new TeamMember
            {
                Seat = seat
            }).ToList()
        };

        await teamRepository.AddTeamAsync(team);
        await teamRepository.SaveChangesAsync();

        await roundRepository.UpdateRoundStatus(currentRound.RoundId, RoundStatus.VoteOnTeam);
        await roundRepository.SaveChangesAsync();

        var teamMembers = game.Players
            .Where(p => selectedSeats.Contains(p.Seat))
            .Select(p => new
            {
                p.TempUserId,
                p.Nickname,
                p.Seat
            })
            .ToList();

        await Clients.Group(gameCode).SendAsync("TeamProposed", new
        {
            leaderSeat = game.LeaderSeat,
            leaderNickname = leader.Nickname,
            teamMembers,
            teamSize = teamMembers.Count
        });
    }


    public async Task VoteOnTeam(string gameCode, bool isApproved)
    {
        var (game, voter) = await GetGameAndPlayerAsync(gameCode);
        if (game == null || voter == null) return;
        
        var round = game.GetCurrentRound();
        if (round == null || !round.IsVotingPhase()) return;

        var team = await teamRepository.GetActiveByRoundIdAsync(round.RoundId);

        var existingVotes = await teamVoteRepository.GetByTeamAsync(team.TeamId);
        
        if (existingVotes.HasPlayerVoted(voter.Seat))
        {
            return;
        }

        var teamVote = new TeamVote()
        {
            TeamId = team.TeamId,
            Seat = voter.Seat,
            IsApproved = isApproved,
            Team = team
        };

        await teamVoteRepository.AddTeamVoteAsync(teamVote);
        await teamVoteRepository.SaveChangesAsync();

        await Clients.Group(gameCode).SendAsync("PlayerVoted", new
        {
            playerSeat = voter.Seat,
            playerNickname = voter.Nickname,
            isApproved
        });

        existingVotes.Add(teamVote);
        var requiredVotes = game.Players.Count - 1; // Everyone except leader votes

        if (existingVotes.HasAllPlayersVoted(requiredVotes))
        {
            // If majority votes yes the team will be approved
            var voteIsApproved = existingVotes.IsApprovedByMajority();
            var approvalCount = existingVotes.CountApprovals();
            var rejectionCount = existingVotes.CountRejections();

            await teamRepository.SaveChangesAsync();
            
            if (voteIsApproved)
            {
                await HandleTeamApproved(game, round, team, gameCode);
            }
            else
            {
                await HandleTeamRejected(game, round, team);
            }
            
            if (game.IsFinished())
            {
                return;
            }
            
            await Clients.Group(gameCode).SendAsync("TeamVoteResult", new
            {
                team.TeamId,
                rejectionCount,
                approvalCount,
                voteIsApproved,
                attemptNumber = game.ConsecutiveRejectedProposals
            });
        }
    }

    private async Task HandleTeamRejected(Game game, Round round, Team team)
    {
        game.ConsecutiveRejectedProposals++;

        if (game.HasReachedMaxRejections())
        {
            await EndGameAsync(game, GameResult.Shapeshifter, "5 consecutive team proposals were rejected");
            return;
        }

        game.LeaderSeat = game.GetNextLeaderSeat();

        round.Status = RoundStatus.TeamSelection;

        team.IsActive = false;

        await gameRepository.SaveChangesAsync();
        await roundRepository.SaveChangesAsync();
        await teamRepository.SaveChangesAsync();
    }

    private async Task HandleTeamApproved(Game game, Round round, Team team, string gameCode)
    {
        game.ConsecutiveRejectedProposals = 0;

        round.Status = RoundStatus.SecretChoices;

        await gameRepository.SaveChangesAsync();
        await roundRepository.SaveChangesAsync();

        await Clients.Group(gameCode).SendAsync("MissionStarted", new
        {
            team.TeamId,
            roundNumber = round.RoundNumber,
        });
    }

    public async Task VoteOnMission(string gameCode, bool isSuccess)
    {
        var (game, voter) = await GetGameAndPlayerAsync(gameCode);
        if (game == null || voter == null) return;
        
        var round = game.GetCurrentRound();
        if (round == null || !round.IsMissionPhase()) return;

        var team = await teamRepository.GetByRoundIdAsync(round.RoundId);

        var existingVotes = await missionVoteRepository.GetByRoundIdAsync(round.RoundId);
        if (existingVotes.HasPlayerVoted(voter.Seat))
        {
            return;
        }

        var missionVote = new MissionVote()
        {
            RoundId = round.RoundId,
            Seat = voter.Seat,
            IsSuccess = isSuccess,
            Round = round
        };

        await missionVoteRepository.AddMissionVoteAsync(missionVote);
        await missionVoteRepository.SaveChangesAsync();

        await Clients.Group(gameCode).SendAsync("MissionVoteSubmitted", new
        {
            playerSeat = voter.Seat,
            playerNickname = voter.Nickname
        });

        existingVotes.Add(missionVote);
        var requiredVotes = team.Members.Count;

        
        if (existingVotes.HasAllPlayersVoted(requiredVotes))
        {
            var successVotes = existingVotes.CountSuccesses();
            var failVotes = existingVotes.CountFailures();
            var voteIsSuccessful = failVotes == 0;
            
            team.IsActive = false;
            await teamRepository.SaveChangesAsync();
            
            

            if (voteIsSuccessful)
            {
                await HandleVoteSuccessful(game, round);
            }
            else
            {
                await HandleVoteSabotaged(game, round);
            }
            if (game.IsFinished())
            {
                return;
            }
            await Clients.Group(gameCode).SendAsync("MissionVoteResult", new
            {
                roundNumber = round.RoundNumber,
                successVotes,
                failVotes
            });

            await StartNextRound(gameCode);
        }
    }

    public async Task StartNextRound(string gameCode)
    {
        var game = await gameRepository.GetByCodeWithPlayersAndRoundsAsync(gameCode);
        if (game == null) return;
        game.RoundCounter++;
        game.LeaderSeat = game.GetNextLeaderSeat();
        await gameOrchestrator.CreateRoundAsync(game, game.RoundCounter, game.LeaderSeat);
        await gamePlayerRepository.SaveChangesAsync();

        await Clients.Group(gameCode).SendAsync("StartNextRound", new
        {
            roundNumber = game.RoundCounter,
            leaderSeat = game.LeaderSeat
        });
    }

    private async Task HandleVoteSabotaged(Game game, Round round)
    {
        game.SabotageCount++;

        round.Result = RoundResult.Sabotage;
        round.Status = RoundStatus.Completed;

        await gameRepository.SaveChangesAsync();
        await roundRepository.SaveChangesAsync();

        if(game.HasWinner())
        {
            await EndGameAsync(game, GameResult.Shapeshifter, "3 sabotaged missions");
        }
    }

    private async Task HandleVoteSuccessful(Game game, Round round)
    {
        
        game.SuccessCount++;

        round.Result = RoundResult.Success;
        round.Status = RoundStatus.Completed;

        await gameRepository.SaveChangesAsync();
        await roundRepository.SaveChangesAsync();

        if (game.HasWinner())
        {
            await EndGameAsync(game, GameResult.Human, "3 successful missions");
        }
    }

    private async Task<(Game? game, GamePlayer? player)> GetGameAndPlayerAsync(string gameCode)
    {
        var game = await gameRepository.GetByCodeWithPlayersAndRoundsAsync(gameCode);
        if (game == null) return(null, null);
        
        var tempUserId = sessionHelper.GetTempUserId();
        if (tempUserId == null) return (game, null);

        var player = game.Players.FindByTempUserId(tempUserId);
        return (game, player);
    }

    private async Task EndGameAsync(Game game, GameResult gameResult, string reason)
    {
        game.Status = GameStatus.Finished;
        game.GameWinner = gameResult;
        await gameRepository.SaveChangesAsync();
    
        await Task.WhenAll(
            BroadcastWinner(game, gameResult, reason),
            CleanupGameAsync(game)
        );
    }

    private async Task BroadcastWinner(Game game, GameResult gameResult, string reason)
    {
        await Clients.Group(game.ConnectionCode).SendAsync("GameEnded", new
        {
            winner = $"{gameResult}",
            reason,
            gameResult
        });
    }
    
    private async Task CleanupGameAsync(Game game)
    {
        //Deleting game for now to minimize stored rows in DB, will later be removed for displaying match history.
        gameRepository.DeleteGame(game);
        await gameRepository.SaveChangesAsync();
    }
    
    public async Task GameStarted(string gameCode)
    {
        await Clients.Group(gameCode).SendAsync("GameStarted");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
