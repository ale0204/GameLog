using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Core.Reviews.DeleteReview;
using GameForum.Domain.Core.Reviews;
using Moq;

namespace GameForum.Tests.Unit.Core.Reviews.DeleteReview;

public class DeleteReviewHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IReviewRepository> _reviews = new();
    private readonly Mock<IAppLogger> _logger = new();

    public DeleteReviewHandlerTests()
    {
        _uow.Setup(u => u.Reviews).Returns(_reviews.Object);
    }

    [Fact]
    public async Task HandleAsync_HappyPath_HardDeletesReview()
    {
        var reviewId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var entity = new ReviewEntity { Id = reviewId, UserId = authorId };
        _reviews.Setup(r => r.GetByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        var handler = new DeleteReviewHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new DeleteReviewCommand(reviewId, authorId));

        Assert.Null(response.ErrorMessage);
        Assert.True(response.Data);
        _reviews.Verify(r => r.Delete(entity), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NonAuthor_ReturnsError()
    {
        var reviewId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var entity = new ReviewEntity { Id = reviewId, UserId = authorId };
        _reviews.Setup(r => r.GetByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        var handler = new DeleteReviewHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new DeleteReviewCommand(reviewId, otherUserId));

        Assert.NotNull(response.ErrorMessage);
        _reviews.Verify(r => r.Delete(It.IsAny<ReviewEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ReviewNotFound_ReturnsError()
    {
        var reviewId = Guid.NewGuid();
        _reviews.Setup(r => r.GetByIdAsync(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync((ReviewEntity?)null);
        var handler = new DeleteReviewHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new DeleteReviewCommand(reviewId, Guid.NewGuid()));

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_EmptyId_ReturnsError()
    {
        var handler = new DeleteReviewHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new DeleteReviewCommand(Guid.Empty, Guid.NewGuid()));

        Assert.NotNull(response.ErrorMessage);
    }
}
