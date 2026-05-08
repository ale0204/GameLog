using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Models;
using GameForum.Application.Core.Forum.CreatePost;
using GameForum.Domain.Core.Forum;
using Moq;

namespace GameForum.Tests.Unit.Core.Forum.CreatePost;

public class CreatePostHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IPostRepository> _posts = new();
    private readonly Mock<IThreadRepository> _threads = new();
    private readonly Mock<IAppLogger> _logger = new();

    public CreatePostHandlerTests()
    {
        _uow.Setup(u => u.Posts).Returns(_posts.Object);
        _uow.Setup(u => u.Threads).Returns(_threads.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidModel_CreatesPostAndUpdatesThreadActivity()
    {
        var threadId = Guid.NewGuid();
        var thread = new ThreadEntity { Id = threadId, Title = "T", LastActivityAt = DateTime.UtcNow.AddDays(-1) };
        _threads
            .Setup(r => r.GetByIdAsync(threadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(thread);
        var model = new PostModel { ThreadId = threadId, UserId = Guid.NewGuid(), Content = "Hi" };
        var handler = new CreatePostHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(model);

        Assert.Null(response.ErrorMessage);
        Assert.NotNull(response.Data);
        Assert.Equal("Hi", response.Data!.Content);
        _posts.Verify(r => r.AddAsync(It.IsAny<PostEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _threads.Verify(r => r.Update(It.Is<ThreadEntity>(t => t.Id == threadId)), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_EmptyContent_ReturnsError()
    {
        var model = new PostModel { ThreadId = Guid.NewGuid(), UserId = Guid.NewGuid(), Content = "  " };
        var handler = new CreatePostHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(model);

        Assert.NotNull(response.ErrorMessage);
        _posts.Verify(r => r.AddAsync(It.IsAny<PostEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NonexistentThread_ReturnsError()
    {
        var threadId = Guid.NewGuid();
        _threads
            .Setup(r => r.GetByIdAsync(threadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ThreadEntity?)null);
        var model = new PostModel { ThreadId = threadId, UserId = Guid.NewGuid(), Content = "Hi" };
        var handler = new CreatePostHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(model);

        Assert.NotNull(response.ErrorMessage);
        _posts.Verify(r => r.AddAsync(It.IsAny<PostEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_LockedThread_ReturnsError()
    {
        var threadId = Guid.NewGuid();
        _threads
            .Setup(r => r.GetByIdAsync(threadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ThreadEntity { Id = threadId, IsLocked = true });
        var model = new PostModel { ThreadId = threadId, UserId = Guid.NewGuid(), Content = "Hi" };
        var handler = new CreatePostHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(model);

        Assert.NotNull(response.ErrorMessage);
        _posts.Verify(r => r.AddAsync(It.IsAny<PostEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
