using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Core.Messages.GetConversation;
using GameForum.Domain.Core.Social;
using Moq;

namespace GameForum.Tests.Unit.Core.Messages.GetConversation;

public class GetConversationHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IMessageRepository> _messages = new();
    private readonly Mock<IAppLogger> _logger = new();

    public GetConversationHandlerTests()
    {
        _uow.Setup(u => u.Messages).Returns(_messages.Object);
    }

    [Fact]
    public async Task HandleAsync_ReturnsMessagesBetweenUsers()
    {
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        var t0 = DateTime.UtcNow.AddMinutes(-10);
        var data = new List<MessageEntity>
        {
            new() { Id = Guid.NewGuid(), SenderId = userA, ReceiverId = userB, Content = "hi",   CreatedAt = t0 },
            new() { Id = Guid.NewGuid(), SenderId = userB, ReceiverId = userA, Content = "hey", CreatedAt = t0.AddMinutes(1) },
            new() { Id = Guid.NewGuid(), SenderId = userA, ReceiverId = userB, Content = "how", CreatedAt = t0.AddMinutes(2) }
        };
        _messages.Setup(r => r.GetConversationAsync(userA, userB, It.IsAny<CancellationToken>())).ReturnsAsync(data);
        var handler = new GetConversationHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new GetConversationQuery(userA, userB));

        Assert.Null(response.ErrorMessage);
        Assert.Equal(3, response.Data!.Count);
        Assert.Equal("hi", response.Data![0].Content);
        Assert.Equal("how", response.Data![2].Content);
    }

    [Fact]
    public async Task HandleAsync_EmptyConversation_ReturnsEmptyList()
    {
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        _messages.Setup(r => r.GetConversationAsync(userA, userB, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MessageEntity>());
        var handler = new GetConversationHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new GetConversationQuery(userA, userB));

        Assert.Null(response.ErrorMessage);
        Assert.Empty(response.Data!);
    }

    [Fact]
    public async Task HandleAsync_EmptyUserAId_ReturnsError()
    {
        var handler = new GetConversationHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new GetConversationQuery(Guid.Empty, Guid.NewGuid()));

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_EmptyUserBId_ReturnsError()
    {
        var handler = new GetConversationHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new GetConversationQuery(Guid.NewGuid(), Guid.Empty));

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_SameUser_ReturnsError()
    {
        var userId = Guid.NewGuid();
        var handler = new GetConversationHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new GetConversationQuery(userId, userId));

        Assert.NotNull(response.ErrorMessage);
    }
}
