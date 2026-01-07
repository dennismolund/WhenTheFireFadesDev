using Domain.Entities;

namespace Domain.Extensions;

public static class VoteExtensions
{
    extension(IEnumerable<TeamVote> votes)
    {
        public int CountApprovals()
        {
            return votes.Count(v => v.IsApproved);
        }

        public int CountRejections()
        {
            return votes.Count(v => !v.IsApproved);
        }

        public bool IsApprovedByMajority()
        {
            var teamVotes = votes as TeamVote[] ?? votes.ToArray();
            var approvalCount = teamVotes.CountApprovals() + 1; // +1 for leader's implicit approval
            var rejectionCount = teamVotes.CountRejections();
            return approvalCount > rejectionCount;
        }
    }

    extension(IEnumerable<MissionVote> votes)
    {
        public int CountSuccesses()
        {
            return votes.Count(v => v.IsSuccess);
        }

        public int CountFailures()
        {
            return votes.Count(v => !v.IsSuccess);
        }

        public bool HasPlayerVoted(int seat)
        {
            return votes.Any(v => v.Seat == seat);
        }

        public bool HasAllPlayersVoted(int requiredVotes)
        {
            return votes.Count() >= requiredVotes;
        }
    }

    extension(IEnumerable<TeamVote> votes)
    {
        public bool HasPlayerVoted(int seat)
        {
            return votes.Any(v => v.Seat == seat);
        }

        public bool HasAllPlayersVoted(int requiredVotes)
        {
            return votes.Count() >= requiredVotes;
        }
    }
}