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
        // Method invoked by the client to create a doc version of the contract
        public async Task CreateContract(string mainPersonID, string accommodationID, string bookingId)
        {
            try
            {
                // Get the main Person details and add it to this composite class object.
                PersonBirthPlace mainPerson = new PersonBirthPlace() { Person = getPersonDetails(mainPersonID) };
                // Get the birthPlace description for the main person.
                mainPerson.BirthPlace = getBirthPlaceDescription(mainPerson.Person);
                // Get the configuration.
                Configuration configuration = getConfiguration();
                // If the configuration is null or there is no person associated, return exception.
                if (configuration is null || configuration.PersonID is null || configuration.PersonID.ToString().Equals(""))
                {
                    throw new Exception("Host not set in configuration!");
                }
                // Get host details.
                Person host = getPersonDetails(_context.Configuration.FirstOrDefault().PersonID.ToString());
                // Get host document details.
                DocumentDocumentType hostDocument = getDocumentDetails(host);
                // If the host document is null, return an exception.
                if (hostDocument is null)
                {
                    throw new Exception("Host document not set in configuration!");
                }

                // Get accommodation details.
                Accommodation accommodation = getAccommodationDetails(accommodationID);
                // Get booking details.
                Booking booking = getBookingDetails(bookingId);
                // Generate the contract document and get its path.
                string contractPath = DocumentProcessing.GenerateContract(mainPerson, getDocumentDetails(mainPerson.Person), accommodation, booking, host, hostDocument);
                // Start file Download.
                await startFileDownloadAsync(contractPath);
                // Update the booking to set ContractPrinted to true.
                updateContractPrintedAsync(booking);
                // No delete file because it is needed to generate the PDF copy.
            }
            catch (Exception e)
            {
                // If any exceptions, send its message to the client.
                await Clients.All.SendAsync("Error", e.Message);
            }
        }

        // Method invoked by the client to create a doc version of the Booking Details document.
        public async Task CreateBookingDetails(string[] personsIDs, string accommodationID, string bookingId)
        {
            try
            {
                // Get details for all people in the list.
                List<Person> persons = getPersonsDetails(personsIDs);
                // Get accommodation details.
                Accommodation accommodation = getAccommodationDetails(accommodationID);
                // Get booking details.
                Booking booking = getBookingDetails(bookingId);
                // Generate the document and get its path.
                string bdPath = DocumentProcessing.GenerateBookingDetails(persons, booking, accommodation, getRoomDetails(booking.RoomID), getChannelDetails(booking.ChannelID));
                // Start file Download.
                await startFileDownloadAsync(bdPath);
                // Delete the document after download (it will have to be recreated anyways next time).
                DocumentProcessing.DeleteDocument(bdPath);
            }
            catch (Exception e)
            {
                // If any exceptions, send its message to the client.
                await Clients.All.SendAsync("Error", e.Message);
            }
        }

        // Method invoked by the client to create a doc version of the pre-checkin document.
        public async Task CreatePreCheckin(string mainPersonID, string accommodationID, string bookingId)
        {
            try
            {
                // Get the main Person details.
                Person mainPerson = getPersonDetails(mainPersonID);
                // Get accommodation details.
                Accommodation accommodation = getAccommodationDetails(accommodationID);
                // Generate the document and get its path.
                string preCheckinPath = DocumentProcessing.GeneratePreCheckIn(mainPerson, accommodation, getBookingDetails(bookingId));
                // Start file Download.
                await startFileDownloadAsync(preCheckinPath);
                // No delete file because it is needed to generate the PDF copy.
            }
            catch (Exception e)
            {
                // If any exceptions, send its message to the client.
                await Clients.All.SendAsync("Error", e.Message);
            }
        }

        // Method to create the PDF version of the doc contract.
        public async Task CreateContractPDF(string bookingId)
        {
            try
            {
                // Generate the document.
                createPDFCopy(bookingId, "Contract");
            }
            catch (Exception e)
            {
                await Clients.All.SendAsync("Error", e.Message);
            }
        }

        // Method to create the PDF version of the doc pre-checkin document.
        public async Task CreatePreCheckinPDF(string bookingId)
        {
            try
            {
                // Generate the document.
                createPDFCopy(bookingId, "Pre-Checkin");
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

        // Method for creating a PDF copy of an existing Microsoft word document (doc)
        private async void createPDFCopy(string bookingID, string documentType)
        {
            // Generate the document and get its path.
            string preCheckinPath = DocumentProcessing.GeneratePDFDocument(bookingID, documentType);
            // Start file Download.
            await startFileDownloadAsync(preCheckinPath);
            // Delete the document after download (it will have to be recreated anyways next time).
            DocumentProcessing.DeleteDocument(preCheckinPath); // Delete the document after download
        }

        // Method for sending to the client the file to be downloaded.
        private async Task startFileDownloadAsync(string filePath)
        {
            // Read the file from memory and store it in an array of byte.
            byte[] file = await File.ReadAllBytesAsync(filePath);
            // Convert the array of byte in a base64 string.
            string base64String = Convert.ToBase64String(file);
            // Send the filename and the file to the client.
            await Clients.All.SendAsync("DownloadFile", Path.GetFileName(filePath), base64String);
        }

        // Method for getting the report lines out of a list of bookings.
        private List<FinancialReportLine> getReportLines(List<Booking> bookings)
        {
            // Initialize the list.
            List<FinancialReportLine> lfrl = new List<FinancialReportLine>();

            foreach (Booking booking in bookings)
            {
                // For each booking, create and populate a financialReportLine with the appropriate data and add it to the list.
                lfrl.Add(new FinancialReportLine() { Booking = booking, MainPerson = getMainPerson(booking), Room = getRoom(booking), Channel = getChannelDetails(booking.ChannelID), GuestCount = getGuestCount(booking) });
            }

            // Return the list.
            return lfrl;
        }

        // Method for getting the accommodationDetails given the id.
        private Accommodation getAccommodationDetails(string accommodationID)
        {
            var details = _context.Accommodation.Find(Convert.ToInt32(accommodationID));
            return details;
        }

        // Method to get a person details given the person id.
        private Person getPersonDetails(string personID)
        {
            var person = _context.Person.Find(Convert.ToInt32(personID));
            return person;
        }

        // Method to get the main person details given a booking-
        private Person getMainPerson(Booking booking)
        {
            int bookingId = booking.id;
            // Get the main person by joining person with the BookingPerson junction table and get the main person of it (RoleRelation=16, 17, 18).
            Person mainPerson = _context.BookingPerson
    .Join(_context.Person,
          bp => bp.PersonID,
          p => p.id,
          (bp, p) => new { bp, p })
    .Where(joined => bookingId == joined.bp.BookingID &&
                     Convert.ToInt32(joined.p.RoleRelation) >= 16 &&
                     Convert.ToInt32(joined.p.RoleRelation) <= 18)
    .Select(joined => joined.p)  // Selects the Person entity
    .ToList()[0];

            return mainPerson;
        }

        // Get the guest count in a booking.
        private int getGuestCount(Booking booking)
        {
            int bookingId = booking.id;
            // Group the booking in Booking person junction table by booking id and count how many people are associated with that booking.
            int guestsPerBooking = _context.BookingPerson
                                        .Where(bp => bookingId == bp.BookingID)
                                        .GroupBy(bp => bp.BookingID)
                                        .Select(g => g.Count()).ToList()[0];
            return guestsPerBooking;
        }

        // Get the room of a booking.
        private Room getRoom(Booking booking)
        {
            int roomId = booking.RoomID;
            Room roomsList = _context.Room.Find(roomId);

            return roomsList;
        }

        // Get the details of all the persons in the array.
        private List<Person> getPersonsDetails(string[] personIDs)
        {
            // Convert the array of string in array of int.
            int[] iPersonIDs = Array.ConvertAll(personIDs, int.Parse);
            var persons = _context.Person.Where(x => iPersonIDs.Contains(x.id)).ToList();
            return persons;
        }

        // Get document details of a person
        private DocumentDocumentType getDocumentDetails(Person person)
        {
            DocumentDocumentType document = new DocumentDocumentType();
            document.Document = _context.Document.Find(person.DocumentID);
            if (document.Document == null)
            {
                return null;
            }
            // Get the document description correspondent with the code.
            document.DocumentType = _context.TipoDocumento.FirstOrDefault(x => x.Codice.Equals(document.Document.DocumentType)).Descrizione;
            return document;
        }

        // Get the booking details given the id.
        private Booking getBookingDetails(string bookingID)
        {
            var booking = _context.Booking.Find(Convert.ToInt32(bookingID));
            return booking;
        }

        // Get the room details given the id.
        private Room getRoomDetails(int? roomID)
        {
            var room = _context.Room.Find(roomID);
            return room;
        }

        // Get the booking channel details given a string id.
        private BookChannel getChannelDetailsS(string? channelId)
        {
            var channel = _context.BookChannel.Find(Convert.ToInt32(channelId));
            return channel;
        }

        // Get the configuration.
        private Configuration getConfiguration()
        {
            var configurations = _context.Configuration.FirstOrDefault(); // only one configuration exist
            return configurations;
        }

        // Get the booking channel details given an int id.
        private BookChannel getChannelDetails(int? channelId)
        {
            var channel = _context.BookChannel.Find(channelId);
            return channel;
        }

        // Get the birth place description of a person.
        private string getBirthPlaceDescription(Person p)
        {
            if (p.BirthCountry.Equals("100000100"))
            { // If birthcountry is Italy
                return _context.Comuni.Find(p.BirthPlace).Descrizione;
            }
            else
            {
                // Else return Foreign (Esteroo)
                return "Estero";
            }
        }

        // Update Contract Printed flag on the database given a booking.
        private async Task updateContractPrintedAsync(Booking booking)
        {
            booking.ContractPrinted = true;
            _context.Update(booking);
            await _context.SaveChangesAsync();
        }
        #endregion
    }
}