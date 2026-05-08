using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.ExternalServices;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;
using GameForum.Domain.Core.Games;

namespace GameForum.Application.Core.Games.SyncGameFromRawg;

public class SyncGameFromRawgHandler : ISyncGameFromRawgHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRawgClient _rawgClient;
    private readonly IAppLogger _logger;

    public SyncGameFromRawgHandler(IUnitOfWork unitOfWork, IRawgClient rawgClient, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _rawgClient = rawgClient;
        _logger = logger;
    }

    public async Task<Response<GameModel>> HandleAsync(int rawgId, CancellationToken cancellationToken = default)
    {
        var response = new Response<GameModel>();
        try
        {
            // 1. Auth check
            // 2. Input validation
            if (rawgId <= 0)
            {
                response.ErrorMessage = "RawgId must be positive";
                return response;
            }

            // 3. Fetch from RAWG
            var dto = await _rawgClient.GetGameDetailsAsync(rawgId, cancellationToken);
            if (dto is null)
            {
                response.ErrorMessage = $"RAWG returned no data for game {rawgId}";
                return response;
            }

            // 4. Upsert game
            var existing = await _unitOfWork.Games.GetByRawgIdAsync(rawgId, cancellationToken);
            GameEntity entity;
            if (existing is null)
            {
                entity = new GameEntity
                {
                    Id = Guid.NewGuid(),
                    RawgId = dto.Id,
                    Title = dto.Name,
                    CoverUrl = dto.BackgroundImage,
                    Description = dto.Description,
                    ReleaseYear = dto.ReleaseYear,
                    Developer = dto.Developer,
                    AverageRating = dto.Rating,
                    TotalPlayers = dto.PlayersCount
                };
                await _unitOfWork.Games.AddAsync(entity, cancellationToken);
            }
            else
            {
                existing.Title = dto.Name;
                existing.CoverUrl = dto.BackgroundImage;
                existing.Description = dto.Description;
                existing.ReleaseYear = dto.ReleaseYear;
                existing.Developer = dto.Developer;
                existing.AverageRating = dto.Rating;
                existing.TotalPlayers = dto.PlayersCount;
                _unitOfWork.Games.Update(existing);
                entity = existing;
            }

            // 5. Upsert genres + platforms (find-or-create per name) and link
            entity.GameGenres.Clear();
            foreach (var name in dto.Genres.Select(n => n?.Trim()).Where(n => !string.IsNullOrEmpty(n)).Distinct())
            {
                var genre = await _unitOfWork.Genres.GetByNameAsync(name!, cancellationToken);
                if (genre is null)
                {
                    genre = new GenreEntity { Id = Guid.NewGuid(), Name = name! };
                    await _unitOfWork.Genres.AddAsync(genre, cancellationToken);
                }
                entity.GameGenres.Add(new GameGenre { GameId = entity.Id, Game = entity, GenreId = genre.Id, Genre = genre });
            }

            entity.GamePlatforms.Clear();
            foreach (var name in dto.Platforms.Select(n => n?.Trim()).Where(n => !string.IsNullOrEmpty(n)).Distinct())
            {
                var platform = await _unitOfWork.Platforms.GetByNameAsync(name!, cancellationToken);
                if (platform is null)
                {
                    platform = new PlatformEntity { Id = Guid.NewGuid(), Name = name! };
                    await _unitOfWork.Platforms.AddAsync(platform, cancellationToken);
                }
                entity.GamePlatforms.Add(new GamePlatform { GameId = entity.Id, Game = entity, PlatformId = platform.Id, Platform = platform });
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            response.Data = entity.ToModel();
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            _logger.Error(ex.Message);
        }
        return response;
    }
}
