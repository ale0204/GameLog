using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.ExternalServices;
using GameForum.Application.Common.Logging;
using GameForum.Application.Core.Games.SyncGameFromRawg;
using GameForum.Domain.Core.Games;
using Moq;

namespace GameForum.Tests.Unit.Core.Games.SyncGameFromRawg;

public class SyncGameFromRawgHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IGameRepository> _games = new();
    private readonly Mock<IGenreRepository> _genres = new();
    private readonly Mock<IPlatformRepository> _platforms = new();
    private readonly Mock<IRawgClient> _rawg = new();
    private readonly Mock<IAppLogger> _logger = new();

    public SyncGameFromRawgHandlerTests()
    {
        _uow.Setup(u => u.Games).Returns(_games.Object);
        _uow.Setup(u => u.Genres).Returns(_genres.Object);
        _uow.Setup(u => u.Platforms).Returns(_platforms.Object);
    }

    private static RawgGameDto MakeDto(int id = 99) =>
        new(id, "Hades", "https://img/cover.jpg", "Roguelike", 2020, "Supergiant", 4.7, 1000,
            new List<string> { "Action", "RPG" },
            new List<string> { "PC", "Switch" });

    [Fact]
    public async Task HandleAsync_NewGame_InsertsAndCreatesGenresAndPlatforms()
    {
        var dto = MakeDto();
        _rawg.Setup(c => c.GetGameDetailsAsync(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _games.Setup(r => r.GetByRawgIdAsync(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync((GameEntity?)null);
        _genres.Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((GenreEntity?)null);
        _platforms.Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((PlatformEntity?)null);
        var handler = new SyncGameFromRawgHandler(_uow.Object, _rawg.Object, _logger.Object);

        var response = await handler.HandleAsync(dto.Id);

        Assert.Null(response.ErrorMessage);
        Assert.NotNull(response.Data);
        Assert.Equal("Hades", response.Data!.Title);
        Assert.Equal(2, response.Data.Genres.Count);
        Assert.Equal(2, response.Data.Platforms.Count);
        _games.Verify(r => r.AddAsync(It.IsAny<GameEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _games.Verify(r => r.Update(It.IsAny<GameEntity>()), Times.Never);
        _genres.Verify(r => r.AddAsync(It.IsAny<GenreEntity>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _platforms.Verify(r => r.AddAsync(It.IsAny<PlatformEntity>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ExistingGame_UpdatesAndReusesGenres()
    {
        var dto = MakeDto();
        var existingGame = new GameEntity
        {
            Id = Guid.NewGuid(),
            RawgId = dto.Id,
            Title = "Old Title",
            AverageRating = 4.0,
            TotalPlayers = 100
        };
        var existingAction = new GenreEntity { Id = Guid.NewGuid(), Name = "Action" };
        var existingPc = new PlatformEntity { Id = Guid.NewGuid(), Name = "PC" };

        _rawg.Setup(c => c.GetGameDetailsAsync(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _games.Setup(r => r.GetByRawgIdAsync(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existingGame);
        _genres.Setup(r => r.GetByNameAsync("Action", It.IsAny<CancellationToken>())).ReturnsAsync(existingAction);
        _genres.Setup(r => r.GetByNameAsync("RPG", It.IsAny<CancellationToken>())).ReturnsAsync((GenreEntity?)null);
        _platforms.Setup(r => r.GetByNameAsync("PC", It.IsAny<CancellationToken>())).ReturnsAsync(existingPc);
        _platforms.Setup(r => r.GetByNameAsync("Switch", It.IsAny<CancellationToken>())).ReturnsAsync((PlatformEntity?)null);
        var handler = new SyncGameFromRawgHandler(_uow.Object, _rawg.Object, _logger.Object);

        var response = await handler.HandleAsync(dto.Id);

        Assert.Null(response.ErrorMessage);
        Assert.Equal("Hades", existingGame.Title);
        Assert.Equal(4.7, existingGame.AverageRating);
        _games.Verify(r => r.Update(existingGame), Times.Once);
        _games.Verify(r => r.AddAsync(It.IsAny<GameEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        _genres.Verify(r => r.AddAsync(It.IsAny<GenreEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _platforms.Verify(r => r.AddAsync(It.IsAny<PlatformEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_RawgReturnsNull_ReturnsError()
    {
        _rawg.Setup(c => c.GetGameDetailsAsync(404, It.IsAny<CancellationToken>())).ReturnsAsync((RawgGameDto?)null);
        var handler = new SyncGameFromRawgHandler(_uow.Object, _rawg.Object, _logger.Object);

        var response = await handler.HandleAsync(404);

        Assert.NotNull(response.ErrorMessage);
        _games.Verify(r => r.AddAsync(It.IsAny<GameEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        _games.Verify(r => r.Update(It.IsAny<GameEntity>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_InvalidRawgId_ReturnsError()
    {
        var handler = new SyncGameFromRawgHandler(_uow.Object, _rawg.Object, _logger.Object);

        var response = await handler.HandleAsync(0);

        Assert.NotNull(response.ErrorMessage);
        _rawg.Verify(c => c.GetGameDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_RawgClientThrows_ReturnsErrorAndLogs()
    {
        _rawg.Setup(c => c.GetGameDetailsAsync(99, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("rawg unreachable"));
        var handler = new SyncGameFromRawgHandler(_uow.Object, _rawg.Object, _logger.Object);

        var response = await handler.HandleAsync(99);

        Assert.NotNull(response.ErrorMessage);
        _logger.Verify(l => l.Error(It.IsAny<string>()), Times.Once);
    }
}
