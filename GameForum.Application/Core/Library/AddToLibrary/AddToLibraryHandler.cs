using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Library.AddToLibrary;

public class AddToLibraryHandler : IAddToLibraryHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public AddToLibraryHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<UserGameModel>> HandleAsync(UserGameModel model, CancellationToken cancellationToken = default)
    {
        var response = new Response<UserGameModel>();
        try
        {
            // 1. Auth check
            // 2. Input validation
            if (model.UserId == Guid.Empty)
            {
                response.ErrorMessage = "UserId is required";
                return response;
            }
            if (model.GameId == Guid.Empty)
            {
                response.ErrorMessage = "GameId is required";
                return response;
            }
            if (model.PersonalRating is { } rating && (rating < 1 || rating > 10))
            {
                response.ErrorMessage = "PersonalRating must be between 1 and 10";
                return response;
            }

            // 3. Domain rules
            if (!await _unitOfWork.Users.ExistsAsync(model.UserId, cancellationToken))
            {
                response.ErrorMessage = "User does not exist";
                return response;
            }
            var game = await _unitOfWork.Games.GetByIdAsync(model.GameId, cancellationToken);
            if (game is null)
            {
                response.ErrorMessage = "Game does not exist";
                return response;
            }
            var existing = await _unitOfWork.UserGames.GetByUserAndGameAsync(model.UserId, model.GameId, cancellationToken);
            if (existing is not null)
            {
                response.ErrorMessage = "Game is already in this user's library";
                return response;
            }

            // 4. Persist
            var entity = model.ToEntity();
            await _unitOfWork.UserGames.AddAsync(entity, cancellationToken);
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
