namespace TeamIt.Models;
public class Message
{
    public int Id { get; set; }
    public int ConversationId { get; set; } // Foreign key to Conversation
    public Conversation Conversation { get; set; }

    public string SenderId { get; set; } // Foreign key to User (sender)
    public User Sender { get; set; }

    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
    
    public bool IsRead { get; set; } // to track if the message was read
}