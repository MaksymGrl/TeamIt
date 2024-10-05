using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeamIt;
using TeamIt.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddSession();
// Configure Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedPhoneNumber = true;
})
    .AddEntityFrameworkStores<MessengerContext>()
    .AddDefaultTokenProviders();
    
builder.Services.AddDbContext<MessengerContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MessengerContext")));

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80); 
});
// Register the SmsSender service
builder.Services.AddTransient<SmsSender>();
// Configure TwilioSettings
builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("Twilio"));



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.MapHub<ChatHub>("/chathub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


/*
// Seed data on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<User>>();
    var seeder = new DataSeeder(userManager);
    await seeder.SeedTestUsersAsync();
}
*/

app.Run();
