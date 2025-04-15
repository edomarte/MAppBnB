using System.ComponentModel.DataAnnotations;
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