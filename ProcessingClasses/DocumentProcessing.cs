using System.Data;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

public class DocumentProcessing
{
    public void GenerateContract(DataRow dr, DataColumnCollection columns)
    {
        using (WordprocessingDocument doc = WordprocessingDocument.Open("..\\DocumentTemplates\\ContractTemplate.dotx", true))
        {

            if (doc is null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            // Assign a reference to the existing document body.
            var mainDocumentPart = doc.MainDocumentPart.Document.Descendants<Text>();
            var a ="";
        }
    }

    public void GenerateCustomerProfile()
    {

    }

    public void GeneratePreCheckIn()
    {

    }
}