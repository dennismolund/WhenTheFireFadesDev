using Domain.Entities;
using Domain.Enums;

namespace Domain.Extensions;

public static class RoundExtensions
{
    public static bool RequiresTeamSelection(this Round round)
    {
        return round.Status == RoundStatus.TeamSelection;
    }

    public static bool IsVotingPhase(this Round round)
    {
        return round.Status == RoundStatus.VoteOnTeam;
    }

    public static bool IsMissionPhase(this Round round)
    {
        return round.Status == RoundStatus.SecretChoices;
    }

    public static bool IsCompleted(this Round round)
    {
        return round.Status == RoundStatus.Completed;
    }

    public static Round? GetCurrentRound(this Game game)
    {
        return game.Rounds.SingleOrDefault(r => r.RoundNumber == game.RoundCounter);
    }
    
    public static Team? GetActiveTeam(this Round round)
    {
        return round.Teams.FirstOrDefault(t => t.IsActive);
    }
}