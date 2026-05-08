using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Core.Games.SearchGames;
using GameForum.Domain.Core.Games;
using Moq;

namespace GameForum.Tests.Unit.Core.Games.SearchGames;

public class SearchGamesHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IGameRepository> _games = new();
    private readonly Mock<IAppLogger> _logger = new();

    public SearchGamesHandlerTests()
    {
        _uow.Setup(u => u.Games).Returns(_games.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidQuery_ReturnsPaginatedResults()
    {
        var entities = new List<GameEntity>
        {
            new() { Id = Guid.NewGuid(), Title = "Witcher 3", RawgId = 1 },
            new() { Id = Guid.NewGuid(), Title = "Witcher 2", RawgId = 2 }
        };
        _games
            .Setup(r => r.SearchAsync("witcher", 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);
        var handler = new SearchGamesHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new SearchGamesQuery("witcher"));

        Assert.Null(response.ErrorMessage);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data!.Count);
    }

    [Fact]
    public async Task HandleAsync_PassesPagingValuesToRepository()
    {
        _games
            .Setup(r => r.SearchAsync("rpg", 3, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameEntity>());
        var handler = new SearchGamesHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new SearchGamesQuery("rpg", Page: 3, PageSize: 10));

        Assert.Null(response.ErrorMessage);
        _games.Verify(r => r.SearchAsync("rpg", 3, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_EmptyQuery_ReturnsError()
    {
        var handler = new SearchGamesHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new SearchGamesQuery("  "));

        Assert.NotNull(response.ErrorMessage);
        _games.Verify(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_InvalidPaging_ReturnsError()
    {
        var handler = new SearchGamesHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new SearchGamesQuery("witcher", Page: 0, PageSize: 20));

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_RepositoryThrows_ReturnsErrorAndLogs()
    {
        _games
            .Setup(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db down"));
        var handler = new SearchGamesHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(new SearchGamesQuery("witcher"));

        Assert.NotNull(response.ErrorMessage);
        _logger.Verify(l => l.Error(It.IsAny<string>()), Times.Once);
    }
}
