using GameForum.Domain.Common;
using GameForum.Domain.Core.Badges;
using GameForum.Domain.Core.Forum;
using GameForum.Domain.Core.Games;
using GameForum.Domain.Core.Library;
using GameForum.Domain.Core.Reviews;
using GameForum.Domain.Core.Social;
using GameForum.Domain.Core.Users.Enums;

namespace GameForum.Domain.Core.Users;

public class UserEntity : BaseEntity
{
    public string Username { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public DateTime? LastSeenAt { get; set; }

    public bool IsProfilePublic { get; set; } = true;

    public Visibility DefaultGameVisibility { get; set; } = Visibility.Public;

    public Guid? CurrentlyPlayingGameId { get; set; }
    public GameEntity? CurrentlyPlayingGame { get; set; }

    public Guid? FavoriteGameId { get; set; }
    public GameEntity? FavoriteGame { get; set; }

    public ICollection<UserGameEntity> Library { get; set; } = new List<UserGameEntity>();
    public ICollection<ThreadEntity> Threads { get; set; } = new List<ThreadEntity>();
    public ICollection<PostEntity> Posts { get; set; } = new List<PostEntity>();
    public ICollection<VoteEntity> Votes { get; set; } = new List<VoteEntity>();
    public ICollection<ReviewEntity> Reviews { get; set; } = new List<ReviewEntity>();
    public ICollection<UserBadgeEntity> Badges { get; set; } = new List<UserBadgeEntity>();
    public ICollection<FriendshipEntity> SentFriendRequests { get; set; } = new List<FriendshipEntity>();
    public ICollection<FriendshipEntity> ReceivedFriendRequests { get; set; } = new List<FriendshipEntity>();
    public ICollection<MessageEntity> SentMessages { get; set; } = new List<MessageEntity>();
    public ICollection<MessageEntity> ReceivedMessages { get; set; } = new List<MessageEntity>();
}
