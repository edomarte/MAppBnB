using System.Diagnostics.CodeAnalysis;
using MAppBnB;
using MAppBnB.Controllers;
using MAppBnB.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NuGet.Common;


namespace SignalRChat.Hubs
{
    public class CreateDocumentsHub : Hub
    {

        private readonly MappBnBContext _context;

        public CreateDocumentsHub(MappBnBContext context)
        {
            _context = context;
        }

        public Accommodation GetAccommodationDetails(string accommodationID)
        {
            var details = _context.Accommodation.Where(x => x.id == int.Parse(accommodationID)).ToList();            
            return details[0];
        }

        public Person GetPersonDetails(string personID)
        {
            var details = _context.Person.Where(x => x.id == int.Parse(personID)).ToList();            
            return details[0];
        }

        public Document GetDocumentDetails(string documentID)
        {
            var details = _context.Document.Where(x => x.id == int.Parse(documentID)).ToList();            
            return details[0];
        }

        public Booking GetBookingDetails(string bookingID)
        {
            var details = _context.Booking.Where(x => x.id == int.Parse(bookingID)).ToList();            
            return details[0];
        }

        public Task<IActionResult> CreateContract(string mainPersonID, string accommodationID, string bookingId)
        {
           Person mainPerson=GetPersonDetails(mainPersonID); 
           Accommodation accopmmodation=GetAccommodationDetails(accommodationID);
           string contractPath = DocumentProcessing.GenerateContract("contractParams", bookingId);
           return DownloadGeneratedDocument(contractPath);
        }

        private string createContractDetailsString(Person mainPerson, Document document, Accommodation accommodation, Booking booking)
        {
            string contractDetailsString="";
            //TODO: sistemare la stringa sotto
            //contractDetailsString+=mainPerson.Name+","+mainPerson.Surname+","+mainPerson.BirthPlace+","+mainPerson.BirthProvince+","+mainPerson.BirthDate+","+document.DocumentType+","+document.SerialNumber+","+document.IssuedBy+","+document.IssuedDate+","+mainPerson.PhonePrefix+","+mainPerson.PhoneNumber+","+mainPerson.Email+","+(booking.Price-booking.Discount)+","+booking.PaymentDate+","+(booking.CheckOutDateTime-booking.CheckinDateTime)+","+booking.CheckinDateTime+","+booking.CheckOutDateTime+","+System.DateTime.Today.ToString("dd/MM/yyyy")+","+accommodation.Address+","+accommodation.City;
            return "Contract details";
        }

        // GET: GenerateDocuments/DowloadFile
        public async Task<IActionResult> DownloadGeneratedDocument(string? filePath)
        {
            var file = System.IO.File.ReadAllBytes(filePath);
            return FileHelper.GetFileResult(file, "application/word", filePath.Substring(filePath.LastIndexOf("\\") + 1));
        }
    }

}

public static class FileHelper
{
    public static FileContentResult GetFileResult(byte[] fileContents, string contentType, string fileName)
    {
        return new FileContentResult(fileContents, contentType)
        {
            FileDownloadName = fileName
        };
    }
}
