using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TeamIt.Models;

namespace TeamIt.Controllers;
public class MessagesController : Controller
{
    private readonly MessengerContext _context;

    // Constructor to inject MessengerContext
    public MessagesController(MessengerContext context)
    {
        _context = context;
    }

    // GET: Messages (List all messages)
    public async Task<IActionResult> Index()
    {
        var messages = await _context.Messages.OrderBy(m => m.Timestamp).ToListAsync() ?? [];
        return View(messages);
    }

    // POST: SendMessage (Send a new message)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(string senderUsername, string receiverUsername, string content)
    {
        if (string.IsNullOrWhiteSpace(senderUsername) || string.IsNullOrWhiteSpace(receiverUsername) || string.IsNullOrWhiteSpace(content))
        {
            // Handle invalid input
            ModelState.AddModelError(string.Empty, "All fields are required.");
            return RedirectToAction("Index");
        }

        // Find the sender and receiver in the database
        var sender = await _context.Users.FirstOrDefaultAsync(u => u.UserName == senderUsername);
        var receiver = await _context.Users.FirstOrDefaultAsync(u => u.UserName == receiverUsername);

        if (sender == null || receiver == null)
        {
            // Handle case where either user does not exist
            ModelState.AddModelError(string.Empty, "Sender or receiver does not exist.");
            return RedirectToAction("Index");
        }

       // Optionally, find if there is an existing conversation between the sender and receiver
        var conversation = await _context.Conversations
            .Where(c => !c.IsGroup)
            .Where(c => c.UserConversations.Any(uc => uc.UserId == sender.Id) && c.UserConversations.Any(uc => uc.UserId == receiver.Id))
            .FirstOrDefaultAsync();

        if (conversation == null)
        {
            // Create a new conversation if none exists
            conversation = new Conversation
            {
                IsGroup = false,
                CreatedAt = DateTime.UtcNow,
                Title = "Dummy",
                UserConversations = new List<UserConversation>
                {
                    new UserConversation { UserId = sender.Id, JoinedAt = DateTime.UtcNow },
                    new UserConversation { UserId = receiver.Id, JoinedAt = DateTime.UtcNow }
                }
            };
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();  // Save the conversation before using its ID
        }

        // Create a new message
        var message = new Message
        {
            ConversationId = conversation.Id,
            SenderId = sender.Id,
            Content = content,
            Timestamp = DateTime.UtcNow
        };

        // Add the message to the database and save changes
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}