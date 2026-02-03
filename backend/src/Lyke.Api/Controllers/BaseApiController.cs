using Lyke.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Lyke.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected ActionResult<ApiResponse<T>> OkResponse<T>(T data)
    {
        return Ok(ApiResponse<T>.Ok(data));
    }

    protected ActionResult<ApiResponse<T>> OkResponse<T>(T data, PaginationMeta meta)
    {
        return Ok(ApiResponse<T>.Ok(data, meta));
    }

    protected ActionResult<ApiResponse> OkResponse()
    {
        return Ok(ApiResponse.Ok());
    }

    protected ActionResult<ApiResponse<T>> NotFoundResponse<T>(string message)
    {
        return NotFound(ApiResponse<T>.Fail("NOT_FOUND", message));
    }

    protected ActionResult<ApiResponse<T>> BadRequestResponse<T>(string message)
    {
        return BadRequest(ApiResponse<T>.Fail("BAD_REQUEST", message));
    }

    protected ActionResult<ApiResponse<T>> ValidationErrorResponse<T>(IDictionary<string, string[]> errors)
    {
        return BadRequest(ApiResponse<T>.Fail("VALIDATION_ERROR", "One or more validation errors occurred", errors));
    }

    protected ActionResult<ApiResponse<T>> UnauthorizedResponse<T>(string message = "Unauthorized")
    {
        return Unauthorized(ApiResponse<T>.Fail("UNAUTHORIZED", message));
    }

    protected Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim != null ? Guid.Parse(userIdClaim) : Guid.Empty;
    }
}
