namespace TeamIt.Models;
public class UserConversation
{
    public string UserId { get; set; }
    public User User { get; set; }

    public int ConversationId { get; set; }
    public Conversation Conversation { get; set; }

    public bool IsAdmin { get; set; } // Indicates if the user is an admin of the conversation
    public DateTime JoinedAt { get; set; }
}