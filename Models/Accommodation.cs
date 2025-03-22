using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MAppBnB;

public class Accommodation
{
    public int id { get; set; }

    [StringLength(30, MinimumLength = 3)]
    [Required]
    [DataType(DataType.Text)]
    public string? Name { get; set; }
    [DataType(DataType.Text)]
    [Required]
    public string? Address { get; set; }
    [DataType(DataType.PostalCode)]
    [Required]
    public string? PostCode { get; set; }
    [DataType(DataType.Text)]
    [Required]
    public string? City { get; set; }
    [DataType(DataType.Text)]
    [Required]
    public string? Province { get; set; }
    [DataType(DataType.Text)]
    [Required]
    public string? Country { get; set; }
    [DataType(DataType.Text)]
    public string? Floor { get; set; }
    [DataType(DataType.Text)]
    public string? UnitApartment { get; set; }
    [DataType(DataType.Text)]
    [RegularExpression(@"^\+(?:[\d]{2,3})$")]
    public string? PhonePrefix { get; set; }

    //[RegularExpression(@"[\d]")]
    [DataType(DataType.PhoneNumber)]
    public string? PhoneNumber { get; set; }
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }
    [DataType(DataType.Text)]
    public string? Website { get; set; }
    [DataType(DataType.Text)]
    [Required]
    public string? CIN { get; set; }
    [DataType(DataType.Text)]
    [Required]
    public string? CIR { get; set; }
    [Required]
    [Precision(18, 2)]
    [DataType(DataType.Currency)]
    public decimal? CleaningFee { get; set; }
    [DataType(DataType.Currency)]
    [Required]
    [Precision(18, 2)]
    public decimal? TownFee { get; set; }
    [DataType(DataType.Text)]
    [MaxLength(6, ErrorMessage = "AW ID Appartamento must be max 6 chars")]

    public string? AWIDAppartamento { get; set; }
}