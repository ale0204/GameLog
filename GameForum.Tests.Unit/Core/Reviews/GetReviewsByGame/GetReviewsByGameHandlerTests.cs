using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Core.Reviews.GetReviewsByGame;
using GameForum.Domain.Core.Reviews;
using Moq;

namespace GameForum.Tests.Unit.Core.Reviews.GetReviewsByGame;

public class GetReviewsByGameHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IReviewRepository> _reviews = new();
    private readonly Mock<IAppLogger> _logger = new();

    public GetReviewsByGameHandlerTests()
    {
        _uow.Setup(u => u.Reviews).Returns(_reviews.Object);
    }

    [Fact]
    public async Task HandleAsync_ReturnsReviewsForGame()
    {
        var gameId = Guid.NewGuid();
        var data = new List<ReviewEntity>
        {
            new() { Id = Guid.NewGuid(), GameId = gameId, UserId = Guid.NewGuid(), Rating = 9, Content = "great" },
            new() { Id = Guid.NewGuid(), GameId = gameId, UserId = Guid.NewGuid(), Rating = 4, Content = "meh" }
        };
        _reviews.Setup(r => r.GetByGameIdAsync(gameId, It.IsAny<CancellationToken>())).ReturnsAsync(data);
        var handler = new GetReviewsByGameHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(gameId);

        Assert.Null(response.ErrorMessage);
        Assert.Equal(2, response.Data!.Count);
    }

    [Fact]
    public async Task HandleAsync_NoReviews_ReturnsEmptyList()
    {
        var gameId = Guid.NewGuid();
        _reviews.Setup(r => r.GetByGameIdAsync(gameId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<ReviewEntity>());
        var handler = new GetReviewsByGameHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(gameId);

        Assert.Null(response.ErrorMessage);
        Assert.Empty(response.Data!);
    }

    [Fact]
    public async Task HandleAsync_EmptyGameId_ReturnsError()
    {
        var handler = new GetReviewsByGameHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(Guid.Empty);

        Assert.NotNull(response.ErrorMessage);
        _reviews.Verify(r => r.GetByGameIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
