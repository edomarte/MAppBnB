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

#region SignalR Methods
        public async Task CreateContract(string mainPersonID, string accommodationID, string bookingId)
        {
            try
            {
                PersonBirthPlace mainPerson = new PersonBirthPlace() { Person = getPersonDetails(mainPersonID) };

                mainPerson.BirthPlace = getBirthPlaceDescription(mainPerson.Person);
                Configuration configuration = getConfiguration();
                if (configuration is null || configuration.PersonID is null || configuration.PersonID.ToString().Equals(""))
                {
                    throw new Exception("Host not set in configuration!");
                }
                //Person host = getBirthPlaceDescription(GetPersonDetails(_context.Configuration.FirstOrDefault().PersonID.ToString()));
                Person host = getPersonDetails(_context.Configuration.FirstOrDefault().PersonID.ToString());

                DocumentDocumentType hostDocument = getDocumentDetails(host);
                if (hostDocument is null)
                {
                    throw new Exception("Host document not set in configuration!");
                }

                Accommodation accommodation = getAccommodationDetails(accommodationID);
                Booking booking = getBookingDetails(bookingId);
                string contractPath = DocumentProcessing.GenerateContract(mainPerson, getDocumentDetails(mainPerson.Person), accommodation, booking, host, hostDocument);

                await startFileDownloadAsync(contractPath);
                // Update the booking to set ContractPrinted to true
                updateIsContractPrintedAsync(booking);
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
                List<Person> persons = getPersonsDetails(personsIDs);
                Accommodation accommodation = getAccommodationDetails(accommodationID);
                Booking booking = getBookingDetails(bookingId);
                string bdPath = DocumentProcessing.GenerateBookingDetails(persons, booking, accommodation, getRoomDetails(booking.RoomID), getChannelDetails(booking.ChannelID));
                await startFileDownloadAsync(bdPath);
                DocumentProcessing.DeleteDocument(bdPath); // Delete the document after download
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
                Person mainPerson = getPersonDetails(mainPersonID);
                Accommodation accommodation = getAccommodationDetails(accommodationID);
                string preCheckinPath = DocumentProcessing.GeneratePreCheckIn(mainPerson, accommodation, getBookingDetails(bookingId));
                await startFileDownloadAsync(preCheckinPath);
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
                await startFileDownloadAsync(contractPath);
                DocumentProcessing.DeleteDocument(contractPath); // Delete the contract after download
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
                string preCheckinPath = DocumentProcessing.GeneratePreCheckinPDF(bookingId);
                await startFileDownloadAsync(preCheckinPath);
                DocumentProcessing.DeleteDocument(preCheckinPath); // Delete the document after download
            }
            catch (Exception e)
            {
                await Clients.All.SendAsync("Error", e.Message);
            }
        }

        private List<Booking> GetBookingsForReport(string accommodationID, string channelID, string dateFrom, string dateTo)
        {
            //If all channels selected, do not search for channelID between the bookings
            if (channelID.Equals("-1"))
            {
                return _context.Booking
                             .Where(x => x.AccommodationID == Convert.ToInt32(accommodationID)
                                        && x.CheckinDateTime >= Convert.ToDateTime(dateFrom)
                                        && x.CheckinDateTime <= Convert.ToDateTime(dateTo))
                             .ToList();
            }
            return _context.Booking
                             .Where(x => x.AccommodationID == Convert.ToInt32(accommodationID)
                                        && x.ChannelID == Convert.ToInt32(channelID)
                                        && x.CheckinDateTime >= Convert.ToDateTime(dateFrom)
                                        && x.CheckinDateTime <= Convert.ToDateTime(dateTo))
                             .ToList(); // Fetches data into memory
        }

        public async Task CreateReportExcel(string accommodationID, string channelID, string dateFrom, string dateTo)
        {
            try
            {
                List<Booking> bookings = GetBookingsForReport(accommodationID, channelID, dateFrom, dateTo);
                string reportPath = DocumentProcessing.GenerateExcelFinancialReport(getReportLines(bookings), getChannelDetailsS(channelID), getAccommodationDetails(accommodationID), dateFrom, dateTo, getConfiguration());

                await startFileDownloadAsync(reportPath);
                DocumentProcessing.DeleteDocument(reportPath); // Delete the document after download
            }
            catch (Exception e)
            {
                await Clients.All.SendAsync("Error", e.Message);
            }
        }

        #endregion

        #region Helper methods
        private async Task startFileDownloadAsync(string filePath)
        {
            byte[] file = await File.ReadAllBytesAsync(filePath);
            string base64String = Convert.ToBase64String(file);

            await Clients.All.SendAsync("DownloadFile", Path.GetFileName(filePath), base64String);
        }
        private List<FinancialReportLine> getReportLines(List<Booking> bookings)
        {
            List<FinancialReportLine> lfrl = new List<FinancialReportLine>();

            foreach (Booking booking in bookings)
            {
                lfrl.Add(new FinancialReportLine() { Booking = booking, MainPerson = getMainPerson(booking), Room = getRoom(booking), Channel = getChannelDetails(booking.ChannelID), GuestCount = getGuestCount(booking) });
            }

            return lfrl;
        }

        private Accommodation getAccommodationDetails(string accommodationID)
        {
            var details = _context.Accommodation.Find(Convert.ToInt32(accommodationID));
            return details;
        }

        private Person getPersonDetails(string personID)
        {
            var person = _context.Person.Find(Convert.ToInt32(personID));
            return person;
        }

        private Person getMainPerson(Booking booking)
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

        private int getGuestCount(Booking booking)
        {
            int bookingId = booking.id;
            int guestsPerBooking = _context.BookingPerson
                                        .Where(bp => bookingId == bp.BookingID)
                                        .GroupBy(bp => bp.BookingID)
                                        .Select(g => g.Count()).ToList()[0];
            return guestsPerBooking;
        }

        private Room getRoom(Booking booking)
        {
            int roomId = booking.RoomID;
            Room roomsList = _context.Room.Find(roomId);

            return roomsList;
        }


        private List<Person> getPersonsDetails(string[] personIDs)
        {
            int[] iPersonIDs = Array.ConvertAll(personIDs, int.Parse);
            var persons = _context.Person.Where(x => iPersonIDs.Contains(x.id)).ToList();
            return persons;
        }

        private DocumentDocumentType getDocumentDetails(Person person)
        {
            DocumentDocumentType document = new DocumentDocumentType();
            document.Document = _context.Document.Find(person.DocumentID);
            if (document.Document == null)
            {
                return null;
            }
            // Substitute the code with the correspondent document description
            document.DocumentType = _context.TipoDocumento.FirstOrDefault(x => x.Codice.Equals(document.Document.DocumentType)).Descrizione;
            return document;
        }

        private Booking getBookingDetails(string bookingID)
        {
            var booking = _context.Booking.Find(Convert.ToInt32(bookingID));
            return booking;
        }

        private Room getRoomDetails(int? roomID)
        {
            var room = _context.Room.Find(roomID);
            return room;
        }

        private BookChannel getChannelDetailsS(string? channelId)
        {
            var channel = _context.BookChannel.Find(Convert.ToInt32(channelId));
            return channel;
        }

        private Configuration getConfiguration()
        {
            var configurations = _context.Configuration.FirstOrDefault(); // only one configuration exist
            return configurations;
        }
        private BookChannel getChannelDetails(int? channelId)
        {
            var channel = _context.BookChannel.Find(channelId);
            return channel;
        }

        private string getBirthPlaceDescription(Person p)
        {
            if (p.BirthCountry.Equals("100000100"))
            { // If birthcountry is Italy
                return _context.Comuni.Find(p.BirthPlace).Descrizione;
            }
            else
            {
                return "Estero";
            }
        }

        private async Task updateIsContractPrintedAsync(Booking booking)
        {
            booking.ContractPrinted = true;
            _context.Update(booking);
            await _context.SaveChangesAsync();
        }
        #endregion
    }
}