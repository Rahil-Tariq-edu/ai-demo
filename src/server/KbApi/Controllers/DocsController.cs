using KbApi.Data;
using KbApi.DTOs;
using KbApi.Models;
using KbApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KbApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IngestionService _ingestion;
    private readonly DocIntelService _docIntel;
    private readonly IHttpClientFactory _httpFactory;
    private readonly PlanService _planService;

    public DocsController(AppDbContext db, IngestionService ingestion, DocIntelService docIntel, IHttpClientFactory httpFactory, PlanService planService)
    {
        _db = db; _ingestion = ingestion; _docIntel = docIntel; _httpFactory = httpFactory; _planService = planService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> List()
    {
        var docs = await _db.Documents.OrderByDescending(d => d.CreatedAt).Select(d => new
        {
            d.Id, d.Title, d.SourceType, d.Status, d.CreatedAt
        }).ToListAsync();
        return Ok(docs);
    }

    [HttpPost("text")]
    public async Task<ActionResult> UploadText([FromBody] UploadTextRequest req)
    {
        var user = await _db.Users.FirstAsync();
        var caps = _planService.GetCaps(user.Plan);
        var docsCount = await _db.Documents.CountAsync(d => d.UserId == user.Id);
        if (docsCount >= caps.MaxDocs) return BadRequest(new { error = "plan_limit", message = "Document limit reached for your plan." });
        var doc = await _ingestion.AddTextAsync(user.Id, req.Title, req.Text);
        return Ok(new { doc.Id });
    }

    [HttpPost("url")]
    public async Task<ActionResult> UploadUrl([FromBody] UploadUrlRequest req)
    {
        var user = await _db.Users.FirstAsync();
        var caps = _planService.GetCaps(user.Plan);
        var docsCount = await _db.Documents.CountAsync(d => d.UserId == user.Id);
        if (docsCount >= caps.MaxDocs) return BadRequest(new { error = "plan_limit", message = "Document limit reached for your plan." });
        var http = _httpFactory.CreateClient();
        var doc = await _ingestion.AddUrlAsync(user.Id, req.Title, req.Url, http);
        return Ok(new { doc.Id });
    }

    [HttpPost("upload")]
    [RequestSizeLimit(60_000_000)]
    public async Task<ActionResult> UploadFile([FromForm] IFormFile file, [FromForm] string? title)
    {
        if (file == null || file.Length == 0) return BadRequest(new { error = "no_file" });
        var user = await _db.Users.FirstAsync();
        var caps = _planService.GetCaps(user.Plan);
        if (file.Length > caps.MaxFileBytes) return BadRequest(new { error = "file_too_large", message = "File exceeds plan limit." });
        var docsCount = await _db.Documents.CountAsync(d => d.UserId == user.Id);
        if (docsCount >= caps.MaxDocs) return BadRequest(new { error = "plan_limit" });
        await using var stream = file.OpenReadStream();
        var doc = await _ingestion.AddFileAsync(user.Id, title ?? file.FileName, stream, file.ContentType, _docIntel.ExtractTextAsync);
        return Ok(new { doc.Id });
    }
}

