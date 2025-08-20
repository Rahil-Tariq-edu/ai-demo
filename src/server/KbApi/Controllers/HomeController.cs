using Microsoft.AspNetCore.Mvc;

namespace KbApi.Controllers;

[ApiController]
[Route("")]
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { name = "KbApi", status = "ok" });
}

