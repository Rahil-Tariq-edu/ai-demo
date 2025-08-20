using KbApi.Data;
using KbApi.DTOs;
using KbApi.Models;
using KbApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KbApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly SearchService _search;
    private readonly OpenAIService _ai;
    private readonly PlanService _planService;

    public ChatController(AppDbContext db, SearchService search, OpenAIService ai, PlanService planService)
    {
        _db = db; _search = search; _ai = ai; _planService = planService;
    }

    [HttpPost("ask")]
    public async Task<ActionResult<AskResponse>> Ask([FromBody] AskRequest req)
    {
        var user = await _db.Users.FirstAsync();

        var citations = await _search.SearchAsync(req.Message, 6);
        var answer = await _ai.GetAnswerAsync(req.Message, citations);

        var conversationId = req.ConversationId ?? Guid.NewGuid();
        if (req.ConversationId == null)
        {
            _db.Conversations.Add(new Conversation { Id = conversationId, UserId = user.Id, StartedAt = DateTime.UtcNow });
        }
        _db.Messages.AddRange(new[]
        {
            new Message { Id = Guid.NewGuid(), ConversationId = conversationId, Role = "User", Text = req.Message, CreatedAt = DateTime.UtcNow },
            new Message { Id = Guid.NewGuid(), ConversationId = conversationId, Role = "Assistant", Text = answer, CreatedAt = DateTime.UtcNow }
        });
        _db.UsageEvents.Add(new UsageEvent { Id = Guid.NewGuid(), UserId = user.Id, EventType = "Query", TokensIn = req.Message.Length / 4, TokensOut = answer.Length / 4, CreatedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync();

        return Ok(new AskResponse { ConversationId = conversationId, Answer = answer, Citations = citations });
    }

    [HttpGet("history")]
    public async Task<ActionResult> History()
    {
        var convs = await _db.Conversations.OrderByDescending(c => c.StartedAt).Take(50).ToListAsync();
        var msgs = await _db.Messages.Where(m => convs.Select(c => c.Id).Contains(m.ConversationId)).OrderBy(m => m.CreatedAt).ToListAsync();
        return Ok(new { conversations = convs, messages = msgs });
    }
}

