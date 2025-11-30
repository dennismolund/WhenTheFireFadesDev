using Domain.Rules;
using FluentAssertions;

namespace WhenTheFireFades.Tests.Domain.Rules;

public class GameRulesTests
{
    #region Constants Tests

    [Fact]
    public void PointsNeededToWin_ShouldBeThree()
    {
        // Assert
        GameRules.PointsNeededToWin.Should().Be(3);
    }

    [Fact]
    public void MaxConsecutiveRejections_ShouldBeFive()
    {
        // Assert
        GameRules.MaxConsecutiveRejections.Should().Be(5);
    }

    [Fact]
    public void MinPlayerCount_ShouldBeTwo()
    {
        // Assert
        GameRules.MinPlayerCount.Should().Be(2);
    }

    #endregion

    #region GetShapeshifterCount Valid Tests

    [Theory]
    [InlineData(2, 1)]
    [InlineData(3, 1)]
    [InlineData(4, 1)]
    public void GetShapeshifterCount_WithTwoToFourPlayers_ShouldReturnOne(int playerCount, int expectedShapeshifters)
    {
        // Act
        var result = GameRules.GetShapeshifterCount(playerCount);

        // Assert
        result.Should().Be(expectedShapeshifters);
    }

    [Theory]
    [InlineData(5, 2)]
    [InlineData(6, 2)]
    public void GetShapeshifterCount_WithFiveToSixPlayers_ShouldReturnTwo(int playerCount, int expectedShapeshifters)
    {
        // Act
        var result = GameRules.GetShapeshifterCount(playerCount);

        // Assert
        result.Should().Be(expectedShapeshifters);
    }

    [Theory]
    [InlineData(7, 3)]
    [InlineData(8, 3)]
    [InlineData(9, 3)]
    public void GetShapeshifterCount_WithSevenToNinePlayers_ShouldReturnThree(int playerCount, int expectedShapeshifters)
    {
        // Act
        var result = GameRules.GetShapeshifterCount(playerCount);

        // Assert
        result.Should().Be(expectedShapeshifters);
    }

    [Fact]
    public void GetShapeshifterCount_WithTenPlayers_ShouldReturnFour()
    {
        // Act
        var result = GameRules.GetShapeshifterCount(10);

        // Assert
        result.Should().Be(4);
    }

    #endregion

    #region GetShapeshifterCount Individual Player Count Tests

    [Fact]
    public void GetShapeshifterCount_WithTwoPlayers_ShouldReturnOne()
    {
        // Act
        var result = GameRules.GetShapeshifterCount(2);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void GetShapeshifterCount_WithThreePlayers_ShouldReturnOne()
    {
        // Act
        var result = GameRules.GetShapeshifterCount(3);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void GetShapeshifterCount_WithFourPlayers_ShouldReturnOne()
    {
        // Act
        var result = GameRules.GetShapeshifterCount(4);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void GetShapeshifterCount_WithFivePlayers_ShouldReturnTwo()
    {
        // Act
        var result = GameRules.GetShapeshifterCount(5);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public void GetShapeshifterCount_WithSixPlayers_ShouldReturnTwo()
    {
        // Act
        var result = GameRules.GetShapeshifterCount(6);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public void GetShapeshifterCount_WithSevenPlayers_ShouldReturnThree()
    {
        // Act
        var result = GameRules.GetShapeshifterCount(7);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public void GetShapeshifterCount_WithEightPlayers_ShouldReturnThree()
    {
        // Act
        var result = GameRules.GetShapeshifterCount(8);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public void GetShapeshifterCount_WithNinePlayers_ShouldReturnThree()
    {
        // Act
        var result = GameRules.GetShapeshifterCount(9);

        // Assert
        result.Should().Be(3);
    }

    #endregion

    #region GetShapeshifterCount Invalid Tests - Below Minimum

    [Fact]
    public void GetShapeshifterCount_WithZeroPlayers_ShouldThrowArgumentException()
    {
        // Act
        var act = () => GameRules.GetShapeshifterCount(0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Need at least 2 players*")
            .And.ParamName.Should().Be("playerCount");
    }

    [Fact]
    public void GetShapeshifterCount_WithOnePlayer_ShouldThrowArgumentException()
    {
        // Act
        var act = () => GameRules.GetShapeshifterCount(1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Need at least 2 players*")
            .And.ParamName.Should().Be("playerCount");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    [InlineData(-10)]
    [InlineData(-100)]
    public void GetShapeshifterCount_WithNegativePlayers_ShouldThrowArgumentException(int playerCount)
    {
        // Act
        var act = () => GameRules.GetShapeshifterCount(playerCount);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Need at least 2 players*")
            .And.ParamName.Should().Be("playerCount");
    }

    #endregion

    #region GetShapeshifterCount Invalid Tests - Above Maximum

    [Fact]
    public void GetShapeshifterCount_WithElevenPlayers_ShouldThrowArgumentException()
    {
        // Act
        var act = () => GameRules.GetShapeshifterCount(11);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Maximum 10 players allowed*")
            .And.ParamName.Should().Be("playerCount");
    }

    [Theory]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(15)]
    [InlineData(20)]
    [InlineData(50)]
    [InlineData(100)]
    public void GetShapeshifterCount_WithTooManyPlayers_ShouldThrowArgumentException(int playerCount)
    {
        // Act
        var act = () => GameRules.GetShapeshifterCount(playerCount);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Maximum 10 players allowed*")
            .And.ParamName.Should().Be("playerCount");
    }

    #endregion

    #region Boundary Tests

    [Fact]
    public void GetShapeshifterCount_WithMinimumPlayerCount_ShouldReturnOne()
    {
        // Arrange
        var minimumPlayers = GameRules.MinPlayerCount;

        // Act
        var result = GameRules.GetShapeshifterCount(minimumPlayers);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void GetShapeshifterCount_WithMaximumPlayerCount_ShouldReturnFour()
    {
        // Arrange
        const int maximumPlayers = 10;

        // Act
        var result = GameRules.GetShapeshifterCount(maximumPlayers);

        // Assert
        result.Should().Be(4);
    }

    [Fact]
    public void GetShapeshifterCount_WithBoundaryFourToFive_ShouldTransitionFromOneToTwo()
    {
        // Act
        var fourPlayerResult = GameRules.GetShapeshifterCount(4);
        var fivePlayerResult = GameRules.GetShapeshifterCount(5);

        // Assert
        fourPlayerResult.Should().Be(1);
        fivePlayerResult.Should().Be(2);
        fivePlayerResult.Should().BeGreaterThan(fourPlayerResult);
    }

    [Fact]
    public void GetShapeshifterCount_WithBoundarySixToSeven_ShouldTransitionFromTwoToThree()
    {
        // Act
        var sixPlayerResult = GameRules.GetShapeshifterCount(6);
        var sevenPlayerResult = GameRules.GetShapeshifterCount(7);

        // Assert
        sixPlayerResult.Should().Be(2);
        sevenPlayerResult.Should().Be(3);
        sevenPlayerResult.Should().BeGreaterThan(sixPlayerResult);
    }

    [Fact]
    public void GetShapeshifterCount_WithBoundaryNineToTen_ShouldTransitionFromThreeToFour()
    {
        // Act
        var ninePlayerResult = GameRules.GetShapeshifterCount(9);
        var tenPlayerResult = GameRules.GetShapeshifterCount(10);

        // Assert
        ninePlayerResult.Should().Be(3);
        tenPlayerResult.Should().Be(4);
        tenPlayerResult.Should().BeGreaterThan(ninePlayerResult);
    }

    #endregion

    #region Shapeshifter Distribution Logic Tests

    [Fact]
    public void GetShapeshifterCount_ShouldNeverReturnZero()
    {
        // Arrange & Act
        var results = new List<int>();
        for (var playerCount = 2; playerCount <= 10; playerCount++)
        {
            results.Add(GameRules.GetShapeshifterCount(playerCount));
        }

        // Assert
        results.Should().AllSatisfy(count => count.Should().BeGreaterThan(0));
    }

    [Fact]
    public void GetShapeshifterCount_ShouldNeverExceedPlayerCount()
    {
        // Arrange & Act & Assert
        for (var playerCount = 2; playerCount <= 10; playerCount++)
        {
            var shapeshifterCount = GameRules.GetShapeshifterCount(playerCount);
            shapeshifterCount.Should().BeLessThan(playerCount,
                $"shapeshifter count should be less than player count for {playerCount} players");
        }
    }

    [Fact]
    public void GetShapeshifterCount_ShouldNeverExceedHalfOfPlayerCount()
    {
        // Arrange & Act & Assert
        for (var playerCount = 2; playerCount <= 10; playerCount++)
        {
            var shapeshifterCount = GameRules.GetShapeshifterCount(playerCount);
            shapeshifterCount.Should().BeLessThanOrEqualTo(playerCount / 2,
                $"shapeshifter count should be at most half of player count for {playerCount} players");
        }
    }

    [Fact]
    public void GetShapeshifterCount_ShouldIncreaseMonotonically()
    {
        // Arrange
        var previousCount = 0;

        // Act & Assert
        for (var playerCount = 2; playerCount <= 10; playerCount++)
        {
            var shapeshifterCount = GameRules.GetShapeshifterCount(playerCount);
            shapeshifterCount.Should().BeGreaterThanOrEqualTo(previousCount,
                $"shapeshifter count should never decrease as player count increases");
            previousCount = shapeshifterCount;
        }
    }

    [Fact]
    public void GetShapeshifterCount_ShouldReturnConsistentResults()
    {
        // Arrange
        const int playerCount = 7;

        // Act
        var firstCall = GameRules.GetShapeshifterCount(playerCount);
        var secondCall = GameRules.GetShapeshifterCount(playerCount);
        var thirdCall = GameRules.GetShapeshifterCount(playerCount);

        // Assert
        firstCall.Should().Be(3);
        secondCall.Should().Be(3);
        thirdCall.Should().Be(3);
        firstCall.Should().Be(secondCall).And.Be(thirdCall);
    }

    #endregion

    #region Game Balance Tests

    [Theory]
    [InlineData(3, 1, 2)] // 1 shapeshifter, 2 humans
    [InlineData(5, 2, 3)] // 2 shapeshifters, 3 humans
    [InlineData(7, 3, 4)] // 3 shapeshifters, 4 humans
    [InlineData(10, 4, 6)] // 4 shapeshifters, 6 humans
    public void GetShapeshifterCount_ShouldMaintainGameBalance(int playerCount, int expectedShapeshifters, int expectedHumans)
    {
        // Act
        var shapeshifterCount = GameRules.GetShapeshifterCount(playerCount);
        var humanCount = playerCount - shapeshifterCount;

        // Assert
        shapeshifterCount.Should().Be(expectedShapeshifters);
        humanCount.Should().Be(expectedHumans);
        humanCount.Should().BeGreaterThan(shapeshifterCount, "humans should outnumber shapeshifters");
    }

    [Fact]
    public void GetShapeshifterCount_WithTwoPlayers_ShouldHaveEqualDistribution()
    {
        // Arrange
        const int playerCount = 2;

        // Act
        var shapeshifterCount = GameRules.GetShapeshifterCount(playerCount);
        var humanCount = playerCount - shapeshifterCount;

        // Assert
        shapeshifterCount.Should().Be(1);
        humanCount.Should().Be(1);
        shapeshifterCount.Should().Be(humanCount, "with 2 players, distribution should be equal");
    }

    [Fact]
    public void GetShapeshifterCount_ForThreeOrMorePlayers_HumansShouldAlwaysBeInMajority()
    {
        // Arrange & Act & Assert
        for (var playerCount = 3; playerCount <= 10; playerCount++)
        {
            var shapeshifterCount = GameRules.GetShapeshifterCount(playerCount);
            var humanCount = playerCount - shapeshifterCount;

            humanCount.Should().BeGreaterThan(shapeshifterCount,
                $"for {playerCount} players, humans ({humanCount}) should outnumber shapeshifters ({shapeshifterCount})");
        }
    }

    [Fact]
    public void GetShapeshifterCount_ForAllValidPlayerCounts_HumansShouldNeverBeOutnumbered()
    {
        // Arrange & Act & Assert
        for (var playerCount = 2; playerCount <= 10; playerCount++)
        {
            var shapeshifterCount = GameRules.GetShapeshifterCount(playerCount);
            var humanCount = playerCount - shapeshifterCount;

            humanCount.Should().BeGreaterThanOrEqualTo(shapeshifterCount,
                $"for {playerCount} players, humans ({humanCount}) should never be outnumbered by shapeshifters ({shapeshifterCount})");
        }
    }

    #endregion

    #region Complete Coverage Test

    [Theory]
    [InlineData(2, 1)]
    [InlineData(3, 1)]
    [InlineData(4, 1)]
    [InlineData(5, 2)]
    [InlineData(6, 2)]
    [InlineData(7, 3)]
    [InlineData(8, 3)]
    [InlineData(9, 3)]
    [InlineData(10, 4)]
    public void GetShapeshifterCount_WithAllValidPlayerCounts_ShouldReturnCorrectCount(int playerCount, int expectedShapeshifters)
    {
        // Act
        var result = GameRules.GetShapeshifterCount(playerCount);

        // Assert
        result.Should().Be(expectedShapeshifters);
    }

    #endregion
}
