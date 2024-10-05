using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using TeamIt.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace TeamIt.Controllers;
public class ConversationsController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly MessengerContext _context;
    private readonly IHubContext<ChatHub> _hubContext;

    public ConversationsController(UserManager<User> userManager, MessengerContext context, IHubContext<ChatHub> hubContext)
    {
        _userManager = userManager;
        _context = context;
        _hubContext = hubContext;
    }

    // Get the logged-in user's conversations
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return Unauthorized();
        }

        // Retrieve conversations where the current user is a participant
        var conversations = await _context.UserConversations
            .Where(uc => uc.UserId == currentUser.Id)
            .Include(uc => uc.Conversation)       
            .ThenInclude(c => c.UserConversations)    
            .ThenInclude(uc => uc.User)                
            .Select(uc => new 
            {
                Conversation = uc.Conversation,
                LastMessage = uc.Conversation.Messages.OrderByDescending(m => m.Timestamp).FirstOrDefault() // Get only the last message
            })         
            .ToListAsync();

        return View((conversations, currentUser));
        }

    // Show form to create a new conversation
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // Handle creating a new conversation
    [HttpPost]
    public async Task<IActionResult> Create(string phoneNumber)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return Unauthorized();
        }

        // Find the user by phone number
        var recipientUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        if (recipientUser == null)
        {
            TempData["Error"] = "User with the specified phone number not found.";
            return RedirectToAction("Index");
        }

        // Retrieve all conversations where either user is a participant
        var allConversations = await _context.UserConversations
            .Where(uc => uc.UserId == currentUser.Id || uc.UserId == recipientUser.Id)
            .Include(uc => uc.Conversation) // Ensure the Conversation entity is included
            .ToListAsync(); // Fetch the data first

        // roup by ConversationId and check if there are exactly two participants (current user and recipient)
        var existingConversation = allConversations
            .GroupBy(uc => uc.ConversationId)
            .Where(g => g.Count() == 2 && g.Any(uc => uc.UserId == currentUser.Id) && g.Any(uc => uc.UserId == recipientUser.Id))
            .Select(g => g.First().Conversation)
            .FirstOrDefault(c => !c.IsGroup); // Ensure it's not a group conversation

        if (existingConversation != null)
        {
            // Return the existing conversation if found
            return RedirectToAction("_ChatMessagesPartial", new { id = existingConversation.Id });
        }

        // Create a new conversation since none exists
        var newConversation = new Conversation
        {
            Title = "Conversation",
            IsGroup = false,
            CreatedAt = DateTime.UtcNow,
            UserConversations = new List<UserConversation>()
        };

        // Add the current user as a participant
        newConversation.UserConversations.Add(new UserConversation
        {
            UserId = currentUser.Id,
            IsAdmin = true, // Make the current user the admin
            JoinedAt = DateTime.UtcNow
        });
        // Add the recipient as a participant
        newConversation.UserConversations.Add(new UserConversation
        {
            UserId = recipientUser.Id,
            IsAdmin = false,
            JoinedAt = DateTime.UtcNow
        });
        // Save the new conversation
        _context.Conversations.Add(newConversation);
        await _context.SaveChangesAsync();

        // Return the newly created conversation
        return RedirectToAction("_ChatMessagesPartial", new { id = newConversation.Id });
    }
    // Display the conversation and its messages
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return Unauthorized();
        }

        // Fetch the conversation and its messages if the user is part of it
        var conversation = await _context.Conversations
            .Include(c => c.Messages)
            .ThenInclude(m => m.Sender)
            .Include(c => c.UserConversations)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserConversations.Any(uc => uc.UserId == currentUser.Id));

        if (conversation == null)
        {
            return NotFound();
        }
        ViewBag.CurrentUserId = currentUser.Id;
        return PartialView("_ChatMessagesPartial", conversation);
    }

    // Handle sending a new message
    [HttpPost]
    public async Task<IActionResult> SendMessage(int conversationId, string messageContent)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return Unauthorized();
        }

        // Ensure the user is part of the conversation
        var conversation = await _context.Conversations
            .Include(c => c.UserConversations)
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserConversations.Any(uc => uc.UserId == currentUser.Id));

        if (conversation == null)
        {
            return NotFound();
        }

        // Create the new message
        var newMessage = new Message
        {
            ConversationId = conversationId,
            SenderId = currentUser.Id,
            Content = messageContent,
            Timestamp = DateTime.UtcNow
        };

        // Add the message to the database
        _context.Messages.Add(newMessage);
        await _context.SaveChangesAsync();

        // Notify clients in the conversation group via SignalR, including the timestamp
        await _hubContext.Clients.Group(conversationId.ToString())
            .SendAsync("ReceiveMessage", currentUser.Id, messageContent, newMessage.Timestamp.ToString("HH:mm"));

        return NoContent();
    }
}