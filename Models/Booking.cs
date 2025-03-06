using System.ComponentModel.DataAnnotations;

namespace MAppBnB;

public class Booking
{
    public int id { get; set; }

    [DataType(DataType.Date)]
    [Required]
    public string? BookingDate { get; set; }
    [DataType(DataType.DateTime)]
    [Required]
    public string? CheckinDateTime { get; set; }
    [DataType(DataType.DateTime)]
    [Required]
    public string? CheckOutDateTime { get; set; }
    [DataType(DataType.Date)]
    [Required]
    public string? PaymentDate { get; set; }
    [DataType(DataType.Text)]
    [Required]
    public int? IsPaid { get; set; }
    [DataType(DataType.Text)]
    [Required]
    public int? ChannelID { get; set; }
    [Required]
    public int? AccommodationID { get; set; }
    [Required]
    public int? RoomID { get; set; }
    [Required]
    [DataType(DataType.Currency)]
    public int? Price { get; set; }
    [Required]
    public int? Discount { get; set; }
    [DataType(DataType.Text)]
    [Required]
    public int? Sent2Police { get; set; }
    [DataType(DataType.Text)]
    [Required]
    public int? Sent2Region { get; set; }
    [DataType(DataType.Text)]
    [Required]
    public int? Sent2Town { get; set; }
    [DataType(DataType.Text)]
    [Required]
    public int? ContractPrinted { get; set; }
}