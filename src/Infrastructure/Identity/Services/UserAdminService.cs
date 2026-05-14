using Application.Abstractions;
using Application.Constants;
using Application.Inputs;
using Application.Outputs;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Services;

public sealed class UserAdminService(UserManager<IdentityUser> userManager) : IUserAdminService
{
    public async Task<CreateUserResult> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (!RoleNames.All.Contains(request.Role))
        {
            return new CreateUserResult(CreateUserStatus.InvalidRole, ErrorMessage: $"Role '{request.Role}' is not valid.");
        }

        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            return new CreateUserResult(CreateUserStatus.AlreadyExists, ErrorMessage: "User already exists.");
        }

        var user = new IdentityUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = false
        };

        var result = await userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            return new CreateUserResult(CreateUserStatus.Error, ErrorMessage: "Could not create user.");
        }

        await userManager.AddToRoleAsync(user, request.Role);

        return new CreateUserResult(CreateUserStatus.Success, user.Id, user.Email, request.Role);
    }

    public async Task<DeleteUserResult> DeleteUserByIdAsync(string id, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return new DeleteUserResult(DeleteUserStatus.NotFound);
        }

        var result = await userManager.DeleteAsync(user);
        return result.Succeeded
            ? new DeleteUserResult(DeleteUserStatus.Success)
            : new DeleteUserResult(DeleteUserStatus.Error, "Could not delete user.");
    }
}
