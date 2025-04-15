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

        // This method is called from the client-side to get the list of rooms for a specific accommodation
        public async Task RoomSelector(string AccommodationID)
        {
            // If accommodationID is not null or empty, fetch the rooms from the database and send them to all connected clients.
            if (AccommodationID != null && !AccommodationID.Equals(""))
            {
                var rooms = _context.Room.Where(x => x.AccommodationId == int.Parse(AccommodationID)).ToList();
                await Clients.All.SendAsync("RoomsList", rooms);
            }
        }
    }
}
