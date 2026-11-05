using Manbaa.Application.Common;

namespace Manbaa.Application.Exceptions;

public class ConflictException(string message = "العملية دي بتتعارض مع بيانات موجودة.") : AppException(ResultStatus.Conflict, message);
