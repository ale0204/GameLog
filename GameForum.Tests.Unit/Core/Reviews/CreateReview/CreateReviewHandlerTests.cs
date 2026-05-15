using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Models;
using GameForum.Application.Core.Reviews.CreateReview;
using GameForum.Domain.Core.Games;
using GameForum.Domain.Core.Reviews;
using Moq;

namespace GameForum.Tests.Unit.Core.Reviews.CreateReview;

public class CreateReviewHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IReviewRepository> _reviews = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IGameRepository> _games = new();
    private readonly Mock<IAppLogger> _logger = new();

    public CreateReviewHandlerTests()
    {
        _uow.Setup(u => u.Reviews).Returns(_reviews.Object);
        _uow.Setup(u => u.Users).Returns(_users.Object);
        _uow.Setup(u => u.Games).Returns(_games.Object);
    }

    private void SetupUserAndGameExist(Guid userId, Guid gameId)
    {
        _users.Setup(r => r.ExistsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _games.Setup(r => r.GetByIdAsync(gameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GameEntity { Id = gameId });
    }

    [Fact]
    public async Task HandleAsync_HappyPath_CreatesReview()
    {
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        SetupUserAndGameExist(userId, gameId);
        _reviews.Setup(r => r.GetByUserAndGameAsync(userId, gameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReviewEntity?)null);
        var handler = new CreateReviewHandler(_uow.Object, _logger.Object);

        var model = new ReviewModel { UserId = userId, GameId = gameId, Rating = 8, Content = "Solid game." };
        var response = await handler.HandleAsync(model);

        Assert.Null(response.ErrorMessage);
        Assert.NotNull(response.Data);
        Assert.NotEqual(Guid.Empty, response.Data!.Id);
        _reviews.Verify(r => r.AddAsync(It.IsAny<ReviewEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    [InlineData(100)]
    public async Task HandleAsync_RatingOutOfRange_ReturnsError(int rating)
    {
        var handler = new CreateReviewHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new ReviewModel
        {
            UserId = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            Rating = rating,
            Content = "x"
        });

        Assert.NotNull(response.ErrorMessage);
        _reviews.Verify(r => r.AddAsync(It.IsAny<ReviewEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task HandleAsync_EmptyContent_ReturnsError(string? content)
    {
        var handler = new CreateReviewHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new ReviewModel
        {
            UserId = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            Rating = 5,
            Content = content!
        });

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_EmptyUserId_ReturnsError()
    {
        var handler = new CreateReviewHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new ReviewModel
        {
            UserId = Guid.Empty,
            GameId = Guid.NewGuid(),
            Rating = 5,
            Content = "x"
        });

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_EmptyGameId_ReturnsError()
    {
        var handler = new CreateReviewHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new ReviewModel
        {
            UserId = Guid.NewGuid(),
            GameId = Guid.Empty,
            Rating = 5,
            Content = "x"
        });

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_UserDoesNotExist_ReturnsError()
    {
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        _users.Setup(r => r.ExistsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = new CreateReviewHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new ReviewModel
        {
            UserId = userId,
            GameId = gameId,
            Rating = 5,
            Content = "x"
        });

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_GameDoesNotExist_ReturnsError()
    {
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        _users.Setup(r => r.ExistsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _games.Setup(r => r.GetByIdAsync(gameId, It.IsAny<CancellationToken>())).ReturnsAsync((GameEntity?)null);
        var handler = new CreateReviewHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new ReviewModel
        {
            UserId = userId,
            GameId = gameId,
            Rating = 5,
            Content = "x"
        });

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_DuplicateReview_ReturnsError()
    {
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        SetupUserAndGameExist(userId, gameId);
        _reviews.Setup(r => r.GetByUserAndGameAsync(userId, gameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReviewEntity { UserId = userId, GameId = gameId });
        var handler = new CreateReviewHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new ReviewModel
        {
            UserId = userId,
            GameId = gameId,
            Rating = 7,
            Content = "x"
        });

        Assert.NotNull(response.ErrorMessage);
        _reviews.Verify(r => r.AddAsync(It.IsAny<ReviewEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
