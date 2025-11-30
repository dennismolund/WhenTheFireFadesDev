using Domain.Entities;
using Domain.Enums;
using Domain.Extensions;
using FluentAssertions;

namespace WhenTheFireFades.Tests.Domain.Extensions;

public class GameExtensionsTests
{
    [Fact]
    public void IsInLobby_WhenStatusIsLobby_ShouldReturnTrue()
    {
        // Arrange
        var game = new Game { Status = GameStatus.Lobby };

        // Act
        var result = game.IsInLobby();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(GameStatus.InProgress)]
    [InlineData(GameStatus.Finished)]
    public void IsInLobby_WhenStatusIsNotLobby_ShouldReturnFalse(GameStatus status)
    {
        // Arrange
        var game = new Game { Status = status };

        // Act
        var result = game.IsInLobby();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsInProgress_WhenStatusIsInProgress_ShouldReturnTrue()
    {
        // Arrange
        var game = new Game { Status = GameStatus.InProgress };

        // Act
        var result = game.IsInProgress();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsFinished_WhenStatusIsFinished_ShouldReturnTrue()
    {
        // Arrange
        var game = new Game { Status = GameStatus.Finished };

        // Act
        var result = game.IsFinished();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(GameStatus.Lobby)]
    [InlineData(GameStatus.InProgress)]
    public void IsActive_WhenStatusIsLobbyOrInProgress_ShouldReturnTrue(GameStatus status)
    {
        // Arrange
        var game = new Game { Status = status };

        // Act
        var result = game.IsActive();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenStatusIsFinished_ShouldReturnFalse()
    {
        // Arrange
        var game = new Game { Status = GameStatus.Finished };

        // Act
        var result = game.IsActive();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(1, 5, 2)]  // Leader at seat 1, 5 players, next is 2
    [InlineData(3, 5, 4)]  // Leader at seat 3, 5 players, next is 4
    [InlineData(5, 5, 1)]  // Leader at seat 5, 5 players, wraps to 1
    public void GetNextLeaderSeat_ShouldReturnCorrectSeat(
        int currentLeaderSeat, 
        int playerCount, 
        int expectedSeat)
    {
        // Arrange
        var game = new Game 
        { 
            LeaderSeat = currentLeaderSeat,
            Players = Enumerable.Range(1, playerCount)
                .Select(i => new GamePlayer { Seat = i })
                .ToList()
        };

        // Act
        var result = game.GetNextLeaderSeat();

        // Assert
        result.Should().Be(expectedSeat);
    }

    [Fact]
    public void HasReachedMaxRejections_WhenAtThreshold_ShouldReturnTrue()
    {
        // Arrange
        var game = new Game { ConsecutiveRejectedProposals = 5 };

        // Act
        var result = game.HasReachedMaxRejections();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasWinner_WhenSuccessCountIs3_ShouldReturnTrue()
    {
        // Arrange
        var game = new Game 
        { 
            SuccessCount = 3,
            SabotageCount = 1
        };

        // Act
        var result = game.HasWinner();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasWinner_WhenSabotageCountIs3_ShouldReturnTrue()
    {
        // Arrange
        var game = new Game 
        { 
            SuccessCount = 1,
            SabotageCount = 3
        };

        // Act
        var result = game.HasWinner();

        // Assert
        result.Should().BeTrue();
    }

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
}