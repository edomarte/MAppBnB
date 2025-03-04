using System.Data;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using MAppBnB;
using Microsoft.AspNetCore.Mvc;

public class DocumentProcessing
{
    static DataTable contractDt;
   
    private static void addFieldstoContractDt()
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

    public static string GenerateContract(Person mainPerson, MAppBnB.Document document, Accommodation accommodation, Booking booking)
    {
        DataRow dr = addRowToContractDt(mainPerson, document, accommodation, booking);
        string bookingId = booking.id.ToString();

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

    private static DataRow addRowToContractDt(Person mainPerson, MAppBnB.Document document, Accommodation accommodation, Booking booking)
    {
        //TODO: contractDT empty?
        contractDt = new DataTable();
        addFieldstoContractDt();
        DataRow dr = contractDt.NewRow();

        dr["Name"] = mainPerson.Name;
        dr["Surname"] = mainPerson.Surname;
        dr["BirthPlace"] = mainPerson.BirthPlace;
        dr["BirthProvince"] = mainPerson.BirthProvince;
        dr["BirthDate"] = mainPerson.BirthDate;
        dr["DocumentType"] = document.DocumentType;
        dr["SerialNumber"] = document.SerialNumber;
        dr["IssuedBy"] = document.IssuedBy;
        dr["IssuedDate"] = document.IssuedDate;
        //dr["CodFisc"];
        dr["PhonePrefix"] = mainPerson.PhonePrefix;
        dr["PhoneNumber"] = mainPerson.PhoneNumber;
        dr["Email"] = mainPerson.Email;
        dr["Price"] = booking.Price - booking.Discount;
        //dr["PriceInLetters"];
        dr["PaymentDate"] = booking.PaymentDate;
        dr["BookingNightsNum"] =(DateTime.Parse(booking.CheckOutDateTime).Date-DateTime.Parse(booking.CheckinDateTime).Date).TotalDays;
        dr["CheckinDate"] = booking.CheckinDateTime.Substring(0,booking.CheckinDateTime.IndexOf("T"));
        dr["CheckoutDate"] = booking.CheckOutDateTime.Substring(0,booking.CheckOutDateTime.IndexOf("T"));
        dr["ContractDate"] = DateTime.Now.Date.ToString("dd/MM/yyyy");
        dr["City"] = accommodation.City;
        dr["Address"] = accommodation.Address;

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