using System.ComponentModel.DataAnnotations;

namespace MAppBnB;

public class BookChannel
{
    public int id { get; set; }

    [StringLength(30, MinimumLength = 3)]
    [Required]
    [DataType(DataType.Text)]
    public string? Name { get; set; }
    [Required]
    public decimal? Fee { get; set; }
}