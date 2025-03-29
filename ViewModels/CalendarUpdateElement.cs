using MAppBnB;

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