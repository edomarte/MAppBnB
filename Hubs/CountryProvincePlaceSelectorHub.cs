using MAppBnB.Data;
using Microsoft.AspNetCore.SignalR;


namespace SignalRChat.Hubs
{
    public class CountryProvincePlaceSelectorHub : Hub
    {

        private readonly MappBnBContext _context;

        public CountryProvincePlaceSelectorHub(MappBnBContext context)
        {
            _context = context;
        }

        public async Task GetProvinces()
        {
            var provinces = _context.Province.ToList();
            await Clients.All.SendAsync("ProvinceList", provinces);
        }

        public async Task GetTowns(string province)
        {
            var towns = _context.Comuni.Where(x => x.Provincia.Equals(province)).ToList();
            await Clients.All.SendAsync("TownsList", towns);
        }

        public async Task GetTown(string codice)
        {
            var town = _context.Comuni.FirstOrDefault(x => x.Codice.Equals(codice));
            await Clients.All.SendAsync("Town", town);
        }
    }

}
