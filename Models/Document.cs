using System.ComponentModel.DataAnnotations;

namespace MAppBnB;

public class Document
{
    public int? id { get; set; }

    [StringLength(30, MinimumLength = 1)]
    [DataType(DataType.Text)]

    public string? SerialNumber { get; set; }

    [DataType(DataType.Text)]
    public string? IssuedBy { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? IssuedDate { get; set; }

    [DataType(DataType.Text)]
    public string? IssuingCountry { get; set; }
    [DataType(DataType.Upload)]
    public byte[]? PdfCopy { get; set; }
    public int? PersonID { get; set; }
}