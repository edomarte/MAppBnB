using MAppBnB;

public class PersonDocumentViewModel
{
    public Person ?Person { get; set; }
    public Document ?Document { get; set; }
    public string ?DocumentTypeName{get;set;}
    public IFormFile ?PdfCopyPath { get; set;}
}