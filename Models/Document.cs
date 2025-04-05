using System.ComponentModel.DataAnnotations;

namespace MAppBnB;

public class Document
{
    public int? id { get; set; }

    public string DocumentType { get; set; }

    [StringLength(30, MinimumLength = 1)]
    [DataType(DataType.Text)]

    public string SerialNumber { get; set; }

    [DataType(DataType.Text)]
    [Display(Name = "Town of Issue (Italian document) or Country of Issue (foreign document)")]
    public string IssuingCountry { get; set; }
    [DataType(DataType.Upload)]
    public byte[]? PdfCopy { get; set; }
    public int PersonID { get; set; }
}