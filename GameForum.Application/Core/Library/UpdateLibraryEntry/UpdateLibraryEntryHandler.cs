using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Library.UpdateLibraryEntry;

public class UpdateLibraryEntryHandler : IUpdateLibraryEntryHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public UpdateLibraryEntryHandler(IUnitOfWork unitOfWork, IAppLogger logger)
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
            if (model.Id == Guid.Empty)
            {
                response.ErrorMessage = "Entry Id is required";
                return response;
            }
            if (model.PersonalRating is { } rating && (rating < 1 || rating > 10))
            {
                response.ErrorMessage = "PersonalRating must be between 1 and 10";
                return response;
            }

            // 3. Domain rules
            var existing = await _unitOfWork.UserGames.GetByIdAsync(model.Id, cancellationToken);
            if (existing is null)
            {
                response.ErrorMessage = "Library entry not found";
                return response;
            }
            if (existing.UserId != model.UserId)
            {
                response.ErrorMessage = "Only the owner can update this library entry";
                return response;
            }

            // 4. Apply + persist
            model.ApplyTo(existing);
            _unitOfWork.UserGames.Update(existing);
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
