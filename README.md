## Knowledge-Base Q&A Chatbot MVP

Stack: React + Vite (TS), ASP.NET Core 8 Web API, SQL Server (EF Core), Azure AI Search + Azure OpenAI (+ optional Document Intelligence).

### Dev Setup (Linux)

1) Ensure .NET 8 SDK and Node 18+ are installed.

2) Backend

```
cd /workspace/src/server/KbApi
dotnet restore
dotnet run
```

Environment config: edit `appsettings.Development.json` with your Azure keys and SQL connection string.

3) Frontend

```
cd /workspace/src/web/kb-web
npm i
npm run dev
```

Open http://localhost:5173

### API Endpoints

- POST `/api/auth/login`
- GET `/api/docs`
- POST `/api/docs/upload` (multipart)
- POST `/api/docs/text`
- POST `/api/docs/url`
- POST `/api/chat/ask`
- GET `/api/chat/history`
- POST `/api/admin/reindex`

### Azure Search Index Creation (REST)

```
PUT https://<your-search>.search.windows.net/indexes/kbchunks?api-version=2024-07-01
api-key: <key>
Content-Type: application/json

{
  "name": "kbchunks",
  "fields": [
    { "name": "id", "type": "Edm.String", "key": true, "filterable": true },
    { "name": "content", "type": "Edm.String", "searchable": true },
    { "name": "title", "type": "Edm.String", "searchable": true },
    { "name": "source", "type": "Edm.String", "filterable": true },
    { "name": "url", "type": "Edm.String" },
    { "name": "chunkNo", "type": "Edm.Int32", "filterable": true }
  ],
  "semantic": { "configurations": [ { "name": "default", "prioritizedFields": { "titleField": { "fieldName": "title" }, "contentFields": [ { "fieldName": "content" } ] } } ] }
}
```

### Notes

- Seed login: admin@example.com / admin123
- Rate limit: 60 req/hour per user (in-memory)
- Plans enforced for max docs and file size

# ai-demo