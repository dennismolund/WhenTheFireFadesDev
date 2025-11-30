using Application.Interfaces;
using Application.UseCases.Games;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Moq;

namespace WhenTheFireFades.Tests.Application.UseCases.Games;

public class StartGameFeatureTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly StartGameFeature _sut; // SUT = System Under Test

    public StartGameFeatureTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _sut = new StartGameFeature(_gameRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidGame_ShouldStartGame()
    {
        // Arrange
        var game = new Game
        {
            GameId = 1,
            Status = GameStatus.Lobby,
            LeaderSeat = 0,
            RoundCounter = 0,
            Players = new List<GamePlayer>
            {
                new() { Seat = 1, TempUserId = 1 },
                new() { Seat = 2, TempUserId = 2 },
                new() { Seat = 3, TempUserId = 3 },
                new() { Seat = 4, TempUserId = 4 },
                new() { Seat = 5, TempUserId = 5 }
            }
        };

        // Act
        await _sut.ExecuteAsync(game);

        // Assert
        game.Status.Should().Be(GameStatus.InProgress);
        game.RoundCounter.Should().Be(1);
        game.LeaderSeat.Should().Be(1);
        
        // Verify roles were assigned
        game.Players.Should().Contain(p => p.Role == PlayerRole.Shapeshifter);
        game.Players.Should().Contain(p => p.Role == PlayerRole.Human);
        
        // Verify SaveChanges was called
        _gameRepositoryMock.Verify(
            x => x.SaveChangesAsync(), 
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullGame_ShouldThrowArgumentException()
    {
        // Arrange
        Game? game = null;

        // Act
        var act = async () => await _sut.ExecuteAsync(game!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Game not found.");
    }

    [Fact]
    public async Task ExecuteAsync_WhenGameNotInLobby_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var game = new Game
        {
            Status = GameStatus.InProgress,
            Players = new List<GamePlayer>
            {
                new() { Seat = 1 },
                new() { Seat = 2 },
                new() { Seat = 3 },
                new() { Seat = 4 },
                new() { Seat = 5 }
            }
        };

        // Act
        var act = async () => await _sut.ExecuteAsync(game);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Game is not in a state that can be started.");
    }

    [Fact]
    public async Task ExecuteAsync_WithTooFewPlayers_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var game = new Game
        {
            Status = GameStatus.Lobby,
            Players = new List<GamePlayer>
            {
                new() { Seat = 1 }
            }
        };

        // Act
        var act = async () => await _sut.ExecuteAsync(game);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Not enough players to start the game.*");
    }

    [Theory]
    [InlineData(5, 2)]  // 5 players should have 2 shapeshifters
    [InlineData(7, 3)]  // 7 players should have 3 shapeshifters
    [InlineData(10, 4)] // 10 players should have 4 shapeshifters
    public async Task ExecuteAsync_ShouldAssignCorrectNumberOfShapeshifters(
        int playerCount, 
        int expectedShapeshifters)
    {
        // Arrange
        var game = new Game
        {
            Status = GameStatus.Lobby,
            Players = Enumerable.Range(1, playerCount)
                .Select(i => new GamePlayer { Seat = i, TempUserId = i })
                .ToList()
        };

        // Act
        await _sut.ExecuteAsync(game);

        // Assert
        var shapeshifterCount = game.Players.Count(p => p.Role == PlayerRole.Shapeshifter);
        shapeshifterCount.Should().Be(expectedShapeshifters);
    }
}