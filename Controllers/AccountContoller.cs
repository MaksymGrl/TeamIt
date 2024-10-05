using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TeamIt.Models;
using Twilio.Rest.Api.V2010.Account;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly SmsSender _smsSender;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, SmsSender smsSender)
    {
        _smsSender = smsSender;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Authentication()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Authentication(string phoneNumber)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        bool registration = false;

        if (user == null)
        {
            registration = true;
            user = new User
            {
                UserName = phoneNumber,
                PhoneNumber = phoneNumber,
                DisplayName = phoneNumber,
                LastSeen = DateTime.UtcNow,
                Status = "offline",
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                // If user creation failed, display errors and return view
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View();
            }
        }

        // Generate verification code
        var verificationCode = new Random().Next(100000, 999999).ToString();
        user.PhoneVerificationCode = verificationCode;
        await _userManager.UpdateAsync(user);

        // Send SMS via Twilio and check the result
        var (success, errorMessage) = _smsSender.SendSms(phoneNumber, $"Your verification code is {verificationCode}");

        if (success)
        {
            // SMS sent successfully, redirect to phone verification page
            Console.WriteLine("Code: " + verificationCode); // For debug purposes

            if (registration)
            {
                return RedirectToAction("VerifyPhoneNumber", new { phoneNumber });
            }
            else
            {
                return RedirectToAction("VerifyPhoneNumberLogin", new { phoneNumber });
            }
        }
        else
        {
            if (registration)
            {
                await _userManager.DeleteAsync(user); // Delete the user if registration failed
            }

            // SMS sending failed, display the error message
            ModelState.AddModelError(string.Empty, errorMessage);
            return View();
        }
    }

    [HttpGet]
    public IActionResult VerifyPhoneNumber(string phoneNumber)
    {
        ViewBag.PhoneNumber = phoneNumber;
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> VerifyPhoneNumber(string phoneNumber, string verificationCode)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        if (user != null && user.PhoneVerificationCode == verificationCode)
        {
            user.PhoneNumberConfirmed = true;
            user.PhoneVerificationCode = null; // Clear the verification code
            await _userManager.UpdateAsync(user);

            return RedirectToAction("CompleteRegistration", new { phoneNumber });
        }

        ModelState.AddModelError(string.Empty, "Invalid verification code.");
        return View();
    }
    [HttpGet]
    public IActionResult VerifyPhoneNumberLogin(string phoneNumber)
    {
        ViewBag.PhoneNumber = phoneNumber;
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> VerifyPhoneNumberLogin(string phoneNumber, string verificationCode)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        if (user != null && user.PhoneVerificationCode == verificationCode)
        {
            user.PhoneVerificationCode = null; // Clear the verification code
            await _userManager.UpdateAsync(user);

            await _signInManager.SignInAsync(user, isPersistent: true);
            return RedirectToAction("Index", "Home");
        }
        ModelState.AddModelError(string.Empty, "Invalid verification code.");
        return View();
    }

    [HttpGet]
    public IActionResult CompleteRegistration(string phoneNumber)
    {
        ViewBag.PhoneNumber = phoneNumber;
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> CompleteRegistration(string phoneNumber, string displayName)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        if (user != null && user.PhoneNumberConfirmed)
        {
            user.DisplayName = displayName;
            await _userManager.UpdateAsync(user);
            // Sign in the user after registration is complete
            await _signInManager.SignInAsync(user, isPersistent: true);
            return RedirectToAction("Index", "Home");

        }
        ModelState.AddModelError(string.Empty, "User doesnt exist or number not confirmed");
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}