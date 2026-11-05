using Manbaa.Application.Common;

namespace Manbaa.Application.Exceptions;

public class BadRequestException(string message = "الطلب غير صحيح.") : AppException(ResultStatus.BadRequest, message);
