using System.ComponentModel.DataAnnotations;

namespace MAppBnB;

public class BookingPerson{
    public int id{get; set;}

    [Required]
    public int ?BookingID{get;set;}
    [Required]
    public int ?PersonID{get;set;}
    
}