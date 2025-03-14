using System.ComponentModel.DataAnnotations;

namespace MAppBnB.Models;

public class TipoAlloggiato
{
    [Key]
    public string? Codice { get; set; }
    public string? Descrizione { get; set; }

}
