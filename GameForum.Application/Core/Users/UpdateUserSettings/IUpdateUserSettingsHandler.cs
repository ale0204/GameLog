using GameForum.Application.Common.Errors;
using GameForum.Application.Common.Models;

namespace GameForum.Application.Core.Users.UpdateUserSettings;

public interface IUpdateUserSettingsHandler
{
    Task<Response<UserModel>> HandleAsync(UserModel model, CancellationToken cancellationToken = default);
}
