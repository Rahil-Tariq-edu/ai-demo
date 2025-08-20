using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Microsoft.Extensions.Options;

namespace KbApi.Services;

public class DocIntelService
{
    private readonly AppSettings _settings;

    public DocIntelService(IOptions<AppSettings> options)
    {
        _settings = options.Value;
    }

    public async Task<string> ExtractTextAsync(Stream fileStream, string fileName, string contentType)
    {
        if (!string.IsNullOrWhiteSpace(_settings.AzureDocIntelEndpoint) && !string.IsNullOrWhiteSpace(_settings.AzureDocIntelKey))
        {
            try
            {
                var client = new DocumentAnalysisClient(new Uri(_settings.AzureDocIntelEndpoint!), new AzureKeyCredential(_settings.AzureDocIntelKey!));
                var operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-read", fileStream);
                var content = string.Join("\n", operation.Value.Pages.SelectMany(p => p.Lines).Select(l => l.Content));
                return content;
            }
            catch
            {
                // Fallback below
            }
        }

        // Fallback: naive text extraction
        using var reader = new StreamReader(fileStream);
        return await reader.ReadToEndAsync();
    }
}

