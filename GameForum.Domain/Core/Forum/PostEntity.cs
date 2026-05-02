using GameForum.Domain.Common;
using GameForum.Domain.Core.Users;

namespace GameForum.Domain.Core.Forum;

public class PostEntity : BaseEntity
{
    public Guid ThreadId { get; set; }
    public ThreadEntity Thread { get; set; } = null!;

    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;

    public string Content { get; set; } = string.Empty;

    public double TrendingScore { get; set; }

    public bool IsDeleted { get; set; }

    public ICollection<VoteEntity> Votes { get; set; } = new List<VoteEntity>();
}
