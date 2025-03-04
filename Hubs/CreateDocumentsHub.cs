using MAppBnB;
using MAppBnB.Data;
using Microsoft.AspNetCore.SignalR;


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

        public Document GetDocumentDetails(Person person)
        {
            var details = _context.Document.Where(x => x.id == person.DocumentID).ToList();
            return details[0];
        }

        public Booking GetBookingDetails(string bookingID)
        {
            var details = _context.Booking.Where(x => x.id == int.Parse(bookingID)).ToList();
            return details[0];
        }

        public async Task CreateContract(string mainPersonID, string accommodationID, string bookingId)
        {
            Person mainPerson = GetPersonDetails(mainPersonID);
            Accommodation accommodation = GetAccommodationDetails(accommodationID);
            string contractPath = DocumentProcessing.GenerateContract(mainPerson, GetDocumentDetails(mainPerson), accommodation, GetBookingDetails(bookingId));
            byte[] file = await File.ReadAllBytesAsync(contractPath);
            string base64String = Convert.ToBase64String(file);

            await Clients.All.SendAsync("ContractFile", contractPath.Substring(contractPath.LastIndexOf("\\") + 1), base64String);

        }
    }
}