using MAppBnB;
using brevo_csharp.Api;
using brevo_csharp.Client;
using brevo_csharp.Model;
using Microsoft.IdentityModel.Tokens;

public class EmailTransmission
{
    static string message;

    // Method to send the contract via email.
    public static string SendContract(Booking booking, Person mainPerson, string accommodationName, string roomName, byte[] contractFile)
    {
        // Prepare email body.
        message = "Hi " + mainPerson.Name + " " + mainPerson.Surname + "!<br>"
        + "Regarding your booking at " + accommodationName + " in room " + roomName + " from " + booking.CheckinDateTime + " to " + booking.CheckOutDateTime + "<br>"
        + "Please see attached copy of the contract." + "<br>"
        + "Kind regards"+ "<br>"
        +accommodationName;

        // Send the email and return the result.
        return SendEmail("edomarte@gmail.com", message, contractFile, "Contract Document", "Contract");
    }

    // Method to send the pre-checkin document via email.
    public static string SendPreCheckIn(Booking booking, Person mainPerson, string accommodationName, string roomName, byte[] contractFile)
    {
        // Prepare email body.
        message = "Hi " + mainPerson.Name + " " + mainPerson.Surname + "!<br>"
        + "Regarding your booking at " + accommodationName + " in room " + roomName + " from " + booking.CheckinDateTime + " to " + booking.CheckOutDateTime + "<br>"
        + "Please see attached copy of the pre-checkin information." + "<br>"
        + "Kind regards"
        + "<br>"
        +accommodationName;

        // Send the email and return the result.
        return SendEmail("edomarte@gmail.com", message, contractFile, "Pre-Checkin Document", "Pre-Checkin");
    }

    // Method to use the Brevo email service API to send the email.
    private static string SendEmail(string emailRecipient, string emailContent, byte[] attachmentBase64, string subject, string attachmentName)
    {
        // Read API Key from Environment Variable
        string brevoApiKey = Environment.GetEnvironmentVariable("BREVO_API_KEY");

        // Throw an exception if the environment variable is empty.
        if (string.IsNullOrEmpty(brevoApiKey))
        {
            throw new Exception("Brevo API key is missing.");
        }

        // Add the API key to the Brevo configuration only if not already populated.
        if (Configuration.Default.ApiKey.IsNullOrEmpty())
            Configuration.Default.ApiKey.Add("api-key", brevoApiKey); // System environment variable

        // Get API instance.
        var apiInstance = new TransactionalEmailsApi();
        // Prepare a new Smtp email object. Populare the various part of the email.
        var sendSmtpEmail = new SendSmtpEmail(
            sender: new SendSmtpEmailSender(email: "edomarte@gmail.com", name: "MAppBnB"), // Developer personal email used because an email for the app has not been setup yet.
            to: new List<SendSmtpEmailTo> { new SendSmtpEmailTo(email: emailRecipient, name: "Guest") },
            subject: subject,
            htmlContent: "<html><body>" + emailContent + "</body></html>",
            attachment: new List<SendSmtpEmailAttachment>{
                new SendSmtpEmailAttachment(content: attachmentBase64,
                name: attachmentName+".pdf"
            )}

        );

        try
        {
            // Send the email and store the result.
            var result = apiInstance.SendTransacEmail(sendSmtpEmail);
            // Return a confirmation if the email is sent correctly.
            return attachmentName +" sent correctly.";
        }
        catch (Exception e)
        {
            // Return the exception message if any issues.
            return "Exception when calling TransactionalEmailsApi.SendTransacEmail: " + e.Message;
        }
    }

}


