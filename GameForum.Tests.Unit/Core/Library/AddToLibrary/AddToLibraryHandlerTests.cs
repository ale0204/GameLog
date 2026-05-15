using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Models;
using GameForum.Application.Core.Library.AddToLibrary;
using GameForum.Domain.Core.Games;
using GameForum.Domain.Core.Library;
using GameForum.Domain.Core.Library.Enums;
using GameForum.Domain.Core.Users;
using Moq;

namespace GameForum.Tests.Unit.Core.Library.AddToLibrary;

public class AddToLibraryHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IUserGameRepository> _userGames = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IGameRepository> _games = new();
    private readonly Mock<IAppLogger> _logger = new();

    public AddToLibraryHandlerTests()
    {
        _uow.Setup(u => u.UserGames).Returns(_userGames.Object);
        _uow.Setup(u => u.Users).Returns(_users.Object);
        _uow.Setup(u => u.Games).Returns(_games.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidModel_AddsEntry()
    {
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        _users.Setup(r => r.ExistsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _games.Setup(r => r.GetByIdAsync(gameId, It.IsAny<CancellationToken>())).ReturnsAsync(new GameEntity { Id = gameId });
        _userGames.Setup(r => r.GetByUserAndGameAsync(userId, gameId, It.IsAny<CancellationToken>())).ReturnsAsync((UserGameEntity?)null);
        var model = new UserGameModel { UserId = userId, GameId = gameId, Status = UserGameStatus.Playing };
        var handler = new AddToLibraryHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(model);

        Assert.Null(response.ErrorMessage);
        Assert.NotNull(response.Data);
        _userGames.Verify(r => r.AddAsync(It.IsAny<UserGameEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_DuplicateEntry_ReturnsError()
    {
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        _users.Setup(r => r.ExistsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _games.Setup(r => r.GetByIdAsync(gameId, It.IsAny<CancellationToken>())).ReturnsAsync(new GameEntity { Id = gameId });
        _userGames.Setup(r => r.GetByUserAndGameAsync(userId, gameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserGameEntity { UserId = userId, GameId = gameId });
        var handler = new AddToLibraryHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new UserGameModel { UserId = userId, GameId = gameId });

        Assert.NotNull(response.ErrorMessage);
        _userGames.Verify(r => r.AddAsync(It.IsAny<UserGameEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_UserDoesNotExist_ReturnsError()
    {
        var userId = Guid.NewGuid();
        _users.Setup(r => r.ExistsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = new AddToLibraryHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new UserGameModel { UserId = userId, GameId = Guid.NewGuid() });

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_GameDoesNotExist_ReturnsError()
    {
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        _users.Setup(r => r.ExistsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _games.Setup(r => r.GetByIdAsync(gameId, It.IsAny<CancellationToken>())).ReturnsAsync((GameEntity?)null);
        var handler = new AddToLibraryHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new UserGameModel { UserId = userId, GameId = gameId });

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_PersonalRatingOutOfRange_ReturnsError()
    {
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        _users.Setup(r => r.ExistsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _games.Setup(r => r.GetByIdAsync(gameId, It.IsAny<CancellationToken>())).ReturnsAsync(new GameEntity { Id = gameId });
        var model = new UserGameModel { UserId = userId, GameId = gameId, PersonalRating = 11 };
        var handler = new AddToLibraryHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(model);

        Assert.NotNull(response.ErrorMessage);
    }
}
