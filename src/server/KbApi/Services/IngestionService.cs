using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using KbApi.Data;
using KbApi.Models;
using Microsoft.EntityFrameworkCore;

namespace KbApi.Services;

public class IngestionService
{
    private readonly AppDbContext _db;
    private readonly SearchClient? _searchClient;

    public IngestionService(AppDbContext db, SearchClient? searchClient)
    {
        _db = db;
        _searchClient = searchClient;
    }

    public async Task<Document> AddTextAsync(Guid userId, string title, string text)
    {
        var doc = new Document
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            SourceType = SourceType.Text,
            FilePathOrUrl = "text",
            Status = DocumentStatus.Uploaded,
            CreatedAt = DateTime.UtcNow
        };
        _db.Documents.Add(doc);
        await _db.SaveChangesAsync();

        var chunks = ChunkText(text, title, null).Select((c, i) => new Chunk
        {
            Id = Guid.NewGuid(),
            DocumentId = doc.Id,
            ChunkNo = i + 1,
            Content = c,
            SourceTitle = title,
            SourceUrl = null,
            CreatedAt = DateTime.UtcNow
        }).ToList();
        _db.Chunks.AddRange(chunks);
        await _db.SaveChangesAsync();

        await IndexChunksAsync(chunks);

        doc.Status = DocumentStatus.Processed;
        await _db.SaveChangesAsync();
        return doc;
    }

    public async Task<Document> AddUrlAsync(Guid userId, string title, string url, HttpClient http)
    {
        var html = await http.GetStringAsync(url);
        var text = HtmlToText(html);
        return await AddTextAsync(userId, title, text);
    }

    public async Task<Document> AddFileAsync(Guid userId, string title, Stream contentStream, string fileName, string contentType, Func<Stream,string,string,Task<string>> extractor)
    {
        var text = await extractor(contentStream, fileName, contentType);
        return await AddTextAsync(userId, title, text);
    }

    public IEnumerable<string> ChunkText(string text, string title, string? url)
    {
        var normalized = System.Text.RegularExpressions.Regex.Replace(text, "\\s+", " ").Trim();
        var maxChars = 4000; // rough 800-1200 tokens
        var overlap = 400;
        var chunks = new List<string>();
        for (int i = 0; i < normalized.Length; i += maxChars - overlap)
        {
            var len = Math.Min(maxChars, normalized.Length - i);
            chunks.Add(normalized.Substring(i, len));
            if (i + len >= normalized.Length) break;
        }
        return chunks;
    }

    private async Task IndexChunksAsync(List<Chunk> chunks)
    {
        if (_searchClient == null) return;
        var batch = IndexDocumentsBatch.Create(chunks.Select(c => IndexDocumentsAction.Upload(new
        {
            id = c.Id.ToString(),
            content = c.Content,
            title = c.SourceTitle,
            source = c.Document?.SourceType.ToString().ToLowerInvariant() ?? "text",
            url = c.SourceUrl ?? string.Empty,
            chunkNo = c.ChunkNo
        })).ToArray());
        await _searchClient.IndexDocumentsAsync(batch);
    }

    private static string HtmlToText(string html)
    {
        var withoutTags = System.Text.RegularExpressions.Regex.Replace(html, "<script[\\s\\S]*?</script>", string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        withoutTags = System.Text.RegularExpressions.Regex.Replace(withoutTags, "<style[\\s\\S]*?</style>", string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        withoutTags = System.Text.RegularExpressions.Regex.Replace(withoutTags, "<[^>]+>", " ");
        withoutTags = System.Text.RegularExpressions.Regex.Replace(withoutTags, "&nbsp;|&amp;|&quot;|&lt;|&gt;", " ");
        return System.Text.RegularExpressions.Regex.Replace(withoutTags, "\\s+", " ").Trim();
    }
}

