using System.ComponentModel.DataAnnotations;

namespace MAppBnB.Models;

public class Configuration
{
    public int? id { get; set; }

    public int? PersonID { get; set; }
    public int? DocumentID { get; set; }
    [DisplayFormat(DataFormatString = "{0:P2}")]
    public decimal IVAVendite { get; set; }
    [DisplayFormat(DataFormatString = "{0:P2}")]

    public decimal IVACommissioni { get; set; }
    [DisplayFormat(DataFormatString = "{0:P2}")]

    public decimal CommissioneBancaria { get; set; }
    [DisplayFormat(DataFormatString = "{0:P2}")]

    public decimal CedolareSecca { get; set; } //flat rate tax

    [DataType(DataType.Text)]
    public string? AlloggiatiWebUsername { get; set; } // Username to be adequately handled for future deployment (Hashing, etc.)
    [DataType(DataType.Password)]
    public string? AlloggiatiWebPassword { get; set; } // Password to be adequately handled for future deployment (Hashing, etc.)
    [DataType(DataType.Text)]
    public string? AlloggiatiWebWSKey { get; set; } // WebWSKey to be adequately handled for future deployment (Hashing, etc.)
    [Display(Name = "ID Appartamenti AlloggiatiWeb")]
    public bool IsGestioneAppartamenti { get; set; }
}
