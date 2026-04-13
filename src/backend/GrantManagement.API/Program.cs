using GrantManagement.Core.Interfaces;
using GrantManagement.Infrastructure.Data;
using GrantManagement.Infrastructure.Repositories;
using GrantManagement.Services.AI;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database (SQL Server — Windows Authentication) ──────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Repositories ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IGrantRepository, GrantRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IAIRepository, AIRepository>();

// ── AI Services ───────────────────────────────────────────────────────────────
builder.Services.AddHttpClient("openai");
builder.Services.AddScoped<IOpenAIService, OpenAIService>();
builder.Services.AddScoped<IContentSuggestionService, ContentSuggestionService>();
builder.Services.AddScoped<IChatbotService, ChatbotService>();

// ── Vector Search (Qdrant) ────────────────────────────────────────────────────
builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
builder.Services.AddSingleton<QdrantVectorService>();
builder.Services.AddSingleton<IVectorSearchService>(sp => sp.GetRequiredService<QdrantVectorService>());
builder.Services.AddHostedService<VectorIndexingService>();

// ── API / Swagger ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// ── CORS (allow Angular dev server) ──────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
