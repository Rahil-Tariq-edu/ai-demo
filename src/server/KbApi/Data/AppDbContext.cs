using KbApi.Models;
using Microsoft.EntityFrameworkCore;

namespace KbApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Chunk> Chunks => Set<Chunk>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<UsageEvent> UsageEvents => Set<UsageEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Document>()
            .HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId);

        modelBuilder.Entity<Chunk>()
            .HasOne(c => c.Document)
            .WithMany()
            .HasForeignKey(c => c.DocumentId);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Conversation)
            .WithMany()
            .HasForeignKey(m => m.ConversationId);
    }
}

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (!db.Users.Any())
        {
            db.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Plan = PlanType.Starter,
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();
        }

        if (!db.Documents.Any())
        {
            var user = db.Users.First();
            var doc = new Document
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Title = "Sample FAQ",
                SourceType = SourceType.Text,
                FilePathOrUrl = "seed",
                Status = DocumentStatus.Processed,
                CreatedAt = DateTime.UtcNow
            };
            db.Documents.Add(doc);
            db.Chunks.Add(new Chunk
            {
                Id = Guid.NewGuid(),
                DocumentId = doc.Id,
                ChunkNo = 1,
                Content = "Q: What is this app? A: A Knowledge Base Chatbot MVP.",
                SourceTitle = doc.Title,
                SourceUrl = "",
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();
        }
    }
}

