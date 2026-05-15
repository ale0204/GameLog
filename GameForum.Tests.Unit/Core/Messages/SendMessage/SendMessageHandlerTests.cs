using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Models;
using GameForum.Application.Core.Messages.SendMessage;
using GameForum.Domain.Core.Social;
using GameForum.Domain.Core.Social.Enums;
using Moq;

namespace GameForum.Tests.Unit.Core.Messages.SendMessage;

public class SendMessageHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IMessageRepository> _messages = new();
    private readonly Mock<IFriendshipRepository> _friendships = new();
    private readonly Mock<IAppLogger> _logger = new();

    public SendMessageHandlerTests()
    {
        _uow.Setup(u => u.Messages).Returns(_messages.Object);
        _uow.Setup(u => u.Friendships).Returns(_friendships.Object);
    }

    private void SetupFriendship(Guid senderId, Guid receiverId, FriendshipStatus? status)
    {
        _friendships.Setup(r => r.GetRelationAsync(senderId, receiverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(status is null
                ? null
                : new FriendshipEntity { SenderId = senderId, ReceiverId = receiverId, Status = status.Value });
    }

    [Fact]
    public async Task HandleAsync_AcceptedFriendship_SendsMessage()
    {
        var sender = Guid.NewGuid();
        var receiver = Guid.NewGuid();
        SetupFriendship(sender, receiver, FriendshipStatus.Accepted);
        var handler = new SendMessageHandler(_uow.Object, _logger.Object);

        var model = new MessageModel { SenderId = sender, ReceiverId = receiver, Content = "hey" };
        var response = await handler.HandleAsync(model);

        Assert.Null(response.ErrorMessage);
        Assert.NotNull(response.Data);
        Assert.NotEqual(Guid.Empty, response.Data!.Id);
        _messages.Verify(r => r.AddAsync(It.IsAny<MessageEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(FriendshipStatus.Pending)]
    [InlineData(FriendshipStatus.Blocked)]
    public async Task HandleAsync_NonAcceptedFriendship_ReturnsError(FriendshipStatus status)
    {
        var sender = Guid.NewGuid();
        var receiver = Guid.NewGuid();
        SetupFriendship(sender, receiver, status);
        var handler = new SendMessageHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new MessageModel { SenderId = sender, ReceiverId = receiver, Content = "hey" });

        Assert.NotNull(response.ErrorMessage);
        _messages.Verify(r => r.AddAsync(It.IsAny<MessageEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NoFriendship_ReturnsError()
    {
        var sender = Guid.NewGuid();
        var receiver = Guid.NewGuid();
        SetupFriendship(sender, receiver, null);
        var handler = new SendMessageHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new MessageModel { SenderId = sender, ReceiverId = receiver, Content = "hey" });

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_SelfMessage_ReturnsError()
    {
        var userId = Guid.NewGuid();
        var handler = new SendMessageHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new MessageModel { SenderId = userId, ReceiverId = userId, Content = "talking to myself" });

        Assert.NotNull(response.ErrorMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task HandleAsync_EmptyContent_ReturnsError(string? content)
    {
        var handler = new SendMessageHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new MessageModel
        {
            SenderId = Guid.NewGuid(),
            ReceiverId = Guid.NewGuid(),
            Content = content!
        });

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_EmptySenderId_ReturnsError()
    {
        var handler = new SendMessageHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new MessageModel
        {
            SenderId = Guid.Empty,
            ReceiverId = Guid.NewGuid(),
            Content = "hi"
        });

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_EmptyReceiverId_ReturnsError()
    {
        var handler = new SendMessageHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new MessageModel
        {
            SenderId = Guid.NewGuid(),
            ReceiverId = Guid.Empty,
            Content = "hi"
        });

        Assert.NotNull(response.ErrorMessage);
    }
}
