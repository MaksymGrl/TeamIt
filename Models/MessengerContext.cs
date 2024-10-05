using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TeamIt.Models;


public class MessengerContext : IdentityDbContext<User>
{
    public MessengerContext(DbContextOptions<MessengerContext> options) : base(options)
    {
    }

    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<UserConversation> UserConversations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Attachment> Attachments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);  


        modelBuilder.Entity<UserConversation>()
            .HasKey(uc => new { uc.UserId, uc.ConversationId });


        modelBuilder.Entity<Message>()
            .HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade); 
            
        // Configure many-to-many relationship
        modelBuilder.Entity<UserConversation>()
            .HasOne(uc => uc.User)
            .WithMany(u => u.UserConversations)
            .HasForeignKey(uc => uc.UserId);

        modelBuilder.Entity<UserConversation>()
            .HasOne(uc => uc.Conversation)
            .WithMany(c => c.UserConversations)
            .HasForeignKey(uc => uc.ConversationId);
    }
}