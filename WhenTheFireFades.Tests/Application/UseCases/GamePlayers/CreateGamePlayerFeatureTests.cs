using Application.Interfaces;
using Application.UseCases.GamePlayers;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Moq;

namespace WhenTheFireFades.Tests.Application.UseCases.GamePlayers;

public class CreateGamePlayerFeatureTests
{
    private readonly Mock<IGamePlayerRepository> _gamePlayerRepositoryMock;
    private readonly CreateGamePlayerFeature _sut;

    public CreateGamePlayerFeatureTests()
    {
        _gamePlayerRepositoryMock = new Mock<IGamePlayerRepository>();
        _sut = new CreateGamePlayerFeature(_gamePlayerRepositoryMock.Object);
    }
    
    [Fact]
    public async Task ExecuteAsync_ShouldCreatePlayerWithCorrectValues()
    {
        // Arrange
        GamePlayer? capturedPlayer = null;
        var game = new Game { GameId = 1 };
        const int tempUserId = 123;
        const string username = "TestPlayer";
        const string userId = "user-456";
        
        
        _gamePlayerRepositoryMock
                .Setup(x => x.GetNextAvailableSeatAsync(1)) 
            .ReturnsAsync(1); 
        
        _gamePlayerRepositoryMock
            .Setup(x => x.AddPlayerAsync(It.IsAny<GamePlayer>()))
            .Callback<GamePlayer>(g => capturedPlayer = g)
            .Returns(Task.CompletedTask);
        
        // Act
        await _sut.ExecuteAsync(game, tempUserId, username, userId);

        // Assert
        capturedPlayer.Should().NotBeNull();
        capturedPlayer!.GameId.Should().Be(1);
        capturedPlayer.TempUserId.Should().Be(123);
        capturedPlayer.Nickname.Should().Be("TestPlayer");
        capturedPlayer.UserId.Should().Be("user-456");
        capturedPlayer.Seat.Should().Be(1);
        capturedPlayer.Role.Should().Be(PlayerRole.Human);
        capturedPlayer.IsConnected.Should().BeTrue();
    }
    
    [Fact]
    public async Task ExecuteAsync_WhenNoUsernameProvided_ShouldGenerateDefaultNickname()
    {
        // Arrange
        GamePlayer? capturedPlayer = null;
        var game = new Game { GameId = 1 };
        const int tempUserId = 456;
    
        _gamePlayerRepositoryMock
            .Setup(x => x.GetNextAvailableSeatAsync(1))
            .ReturnsAsync(2);
    
        _gamePlayerRepositoryMock
            .Setup(x => x.AddPlayerAsync(It.IsAny<GamePlayer>()))
            .Callback<GamePlayer>(p => capturedPlayer = p)
            .Returns(Task.CompletedTask);
    
        // Act
        await _sut.ExecuteAsync(game, tempUserId, creatorUsername: null);  // No username!

        // Assert
        capturedPlayer.Should().NotBeNull();
        capturedPlayer!.Nickname.Should().Be("Player#456");  // Should use default format
    }
    
    [Fact]
    public async Task ExecuteAsync_ShouldCallRepositoryMethodsInCorrectOrder()
    {
        // Arrange
        var game = new Game { GameId = 1 };
        var callOrder = new List<string>();
    
        _gamePlayerRepositoryMock
            .Setup(x => x.GetNextAvailableSeatAsync(1))
            .Callback(() => callOrder.Add("GetNextSeat"))
            .ReturnsAsync(1);
    
        _gamePlayerRepositoryMock
            .Setup(x => x.AddPlayerAsync(It.IsAny<GamePlayer>()))
            .Callback(() => callOrder.Add("AddPlayer"))
            .Returns(Task.CompletedTask);
    
        _gamePlayerRepositoryMock
            .Setup(x => x.SaveChangesAsync())
            .Callback(() => callOrder.Add("SaveChanges"))
            .Returns(Task.CompletedTask);
    
        // Act
        await _sut.ExecuteAsync(game, 123, "Test");

        // Assert
        callOrder.Should().ContainInOrder("GetNextSeat", "AddPlayer", "SaveChanges");
    }
    
    [Fact]
    public async Task ExecuteAsync_ShouldCallSaveChangesAsync()
    {
        // Arrange
        var game = new Game { GameId = 1 };
    
        _gamePlayerRepositoryMock
            .Setup(x => x.GetNextAvailableSeatAsync(1))
            .ReturnsAsync(1);
    
        // Act
        await _sut.ExecuteAsync(game, 123, "Test");

        // Assert
        _gamePlayerRepositoryMock.Verify(
            x => x.SaveChangesAsync(), 
            Times.Once
        );
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task ExecuteAsync_ShouldAssignCorrectSeat(int expectedSeat)
    {
        // Arrange
        GamePlayer? capturedPlayer = null;
        var game = new Game { GameId = 1 };
    
        _gamePlayerRepositoryMock
            .Setup(x => x.GetNextAvailableSeatAsync(1))
            .ReturnsAsync(expectedSeat);  // Different seat each time
    
        _gamePlayerRepositoryMock
            .Setup(x => x.AddPlayerAsync(It.IsAny<GamePlayer>()))
            .Callback<GamePlayer>(p => capturedPlayer = p)
            .Returns(Task.CompletedTask);
    
        // Act
        await _sut.ExecuteAsync(game, 123, "Test");

        // Assert
        capturedPlayer!.Seat.Should().Be(expectedSeat);
    }
}