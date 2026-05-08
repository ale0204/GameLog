using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Core.Forum.GetThreadsByCategory;
using GameForum.Domain.Core.Forum;
using Moq;

namespace GameForum.Tests.Unit.Core.Forum.GetThreadsByCategory;

public class GetThreadsByCategoryHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IThreadRepository> _threads = new();
    private readonly Mock<IAppLogger> _logger = new();

    public GetThreadsByCategoryHandlerTests()
    {
        _uow.Setup(u => u.Threads).Returns(_threads.Object);
    }

    [Fact]
    public async Task HandleAsync_Existing_ReturnsThreads()
    {
        var categoryId = Guid.NewGuid();
        var entities = new List<ThreadEntity>
        {
            new() { Id = Guid.NewGuid(), Title = "T1", CategoryId = categoryId, UserId = Guid.NewGuid() },
            new() { Id = Guid.NewGuid(), Title = "T2", CategoryId = categoryId, UserId = Guid.NewGuid() }
        };
        _threads
            .Setup(r => r.GetByCategoryIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);
        var handler = new GetThreadsByCategoryHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(categoryId);

        Assert.Null(response.ErrorMessage);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data!.Count);
    }

    [Fact]
    public async Task HandleAsync_EmptyCategoryId_ReturnsError()
    {
        var handler = new GetThreadsByCategoryHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(Guid.Empty);

        Assert.NotNull(response.ErrorMessage);
        _threads.Verify(r => r.GetByCategoryIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_RepositoryThrows_ReturnsErrorAndLogs()
    {
        var categoryId = Guid.NewGuid();
        _threads
            .Setup(r => r.GetByCategoryIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db down"));
        var handler = new GetThreadsByCategoryHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(categoryId);

        Assert.NotNull(response.ErrorMessage);
        _logger.Verify(l => l.Error(It.IsAny<string>()), Times.Once);
    }
}
