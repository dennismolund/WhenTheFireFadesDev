using Domain.Entities;
using Domain.Enums;
using Domain.Extensions;
using FluentAssertions;

namespace WhenTheFireFades.Tests.Domain.Extensions;

public class RoundExtensionsTests
{
    #region RequiresTeamSelection Tests

    [Fact]
    public void RequiresTeamSelection_WhenStatusIsTeamSelection_ShouldReturnTrue()
    {
        // Arrange
        var round = new Round { Status = RoundStatus.TeamSelection };

        // Act
        var result = round.RequiresTeamSelection();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(RoundStatus.VoteOnTeam)]
    [InlineData(RoundStatus.SecretChoices)]
    [InlineData(RoundStatus.Completed)]
    public void RequiresTeamSelection_WhenStatusIsNotTeamSelection_ShouldReturnFalse(RoundStatus status)
    {
        // Arrange
        var round = new Round { Status = status };

        // Act
        var result = round.RequiresTeamSelection();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsVotingPhase Tests

    [Fact]
    public void IsVotingPhase_WhenStatusIsVoteOnTeam_ShouldReturnTrue()
    {
        // Arrange
        var round = new Round { Status = RoundStatus.VoteOnTeam };

        // Act
        var result = round.IsVotingPhase();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(RoundStatus.TeamSelection)]
    [InlineData(RoundStatus.SecretChoices)]
    [InlineData(RoundStatus.Completed)]
    public void IsVotingPhase_WhenStatusIsNotVoteOnTeam_ShouldReturnFalse(RoundStatus status)
    {
        // Arrange
        var round = new Round { Status = status };

        // Act
        var result = round.IsVotingPhase();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsMissionPhase Tests

    [Fact]
    public void IsMissionPhase_WhenStatusIsSecretChoices_ShouldReturnTrue()
    {
        // Arrange
        var round = new Round { Status = RoundStatus.SecretChoices };

        // Act
        var result = round.IsMissionPhase();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(RoundStatus.TeamSelection)]
    [InlineData(RoundStatus.VoteOnTeam)]
    [InlineData(RoundStatus.Completed)]
    public void IsMissionPhase_WhenStatusIsNotSecretChoices_ShouldReturnFalse(RoundStatus status)
    {
        // Arrange
        var round = new Round { Status = status };

        // Act
        var result = round.IsMissionPhase();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsCompleted Tests

    [Fact]
    public void IsCompleted_WhenStatusIsCompleted_ShouldReturnTrue()
    {
        // Arrange
        var round = new Round { Status = RoundStatus.Completed };

        // Act
        var result = round.IsCompleted();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(RoundStatus.TeamSelection)]
    [InlineData(RoundStatus.VoteOnTeam)]
    [InlineData(RoundStatus.SecretChoices)]
    public void IsCompleted_WhenStatusIsNotCompleted_ShouldReturnFalse(RoundStatus status)
    {
        // Arrange
        var round = new Round { Status = status };

        // Act
        var result = round.IsCompleted();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetCurrentRound Tests

    [Fact]
    public void GetCurrentRound_WhenRoundsExist_ShouldReturnLatestRound()
    {
        // Arrange
        var game = new Game
        {
            Rounds = new List<Round>
            {
                new() { RoundNumber = 1 },
                new() { RoundNumber = 2 },
                new() { RoundNumber = 3 }
            }
        };

        // Act
        var result = game.GetCurrentRound();

        // Assert
        result.Should().NotBeNull();
        result!.RoundNumber.Should().Be(3);
    }

    [Fact]
    public void GetCurrentRound_WhenNoRounds_ShouldReturnNull()
    {
        // Arrange
        var game = new Game { Rounds = new List<Round>() };

        // Act
        var result = game.GetCurrentRound();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentRound_WhenRoundsAreOutOfOrder_ShouldReturnHighestRoundNumber()
    {
        // Arrange
        var game = new Game
        {
            Rounds = new List<Round>
            {
                new() { RoundNumber = 3 },
                new() { RoundNumber = 1 },
                new() { RoundNumber = 5 },
                new() { RoundNumber = 2 }
            }
        };

        // Act
        var result = game.GetCurrentRound();

        // Assert
        result.Should().NotBeNull();
        result!.RoundNumber.Should().Be(5);
    }

    #endregion

    #region GetActiveTeam Tests

    [Fact]
    public void GetActiveTeam_WhenActiveTeamExists_ShouldReturnActiveTeam()
    {
        // Arrange
        var round = new Round
        {
            Teams = new List<Team>
            {
                new() { TeamId = 1, IsActive = false },
                new() { TeamId = 2, IsActive = true },
                new() { TeamId = 3, IsActive = false }
            }
        };

        // Act
        var result = round.GetActiveTeam();

        // Assert
        result.Should().NotBeNull();
        result!.TeamId.Should().Be(2);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void GetActiveTeam_WhenNoActiveTeam_ShouldReturnNull()
    {
        // Arrange
        var round = new Round
        {
            Teams = new List<Team>
            {
                new() { TeamId = 1, IsActive = false },
                new() { TeamId = 2, IsActive = false }
            }
        };

        // Act
        var result = round.GetActiveTeam();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetActiveTeam_WhenNoTeams_ShouldReturnNull()
    {
        // Arrange
        var round = new Round { Teams = new List<Team>() };

        // Act
        var result = round.GetActiveTeam();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetActiveTeam_WhenMultipleActiveTeams_ShouldReturnFirstActiveTeam()
    {
        // Arrange
        var round = new Round
        {
            Teams = new List<Team>
            {
                new() { TeamId = 1, IsActive = false },
                new() { TeamId = 2, IsActive = true },
                new() { TeamId = 3, IsActive = true }
            }
        };

        // Act
        var result = round.GetActiveTeam();

        // Assert
        result.Should().NotBeNull();
        result!.TeamId.Should().Be(2);
    }

    #endregion
}
