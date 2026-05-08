using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Models;
using GameForum.Application.Core.Forum.CreateThread;
using GameForum.Domain.Core.Forum;
using Moq;

namespace GameForum.Tests.Unit.Core.Forum.CreateThread;

public class CreateThreadHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IThreadRepository> _threads = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<IAppLogger> _logger = new();

    public CreateThreadHandlerTests()
    {
        _uow.Setup(u => u.Threads).Returns(_threads.Object);
        _uow.Setup(u => u.Categories).Returns(_categories.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidModel_CreatesThread()
    {
        var categoryId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _categories
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CategoryEntity { Id = categoryId, Name = "General" });
        var model = new ThreadModel { Title = "Hello", UserId = userId, CategoryId = categoryId };
        var handler = new CreateThreadHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(model);

        Assert.Null(response.ErrorMessage);
        Assert.NotNull(response.Data);
        Assert.Equal("Hello", response.Data!.Title);
        Assert.NotEqual(Guid.Empty, response.Data.Id);
        _threads.Verify(r => r.AddAsync(It.IsAny<ThreadEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_EmptyTitle_ReturnsError()
    {
        var model = new ThreadModel { Title = "", UserId = Guid.NewGuid(), CategoryId = Guid.NewGuid() };
        var handler = new CreateThreadHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(model);

        Assert.NotNull(response.ErrorMessage);
        Assert.Null(response.Data);
        _threads.Verify(r => r.AddAsync(It.IsAny<ThreadEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_MissingUserId_ReturnsError()
    {
        var model = new ThreadModel { Title = "T", UserId = Guid.Empty, CategoryId = Guid.NewGuid() };
        var handler = new CreateThreadHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(model);

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_NonexistentCategory_ReturnsError()
    {
        var categoryId = Guid.NewGuid();
        _categories
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CategoryEntity?)null);
        var model = new ThreadModel { Title = "T", UserId = Guid.NewGuid(), CategoryId = categoryId };
        var handler = new CreateThreadHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(model);

        Assert.NotNull(response.ErrorMessage);
        _threads.Verify(r => r.AddAsync(It.IsAny<ThreadEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
