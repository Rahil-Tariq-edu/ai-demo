using System.Text;
using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using KbApi.Data;
using KbApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Config
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

// CORS
var frontendOrigin = builder.Configuration.GetValue<string>("FrontendOrigin") ?? "http://localhost:5173";
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(frontendOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// EF Core
var connectionString = builder.Configuration.GetConnectionString("Default") ?? builder.Configuration["SQL_CONNECTION_STRING"];
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, "kbapp.db")}");
    }
});

// Authentication (JWT)
var jwtKey = builder.Configuration.GetValue<string>("Jwt:Key") ?? "dev_secret_key_change_me";
var jwtIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer") ?? "kbapi";
var jwtAudience = builder.Configuration.GetValue<string>("Jwt:Audience") ?? "kbweb";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = key,
        ClockSkew = TimeSpan.FromMinutes(2)
    };
});

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Azure clients
builder.Services.AddHttpClient();
builder.Services.AddSingleton(provider =>
{
    var cfg = provider.GetRequiredService<IOptions<AppSettings>>().Value;
    var endpoint = cfg.AzureSearchEndpoint;
    var apiKey = cfg.AzureSearchApiKey;
    if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
    {
        return null!; // Services will handle nulls gracefully
    }
    var credential = new AzureKeyCredential(apiKey);
    return new SearchClient(new Uri(endpoint), cfg.AzureSearchIndex ?? "kbchunks", credential);
});
builder.Services.AddSingleton(provider =>
{
    var cfg = provider.GetRequiredService<IOptions<AppSettings>>().Value;
    var endpoint = cfg.AzureSearchEndpoint;
    var apiKey = cfg.AzureSearchApiKey;
    if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
    {
        return null!;
    }
    var credential = new AzureKeyCredential(apiKey);
    return new SearchIndexClient(new Uri(endpoint), credential);
});
builder.Services.AddSingleton(provider =>
{
    var cfg = provider.GetRequiredService<IOptions<AppSettings>>().Value;
    if (string.IsNullOrWhiteSpace(cfg.AzureOpenAIEndpoint) || string.IsNullOrWhiteSpace(cfg.AzureOpenAIApiKey))
    {
        return null!;
    }
    return new OpenAIClient(new Uri(cfg.AzureOpenAIEndpoint!), new AzureKeyCredential(cfg.AzureOpenAIApiKey!));
});

// Services
builder.Services.AddMemoryCache();
builder.Services.AddScoped<OpenAIService>();
builder.Services.AddScoped<SearchService>();
builder.Services.AddScoped<DocIntelService>();
builder.Services.AddScoped<IngestionService>();
builder.Services.AddScoped<RateLimitService>();
builder.Services.AddScoped<PlanService>();

// File upload limits
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1024L * 1024L * 100L; // 100MB
});

var app = builder.Build();

// Ensure DB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();
        DbSeeder.Seed(db);
    }
    catch
    {
        db.Database.EnsureCreated();
        DbSeeder.Seed(db);
    }
}

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

// Rate limit middleware
app.Use(async (context, next) =>
{
    var rateLimiter = context.RequestServices.GetRequiredService<RateLimitService>();
    var allowed = await rateLimiter.IsAllowedAsync(context);
    if (!allowed)
    {
        context.Response.StatusCode = 429;
        await context.Response.WriteAsJsonAsync(new { error = "rate_limited", message = "Rate limit exceeded. Try again later." });
        return;
    }
    await next();
});

app.MapControllers();

app.Run();

public sealed class AppSettings
{
    public string? AzureSearchEndpoint { get; set; }
    public string? AzureSearchApiKey { get; set; }
    public string? AzureSearchIndex { get; set; }
    public string? AzureOpenAIEndpoint { get; set; }
    public string? AzureOpenAIApiKey { get; set; }
    public string? AzureOpenAIDeployment { get; set; }
    public string? AzureOpenAIEmbedDeployment { get; set; }
    public string? AzureDocIntelEndpoint { get; set; }
    public string? AzureDocIntelKey { get; set; }
}

