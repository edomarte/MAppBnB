using System.ComponentModel.DataAnnotations;

namespace MAppBnB.Models;

public class Stati
{
    [Key]
    public string? Codice { get; set; }
    public string? Descrizione { get; set; }
    public string? Provincia { get; set; }
    public DateTime DataFineVal { get; set; }

}
