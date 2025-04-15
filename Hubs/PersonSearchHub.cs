using MAppBnB;
using MAppBnB.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;


namespace SignalRChat.Hubs
{
    public class PersonSearchHub : Hub
    {

        private readonly MappBnBContext _context;

        public PersonSearchHub(MappBnBContext context)
        {
            _context = context;
        }

        #region SignalR Methods
        
        // Method called by the client to search for persons given the surname.
        public async Task SearchPerson(string personSurname, string[] personsInBooking)
        {
            // Get the persons already in the booking
            int[] personsInBookingInt = personsInBooking.Select(int.Parse).ToArray();
            // Get all the persons from the database which surname contains the searched one.
            var persons = _context.Person.Where(p => p.Surname.Contains(personSurname)).ToList();
            // Get the list of people to be excluded because already in the booking.
            var excluded = persons.Where(p => personsInBookingInt.Contains(p.id)).ToList();
            // The host must not be searchable for adding it into a booking.
            var host = _context.Person.FirstOrDefault(x => x.RoleRelation == 99);
            // Add the host to the excluded list.
            excluded.Add(host);
            // Remove the persons to be excluded from the list. The comparer allows to compare different persons via their id or hashcode.
            persons = persons.Except(excluded, new PersonComparer()).ToList();

            // Send to the clients the personRoleName list.
            await Clients.All.SendAsync("ResultList", createPersonRoleNamesList(persons));
        }

        // Method called by the client to check if a person is already in a booking given personid, date from, and date to.
        public string[] IsPersonAlreadyInBooking(string personId, string dateFrom, string dateTo)
        {
            // Get the list bookings between datefrom and dateto where the person is in.
            List<Booking> contemporaryBookingFinded = isPersonInContemporaryBooking(personId, dateFrom, dateTo);

            // If there is at least one contemporary booking, return the name
            if (contemporaryBookingFinded.Count > 0)
            {
                // Get the person details
                Person finded = getPersonDetails(personId);
                // Return the message to the client with name, surname, and the first contemporary booking.
                return [finded.Name + " " + finded.Surname, contemporaryBookingFinded[0].id.ToString()];
            }
            // If no contemporary booking is found, return null.
            return null;
        }

        #endregion

        #region Helper Methods

        /// This method creates a list of PersonRoleNames objects from a list of Person objects.
        private List<PersonRoleNames> createPersonRoleNamesList(List<Person> personsL)
        {
            List<PersonRoleNames> lprn = new List<PersonRoleNames>();
            // Loop through each person in the list and create a PersonRoleNames object. RoleName comes from finding the RoleRelation code in the TipoAlloggiato table.
            foreach (Person person in personsL)
            {
                lprn.Add(new PersonRoleNames() { Person = person, RoleName = _context.TipoAlloggiato.FirstOrDefault(x => x.Codice == person.RoleRelation).Descrizione });
            }
            return lprn;
        }

        // This method checks if a person is already in a booking.
        private List<Booking> isPersonInContemporaryBooking(string personId, string dateFrom, string dateTo)
        {
            // Search for the person's ID in the BookingPerson junction table.
            List<int> searchedPerson = _context.BookingPerson.Where(x => x.PersonID == Convert.ToInt32(personId)).Select(x => x.BookingID).ToList();
            List<Booking> contemporaryBookingFinded;
            // If the person is found in the BookingPerson table, check if they have a booking within the specified date range.
            if (searchedPerson != null)
            {
                // Check the bookings where the person is found. Check if the checkin date time of these bookings is greater or equal to the dateFrom and less than or equal to the dateTo.
                contemporaryBookingFinded = _context.Booking.Where(x => searchedPerson.Contains(x.id)
                                              && x.CheckinDateTime >= Convert.ToDateTime(dateFrom)
                                        && x.CheckinDateTime <= Convert.ToDateTime(dateTo)).ToList();
                return contemporaryBookingFinded;
            }
            // If the person is not found in the BookingPerson table, return an empty list.
            return null;
        }

        // This method retrieves the details of a person based on their ID.
        private Person getPersonDetails(string personID)
        {
            var person = _context.Person.Find(int.Parse(personID));
            return person;
        }

        #endregion
    }
}
