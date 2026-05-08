namespace GameForum.Application.Common.DataAccess;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IGameRepository Games { get; }
    IGenreRepository Genres { get; }
    IPlatformRepository Platforms { get; }
    IUserGameRepository UserGames { get; }
    ICategoryRepository Categories { get; }
    IThreadRepository Threads { get; }
    IPostRepository Posts { get; }
    IVoteRepository Votes { get; }
    IFriendshipRepository Friendships { get; }
    IMessageRepository Messages { get; }
    IReviewRepository Reviews { get; }
    IBadgeRepository Badges { get; }

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
