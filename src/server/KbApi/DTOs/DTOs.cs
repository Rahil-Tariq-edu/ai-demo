namespace KbApi.DTOs;

public class LoginRequest { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
public class LoginResponse { public string Token { get; set; } = string.Empty; public string Email { get; set; } = string.Empty; public string Plan { get; set; } = string.Empty; }

public class UploadTextRequest { public string Title { get; set; } = string.Empty; public string Text { get; set; } = string.Empty; }
public class UploadUrlRequest { public string Title { get; set; } = string.Empty; public string Url { get; set; } = string.Empty; }

public class AskRequest { public Guid? ConversationId { get; set; } public string Message { get; set; } = string.Empty; }
public class AskResponse { public Guid ConversationId { get; set; } public string Answer { get; set; } = string.Empty; public List<Citation> Citations { get; set; } = new(); }
public class Citation { public int Index { get; set; } public string Title { get; set; } = string.Empty; public string Url { get; set; } = string.Empty; public string Excerpt { get; set; } = string.Empty; }

