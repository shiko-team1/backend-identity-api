using Application.Abstractions;
using Application.Inputs;
using Application.Outputs;

namespace Presentation.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/login", Login);
        group.MapGet("/email-status", CheckEmailStatus);
        group.MapPost("/confirm-email", ConfirmEmail);
        group.MapPost("/set-password", SetPassword);
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
            SetPasswordStatus.Success => Results.Ok(),
            _ => Results.Problem("Set password failed.")
        };
    }
}
