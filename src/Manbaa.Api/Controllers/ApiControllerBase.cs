using Microsoft.AspNetCore.Mvc;

using Manbaa.Application.Common;

namespace Manbaa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected ObjectResult Respond(Result result) => StatusCode(result.StatusCode, result);
}
