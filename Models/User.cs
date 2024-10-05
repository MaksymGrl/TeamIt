using Microsoft.AspNetCore.Identity;

namespace TeamIt.Models;
public class User : IdentityUser
    {
        public string? DisplayName { get; set; }
        public string? PhoneVerificationCode {get; set;}
        public string Status { get; set; } // e.g., online, offline, busy
        public DateTime LastSeen { get; set; }
        public ICollection<UserConversation>? UserConversations { get; set; }
    }