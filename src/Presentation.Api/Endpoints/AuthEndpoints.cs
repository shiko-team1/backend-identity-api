using Application.Abstractions;
using Application.Inputs;
using Application.Outputs;
using Application.Outputs.Statuses;
using Identity.Api.Security;

namespace Presentation.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth")
            .AddEndpointFilter<ApiKeyMiddleware>();

        group.MapPost("/login", Login);
        group.MapGet("/email-status", CheckEmailStatus);
        group.MapPost("/confirm-email", ConfirmEmail);

        var gatewayGroup = app.MapGroup("/api/auth/gateway")
                .AddEndpointFilter<ApiKeyMiddleware>(); 

        gatewayGroup.MapPost("/set-password", SetPassword);
        gatewayGroup.MapPut("/change-password", ChangePassword);
        gatewayGroup.MapPost("/verify-password", VerifyPassword);
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);

        return result.Status switch
        {
            LoginStatus.UserNotFound => Results.Unauthorized(),
            LoginStatus.InvalidCredentials => Results.Unauthorized(),
            LoginStatus.EmailNotConfirmed => Results.Forbid(),
            LoginStatus.Success => Results.Ok(result.User),
            _ => Results.Problem("Login failed.")
        };
    }

    private static async Task<IResult> CheckEmailStatus(
        string email,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.CheckEmailAsync(email, cancellationToken);
        return result.Status switch
        {
            EmailStatus.UserNotFound => Results.NotFound(),
            EmailStatus.NotConfirmed => Results.Ok(new { confirmed = false }),
            EmailStatus.Confirmed => Results.Ok(new { confirmed = true }),
            _ => Results.Problem("Email check failed.")
        };
    }

    private static async Task<IResult> ConfirmEmail(
        ConfirmEmailRequest request,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.ConfirmEmailAsync(request, cancellationToken);
        return result.Status switch
        {
            ConfirmEmailStatus.UserNotFound => Results.NotFound(),
            ConfirmEmailStatus.AlreadyConfirmed => Results.Ok(),
            ConfirmEmailStatus.Confirmed => Results.Ok(),
            ConfirmEmailStatus.Error => Results.BadRequest(result.ErrorMessage),
            _ => Results.Problem("Email confirmation failed.")
        };
    }

    private static async Task<IResult> SetPassword(
        SetPasswordRequest request,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.SetPasswordAsync(request, cancellationToken);
        return result.Status switch
        {
            SetPasswordStatus.UserNotFound => Results.NotFound(),
            SetPasswordStatus.AlreadyHasPassword => Results.Conflict(),
            SetPasswordStatus.InvalidPassword => Results.BadRequest(result.ErrorMessage),
            SetPasswordStatus.Error => Results.BadRequest(result.ErrorMessage),
            SetPasswordStatus.Success => Results.Ok(new { userId = result.UserId }),
            _ => Results.Problem("Set password failed.")
        };
    }

    private static async Task<IResult> ChangePassword(
    ChangePasswordRequest request,
    IAuthService authService,
    CancellationToken cancellationToken)
    {
        var result = await authService.ChangePasswordAsync(request, cancellationToken);
        return result.Status switch
        {
            ChangePasswordStatus.UserNotFound => Results.NotFound(),
            ChangePasswordStatus.InvalidCurrentPassword => Results.BadRequest("Invalid current password."),
            ChangePasswordStatus.InvalidNewPassword => Results.BadRequest(result.ErrorMessage),
            ChangePasswordStatus.Success => Results.Ok(),
            _ => Results.Problem("Password change failed.")
        };
    }

    private static async Task<IResult> VerifyPassword(
        VerifyPasswordRequest request,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.VerifyPasswordAsync(request, cancellationToken);
        return result.Status switch
        {
            VerifyPasswordStatus.UserNotFound => Results.NotFound(),
            VerifyPasswordStatus.InvalidPassword => Results.BadRequest("Invalid password."),
            VerifyPasswordStatus.Valid => Results.Ok(new { valid = true }),
            _ => Results.Problem("Password verification failed.")
        };
    }
}
