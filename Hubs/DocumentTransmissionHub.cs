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
            var accommodation = _context.Accommodation.Where(x => x.id == accommodationId).ToList();
            return accommodation[0].AWIDAppartamento;
        }

        private Document GetDocumentDetails(Person person)
        {
            var document = _context.Document.Where(x => x.id == person.DocumentID).ToList();
            return document[0];
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

        public async Task SendToTown(string bookingID, string personID)
        {
            var booking = _context.Booking.Where(x => x.id == int.Parse(bookingID)).ToList();
            Person mainPerson = GetPersonDetails(personID);
            string transmissionResult=EmailTransmission.SendDocsToTown(GetBookingDetails(bookingID), mainPerson, GetDocumentDetails(mainPerson));
            await Clients.All.SendAsync("TransmissionResult", transmissionResult);
        }

        public async Task SendToRegionPolice(string bookingID, string[] personID)
        {
            var booking = _context.Booking.Where(x => x.id == int.Parse(bookingID)).ToList();
            List<Person> persons = GetPersonsDetails(personID);
            string transmissionResult=SOAPTransmission.SendDocsToRegionPolice(GetBookingDetails(bookingID), persons, GetDocumentDetails(persons.First()), GetConfiguration(),GetAWIDAppartamento(booking[0].AccommodationID));
            await Clients.All.SendAsync("TransmissionResult", transmissionResult); //TODO: transmission message
        }
    }
}
