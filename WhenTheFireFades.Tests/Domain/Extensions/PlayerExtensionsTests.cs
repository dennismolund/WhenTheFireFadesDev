using Domain.Entities;
using Domain.Enums;
using Domain.Extensions;
using FluentAssertions;

namespace WhenTheFireFades.Tests.Domain.Extensions;

public class PlayerExtensionsTests
{
    #region FindByTempUserId Tests

    [Fact]
    public void FindByTempUserId_WhenTempUserIdExistsInCollection_ShouldReturnPlayer()
    {
        // Arrange
        var players = new List<GamePlayer>
        {
            new() { GamePlayerId = 1, TempUserId = 100, Seat = 1 },
            new() { GamePlayerId = 2, TempUserId = 200, Seat = 2 },
            new() { GamePlayerId = 3, TempUserId = 300, Seat = 3 }
        };

        // Act
        var result = players.FindByTempUserId(200);

        // Assert
        result.Should().NotBeNull();
        result!.GamePlayerId.Should().Be(2);
        result.TempUserId.Should().Be(200);
    }

    [Fact]
    public void FindByTempUserId_WhenTempUserIdDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var players = new List<GamePlayer>
        {
            new() { GamePlayerId = 1, TempUserId = 100, Seat = 1 },
            new() { GamePlayerId = 2, TempUserId = 200, Seat = 2 }
        };

        // Act
        var result = players.FindByTempUserId(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FindByTempUserId_WhenTempUserIdIsNull_ShouldReturnNull()
    {
        // Arrange
        var players = new List<GamePlayer>
        {
            new() { GamePlayerId = 1, TempUserId = 100, Seat = 1 },
            new() { GamePlayerId = 2, TempUserId = 200, Seat = 2 }
        };

        // Act
        var result = players.FindByTempUserId(null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FindByTempUserId_WhenCollectionIsEmpty_ShouldReturnNull()
    {
        // Arrange
        var players = new List<GamePlayer>();

        // Act
        var result = players.FindByTempUserId(100);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region FindBySeat Tests

    [Fact]
    public void FindBySeat_WhenSeatExistsInCollection_ShouldReturnPlayer()
    {
        // Arrange
        var players = new List<GamePlayer>
        {
            new() { GamePlayerId = 1, Seat = 1 },
            new() { GamePlayerId = 2, Seat = 2 },
            new() { GamePlayerId = 3, Seat = 3 }
        };

        // Act
        var result = players.FindBySeat(2);

        // Assert
        result.Should().NotBeNull();
        result!.GamePlayerId.Should().Be(2);
        result.Seat.Should().Be(2);
    }

    [Fact]
    public void FindBySeat_WhenSeatDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var players = new List<GamePlayer>
        {
            new() { GamePlayerId = 1, Seat = 1 },
            new() { GamePlayerId = 2, Seat = 2 }
        };

        // Act
        var result = players.FindBySeat(5);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FindBySeat_WhenCollectionIsEmpty_ShouldReturnNull()
    {
        // Arrange
        var players = new List<GamePlayer>();

        // Act
        var result = players.FindBySeat(1);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetShapeshifters Tests

    [Fact]
    public void GetShapeshifters_WhenShapeshiftersExist_ShouldReturnOnlyShapeshifters()
    {
        // Arrange
        var players = new List<GamePlayer>
        {
            new() { GamePlayerId = 1, Role = PlayerRole.Human, Seat = 1 },
            new() { GamePlayerId = 2, Role = PlayerRole.Shapeshifter, Seat = 2 },
            new() { GamePlayerId = 3, Role = PlayerRole.Human, Seat = 3 },
            new() { GamePlayerId = 4, Role = PlayerRole.Shapeshifter, Seat = 4 }
        };

        // Act
        var result = players.GetShapeshifters();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.Role.Should().Be(PlayerRole.Shapeshifter));
        result.Select(p => p.GamePlayerId).Should().Contain(new[] { 2, 4 });
    }

    [Fact]
    public void GetShapeshifters_WhenNoShapeshiftersExist_ShouldReturnEmptyList()
    {
        // Arrange
        var players = new List<GamePlayer>
        {
            new() { GamePlayerId = 1, Role = PlayerRole.Human, Seat = 1 },
            new() { GamePlayerId = 2, Role = PlayerRole.Human, Seat = 2 }
        };

        // Act
        var result = players.GetShapeshifters();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetShapeshifters_WhenCollectionIsEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        var players = new List<GamePlayer>();

        // Act
        var result = players.GetShapeshifters();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetHumans Tests

    [Fact]
    public void GetHumans_WhenHumansExist_ShouldReturnOnlyHumans()
    {
        // Arrange
        var players = new List<GamePlayer>
        {
            new() { GamePlayerId = 1, Role = PlayerRole.Human, Seat = 1 },
            new() { GamePlayerId = 2, Role = PlayerRole.Shapeshifter, Seat = 2 },
            new() { GamePlayerId = 3, Role = PlayerRole.Human, Seat = 3 },
            new() { GamePlayerId = 4, Role = PlayerRole.Shapeshifter, Seat = 4 }
        };

        // Act
        var result = players.GetHumans();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.Role.Should().Be(PlayerRole.Human));
        result.Select(p => p.GamePlayerId).Should().Contain(new[] { 1, 3 });
    }

    [Fact]
    public void GetHumans_WhenNoHumansExist_ShouldReturnEmptyList()
    {
        // Arrange
        var players = new List<GamePlayer>
        {
            new() { GamePlayerId = 1, Role = PlayerRole.Shapeshifter, Seat = 1 },
            new() { GamePlayerId = 2, Role = PlayerRole.Shapeshifter, Seat = 2 }
        };

        // Act
        var result = players.GetHumans();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetHumans_WhenCollectionIsEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        var players = new List<GamePlayer>();

        // Act
        var result = players.GetHumans();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region IsLeader Tests

    [Fact]
    public void IsLeader_WhenPlayerIsTheLeader_ShouldReturnTrue()
    {
        // Arrange
        var player = new GamePlayer { Seat = 3 };
        var game = new Game { LeaderSeat = 3 };

        // Act
        var result = player.IsLeader(game);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsLeader_WhenPlayerIsNotTheLeader_ShouldReturnFalse()
    {
        // Arrange
        var player = new GamePlayer { Seat = 2 };
        var game = new Game { LeaderSeat = 5 };

        // Act
        var result = player.IsLeader(game);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsOnTeam Tests

    [Fact]
    public void IsOnTeam_WhenPlayerIsOnTeam_ShouldReturnTrue()
    {
        // Arrange
        var player = new GamePlayer { Seat = 3 };
        var team = new Team
        {
            Members = new List<TeamMember>
            {
                new() { Seat = 1 },
                new() { Seat = 3 },
                new() { Seat = 5 }
            }
        };

        // Act
        var result = player.IsOnTeam(team);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsOnTeam_WhenPlayerIsNotOnTeam_ShouldReturnFalse()
    {
        // Arrange
        var player = new GamePlayer { Seat = 4 };
        var team = new Team
        {
            Members = new List<TeamMember>
            {
                new() { Seat = 1 },
                new() { Seat = 2 },
                new() { Seat = 3 }
            }
        };

        // Act
        var result = player.IsOnTeam(team);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsOnTeam_WhenTeamHasNoMembers_ShouldReturnFalse()
    {
        // Arrange
        var player = new GamePlayer { Seat = 1 };
        var team = new Team { Members = new List<TeamMember>() };

        // Act
        var result = player.IsOnTeam(team);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
