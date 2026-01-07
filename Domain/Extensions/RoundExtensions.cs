using Domain.Entities;
using Domain.Enums;

namespace Domain.Extensions;

public static class RoundExtensions
{
    extension(Round round)
    {
        public bool RequiresTeamSelection()
        {
            return round.Status == RoundStatus.TeamSelection;
        }

        public bool IsVotingPhase()
        {
            return round.Status == RoundStatus.VoteOnTeam;
        }

        public bool IsMissionPhase()
        {
            return round.Status == RoundStatus.SecretChoices;
        }

        public bool IsCompleted()
        {
            return round.Status == RoundStatus.Completed;
        }

        public Team? GetActiveTeam()
        {
            return round.Teams.FirstOrDefault(t => t.IsActive);
        }
    }

    extension(Game game)
    {
        public Round? GetCurrentRound()
        {
            return game.Rounds
                .OrderByDescending(r => r.RoundNumber)
                .FirstOrDefault();    
        }
    }
}