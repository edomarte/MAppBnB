using System.Configuration;
using DocumentFormat.OpenXml.Math;
using MAppBnB;
using MAppBnB.Data;
using Microsoft.AspNetCore.SignalR;
using Configuration = MAppBnB.Models.Configuration;


namespace SignalRChat.Hubs //TODO: Change namespace
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

        public List<Person> GetMainPersons(List<Booking> bookings)
        {
            List<int> bookingIds = bookings.Select(b => b.id).ToList(); //Just the first one for each booking
            List<Person> mainPersonList = _context.BookingPerson
    .Join(_context.Person,
          bp => bp.PersonID,
          p => p.id,
          (bp, p) => new { bp, p })
    .Where(joined => bookingIds.Contains(joined.bp.BookingID) && 
                     Convert.ToInt32(joined.p.RoleRelation) >= 16 && 
                     Convert.ToInt32(joined.p.RoleRelation) <= 18)
    .Select(joined => joined.p)  // Selects the Person entity
    .ToList(); // Change Host in tipoalloggiato in a number

            return mainPersonList;
        }

        public List<int> GetGuestNums(List<Booking> bookings)
        {
            List<int> bookingIds = bookings.Select(b => b.id).ToList();
            List<int> guestsPerBooking = _context.BookingPerson
                                        .Where(bp=>bookingIds.Contains(bp.id))
                                        .GroupBy(bp => bp.BookingID)
                                        .Select(g => g.Count())
                                        .ToList();

            return guestsPerBooking;
        }

        public List<Room> GetRooms(List<Booking> bookings)
        {
            List<int> roomIds = bookings.Select(b => b.RoomID).ToList();
            List<Room> mainPersonList = _context.Room.Where(r => roomIds.Contains(r.id)).ToList();

            return mainPersonList;
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

        public BookChannel GetChannelDetailsS(string? channelId)
        {
            var channel = _context.BookChannel.Where(x => x.id == Convert.ToInt32(channelId)).ToList();
            return channel[0];
        }

        public Configuration GetConfiguration()
        {
            var configurations = _context.Configuration.ToList();
            return configurations[0];
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

            await Clients.All.SendAsync("DownloadFile", contractPath.Substring(contractPath.LastIndexOf("\\") + 1), base64String);

        }

        public async Task CreatePreCheckin(string mainPersonID, string accommodationID, string bookingId)
        {
            Person mainPerson = GetPersonDetails(mainPersonID);
            Accommodation accommodation = GetAccommodationDetails(accommodationID);
            string contractPath = DocumentProcessing.GeneratePreCheckIn(mainPerson, accommodation, GetBookingDetails(bookingId));
            byte[] file = await File.ReadAllBytesAsync(contractPath);
            string base64String = Convert.ToBase64String(file);

            await Clients.All.SendAsync("DownloadFile", contractPath.Substring(contractPath.LastIndexOf("\\") + 1), base64String);

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

            await Clients.All.SendAsync("DownloadFile", contractPath.Substring(contractPath.LastIndexOf("\\") + 1), base64String);

        }

        private List<Booking> GetBookingsForReport(string accommodationID, string channelID, string dateFrom, string dateTo)
        {
            // First, fetch the necessary data into memory
            return _context.Booking
                             .Where(x => x.AccommodationID == Convert.ToInt32(accommodationID)
                                        && x.ChannelID == Convert.ToInt32(channelID)
                                        && x.CheckinDateTime >= Convert.ToDateTime(dateFrom)
                                        && x.CheckinDateTime <= Convert.ToDateTime(dateTo))
                             .ToList(); // Fetches data into memory
        }

        public async Task CreateReportExcel(string accommodationID, string channelID, string dateFrom, string dateTo)
        {
            List<Booking> bookings = GetBookingsForReport(accommodationID, channelID, dateFrom, dateTo);
            string contractPath = DocumentProcessing.GenerateExcelFinancialReport(bookings, GetChannelDetailsS(channelID), GetMainPersons(bookings), GetRooms(bookings), GetAccommodationDetails(accommodationID), dateFrom, dateTo, GetConfiguration(), GetGuestNums(bookings));

            byte[] file = await File.ReadAllBytesAsync(contractPath);
            string base64String = Convert.ToBase64String(file);

            await Clients.All.SendAsync("DownloadFile", contractPath.Substring(contractPath.LastIndexOf("\\") + 1), base64String);

        }
    }
}

//TODO: what happens when  same person book two different times with two different roles?6