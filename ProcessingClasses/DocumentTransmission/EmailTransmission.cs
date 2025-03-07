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
    public static void SendDocsToTown(Booking booking, Person mainPerson, Document document)
    {
        message = "Main person on booking: " + mainPerson.Name + " " + mainPerson.Surname + "\n"
        + "Booking from " + booking.CheckinDateTime + " to " + booking.CheckOutDateTime + "\n"
        + "Document type: " + document.DocumentType + "\n"
        + "Document number: " + document.SerialNumber + "\n"
        + "Document issue date: " + document.IssuedDate + "\n"
        + "Document issued by: " + document.IssuedBy + "\n"
        + "Document from: " + document.IssuingCountry + "\n"
        + "Please see attached copy of the document." + "\n"
        + "Kind regards";

        SendEmail("edomarte@gmail.com", message, document.PdfCopy);
    }

    private static void SendEmail(string emailRecipient, string emailContent, byte[] attachmentBase64)
    {
        Configuration.Default.ApiKey.Add("api-key", "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");

        var apiInstance = new TransactionalEmailsApi();
        var sendSmtpEmail = new SendSmtpEmail(
            sender: new SendSmtpEmailSender(email: "edomarte@gmail.com", name: "MAppBnB"),
            to: new List<SendSmtpEmailTo> { new SendSmtpEmailTo(email: "edomarte@live.it", name: "Tester") },
            subject: "Subject",
            htmlContent: "<html><body><h1>Email Body</h1></body></html>",
            attachment: new List<SendSmtpEmailAttachment>{
                new SendSmtpEmailAttachment(content: attachmentBase64,
                name: "Document.pdf"
)}

        );

        try
        {
            var result = apiInstance.SendTransacEmail(sendSmtpEmail);
            Console.WriteLine(result.ToString());
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception when calling TransactionalEmailsApi.SendTransacEmail: " + e.Message);
        }
    }

}


