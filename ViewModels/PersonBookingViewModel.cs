using MAppBnB;

public class PersonBookingViewModel
{
    public string ?PersonIDs { get; set; }
    public Booking ?Booking { get; set; }

    public List<PersonRoleNames>? PeopleInBooking{get;set;}

}