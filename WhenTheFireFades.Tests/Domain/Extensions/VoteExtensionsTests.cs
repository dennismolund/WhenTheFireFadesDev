using Domain.Entities;
using Domain.Extensions;
using FluentAssertions;

namespace WhenTheFireFades.Tests.Domain.Extensions;

public class VoteExtensionsTests
{
    #region CountApprovals Tests

    [Fact]
    public void CountApprovals_WhenAllVotesAreApproved_ShouldReturnTotalCount()
    {
        // Arrange
        var votes = new List<TeamVote>
        {
            new() { Seat = 1, IsApproved = true },
            new() { Seat = 2, IsApproved = true },
            new() { Seat = 3, IsApproved = true }
        };

        // Act
        var result = votes.CountApprovals();

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public void CountApprovals_WhenSomeVotesAreApproved_ShouldReturnApprovalCount()
    {
        // Arrange
        var votes = new List<TeamVote>
        {
            new() { Seat = 1, IsApproved = true },
            new() { Seat = 2, IsApproved = false },
            new() { Seat = 3, IsApproved = true },
            new() { Seat = 4, IsApproved = false }
        };

        // Act
        var result = votes.CountApprovals();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public void CountApprovals_WhenNoVotesAreApproved_ShouldReturnZero()
    {
        // Arrange
        var votes = new List<TeamVote>
        {
            new() { Seat = 1, IsApproved = false },
            new() { Seat = 2, IsApproved = false }
        };

        // Act
        var result = votes.CountApprovals();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CountApprovals_WhenCollectionIsEmpty_ShouldReturnZero()
    {
        // Arrange
        var votes = new List<TeamVote>();

        // Act
        var result = votes.CountApprovals();

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region CountRejections Tests

    [Fact]
    public void CountRejections_WhenAllVotesAreRejected_ShouldReturnTotalCount()
    {
        // Arrange
        var votes = new List<TeamVote>
        {
            new() { Seat = 1, IsApproved = false },
            new() { Seat = 2, IsApproved = false },
            new() { Seat = 3, IsApproved = false }
        };

        // Act
        var result = votes.CountRejections();

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public void CountRejections_WhenSomeVotesAreRejected_ShouldReturnRejectionCount()
    {
        // Arrange
        var votes = new List<TeamVote>
        {
            new() { Seat = 1, IsApproved = true },
            new() { Seat = 2, IsApproved = false },
            new() { Seat = 3, IsApproved = true },
            new() { Seat = 4, IsApproved = false }
        };

        // Act
        var result = votes.CountRejections();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public void CountRejections_WhenNoVotesAreRejected_ShouldReturnZero()
    {
        // Arrange
        var votes = new List<TeamVote>
        {
            new() { Seat = 1, IsApproved = true },
            new() { Seat = 2, IsApproved = true }
        };

        // Act
        var result = votes.CountRejections();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CountRejections_WhenCollectionIsEmpty_ShouldReturnZero()
    {
        // Arrange
        var votes = new List<TeamVote>();

        // Act
        var result = votes.CountRejections();

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region IsApprovedByMajority Tests

    [Fact]
    public void IsApprovedByMajority_WhenApprovalsExceedRejections_ShouldReturnTrue()
    {
        // Arrange - 3 approvals + 1 leader = 4, 2 rejections
        var votes = new List<TeamVote>
        {
            new() { Seat = 1, IsApproved = true },
            new() { Seat = 2, IsApproved = true },
            new() { Seat = 3, IsApproved = false },
            new() { Seat = 4, IsApproved = true },
            new() { Seat = 5, IsApproved = false }
        };

        // Act
        var result = votes.IsApprovedByMajority();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsApprovedByMajority_WhenRejectionsExceedApprovals_ShouldReturnFalse()
    {
        // Arrange - 1 approval + 1 leader = 2, 3 rejections
        var votes = new List<TeamVote>
        {
            new() { Seat = 1, IsApproved = false },
            new() { Seat = 2, IsApproved = false },
            new() { Seat = 3, IsApproved = false },
            new() { Seat = 4, IsApproved = true }
        };

        // Act
        var result = votes.IsApprovedByMajority();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsApprovedByMajority_WhenTied_ShouldReturnFalse()
    {
        // Arrange - 2 approvals + 1 leader = 3, 3 rejections
        var votes = new List<TeamVote>
        {
            new() { Seat = 1, IsApproved = true },
            new() { Seat = 2, IsApproved = false },
            new() { Seat = 3, IsApproved = true },
            new() { Seat = 4, IsApproved = false },
            new() { Seat = 5, IsApproved = false }
        };

        // Act
        var result = votes.IsApprovedByMajority();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsApprovedByMajority_WhenNoVotes_ShouldReturnTrue()
    {
        // Arrange - 0 approvals + 1 leader = 1, 0 rejections
        var votes = new List<TeamVote>();

        // Act
        var result = votes.IsApprovedByMajority();

        // Assert
        result.Should().BeTrue(); // Leader's implicit approval wins
    }

    [Fact]
    public void IsApprovedByMajority_WhenOnlyRejections_ShouldConsiderLeaderApproval()
    {
        // Arrange - 0 approvals + 1 leader = 1, 1 rejection
        var votes = new List<TeamVote>
        {
            new() { Seat = 1, IsApproved = false }
        };

        // Act
        var result = votes.IsApprovedByMajority();

        // Assert
        result.Should().BeFalse(); // 1 approval vs 1 rejection = tie, needs to exceed
    }

    #endregion

    #region CountSuccesses Tests

    [Fact]
    public void CountSuccesses_WhenAllVotesAreSuccesses_ShouldReturnTotalCount()
    {
        // Arrange
        var votes = new List<MissionVote>
        {
            new() { Seat = 1, IsSuccess = true },
            new() { Seat = 2, IsSuccess = true },
            new() { Seat = 3, IsSuccess = true }
        };

        // Act
        var result = votes.CountSuccesses();

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public void CountSuccesses_WhenSomeVotesAreSuccesses_ShouldReturnSuccessCount()
    {
        // Arrange
        var votes = new List<MissionVote>
        {
            new() { Seat = 1, IsSuccess = true },
            new() { Seat = 2, IsSuccess = false },
            new() { Seat = 3, IsSuccess = true },
            new() { Seat = 4, IsSuccess = false }
        };

        // Act
        var result = votes.CountSuccesses();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public void CountSuccesses_WhenNoVotesAreSuccesses_ShouldReturnZero()
    {
        // Arrange
        var votes = new List<MissionVote>
        {
            new() { Seat = 1, IsSuccess = false },
            new() { Seat = 2, IsSuccess = false }
        };

        // Act
        var result = votes.CountSuccesses();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CountSuccesses_WhenCollectionIsEmpty_ShouldReturnZero()
    {
        // Arrange
        var votes = new List<MissionVote>();

        // Act
        var result = votes.CountSuccesses();

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region CountFailures Tests

    [Fact]
    public void CountFailures_WhenAllVotesAreFailures_ShouldReturnTotalCount()
    {
        // Arrange
        var votes = new List<MissionVote>
        {
            new() { Seat = 1, IsSuccess = false },
            new() { Seat = 2, IsSuccess = false },
            new() { Seat = 3, IsSuccess = false }
        };

        // Act
        var result = votes.CountFailures();

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public void CountFailures_WhenSomeVotesAreFailures_ShouldReturnFailureCount()
    {
        // Arrange
        var votes = new List<MissionVote>
        {
            new() { Seat = 1, IsSuccess = true },
            new() { Seat = 2, IsSuccess = false },
            new() { Seat = 3, IsSuccess = true },
            new() { Seat = 4, IsSuccess = false }
        };

        // Act
        var result = votes.CountFailures();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public void CountFailures_WhenNoVotesAreFailures_ShouldReturnZero()
    {
        // Arrange
        var votes = new List<MissionVote>
        {
            new() { Seat = 1, IsSuccess = true },
            new() { Seat = 2, IsSuccess = true }
        };

        // Act
        var result = votes.CountFailures();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CountFailures_WhenCollectionIsEmpty_ShouldReturnZero()
    {
        // Arrange
        var votes = new List<MissionVote>();

        // Act
        var result = votes.CountFailures();

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region HasPlayerVoted (TeamVote) Tests

    [Fact]
    public void HasPlayerVoted_TeamVote_WhenPlayerHasVoted_ShouldReturnTrue()
    {
        // Arrange
        var votes = new List<TeamVote>
        {
            new() { Seat = 1, IsApproved = true },
            new() { Seat = 2, IsApproved = false },
            new() { Seat = 3, IsApproved = true }
        };

        // Act
        var result = votes.HasPlayerVoted(2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasPlayerVoted_TeamVote_WhenPlayerHasNotVoted_ShouldReturnFalse()
    {
        // Arrange
        var votes = new List<TeamVote>
        {
            new() { Seat = 1, IsApproved = true },
            new() { Seat = 2, IsApproved = false },
            new() { Seat = 3, IsApproved = true }
        };

        // Act
        var result = votes.HasPlayerVoted(5);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasPlayerVoted_TeamVote_WhenCollectionIsEmpty_ShouldReturnFalse()
    {
        // Arrange
        var votes = new List<TeamVote>();

        // Act
        var result = votes.HasPlayerVoted(1);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region HasPlayerVoted (MissionVote) Tests

    [Fact]
    public void HasPlayerVoted_MissionVote_WhenPlayerHasVoted_ShouldReturnTrue()
    {
        // Arrange
        var votes = new List<MissionVote>
        {
            new() { Seat = 1, IsSuccess = true },
            new() { Seat = 2, IsSuccess = false },
            new() { Seat = 3, IsSuccess = true }
        };

        // Act
        var result = votes.HasPlayerVoted(2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasPlayerVoted_MissionVote_WhenPlayerHasNotVoted_ShouldReturnFalse()
    {
        // Arrange
        var votes = new List<MissionVote>
        {
            new() { Seat = 1, IsSuccess = true },
            new() { Seat = 2, IsSuccess = false },
            new() { Seat = 3, IsSuccess = true }
        };

        // Act
        var result = votes.HasPlayerVoted(5);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasPlayerVoted_MissionVote_WhenCollectionIsEmpty_ShouldReturnFalse()
    {
        // Arrange
        var votes = new List<MissionVote>();

        // Act
        var result = votes.HasPlayerVoted(1);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region HasAllPlayersVoted (TeamVote) Tests

    [Fact]
    public void HasAllPlayersVoted_TeamVote_WhenVoteCountEqualsRequired_ShouldReturnTrue()
    {
        // Arrange
        var votes = new List<TeamVote>
        {
            new() { Seat = 1, IsApproved = true },
            new() { Seat = 2, IsApproved = false },
            new() { Seat = 3, IsApproved = true }
        };

        // Act
        var result = votes.HasAllPlayersVoted(3);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasAllPlayersVoted_TeamVote_WhenVoteCountExceedsRequired_ShouldReturnTrue()
    {
        // Arrange
        var votes = new List<TeamVote>
        {
            new() { Seat = 1, IsApproved = true },
            new() { Seat = 2, IsApproved = false },
            new() { Seat = 3, IsApproved = true },
            new() { Seat = 4, IsApproved = true }
        };

        // Act
        var result = votes.HasAllPlayersVoted(3);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasAllPlayersVoted_TeamVote_WhenVoteCountLessThanRequired_ShouldReturnFalse()
    {
        // Arrange
        var votes = new List<TeamVote>
        {
            new() { Seat = 1, IsApproved = true },
            new() { Seat = 2, IsApproved = false }
        };

        // Act
        var result = votes.HasAllPlayersVoted(3);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasAllPlayersVoted_TeamVote_WhenCollectionIsEmptyAndRequiredIsZero_ShouldReturnTrue()
    {
        // Arrange
        var votes = new List<TeamVote>();

        // Act
        var result = votes.HasAllPlayersVoted(0);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasAllPlayersVoted_TeamVote_WhenCollectionIsEmptyAndRequiredIsNonZero_ShouldReturnFalse()
    {
        // Arrange
        var votes = new List<TeamVote>();

        // Act
        var result = votes.HasAllPlayersVoted(3);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region HasAllPlayersVoted (MissionVote) Tests

    [Fact]
    public void HasAllPlayersVoted_MissionVote_WhenVoteCountEqualsRequired_ShouldReturnTrue()
    {
        // Arrange
        var votes = new List<MissionVote>
        {
            new() { Seat = 1, IsSuccess = true },
            new() { Seat = 2, IsSuccess = false },
            new() { Seat = 3, IsSuccess = true }
        };

        // Act
        var result = votes.HasAllPlayersVoted(3);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasAllPlayersVoted_MissionVote_WhenVoteCountExceedsRequired_ShouldReturnTrue()
    {
        // Arrange
        var votes = new List<MissionVote>
        {
            new() { Seat = 1, IsSuccess = true },
            new() { Seat = 2, IsSuccess = false },
            new() { Seat = 3, IsSuccess = true },
            new() { Seat = 4, IsSuccess = true }
        };

        // Act
        var result = votes.HasAllPlayersVoted(3);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasAllPlayersVoted_MissionVote_WhenVoteCountLessThanRequired_ShouldReturnFalse()
    {
        // Arrange
        var votes = new List<MissionVote>
        {
            new() { Seat = 1, IsSuccess = true },
            new() { Seat = 2, IsSuccess = false }
        };

        // Act
        var result = votes.HasAllPlayersVoted(3);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasAllPlayersVoted_MissionVote_WhenCollectionIsEmptyAndRequiredIsZero_ShouldReturnTrue()
    {
        // Arrange
        var votes = new List<MissionVote>();

        // Act
        var result = votes.HasAllPlayersVoted(0);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasAllPlayersVoted_MissionVote_WhenCollectionIsEmptyAndRequiredIsNonZero_ShouldReturnFalse()
    {
        // Arrange
        var votes = new List<MissionVote>();

        // Act
        var result = votes.HasAllPlayersVoted(3);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
