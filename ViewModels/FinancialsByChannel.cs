using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

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