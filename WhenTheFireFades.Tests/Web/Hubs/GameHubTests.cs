using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Web.Helpers;
using Web.Hubs;
using System.Text.Json;

namespace WhenTheFireFades.Tests.Web.Hubs;

public class GameHubTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly Mock<IGamePlayerRepository> _gamePlayerRepositoryMock;
    private readonly Mock<IRoundRepository> _roundRepositoryMock;
    private readonly Mock<ITeamRepository> _teamRepositoryMock;
    private readonly Mock<ITeamVoteRepository> _teamVoteRepositoryMock;
    private readonly Mock<IMissionVoteRepository> _missionVoteRepositoryMock;
    private readonly GameOrchestrator _gameOrchestrator;
    private readonly Mock<SessionHelper> _sessionHelperMock;
    private readonly Mock<IGroupManager> _groupManagerMock;
    private readonly Mock<IHubCallerClients> _clientsMock;
    private readonly Mock<IClientProxy> _clientProxyMock;
    private readonly Mock<HubCallerContext> _contextMock;
    private readonly GameHub _sut;

    public GameHubTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _gamePlayerRepositoryMock = new Mock<IGamePlayerRepository>();
        _roundRepositoryMock = new Mock<IRoundRepository>();
        _teamRepositoryMock = new Mock<ITeamRepository>();
        _teamVoteRepositoryMock = new Mock<ITeamVoteRepository>();
        _missionVoteRepositoryMock = new Mock<IMissionVoteRepository>();
        // Create a real GameOrchestrator instance with mocked dependencies
        _gameOrchestrator = new GameOrchestrator(
            new global::Application.UseCases.Games.CreateGameFeature(_gameRepositoryMock.Object),
            new global::Application.UseCases.GamePlayers.CreateGamePlayerFeature(_gamePlayerRepositoryMock.Object),
            new global::Application.UseCases.Games.StartGameFeature(_gameRepositoryMock.Object),
            new global::Application.UseCases.Rounds.CreateRoundFeature(_roundRepositoryMock.Object)
        );
        _sessionHelperMock = new Mock<SessionHelper>(Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>());

        _groupManagerMock = new Mock<IGroupManager>();
        _clientsMock = new Mock<IHubCallerClients>();
        _clientProxyMock = new Mock<IClientProxy>();
        _contextMock = new Mock<HubCallerContext>();

        _sut = new GameHub(
            _gameRepositoryMock.Object,
            _gamePlayerRepositoryMock.Object,
            _roundRepositoryMock.Object,
            _teamRepositoryMock.Object,
            _teamVoteRepositoryMock.Object,
            _missionVoteRepositoryMock.Object,
            _gameOrchestrator,
            _sessionHelperMock.Object
        );

        // Setup SignalR hub context
        _sut.Groups = _groupManagerMock.Object;
        _sut.Clients = _clientsMock.Object;
        _sut.Context = _contextMock.Object;

        _contextMock.Setup(x => x.ConnectionId).Returns("test-connection-id");
        _clientsMock.Setup(x => x.Group(It.IsAny<string>())).Returns(_clientProxyMock.Object);
    }

    #region JoinGameLobby Tests

    [Fact]
    public async Task JoinGameLobby_ShouldAddUserToGroup()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGame(gameCode);
        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAsync(gameCode))
            .ReturnsAsync(game);

        // Act
        await _sut.JoinGameLobby(gameCode);

        // Assert
        _groupManagerMock.Verify(
            x => x.AddToGroupAsync("test-connection-id", gameCode, default),
            Times.Once
        );
    }

    [Fact]
    public async Task JoinGameLobby_WhenGameExists_ShouldBroadcastPlayerJoinedEvent()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGame(gameCode);
        game.Players.Add(new GamePlayer
        {
            TempUserId = 12345,
            Nickname = "Player1",
            Seat = 1,
            IsConnected = true
        });

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAsync(gameCode))
            .ReturnsAsync(game);

        // Act
        await _sut.JoinGameLobby(gameCode);

        // Assert
        _clientProxyMock.Verify(
            x => x.SendCoreAsync(
                "PlayerJoined",
                It.Is<object[]>(o =>
                    o.Length == 1 &&
                    o[0] != null
                ),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task JoinGameLobby_WhenGameDoesNotExist_ShouldNotBroadcastEvent()
    {
        // Arrange
        var gameCode = "INVALID";
        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAsync(gameCode))
            .ReturnsAsync((Game?)null);

        // Act
        await _sut.JoinGameLobby(gameCode);

        // Assert
        _clientProxyMock.Verify(
            x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default),
            Times.Never
        );
    }

    [Fact]
    public async Task JoinGameLobby_ShouldIncludeAllPlayersInBroadcast()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGame(gameCode);
        game.Players.Add(new GamePlayer { TempUserId = 1, Nickname = "Player1", Seat = 1, IsConnected = true });
        game.Players.Add(new GamePlayer { TempUserId = 2, Nickname = "Player2", Seat = 2, IsConnected = false });
        game.Players.Add(new GamePlayer { TempUserId = 3, Nickname = "Player3", Seat = 3, IsConnected = true });

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAsync(gameCode))
            .ReturnsAsync(game);

        object? capturedPayload = null;
        _clientProxyMock.Setup(x => x.SendCoreAsync(
                "PlayerJoined",
                It.IsAny<object[]>(),
                default
            ))
            .Callback<string, object[], CancellationToken>((method, args, ct) => capturedPayload = args[0])
            .Returns(Task.CompletedTask);

        // Act
        await _sut.JoinGameLobby(gameCode);

        // Assert
        capturedPayload.Should().NotBeNull();
        var payload = ToJsonElement(capturedPayload);
        payload.GetProperty("totalPlayers").GetInt32().Should().Be(3);
    }

    #endregion

    #region JoinGame Tests

    [Fact]
    public async Task JoinGame_ShouldAddUserToGroup()
    {
        // Arrange
        var gameCode = "ABC123";

        // Act
        await _sut.JoinGame(gameCode);

        // Assert
        _groupManagerMock.Verify(
            x => x.AddToGroupAsync("test-connection-id", gameCode, default),
            Times.Once
        );
    }

    #endregion

    #region ProposeTeam Tests

    [Fact]
    public async Task ProposeTeam_WhenGameCodeIsNull_ShouldReturnEarly()
    {
        // Arrange
        string gameCode = null!;
        var selectedSeats = new List<int> { 1, 2 };

        // Act
        await _sut.ProposeTeam(gameCode, selectedSeats);

        // Assert
        _gameRepositoryMock.Verify(
            x => x.GetByCodeWithPlayersAndRoundsAsync(It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ProposeTeam_WhenGameCodeIsWhitespace_ShouldReturnEarly()
    {
        // Arrange
        var gameCode = "   ";
        var selectedSeats = new List<int> { 1, 2 };

        // Act
        await _sut.ProposeTeam(gameCode, selectedSeats);

        // Assert
        _gameRepositoryMock.Verify(
            x => x.GetByCodeWithPlayersAndRoundsAsync(It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ProposeTeam_ShouldNormalizeGameCode()
    {
        // Arrange
        var gameCode = "  abc123  ";
        var normalizedCode = "ABC123";
        var selectedSeats = new List<int> { 1, 2 };

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(normalizedCode))
            .ReturnsAsync((Game?)null);

        // Act
        await _sut.ProposeTeam(gameCode, selectedSeats);

        // Assert
        _gameRepositoryMock.Verify(
            x => x.GetByCodeWithPlayersAndRoundsAsync(normalizedCode),
            Times.Once
        );
    }

    [Fact]
    public async Task ProposeTeam_WhenGameDoesNotExist_ShouldReturnEarly()
    {
        // Arrange
        var gameCode = "INVALID";
        var selectedSeats = new List<int> { 1, 2 };

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync((Game?)null);

        // Act
        await _sut.ProposeTeam(gameCode, selectedSeats);

        // Assert
        _sessionHelperMock.Verify(x => x.GetTempUserId(), Times.Never);
    }

    [Fact]
    public async Task ProposeTeam_WhenTempUserIdIsNull_ShouldReturnEarly()
    {
        // Arrange
        var gameCode = "ABC123";
        var selectedSeats = new List<int> { 1, 2 };
        var game = CreateTestGame(gameCode);

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns((int?)null);

        // Act
        await _sut.ProposeTeam(gameCode, selectedSeats);

        // Assert
        _teamRepositoryMock.Verify(x => x.AddTeamAsync(It.IsAny<Team>()), Times.Never);
    }

    [Fact]
    public async Task ProposeTeam_WhenUserIsNotLeader_ShouldReturnEarly()
    {
        // Arrange
        var gameCode = "ABC123";
        var selectedSeats = new List<int> { 1, 2 };
        var game = CreateTestGame(gameCode);
        game.LeaderSeat = 1;
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Leader", Seat = 1 });
        game.Players.Add(new GamePlayer { TempUserId = 67890, Nickname = "NotLeader", Seat = 2 });

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(67890); // Not the leader

        // Act
        await _sut.ProposeTeam(gameCode, selectedSeats);

        // Assert
        _teamRepositoryMock.Verify(x => x.AddTeamAsync(It.IsAny<Team>()), Times.Never);
    }

    [Fact]
    public async Task ProposeTeam_WhenNoCurrentRound_ShouldReturnEarly()
    {
        // Arrange
        var gameCode = "ABC123";
        var selectedSeats = new List<int> { 1, 2 };
        var game = CreateTestGame(gameCode);
        game.LeaderSeat = 1;
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Leader", Seat = 1 });
        game.Rounds.Clear(); // No rounds

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);

        // Act
        await _sut.ProposeTeam(gameCode, selectedSeats);

        // Assert
        _teamRepositoryMock.Verify(x => x.AddTeamAsync(It.IsAny<Team>()), Times.Never);
    }

    [Fact]
    public async Task ProposeTeam_WhenTeamSizeDoesNotMatchRoundRequirement_ShouldReturnEarly()
    {
        // Arrange
        var gameCode = "ABC123";
        var selectedSeats = new List<int> { 1, 2 }; // 2 seats
        var game = CreateTestGameWithRound(gameCode, teamSize: 3); // Requires 3
        game.LeaderSeat = 1;
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Leader", Seat = 1 });

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);

        // Act
        await _sut.ProposeTeam(gameCode, selectedSeats);

        // Assert
        _teamRepositoryMock.Verify(x => x.AddTeamAsync(It.IsAny<Team>()), Times.Never);
    }

    [Fact]
    public async Task ProposeTeam_WhenAllValidationsPass_ShouldCreateTeam()
    {
        // Arrange
        var gameCode = "ABC123";
        var selectedSeats = new List<int> { 1, 2 };
        var game = CreateTestGameWithRound(gameCode, teamSize: 2);
        game.LeaderSeat = 1;
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Leader", Seat = 1 });
        game.Players.Add(new GamePlayer { TempUserId = 67890, Nickname = "Player2", Seat = 2 });

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);

        Team? capturedTeam = null;
        _teamRepositoryMock.Setup(x => x.AddTeamAsync(It.IsAny<Team>()))
            .Callback<Team>(t => capturedTeam = t)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ProposeTeam(gameCode, selectedSeats);

        // Assert
        _teamRepositoryMock.Verify(x => x.AddTeamAsync(It.IsAny<Team>()), Times.Once);
        capturedTeam.Should().NotBeNull();
        capturedTeam!.IsActive.Should().BeTrue();
        capturedTeam.Members.Should().HaveCount(2);
        capturedTeam.Members.Select(m => m.Seat).Should().BeEquivalentTo(selectedSeats);
    }

    [Fact]
    public async Task ProposeTeam_WhenAllValidationsPass_ShouldUpdateRoundStatus()
    {
        // Arrange
        var gameCode = "ABC123";
        var selectedSeats = new List<int> { 1, 2 };
        var game = CreateTestGameWithRound(gameCode, teamSize: 2);
        game.LeaderSeat = 1;
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Leader", Seat = 1 });
        game.Players.Add(new GamePlayer { TempUserId = 67890, Nickname = "Player2", Seat = 2 });

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);

        // Act
        await _sut.ProposeTeam(gameCode, selectedSeats);

        // Assert
        _roundRepositoryMock.Verify(
            x => x.UpdateRoundStatus(It.IsAny<int>(), RoundStatus.VoteOnTeam),
            Times.Once
        );
        _roundRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ProposeTeam_WhenAllValidationsPass_ShouldBroadcastTeamProposed()
    {
        // Arrange
        var gameCode = "ABC123";
        var selectedSeats = new List<int> { 1, 2 };
        var game = CreateTestGameWithRound(gameCode, teamSize: 2);
        game.LeaderSeat = 1;
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Leader", Seat = 1 });
        game.Players.Add(new GamePlayer { TempUserId = 67890, Nickname = "Player2", Seat = 2 });

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);

        // Act
        await _sut.ProposeTeam(gameCode, selectedSeats);

        // Assert
        _clientProxyMock.Verify(
            x => x.SendCoreAsync(
                "TeamProposed",
                It.Is<object[]>(o => o.Length == 1),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ProposeTeam_ShouldCallRepositoryMethodsInCorrectOrder()
    {
        // Arrange
        var gameCode = "ABC123";
        var selectedSeats = new List<int> { 1, 2 };
        var game = CreateTestGameWithRound(gameCode, teamSize: 2);
        game.LeaderSeat = 1;
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Leader", Seat = 1 });
        game.Players.Add(new GamePlayer { TempUserId = 67890, Nickname = "Player2", Seat = 2 });

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);

        var callOrder = new List<string>();
        _teamRepositoryMock.Setup(x => x.AddTeamAsync(It.IsAny<Team>()))
            .Callback(() => callOrder.Add("AddTeam"))
            .Returns(Task.CompletedTask);
        _teamRepositoryMock.Setup(x => x.SaveChangesAsync())
            .Callback(() => callOrder.Add("SaveTeamChanges"))
            .Returns(Task.CompletedTask);
        _roundRepositoryMock.Setup(x => x.UpdateRoundStatus(It.IsAny<int>(), It.IsAny<RoundStatus>()))
            .Callback(() => callOrder.Add("UpdateRoundStatus"))
            .Returns(Task.CompletedTask);
        _roundRepositoryMock.Setup(x => x.SaveChangesAsync())
            .Callback(() => callOrder.Add("SaveRoundChanges"))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ProposeTeam(gameCode, selectedSeats);

        // Assert
        callOrder.Should().ContainInOrder("AddTeam", "SaveTeamChanges", "UpdateRoundStatus", "SaveRoundChanges");
    }

    #endregion

    #region VoteOnTeam Tests

    [Fact]
    public async Task VoteOnTeam_WhenGameDoesNotExist_ShouldReturnEarly()
    {
        // Arrange
        var gameCode = "INVALID";
        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync((Game?)null);

        // Act
        await _sut.VoteOnTeam(gameCode, true);

        // Assert
        _teamVoteRepositoryMock.Verify(
            x => x.AddTeamVoteAsync(It.IsAny<TeamVote>()),
            Times.Never
        );
    }

    [Fact]
    public async Task VoteOnTeam_WhenPlayerNotFound_ShouldReturnEarly()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGameWithRound(gameCode, teamSize: 2);

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns((int?)null);

        // Act
        await _sut.VoteOnTeam(gameCode, true);

        // Assert
        _teamVoteRepositoryMock.Verify(
            x => x.AddTeamVoteAsync(It.IsAny<TeamVote>()),
            Times.Never
        );
    }

    [Fact]
    public async Task VoteOnTeam_WhenNoCurrentRound_ShouldReturnEarly()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGame(gameCode);
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });
        game.Rounds.Clear();

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);

        // Act
        await _sut.VoteOnTeam(gameCode, true);

        // Assert
        _teamVoteRepositoryMock.Verify(
            x => x.AddTeamVoteAsync(It.IsAny<TeamVote>()),
            Times.Never
        );
    }

    [Fact]
    public async Task VoteOnTeam_WhenRoundNotInVotingPhase_ShouldReturnEarly()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGameWithRound(gameCode, teamSize: 2, roundStatus: RoundStatus.TeamSelection);
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);

        // Act
        await _sut.VoteOnTeam(gameCode, true);

        // Assert
        _teamVoteRepositoryMock.Verify(
            x => x.AddTeamVoteAsync(It.IsAny<TeamVote>()),
            Times.Never
        );
    }

    [Fact]
    public async Task VoteOnTeam_WhenPlayerAlreadyVoted_ShouldReturnEarly()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGameWithRound(gameCode, teamSize: 2, roundStatus: RoundStatus.VoteOnTeam);
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });

        var team = new Team { TeamId = 1, RoundId = game.Rounds.First().RoundId, IsActive = true };
        var existingVotes = new List<TeamVote>
        {
            new TeamVote { TeamId = 1, Seat = 1, IsApproved = true } // Player already voted
        };

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);
        _teamRepositoryMock.Setup(x => x.GetActiveByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(team);
        _teamVoteRepositoryMock.Setup(x => x.GetByTeamAsync(It.IsAny<int>()))
            .ReturnsAsync(existingVotes);

        // Act
        await _sut.VoteOnTeam(gameCode, true);

        // Assert
        _teamVoteRepositoryMock.Verify(
            x => x.AddTeamVoteAsync(It.IsAny<TeamVote>()),
            Times.Never
        );
    }

    [Fact]
    public async Task VoteOnTeam_WhenValid_ShouldAddVote()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGameWithRound(gameCode, teamSize: 2, roundStatus: RoundStatus.VoteOnTeam);
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });

        var team = new Team { TeamId = 1, RoundId = game.Rounds.First().RoundId, IsActive = true };
        var existingVotes = new List<TeamVote>();

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);
        _teamRepositoryMock.Setup(x => x.GetActiveByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(team);
        _teamVoteRepositoryMock.Setup(x => x.GetByTeamAsync(It.IsAny<int>()))
            .ReturnsAsync(existingVotes);

        TeamVote? capturedVote = null;
        _teamVoteRepositoryMock.Setup(x => x.AddTeamVoteAsync(It.IsAny<TeamVote>()))
            .Callback<TeamVote>(v => capturedVote = v)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.VoteOnTeam(gameCode, true);

        // Assert
        _teamVoteRepositoryMock.Verify(x => x.AddTeamVoteAsync(It.IsAny<TeamVote>()), Times.Once);
        capturedVote.Should().NotBeNull();
        capturedVote!.Seat.Should().Be(1);
        capturedVote.IsApproved.Should().BeTrue();
        capturedVote.TeamId.Should().Be(1);
    }

    [Fact]
    public async Task VoteOnTeam_WhenValid_ShouldBroadcastPlayerVoted()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGameWithRound(gameCode, teamSize: 2, roundStatus: RoundStatus.VoteOnTeam);
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });

        var team = new Team { TeamId = 1, RoundId = game.Rounds.First().RoundId, IsActive = true };
        var existingVotes = new List<TeamVote>();

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);
        _teamRepositoryMock.Setup(x => x.GetActiveByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(team);
        _teamVoteRepositoryMock.Setup(x => x.GetByTeamAsync(It.IsAny<int>()))
            .ReturnsAsync(existingVotes);

        // Act
        await _sut.VoteOnTeam(gameCode, false);

        // Assert
        _clientProxyMock.Verify(
            x => x.SendCoreAsync(
                "PlayerVoted",
                It.Is<object[]>(o => o.Length == 1),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task VoteOnTeam_WhenNotAllPlayersVoted_ShouldNotBroadcastResult()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGameWithRound(gameCode, teamSize: 2, roundStatus: RoundStatus.VoteOnTeam);
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });
        game.Players.Add(new GamePlayer { TempUserId = 67890, Nickname = "Player2", Seat = 2 });
        game.Players.Add(new GamePlayer { TempUserId = 11111, Nickname = "Player3", Seat = 3 });

        var team = new Team { TeamId = 1, RoundId = game.Rounds.First().RoundId, IsActive = true };
        var existingVotes = new List<TeamVote>(); // Need 2 votes (3 players - 1 leader), but will only have 1

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);
        _teamRepositoryMock.Setup(x => x.GetActiveByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(team);
        _teamVoteRepositoryMock.Setup(x => x.GetByTeamAsync(It.IsAny<int>()))
            .ReturnsAsync(existingVotes);

        // Act
        await _sut.VoteOnTeam(gameCode, true);

        // Assert
        _clientProxyMock.Verify(
            x => x.SendCoreAsync(
                "TeamVoteResult",
                It.IsAny<object[]>(),
                default
            ),
            Times.Never
        );
    }

    [Fact]
    public async Task VoteOnTeam_WhenAllPlayersVotedAndApproved_ShouldBroadcastApprovalAndStartMission()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGameWithRound(gameCode, teamSize: 2, roundStatus: RoundStatus.VoteOnTeam);
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });
        game.Players.Add(new GamePlayer { TempUserId = 67890, Nickname = "Player2", Seat = 2 });
        game.LeaderSeat = 1;

        var round = game.Rounds.First();
        var team = new Team { TeamId = 1, RoundId = round.RoundId, IsActive = true };
        var existingVotes = new List<TeamVote>
        {
            new TeamVote { TeamId = 1, Seat = 2, IsApproved = true } // First vote (leader doesn't vote)
        };

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);
        _teamRepositoryMock.Setup(x => x.GetActiveByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(team);
        _teamVoteRepositoryMock.Setup(x => x.GetByTeamAsync(It.IsAny<int>()))
            .ReturnsAsync(existingVotes);

        // Act - This is the last vote needed
        await _sut.VoteOnTeam(gameCode, true);

        // Assert
        _clientProxyMock.Verify(
            x => x.SendCoreAsync(
                "TeamVoteResult",
                It.Is<object[]>(o => o.Length == 1),
                default
            ),
            Times.Once
        );
        _clientProxyMock.Verify(
            x => x.SendCoreAsync(
                "MissionStarted",
                It.Is<object[]>(o => o.Length == 1),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task VoteOnTeam_WhenAllPlayersVotedAndRejected_ShouldBroadcastRejectionAndUpdateLeader()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGameWithRound(gameCode, teamSize: 2, roundStatus: RoundStatus.VoteOnTeam);
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });
        game.Players.Add(new GamePlayer { TempUserId = 67890, Nickname = "Player2", Seat = 2 });
        game.LeaderSeat = 1;
        game.ConsecutiveRejectedProposals = 0;

        var round = game.Rounds.First();
        var team = new Team { TeamId = 1, RoundId = round.RoundId, IsActive = true };
        var existingVotes = new List<TeamVote>
        {
            new TeamVote { TeamId = 1, Seat = 2, IsApproved = false } // First rejection
        };

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);
        _teamRepositoryMock.Setup(x => x.GetActiveByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(team);
        _teamVoteRepositoryMock.Setup(x => x.GetByTeamAsync(It.IsAny<int>()))
            .ReturnsAsync(existingVotes);

        // Act - This is the last vote needed (rejection)
        await _sut.VoteOnTeam(gameCode, false);

        // Assert
        _clientProxyMock.Verify(
            x => x.SendCoreAsync(
                "TeamVoteResult",
                It.Is<object[]>(o => o.Length == 1),
                default
            ),
            Times.Once
        );
        game.ConsecutiveRejectedProposals.Should().Be(1);
        game.LeaderSeat.Should().Be(2); // Should move to next player
        round.Status.Should().Be(RoundStatus.TeamSelection);
        team.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task VoteOnTeam_WhenFiveConsecutiveRejections_ShouldEndGame()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGameWithRound(gameCode, teamSize: 2, roundStatus: RoundStatus.VoteOnTeam);
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });
        game.Players.Add(new GamePlayer { TempUserId = 67890, Nickname = "Player2", Seat = 2 });
        game.LeaderSeat = 1;
        game.ConsecutiveRejectedProposals = 4; // Already at 4, this will be the 5th

        var round = game.Rounds.First();
        var team = new Team { TeamId = 1, RoundId = round.RoundId, IsActive = true };
        var existingVotes = new List<TeamVote>
        {
            new TeamVote { TeamId = 1, Seat = 2, IsApproved = false }
        };

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);
        _teamRepositoryMock.Setup(x => x.GetActiveByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(team);
        _teamVoteRepositoryMock.Setup(x => x.GetByTeamAsync(It.IsAny<int>()))
            .ReturnsAsync(existingVotes);

        // Act
        await _sut.VoteOnTeam(gameCode, false);

        // Assert
        game.ConsecutiveRejectedProposals.Should().Be(5);
        game.Status.Should().Be(GameStatus.Finished);
        game.GameWinner.Should().Be(GameResult.Shapeshifter);
        _clientProxyMock.Verify(
            x => x.SendCoreAsync(
                "GameEnded",
                It.Is<object[]>(o => o.Length == 1),
                default
            ),
            Times.Once
        );
    }

    #endregion

    #region VoteOnMission Tests

    [Fact]
    public async Task VoteOnMission_WhenGameDoesNotExist_ShouldReturnEarly()
    {
        // Arrange
        var gameCode = "INVALID";
        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync((Game?)null);

        // Act
        await _sut.VoteOnMission(gameCode, true);

        // Assert
        _missionVoteRepositoryMock.Verify(
            x => x.AddMissionVoteAsync(It.IsAny<MissionVote>()),
            Times.Never
        );
    }

    [Fact]
    public async Task VoteOnMission_WhenPlayerNotFound_ShouldReturnEarly()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGameWithRound(gameCode, teamSize: 2);

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns((int?)null);

        // Act
        await _sut.VoteOnMission(gameCode, true);

        // Assert
        _missionVoteRepositoryMock.Verify(
            x => x.AddMissionVoteAsync(It.IsAny<MissionVote>()),
            Times.Never
        );
    }

    [Fact]
    public async Task VoteOnMission_WhenRoundNotInMissionPhase_ShouldReturnEarly()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGameWithRound(gameCode, teamSize: 2, roundStatus: RoundStatus.VoteOnTeam);
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);

        // Act
        await _sut.VoteOnMission(gameCode, true);

        // Assert
        _missionVoteRepositoryMock.Verify(
            x => x.AddMissionVoteAsync(It.IsAny<MissionVote>()),
            Times.Never
        );
    }

    [Fact]
    public async Task VoteOnMission_WhenPlayerAlreadyVoted_ShouldReturnEarly()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGameWithRound(gameCode, teamSize: 2, roundStatus: RoundStatus.SecretChoices);
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });

        var existingVotes = new List<MissionVote>
        {
            new MissionVote { RoundId = game.Rounds.First().RoundId, Seat = 1, IsSuccess = true }
        };

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);
        _teamRepositoryMock.Setup(x => x.GetByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Team
            {
                TeamId = 1,
                RoundId = game.Rounds.First().RoundId,
                Members = new List<TeamMember> { new TeamMember { Seat = 1 } }
            });
        _missionVoteRepositoryMock.Setup(x => x.GetByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(existingVotes);

        // Act
        await _sut.VoteOnMission(gameCode, true);

        // Assert
        _missionVoteRepositoryMock.Verify(
            x => x.AddMissionVoteAsync(It.IsAny<MissionVote>()),
            Times.Never
        );
    }

    [Fact]
    public async Task VoteOnMission_WhenValid_ShouldAddVote()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGameWithRound(gameCode, teamSize: 2, roundStatus: RoundStatus.SecretChoices);
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });
        game.Players.Add(new GamePlayer { TempUserId = 67890, Nickname = "Player2", Seat = 2 });

        var existingVotes = new List<MissionVote>();

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);
        _teamRepositoryMock.Setup(x => x.GetByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Team
            {
                TeamId = 1,
                RoundId = game.Rounds.First().RoundId,
                Members = new List<TeamMember> { new TeamMember { Seat = 1 } }
            });
        _missionVoteRepositoryMock.Setup(x => x.GetByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(existingVotes);

        MissionVote? capturedVote = null;
        _missionVoteRepositoryMock.Setup(x => x.AddMissionVoteAsync(It.IsAny<MissionVote>()))
            .Callback<MissionVote>(v => capturedVote = v)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.VoteOnMission(gameCode, false);

        // Assert
        _missionVoteRepositoryMock.Verify(x => x.AddMissionVoteAsync(It.IsAny<MissionVote>()), Times.Once);
        capturedVote.Should().NotBeNull();
        capturedVote!.Seat.Should().Be(1);
        capturedVote.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task VoteOnMission_WhenValid_ShouldBroadcastMissionVoteSubmitted()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGameWithRound(gameCode, teamSize: 2, roundStatus: RoundStatus.SecretChoices);
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });
        game.Players.Add(new GamePlayer { TempUserId = 67890, Nickname = "Player2", Seat = 2 });

        var existingVotes = new List<MissionVote>();

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);
        _teamRepositoryMock.Setup(x => x.GetByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Team
            {
                TeamId = 1,
                RoundId = game.Rounds.First().RoundId,
                Members = new List<TeamMember> { new TeamMember { Seat = 1 } }
            });
        _missionVoteRepositoryMock.Setup(x => x.GetByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(existingVotes);

        // Act
        await _sut.VoteOnMission(gameCode, true);

        // Assert
        _clientProxyMock.Verify(
            x => x.SendCoreAsync(
                "MissionVoteSubmitted",
                It.Is<object[]>(o => o.Length == 1),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task VoteOnMission_WhenAllVotesInAndAllSuccess_ShouldIncrementSuccessCount()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGameWithRound(gameCode, teamSize: 2, roundStatus: RoundStatus.SecretChoices);
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });
        game.Players.Add(new GamePlayer { TempUserId = 67890, Nickname = "Player2", Seat = 2 });
        game.SuccessCount = 0;

        var round = game.Rounds.First();
        var existingVotes = new List<MissionVote>
        {
            new MissionVote { RoundId = round.RoundId, Seat = 2, IsSuccess = true }
        };

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);
        _teamRepositoryMock.Setup(x => x.GetByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Team
            {
                TeamId = 1,
                RoundId = round.RoundId,
                Members = new List<TeamMember>
                {
                    new TeamMember { Seat = 1 },
                    new TeamMember { Seat = 2 }
                }
            });
        _missionVoteRepositoryMock.Setup(x => x.GetByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(existingVotes);

        // Act - This is the last vote needed (success)
        await _sut.VoteOnMission(gameCode, true);

        // Assert
        game.SuccessCount.Should().Be(1);
        round.Result.Should().Be(RoundResult.Success);
        round.Status.Should().Be(RoundStatus.Completed);
        _clientProxyMock.Verify(
            x => x.SendCoreAsync(
                "MissionVoteResult",
                It.Is<object[]>(o => o.Length == 1),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task VoteOnMission_WhenAllVotesInAndAnyFail_ShouldIncrementSabotageCount()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGameWithRound(gameCode, teamSize: 2, roundStatus: RoundStatus.SecretChoices);
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });
        game.Players.Add(new GamePlayer { TempUserId = 67890, Nickname = "Player2", Seat = 2 });
        game.SabotageCount = 0;

        var round = game.Rounds.First();
        var existingVotes = new List<MissionVote>
        {
            new MissionVote { RoundId = round.RoundId, Seat = 2, IsSuccess = true }
        };

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);
        _teamRepositoryMock.Setup(x => x.GetByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Team
            {
                TeamId = 1,
                RoundId = round.RoundId,
                Members = new List<TeamMember>
                {
                    new TeamMember { Seat = 1 },
                    new TeamMember { Seat = 2 }
                }
            });
        _missionVoteRepositoryMock.Setup(x => x.GetByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(existingVotes);

        // Act - This is the last vote (fail)
        await _sut.VoteOnMission(gameCode, false);

        // Assert
        game.SabotageCount.Should().Be(1);
        round.Result.Should().Be(RoundResult.Sabotage);
        round.Status.Should().Be(RoundStatus.Completed);
    }

    [Fact]
    public async Task VoteOnMission_WhenThreeSuccesses_ShouldEndGameWithHumanWin()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGameWithRound(gameCode, teamSize: 2, roundStatus: RoundStatus.SecretChoices);
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });
        game.SuccessCount = 2; // Already at 2, this will be the 3rd

        var round = game.Rounds.First();
        var existingVotes = new List<MissionVote>
        {
            new MissionVote { RoundId = round.RoundId, Seat = 2, IsSuccess = true }
        };

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);
        _teamRepositoryMock.Setup(x => x.GetByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Team
            {
                TeamId = 1,
                RoundId = round.RoundId,
                Members = new List<TeamMember>
                {
                    new TeamMember { Seat = 1 },
                    new TeamMember { Seat = 2 }
                }
            });
        _missionVoteRepositoryMock.Setup(x => x.GetByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(existingVotes);

        // Act
        await _sut.VoteOnMission(gameCode, true);

        // Assert
        game.SuccessCount.Should().Be(3);
        game.Status.Should().Be(GameStatus.Finished);
        game.GameWinner.Should().Be(GameResult.Human);
        _clientProxyMock.Verify(
            x => x.SendCoreAsync(
                "GameEnded",
                It.Is<object[]>(o => o.Length == 1),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task VoteOnMission_WhenThreeSabotages_ShouldEndGameWithShapeshifterWin()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGameWithRound(gameCode, teamSize: 2, roundStatus: RoundStatus.SecretChoices);
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });
        game.SabotageCount = 2; // Already at 2, this will be the 3rd

        var round = game.Rounds.First();
        var existingVotes = new List<MissionVote>
        {
            new MissionVote { RoundId = round.RoundId, Seat = 2, IsSuccess = true }
        };

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);
        _teamRepositoryMock.Setup(x => x.GetByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Team
            {
                TeamId = 1,
                RoundId = round.RoundId,
                Members = new List<TeamMember>
                {
                    new TeamMember { Seat = 1 },
                    new TeamMember { Seat = 2 }
                }
            });
        _missionVoteRepositoryMock.Setup(x => x.GetByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(existingVotes);

        // Act
        await _sut.VoteOnMission(gameCode, false);

        // Assert
        game.SabotageCount.Should().Be(3);
        game.Status.Should().Be(GameStatus.Finished);
        game.GameWinner.Should().Be(GameResult.Shapeshifter);
    }

    [Fact]
    public async Task VoteOnMission_WhenAllVotesInAndNotFinished_ShouldCallStartNextRound()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGameWithRound(gameCode, teamSize: 2, roundStatus: RoundStatus.SecretChoices);
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });
        game.Players.Add(new GamePlayer { TempUserId = 67890, Nickname = "Player2", Seat = 2 });
        game.SuccessCount = 0;
        game.RoundCounter = 1;

        var round = game.Rounds.First();
        var existingVotes = new List<MissionVote>
        {
            new MissionVote { RoundId = round.RoundId, Seat = 2, IsSuccess = true }
        };

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);
        _sessionHelperMock.Setup(x => x.GetTempUserId()).Returns(12345);
        _teamRepositoryMock.Setup(x => x.GetByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Team
            {
                TeamId = 1,
                RoundId = round.RoundId,
                Members = new List<TeamMember>
                {
                    new TeamMember { Seat = 1 },
                    new TeamMember { Seat = 2 }
                }
            });
        _missionVoteRepositoryMock.Setup(x => x.GetByRoundIdAsync(It.IsAny<int>()))
            .ReturnsAsync(existingVotes);

        // Act
        await _sut.VoteOnMission(gameCode, true);

        // Assert
        _clientProxyMock.Verify(
            x => x.SendCoreAsync(
                "StartNextRound",
                It.Is<object[]>(o => o.Length == 1),
                default
            ),
            Times.Once
        );
    }

    #endregion

    #region StartNextRound Tests

    [Fact]
    public async Task StartNextRound_WhenGameDoesNotExist_ShouldReturnEarly()
    {
        // Arrange
        var gameCode = "INVALID";
        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync((Game?)null);

        // Act
        await _sut.StartNextRound(gameCode);

        // Assert
        _gamePlayerRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task StartNextRound_ShouldIncrementRoundCounter()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGame(gameCode);
        game.RoundCounter = 1;
        game.LeaderSeat = 1;
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });
        game.Players.Add(new GamePlayer { TempUserId = 67890, Nickname = "Player2", Seat = 2 });

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);

        // Act
        await _sut.StartNextRound(gameCode);

        // Assert
        game.RoundCounter.Should().Be(2);
    }

    [Fact]
    public async Task StartNextRound_ShouldUpdateLeaderSeat()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGame(gameCode);
        game.RoundCounter = 1;
        game.LeaderSeat = 1;
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });
        game.Players.Add(new GamePlayer { TempUserId = 67890, Nickname = "Player2", Seat = 2 });

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);

        // Act
        await _sut.StartNextRound(gameCode);

        // Assert
        game.LeaderSeat.Should().Be(2);
    }


    [Fact]
    public async Task StartNextRound_ShouldBroadcastStartNextRound()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGame(gameCode);
        game.RoundCounter = 1;
        game.LeaderSeat = 1;
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });
        game.Players.Add(new GamePlayer { TempUserId = 67890, Nickname = "Player2", Seat = 2 });

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);

        // Act
        await _sut.StartNextRound(gameCode);

        // Assert
        _clientProxyMock.Verify(
            x => x.SendCoreAsync(
                "StartNextRound",
                It.Is<object[]>(o => o.Length == 1),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task StartNextRound_ShouldSaveChanges()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGame(gameCode);
        game.RoundCounter = 1;
        game.LeaderSeat = 1;
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });
        game.Players.Add(new GamePlayer { TempUserId = 67890, Nickname = "Player2", Seat = 2 });

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);

        // Act
        await _sut.StartNextRound(gameCode);

        // Assert
        _gamePlayerRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task StartNextRound_WhenLeaderAtLastSeat_ShouldWrapToFirstSeat()
    {
        // Arrange
        var gameCode = "ABC123";
        var game = CreateTestGame(gameCode);
        game.RoundCounter = 1;
        game.LeaderSeat = 3; // Last seat
        game.Players.Add(new GamePlayer { TempUserId = 12345, Nickname = "Player1", Seat = 1 });
        game.Players.Add(new GamePlayer { TempUserId = 67890, Nickname = "Player2", Seat = 2 });
        game.Players.Add(new GamePlayer { TempUserId = 11111, Nickname = "Player3", Seat = 3 });

        _gameRepositoryMock.Setup(x => x.GetByCodeWithPlayersAndRoundsAsync(gameCode))
            .ReturnsAsync(game);

        // Act
        await _sut.StartNextRound(gameCode);

        // Assert
        game.LeaderSeat.Should().Be(1); // Should wrap to first seat
    }

    #endregion

    #region GameStarted Tests

    [Fact]
    public async Task GameStarted_ShouldBroadcastGameStartedEvent()
    {
        // Arrange
        var gameCode = "ABC123";

        // Act
        await _sut.GameStarted(gameCode);

        // Assert
        _clientProxyMock.Verify(
            x => x.SendCoreAsync(
                "GameStarted",
                It.Is<object[]>(o => o.Length == 0),
                default
            ),
            Times.Once
        );
    }

    #endregion

    #region Helper Methods

    private Game CreateTestGame(string gameCode)
    {
        return new Game
        {
            GameId = 1,
            ConnectionCode = gameCode,
            LeaderSeat = 1,
            Status = GameStatus.InProgress,
            GameWinner = GameResult.Unknown,
            RoundCounter = 1,
            SuccessCount = 0,
            SabotageCount = 0,
            ConsecutiveRejectedProposals = 0,
            Players = new List<GamePlayer>(),
            Rounds = new List<Round>()
        };
    }

    private Game CreateTestGameWithRound(string gameCode, int teamSize, RoundStatus roundStatus = RoundStatus.TeamSelection)
    {
        var game = CreateTestGame(gameCode);
        var round = new Round
        {
            RoundId = 1,
            GameId = game.GameId,
            RoundNumber = 1,
            LeaderSeat = 1,
            Status = roundStatus,
            Result = RoundResult.Unknown,
            TeamSize = teamSize,
            Game = game,
            Teams = new List<Team>(),
            MissionVotes = new List<MissionVote>()
        };
        game.Rounds.Add(round);
        return game;
    }

    private static JsonElement ToJsonElement(object? obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        var json = JsonSerializer.Serialize(obj);
        return JsonSerializer.Deserialize<JsonElement>(json);
    }

    #endregion
}
