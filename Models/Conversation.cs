namespace TeamIt.Models;
public class Conversation
{
    public int Id { get; set; }
    public string Title { get; set; } // For group chats
    public bool IsGroup { get; set; } // True if the conversation is a group chat

    // List of participants in the conversation
    public ICollection<UserConversation> UserConversations { get; set; }
    public ICollection<Message> Messages { get; set; } 

    public DateTime CreatedAt { get; set; }
}