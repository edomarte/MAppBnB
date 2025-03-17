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
    public static string SendDocsToTown(Booking booking, Person mainPerson, Document document)
    {
        message = "Main person on booking: " + mainPerson.Name + " " + mainPerson.Surname + "\n"
        + "Booking from " + booking.CheckinDateTime + " to " + booking.CheckOutDateTime + "\n"
        + "Document type: " + document.DocumentType + "\n"
        + "Document number: " + document.SerialNumber + "\n"
        + "Document from: " + document.IssuingCountry + "\n"
        + "Please see attached copy of the document." + "\n"
        + "Kind regards";

        return SendEmail("edomarte@gmail.com", message, document.PdfCopy, "Booking Documents");
    }

    private static string SendEmail(string emailRecipient, string emailContent, byte[] attachmentBase64, string subject)
    {
        Configuration.Default.ApiKey.Add("api-key", ""); // Latest is on github repository secret

        var apiInstance = new TransactionalEmailsApi();
        var sendSmtpEmail = new SendSmtpEmail(
            sender: new SendSmtpEmailSender(email: "edomarte@gmail.com", name: "MAppBnB"),
            to: new List<SendSmtpEmailTo> { new SendSmtpEmailTo(email: emailRecipient, name: "Town") },
            subject: subject,
            htmlContent: "<html><body><h1>Email Body</h1></body></html>",
            attachment: new List<SendSmtpEmailAttachment>{
                new SendSmtpEmailAttachment(content: attachmentBase64,
                name: "Document.pdf"
            )}

        );

        try
        {
            var result = apiInstance.SendTransacEmail(sendSmtpEmail);
            return result.ToString();
        }
        catch (Exception e)
        {
            return "Exception when calling TransactionalEmailsApi.SendTransacEmail: " + e.Message;
        }
    }

}


