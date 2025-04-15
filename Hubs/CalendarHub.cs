using MAppBnB;
using MAppBnB.Data;
using Microsoft.AspNetCore.SignalR;


namespace SignalRChat.Hubs
{
    // Class managing the SignalR hub for the calendar.
    public class CalendarHub : Hub
    {

        private readonly MappBnBContext _context;

        public CalendarHub(MappBnBContext context)
        {
            _context = context;
        }

        #region Hub Method

        // Method to get bookings for a specific accommodation in a given month and year.
        public async Task GetBookingsInMonth(string accommodationId, string monthYear)
        {
            // Check if accommodationId is empty and send null to all clients if it is.
            if (accommodationId.Equals(""))
            {
                await Clients.All.SendAsync("UpdateCalendar", null);
                return;
            }

            // Parse the monthYear string to extract year and month.
            int selectedYear = int.Parse(monthYear.Split('-')[0]);  // Extract year from "yyyy-MM"
            int selectedMonth = int.Parse(monthYear.Split('-')[1]); // Extract month from "yyyy-MM"

            // Get bookings for the specified accommodation and month/year. All the accommodations that start or end in that month/year must be showed.
            List<Booking> bookings = _context.Booking.Where(b => b.AccommodationID == Convert.ToInt32(accommodationId)
            && ((b.CheckinDateTime.Year == selectedYear
            && b.CheckinDateTime.Month == selectedMonth)
            || (b.CheckOutDateTime.Year == selectedYear
            && b.CheckOutDateTime.Month == selectedMonth))).ToList();

            // If no bookings are found, send null to all clients.
            await Clients.All.SendAsync("UpdateCalendar", getCalendarUpdateElements(bookings));
        }

        #endregion

        #region Helper Methods

        // Method to get a list of CalendarUpdateElement objects from a list of bookings.
        private List<CalendarUpdateElement> getCalendarUpdateElements(List<Booking> bookings)
        {
            List<CalendarUpdateElement> lcue = new List<CalendarUpdateElement>();

            // For each booking, create a CalendarUpdateElement object and add it to the list.
            foreach (Booking b in bookings)
            {
                // Booking id, check-in and check-out dates, room, and main person are used to create the CalendarUpdateElement object.
                lcue.Add(new CalendarUpdateElement(b.id, b.CheckinDateTime.DayOfYear, b.CheckOutDateTime.DayOfYear, getRoom(b), getMainPerson(b)));
            }
            return lcue;
        }

        // Method to get the main person associated with a booking.
        private Person getMainPerson(Booking booking)
        {
            int bookingId = booking.id;
            // Get the main person associated with the booking by joining BookingPerson and Person tables.
            // The main person is determined by the RoleRelation property, which should be between 16 and 18.
            Person mainPerson = _context.BookingPerson
    .Join(_context.Person,
          bp => bp.PersonID,
          p => p.id,
          (bp, p) => new { bp, p })
    .Where(joined => bookingId == joined.bp.BookingID &&
                     joined.p.RoleRelation >= 16 &&
                     joined.p.RoleRelation <= 18)
    .Select(joined => joined.p)  // Selects the Person entity
    .ToList()[0]; // Get the first element of the list (it should be only one)

            return mainPerson;
        }

        // Method to get the room associated with a booking.
        private Room getRoom(Booking booking)
        {
            int roomId = booking.RoomID;
            Room room = _context.Room.FirstOrDefault(x => x.id == roomId);

            return room;
        }
        #endregion
    }



}