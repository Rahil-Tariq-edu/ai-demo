using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using KbApi.DTOs;
using Microsoft.Extensions.Options;

namespace KbApi.Services;

public class SearchService
{
    private readonly SearchClient? _searchClient;
    private readonly SearchIndexClient? _indexClient;
    private readonly AppSettings _settings;

    public SearchService(SearchClient? searchClient, SearchIndexClient? indexClient, IOptions<AppSettings> options)
    {
        _searchClient = searchClient;
        _indexClient = indexClient;
        _settings = options.Value;
    }

    public async Task EnsureIndexAsync()
    {
        if (_indexClient == null) return;
        var name = _settings.AzureSearchIndex ?? "kbchunks";
        var exists = false;
        await foreach (var idxName in _indexClient.GetIndexNamesAsync())
        {
            if (string.Equals(idxName, name, StringComparison.OrdinalIgnoreCase)) { exists = true; break; }
        }
        if (exists) return;

        var fields = new List<SearchField>
        {
            new SimpleField("id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true },
            new SearchableField("content") { IsFilterable = false, IsSortable = false, IsFacetable = false },
            new SearchableField("title") { IsFilterable = false },
            new SimpleField("source", SearchFieldDataType.String) { IsFilterable = true },
            new SimpleField("url", SearchFieldDataType.String) { IsFilterable = false },
            new SimpleField("chunkNo", SearchFieldDataType.Int32) { IsFilterable = true }
        };
        var definition = new SearchIndex(name, fields)
        {
            SemanticSettings = new SemanticSettings(new[] { new SemanticConfiguration("default", new PrioritizedFields()
            {
                TitleField = new SemanticField("title"),
                ContentFields = { new SemanticField("content") }
            }) })
        };

        await _indexClient.CreateOrUpdateIndexAsync(definition);
    }

    public async Task<List<Citation>> SearchAsync(string query, int top = 6)
    {
        if (_searchClient == null) return new List<Citation>{ new Citation{ Index = 1, Title = "Seed", Url = "", Excerpt = "Seed data" } };
        var options = new SearchOptions
        {
            Size = top,
            QueryType = SearchQueryType.Semantic,
            SemanticConfigurationName = "default",
            QueryLanguage = QueryLanguage.EnUs
        };
        options.Select.Add("title");
        options.Select.Add("url");
        options.Select.Add("content");
        options.Select.Add("chunkNo");

        var results = await _searchClient.SearchAsync<SearchDocument>(query, options);
        var citations = new List<Citation>();
        var i = 1;
        await foreach (var result in results.Value.GetResultsAsync())
        {
            var doc = result.Document;
            citations.Add(new Citation
            {
                Index = i++,
                Title = doc["title"]?.ToString() ?? "",
                Url = doc["url"]?.ToString() ?? "",
                Excerpt = doc["content"]?.ToString() ?? ""
            });
        }
        return citations;
    }
}

