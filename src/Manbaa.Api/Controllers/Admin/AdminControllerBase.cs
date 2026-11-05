using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using Manbaa.Domain.Constants;

namespace Manbaa.Api.Controllers.Admin;

/// <summary>Base for the system admin's endpoints, all under /api/admin.</summary>
[Authorize(Roles = RoleNames.SystemAdmin)]
[Route("api/admin/[controller]")]
public abstract class AdminControllerBase : ApiControllerBase;
