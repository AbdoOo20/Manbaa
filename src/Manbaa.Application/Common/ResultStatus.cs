namespace Manbaa.Application.Common;

public enum ResultStatus
{
    Ok = 200,

    Created = 201,

    BadRequest = 400,

    Unauthorized = 401,

    Forbidden = 403,

    NotFound = 404,

    Conflict = 409,

    Gone = 410,

    UnprocessableEntity = 422,

    TooManyRequests = 429,

    InternalServerError = 500
}
