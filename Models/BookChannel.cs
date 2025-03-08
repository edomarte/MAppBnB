using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MAppBnB;

public class BookChannel
{
    public int id { get; set; }

    [StringLength(30, MinimumLength = 3)]
    [Required]
    [DataType(DataType.Text)]
    public string? Name { get; set; }
    [Required]
    [Precision(18,2)]
    public decimal? Fee { get; set; }
}