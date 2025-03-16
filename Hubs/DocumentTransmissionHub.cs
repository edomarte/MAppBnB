using MAppBnB;
using MAppBnB.Data;
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

        public Person GetPersonDetails(string personID)
        {
            var person = _context.Person.Where(x => x.id == int.Parse(personID)).ToList();
            return person[0];
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

        public List<Person> GetPersonsDetails(string[] personsID)
        {
            int[]personsIDInt=Array.ConvertAll(personsID, s => int.Parse(s));


            var persons = _context.Person.Where(x => personsIDInt.Contains(x.id)).ToList();
            return persons;
        }

                public async Task SendToTown(string bookingID, string personID)
        {
            var booking = _context.Booking.Where(x => x.id == int.Parse(bookingID)).ToList();
            Person mainPerson=GetPersonDetails(personID);
            EmailTransmission.SendDocsToTown(GetBookingDetails(bookingID),mainPerson,GetDocumentDetails(mainPerson));
            await Clients.All.SendAsync("TransmissionResult", "");
        }

        public async Task SendToRegionPolice(string bookingID, string[] personID)
        {
            var booking = _context.Booking.Where(x => x.id == int.Parse(bookingID)).ToList();
            List<Person> persons=GetPersonsDetails(personID);
            SOAPTransmission.SendDocsToRegionPolice(GetBookingDetails(bookingID),persons,GetDocumentDetails(persons.First()));
            await Clients.All.SendAsync("TransmissionResult", ""); //TODO: transmission message
        }
    }
}
