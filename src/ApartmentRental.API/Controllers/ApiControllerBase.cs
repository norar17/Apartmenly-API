using System.Security.Claims;
using ApartmentRental.Domain.Enums;
using ApartmentRental.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ApartmentRental.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected Guid CurrentUserId
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    protected UserRole CurrentUserRole
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<UserRole>(value, out var role) ? role : default;
        }
    }

    protected ActionResult<ApiResponse<T>> FromResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(ApiResponse<T>.Ok(result.Value));
        }

        return StatusCodeForError(result.ErrorCode, ApiResponse<T>.Fail(result.Error!, result.ErrorCode));
    }

    protected IActionResult FromResult(Result result)
    {
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return StatusCodeForError(result.ErrorCode, ApiResponse<object>.Fail(result.Error!, result.ErrorCode));
    }

    // Maps a Result's ErrorCode to the matching HTTP status - keeps that
    // decision in one place instead of scattered across every controller.
    private ObjectResult StatusCodeForError<T>(string? errorCode, T body)
    {
        var statusCode = errorCode switch
        {
            "NOT_FOUND" => StatusCodes.Status404NotFound,
            "CONFLICT" or "DUPLICATE" or "HAS_ACTIVE_LEASE" => StatusCodes.Status409Conflict,
            "UNAUTHORIZED" or "INVALID_CREDENTIALS" => StatusCodes.Status401Unauthorized,
            "FORBIDDEN" => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest
        };

        return StatusCode(statusCode, body);
    }
}
