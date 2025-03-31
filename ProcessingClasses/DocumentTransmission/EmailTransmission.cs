using MAppBnB;
using brevo_csharp.Api;
using brevo_csharp.Client;
using brevo_csharp.Model;
using Microsoft.IdentityModel.Tokens;

public class EmailTransmission
{
    static string message;
    public static string SendContract(Booking booking, Person mainPerson, string accommodationName, string roomName, byte[] contractFile)
    {
        message = "Hi " + mainPerson.Name + " " + mainPerson.Surname + "!<br>"
        + "Regarding your booking at " + accommodationName + " in room " + roomName + " from " + booking.CheckinDateTime + " to " + booking.CheckOutDateTime + "<br>"
        + "Please see attached copy of the contract." + "<br>"
        + "Kind regards";

        return SendEmail("edomarte@gmail.com", message, contractFile, "Booking Documents");
    }

    private static string SendEmail(string emailRecipient, string emailContent, byte[] attachmentBase64, string subject)
    {
        // Read API Key from Environment Variable
        string brevoApiKey = Environment.GetEnvironmentVariable("BREVO_API_KEY");

        if (string.IsNullOrEmpty(brevoApiKey))
        {
            throw new Exception("Brevo API key is missing.");
        }

        if (Configuration.Default.ApiKey.IsNullOrEmpty())
            Configuration.Default.ApiKey.Add("api-key", brevoApiKey); // System environment variable

        var apiInstance = new TransactionalEmailsApi();
        var sendSmtpEmail = new SendSmtpEmail(
            sender: new SendSmtpEmailSender(email: "edomarte@gmail.com", name: "MAppBnB"), // Developer personal email used because an email for the app has not been setup yet.
            to: new List<SendSmtpEmailTo> { new SendSmtpEmailTo(email: emailRecipient, name: "Guest") },
            subject: subject,
            htmlContent: "<html><body>" + emailContent + "</body></html>",
            attachment: new List<SendSmtpEmailAttachment>{
                new SendSmtpEmailAttachment(content: attachmentBase64,
                name: "Contract.pdf"
            )}

        );

        try
        {
            var result = apiInstance.SendTransacEmail(sendSmtpEmail);
            string temp = result.ToString();
            return "Email sent correctly.";
        }
        catch (Exception e)
        {
            return "Exception when calling TransactionalEmailsApi.SendTransacEmail: " + e.Message;
        }
    }

}


