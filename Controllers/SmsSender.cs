using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

public class SmsSender
{
    private readonly IConfiguration _config;

    public SmsSender(IConfiguration config)
    {
        _config = config;
    }

    
     public (bool Success, string? ErrorMessage) SendSms(string phoneNumber, string message)
    {
        var accountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
        var authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
        var fromPhone = _config["Twilio:FromPhone"];
        try
        {
            // Initialize Twilio client
            TwilioClient.Init(accountSid, authToken);

            // Send SMS message
            var messageResource = MessageResource.Create(
                to: new PhoneNumber(phoneNumber),
                from: new PhoneNumber(fromPhone),
                body: message
            );

            // Check if the SMS was successfully queued or sent
            if (messageResource.Status == MessageResource.StatusEnum.Queued || messageResource.Status == MessageResource.StatusEnum.Sent)
            {
                return (true, null); // SMS sent successfully
            }
            else
            {
                return (false, $"SMS sending failed. Status: {messageResource.Status}");
            }
        }
        catch (ApiException apiEx)
        {
            // Handle Twilio-specific API errors (like invalid phone number, etc.)
            return (false, $"Twilio API Error: {apiEx.Message}");
        }
        catch (Exception ex)
        {
            // Handle general exceptions
            return (false, $"An error occurred while sending SMS: {ex.Message}");
        }
    }
}