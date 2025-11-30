using Application.Interfaces;
using Application.UseCases.Rounds;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Moq;

namespace WhenTheFireFades.Tests.Application.UseCases.Rounds;

public class CreateRoundFeatureTests
{
    private readonly Mock<IRoundRepository> _roundRepositoryMock;
    private readonly CreateRoundFeature _sut;

    public CreateRoundFeatureTests()
    {
        _roundRepositoryMock = new Mock<IRoundRepository>();
        _sut = new CreateRoundFeature(_roundRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateRoundWithCorrectValues()
    {
        // Arrange
        Round? capturedRound = null;
        var game = CreateGameWithPlayers(5);
        const int roundNumber = 1;
        const int leaderSeat = 1;

        _roundRepositoryMock
            .Setup(x => x.AddRoundAsync(It.IsAny<Round>()))
            .Callback<Round>(r => capturedRound = r)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(game, roundNumber, leaderSeat);

        // Assert
        capturedRound.Should().NotBeNull();
        capturedRound!.GameId.Should().Be(game.GameId);
        capturedRound.RoundNumber.Should().Be(1);
        capturedRound.LeaderSeat.Should().Be(1);
        capturedRound.Status.Should().Be(RoundStatus.TeamSelection);
        capturedRound.Result.Should().Be(RoundResult.Unknown);
    }

    [Theory]
    [InlineData(5, 1, 2)]  // 5 players, round 1 = team size 2
    [InlineData(5, 2, 3)]  // 5 players, round 2 = team size 3
    [InlineData(5, 3, 2)]  // 5 players, round 3 = team size 2
    [InlineData(6, 1, 2)]  // 6 players, round 1 = team size 2
    [InlineData(6, 3, 4)]  // 6 players, round 3 = team size 4
    [InlineData(7, 2, 3)]  // 7 players, round 2 = team size 3
    [InlineData(10, 1, 3)] // 10 players, round 1 = team size 3
    public async Task ExecuteAsync_ShouldCalculateCorrectTeamSize(
        int playerCount, 
        int roundNumber, 
        int expectedTeamSize)
    {
        // Arrange
        Round? capturedRound = null;
        var game = CreateGameWithPlayers(playerCount);

        _roundRepositoryMock
            .Setup(x => x.AddRoundAsync(It.IsAny<Round>()))
            .Callback<Round>(r => capturedRound = r)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(game, roundNumber, leaderSeat: 1);

        // Assert
        capturedRound.Should().NotBeNull();
        capturedRound!.TeamSize.Should().Be(expectedTeamSize);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallAddRoundAsync()
    {
        // Arrange
        var game = CreateGameWithPlayers(5);

        // Act
        await _sut.ExecuteAsync(game, roundNumber: 1, leaderSeat: 1);

        // Assert
        _roundRepositoryMock.Verify(
            x => x.AddRoundAsync(It.Is<Round>(r => 
                r.GameId == game.GameId &&
                r.RoundNumber == 1 &&
                r.LeaderSeat == 1
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallSaveChangesAsync()
    {
        // Arrange
        var game = CreateGameWithPlayers(5);

        // Act
        await _sut.ExecuteAsync(game, roundNumber: 1, leaderSeat: 1);

        // Assert
        _roundRepositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoryMethodsInCorrectOrder()
    {
        // Arrange
        var game = CreateGameWithPlayers(5);
        var callOrder = new List<string>();

        _roundRepositoryMock
            .Setup(x => x.AddRoundAsync(It.IsAny<Round>()))
            .Callback(() => callOrder.Add("AddRound"))
            .Returns(Task.CompletedTask);

        _roundRepositoryMock
            .Setup(x => x.SaveChangesAsync())
            .Callback(() => callOrder.Add("SaveChanges"))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(game, roundNumber: 1, leaderSeat: 1);

        // Assert
        callOrder.Should().ContainInOrder("AddRound", "SaveChanges");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task ExecuteAsync_ShouldSetCorrectRoundNumber(int roundNumber)
    {
        // Arrange
        Round? capturedRound = null;
        var game = CreateGameWithPlayers(5);

        _roundRepositoryMock
            .Setup(x => x.AddRoundAsync(It.IsAny<Round>()))
            .Callback<Round>(r => capturedRound = r)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(game, roundNumber, leaderSeat: 1);

        // Assert
        capturedRound!.RoundNumber.Should().Be(roundNumber);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task ExecuteAsync_ShouldSetCorrectLeaderSeat(int leaderSeat)
    {
        // Arrange
        Round? capturedRound = null;
        var game = CreateGameWithPlayers(10);

        _roundRepositoryMock
            .Setup(x => x.AddRoundAsync(It.IsAny<Round>()))
            .Callback<Round>(r => capturedRound = r)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(game, roundNumber: 1, leaderSeat);

        // Assert
        capturedRound!.LeaderSeat.Should().Be(leaderSeat);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldAlwaysSetStatusToTeamSelection()
    {
        // Arrange
        Round? capturedRound = null;
        var game = CreateGameWithPlayers(5);

        _roundRepositoryMock
            .Setup(x => x.AddRoundAsync(It.IsAny<Round>()))
            .Callback<Round>(r => capturedRound = r)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(game, roundNumber: 1, leaderSeat: 1);

        // Assert
        capturedRound!.Status.Should().Be(RoundStatus.TeamSelection);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldAlwaysSetResultToUnknown()
    {
        // Arrange
        Round? capturedRound = null;
        var game = CreateGameWithPlayers(5);

        _roundRepositoryMock
            .Setup(x => x.AddRoundAsync(It.IsAny<Round>()))
            .Callback<Round>(r => capturedRound = r)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(game, roundNumber: 1, leaderSeat: 1);

        // Assert
        capturedRound!.Result.Should().Be(RoundResult.Unknown);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryThrowsException_ShouldPropagate()
    {
        // Arrange
        var game = CreateGameWithPlayers(5);

        _roundRepositoryMock
            .Setup(x => x.AddRoundAsync(It.IsAny<Round>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var act = async () => await _sut.ExecuteAsync(game, roundNumber: 1, leaderSeat: 1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }

    [Theory]
    [InlineData(2, 1, 2)]  // Testing mode - 2 players
    [InlineData(3, 2, 3)]  // Testing mode - 3 players
    [InlineData(4, 3, 2)]  // Testing mode - 4 players
    public async Task ExecuteAsync_WithTestingPlayerCounts_ShouldCalculateCorrectTeamSize(
        int playerCount,
        int roundNumber,
        int expectedTeamSize)
    {
        // Arrange
        Round? capturedRound = null;
        var game = CreateGameWithPlayers(playerCount);

        _roundRepositoryMock
            .Setup(x => x.AddRoundAsync(It.IsAny<Round>()))
            .Callback<Round>(r => capturedRound = r)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(game, roundNumber, leaderSeat: 1);

        // Assert
        capturedRound!.TeamSize.Should().Be(expectedTeamSize);
    }

    #region Helper Methods

    private static Game CreateGameWithPlayers(int playerCount)
    {
        var game = new Game
        {
            GameId = 1,
            Players = Enumerable.Range(1, playerCount)
                .Select(i => new GamePlayer
                {
                    Seat = i,
                    TempUserId = i,
                    Nickname = $"Player{i}"
                })
                .ToList()
        };

        return game;
    }

    #endregion
}