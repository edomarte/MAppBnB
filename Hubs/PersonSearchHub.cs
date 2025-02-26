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

        public async Task SearchPerson(string personName)
        {
            var persons = _context.Person.Where(p => p.Name.Contains(personName)).ToList();
            await Clients.All.SendAsync("ResultList", persons);
        }
    }
}