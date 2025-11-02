using Domain.Rules;

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

        if (game != null)
        {
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
    }
    public async Task JoinGame(string gameCode)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
    }
    
    public async Task ProposeTeam(string code, List<int> selectedSeats)
    {
        if (string.IsNullOrWhiteSpace(code))
            return;

        code = code.Trim().ToUpperInvariant();

        var game = await gameRepository.GetByCodeWithPlayersAndRoundsAsync(code);
        if (game == null)
            return;

        var tempUserId = sessionHelper.GetTempUserId();
        if (tempUserId == null)
            return;

        var leader = game.Players.SingleOrDefault(p => p.Seat == game.LeaderSeat);
        if (leader == null || leader.TempUserId != tempUserId)
            return;

        var currentRound = game.Rounds.SingleOrDefault(r => r.RoundNumber == game.RoundCounter);
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

        await Clients.Group(code).SendAsync("TeamProposed", new
        {
            leaderSeat = game.LeaderSeat,
            leaderNickname = leader.Nickname,
            teamMembers,
            teamSize = teamMembers.Count
        });
    }


    public async Task VoteOnTeam(string gameCode, bool isApproved)
    {
        var game = await gameRepository.GetByCodeWithPlayersAndRoundsAsync(gameCode);
        if (game == null) return;

        var tempUserId = sessionHelper.GetTempUserId();
        if (tempUserId == null) return;

        var voter = game.Players.FirstOrDefault(p => p.TempUserId == tempUserId.Value);
        if (voter == null) return;

        var round = game.Rounds.OrderByDescending(r => r.RoundNumber).FirstOrDefault();
        if (round == null || round.Status != RoundStatus.VoteOnTeam) return;

        var team = await teamRepository.GetActiveByRoundIdAsync(round.RoundId);

        var existingVotes = await teamVoteRepository.GetByTeamAsync(team.TeamId);
        if (existingVotes.Any(v => v.Seat == voter.Seat))
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

        if (existingVotes.Count >= requiredVotes)
        {
            // If majority votes yes the team will be approved
            var approvalCount = existingVotes.Count(v => v.IsApproved) + 1; // +1 to count leaders vote
            var rejectionCount = existingVotes.Count(v => !v.IsApproved);
            var voteIsApproved = approvalCount > rejectionCount;

            await teamRepository.SaveChangesAsync();


            if (voteIsApproved)
            {
                await HandleTeamApproved(game, round, team, gameCode);
            }
            else
            {
                await HandleTeamRejected(game, round, team, gameCode);
            }
            
            if (game.Status == GameStatus.Finished)
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

    private async Task HandleTeamRejected(Game game, Round round, Team team, string gameCode)
    {
        game.ConsecutiveRejectedProposals++;

        if (game.ConsecutiveRejectedProposals >= GameRules.MaxConsecutiveRejections)
        {
            game.Status = GameStatus.Finished;
            game.GameWinner = GameResult.Shapeshifter;

            await gameRepository.SaveChangesAsync();

            await Clients.Group(gameCode).SendAsync("GameEnded", new
            {
                winner = "Shapeshifters",
                reason = "5 consecutive team proposals were rejected",
                gameResult = GameResult.Shapeshifter
            });

            //Deleting game for now to minimize stored rows in DB, will later be removed for displaying match history.
            gameRepository.DeleteGame(game);
            await gameRepository.SaveChangesAsync();
            
            return;
        }

        game.LeaderSeat = GetNewLeaderSet(game);

        round.Status = RoundStatus.TeamSelection;

        team.IsActive = false;

        await gameRepository.SaveChangesAsync();
        await roundRepository.SaveChangesAsync();
        await teamRepository.SaveChangesAsync();
    }

    private static int GetNewLeaderSet(Game game)
    {
        return (game.LeaderSeat == game.Players.Count) ? 1 : game.LeaderSeat + 1;
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
        var game = await gameRepository.GetByCodeWithPlayersAndRoundsAsync(gameCode);
        if (game == null) return;

        var tempUserId = sessionHelper.GetTempUserId();

        var voter = game.Players.FirstOrDefault(p => p.TempUserId == tempUserId);
        if (voter == null) return;

        var round = game.Rounds.OrderByDescending(r => r.RoundNumber).FirstOrDefault();
        if (round == null || round.Status != RoundStatus.SecretChoices) return;

        var team = await teamRepository.GetByRoundIdAsync(round.RoundId);

        var existingVotes = await missionVoteRepository.GetByRoundIdAsync(round.RoundId);
        if (existingVotes.Any(v => v.Seat == voter.Seat))
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

        
        if (existingVotes.Count >= requiredVotes)
        {
            var successVotes = existingVotes.Count(v => v.IsSuccess);
            var failVotes = existingVotes.Count(v => !v.IsSuccess);
            var voteIsSuccessful = failVotes == 0;
            
            team.IsActive = false;
            await teamRepository.SaveChangesAsync();

            if (voteIsSuccessful)
            {
                await HandleVoteSuccessful(game, round, gameCode);
            }
            else
            {
                await HandleVoteSabotaged(game, round, gameCode);
            }

            if (game.Status == GameStatus.Finished)
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
        game.LeaderSeat = GetNewLeaderSet(game);
        await gameOrchestrator.CreateRoundAsync(game, game.RoundCounter, game.LeaderSeat);
        await gamePlayerRepository.SaveChangesAsync();

        await Clients.Group(gameCode).SendAsync("StartNextRound", new
        {
            roundNumber = game.RoundCounter,
            leaderSeat = game.LeaderSeat
        });
    }

    private async Task HandleVoteSabotaged(Game game, Round round, string gameCode)
    {
        game.SabotageCount++;

        round.Result = RoundResult.Sabotage;
        round.Status = RoundStatus.Completed;

        await gameRepository.SaveChangesAsync();
        await roundRepository.SaveChangesAsync();

        if(game.SabotageCount >= GameRules.SabotagesNeededToLose)
        {
            
            game.Status = GameStatus.Finished;
            game.GameWinner = GameResult.Shapeshifter;
            await gameRepository.SaveChangesAsync();
            
            await Clients.Group(gameCode).SendAsync("GameEnded", new
            {
                winner = "Shapeshifters",
                reason = "3 sabotaged missions",
                gameResult = GameResult.Shapeshifter
            });
            
            //Deleting game for now to minimize stored rows in DB, will later be removed for displaying match history.
            gameRepository.DeleteGame(game);
            await gameRepository.SaveChangesAsync();
        }
    }

    private async Task HandleVoteSuccessful(Game game, Round round, string gameCode)
    {
        
        game.SuccessCount++;

        round.Result = RoundResult.Success;
        round.Status = RoundStatus.Completed;

        await gameRepository.SaveChangesAsync();
        await roundRepository.SaveChangesAsync();

        if (game.SuccessCount >= 3)
        {
            game.Status = GameStatus.Finished;
            game.GameWinner = GameResult.Human;
            await gameRepository.SaveChangesAsync();
            await Clients.Group(gameCode).SendAsync("GameEnded", new
            {
                winner = "Humans",
                reason = "3 successful missions",
                gameResult = GameResult.Human
            });
            
            //Deleting game for now to minimize stored rows in DB, will later be removed for displaying match history.
            gameRepository.DeleteGame(game);
            await gameRepository.SaveChangesAsync();
        }
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
