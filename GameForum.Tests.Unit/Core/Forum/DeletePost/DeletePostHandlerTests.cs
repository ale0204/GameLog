using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Core.Forum.DeletePost;
using GameForum.Domain.Core.Forum;
using Moq;

namespace GameForum.Tests.Unit.Core.Forum.DeletePost;

public class DeletePostHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IPostRepository> _posts = new();
    private readonly Mock<IAppLogger> _logger = new();

    public DeletePostHandlerTests()
    {
        _uow.Setup(u => u.Posts).Returns(_posts.Object);
    }

    [Fact]
    public async Task HandleAsync_AuthorRequester_SoftDeletesAndReturnsTrue()
    {
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var post = new PostEntity { Id = postId, UserId = authorId, Content = "x", IsDeleted = false };
        _posts
            .Setup(r => r.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);
        var handler = new DeletePostHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new DeletePostCommand(postId, authorId));

        Assert.Null(response.ErrorMessage);
        Assert.True(response.Data);
        Assert.True(post.IsDeleted);
        _posts.Verify(r => r.Update(It.Is<PostEntity>(p => p.Id == postId && p.IsDeleted)), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _posts.Verify(r => r.Delete(It.IsAny<PostEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_PostNotFound_ReturnsError()
    {
        var postId = Guid.NewGuid();
        _posts
            .Setup(r => r.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PostEntity?)null);
        var handler = new DeletePostHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new DeletePostCommand(postId, Guid.NewGuid()));

        Assert.NotNull(response.ErrorMessage);
        _posts.Verify(r => r.Update(It.IsAny<PostEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NonAuthorRequester_ReturnsError()
    {
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var post = new PostEntity { Id = postId, UserId = authorId, Content = "x" };
        _posts
            .Setup(r => r.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);
        var handler = new DeletePostHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new DeletePostCommand(postId, otherId));

        Assert.NotNull(response.ErrorMessage);
        Assert.False(post.IsDeleted);
        _posts.Verify(r => r.Update(It.IsAny<PostEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_AlreadyDeleted_ReturnsError()
    {
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var post = new PostEntity { Id = postId, UserId = authorId, IsDeleted = true };
        _posts
            .Setup(r => r.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);
        var handler = new DeletePostHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new DeletePostCommand(postId, authorId));

        Assert.NotNull(response.ErrorMessage);
        _posts.Verify(r => r.Update(It.IsAny<PostEntity>()), Times.Never);
    }
}
