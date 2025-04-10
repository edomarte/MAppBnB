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
            var details = _context.Accommodation.Find(Convert.ToInt32(accommodationID));
            return details;
        }

        public Person GetPersonDetails(string personID)
        {
            var person = _context.Person.Find(Convert.ToInt32(personID));
            return person;
        }

        public Person GetMainPerson(Booking booking)
        {
            int bookingId = booking.id;
            Person mainPerson = _context.BookingPerson
    .Join(_context.Person,
          bp => bp.PersonID,
          p => p.id,
          (bp, p) => new { bp, p })
    .Where(joined => bookingId == joined.bp.BookingID &&
                     Convert.ToInt32(joined.p.RoleRelation) >= 16 &&
                     Convert.ToInt32(joined.p.RoleRelation) <= 18)
    .Select(joined => joined.p)  // Selects the Person entity
    .ToList()[0]; // Change Host in tipoalloggiato in a number

            return mainPerson;
        }

        public int GetGuestCount(Booking booking)
        {
            int bookingId = booking.id;
            int guestsPerBooking = _context.BookingPerson
                                        .Where(bp => bookingId == bp.BookingID)
                                        .GroupBy(bp => bp.BookingID)
                                        .Select(g => g.Count()).ToList()[0];
            return guestsPerBooking;
        }

        public Room GetRoom(Booking booking)
        {
            int roomId = booking.RoomID;
            Room roomsList = _context.Room.Find(roomId);

            return roomsList;
        }


        public List<Person> GetPersonsDetails(string[] personIDs)
        {
            int[] iPersonIDs = Array.ConvertAll(personIDs, int.Parse);
            var persons = _context.Person.Where(x => iPersonIDs.Contains(x.id)).ToList();
            return persons;
        }

        public Document GetDocumentDetails(Person person)
        {
            var document = _context.Document.Find(person.DocumentID);
            // Substitute the code with the correspondent document description
            document.DocumentType = _context.TipoDocumento.Find(document.DocumentType).Descrizione;
            return document;
        }

        public Booking GetBookingDetails(string bookingID)
        {
            var booking = _context.Booking.Find(Convert.ToInt32(bookingID));
            return booking;
        }

        public Room GetRoomDetails(int? roomID)
        {
            var room = _context.Room.Find(roomID);
            return room;
        }

        public BookChannel GetChannelDetailsS(string? channelId)
        {
            var channel = _context.BookChannel.Find(Convert.ToInt32(channelId));
            return channel;
        }

        public Configuration GetConfiguration()
        {
            var configurations = _context.Configuration.FirstOrDefault(); // only one configuration exist
            return configurations;
        }
        public BookChannel GetChannelDetails(int? channelId)
        {
            var channel = _context.BookChannel.Find(channelId);
            return channel;
        }

        private Person getBirthPlaceDescription(Person p)
        {
            if (p.BirthCountry.Equals("100000100"))
            { // If birthcountry is Italy
                p.BirthPlace = _context.Comuni.Find(p.BirthPlace).Descrizione;
            }
            else
            {
                p.BirthPlace = "Estero";
            }
            return p;
        }

        private async Task updateIsContractPrintedAsync(Booking booking)
        {
            booking.ContractPrinted = true;
            _context.Update(booking);
            await _context.SaveChangesAsync();
        }

        public async Task CreateContract(string mainPersonID, string accommodationID, string bookingId)
        {
            try
            {
                Person mainPerson = getBirthPlaceDescription(GetPersonDetails(mainPersonID));
                if (_context.Configuration.FirstOrDefault().PersonID.ToString().Equals(""))
                {
                    throw new Exception("Host not set in configuration!");
                }
                //Person host = getBirthPlaceDescription(GetPersonDetails(_context.Configuration.FirstOrDefault().PersonID.ToString()));
                Person host = GetPersonDetails(_context.Configuration.FirstOrDefault().PersonID.ToString());
                Accommodation accommodation = GetAccommodationDetails(accommodationID);
                Booking booking = GetBookingDetails(bookingId);
                string contractPath = DocumentProcessing.GenerateContract(mainPerson, GetDocumentDetails(mainPerson), accommodation, booking, host, GetDocumentDetails(host));
                byte[] file = await File.ReadAllBytesAsync(contractPath);
                string base64String = Convert.ToBase64String(file);
                updateIsContractPrintedAsync(booking);
                await Clients.All.SendAsync("DownloadFile", contractPath.Substring(contractPath.LastIndexOf("\\") + 1), base64String);
            }
            catch (Exception e)
            {
                await Clients.All.SendAsync("Error", e.Message);
            }
        }

        public async Task CreateBookingDetails(string[] personsIDs, string accommodationID, string bookingId)
        {
            try
            {
                List<Person> persons = GetPersonsDetails(personsIDs);
                Accommodation accommodation = GetAccommodationDetails(accommodationID);
                Booking booking = GetBookingDetails(bookingId);
                string contractPath = DocumentProcessing.GenerateBookingDetails(persons, booking, accommodation, GetRoomDetails(booking.RoomID), GetChannelDetails(booking.ChannelID));
                byte[] file = await File.ReadAllBytesAsync(contractPath);
                string base64String = Convert.ToBase64String(file);

                await Clients.All.SendAsync("DownloadFile", contractPath.Substring(contractPath.LastIndexOf("\\") + 1), base64String);
            }
            catch (Exception e)
            {
                await Clients.All.SendAsync("Error", e.Message);

            }
        }

        public async Task CreatePreCheckin(string mainPersonID, string accommodationID, string bookingId)
        {
            try
            {
                Person mainPerson = GetPersonDetails(mainPersonID);
                Accommodation accommodation = GetAccommodationDetails(accommodationID);
                string contractPath = DocumentProcessing.GeneratePreCheckIn(mainPerson, accommodation, GetBookingDetails(bookingId));
                byte[] file = await File.ReadAllBytesAsync(contractPath);
                string base64String = Convert.ToBase64String(file);

                await Clients.All.SendAsync("DownloadFile", contractPath.Substring(contractPath.LastIndexOf("\\") + 1), base64String);
            }
            catch (Exception e)
            {
                await Clients.All.SendAsync("Error", e.Message);

            }
        }

        public async Task CreateContractPDF(string bookingId)
        {
            try
            {
                string contractPath = DocumentProcessing.GenerateContractPDF(bookingId);
                byte[] file = await File.ReadAllBytesAsync(contractPath);
                string base64String = Convert.ToBase64String(file);
                await Clients.All.SendAsync("DownloadFile", contractPath.Substring(contractPath.LastIndexOf("\\") + 1), base64String);
            }
            catch (Exception e)
            {
                await Clients.All.SendAsync("Error", e.Message);
            }
        }

        public async Task CreatePreCheckinPDF(string bookingId)
        {
            try
            {
                string contractPath = DocumentProcessing.GeneratePreCheckinPDF(bookingId);
                byte[] file = await File.ReadAllBytesAsync(contractPath);
                string base64String = Convert.ToBase64String(file);
                await Clients.All.SendAsync("DownloadFile", contractPath.Substring(contractPath.LastIndexOf("\\") + 1), base64String);
            }
            catch (Exception e)
            {
                await Clients.All.SendAsync("Error", e.Message);
            }
        }

        private List<Booking> GetBookingsForReport(string accommodationID, string channelID, string dateFrom, string dateTo)
        {

            //If all channels
            if (channelID.Equals("0"))
            {
                return _context.Booking
                                             .Where(x => x.AccommodationID == Convert.ToInt32(accommodationID)
                                                        && x.CheckinDateTime >= Convert.ToDateTime(dateFrom)
                                                        && x.CheckinDateTime <= Convert.ToDateTime(dateTo))
                                             .ToList(); // Fetches data into memory
            }
            else
            {
                return _context.Booking
                                             .Where(x => x.AccommodationID == Convert.ToInt32(accommodationID)
                                                        && x.ChannelID == Convert.ToInt32(channelID)
                                                        && x.CheckinDateTime >= Convert.ToDateTime(dateFrom)
                                                        && x.CheckinDateTime <= Convert.ToDateTime(dateTo))
                                             .ToList(); // Fetches data into memory
            }

            // First, fetch the necessary data into memory

        }

        public async Task CreateReportExcel(string accommodationID, string channelID, string dateFrom, string dateTo)
        {
            try
            {
                List<Booking> bookings = GetBookingsForReport(accommodationID, channelID, dateFrom, dateTo);
                string contractPath = DocumentProcessing.GenerateExcelFinancialReport(getReportLines(bookings), GetChannelDetailsS(channelID), GetAccommodationDetails(accommodationID), dateFrom, dateTo, GetConfiguration());

                byte[] file = await File.ReadAllBytesAsync(contractPath);
                string base64String = Convert.ToBase64String(file);

                await Clients.All.SendAsync("DownloadFile", contractPath.Substring(contractPath.LastIndexOf("\\") + 1), base64String);
            }
            catch (Exception e)
            {
                await Clients.All.SendAsync("Error", e.Message);
            }
        }

        private List<FinancialReportLine> getReportLines(List<Booking> bookings)
        {
            List<FinancialReportLine> lfrl = new List<FinancialReportLine>();

            foreach (Booking booking in bookings)
            {
                lfrl.Add(new FinancialReportLine() { Booking = booking, MainPerson = GetMainPerson(booking), Room = GetRoom(booking), GuestCount = GetGuestCount(booking) });
            }

            return lfrl;
        }
    }
}