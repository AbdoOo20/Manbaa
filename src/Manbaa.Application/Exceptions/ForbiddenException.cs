using Manbaa.Application.Common;

namespace Manbaa.Application.Exceptions;

public class ForbiddenException(string message = "مش مسموح لك بالعملية دي.") : AppException(ResultStatus.Forbidden, message);
