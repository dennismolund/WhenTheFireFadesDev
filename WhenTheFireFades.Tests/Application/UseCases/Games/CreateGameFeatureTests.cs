using Application.Interfaces;
using Application.UseCases.Games;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Moq;

namespace WhenTheFireFades.Tests.Application.UseCases.Games;

public class CreateGameFeatureTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly CreateGameFeature _sut;

    public CreateGameFeatureTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _sut = new CreateGameFeature(_gameRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateGameWithCorrectInitialValues()
    {
        // Arrange
        Game? capturedGame = null;
        
        // Capture the game that gets passed to AddGameAsync
        _gameRepositoryMock
            .Setup(x => x.AddGameAsync(It.IsAny<Game>()))
            .Callback<Game>(g => capturedGame = g)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.Should().NotBeNull();
        result.LeaderSeat.Should().Be(1);
        result.Status.Should().Be(GameStatus.Lobby);
        result.GameWinner.Should().Be(GameResult.Unknown);
        result.RoundCounter.Should().Be(0);
        result.SuccessCount.Should().Be(0);
        result.SabotageCount.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldGenerateConnectionCode()
    {
        // Arrange
        _gameRepositoryMock
            .Setup(x => x.AddGameAsync(It.IsAny<Game>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.ConnectionCode.Should().NotBeNullOrEmpty();
        result.ConnectionCode.Should().HaveLength(6);
        result.ConnectionCode.Should().MatchRegex("^[A-Z0-9]{6}$");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldGenerateUniqueConnectionCodes()
    {
        // Arrange
        _gameRepositoryMock
            .Setup(x => x.AddGameAsync(It.IsAny<Game>()))
            .Returns(Task.CompletedTask);

        // Act - Create multiple games
        var game1 = await _sut.ExecuteAsync();
        var game2 = await _sut.ExecuteAsync();
        var game3 = await _sut.ExecuteAsync();

        // Assert - Codes should be different (very high probability)
        var codes = new[] { game1.ConnectionCode, game2.ConnectionCode, game3.ConnectionCode };
        codes.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallAddGameAsync()
    {
        // Arrange
        _gameRepositoryMock
            .Setup(x => x.AddGameAsync(It.IsAny<Game>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync();

        // Assert
        _gameRepositoryMock.Verify(
            x => x.AddGameAsync(It.Is<Game>(g => 
                g.Status == GameStatus.Lobby && 
                g.LeaderSeat == 1
            )), 
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallSaveChangesAsync()
    {
        // Arrange
        _gameRepositoryMock
            .Setup(x => x.AddGameAsync(It.IsAny<Game>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync();

        // Assert
        _gameRepositoryMock.Verify(
            x => x.SaveChangesAsync(), 
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoryMethodsInCorrectOrder()
    {
        // Arrange
        var callOrder = new List<string>();
        
        _gameRepositoryMock
            .Setup(x => x.AddGameAsync(It.IsAny<Game>()))
            .Callback(() => callOrder.Add("AddGame"))
            .Returns(Task.CompletedTask);
            
        _gameRepositoryMock
            .Setup(x => x.SaveChangesAsync())
            .Callback(() => callOrder.Add("SaveChanges"))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync();

        // Assert
        callOrder.Should().ContainInOrder("AddGame", "SaveChanges");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnTheSameGameThatWasAdded()
    {
        // Arrange
        Game? addedGame = null;
        
        _gameRepositoryMock
            .Setup(x => x.AddGameAsync(It.IsAny<Game>()))
            .Callback<Game>(g => addedGame = g)
            .Returns(Task.CompletedTask);

        // Act
        var returnedGame = await _sut.ExecuteAsync();

        // Assert
        returnedGame.Should().BeSameAs(addedGame);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryThrowsException_ShouldPropagate()
    {
        // Arrange
        _gameRepositoryMock
            .Setup(x => x.AddGameAsync(It.IsAny<Game>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var act = async () => await _sut.ExecuteAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }
}