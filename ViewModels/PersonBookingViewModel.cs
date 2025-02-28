using MAppBnB;
using SignalRChat.Hubs;

public class PersonBookingViewModel
{
    public string ?PersonIDs { get; set; }
    public Booking ?Booking { get; set; }

    public Person[]? PeopleInBooking{get;set;}

}