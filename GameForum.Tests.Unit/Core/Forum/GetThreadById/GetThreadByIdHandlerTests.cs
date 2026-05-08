using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Logging;
using GameForum.Application.Core.Forum.GetThreadById;
using GameForum.Domain.Core.Forum;
using Moq;

namespace GameForum.Tests.Unit.Core.Forum.GetThreadById;

public class GetThreadByIdHandlerTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IThreadRepository> _threads = new();
    private readonly Mock<IAppLogger> _logger = new();

    public GetThreadByIdHandlerTests()
    {
        _uow.Setup(u => u.Threads).Returns(_threads.Object);
    }

    [Fact]
    public async Task HandleAsync_Existing_ReturnsThread()
    {
        var id = Guid.NewGuid();
        var entity = new ThreadEntity { Id = id, Title = "Hello", UserId = Guid.NewGuid(), CategoryId = Guid.NewGuid() };
        _threads
            .Setup(r => r.GetByIdWithDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        var handler = new GetThreadByIdHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(id);

        Assert.Null(response.ErrorMessage);
        Assert.NotNull(response.Data);
        Assert.Equal(id, response.Data!.Id);
    }

    [Fact]
    public async Task HandleAsync_EmptyId_ReturnsError()
    {
        var handler = new GetThreadByIdHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(Guid.Empty);

        Assert.NotNull(response.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_NotFound_ReturnsError()
    {
        var id = Guid.NewGuid();
        _threads
            .Setup(r => r.GetByIdWithDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ThreadEntity?)null);
        var handler = new GetThreadByIdHandler(_uow.Object, _logger.Object);

        var response = await handler.HandleAsync(id);

        Assert.NotNull(response.ErrorMessage);
        Assert.Null(response.Data);
    }
}
