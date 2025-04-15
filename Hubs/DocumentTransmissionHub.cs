using MAppBnB;
using MAppBnB.Data;
using MAppBnB.Models;
using Microsoft.AspNetCore.SignalR;


namespace SignalRChat.Hubs
{
    public class DocumentTransmissionHub : Hub
    {

        private readonly MappBnBContext _context;

        public DocumentTransmissionHub(MappBnBContext context)
        {
            _context = context;
        }


        #region SignalR Methods

        // Method called by the client to send the contract to a guest.
        public async Task SendContract(string bookingID, string personID)
        {
            // Get main person and booking details given the parameters.
            Person mainPerson = getPersonDetails(personID);
            Booking booking = getBookingDetails(bookingID);

            // Get the contract path combining the current directory with DocumentTemplates, Contract, the booking id, and .pdf.
            // Path.Combine ensure the path compatibility regardless of the operating system.
            string contractPath = Path.Combine(Directory.GetCurrentDirectory(), "DocumentTemplates", "Contract" + bookingID + ".pdf");


            if (File.Exists(contractPath))
            {
                try
                {
                    // If the PDF contract exists, read all bytes from the file.
                    byte[] contractFile = await File.ReadAllBytesAsync(contractPath);
                    // Call the method for transmitting the email passing the appropriate parameters.
                    string transmissionResult = EmailTransmission.SendContract(booking, mainPerson, getAccommodation(booking.AccommodationID).Name, getRoom(booking.RoomID).Name, contractFile);
                    // Update the flag on the view.
                    await updateIsContractSentAsync(booking);
                    // Send the transmission result to the clients.
                    await Clients.All.SendAsync("TransmissionResult", transmissionResult);
                }
                catch (Exception e)
                {
                    // If any issues, send the exception message to the client.
                    await Clients.All.SendAsync("TransmissionResult", e.Message);
                }
            }
            else
            {
                // If the PDF contract does not exist, return the error message.
                await Clients.All.SendAsync("TransmissionResult", "Generate a Contract PDF first!");
            }
        }

        // Method called by the client to send the pre-check-in document to a guest.
        public async Task SendPreCheckIn(string bookingID, string personID)
        {
            // Get main person and booking details given the parameters.
            Person mainPerson = getPersonDetails(personID);
            Booking booking = getBookingDetails(bookingID);

            // Get the contract path combining the current directory with DocumentTemplates, Contract, the booking id, and .pdf.
            // Path.Combine ensure the path compatibility regardless of the operating system.
            string preCheckInPath = Path.Combine(Directory.GetCurrentDirectory(), "DocumentTemplates", "Pre-Checkin" + bookingID + ".pdf");

            if (File.Exists(preCheckInPath))
            {
                try
                {
                    // If the PDF document exists, read all bytes from the file.
                    byte[] preCheckInFile = await File.ReadAllBytesAsync(preCheckInPath);
                    // Call the method for transmitting the email passing the appropriate parameters.
                    string transmissionResult = EmailTransmission.SendPreCheckIn(booking, mainPerson, getAccommodation(booking.AccommodationID).Name, getRoom(booking.RoomID).Name, preCheckInFile);
                    // Update the flag on the view.
                    await updateIsPreCheckinSentAsync(booking);
                    // Send the transmission result to the clients.
                    await Clients.All.SendAsync("TransmissionResult", transmissionResult);
                }
                catch (Exception e)
                {
                    // If any issues, send the exception message to the client.
                    await Clients.All.SendAsync("TransmissionResult", e.Message);
                }
            }
            else
            {
                // If the PDF contract does not exist, return the error message.
                await Clients.All.SendAsync("TransmissionResult", "Generate a Pre-CheckIn PDF first!");
            }
        }

        // Method called by the client to send the information to the police webservice.
        public async Task SendToRegionPolice(string bookingID, string[] personID)
        {
            // Get list of persons in the booking, the configuration, and booking details given the parameters.
            Booking booking = getBookingDetails(bookingID);

            // If the flag Sent2Police is true, return the message that the data has already been sent to the police and it cannot be sent twice..
            if(booking.Sent2Police==true){
                await Clients.All.SendAsync("TransmissionResult", "The data has been already sent to the police. It cannot be sent twice.");
            }

            List<Person> persons = getPersonsDetails(personID);
            Configuration configuration = getConfiguration();

            // If configuration does not exist or some required fields are null, return error message.
            if (configuration is null || configuration.AlloggiatiWebUsername == null || configuration.AlloggiatiWebPassword == null || configuration.AlloggiatiWebWSKey == null)
            {
                await Clients.All.SendAsync("TransmissionResult", "Alloggiati Web fields in Configuration not set. Please check the configuration.");
                return;
            }

            try
            {
                // Get AlloggiatiWeb ID Appartamento from accommodation.
                string AWIDAppartamento = getAWIDAppartamento(booking.AccommodationID);
                // If the host has multiple apartments and the AW ID appartamento is null, return error message.
                if (configuration.IsGestioneAppartamenti && AWIDAppartamento is null)
                    await Clients.All.SendAsync("TransmissionResult", "Missing AW IDAppartamento for AlloggiatiWeb. Please check the accommodation details.");

                // Send the required information to the method that send the data to the police webservice.
                string transmissionResult = SOAPTransmission.SendDocsToPolice(booking, persons, getDocumentDetails(persons.First()), configuration, AWIDAppartamento);
                // Update the flag on the view.
                await updateSent2PoliceAsync(booking);
                // Send the transmission result to the clients.
                await Clients.All.SendAsync("TransmissionResult", transmissionResult);
            }
            catch (Exception e)
            {
                // If any issues, send the exception message to the client.
                await Clients.All.SendAsync("TransmissionResult", e.Message);
            }
        }
        #endregion

        #region  Helper Methods

        // Get person details given the id.
        private Person getPersonDetails(string personID)
        {
            var person = _context.Person.Find(int.Parse(personID));
            return person;
        }

        // Get the Alloggiati Web ID Appartamento from the accommodation given the id.
        private string getAWIDAppartamento(int accommodationId)
        {
            var accommodation = _context.Accommodation.FirstOrDefault(x => x.id == accommodationId);
            return accommodation.AWIDAppartamento;
        }

        // Get the document details given the person.
        private Document getDocumentDetails(Person person)
        {
            var document = _context.Document.Find(person.DocumentID);
            return document;
        }

        // Get the booking details given the booking id.
        private Booking getBookingDetails(string bookingID)
        {
            var booking = _context.Booking.Find(int.Parse(bookingID));
            return booking;
        }

        // Get persons details given an array of person ids.
        private List<Person> getPersonsDetails(string[] personsID)
        {
            // Convert all the elements in the string array to int.
            int[] personsIDInt = Array.ConvertAll(personsID, s => int.Parse(s));
            // Get all the persons which id is contained in the int array of ids.
            var persons = _context.Person.Where(x => personsIDInt.Contains(x.id)).ToList();
            // Return the array.
            return persons;
        }

        // Get the configuration (there is only one line in the table).
        private Configuration getConfiguration()
        {
            var configuration = _context.Configuration.FirstOrDefault();
            return configuration;
        }

        // Get the room details given the id.
        public Room getRoom(int roomID)
        {
            Room room = _context.Room.Find(roomID);

            return room;
        }

        // Get the accommodation details given the id.
        private Accommodation getAccommodation(int accommodationID)
        {
            var details = _context.Accommodation.Find(accommodationID);
            return details;
        }

        // Update the isContractSent flag in the booking element and save it to the database.
        private async Task updateIsContractSentAsync(Booking booking)
        {
            booking.ContractSent = true;
            _context.Update(booking);
            await _context.SaveChangesAsync();
        }

        // Update the isPreCheckinSent flag in the booking element and save it to the database.
        private async Task updateIsPreCheckinSentAsync(Booking booking)
        {
            booking.PreCheckinSent = true;
            _context.Update(booking);
            await _context.SaveChangesAsync();
        }

        // Update the Sent2Police flag in the booking element and save it to the database.
        private async Task updateSent2PoliceAsync(Booking booking)
        {
            booking.Sent2Police = true;

            _context.Update(booking);
            await _context.SaveChangesAsync();
        }
        #endregion
    }
}
