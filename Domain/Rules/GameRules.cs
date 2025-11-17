namespace Domain.Rules;

public static class GameRules
{
    public const int PointsNeededToWin = 3;
    public const int MaxConsecutiveRejections = 5;

    public const int MinPlayerCount = 2;
    private const int MaxPlayerCount = 10;
    
    public static int GetShapeshifterCount(int playerCount)
    {
        if (playerCount < MinPlayerCount)
            throw new ArgumentException($"Need at least {MinPlayerCount} players", nameof(playerCount));
            
        if (playerCount > MaxPlayerCount)
            throw new ArgumentException($"Maximum {MaxPlayerCount} players allowed", nameof(playerCount));
        
        return playerCount switch
        {
            >= 10 => 4,
            >= 7 => 3,
            >= 5 => 2,
            _ => 1
        };
    }
    
}