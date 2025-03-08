using System.ComponentModel.DataAnnotations;
<<<<<<< HEAD
using Microsoft.EntityFrameworkCore;
=======
>>>>>>> 3585d287e58e65249e86a8d8e4f25333c08dcad8

namespace MAppBnB;

public class BookChannel
{
    public int id { get; set; }

    [StringLength(30, MinimumLength = 3)]
    [Required]
    [DataType(DataType.Text)]
    public string? Name { get; set; }
    [Required]
<<<<<<< HEAD
    [Precision(18,2)]
=======
>>>>>>> 3585d287e58e65249e86a8d8e4f25333c08dcad8
    public decimal? Fee { get; set; }
}