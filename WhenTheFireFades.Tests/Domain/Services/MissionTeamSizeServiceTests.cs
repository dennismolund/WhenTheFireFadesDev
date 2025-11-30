using Domain.Services;
using FluentAssertions;

namespace WhenTheFireFades.Tests.Domain.Services;

public class MissionTeamSizeServiceTests
{
    #region Valid Player Count and Round Number Tests

    [Theory]
    [InlineData(2, 1, 2)]
    [InlineData(2, 2, 2)]
    [InlineData(2, 3, 2)]
    [InlineData(2, 4, 2)]
    [InlineData(2, 5, 2)]
    public void GetMissionTeamSize_WithTwoPlayers_ShouldReturnCorrectTeamSize(int playerCount, int roundNumber, int expectedSize)
    {
        // Act
        var result = MissionTeamSizeService.GetMissionTeamSize(playerCount, roundNumber);

        // Assert
        result.Should().Be(expectedSize);
    }

    [Theory]
    [InlineData(3, 1, 2)]
    [InlineData(3, 2, 3)]
    [InlineData(3, 3, 2)]
    [InlineData(3, 4, 3)]
    [InlineData(3, 5, 3)]
    public void GetMissionTeamSize_WithThreePlayers_ShouldReturnCorrectTeamSize(int playerCount, int roundNumber, int expectedSize)
    {
        // Act
        var result = MissionTeamSizeService.GetMissionTeamSize(playerCount, roundNumber);

        // Assert
        result.Should().Be(expectedSize);
    }

    [Theory]
    [InlineData(4, 1, 2)]
    [InlineData(4, 2, 3)]
    [InlineData(4, 3, 2)]
    [InlineData(4, 4, 3)]
    [InlineData(4, 5, 3)]
    public void GetMissionTeamSize_WithFourPlayers_ShouldReturnCorrectTeamSize(int playerCount, int roundNumber, int expectedSize)
    {
        // Act
        var result = MissionTeamSizeService.GetMissionTeamSize(playerCount, roundNumber);

        // Assert
        result.Should().Be(expectedSize);
    }

    [Theory]
    [InlineData(5, 1, 2)]
    [InlineData(5, 2, 3)]
    [InlineData(5, 3, 2)]
    [InlineData(5, 4, 3)]
    [InlineData(5, 5, 3)]
    public void GetMissionTeamSize_WithFivePlayers_ShouldReturnCorrectTeamSize(int playerCount, int roundNumber, int expectedSize)
    {
        // Act
        var result = MissionTeamSizeService.GetMissionTeamSize(playerCount, roundNumber);

        // Assert
        result.Should().Be(expectedSize);
    }

    [Theory]
    [InlineData(6, 1, 2)]
    [InlineData(6, 2, 3)]
    [InlineData(6, 3, 4)]
    [InlineData(6, 4, 3)]
    [InlineData(6, 5, 4)]
    public void GetMissionTeamSize_WithSixPlayers_ShouldReturnCorrectTeamSize(int playerCount, int roundNumber, int expectedSize)
    {
        // Act
        var result = MissionTeamSizeService.GetMissionTeamSize(playerCount, roundNumber);

        // Assert
        result.Should().Be(expectedSize);
    }

    [Theory]
    [InlineData(7, 1, 2)]
    [InlineData(7, 2, 3)]
    [InlineData(7, 3, 3)]
    [InlineData(7, 4, 4)]
    [InlineData(7, 5, 4)]
    public void GetMissionTeamSize_WithSevenPlayers_ShouldReturnCorrectTeamSize(int playerCount, int roundNumber, int expectedSize)
    {
        // Act
        var result = MissionTeamSizeService.GetMissionTeamSize(playerCount, roundNumber);

        // Assert
        result.Should().Be(expectedSize);
    }

    [Theory]
    [InlineData(8, 1, 3)]
    [InlineData(8, 2, 4)]
    [InlineData(8, 3, 4)]
    [InlineData(8, 4, 5)]
    [InlineData(8, 5, 5)]
    public void GetMissionTeamSize_WithEightPlayers_ShouldReturnCorrectTeamSize(int playerCount, int roundNumber, int expectedSize)
    {
        // Act
        var result = MissionTeamSizeService.GetMissionTeamSize(playerCount, roundNumber);

        // Assert
        result.Should().Be(expectedSize);
    }

    [Theory]
    [InlineData(9, 1, 3)]
    [InlineData(9, 2, 4)]
    [InlineData(9, 3, 4)]
    [InlineData(9, 4, 5)]
    [InlineData(9, 5, 5)]
    public void GetMissionTeamSize_WithNinePlayers_ShouldReturnCorrectTeamSize(int playerCount, int roundNumber, int expectedSize)
    {
        // Act
        var result = MissionTeamSizeService.GetMissionTeamSize(playerCount, roundNumber);

        // Assert
        result.Should().Be(expectedSize);
    }

    [Theory]
    [InlineData(10, 1, 3)]
    [InlineData(10, 2, 4)]
    [InlineData(10, 3, 4)]
    [InlineData(10, 4, 5)]
    [InlineData(10, 5, 5)]
    public void GetMissionTeamSize_WithTenPlayers_ShouldReturnCorrectTeamSize(int playerCount, int roundNumber, int expectedSize)
    {
        // Act
        var result = MissionTeamSizeService.GetMissionTeamSize(playerCount, roundNumber);

        // Assert
        result.Should().Be(expectedSize);
    }

    #endregion

    #region Invalid Player Count Tests

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void GetMissionTeamSize_WithPlayerCountLessThanMinimum_ShouldThrowInvalidOperationException(int playerCount)
    {
        // Act
        var act = () => MissionTeamSizeService.GetMissionTeamSize(playerCount, 1);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Unsupported player count: {playerCount}");
    }

    [Theory]
    [InlineData(11)]
    [InlineData(15)]
    [InlineData(100)]
    public void GetMissionTeamSize_WithPlayerCountGreaterThanMaximum_ShouldThrowInvalidOperationException(int playerCount)
    {
        // Act
        var act = () => MissionTeamSizeService.GetMissionTeamSize(playerCount, 1);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Unsupported player count: {playerCount}");
    }

    #endregion

    #region Invalid Round Number Tests

    [Theory]
    [InlineData(5, 0)]
    [InlineData(5, -1)]
    [InlineData(5, -10)]
    public void GetMissionTeamSize_WithRoundNumberLessThanOne_ShouldThrowArgumentOutOfRangeException(int playerCount, int roundNumber)
    {
        // Act
        var act = () => MissionTeamSizeService.GetMissionTeamSize(playerCount, roundNumber);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Round number is out of range.*")
            .And.ParamName.Should().Be("roundNumber");
    }

    [Theory]
    [InlineData(5, 6)]
    [InlineData(5, 7)]
    [InlineData(5, 10)]
    [InlineData(5, 100)]
    public void GetMissionTeamSize_WithRoundNumberGreaterThanFive_ShouldThrowArgumentOutOfRangeException(int playerCount, int roundNumber)
    {
        // Act
        var act = () => MissionTeamSizeService.GetMissionTeamSize(playerCount, roundNumber);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Round number is out of range.*")
            .And.ParamName.Should().Be("roundNumber");
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void GetMissionTeamSize_WithMinimumPlayerCountAndFirstRound_ShouldReturnCorrectSize()
    {
        // Arrange
        const int playerCount = 2;
        const int roundNumber = 1;

        // Act
        var result = MissionTeamSizeService.GetMissionTeamSize(playerCount, roundNumber);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public void GetMissionTeamSize_WithMaximumPlayerCountAndLastRound_ShouldReturnCorrectSize()
    {
        // Arrange
        const int playerCount = 10;
        const int roundNumber = 5;

        // Act
        var result = MissionTeamSizeService.GetMissionTeamSize(playerCount, roundNumber);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public void GetMissionTeamSize_WithSamePlayerCountDifferentRounds_ShouldReturnDifferentSizes()
    {
        // Arrange
        const int playerCount = 6;

        // Act
        var round1Size = MissionTeamSizeService.GetMissionTeamSize(playerCount, 1);
        var round3Size = MissionTeamSizeService.GetMissionTeamSize(playerCount, 3);

        // Assert
        round1Size.Should().Be(2);
        round3Size.Should().Be(4);
        round1Size.Should().NotBe(round3Size);
    }

    [Fact]
    public void GetMissionTeamSize_WithDifferentPlayerCountsSameRound_ShouldReturnDifferentSizes()
    {
        // Arrange
        const int roundNumber = 1;

        // Act
        var fivePlayerSize = MissionTeamSizeService.GetMissionTeamSize(5, roundNumber);
        var eightPlayerSize = MissionTeamSizeService.GetMissionTeamSize(8, roundNumber);

        // Assert
        fivePlayerSize.Should().Be(2);
        eightPlayerSize.Should().Be(3);
        fivePlayerSize.Should().NotBe(eightPlayerSize);
    }

    #endregion

    #region Boundary Tests

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    public void GetMissionTeamSize_WithValidPlayerCountAndBoundaryRound1_ShouldNotThrow(int playerCount)
    {
        // Act
        var act = () => MissionTeamSizeService.GetMissionTeamSize(playerCount, 1);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    public void GetMissionTeamSize_WithValidPlayerCountAndBoundaryRound5_ShouldNotThrow(int playerCount)
    {
        // Act
        var act = () => MissionTeamSizeService.GetMissionTeamSize(playerCount, 5);

        // Assert
        act.Should().NotThrow();
    }

    #endregion
}
