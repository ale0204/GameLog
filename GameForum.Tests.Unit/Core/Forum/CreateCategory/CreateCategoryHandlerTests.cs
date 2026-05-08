using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Models;
using GameForum.Application.Core.Forum.CreateCategory;
using GameForum.Domain.Core.Forum;
using Moq;

namespace GameForum.Tests.Unit.Core.Forum.CreateCategory;

public class CreateCategoryHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<IAppLogger> _logger = new();

    public CreateCategoryHandlerTests()
    {
        _uow.Setup(u => u.Categories).Returns(_categories.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidModel_CreatesCategoryAndReturnsModel()
    {
        var model = new CategoryModel { Name = "General", Description = "General talk", SortOrder = 1 };
        var handler = new CreateCategoryHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(model);

        Assert.Null(response.ErrorMessage);
        Assert.NotNull(response.Data);
        Assert.Equal("General", response.Data!.Name);
        Assert.NotEqual(Guid.Empty, response.Data.Id);
        _categories.Verify(r => r.AddAsync(It.IsAny<CategoryEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_EmptyName_ReturnsErrorWithoutSaving()
    {
        var model = new CategoryModel { Name = "  ", Description = "x" };
        var handler = new CreateCategoryHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(model);

        Assert.NotNull(response.ErrorMessage);
        Assert.Null(response.Data);
        _categories.Verify(r => r.AddAsync(It.IsAny<CategoryEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_RepositoryThrows_ReturnsErrorAndLogs()
    {
        var model = new CategoryModel { Name = "General" };
        _categories
            .Setup(r => r.AddAsync(It.IsAny<CategoryEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));
        var handler = new CreateCategoryHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(model);

        Assert.NotNull(response.ErrorMessage);
        _logger.Verify(l => l.Error(It.IsAny<string>()), Times.Once);
    }
}
