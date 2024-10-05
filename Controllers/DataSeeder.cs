using Microsoft.AspNetCore.Identity;
using TeamIt.Models;

public class DataSeeder
{
    private readonly UserManager<User> _userManager;

    public DataSeeder(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task SeedTestUsersAsync()
    {
        // Check if users already exist
        if (await _userManager.FindByEmailAsync("testuser1@example.com") == null)
        {
            var user1 = new User
            {
                UserName = "testuser1",
                Email = "testuser1@example.com",
                Status = "online",
                LastSeen = DateTime.UtcNow
            };

            await _userManager.CreateAsync(user1, "Test@123"); // Creating with password
        }

        if (await _userManager.FindByEmailAsync("testuser2@example.com") == null)
        {
            var user2 = new User
            {
                UserName = "testuser2",
                Email = "testuser2@example.com",
                Status = "offline",
                LastSeen = DateTime.UtcNow.AddMinutes(-30)
            };

            await _userManager.CreateAsync(user2, "Test@123"); // Creating with password
        }
    }
}