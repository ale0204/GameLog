using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Core.Games.GetGameById;
using GameForum.Domain.Core.Games;
using Moq;

namespace GameForum.Tests.Unit.Core.Games.GetGameById;

public class GetGameByIdHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IGameRepository> _games = new();
    private readonly Mock<IAppLogger> _logger = new();

    public GetGameByIdHandlerTests()
    {
        _uow.Setup(u => u.Games).Returns(_games.Object);
    }

    [Fact]
    public async Task HandleAsync_Existing_ReturnsGame()
    {
        var id = Guid.NewGuid();
        var entity = new GameEntity { Id = id, Title = "Hades", RawgId = 99 };
        _games
            .Setup(r => r.GetByIdWithDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        var handler = new GetGameByIdHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(id);

        Assert.Null(response.ErrorMessage);
        Assert.NotNull(response.Data);
        Assert.Equal("Hades", response.Data!.Title);
    }

    [Fact]
    public async Task HandleAsync_EmptyId_ReturnsError()
    {
        var handler = new GetGameByIdHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(Guid.Empty);

        Assert.NotNull(response.ErrorMessage);
        _games.Verify(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NotFound_ReturnsError()
    {
        var id = Guid.NewGuid();
        _games
            .Setup(r => r.GetByIdWithDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameEntity?)null);
        var handler = new GetGameByIdHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(id);

        Assert.NotNull(response.ErrorMessage);
        Assert.Null(response.Data);
    }
}
