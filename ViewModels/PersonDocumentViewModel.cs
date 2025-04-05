using MAppBnB;

public class PersonDocumentViewModel
{
    public Person ?Person { get; set; }
    public Document ?Document { get; set; }
    public IFormFile ?PdfCopyPath { get; set;}
    public string ?RoleName{get;set;}
    public string ?BirthCountryName{get;set;}
    public string ?BirthPlaceName{get;set;}
}