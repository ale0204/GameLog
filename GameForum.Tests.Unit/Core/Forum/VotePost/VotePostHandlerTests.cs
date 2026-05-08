using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Models;
using GameForum.Application.Core.Forum.VotePost;
using GameForum.Domain.Core.Forum;
using GameForum.Domain.Core.Forum.Enums;
using Moq;

namespace GameForum.Tests.Unit.Core.Forum.VotePost;

public class VotePostHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IVoteRepository> _votes = new();
    private readonly Mock<IPostRepository> _posts = new();
    private readonly Mock<IAppLogger> _logger = new();

    public VotePostHandlerTests()
    {
        _uow.Setup(u => u.Votes).Returns(_votes.Object);
        _uow.Setup(u => u.Posts).Returns(_posts.Object);
    }

    [Fact]
    public async Task HandleAsync_NoExistingVote_AddsNewVote()
    {
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _posts
            .Setup(r => r.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PostEntity { Id = postId });
        _votes
            .Setup(r => r.GetByUserAndPostAsync(userId, postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VoteEntity?)null);
        var model = new VoteModel { PostId = postId, UserId = userId, Value = VoteValue.Up };
        var handler = new VotePostHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(model);

        Assert.Null(response.ErrorMessage);
        Assert.NotNull(response.Data);
        Assert.Equal(VoteValue.Up, response.Data!.Value);
        _votes.Verify(r => r.AddAsync(It.IsAny<VoteEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _votes.Verify(r => r.Update(It.IsAny<VoteEntity>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ExistingVoteSameValue_ReturnsErrorWithoutSaving()
    {
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _posts
            .Setup(r => r.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PostEntity { Id = postId });
        var existing = new VoteEntity { Id = Guid.NewGuid(), PostId = postId, UserId = userId, Value = VoteValue.Up };
        _votes
            .Setup(r => r.GetByUserAndPostAsync(userId, postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        var model = new VoteModel { PostId = postId, UserId = userId, Value = VoteValue.Up };
        var handler = new VotePostHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(model);

        Assert.NotNull(response.ErrorMessage);
        _votes.Verify(r => r.AddAsync(It.IsAny<VoteEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        _votes.Verify(r => r.Update(It.IsAny<VoteEntity>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ExistingVoteDifferentValue_UpdatesVote()
    {
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _posts
            .Setup(r => r.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PostEntity { Id = postId });
        var existing = new VoteEntity { Id = Guid.NewGuid(), PostId = postId, UserId = userId, Value = VoteValue.Down };
        _votes
            .Setup(r => r.GetByUserAndPostAsync(userId, postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        var model = new VoteModel { PostId = postId, UserId = userId, Value = VoteValue.Up };
        var handler = new VotePostHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(model);

        Assert.Null(response.ErrorMessage);
        Assert.Equal(VoteValue.Up, existing.Value);
        _votes.Verify(r => r.Update(It.Is<VoteEntity>(v => v.Value == VoteValue.Up)), Times.Once);
        _votes.Verify(r => r.AddAsync(It.IsAny<VoteEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_PostNotFound_ReturnsError()
    {
        var postId = Guid.NewGuid();
        _posts
            .Setup(r => r.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PostEntity?)null);
        var model = new VoteModel { PostId = postId, UserId = Guid.NewGuid(), Value = VoteValue.Up };
        var handler = new VotePostHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(model);

        Assert.NotNull(response.ErrorMessage);
        _votes.Verify(r => r.AddAsync(It.IsAny<VoteEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_DeletedPost_ReturnsError()
    {
        var postId = Guid.NewGuid();
        _posts
            .Setup(r => r.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PostEntity { Id = postId, IsDeleted = true });
        var model = new VoteModel { PostId = postId, UserId = Guid.NewGuid(), Value = VoteValue.Up };
        var handler = new VotePostHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(model);

        Assert.NotNull(response.ErrorMessage);
        _votes.Verify(r => r.AddAsync(It.IsAny<VoteEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
