using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Core.Friends.RespondToFriendRequest;
using GameForum.Domain.Core.Social;
using GameForum.Domain.Core.Social.Enums;
using Moq;

namespace GameForum.Tests.Unit.Core.Friends.RespondToFriendRequest;

public class RespondToFriendRequestHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IFriendshipRepository> _friendships = new();
    private readonly Mock<IAppLogger> _logger = new();

    public RespondToFriendRequestHandlerTests()
    {
        _uow.Setup(u => u.Friendships).Returns(_friendships.Object);
    }

    [Theory]
    [InlineData(FriendshipStatus.Accepted)]
    [InlineData(FriendshipStatus.Blocked)]
    public async Task HandleAsync_ReceiverRespondsToPending_UpdatesStatus(FriendshipStatus newStatus)
    {
        var friendshipId = Guid.NewGuid();
        var sender = Guid.NewGuid();
        var receiver = Guid.NewGuid();
        var entity = new FriendshipEntity
        {
            Id = friendshipId,
            SenderId = sender,
            ReceiverId = receiver,
            Status = FriendshipStatus.Pending
        };
        _friendships.Setup(r => r.GetByIdAsync(friendshipId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        var handler = new RespondToFriendRequestHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new RespondToFriendRequestCommand(friendshipId, newStatus, receiver));

        Assert.Null(response.ErrorMessage);
        Assert.Equal(newStatus, response.Data!.Status);
        Assert.Equal(newStatus, entity.Status);
        _friendships.Verify(r => r.Update(entity), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_FriendshipNotFound_ReturnsError()
    {
        var friendshipId = Guid.NewGuid();
        _friendships.Setup(r => r.GetByIdAsync(friendshipId, It.IsAny<CancellationToken>())).ReturnsAsync((FriendshipEntity?)null);
        var handler = new RespondToFriendRequestHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new RespondToFriendRequestCommand(friendshipId, FriendshipStatus.Accepted, Guid.NewGuid()));

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_NonReceiverResponds_ReturnsError()
    {
        var friendshipId = Guid.NewGuid();
        var sender = Guid.NewGuid();
        var receiver = Guid.NewGuid();
        var stranger = Guid.NewGuid();
        var entity = new FriendshipEntity
        {
            Id = friendshipId,
            SenderId = sender,
            ReceiverId = receiver,
            Status = FriendshipStatus.Pending
        };
        _friendships.Setup(r => r.GetByIdAsync(friendshipId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        var handler = new RespondToFriendRequestHandler(_uow.Object, _logger.Object);

        // Sender cannot accept own request
        var response1 = await handler.HandleAsync(new RespondToFriendRequestCommand(friendshipId, FriendshipStatus.Accepted, sender));
        Assert.NotNull(response1.ErrorMessage);

        // Stranger cannot accept either
        var response2 = await handler.HandleAsync(new RespondToFriendRequestCommand(friendshipId, FriendshipStatus.Accepted, stranger));
        Assert.NotNull(response2.ErrorMessage);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(FriendshipStatus.Accepted)]
    [InlineData(FriendshipStatus.Blocked)]
    public async Task HandleAsync_FriendshipNotPending_ReturnsError(FriendshipStatus currentStatus)
    {
        var friendshipId = Guid.NewGuid();
        var receiver = Guid.NewGuid();
        var entity = new FriendshipEntity
        {
            Id = friendshipId,
            SenderId = Guid.NewGuid(),
            ReceiverId = receiver,
            Status = currentStatus
        };
        _friendships.Setup(r => r.GetByIdAsync(friendshipId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        var handler = new RespondToFriendRequestHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new RespondToFriendRequestCommand(friendshipId, FriendshipStatus.Accepted, receiver));

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_NewStatusIsPending_ReturnsError()
    {
        var friendshipId = Guid.NewGuid();
        var receiver = Guid.NewGuid();
        var entity = new FriendshipEntity
        {
            Id = friendshipId,
            SenderId = Guid.NewGuid(),
            ReceiverId = receiver,
            Status = FriendshipStatus.Pending
        };
        _friendships.Setup(r => r.GetByIdAsync(friendshipId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        var handler = new RespondToFriendRequestHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new RespondToFriendRequestCommand(friendshipId, FriendshipStatus.Pending, receiver));

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_EmptyFriendshipId_ReturnsError()
    {
        var handler = new RespondToFriendRequestHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new RespondToFriendRequestCommand(Guid.Empty, FriendshipStatus.Accepted, Guid.NewGuid()));

        Assert.NotNull(response.ErrorMessage);
    }
}
