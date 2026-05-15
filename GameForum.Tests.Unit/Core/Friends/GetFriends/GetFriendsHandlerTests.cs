using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Core.Friends.GetFriends;
using GameForum.Domain.Core.Social;
using GameForum.Domain.Core.Social.Enums;
using Moq;

namespace GameForum.Tests.Unit.Core.Friends.GetFriends;

public class GetFriendsHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IFriendshipRepository> _friendships = new();
    private readonly Mock<IAppLogger> _logger = new();

    public GetFriendsHandlerTests()
    {
        _uow.Setup(u => u.Friendships).Returns(_friendships.Object);
    }

    [Fact]
    public async Task HandleAsync_ReturnsAcceptedFriends()
    {
        var userId = Guid.NewGuid();
        var data = new List<FriendshipEntity>
        {
            new() { Id = Guid.NewGuid(), SenderId = userId, ReceiverId = Guid.NewGuid(), Status = FriendshipStatus.Accepted },
            new() { Id = Guid.NewGuid(), SenderId = Guid.NewGuid(), ReceiverId = userId, Status = FriendshipStatus.Accepted }
        };
        _friendships.Setup(r => r.GetAcceptedFriendsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(data);
        var handler = new GetFriendsHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(userId);

        Assert.Null(response.ErrorMessage);
        Assert.Equal(2, response.Data!.Count);
    }

    [Fact]
    public async Task HandleAsync_NoFriends_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();
        _friendships.Setup(r => r.GetAcceptedFriendsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FriendshipEntity>());
        var handler = new GetFriendsHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(userId);

        Assert.Null(response.ErrorMessage);
        Assert.Empty(response.Data!);
    }

    [Fact]
    public async Task HandleAsync_EmptyUserId_ReturnsError()
    {
        var handler = new GetFriendsHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(Guid.Empty);

        Assert.NotNull(response.ErrorMessage);
        _friendships.Verify(r => r.GetAcceptedFriendsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
