using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Users.UpdateUserSettings;

public class UpdateUserSettingsHandler : IUpdateUserSettingsHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public UpdateUserSettingsHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<UserModel>> HandleAsync(UserModel model, CancellationToken cancellationToken = default)
    {
        var response = new Response<UserModel>();
        try
        {
            // 1. Auth check (only the owner should be able to update — placeholder)
            if (model.Id == Guid.Empty)
            {
                response.ErrorMessage = "User Id is required";
                return response;
            }

            var existing = await _unitOfWork.Users.GetByIdAsync(model.Id, cancellationToken);
            if (existing is null)
            {
                response.ErrorMessage = "User not found";
                return response;
            }

            // CurrentlyPlayingGameId, if set, must reference a game in the user's library
            if (model.CurrentlyPlayingGameId is { } gameId)
            {
                var entry = await _unitOfWork.UserGames.GetByUserAndGameAsync(model.Id, gameId, cancellationToken);
                if (entry is null)
                {
                    response.ErrorMessage = "CurrentlyPlayingGameId must reference a game in the user's library";
                    return response;
                }
            }

            model.ApplySettingsTo(existing);
            _unitOfWork.Users.Update(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.Data = existing.ToModel();
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            _logger.Error(ex.Message);
        }
        return response;
    }
}
