using System.Net.Mail;
using System.Net.Mime;
using MAppBnB;
using brevo_csharp.Api;
using brevo_csharp.Client;
using brevo_csharp.Model;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

public class EmailTransmission
{
    static string message;
    public static string SendDocsToTown(Booking booking, Person mainPerson, Document document, string accommodationName, string roomName)
    {
        message = "Hi " + mainPerson.Name + " " + mainPerson.Surname + "!\n"
        + "Regarding your booking at "+accommodationName+" in room "+roomName+" from " + booking.CheckinDateTime + " to " + booking.CheckOutDateTime + "\n"
        + "Please see attached copy of the contract." + "\n"
        + "Kind regards";

        return SendEmail("edomarte@gmail.com", message, document.PdfCopy, "Booking Documents");
    }

    private static string SendEmail(string emailRecipient, string emailContent, byte[] attachmentBase64, string subject)
    {
        Configuration.Default.ApiKey.Add("api-key", ""); // TODO: Latest is on github repository secret

        var apiInstance = new TransactionalEmailsApi();
        var sendSmtpEmail = new SendSmtpEmail(
            sender: new SendSmtpEmailSender(email: "edomarte@gmail.com", name: "MAppBnB"), // Developer personal email used because an email for the app has not been setup yet.
            to: new List<SendSmtpEmailTo> { new SendSmtpEmailTo(email: emailRecipient, name: "Guest") },
            subject: subject,
            htmlContent: "<html><body>"+emailContent+"</body></html>",
            attachment: new List<SendSmtpEmailAttachment>{
                new SendSmtpEmailAttachment(content: attachmentBase64,
                name: "Contract.pdf"
            )}

        );

        try
        {
            var result = apiInstance.SendTransacEmail(sendSmtpEmail);
            string temp=result.ToString();
            //return result.ToString(); //TODO: verify the result
            return "Email sent correctly.";
        }
        catch (Exception e)
        {
            return "Exception when calling TransactionalEmailsApi.SendTransacEmail: " + e.Message;
        }
    }

}


