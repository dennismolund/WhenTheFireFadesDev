using Domain.Entities;
using Domain.Enums;
using Domain.Rules;

namespace Domain.Extensions;

public static class GameExtensions
{
    public static bool IsActive(this Game game)
    {
        return game.Status is GameStatus.Lobby or GameStatus.InProgress;
    }
    
    public static bool IsInLobby(this Game game)
    {
        return game.Status is GameStatus.Lobby;
    }
    
    public static bool IsInProgress(this Game game)
    {
        return game.Status is GameStatus.Lobby;
    }
    
    public static bool IsFinished(this Game game)
    {
        return game.Status is GameStatus.Finished;
    }
    
    public static bool CanStart(this Game game)
    {
        return game is { Status: GameStatus.Lobby, Players.Count: >= GameRules.MinPlayerCount };    
    }
    
    public static bool HasReachedMaxRejections(this Game game)
    {
        return game.ConsecutiveRejectedProposals >= GameRules.MaxConsecutiveRejections;
    }

    public static int GetNextLeaderSeat(this Game game)
    {
        return game.LeaderSeat == game.Players.Count ? 1 : game.LeaderSeat + 1;
    }

    public static bool HasWinner(this Game game)
    {
        return game.SuccessCount >= GameRules.PointsNeededToWin || 
               game.SabotageCount >= GameRules.PointsNeededToWin;
    }
    
}