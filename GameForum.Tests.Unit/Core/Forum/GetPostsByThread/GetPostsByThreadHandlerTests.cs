using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Core.Forum.GetPostsByThread;
using GameForum.Domain.Core.Forum;
using Moq;

namespace GameForum.Tests.Unit.Core.Forum.GetPostsByThread;

public class GetPostsByThreadHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IPostRepository> _posts = new();
    private readonly Mock<IAppLogger> _logger = new();

    public GetPostsByThreadHandlerTests()
    {
        _uow.Setup(u => u.Posts).Returns(_posts.Object);
    }

    [Fact]
    public async Task HandleAsync_Existing_ReturnsPosts()
    {
        var threadId = Guid.NewGuid();
        var entities = new List<PostEntity>
        {
            new() { Id = Guid.NewGuid(), ThreadId = threadId, UserId = Guid.NewGuid(), Content = "P1" },
            new() { Id = Guid.NewGuid(), ThreadId = threadId, UserId = Guid.NewGuid(), Content = "P2" }
        };
        _posts
            .Setup(r => r.GetByThreadIdAsync(threadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);
        var handler = new GetPostsByThreadHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(threadId);

        Assert.Null(response.ErrorMessage);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data!.Count);
    }

    [Fact]
    public async Task HandleAsync_EmptyThreadId_ReturnsError()
    {
        var handler = new GetPostsByThreadHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(Guid.Empty);

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_RepositoryThrows_ReturnsErrorAndLogs()
    {
        var threadId = Guid.NewGuid();
        _posts
            .Setup(r => r.GetByThreadIdAsync(threadId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db down"));
        var handler = new GetPostsByThreadHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(threadId);

        Assert.NotNull(response.ErrorMessage);
        _logger.Verify(l => l.Error(It.IsAny<string>()), Times.Once);
    }
}
