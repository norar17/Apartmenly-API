using ApartmentRental.Application.Common.Models;
using ApartmentRental.Application.Notifications.DTOs;
using ApartmentRental.Application.Notifications.Interfaces;
using ApartmentRental.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApartmentRental.API.Controllers;

[Authorize]
public class NotificationsController : ApiControllerBase
{
    private readonly INotificationQueryService _notificationQueryService;

    public NotificationsController(INotificationQueryService notificationQueryService)
    {
        _notificationQueryService = notificationQueryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<NotificationDto>>>> GetAll([FromQuery] PaginationParams pagination, CancellationToken cancellationToken)
    {
        var result = await _notificationQueryService.GetForUserAsync(CurrentUserId, CurrentUserRole, pagination, cancellationToken);
        return Ok(ApiResponse<PagedResult<NotificationDto>>.Ok(result));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount(CancellationToken cancellationToken)
    {
        var count = await _notificationQueryService.GetUnreadCountAsync(CurrentUserId, CurrentUserRole, cancellationToken);
        return Ok(ApiResponse<int>.Ok(count));
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
        => FromResult(await _notificationQueryService.MarkAsReadAsync(CurrentUserId, id, cancellationToken));

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
        => FromResult(await _notificationQueryService.MarkAllAsReadAsync(CurrentUserId, CurrentUserRole, cancellationToken));
}
