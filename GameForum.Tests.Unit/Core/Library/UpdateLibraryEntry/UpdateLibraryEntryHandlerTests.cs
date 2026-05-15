using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Models;
using GameForum.Application.Core.Library.UpdateLibraryEntry;
using GameForum.Domain.Core.Library;
using GameForum.Domain.Core.Library.Enums;
using Moq;

namespace GameForum.Tests.Unit.Core.Library.UpdateLibraryEntry;

public class UpdateLibraryEntryHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IUserGameRepository> _userGames = new();
    private readonly Mock<IAppLogger> _logger = new();

    public UpdateLibraryEntryHandlerTests()
    {
        _uow.Setup(u => u.UserGames).Returns(_userGames.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidModel_UpdatesEntry()
    {
        var entryId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existing = new UserGameEntity { Id = entryId, UserId = userId, GameId = Guid.NewGuid(), Status = UserGameStatus.Wishlist };
        _userGames.Setup(r => r.GetByIdAsync(entryId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        var model = new UserGameModel
        {
            Id = entryId,
            UserId = userId,
            GameId = existing.GameId,
            Status = UserGameStatus.Completed,
            PersonalRating = 8,
            HoursPlayed = 42.5m,
            Notes = "good game"
        };
        var handler = new UpdateLibraryEntryHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(model);

        Assert.Null(response.ErrorMessage);
        Assert.Equal(UserGameStatus.Completed, existing.Status);
        Assert.Equal(8, existing.PersonalRating);
        _userGames.Verify(r => r.Update(existing), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_EntryNotFound_ReturnsError()
    {
        var entryId = Guid.NewGuid();
        _userGames.Setup(r => r.GetByIdAsync(entryId, It.IsAny<CancellationToken>())).ReturnsAsync((UserGameEntity?)null);
        var handler = new UpdateLibraryEntryHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new UserGameModel { Id = entryId, UserId = Guid.NewGuid(), GameId = Guid.NewGuid() });

        Assert.NotNull(response.ErrorMessage);
        _userGames.Verify(r => r.Update(It.IsAny<UserGameEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NonOwnerRequester_ReturnsError()
    {
        var entryId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        _userGames.Setup(r => r.GetByIdAsync(entryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserGameEntity { Id = entryId, UserId = ownerId, GameId = Guid.NewGuid() });
        var model = new UserGameModel { Id = entryId, UserId = otherId, GameId = Guid.NewGuid() };
        var handler = new UpdateLibraryEntryHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(model);

        Assert.NotNull(response.ErrorMessage);
        _userGames.Verify(r => r.Update(It.IsAny<UserGameEntity>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    [InlineData(-1)]
    public async Task HandleAsync_RatingOutOfRange_ReturnsError(int rating)
    {
        var entryId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _userGames.Setup(r => r.GetByIdAsync(entryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserGameEntity { Id = entryId, UserId = userId, GameId = Guid.NewGuid() });
        var handler = new UpdateLibraryEntryHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new UserGameModel { Id = entryId, UserId = userId, GameId = Guid.NewGuid(), PersonalRating = rating });

        Assert.NotNull(response.ErrorMessage);
    }
}
