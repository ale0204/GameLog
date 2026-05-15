using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Models;
using GameForum.Application.Core.Users.UpdateUserSettings;
using GameForum.Domain.Core.Library;
using GameForum.Domain.Core.Users;
using GameForum.Domain.Core.Users.Enums;
using Moq;

namespace GameForum.Tests.Unit.Core.Users.UpdateUserSettings;

public class UpdateUserSettingsHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IUserGameRepository> _userGames = new();
    private readonly Mock<IAppLogger> _logger = new();

    public UpdateUserSettingsHandlerTests()
    {
        _uow.Setup(u => u.Users).Returns(_users.Object);
        _uow.Setup(u => u.UserGames).Returns(_userGames.Object);
    }

    [Fact]
    public async Task HandleAsync_HappyPath_UpdatesAndReturnsModel()
    {
        var userId = Guid.NewGuid();
        var existing = new UserEntity { Id = userId, Username = "alex", DisplayName = "Old", IsProfilePublic = true };
        _users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        var handler = new UpdateUserSettingsHandler(_uow.Object, _logger.Object);

        var model = new UserModel
        {
            Id = userId,
            DisplayName = "New",
            Bio = "Hello",
            AvatarUrl = "http://a.png",
            IsProfilePublic = false,
            DefaultGameVisibility = Visibility.FriendsOnly
        };
        var response = await handler.HandleAsync(model);

        Assert.Null(response.ErrorMessage);
        Assert.NotNull(response.Data);
        Assert.Equal("New", response.Data!.DisplayName);
        Assert.False(response.Data!.IsProfilePublic);
        _users.Verify(r => r.Update(existing), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_EmptyId_ReturnsError()
    {
        var handler = new UpdateUserSettingsHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new UserModel { Id = Guid.Empty });

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsError()
    {
        var userId = Guid.NewGuid();
        _users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((UserEntity?)null);
        var handler = new UpdateUserSettingsHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new UserModel { Id = userId });

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_CurrentlyPlayingGameNotInLibrary_ReturnsError()
    {
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var existing = new UserEntity { Id = userId, Username = "alex" };
        _users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _userGames.Setup(r => r.GetByUserAndGameAsync(userId, gameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserGameEntity?)null);
        var handler = new UpdateUserSettingsHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new UserModel { Id = userId, CurrentlyPlayingGameId = gameId });

        Assert.NotNull(response.ErrorMessage);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_CurrentlyPlayingGameInLibrary_Succeeds()
    {
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var existing = new UserEntity { Id = userId, Username = "alex" };
        _users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _userGames.Setup(r => r.GetByUserAndGameAsync(userId, gameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserGameEntity { UserId = userId, GameId = gameId });
        var handler = new UpdateUserSettingsHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new UserModel { Id = userId, CurrentlyPlayingGameId = gameId });

        Assert.Null(response.ErrorMessage);
        Assert.Equal(gameId, response.Data!.CurrentlyPlayingGameId);
    }

    [Fact]
    public async Task HandleAsync_NullCurrentlyPlayingGame_DoesNotQueryLibrary()
    {
        var userId = Guid.NewGuid();
        var existing = new UserEntity { Id = userId, Username = "alex" };
        _users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        var handler = new UpdateUserSettingsHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new UserModel { Id = userId, CurrentlyPlayingGameId = null });

        Assert.Null(response.ErrorMessage);
        _userGames.Verify(r => r.GetByUserAndGameAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
