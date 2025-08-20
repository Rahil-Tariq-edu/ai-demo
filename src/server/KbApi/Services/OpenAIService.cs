using Azure.AI.OpenAI;
using KbApi.DTOs;
using Microsoft.Extensions.Options;

namespace KbApi.Services;

public class OpenAIService
{
    private readonly OpenAIClient? _client;
    private readonly AppSettings _settings;

    public OpenAIService(OpenAIClient? client, IOptions<AppSettings> options)
    {
        _client = client;
        _settings = options.Value;
    }

    public const string SystemPrompt = "You answer strictly from the provided sources. If not found, say \"I don’t know based on the current knowledge base.\" Be concise, professional, and include citations like [1], [2] that map to the sources list.";

    public string BuildPrompt(string question, List<Citation> sources)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("SYSTEM:");
        sb.AppendLine(SystemPrompt);
        sb.AppendLine();
        sb.AppendLine("USER QUESTION:");
        sb.AppendLine(question);
        sb.AppendLine();
        sb.AppendLine("SOURCES:");
        foreach (var c in sources)
        {
            sb.AppendLine($"[{c.Index}] Title: {c.Title}");
            sb.AppendLine($"URL: {c.Url}");
            var excerpt = c.Excerpt.Length > 1200 ? c.Excerpt.Substring(0, 1200) + "..." : c.Excerpt;
            sb.AppendLine("Excerpt:");
            sb.AppendLine(excerpt);
            sb.AppendLine();
        }
        return sb.ToString();
    }

    public async Task<string> GetAnswerAsync(string question, List<Citation> sources)
    {
        var content = BuildPrompt(question, sources);
        if (_client == null)
        {
            return "This is a local demo answer. Configure Azure OpenAI to enable real responses.\n\nCitations: " + string.Join(", ", sources.Select(s => $"[{s.Index}]"));
        }
        var deployment = _settings.AzureOpenAIDeployment ?? "gpt-4o-mini";
        var messages = new List<ChatRequestMessage>
        {
            new ChatRequestSystemMessage(SystemPrompt),
            new ChatRequestUserMessage(content)
        };
        var response = await _client.GetChatCompletionsAsync(new ChatCompletionsOptions(deployment, messages)
        {
            Temperature = 0.2f,
            MaxTokens = 600
        });
        return response.Value.Choices[0].Message.Content ?? string.Empty;
    }
}

