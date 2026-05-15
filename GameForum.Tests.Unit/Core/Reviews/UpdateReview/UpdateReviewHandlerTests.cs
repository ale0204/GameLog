using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Models;
using GameForum.Application.Core.Reviews.UpdateReview;
using GameForum.Domain.Core.Reviews;
using Moq;

namespace GameForum.Tests.Unit.Core.Reviews.UpdateReview;

public class UpdateReviewHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IReviewRepository> _reviews = new();
    private readonly Mock<IAppLogger> _logger = new();

    public UpdateReviewHandlerTests()
    {
        _uow.Setup(u => u.Reviews).Returns(_reviews.Object);
    }

    [Fact]
    public async Task HandleAsync_HappyPath_UpdatesReview()
    {
        var reviewId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var existing = new ReviewEntity { Id = reviewId, UserId = authorId, GameId = Guid.NewGuid(), Rating = 5, Content = "old" };
        _reviews.Setup(r => r.GetByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        var handler = new UpdateReviewHandler(_uow.Object, _logger.Object);

        var model = new ReviewModel { Id = reviewId, UserId = authorId, Rating = 9, Content = "much better now" };
        var response = await handler.HandleAsync(model);

        Assert.Null(response.ErrorMessage);
        Assert.Equal(9, response.Data!.Rating);
        Assert.Equal("much better now", response.Data!.Content);
        _reviews.Verify(r => r.Update(existing), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_EmptyId_ReturnsError()
    {
        var handler = new UpdateReviewHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new ReviewModel { Id = Guid.Empty, UserId = Guid.NewGuid(), Rating = 5, Content = "x" });

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_ReviewNotFound_ReturnsError()
    {
        var reviewId = Guid.NewGuid();
        _reviews.Setup(r => r.GetByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync((ReviewEntity?)null);
        var handler = new UpdateReviewHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new ReviewModel { Id = reviewId, UserId = Guid.NewGuid(), Rating = 5, Content = "x" });

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_NonAuthor_ReturnsError()
    {
        var reviewId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var existing = new ReviewEntity { Id = reviewId, UserId = authorId, GameId = Guid.NewGuid(), Rating = 5, Content = "x" };
        _reviews.Setup(r => r.GetByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        var handler = new UpdateReviewHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new ReviewModel { Id = reviewId, UserId = otherUserId, Rating = 5, Content = "x" });

        Assert.NotNull(response.ErrorMessage);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public async Task HandleAsync_RatingOutOfRange_ReturnsError(int rating)
    {
        var handler = new UpdateReviewHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new ReviewModel { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Rating = rating, Content = "x" });

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_EmptyContent_ReturnsError()
    {
        var handler = new UpdateReviewHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new ReviewModel { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Rating = 5, Content = "" });

        Assert.NotNull(response.ErrorMessage);
    }
}
