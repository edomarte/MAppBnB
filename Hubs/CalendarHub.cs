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
                lcue.Add(new CalendarUpdateElement(b.id, b.CheckinDateTime.Value.DayOfYear,b.CheckOutDateTime.Value.DayOfYear));
            }
            return lcue;
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

    public CalendarUpdateElement(int id, int checkinDay, int checkoutDay)
    {
        this.Id = id;
        this.CheckinDay = checkinDay;
        this.CheckoutDay = checkoutDay;
    }
}