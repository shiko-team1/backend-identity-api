using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Outputs;

public enum CreateUserStatus
{
    Success,
    InvalidRole,
    AlreadyExists,
    Error
}
