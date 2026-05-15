using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Core.Library.DeleteLibraryEntry;
using GameForum.Domain.Core.Library;
using Moq;

namespace GameForum.Tests.Unit.Core.Library.DeleteLibraryEntry;

public class DeleteLibraryEntryHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IUserGameRepository> _userGames = new();
    private readonly Mock<IAppLogger> _logger = new();

    public DeleteLibraryEntryHandlerTests()
    {
        _uow.Setup(u => u.UserGames).Returns(_userGames.Object);
    }

    [Fact]
    public async Task HandleAsync_OwnerRequester_HardDeletesEntry()
    {
        var entryId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var entry = new UserGameEntity { Id = entryId, UserId = ownerId, GameId = Guid.NewGuid() };
        _userGames.Setup(r => r.GetByIdAsync(entryId, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        var handler = new DeleteLibraryEntryHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new DeleteLibraryEntryCommand(entryId, ownerId));

        Assert.Null(response.ErrorMessage);
        Assert.True(response.Data);
        _userGames.Verify(r => r.Delete(entry), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NonOwnerRequester_ReturnsError()
    {
        var entryId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        _userGames.Setup(r => r.GetByIdAsync(entryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserGameEntity { Id = entryId, UserId = ownerId, GameId = Guid.NewGuid() });
        var handler = new DeleteLibraryEntryHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new DeleteLibraryEntryCommand(entryId, Guid.NewGuid()));

        Assert.NotNull(response.ErrorMessage);
        _userGames.Verify(r => r.Delete(It.IsAny<UserGameEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_EntryNotFound_ReturnsError()
    {
        var entryId = Guid.NewGuid();
        _userGames.Setup(r => r.GetByIdAsync(entryId, It.IsAny<CancellationToken>())).ReturnsAsync((UserGameEntity?)null);
        var handler = new DeleteLibraryEntryHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new DeleteLibraryEntryCommand(entryId, Guid.NewGuid()));

        Assert.NotNull(response.ErrorMessage);
    }
}
