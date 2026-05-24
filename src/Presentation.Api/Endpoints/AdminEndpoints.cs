using Application.Abstractions;
using Application.Inputs;
using Application.Outputs;
using Identity.Api.Security;

namespace Presentation.Api.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .AddEndpointFilter<ApiKeyMiddleware>();

        group.MapPost("/users", CreateUser);
        group.MapDelete("/users/{id}", DeleteUserById);
    }

    private static async Task<IResult> CreateUser(
        CreateUserRequest request,
        IUserAdminService userAdminService,
        CancellationToken cancellationToken)
    {
        var result = await userAdminService.CreateUserAsync(request, cancellationToken);

        return result.Status switch
        {
            CreateUserStatus.InvalidRole => Results.BadRequest(result.ErrorMessage),
            CreateUserStatus.AlreadyExists => Results.Conflict(result.ErrorMessage),
            CreateUserStatus.Error => Results.BadRequest(result.ErrorMessage),
            CreateUserStatus.Success => Results.Created($"/api/admin/users/{result.UserId}", result),
            _ => Results.Problem("Create user failed.")
        };
    }

    private static async Task<IResult> DeleteUserById(
        string id,
        IUserAdminService userAdminService,
        CancellationToken cancellationToken)
    {
        var result = await userAdminService.DeleteUserByIdAsync(id, cancellationToken);

        return result.Status switch
        {
            DeleteUserStatus.NotFound => Results.NotFound(),
            DeleteUserStatus.Error => Results.BadRequest(result.ErrorMessage),
            DeleteUserStatus.Success => Results.NoContent(),
            _ => Results.Problem("Delete user failed.")
        };
    }
}