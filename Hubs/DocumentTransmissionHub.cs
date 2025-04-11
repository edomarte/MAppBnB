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

        private Person GetPersonDetails(string personID)
        {
            var person = _context.Person.Where(x => x.id == int.Parse(personID)).ToList();
            return person[0];
        }
        private string GetAWIDAppartamento(int accommodationId)
        {
            var accommodation = _context.Accommodation.FirstOrDefault(x => x.id == accommodationId);
            return accommodation.AWIDAppartamento;
        }

        private Document GetDocumentDetails(Person person)
        {
            var document = _context.Document.Find(person.DocumentID);
            return document;
        }

        private Booking GetBookingDetails(string bookingID)
        {
            var booking = _context.Booking.Where(x => x.id == int.Parse(bookingID)).ToList();
            return booking[0];
        }

        private List<Person> GetPersonsDetails(string[] personsID)
        {
            int[] personsIDInt = Array.ConvertAll(personsID, s => int.Parse(s));


            var persons = _context.Person.Where(x => personsIDInt.Contains(x.id)).ToList();
            return persons;
        }

        private Configuration GetConfiguration()
        {
            var configuration = _context.Configuration.ToList();
            return configuration[0];
        }

        public Room GetRoom(int roomID)
        {
            Room roomsList = _context.Room.Find(roomID);

            return roomsList;
        }
        public Accommodation GetAccommodation(int accommodationID)
        {
            var details = _context.Accommodation.Find(accommodationID);
            return details;
        }

        private async Task updateIsContractSentAsync(Booking booking)
        {
            booking.ContractSent = true;
            _context.Update(booking);
            await _context.SaveChangesAsync();
        }

        private async Task updateIsPreCheckinSentAsync(Booking booking)
        {
            booking.PreCheckinSent = true;
            _context.Update(booking);
            await _context.SaveChangesAsync();
        }

        private async Task updateIsSentToPoliceAsync(Booking booking)
        {
            booking.Sent2Police = true;

            _context.Update(booking);
            await _context.SaveChangesAsync();
        }

        public async Task SendContract(string bookingID, string personID)
        {
            Person mainPerson = GetPersonDetails(personID);
            Booking booking = GetBookingDetails(bookingID);

            string contractPath = "..\\MAppBnB\\DocumentTemplates\\Contract" + bookingID + ".pdf";
            //string base64String = Convert.ToBase64String(contract);

            if (File.Exists(contractPath))
            {
                try
                {
                    byte[] contractFile = await File.ReadAllBytesAsync(contractPath);
                    string transmissionResult = EmailTransmission.SendContract(booking, mainPerson, GetAccommodation(booking.AccommodationID).Name, GetRoom(booking.RoomID).Name, contractFile);
                    await updateIsContractSentAsync(booking);
                    await Clients.All.SendAsync("TransmissionResult", transmissionResult);
                }
                catch (Exception e)
                {
                    await Clients.All.SendAsync("TransmissionResult", e.Message);
                }
            }
            else
            {
                await Clients.All.SendAsync("TransmissionResult", "Generate a Contract PDF first!");
            }
        }

        public async Task SendPreCheckIn(string bookingID, string personID)
        {
            Person mainPerson = GetPersonDetails(personID);
            Booking booking = GetBookingDetails(bookingID);

            string contractPath = "..\\MAppBnB\\DocumentTemplates\\Pre-Checkin" + bookingID + ".pdf";
            //string base64String = Convert.ToBase64String(contract);

            if (File.Exists(contractPath))
            {
                try
                {
                    byte[] contractFile = await File.ReadAllBytesAsync(contractPath);
                    string transmissionResult = EmailTransmission.SendPreCheckIn(booking, mainPerson, GetAccommodation(booking.AccommodationID).Name, GetRoom(booking.RoomID).Name, contractFile);
                    await updateIsPreCheckinSentAsync(booking);
                    await Clients.All.SendAsync("TransmissionResult", transmissionResult);
                }
                catch (Exception e)
                {
                    await Clients.All.SendAsync("TransmissionResult", e.Message);
                }
            }
            else
            {
                await Clients.All.SendAsync("TransmissionResult", "Generate a Pre-CheckIn PDF first!");
            }
        }

        public async Task SendToRegionPolice(string bookingID, string[] personID)
        {
            var booking = GetBookingDetails(bookingID);
            List<Person> persons = GetPersonsDetails(personID);
            Configuration configuration = GetConfiguration();

            if (configuration is null || configuration.AlloggiatiWebUsername == null || configuration.AlloggiatiWebPassword == null || configuration.AlloggiatiWebWSKey == null)
            {
                await Clients.All.SendAsync("TransmissionResult", "Alloggiati Web fields in Configuration not set. Please check the configuration.");
                return;
            }
            try
            {
                string AWIDAppartamento = GetAWIDAppartamento(booking.AccommodationID);
                if (configuration.IsGestioneAppartamenti && AWIDAppartamento is null)
                    await Clients.All.SendAsync("TransmissionResult", "Missing AW IDAppartamento for AlloggiatiWeb. Please check the accommodation details.");

                string transmissionResult = SOAPTransmission.SendDocsToPolice(booking, persons, GetDocumentDetails(persons.First()), configuration, AWIDAppartamento);
                await updateIsSentToPoliceAsync(booking);
                await Clients.All.SendAsync("TransmissionResult", transmissionResult);
            }
            catch (Exception e)
            {
                await Clients.All.SendAsync("TransmissionResult", e.Message);
            }
        }
    }
}
