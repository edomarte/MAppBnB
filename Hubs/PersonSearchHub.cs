using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using MAppBnB;
using MAppBnB.Controllers;
using MAppBnB.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PdfSharp.Snippets;


namespace SignalRChat.Hubs
{
    public class PersonSearchHub : Hub
    {

        private readonly MappBnBContext _context;

        public PersonSearchHub(MappBnBContext context)
        {
            _context = context;
        }

        public async Task SearchPerson(string personName, string[] personsInBooking)
        {
            int[] personsInBookingInt = personsInBooking.Select(int.Parse).ToArray();
            var persons = _context.Person.Where(p => p.Surname.Contains(personName));
            var excluded = persons.Where(p => personsInBookingInt.Contains(p.id));
            var personsL = persons.ToList();
            var excludedL = excluded.ToList();
            personsL = personsL.Except(excludedL, new PersonComparer()).ToList();

            await Clients.All.SendAsync("ResultList", createPersonRoleNamesList(personsL));
        }

        public List<PersonRoleNames> createPersonRoleNamesList(List<Person> personsL)
        {
            List<PersonRoleNames> lprn = new List<PersonRoleNames>();
            foreach (Person person in personsL)
            {
                lprn.Add(new PersonRoleNames() { Person = person, RoleName = _context.TipoAlloggiato.FirstOrDefault(x => x.Codice == person.RoleRelation).Descrizione });
            }
            return lprn;
        }

        private List<Booking> isPersonInContemporaryBooking(string personId, string dateFrom, string dateTo){
            List<int> searchedPerson= _context.BookingPerson.Where(x=>x.PersonID== Convert.ToInt32(personId)).Select(x => x.BookingID).ToList();
            List<Booking> contemporaryBookingFinded;
            if(searchedPerson!=null){
                contemporaryBookingFinded=_context.Booking.Where(x=>searchedPerson.Contains(x.id)
                                              && x.CheckinDateTime >= Convert.ToDateTime(dateFrom)
                                        && x.CheckinDateTime <= Convert.ToDateTime(dateTo)).ToList();   
                return contemporaryBookingFinded;
            }
                

            return null;
        }

        public string[] IsPersonAlreadyInBooking(string personId, string dateFrom, string dateTo){
            List<Booking> contemporaryBookingFinded=isPersonInContemporaryBooking(personId, dateFrom, dateTo);
            if(contemporaryBookingFinded.Count>0){
                Person finded=GetPersonDetails(personId);
                return [finded.Name+" "+finded.Surname,contemporaryBookingFinded[0].id.ToString()];
            }
            return null;
        }
        public Person GetPersonDetails(string personID)
        {
            var person = _context.Person.Find(int.Parse(personID));
            return person;
        }
    }
}
