using System.ComponentModel.DataAnnotations;

namespace MAppBnB;

public class Room{
    public int id{get; set;}

    [StringLength(30,MinimumLength =1)]
    [Required]
    public string ?Name{get;set;}
    [Required]
    [Range(1,100)]
    public int ?Capacity{get;set;}
    [Required]
    [Display(Name = "Accommodation")]
    public int ?AccommodationId{get;set;}
    [Required]
    [DataType(DataType.Currency)]
    public int ?BasicPrice{get;set;}
}