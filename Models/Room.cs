using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

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
    [Precision(18,2)]
    public decimal ?BasicPrice{get;set;}
}