using System.Diagnostics.CodeAnalysis;
using MAppBnB;
using MAppBnB.Controllers;
using MAppBnB.Data;
using Microsoft.AspNetCore.SignalR;


namespace SignalRChat.Hubs
{
    public class PersonSearchHub : Hub
    {

        private readonly MappBnBContext _context;

        public PersonSearchHub(MappBnBContext context)
        {
            _context = context;
        }

        public async Task SearchPerson(string personName, String[]personsInBooking)
        {
            int[] personsInBookingInt = personsInBooking.Select(int.Parse).ToArray();
            //var persons = _context.Person.Where(p => p.Name.Contains(personName)).ToList();
            var persons = _context.Person.Where(p => p.Name.Contains(personName));
            var excluded= persons.Where(p=> personsInBookingInt.Contains(p.id));
            var personsL=persons.ToList();
            var excludedL=excluded.ToList();
            personsL=personsL.Except(excludedL, new PersonComparer()).ToList();
            
            await Clients.All.SendAsync("ResultList", personsL);
        }
    }
}
