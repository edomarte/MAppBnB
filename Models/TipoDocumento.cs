using System.ComponentModel.DataAnnotations;

namespace MAppBnB.Models;

public class TipoDocumento
{
    [Key]
    public string? Codice { get; set; }

    public string? Descrizione { get; set; }

}
