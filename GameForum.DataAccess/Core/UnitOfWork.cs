using GameForum.Application.Common.DataAccess;
using GameForum.DataAccess.Core.EfCore;
using GameForum.DataAccess.Core.Repositories;

namespace GameForum.DataAccess.Core;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    private IUserRepository? _users;
    private IGameRepository? _games;
    private IGenreRepository? _genres;
    private IPlatformRepository? _platforms;
    private IUserGameRepository? _userGames;
    private ICategoryRepository? _categories;
    private IThreadRepository? _threads;
    private IPostRepository? _posts;
    private IVoteRepository? _votes;
    private IFriendshipRepository? _friendships;
    private IMessageRepository? _messages;
    private IReviewRepository? _reviews;
    private IBadgeRepository? _badges;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IGameRepository Games => _games ??= new GameRepository(_context);
    public IGenreRepository Genres => _genres ??= new GenreRepository(_context);
    public IPlatformRepository Platforms => _platforms ??= new PlatformRepository(_context);
    public IUserGameRepository UserGames => _userGames ??= new UserGameRepository(_context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
    public IThreadRepository Threads => _threads ??= new ThreadRepository(_context);
    public IPostRepository Posts => _posts ??= new PostRepository(_context);
    public IVoteRepository Votes => _votes ??= new VoteRepository(_context);
    public IFriendshipRepository Friendships => _friendships ??= new FriendshipRepository(_context);
    public IMessageRepository Messages => _messages ??= new MessageRepository(_context);
    public IReviewRepository Reviews => _reviews ??= new ReviewRepository(_context);
    public IBadgeRepository Badges => _badges ??= new BadgeRepository(_context);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
