// Файл: HealthController.cs
using Microsoft.AspNetCore.Mvc;

namespace EduPlatform.Host.Controllers;

// Контроллер HealthController группирует HTTP-эндпоинты и делегирует работу в прикладные сценарии.
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
}
