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
            var person = _context.Person.Where(x => x.id == int.Parse(personID)).ToList();
            return person[0];
        }

        public List<Person> GetPersonsDetails(string[] personIDs)
        {
            int[] iPersonIDs = Array.ConvertAll(personIDs, int.Parse);
            var persons = _context.Person.Where(x => iPersonIDs.Contains(x.id)).ToList();
            return persons;
        }

        public Document GetDocumentDetails(Person person)
        {
            var document = _context.Document.Where(x => x.id == person.DocumentID).ToList();
            return document[0];
        }

        public Booking GetBookingDetails(string bookingID)
        {
            var booking = _context.Booking.Where(x => x.id == int.Parse(bookingID)).ToList();
            return booking[0];
        }

        public Room GetRoomDetails(int? roomID)
        {
            var room = _context.Room.Where(x => x.id == roomID).ToList();
            return room[0];
        }

        public BookChannel GetChannelDetails(int? channelId)
        {
            var channel = _context.BookChannel.Where(x => x.id == channelId).ToList();
            return channel[0];
        }

        public async Task CreateContract(string mainPersonID, string accommodationID, string bookingId)
        {
            Person mainPerson = GetPersonDetails(mainPersonID);
            Accommodation accommodation = GetAccommodationDetails(accommodationID);
            string contractPath = DocumentProcessing.GenerateContract(mainPerson, GetDocumentDetails(mainPerson), accommodation, GetBookingDetails(bookingId));
            byte[] file = await File.ReadAllBytesAsync(contractPath);
            string base64String = Convert.ToBase64String(file);

            await Clients.All.SendAsync("DownloadFile", contractPath.Substring(contractPath.LastIndexOf("\\") + 1), base64String);

        }

        public async Task CreateBookingDetails(string[] personsIDs, string accommodationID, string bookingId)
        {
            List<Person> persons = GetPersonsDetails(personsIDs);
            Accommodation accommodation = GetAccommodationDetails(accommodationID);
            Booking booking = GetBookingDetails(bookingId);
            string contractPath = DocumentProcessing.GenerateBookingDetails(persons, booking, accommodation, GetRoomDetails(booking.RoomID), GetChannelDetails(booking.ChannelID));
            byte[] file = await File.ReadAllBytesAsync(contractPath);
            string base64String = Convert.ToBase64String(file);

            await Clients.All.SendAsync("WordFile", contractPath.Substring(contractPath.LastIndexOf("\\") + 1), base64String);

        }

        public async Task CreatePreCheckin(string mainPersonID, string accommodationID, string bookingId)
        {
            Person mainPerson = GetPersonDetails(mainPersonID);
            Accommodation accommodation = GetAccommodationDetails(accommodationID);
            string contractPath = DocumentProcessing.GeneratePreCheckIn(mainPerson, accommodation, GetBookingDetails(bookingId));
            byte[] file = await File.ReadAllBytesAsync(contractPath);
            string base64String = Convert.ToBase64String(file);

            await Clients.All.SendAsync("WordFile", contractPath.Substring(contractPath.LastIndexOf("\\") + 1), base64String);

        }

        public async Task CreateContractPDF(string bookingId)
        {
            string contractPath = DocumentProcessing.GenerateContractPDF(bookingId);
            byte[] file = await File.ReadAllBytesAsync(contractPath);
            string base64String = Convert.ToBase64String(file);

            await Clients.All.SendAsync("DownloadFile", contractPath.Substring(contractPath.LastIndexOf("\\") + 1), base64String);

        }

        public async Task CreatePreCheckinPDF(string bookingId)
        {
            string contractPath = DocumentProcessing.GeneratePreCheckinPDF(bookingId);
            byte[] file = await File.ReadAllBytesAsync(contractPath);
            string base64String = Convert.ToBase64String(file);

            await Clients.All.SendAsync("PDFFile", contractPath.Substring(contractPath.LastIndexOf("\\") + 1), base64String);

        }
    }
}