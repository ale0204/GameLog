using GameForum.Domain.Common;
using GameForum.Domain.Core.Forum.Enums;
using GameForum.Domain.Core.Users;

namespace GameForum.Domain.Core.Forum;

public class VoteEntity : BaseEntity
{
    public Guid PostId { get; set; }
    public PostEntity Post { get; set; } = null!;

    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;

    public VoteValue Value { get; set; }
}
