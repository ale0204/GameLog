using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Core.Users.GetUserByUsername;
using GameForum.Domain.Core.Users;
using Moq;

namespace GameForum.Tests.Unit.Core.Users.GetUserByUsername;

public class GetUserByUsernameHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IAppLogger> _logger = new();

    public GetUserByUsernameHandlerTests()
    {
        _uow.Setup(u => u.Users).Returns(_users.Object);
    }

    [Fact]
    public async Task HandleAsync_UserExists_ReturnsModel()
    {
        var entity = new UserEntity { Id = Guid.NewGuid(), Username = "alex", DisplayName = "Alex" };
        _users.Setup(r => r.GetByUsernameAsync("alex", It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        var handler = new GetUserByUsernameHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync("alex");

        Assert.Null(response.ErrorMessage);
        Assert.NotNull(response.Data);
        Assert.Equal("alex", response.Data!.Username);
        Assert.Equal(entity.Id, response.Data!.Id);
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsError()
    {
        _users.Setup(r => r.GetByUsernameAsync("ghost", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);
        var handler = new GetUserByUsernameHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync("ghost");

        Assert.NotNull(response.ErrorMessage);
        Assert.Null(response.Data);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task HandleAsync_EmptyUsername_ReturnsError(string? username)
    {
        var handler = new GetUserByUsernameHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(username!);

        Assert.NotNull(response.ErrorMessage);
        _users.Verify(r => r.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
