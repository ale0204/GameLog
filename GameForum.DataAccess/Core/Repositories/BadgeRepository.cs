using GameForum.Application.Common.DataAccess;
using GameForum.DataAccess.Core.EfCore;
using GameForum.Domain.Core.Badges;

namespace GameForum.DataAccess.Core.Repositories;

public class BadgeRepository : Repository<BadgeEntity>, IBadgeRepository
{
    public BadgeRepository(AppDbContext context) : base(context) { }
}
