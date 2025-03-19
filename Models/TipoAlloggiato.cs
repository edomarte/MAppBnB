using System.ComponentModel.DataAnnotations;

namespace MAppBnB.Models;

public class TipoAlloggiato
{
    [Key]
    public int? Codice { get; set; }
    public string? Descrizione { get; set; }

}
