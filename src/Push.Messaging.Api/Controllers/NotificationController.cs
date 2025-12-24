using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Push.Messaging.Application.Interfaces;
using Push.Messaging.Shared.Dtos;
using Push.Messaging.Shared.Requests;

namespace Push.Messaging.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(
        INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("Send")]
    public async Task<IActionResult> SendAsync(NotificationRequestModel dto)
    {
        await _notificationService.PublishAsync(Convert.ToInt32(User.FindFirst("sub")?.Value), new NotificationDto
        {
            Title = dto.Title,
            Body = dto.Body
        });

        return Ok();
    }
}