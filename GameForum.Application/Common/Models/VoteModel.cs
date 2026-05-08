using GameForum.Domain.Core.Forum.Enums;

namespace GameForum.Application.Common.Models;

public class VoteModel
{
    public Guid Id { get; set; }

    public Guid PostId { get; set; }

    public Guid UserId { get; set; }

    public VoteValue Value { get; set; }
}
