using Microsoft.AspNetCore.Mvc;
using StepUp.Application.Interfaces;

namespace StepUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : BaseController
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult> GetNotifications([FromQuery] Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (userId == Guid.Empty)
            {
                return BadRequest(new { success = false, message = "ID utilizator invalid." });
            }

            var notifications = await _notificationService.GetUserNotificationsAsync(userId, cancellationToken);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "A apărut o eroare.", error = ex.Message });
        }
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult> GetUnreadCount([FromQuery] Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (userId == Guid.Empty)
            {
                return BadRequest(new { success = false, message = "ID utilizator invalid." });
            }

            var count = await _notificationService.GetUnreadCountAsync(userId, cancellationToken);
            return Ok(new { count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "A apărut o eroare.", error = ex.Message });
        }
    }

    [HttpPost("mark-read")]
    public async Task<ActionResult> MarkAllRead([FromQuery] Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _notificationService.MarkAllAsReadAsync(userId, cancellationToken);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "A apărut o eroare.", error = ex.Message });
        }
    }
}
