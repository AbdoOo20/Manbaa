using Manbaa.Application.Common;

namespace Manbaa.Application.Exceptions;

public class NotFoundException(string message = "العنصر المطلوب مش موجود.") : AppException(ResultStatus.NotFound, message);
