using MAppBnB.Data;
using Microsoft.AspNetCore.SignalR;


namespace SignalRChat.Hubs
{

    // Class managing the SignalR hub for the calendar.
    public class CountryProvincePlaceSelectorHub : Hub
    {
        private readonly MappBnBContext _context;

        public CountryProvincePlaceSelectorHub(MappBnBContext context)
        {
            _context = context;
        }

        // Method to get the list of provinces and send it to all connected clients.
        public async Task GetProvinces()
        {
            var provinces = _context.Province.ToList();
            await Clients.All.SendAsync("ProvinceList", provinces);
        }

        // Method to get the list of towns given a province and send it to all connected clients.
        public async Task GetTowns(string province)
        {
            // Towns ordered by description and filtered by province.
            var towns = _context.Comuni.Where(x => x.Provincia.Equals(province)).OrderBy(x => x.Descrizione).ToList();
            await Clients.All.SendAsync("TownsList", towns);
        }

        // Method to get the list of all towns and send it to all connected clients.
        public async Task GetAllTowns()
        {
            // Towns ordered by description
            var towns = _context.Comuni.OrderBy(x => x.Descrizione).ToList();
            await Clients.All.SendAsync("AllTownsList", towns);
        }

        // Method to get a town given its codice (id) and send it to all connected clients.
        public async Task GetTown(string codice)
        {
            var town = _context.Comuni.FirstOrDefault(x => x.Codice.Equals(codice));
            await Clients.All.SendAsync("Town", town);
        }
    }

}
