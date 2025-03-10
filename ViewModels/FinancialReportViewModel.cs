using System.ComponentModel.DataAnnotations;
using MAppBnB;
using Microsoft.EntityFrameworkCore;

public class FinancialReportsDetailsViewModel
{
    public int? id { get; set; }
    public int? AccommodationID { get; set; }
    [DataType(DataType.Date)]
    public DateTime? DateFrom { get; set; }
    [DataType(DataType.Date)]
    public DateTime? DateTo { get; set; }
    public List<FinancialsByChannel>? FinancialsByChannels { get; set; }
}

public class FinancialsByChannel
{
    public int? id { get; set; }
    [DataType(DataType.Text)]
    public string? ChannelName { get; set; }
    public int? TotBookings { get; set; }
    public int? TotNights { get; set; }
    [DataType(DataType.Currency)]
    [Precision(18, 2)]
    public decimal? GrossRevenue { get; set; }
    [DataType(DataType.Currency)]
    [Precision(18, 2)]
    public decimal? NetRevenue { get; set; }

}