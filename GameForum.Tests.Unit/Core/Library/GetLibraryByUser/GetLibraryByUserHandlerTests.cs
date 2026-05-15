using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Core.Library.GetLibraryByUser;
using GameForum.Domain.Core.Library;
using GameForum.Domain.Core.Social;
using GameForum.Domain.Core.Social.Enums;
using GameForum.Domain.Core.Users;
using GameForum.Domain.Core.Users.Enums;
using Moq;

namespace GameForum.Tests.Unit.Core.Library.GetLibraryByUser;

public class GetLibraryByUserHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IUserGameRepository> _userGames = new();
    private readonly Mock<IFriendshipRepository> _friendships = new();
    private readonly Mock<IAppLogger> _logger = new();

    public GetLibraryByUserHandlerTests()
    {
        _uow.Setup(u => u.Users).Returns(_users.Object);
        _uow.Setup(u => u.UserGames).Returns(_userGames.Object);
        _uow.Setup(u => u.Friendships).Returns(_friendships.Object);
    }

    private void SetupOwner(Guid ownerId, bool isProfilePublic, params Visibility[] entryVisibilities)
    {
        _users.Setup(r => r.GetByIdAsync(ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserEntity { Id = ownerId, IsProfilePublic = isProfilePublic, Username = "owner" });
        var entries = entryVisibilities
            .Select(v => new UserGameEntity { Id = Guid.NewGuid(), UserId = ownerId, GameId = Guid.NewGuid(), Visibility = v })
            .ToList();
        _userGames.Setup(r => r.GetByUserIdAsync(ownerId, It.IsAny<CancellationToken>())).ReturnsAsync(entries);
    }

    private void SetupFriendship(Guid ownerId, Guid? requesterId, FriendshipStatus? status)
    {
        if (requesterId is null) return;
        _friendships.Setup(r => r.GetRelationAsync(ownerId, requesterId.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(status is null
                ? null
                : new FriendshipEntity { SenderId = ownerId, ReceiverId = requesterId.Value, Status = status.Value });
    }

    [Fact]
    public async Task HandleAsync_OwnerSelf_ReturnsAllEntriesIncludingPrivate()
    {
        var ownerId = Guid.NewGuid();
        SetupOwner(ownerId, isProfilePublic: false, Visibility.Public, Visibility.FriendsOnly, Visibility.Private);
        var handler = new GetLibraryByUserHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new GetLibraryByUserQuery(ownerId, ownerId));

        Assert.Null(response.ErrorMessage);
        Assert.Equal(3, response.Data!.Count);
    }

    [Theory]
    // public profile
    [InlineData(true, Visibility.Public, "stranger", true)]
    [InlineData(true, Visibility.FriendsOnly, "stranger", false)]
    [InlineData(true, Visibility.FriendsOnly, "friend", true)]
    [InlineData(true, Visibility.Private, "friend", false)]
    [InlineData(true, Visibility.Public, "anonymous", true)]
    // private profile
    [InlineData(false, Visibility.Public, "stranger", false)]
    [InlineData(false, Visibility.Public, "friend", true)]
    [InlineData(false, Visibility.FriendsOnly, "friend", true)]
    [InlineData(false, Visibility.Public, "anonymous", false)]
    public async Task HandleAsync_VisibilityMatrix(bool ownerProfilePublic, Visibility entryVisibility, string relation, bool expectedVisible)
    {
        var ownerId = Guid.NewGuid();
        SetupOwner(ownerId, ownerProfilePublic, entryVisibility);

        Guid? requesterId = relation == "anonymous" ? null : Guid.NewGuid();
        FriendshipStatus? friendStatus = relation switch
        {
            "friend" => FriendshipStatus.Accepted,
            "stranger" => null,
            "anonymous" => null,
            _ => null
        };
        SetupFriendship(ownerId, requesterId, friendStatus);

        var handler = new GetLibraryByUserHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new GetLibraryByUserQuery(ownerId, requesterId));

        Assert.Null(response.ErrorMessage);
        Assert.NotNull(response.Data);
        if (expectedVisible)
            Assert.Single(response.Data!);
        else
            Assert.Empty(response.Data!);
    }

    [Fact]
    public async Task HandleAsync_FriendshipPendingNotAccepted_TreatedAsStranger()
    {
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        SetupOwner(ownerId, isProfilePublic: true, Visibility.FriendsOnly);
        SetupFriendship(ownerId, requesterId, FriendshipStatus.Pending);
        var handler = new GetLibraryByUserHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new GetLibraryByUserQuery(ownerId, requesterId));

        Assert.Null(response.ErrorMessage);
        Assert.Empty(response.Data!);
    }

    [Fact]
    public async Task HandleAsync_OwnerNotFound_ReturnsError()
    {
        var ownerId = Guid.NewGuid();
        _users.Setup(r => r.GetByIdAsync(ownerId, It.IsAny<CancellationToken>())).ReturnsAsync((UserEntity?)null);
        var handler = new GetLibraryByUserHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new GetLibraryByUserQuery(ownerId, Guid.NewGuid()));

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_EmptyOwnerId_ReturnsError()
    {
        var handler = new GetLibraryByUserHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new GetLibraryByUserQuery(Guid.Empty, null));

        Assert.NotNull(response.ErrorMessage);
    }
}
