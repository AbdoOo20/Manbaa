using Manbaa.Application.Common;

namespace Manbaa.Application.Exceptions;

public abstract class AppException(ResultStatus status, string message) : Exception(message)
{
    public ResultStatus Status { get; } = status;
}
