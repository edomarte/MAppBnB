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
                lcue.Add(new CalendarUpdateElement(b.id, b.CheckinDateTime.Value.DayOfYear,b.CheckOutDateTime.Value.DayOfYear, GetRoom(b), GetMainPerson(b)));
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
                     Convert.ToInt32(joined.p.RoleRelation) >= 16 &&
                     Convert.ToInt32(joined.p.RoleRelation) <= 18)
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
            && ((b.CheckinDateTime.Value.Year == selectedYear
            && b.CheckinDateTime.Value.Month == selectedMonth)
            || (b.CheckOutDateTime.Value.Year == selectedYear
            && b.CheckOutDateTime.Value.Month == selectedMonth))).ToList();


            await Clients.All.SendAsync("UpdateCalendar", getCalendarUpdateElements(bookings));
        }
    }

}

public class CalendarUpdateElement {
    public int Id {get;set;}
    public int CheckinDay {get;set;}
    public int CheckoutDay{get;set;}
    public Room Room{get;set;}
    public Person MainPerson{get;set;}

    public CalendarUpdateElement(int id, int checkinDay, int checkoutDay, Room room, Person mainPerson)
    {
        Id = id;
        CheckinDay = checkinDay;
        CheckoutDay = checkoutDay;
        Room=room;
        MainPerson = mainPerson;
    }
}