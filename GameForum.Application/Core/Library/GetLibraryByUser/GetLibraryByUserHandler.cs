using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;
using GameForum.Domain.Core.Social.Enums;
using GameForum.Domain.Core.Users.Enums;

namespace GameForum.Application.Core.Library.GetLibraryByUser;

public class GetLibraryByUserHandler : IGetLibraryByUserHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public GetLibraryByUserHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<List<UserGameModel>>> HandleAsync(GetLibraryByUserQuery query, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<UserGameModel>>();
        try
        {
            // 1. Auth check (public endpoint, requester may be null = anonymous)
            if (query.OwnerId == Guid.Empty)
            {
                response.ErrorMessage = "OwnerId is required";
                return response;
            }

            var owner = await _unitOfWork.Users.GetByIdAsync(query.OwnerId, cancellationToken);
            if (owner is null)
            {
                response.ErrorMessage = "User not found";
                return response;
            }

            var entries = await _unitOfWork.UserGames.GetByUserIdAsync(query.OwnerId, cancellationToken);

            // Owner viewing own library: see everything regardless of profile or per-entry visibility
            if (query.RequesterId == query.OwnerId)
            {
                response.Data = entries.ToModelList();
                return response;
            }

            // Determine relationship: friend (Accepted) vs stranger
            var isFriend = false;
            if (query.RequesterId is { } requesterId)
            {
                var relation = await _unitOfWork.Friendships.GetRelationAsync(query.OwnerId, requesterId, cancellationToken);
                isFriend = relation?.Status == FriendshipStatus.Accepted;
            }

            // Profile-private and not friend → empty list
            if (!owner.IsProfilePublic && !isFriend)
            {
                response.Data = new List<UserGameModel>();
                return response;
            }

            // Apply per-entry visibility filter
            var filtered = entries
                .Where(e => isFriend
                    ? e.Visibility == Visibility.Public || e.Visibility == Visibility.FriendsOnly
                    : e.Visibility == Visibility.Public)
                .ToList();

            response.Data = filtered.ToModelList();
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            _logger.Error(ex.Message);
        }
        return response;
    }
}
