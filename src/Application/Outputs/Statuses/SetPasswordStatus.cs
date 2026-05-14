namespace Application.Outputs;

public enum SetPasswordStatus
{
    UserNotFound,
    AlreadyHasPassword,
    InvalidPassword,
    Error,
    Success
}
