using Domain.Entities;

namespace Domain.Extensions;

public static class VoteExtensions
{
    public static int CountApprovals(this IEnumerable<TeamVote> votes)
    {
        return votes.Count(v => v.IsApproved);
    }

    public static int CountRejections(this IEnumerable<TeamVote> votes)
    {
        return votes.Count(v => !v.IsApproved);
    }

    public static bool IsApprovedByMajority(this IEnumerable<TeamVote> votes)
    {
        var teamVotes = votes as TeamVote[] ?? votes.ToArray();
        var approvalCount = teamVotes.CountApprovals() + 1; // +1 for leader's implicit approval
        var rejectionCount = teamVotes.CountRejections();
        return approvalCount > rejectionCount;
    }

    public static int CountSuccesses(this IEnumerable<MissionVote> votes)
    {
        return votes.Count(v => v.IsSuccess);
    }

    public static int CountFailures(this IEnumerable<MissionVote> votes)
    {
        return votes.Count(v => !v.IsSuccess);
    }

    public static bool HasPlayerVoted(this IEnumerable<TeamVote> votes, int seat)
    {
        return votes.Any(v => v.Seat == seat);
    }

    public static bool HasPlayerVoted(this IEnumerable<MissionVote> votes, int seat)
    {
        return votes.Any(v => v.Seat == seat);
    }
    
    public static bool HasAllPlayersVoted(this IEnumerable<TeamVote> votes, int requiredVotes)
    {
        return votes.Count() >= requiredVotes;
    }
    
    public static bool HasAllPlayersVoted(this IEnumerable<MissionVote> votes, int requiredVotes)
    {
        return votes.Count() >= requiredVotes;
    }
}