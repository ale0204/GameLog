using GameForum.Domain.Common;
using GameForum.Domain.Core.Badges;
using GameForum.Domain.Core.Forum;
using GameForum.Domain.Core.Games;
using GameForum.Domain.Core.Library;
using GameForum.Domain.Core.Reviews;
using GameForum.Domain.Core.Social;
using GameForum.Domain.Core.Users;
using Microsoft.EntityFrameworkCore;

namespace GameForum.DataAccess.Core.EfCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<GameEntity> Games => Set<GameEntity>();
    public DbSet<GenreEntity> Genres => Set<GenreEntity>();
    public DbSet<PlatformEntity> Platforms => Set<PlatformEntity>();
    public DbSet<GameGenre> GameGenres => Set<GameGenre>();
    public DbSet<GamePlatform> GamePlatforms => Set<GamePlatform>();
    public DbSet<UserGameEntity> UserGames => Set<UserGameEntity>();
    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();
    public DbSet<ThreadEntity> Threads => Set<ThreadEntity>();
    public DbSet<PostEntity> Posts => Set<PostEntity>();
    public DbSet<VoteEntity> Votes => Set<VoteEntity>();
    public DbSet<FriendshipEntity> Friendships => Set<FriendshipEntity>();
    public DbSet<MessageEntity> Messages => Set<MessageEntity>();
    public DbSet<ReviewEntity> Reviews => Set<ReviewEntity>();
    public DbSet<BadgeEntity> Badges => Set<BadgeEntity>();
    public DbSet<UserBadgeEntity> UserBadges => Set<UserBadgeEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== UserEntity =====
        modelBuilder.Entity<UserEntity>(b =>
        {
            b.HasKey(u => u.Id);
            b.Property(u => u.Username).IsRequired().HasMaxLength(64);
            b.Property(u => u.DisplayName).IsRequired().HasMaxLength(128);
            b.Property(u => u.AvatarUrl).HasMaxLength(512);
            b.Property(u => u.Bio).HasMaxLength(2048);

            b.HasIndex(u => u.Username).IsUnique();

            b.HasOne(u => u.CurrentlyPlayingGame)
                .WithMany()
                .HasForeignKey(u => u.CurrentlyPlayingGameId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(u => u.FavoriteGame)
                .WithMany()
                .HasForeignKey(u => u.FavoriteGameId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ===== GameEntity =====
        modelBuilder.Entity<GameEntity>(b =>
        {
            b.HasKey(g => g.Id);
            b.Property(g => g.Title).IsRequired().HasMaxLength(256);
            b.Property(g => g.CoverUrl).HasMaxLength(512);
            b.Property(g => g.Description).HasMaxLength(8192);
            b.Property(g => g.Developer).HasMaxLength(256);

            b.HasIndex(g => g.RawgId).IsUnique();
        });

        // ===== GenreEntity =====
        modelBuilder.Entity<GenreEntity>(b =>
        {
            b.HasKey(g => g.Id);
            b.Property(g => g.Name).IsRequired().HasMaxLength(64);
            b.HasIndex(g => g.Name).IsUnique();
        });

        // ===== PlatformEntity =====
        modelBuilder.Entity<PlatformEntity>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Name).IsRequired().HasMaxLength(64);
            b.HasIndex(p => p.Name).IsUnique();
        });

        // ===== GameGenre (m:n) =====
        modelBuilder.Entity<GameGenre>(b =>
        {
            b.HasKey(gg => new { gg.GameId, gg.GenreId });

            b.HasOne(gg => gg.Game)
                .WithMany(g => g.GameGenres)
                .HasForeignKey(gg => gg.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(gg => gg.Genre)
                .WithMany(g => g.GameGenres)
                .HasForeignKey(gg => gg.GenreId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===== GamePlatform (m:n) =====
        modelBuilder.Entity<GamePlatform>(b =>
        {
            b.HasKey(gp => new { gp.GameId, gp.PlatformId });

            b.HasOne(gp => gp.Game)
                .WithMany(g => g.GamePlatforms)
                .HasForeignKey(gp => gp.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(gp => gp.Platform)
                .WithMany(p => p.GamePlatforms)
                .HasForeignKey(gp => gp.PlatformId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===== UserGameEntity =====
        modelBuilder.Entity<UserGameEntity>(b =>
        {
            b.HasKey(ug => ug.Id);
            b.Property(ug => ug.HoursPlayed).HasColumnType("numeric(8,2)");
            b.Property(ug => ug.Notes).HasMaxLength(2048);

            b.HasIndex(ug => new { ug.UserId, ug.GameId }).IsUnique();

            b.HasOne(ug => ug.User)
                .WithMany(u => u.Library)
                .HasForeignKey(ug => ug.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(ug => ug.Game)
                .WithMany(g => g.LibraryEntries)
                .HasForeignKey(ug => ug.GameId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===== CategoryEntity =====
        modelBuilder.Entity<CategoryEntity>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Name).IsRequired().HasMaxLength(128);
            b.Property(c => c.Description).IsRequired().HasMaxLength(1024);
            b.Property(c => c.IconUrl).HasMaxLength(512);
            b.HasIndex(c => c.Name).IsUnique();
        });

        // ===== ThreadEntity =====
        modelBuilder.Entity<ThreadEntity>(b =>
        {
            b.HasKey(t => t.Id);
            b.Property(t => t.Title).IsRequired().HasMaxLength(256);

            b.HasOne(t => t.User)
                .WithMany(u => u.Threads)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(t => t.Category)
                .WithMany(c => c.Threads)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(t => t.Game)
                .WithMany(g => g.Threads)
                .HasForeignKey(t => t.GameId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(t => t.CategoryId);
            b.HasIndex(t => t.GameId);
            b.HasIndex(t => t.LastActivityAt);
        });

        // ===== PostEntity =====
        modelBuilder.Entity<PostEntity>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Content).IsRequired().HasMaxLength(16384);

            b.HasOne(p => p.Thread)
                .WithMany(t => t.Posts)
                .HasForeignKey(p => p.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(p => p.ThreadId);

            // Soft delete query filter
            b.HasQueryFilter(p => !p.IsDeleted);
        });

        // ===== VoteEntity =====
        modelBuilder.Entity<VoteEntity>(b =>
        {
            b.HasKey(v => v.Id);

            b.HasOne(v => v.Post)
                .WithMany(p => p.Votes)
                .HasForeignKey(v => v.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(v => v.User)
                .WithMany(u => u.Votes)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(v => new { v.UserId, v.PostId }).IsUnique();
        });

        // ===== FriendshipEntity =====
        modelBuilder.Entity<FriendshipEntity>(b =>
        {
            b.HasKey(f => f.Id);

            b.HasOne(f => f.Sender)
                .WithMany(u => u.SentFriendRequests)
                .HasForeignKey(f => f.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(f => f.Receiver)
                .WithMany(u => u.ReceivedFriendRequests)
                .HasForeignKey(f => f.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(f => new { f.SenderId, f.ReceiverId }).IsUnique();
        });

        // ===== MessageEntity =====
        modelBuilder.Entity<MessageEntity>(b =>
        {
            b.HasKey(m => m.Id);
            b.Property(m => m.Content).IsRequired().HasMaxLength(4096);

            b.HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(m => new { m.SenderId, m.ReceiverId, m.CreatedAt });
        });

        // ===== ReviewEntity =====
        modelBuilder.Entity<ReviewEntity>(b =>
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.Content).IsRequired().HasMaxLength(8192);

            b.HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(r => r.Game)
                .WithMany(g => g.Reviews)
                .HasForeignKey(r => r.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(r => new { r.UserId, r.GameId }).IsUnique();
        });

        // ===== BadgeEntity =====
        modelBuilder.Entity<BadgeEntity>(b =>
        {
            b.HasKey(bd => bd.Id);
            b.Property(bd => bd.Name).IsRequired().HasMaxLength(128);
            b.Property(bd => bd.Description).IsRequired().HasMaxLength(1024);
            b.Property(bd => bd.IconUrl).HasMaxLength(512);
            b.Property(bd => bd.Condition).IsRequired().HasMaxLength(512);
            b.HasIndex(bd => bd.Name).IsUnique();
        });

        // ===== UserBadgeEntity =====
        modelBuilder.Entity<UserBadgeEntity>(b =>
        {
            b.HasKey(ub => ub.Id);

            b.HasOne(ub => ub.User)
                .WithMany(u => u.Badges)
                .HasForeignKey(ub => ub.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(ub => ub.Badge)
                .WithMany(bd => bd.UserBadges)
                .HasForeignKey(ub => ub.BadgeId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(ub => new { ub.UserId, ub.BadgeId }).IsUnique();
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default)
                    entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
