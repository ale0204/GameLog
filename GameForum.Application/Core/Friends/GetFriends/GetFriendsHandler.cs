using GameForum.Application.Common.DataAccess;
using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Logging;
using GameForum.Application.Common.Mappings;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Friends.GetFriends;

public class GetFriendsHandler : IGetFriendsHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger _logger;

    public GetFriendsHandler(IUnitOfWork unitOfWork, IAppLogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<List<FriendshipModel>>> HandleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var response = new Response<List<FriendshipModel>>();
        try
        {
            // 1. Auth check (public endpoint — anyone can see a user's friends)
            if (userId == Guid.Empty)
            {
                response.ErrorMessage = "UserId is required";
                return response;
            }

            var friends = await _unitOfWork.Friendships.GetAcceptedFriendsAsync(userId, cancellationToken);
            response.Data = friends.ToModelList();
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            _logger.Error(ex.Message);
        }
        return response;
    }
}
