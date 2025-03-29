using System.Diagnostics.CodeAnalysis;
using MAppBnB;
using MAppBnB.Controllers;
using MAppBnB.Data;
using Microsoft.AspNetCore.SignalR;


namespace SignalRChat.Hubs
{
    public class CalendarHub : Hub
    {

        private readonly MappBnBContext _context;

        public CalendarHub(MappBnBContext context)
        {
            _context = context;
        }

        private List<CalendarUpdateElement> getCalendarUpdateElements(List<Booking> bookings){
            List<CalendarUpdateElement> lcue= new List<CalendarUpdateElement>();

            foreach(Booking b in bookings){
                lcue.Add(new CalendarUpdateElement(b.id, b.CheckinDateTime.DayOfYear,b.CheckOutDateTime.DayOfYear, GetRoom(b), GetMainPerson(b)));
            }
            return lcue;
        }

         public Person GetMainPerson(Booking booking)
        {
            int bookingId = booking.id; //Just the first one for each booking
            Person mainPerson = _context.BookingPerson
    .Join(_context.Person,
          bp => bp.PersonID,
          p => p.id,
          (bp, p) => new { bp, p })
    .Where(joined => bookingId==joined.bp.BookingID &&
                     joined.p.RoleRelation >= 16 &&
                     joined.p.RoleRelation <= 18)
    .Select(joined => joined.p)  // Selects the Person entity
    .ToList()[0]; // Change Host in tipoalloggiato in a number

            return mainPerson;
        }

        public Room GetRoom(Booking booking)
        {
            int roomId = booking.RoomID;
            Room room = _context.Room.FirstOrDefault(x=>x.id==roomId);

            return room;
        }


        public async Task GetBookingsInMonth(string accommodationId, string monthYear)
        {
            if(accommodationId.Equals("")){
                await Clients.All.SendAsync("UpdateCalendar", null);
                return;
            }
            
            int selectedYear = int.Parse(monthYear.Split('-')[0]);  // Extract year from "yyyy-MM"
            int selectedMonth = int.Parse(monthYear.Split('-')[1]); // Extract month from "yyyy-MM"

            List<Booking> bookings = _context.Booking.Where(b => b.AccommodationID == Convert.ToInt32(accommodationId)
            && ((b.CheckinDateTime.Year == selectedYear
            && b.CheckinDateTime.Month == selectedMonth)
            || (b.CheckOutDateTime.Year == selectedYear
            && b.CheckOutDateTime.Month == selectedMonth))).ToList();


            await Clients.All.SendAsync("UpdateCalendar", getCalendarUpdateElements(bookings));
        }
    }

}