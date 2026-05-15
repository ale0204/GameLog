using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Core.Library.UpdateEntryVisibility;
using GameForum.Domain.Core.Library;
using GameForum.Domain.Core.Users.Enums;
using Moq;

namespace GameForum.Tests.Unit.Core.Library.UpdateEntryVisibility;

public class UpdateEntryVisibilityHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IUserGameRepository> _userGames = new();
    private readonly Mock<IAppLogger> _logger = new();

    public UpdateEntryVisibilityHandlerTests()
    {
        _uow.Setup(u => u.UserGames).Returns(_userGames.Object);
    }

    [Fact]
    public async Task HandleAsync_OwnerRequester_UpdatesVisibility()
    {
        var entryId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var entry = new UserGameEntity { Id = entryId, UserId = ownerId, GameId = Guid.NewGuid(), Visibility = Visibility.Public };
        _userGames.Setup(r => r.GetByIdAsync(entryId, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        var handler = new UpdateEntryVisibilityHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new UpdateVisibilityCommand(entryId, Visibility.FriendsOnly, ownerId));

        Assert.Null(response.ErrorMessage);
        Assert.Equal(Visibility.FriendsOnly, entry.Visibility);
        _userGames.Verify(r => r.Update(entry), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NonOwnerRequester_ReturnsError()
    {
        var entryId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var entry = new UserGameEntity { Id = entryId, UserId = ownerId, GameId = Guid.NewGuid() };
        _userGames.Setup(r => r.GetByIdAsync(entryId, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        var handler = new UpdateEntryVisibilityHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new UpdateVisibilityCommand(entryId, Visibility.Private, Guid.NewGuid()));

        Assert.NotNull(response.ErrorMessage);
        _userGames.Verify(r => r.Update(It.IsAny<UserGameEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_EntryNotFound_ReturnsError()
    {
        var entryId = Guid.NewGuid();
        _userGames.Setup(r => r.GetByIdAsync(entryId, It.IsAny<CancellationToken>())).ReturnsAsync((UserGameEntity?)null);
        var handler = new UpdateEntryVisibilityHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new UpdateVisibilityCommand(entryId, Visibility.Public, Guid.NewGuid()));

        Assert.NotNull(response.ErrorMessage);
    }
}
