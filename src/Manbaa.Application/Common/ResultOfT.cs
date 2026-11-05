namespace Manbaa.Application.Common;

public class Result<T> : Result
{
    private Result(ResultStatus status, T? data, string? message, IReadOnlyList<string>? errors) : base(status, message, errors)
    {
        Data = data;
    }

    public T? Data { get; }

    public static Result<T> Ok(T data, string? message = null) => new(ResultStatus.Ok, data, message, null);

    public static Result<T> Created(T data, string? message = null) => new(ResultStatus.Created, data, message, null);

    public static new Result<T> BadRequest(params string[] errors) => new(ResultStatus.BadRequest, default, "الطلب غير صحيح.", errors);

    public static new Result<T> BadRequest(IEnumerable<string> errors) => new(ResultStatus.BadRequest, default, "الطلب غير صحيح.", errors.ToList());

    public static new Result<T> Unauthorized(string? message = null) => new(ResultStatus.Unauthorized, default, message ?? "لازم تسجّل دخول الأول.", null);

    public static new Result<T> Forbidden(string? message = null) => new(ResultStatus.Forbidden, default, message ?? "مش مسموح لك بالعملية دي.", null);

    public static new Result<T> NotFound(string? message = null) => new(ResultStatus.NotFound, default, message ?? "العنصر المطلوب مش موجود.", null);

    public static new Result<T> Conflict(string? message = null) => new(ResultStatus.Conflict, default, message ?? "العملية دي بتتعارض مع بيانات موجودة.", null);

    public static new Result<T> Gone(string? message = null) => new(ResultStatus.Gone, default, message ?? "العنصر ده مبقاش متاح.", null);

    public static new Result<T> UnprocessableEntity(params string[] errors) => new(ResultStatus.UnprocessableEntity, default, "البيانات المدخلة فيها أخطاء.", errors);

    public static new Result<T> TooManyRequests(string? message = null) => new(ResultStatus.TooManyRequests, default, message ?? "طلبات كتير في وقت قصير. حاول تاني بعد شوية.", null);

    public static Result<T> From(Result failure) => new(failure.Status, default, failure.Message, failure.Errors);

    public static new Result<T> InternalServerError(string? message = null) => new(ResultStatus.InternalServerError, default, message ?? "حصل خطأ غير متوقع.", null);
}
