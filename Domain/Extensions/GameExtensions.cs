using Domain.Entities;
using Domain.Enums;
using Domain.Rules;

namespace Domain.Extensions;

public static class GameExtensions
{
    extension(Game game)
    {
        public bool IsActive()
        {
            return game.Status is GameStatus.Lobby or GameStatus.InProgress;
        }

        public bool IsInLobby()
        {
            return game.Status is GameStatus.Lobby;
        }

        public bool IsInProgress()
        {
            return game.Status is GameStatus.InProgress;
        }

        public bool IsFinished()
        {
            return game.Status is GameStatus.Finished;
        }

        public bool HasEnoughPlayers()
        {
            return game.Players.Count >= GameRules.MinPlayerCount;    
        }

        public bool HasReachedMaxRejections()
        {
            return game.ConsecutiveRejectedProposals >= GameRules.MaxConsecutiveRejections;
        }

        public int GetNextLeaderSeat()
        {
            return game.LeaderSeat == game.Players.Count ? 1 : game.LeaderSeat + 1;
        }

        public bool HasWinner()
        {
            return game.SuccessCount >= GameRules.PointsNeededToWin || 
                   game.SabotageCount >= GameRules.PointsNeededToWin;
        }
    }
}