using Domain.Entities;
using Domain.Enums;
using Domain.Services;
using FluentAssertions;

namespace WhenTheFireFades.Tests.Domain.Services;

public class RoleAssignmentServiceTests
{
    #region Role Assignment Count Tests

    [Fact]
    public void AssignRoles_WithTwoPlayers_ShouldAssignOneShapeshifterAndOneHuman()
    {
        // Arrange
        var players = CreatePlayers(2);

        // Act
        RoleAssignmentService.AssignRoles(players);

        // Assert
        players.Count(p => p.Role == PlayerRole.Shapeshifter).Should().Be(1);
        players.Count(p => p.Role == PlayerRole.Human).Should().Be(1);
    }

    [Fact]
    public void AssignRoles_WithThreePlayers_ShouldAssignOneShapeshifterAndTwoHumans()
    {
        // Arrange
        var players = CreatePlayers(3);

        // Act
        RoleAssignmentService.AssignRoles(players);

        // Assert
        players.Count(p => p.Role == PlayerRole.Shapeshifter).Should().Be(1);
        players.Count(p => p.Role == PlayerRole.Human).Should().Be(2);
    }

    [Fact]
    public void AssignRoles_WithFourPlayers_ShouldAssignOneShapeshifterAndThreeHumans()
    {
        // Arrange
        var players = CreatePlayers(4);

        // Act
        RoleAssignmentService.AssignRoles(players);

        // Assert
        players.Count(p => p.Role == PlayerRole.Shapeshifter).Should().Be(1);
        players.Count(p => p.Role == PlayerRole.Human).Should().Be(3);
    }

    [Fact]
    public void AssignRoles_WithFivePlayers_ShouldAssignTwoShapeshiftersAndThreeHumans()
    {
        // Arrange
        var players = CreatePlayers(5);

        // Act
        RoleAssignmentService.AssignRoles(players);

        // Assert
        players.Count(p => p.Role == PlayerRole.Shapeshifter).Should().Be(2);
        players.Count(p => p.Role == PlayerRole.Human).Should().Be(3);
    }

    [Fact]
    public void AssignRoles_WithSixPlayers_ShouldAssignTwoShapeshiftersAndFourHumans()
    {
        // Arrange
        var players = CreatePlayers(6);

        // Act
        RoleAssignmentService.AssignRoles(players);

        // Assert
        players.Count(p => p.Role == PlayerRole.Shapeshifter).Should().Be(2);
        players.Count(p => p.Role == PlayerRole.Human).Should().Be(4);
    }

    [Fact]
    public void AssignRoles_WithSevenPlayers_ShouldAssignThreeShapeshiftersAndFourHumans()
    {
        // Arrange
        var players = CreatePlayers(7);

        // Act
        RoleAssignmentService.AssignRoles(players);

        // Assert
        players.Count(p => p.Role == PlayerRole.Shapeshifter).Should().Be(3);
        players.Count(p => p.Role == PlayerRole.Human).Should().Be(4);
    }

    [Fact]
    public void AssignRoles_WithEightPlayers_ShouldAssignThreeShapeshiftersAndFiveHumans()
    {
        // Arrange
        var players = CreatePlayers(8);

        // Act
        RoleAssignmentService.AssignRoles(players);

        // Assert
        players.Count(p => p.Role == PlayerRole.Shapeshifter).Should().Be(3);
        players.Count(p => p.Role == PlayerRole.Human).Should().Be(5);
    }

    [Fact]
    public void AssignRoles_WithNinePlayers_ShouldAssignThreeShapeshiftersAndSixHumans()
    {
        // Arrange
        var players = CreatePlayers(9);

        // Act
        RoleAssignmentService.AssignRoles(players);

        // Assert
        players.Count(p => p.Role == PlayerRole.Shapeshifter).Should().Be(3);
        players.Count(p => p.Role == PlayerRole.Human).Should().Be(6);
    }

    [Fact]
    public void AssignRoles_WithTenPlayers_ShouldAssignFourShapeshiftersAndSixHumans()
    {
        // Arrange
        var players = CreatePlayers(10);

        // Act
        RoleAssignmentService.AssignRoles(players);

        // Assert
        players.Count(p => p.Role == PlayerRole.Shapeshifter).Should().Be(4);
        players.Count(p => p.Role == PlayerRole.Human).Should().Be(6);
    }

    #endregion

    #region All Players Assigned Tests

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
    public void AssignRoles_ShouldAssignRoleToEveryPlayer(int playerCount)
    {
        // Arrange
        var players = CreatePlayers(playerCount);

        // Act
        RoleAssignmentService.AssignRoles(players);

        // Assert
        players.Should().AllSatisfy(p => p.Role.Should().BeOneOf(PlayerRole.Human, PlayerRole.Shapeshifter));
        players.Count.Should().Be(playerCount);
    }

    #endregion

    #region Randomness Tests

    [Fact]
    public void AssignRoles_CalledMultipleTimes_ShouldProduceVariedRoleAssignments()
    {
        // Arrange
        const int iterations = 100;
        const int playerCount = 5;
        var assignments = new List<List<int>>(); // Track which seats get shapeshifter role

        // Act
        for (var i = 0; i < iterations; i++)
        {
            var players = CreatePlayers(playerCount);
            RoleAssignmentService.AssignRoles(players);

            var shapeshifterSeats = players
                .Where(p => p.Role == PlayerRole.Shapeshifter)
                .Select(p => p.Seat)
                .OrderBy(s => s)
                .ToList();

            assignments.Add(shapeshifterSeats);
        }

        // Assert - should have at least some variation in assignments
        var uniqueAssignments = assignments.Distinct(new ListComparer()).Count();
        uniqueAssignments.Should().BeGreaterThan(1, "role assignment should be randomized");
    }

    [Fact]
    public void AssignRoles_CalledMultipleTimes_ShouldDistributeShapeshifterRoleAcrossAllPlayers()
    {
        // Arrange
        const int iterations = 200;
        const int playerCount = 5;
        var shapeshifterCounts = new Dictionary<int, int>();

        // Initialize counters for each seat
        for (var i = 1; i <= playerCount; i++)
        {
            shapeshifterCounts[i] = 0;
        }

        // Act
        for (var i = 0; i < iterations; i++)
        {
            var players = CreatePlayers(playerCount);
            RoleAssignmentService.AssignRoles(players);

            foreach (var player in players.Where(p => p.Role == PlayerRole.Shapeshifter))
            {
                shapeshifterCounts[player.Seat]++;
            }
        }

        // Assert - each seat should have been a shapeshifter at least once
        shapeshifterCounts.Values.Should().AllSatisfy(count =>
            count.Should().BeGreaterThan(0, "each player should have a chance to be a shapeshifter"));
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void AssignRoles_WithMinimumPlayerCount_ShouldAssignRolesCorrectly()
    {
        // Arrange
        var players = CreatePlayers(2);

        // Act
        RoleAssignmentService.AssignRoles(players);

        // Assert
        players.Should().HaveCount(2);
        players.Count(p => p.Role == PlayerRole.Shapeshifter).Should().Be(1);
        players.Count(p => p.Role == PlayerRole.Human).Should().Be(1);
    }

    [Fact]
    public void AssignRoles_WithMaximumPlayerCount_ShouldAssignRolesCorrectly()
    {
        // Arrange
        var players = CreatePlayers(10);

        // Act
        RoleAssignmentService.AssignRoles(players);

        // Assert
        players.Should().HaveCount(10);
        players.Count(p => p.Role == PlayerRole.Shapeshifter).Should().Be(4);
        players.Count(p => p.Role == PlayerRole.Human).Should().Be(6);
    }

    [Fact]
    public void AssignRoles_WithEmptyList_ShouldThrowArgumentException()
    {
        // Arrange
        var players = new List<GamePlayer>();

        // Act
        var act = () => RoleAssignmentService.AssignRoles(players);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Need at least 2 players*")
            .And.ParamName.Should().Be("playerCount");
    }

    [Fact]
    public void AssignRoles_WithOnePlayer_ShouldThrowArgumentException()
    {
        // Arrange
        var players = CreatePlayers(1);

        // Act
        var act = () => RoleAssignmentService.AssignRoles(players);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Need at least 2 players*")
            .And.ParamName.Should().Be("playerCount");
    }

    [Theory]
    [InlineData(11)]
    [InlineData(15)]
    [InlineData(20)]
    public void AssignRoles_WithTooManyPlayers_ShouldThrowArgumentException(int playerCount)
    {
        // Arrange
        var players = CreatePlayers(playerCount);

        // Act
        var act = () => RoleAssignmentService.AssignRoles(players);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Maximum 10 players allowed*")
            .And.ParamName.Should().Be("playerCount");
    }

    #endregion

    #region Consistency Tests

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(10)]
    public void AssignRoles_ShouldMaintainTotalPlayerCount(int playerCount)
    {
        // Arrange
        var players = CreatePlayers(playerCount);

        // Act
        RoleAssignmentService.AssignRoles(players);

        // Assert
        players.Should().HaveCount(playerCount);
        (players.Count(p => p.Role == PlayerRole.Shapeshifter) +
         players.Count(p => p.Role == PlayerRole.Human)).Should().Be(playerCount);
    }

    [Fact]
    public void AssignRoles_CalledTwiceOnSamePlayers_ShouldReassignRoles()
    {
        // Arrange
        var players = CreatePlayers(5);
        RoleAssignmentService.AssignRoles(players);
        var firstAssignment = players.ToDictionary(p => p.Seat, p => p.Role);

        // Act
        RoleAssignmentService.AssignRoles(players);

        // Assert
        players.Count(p => p.Role == PlayerRole.Shapeshifter).Should().Be(2);
        players.Count(p => p.Role == PlayerRole.Human).Should().Be(3);
    }

    #endregion

    #region Player Identity Tests

    [Fact]
    public void AssignRoles_ShouldNotModifyPlayerIdentities()
    {
        // Arrange
        var players = new List<GamePlayer>
        {
            new() { GamePlayerId = 1, Seat = 1, Nickname = "Alice" },
            new() { GamePlayerId = 2, Seat = 2, Nickname = "Bob" },
            new() { GamePlayerId = 3, Seat = 3, Nickname = "Charlie" },
            new() { GamePlayerId = 4, Seat = 4, Nickname = "Diana" },
            new() { GamePlayerId = 5, Seat = 5, Nickname = "Eve" }
        };

        // Act
        RoleAssignmentService.AssignRoles(players);

        // Assert
        players.Should().ContainSingle(p => p.GamePlayerId == 1 && p.Nickname == "Alice");
        players.Should().ContainSingle(p => p.GamePlayerId == 2 && p.Nickname == "Bob");
        players.Should().ContainSingle(p => p.GamePlayerId == 3 && p.Nickname == "Charlie");
        players.Should().ContainSingle(p => p.GamePlayerId == 4 && p.Nickname == "Diana");
        players.Should().ContainSingle(p => p.GamePlayerId == 5 && p.Nickname == "Eve");
    }

    [Fact]
    public void AssignRoles_ShouldNotModifyPlayerCount()
    {
        // Arrange
        var players = CreatePlayers(7);
        var originalCount = players.Count;

        // Act
        RoleAssignmentService.AssignRoles(players);

        // Assert
        players.Count.Should().Be(originalCount);
    }

    #endregion

    #region Helper Methods

    private static List<GamePlayer> CreatePlayers(int count)
    {
        var players = new List<GamePlayer>();
        for (var i = 1; i <= count; i++)
        {
            players.Add(new GamePlayer
            {
                GamePlayerId = i,
                Seat = i,
                Nickname = $"Player{i}"
            });
        }
        return players;
    }

    // Helper class for comparing lists in randomness tests
    private class ListComparer : IEqualityComparer<List<int>>
    {
        public bool Equals(List<int>? x, List<int>? y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.SequenceEqual(y);
        }

        public int GetHashCode(List<int> obj)
        {
            return obj.Aggregate(0, (acc, val) => acc ^ val.GetHashCode());
        }
    }

    #endregion
}
