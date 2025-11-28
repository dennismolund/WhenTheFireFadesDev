using Domain.Entities;
using Domain.Enums;

namespace Domain.Extensions;

public static class PlayerExtensions
{
    public static GamePlayer? FindByTempUserId(this IEnumerable<GamePlayer> players, int? tempUserId)
    {
        return tempUserId.HasValue 
            ? players.FirstOrDefault(p => p.TempUserId == tempUserId) 
            : null;
    }

    public static GamePlayer? FindBySeat(this IEnumerable<GamePlayer> players, int seat)
    {
        return players.FirstOrDefault(p => p.Seat == seat);
    }

    public static List<GamePlayer> GetShapeshifters(this IEnumerable<GamePlayer> players)
    {
        return players.Where(p => p.Role == PlayerRole.Shapeshifter).ToList();
    }

    public static List<GamePlayer> GetHumans(this IEnumerable<GamePlayer> players)
    {
        return players.Where(p => p.Role == PlayerRole.Human).ToList();
    }

    public static bool IsLeader(this GamePlayer player, Game game)
    {
        return player.Seat == game.LeaderSeat;
    }

    public static bool IsOnTeam(this GamePlayer player, Team team)
    {
        return team.Members.Any(m => m.Seat == player.Seat);
    }
}