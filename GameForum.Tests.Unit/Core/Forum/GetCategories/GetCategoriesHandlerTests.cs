using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Core.Forum.GetCategories;
using GameForum.Domain.Core.Forum;
using Moq;

namespace GameForum.Tests.Unit.Core.Forum.GetCategories;

public class GetCategoriesHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<IAppLogger> _logger = new();

    public GetCategoriesHandlerTests()
    {
        _uow.Setup(u => u.Categories).Returns(_categories.Object);
    }

    [Fact]
    public async Task HandleAsync_ReturnsAllCategoriesAsModels()
    {
        var entities = new List<CategoryEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "General", SortOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "Off-topic", SortOrder = 2 }
        };
        _categories
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);
        var handler = new GetCategoriesHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync();

        Assert.Null(response.ErrorMessage);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data!.Count);
    }

    [Fact]
    public async Task HandleAsync_NoCategories_ReturnsEmptyList()
    {
        _categories
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CategoryEntity>());
        var handler = new GetCategoriesHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync();

        Assert.Null(response.ErrorMessage);
        Assert.NotNull(response.Data);
        Assert.Empty(response.Data!);
    }

    [Fact]
    public async Task HandleAsync_RepositoryThrows_ReturnsErrorAndLogs()
    {
        _categories
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db down"));
        var handler = new GetCategoriesHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync();

        Assert.NotNull(response.ErrorMessage);
        _logger.Verify(l => l.Error(It.IsAny<string>()), Times.Once);
    }
}
