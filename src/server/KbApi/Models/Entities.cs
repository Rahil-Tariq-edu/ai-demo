namespace KbApi.Models;

public enum PlanType { Starter = 0, Pro = 1, Team = 2 }
public enum SourceType { Upload = 0, Text = 1, Url = 2 }
public enum DocumentStatus { Uploaded = 0, Processed = 1 }

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public PlanType Plan { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Document
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string Title { get; set; } = string.Empty;
    public SourceType SourceType { get; set; }
    public string FilePathOrUrl { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Chunk
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Document? Document { get; set; }
    public int ChunkNo { get; set; }
    public string Content { get; set; } = string.Empty;
    public string SourceTitle { get; set; } = string.Empty;
    public string? SourceUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Conversation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartedAt { get; set; }
}

public class Message
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Conversation? Conversation { get; set; }
    public string Role { get; set; } = string.Empty; // User/Assistant/System
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UsageEvent
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string EventType { get; set; } = string.Empty; // Ingest/Query
    public int TokensIn { get; set; }
    public int TokensOut { get; set; }
    public DateTime CreatedAt { get; set; }
}

