using Domain.Entities;
using Domain.Enums;

namespace Domain.Extensions;

public static class PlayerExtensions
{
    extension(IEnumerable<GamePlayer> players)
    {
        public GamePlayer? FindByTempUserId(int? tempUserId)
        {
            return tempUserId.HasValue 
                ? players.FirstOrDefault(p => p.TempUserId == tempUserId) 
                : null;
        }

        public GamePlayer? FindBySeat(int seat)
        {
            return players.FirstOrDefault(p => p.Seat == seat);
        }

        public List<GamePlayer> GetShapeshifters()
        {
            return players.Where(p => p.Role == PlayerRole.Shapeshifter).ToList();
        }

        public List<GamePlayer> GetHumans()
        {
            return players.Where(p => p.Role == PlayerRole.Human).ToList();
        }
    }

    extension(GamePlayer player)
    {
        public bool IsLeader(Game game)
        {
            return player.Seat == game.LeaderSeat;
        }

        public bool IsOnTeam(Team team)
        {
            return team.Members.Any(m => m.Seat == player.Seat);
        }
    }
}