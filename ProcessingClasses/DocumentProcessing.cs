using System.Data;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;

public class DocumentProcessing
{
    static DataTable contractDt;
    DocumentProcessing()
    {
        contractDt = new DataTable();
        addFieldstoContractDt();
    }

    private void addFieldstoContractDt()
    {
        contractDt.Columns.Add("Name");
        contractDt.Columns.Add("Surname");
        contractDt.Columns.Add("BirthPlace");
        contractDt.Columns.Add("BirthProvince");
        contractDt.Columns.Add("BirthDate");
        contractDt.Columns.Add("DocumentType");
        contractDt.Columns.Add("SerialNumber");
        contractDt.Columns.Add("IssuedBy");
        contractDt.Columns.Add("IssuedDate");
        //contractDt.Columns.Add("CodFisc");
        contractDt.Columns.Add("PhonePrefix");
        contractDt.Columns.Add("PhoneNumber");
        contractDt.Columns.Add("Email");
        contractDt.Columns.Add("Price");
        //contractDt.Columns.Add("PriceInLetters");
        contractDt.Columns.Add("PaymentDate");
        contractDt.Columns.Add("BookingNightsNum");
        contractDt.Columns.Add("CheckinDate");
        contractDt.Columns.Add("CheckoutDate");
        contractDt.Columns.Add("ContractDate");
        contractDt.Columns.Add("City");
        contractDt.Columns.Add("Address");
    }

    public static string GenerateContract(string contractParams, string bookingId)
    {
        DataRow dr=addRowToContractDt(contractParams);

        string contractPath = "..\\DocumentTemplates\\Contract" + bookingId + ".docx";

        File.Copy("..\\DocumentTemplates\\ContractTemplate.docx", "..\\DocumentTemplates\\Contract" + bookingId + ".docx", true);
        using (WordprocessingDocument doc = WordprocessingDocument.Open("..\\DocumentTemplates\\Contract" + bookingId + ".docx", true))
        {

            if (doc is null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            // Assign a reference to the existing document body.
            var mainDocumentPart = doc.MainDocumentPart.Document.Descendants<Text>().Where(t => t.Text.Contains("«")).ToList();

            foreach (Text placeholder in mainDocumentPart)
            {
                string columnName = placeholder.Text.Replace("«", "").Replace("»", "");
                if (contractDt.Columns.Contains(columnName))
                {
                    placeholder.Text = dr[columnName].ToString();
                }
            }

            doc.Save();
        }
        return contractPath;
    }

    private static DataRow addRowToContractDt(string contractParams)
    {
        DataRow dr = contractDt.NewRow();
        string[] parameters = contractParams.Split(";");
        for (int i = 0; i < parameters.Length; i++)
        {
            dr[i] = parameters[i];
        }
        contractDt.Rows.Add(dr);
        return dr;
    }

    public void GenerateCustomerProfile()
    {

    }

    public void GeneratePreCheckIn()
    {

    }


}