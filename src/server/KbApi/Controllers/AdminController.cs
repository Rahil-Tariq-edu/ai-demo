using KbApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace KbApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly SearchService _search;
    public AdminController(SearchService search)
    {
        _search = search;
    }

    [HttpPost("reindex")]
    public async Task<ActionResult> Reindex()
    {
        await _search.EnsureIndexAsync();
        return Ok(new { status = "ok" });
    }
}

