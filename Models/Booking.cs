using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MAppBnB;

public class Booking
{
    public int id { get; set; }

    [DataType(DataType.Date)]
    [Required]
    public DateOnly? BookingDate { get; set; }
    [DataType(DataType.DateTime)]
    [Required]
    public DateTime? CheckinDateTime { get; set; }
    [DataType(DataType.DateTime)]
    [Required]
    public DateTime? CheckOutDateTime { get; set; }
    [DataType(DataType.Date)]
    public DateOnly? PaymentDate { get; set; }
    [DataType(DataType.Text)]
    [Required]
    public bool? IsPaid { get; set; }
    [DataType(DataType.Text)]
    [Required]
    public int? ChannelID { get; set; }
    [Required]
    public int AccommodationID { get; set; }
    [Required]
    public int RoomID { get; set; }
    [Required]
    [Precision(18,2)]
    [DataType(DataType.Currency)]
    public decimal? Price { get; set; }
    [Required]
    [Precision(18,2)]
    [DataType(DataType.Currency)]
    public decimal? Discount { get; set; }
    [DataType(DataType.Text)]
    [Required]
    public bool? Sent2Police { get; set; }
    [DataType(DataType.Text)]
    [Required]
    public bool? Sent2Region { get; set; }
    [DataType(DataType.Text)]
    [Required]
    public bool? Sent2Town { get; set; }
    [DataType(DataType.Text)]
    [Required]
    public bool? ContractPrinted { get; set; }
}