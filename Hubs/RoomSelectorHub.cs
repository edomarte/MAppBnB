using System.Diagnostics.CodeAnalysis;
using MAppBnB;
using MAppBnB.Controllers;
using MAppBnB.Data;
using Microsoft.AspNetCore.SignalR;


namespace SignalRChat.Hubs
{
    public class RoomSelectorHub : Hub
    {

        private readonly MappBnBContext _context;

        public RoomSelectorHub(MappBnBContext context)
        {
            _context = context;
        }

        public async Task RoomSelector(string AccommodationID)
        {
            if (!AccommodationID.Equals("") && AccommodationID != null)
            {
                var rooms = _context.Room.Where(x => x.AccommodationId == int.Parse(AccommodationID)).ToList();
                await Clients.All.SendAsync("RoomsList", rooms);
            }
        }
    }

}
