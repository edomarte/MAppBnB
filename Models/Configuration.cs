using System.ComponentModel.DataAnnotations;

namespace MAppBnB.Models;

public class Configuration
{
    public int? id { get; set; }

    public int? PersonID{get;set;}
    public int? DocumentID { get; set; }
    [DataType(DataType.Text)]
    public string? AlloggiatiWebUsername { get; set; } // Username to be adequately handled for future deployment (Hashing, etc.)
    [DataType(DataType.Password)]
    public string? AlloggiatiWebPassword { get; set; } // Password to be adequately handled for future deployment (Hashing, etc.)
    [DataType(DataType.Text)]
    public string? AlloggiatiWebWSKey { get; set; } // WebWSKey to be adequately handled for future deployment (Hashing, etc.)
    [Display(Name ="ID Appartamenti AlloggiatiWeb")]
    public bool IsGestioneAppartamenti { get; set; }
}
