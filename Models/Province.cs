using System.ComponentModel.DataAnnotations;

namespace MAppBnB.Models;

public class Province
{
    [Key]
    public string? Codice { get; set; }
    public string? Descrizione { get; set; }

}
