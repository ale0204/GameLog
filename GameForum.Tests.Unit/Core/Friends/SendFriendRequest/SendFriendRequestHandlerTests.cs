using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Core.Friends.SendFriendRequest;
using GameForum.Domain.Core.Social;
using GameForum.Domain.Core.Social.Enums;
using GameForum.Domain.Core.Users;
using Moq;

namespace GameForum.Tests.Unit.Core.Friends.SendFriendRequest;

public class SendFriendRequestHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IFriendshipRepository> _friendships = new();
    private readonly Mock<IAppLogger> _logger = new();

    public SendFriendRequestHandlerTests()
    {
        _uow.Setup(u => u.Users).Returns(_users.Object);
        _uow.Setup(u => u.Friendships).Returns(_friendships.Object);
    }

    private void SetupBothUsersExist(Guid sender, Guid receiver)
    {
        _users.Setup(r => r.ExistsAsync(sender, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _users.Setup(r => r.ExistsAsync(receiver, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    [Fact]
    public async Task HandleAsync_HappyPath_CreatesPendingFriendship()
    {
        var sender = Guid.NewGuid();
        var receiver = Guid.NewGuid();
        SetupBothUsersExist(sender, receiver);
        _friendships.Setup(r => r.GetRelationAsync(sender, receiver, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FriendshipEntity?)null);
        var handler = new SendFriendRequestHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new SendFriendRequestCommand(sender, receiver));

        Assert.Null(response.ErrorMessage);
        Assert.NotNull(response.Data);
        Assert.Equal(FriendshipStatus.Pending, response.Data!.Status);
        Assert.Equal(sender, response.Data!.SenderId);
        Assert.Equal(receiver, response.Data!.ReceiverId);
        _friendships.Verify(r => r.AddAsync(It.IsAny<FriendshipEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_SelfRequest_ReturnsError()
    {
        var userId = Guid.NewGuid();
        var handler = new SendFriendRequestHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new SendFriendRequestCommand(userId, userId));

        Assert.NotNull(response.ErrorMessage);
        _friendships.Verify(r => r.AddAsync(It.IsAny<FriendshipEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_EmptySenderId_ReturnsError()
    {
        var handler = new SendFriendRequestHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new SendFriendRequestCommand(Guid.Empty, Guid.NewGuid()));

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_EmptyReceiverId_ReturnsError()
    {
        var handler = new SendFriendRequestHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new SendFriendRequestCommand(Guid.NewGuid(), Guid.Empty));

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_SenderDoesNotExist_ReturnsError()
    {
        var sender = Guid.NewGuid();
        var receiver = Guid.NewGuid();
        _users.Setup(r => r.ExistsAsync(sender, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _users.Setup(r => r.ExistsAsync(receiver, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new SendFriendRequestHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new SendFriendRequestCommand(sender, receiver));

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_ReceiverDoesNotExist_ReturnsError()
    {
        var sender = Guid.NewGuid();
        var receiver = Guid.NewGuid();
        _users.Setup(r => r.ExistsAsync(sender, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _users.Setup(r => r.ExistsAsync(receiver, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = new SendFriendRequestHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new SendFriendRequestCommand(sender, receiver));

        Assert.NotNull(response.ErrorMessage);
    }

    [Theory]
    [InlineData(FriendshipStatus.Pending)]
    [InlineData(FriendshipStatus.Accepted)]
    [InlineData(FriendshipStatus.Blocked)]
    public async Task HandleAsync_RelationAlreadyExists_ReturnsError(FriendshipStatus existingStatus)
    {
        var sender = Guid.NewGuid();
        var receiver = Guid.NewGuid();
        SetupBothUsersExist(sender, receiver);
        _friendships.Setup(r => r.GetRelationAsync(sender, receiver, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FriendshipEntity { SenderId = sender, ReceiverId = receiver, Status = existingStatus });
        var handler = new SendFriendRequestHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new SendFriendRequestCommand(sender, receiver));

        Assert.NotNull(response.ErrorMessage);
        _friendships.Verify(r => r.AddAsync(It.IsAny<FriendshipEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
