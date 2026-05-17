namespace Application.Outputs;

public enum ChangePasswordStatus
{
    UserNotFound,
    InvalidCurrentPassword,
    InvalidNewPassword,
    Success
}
