using System.Text.Json.Serialization;

namespace Manbaa.Application.Common;

public class Result
{
    protected Result(ResultStatus status, string? message, IReadOnlyList<string>? errors)
    {
        Status = status;

        Message = message;

        Errors = errors ?? [];
    }

    [JsonIgnore]
    public ResultStatus Status { get; }

    public bool Succeeded => (int)Status is >= 200 and < 300;

    public int StatusCode => (int)Status;

    public string? Message { get; }

    public IReadOnlyList<string> Errors { get; }

    public static Result Ok(string? message = null) => new(ResultStatus.Ok, message, null);

    public static Result Created(string? message = null) => new(ResultStatus.Created, message, null);

    public static Result BadRequest(params string[] errors) => new(ResultStatus.BadRequest, "الطلب غير صحيح.", errors);

    public static Result BadRequest(IEnumerable<string> errors) => new(ResultStatus.BadRequest, "الطلب غير صحيح.", errors.ToList());

    public static Result Unauthorized(string? message = null) => new(ResultStatus.Unauthorized, message ?? "لازم تسجّل دخول الأول.", null);

    public static Result Forbidden(string? message = null) => new(ResultStatus.Forbidden, message ?? "مش مسموح لك بالعملية دي.", null);

    public static Result NotFound(string? message = null) => new(ResultStatus.NotFound, message ?? "العنصر المطلوب مش موجود.", null);

    public static Result Conflict(string? message = null) => new(ResultStatus.Conflict, message ?? "العملية دي بتتعارض مع بيانات موجودة.", null);

    public static Result Failure(ResultStatus status, string? message = null, IReadOnlyList<string>? errors = null) => new(status, message, errors);

    public static Result Gone(string? message = null) => new(ResultStatus.Gone, message ?? "العنصر ده مبقاش متاح.", null);

    public static Result UnprocessableEntity(params string[] errors) => new(ResultStatus.UnprocessableEntity, "البيانات المدخلة فيها أخطاء.", errors);

    public static Result TooManyRequests(string? message = null) => new(ResultStatus.TooManyRequests, message ?? "طلبات كتير في وقت قصير. حاول تاني بعد شوية.", null);

    public static Result InternalServerError(string? message = null) => new(ResultStatus.InternalServerError, message ?? "حصل خطأ غير متوقع.", null);
}
