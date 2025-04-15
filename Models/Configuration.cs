using System.ComponentModel.DataAnnotations;

namespace MAppBnB.Models;

public class Configuration
{
    public int? id { get; set; }

    public int? PersonID { get; set; }
    public int? DocumentID { get; set; }
    [DisplayFormat(DataFormatString = "{0:P2}")]
    public decimal IVAVendite { get; set; } // VAT on Bookings
    [DisplayFormat(DataFormatString = "{0:P2}")]

    public decimal IVACommissioni { get; set; } // VAT on Fees
    [DisplayFormat(DataFormatString = "{0:P2}")]

    public decimal CommissioneBancaria { get; set; } // Bank Commission
    [DisplayFormat(DataFormatString = "{0:P3}")]

    public decimal CedolareSecca { get; set; } // Flat rate tax

    [DataType(DataType.Text)]
    public string? AlloggiatiWebUsername { get; set; } // Username to be adequately handled for future deployment (Hashing, etc.)
    [DataType(DataType.Password)]
    public string? AlloggiatiWebPassword { get; set; } // Password to be adequately handled for future deployment (Hashing, etc.)
    [DataType(DataType.Text)]
    public string? AlloggiatiWebWSKey { get; set; } // WebWSKey to be adequately handled for future deployment (Hashing, etc.)
    [Display(Name = "Do you have a Gestione Appartamenti for AlloggiatiWeb?")]
    public bool IsGestioneAppartamenti { get; set; }
}
