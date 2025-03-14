using System.Data;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using MAppBnB;


public class DocumentProcessing
{
    static DataTable contractDt;
    static DataTable bookingDetailsDt;
    static DataTable preCheckinDt;

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
            var mainDocumentPart = doc.MainDocumentPart.Document.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>().Where(t => t.Text.Contains("«")).ToList();



            foreach (DocumentFormat.OpenXml.Wordprocessing.Text placeholder in mainDocumentPart)
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
        //dr["CodFisc"];
        dr["PhonePrefix"] = mainPerson.PhonePrefix;
        dr["PhoneNumber"] = mainPerson.PhoneNumber;
        dr["Email"] = mainPerson.Email;
        dr["Price"] = booking.Price - booking.Discount;
        //dr["PriceInLetters"];
        dr["PaymentDate"] = booking.PaymentDate;
        //TODO
        /*dr["BookingNightsNum"] = (DateTime.Parse(booking.CheckOutDateTime).Date - DateTime.Parse(booking.CheckinDateTime).Date).TotalDays;
        dr["CheckinDate"] = booking.CheckinDateTime.Substring(0, booking.CheckinDateTime.IndexOf("T"));
        dr["CheckoutDate"] = booking.CheckOutDateTime.Substring(0, booking.CheckOutDateTime.IndexOf("T"));*/
        dr["ContractDate"] = DateTime.Now.Date.ToString("dd/MM/yyyy");
        dr["City"] = accommodation.City;
        dr["Address"] = accommodation.Address;

        contractDt.Rows.Add(dr);
        return dr;
    }

    public static string GenerateContractPDF(string bookingId)
    {
        string docPath = "..\\DocumentTemplates\\Contract" + bookingId + ".docx";
        string pdfPath = "..\\DocumentTemplates\\Contract" + bookingId + ".pdf";
        MigraDocPDF.ConvertWordToPdf(docPath, pdfPath);
        return pdfPath;
    }

    public static string GeneratePreCheckinPDF(string bookingId)
    {
        string docPath = "..\\DocumentTemplates\\Pre-Checkin" + bookingId + ".docx";
        string pdfPath = "..\\DocumentTemplates\\Pre-Checkin" + bookingId + ".pdf";
        MigraDocPDF.ConvertWordToPdf(docPath, pdfPath);
        return pdfPath;
    }

    public static string GenerateBookingDetails(List<Person> persons, Booking booking, Accommodation accommodation, Room room, BookChannel channel)
    {
        DataRow dr = addRowTobookingDetailsDt(persons, booking, accommodation, room, channel);
        string bookingId = booking.id.ToString();

        string bookingDetailsPath = "..\\DocumentTemplates\\BookingDetails" + bookingId + ".docx";

        File.Copy("..\\DocumentTemplates\\BookingDetailsTemplate.docx", "..\\DocumentTemplates\\BookingDetails" + bookingId + ".docx", true);
        using (WordprocessingDocument doc = WordprocessingDocument.Open("..\\DocumentTemplates\\BookingDetails" + bookingId + ".docx", true))
        {

            if (doc is null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            // Assign a reference to the existing document body.
            var mainDocumentPart = doc.MainDocumentPart.Document.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>().Where(t => t.Text.Contains("«")).ToList();

            foreach (DocumentFormat.OpenXml.Wordprocessing.Text placeholder in mainDocumentPart)
            {
                string columnName = placeholder.Text.Replace("«", "").Replace("»", "");
                if (bookingDetailsDt.Columns.Contains(columnName))
                {
                    placeholder.Text = dr[columnName].ToString();
                }
            }

            doc.Save();
        }
        return bookingDetailsPath;
    }

    private static DataRow addRowTobookingDetailsDt(List<Person> persons, Booking booking, Accommodation accommodation, Room room, BookChannel channel)
    {
        bookingDetailsDt = new DataTable();
        addFieldstoBookingDetailsDt();
        DataRow dr = bookingDetailsDt.NewRow();

        dr["Name"] = persons[0].Name;
        dr["Surname"] = persons[0].Surname;
        dr["BookingDate"] = booking.BookingDate;
        dr["Channel"] = channel.Name;
        dr["PhonePrefix"] = persons[0].PhonePrefix;
        dr["PhoneNumber"] = persons[0].PhoneNumber;
        dr["Room"] = room.Name;
        dr["CheckinDate"] = booking.CheckinDateTime;
        dr["CheckOutDate"] = booking.CheckOutDateTime;
        dr["Price"] = booking.Price - booking.Discount;
        dr["CleaningFee"] = accommodation.CleaningFee;
        dr["TownFee"] = accommodation.TownFee;
        dr["OTAFee"] = (booking.Price - booking.Discount) * channel.Fee;
        dr["MainGuest"] = persons[0].Name + " " + persons[0].Surname;
        if (persons.Count > 1)
        {
            dr["Guest1"] = persons[1].Name + " " + persons[1].Surname;
            if (persons.Count > 2)
            {
                dr["Guest2"] = persons[2].Name + " " + persons[2].Surname;
                if (persons.Count > 3)
                {
                    dr["Guest3"] = persons[3].Name + " " + persons[3].Surname;
                    if (persons.Count > 4)
                    {
                        dr["Guest4"] = persons[4].Name + " " + persons[4].Surname;
                    }
                }
            }
        }
        dr["Sent2Police"] = booking.Sent2Police;
        dr["Sent2Region"] = booking.Sent2Region;
        dr["Sent2Town"] = booking.Sent2Town;
        dr["ContractPrinted"] = booking.ContractPrinted;

        bookingDetailsDt.Rows.Add(dr);
        return dr;
    }

    private static void addFieldstoBookingDetailsDt()
    {
        bookingDetailsDt.Columns.Add("Name");
        bookingDetailsDt.Columns.Add("Surname");
        bookingDetailsDt.Columns.Add("BookingDate");
        bookingDetailsDt.Columns.Add("Channel");
        bookingDetailsDt.Columns.Add("PhonePrefix");
        bookingDetailsDt.Columns.Add("PhoneNumber");
        bookingDetailsDt.Columns.Add("Room");
        bookingDetailsDt.Columns.Add("CheckinDate");
        bookingDetailsDt.Columns.Add("CheckOutDate");
        bookingDetailsDt.Columns.Add("Price");
        bookingDetailsDt.Columns.Add("CleaningFee");
        bookingDetailsDt.Columns.Add("TownFee");
        bookingDetailsDt.Columns.Add("OTAFee");
        bookingDetailsDt.Columns.Add("MainGuest");
        bookingDetailsDt.Columns.Add("Guest1");
        bookingDetailsDt.Columns.Add("Guest2");
        bookingDetailsDt.Columns.Add("Guest3");
        bookingDetailsDt.Columns.Add("Guest4");
        bookingDetailsDt.Columns.Add("Sent2Police");
        bookingDetailsDt.Columns.Add("Sent2Region");
        bookingDetailsDt.Columns.Add("Sent2Town");
        bookingDetailsDt.Columns.Add("ContractPrinted");
    }

    public static string GeneratePreCheckIn(Person mainPerson, Accommodation accommodation, Booking booking)
    {
        DataRow dr = addRowToPreCheckinDt(mainPerson, accommodation, booking);
        string bookingId = booking.id.ToString();

        string preCheckinPath = "..\\DocumentTemplates\\Pre-Checkin" + bookingId + ".docx";

        File.Copy("..\\DocumentTemplates\\Pre-CheckinTemplate.docx", "..\\DocumentTemplates\\Pre-Checkin" + bookingId + ".docx", true);
        using (WordprocessingDocument doc = WordprocessingDocument.Open("..\\DocumentTemplates\\Pre-Checkin" + bookingId + ".docx", true))
        {

            if (doc is null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            // Assign a reference to the existing document body.
            var mainDocumentPart = doc.MainDocumentPart.Document.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>().Where(t => t.Text.Contains("«")).ToList();

            foreach (DocumentFormat.OpenXml.Wordprocessing.Text placeholder in mainDocumentPart)
            {
                string columnName = placeholder.Text.Replace("«", "").Replace("»", "");
                if (preCheckinDt.Columns.Contains(columnName))
                {
                    placeholder.Text = dr[columnName].ToString();
                }
            }

            doc.Save();
        }
        return preCheckinPath;
    }

    private static DataRow addRowToPreCheckinDt(Person mainPerson, Accommodation accommodation, Booking booking)
    {
        preCheckinDt = new DataTable();
        addFieldstoPreCheckinDt();
        DataRow dr = preCheckinDt.NewRow();

        dr["Name"] = mainPerson.Name;
        dr["AccommodationName"] = accommodation.Name;
        dr["City"] = accommodation.City;
        dr["AccommodationWebsite"] = accommodation.Website;
        //TODO
        //dr["CheckinDate"] = booking.CheckinDateTime.Substring(0, booking.CheckinDateTime.IndexOf("T"));

        preCheckinDt.Rows.Add(dr);
        return dr;
    }

    private static void addFieldstoPreCheckinDt()
    {
        preCheckinDt.Columns.Add("Name");
        preCheckinDt.Columns.Add("AccommodationName");
        preCheckinDt.Columns.Add("City");
        preCheckinDt.Columns.Add("CheckinDate");
        preCheckinDt.Columns.Add("AccommodationWebsite");
    }

    public static string GenerateExcelFinancialReport(List<Booking> bookings) //TODO: Channel name, accommodation name, datefrom, dateto
    {

        string contractPath = "..\\DocumentTemplates\\Report" + 1 + ".xlsx"; //TODO: add Channel name, accommodation name, datefrom, dateto to path

        File.Copy("..\\DocumentTemplates\\Report.xlsx", "..\\DocumentTemplates\\Report" + 1 + ".xlsx", true);
        using (SpreadsheetDocument doc = SpreadsheetDocument.Open("..\\DocumentTemplates\\Report" + 1 + ".xlsx", true))
        {

            if (doc is null)
            {
                throw new ArgumentNullException(nameof(doc));
            }
            WorkbookPart workbookPart = doc.WorkbookPart;
            Sheet sheet = workbookPart.Workbook.Sheets.GetFirstChild<Sheet>();

            if (sheet != null)
            {
                WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                if (sheetData != null)
                {
                    foreach (Booking b in bookings)
                    {
                        sheetData.Append(addFieldsToRow(b)); 
                    }
                    //TODO: somma ultima riga dove serve
                    worksheetPart.Worksheet.Save();
                }
            }
            doc.Save();
        }
        return contractPath;
    }

    private static Row addFieldsToRow(Booking b) //TODO: other objects needed
    {
        Row row = new Row();
        //TODO: completare sotto
        row.Append(
            new Cell() { DataType = CellValues.String, CellValue = new CellValue(b.CheckinDateTime?.ToString("yyyy-MM-dd") ?? string.Empty) },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("Ospite") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("Camera") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("Importo per notte") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("#notti") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("#ospiti") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("#ospiti esenti") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("notti imponibili") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("Lordo") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("Sconto%") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("Importo Sconto") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("Lordo Scontato") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("Extra") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("Lordo scontato + extra (commissioni)") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("Imposta soggiorno") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("Lordo scontato + extra") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("IVA vendite") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("Commissione") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("Commissione Bancaria") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("Totale commissioni") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("IVA su commissioni") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("Totale costi") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("Totale netto lordo cedolare") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("Cedolare secca") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("Totale netto") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("Fattura costi") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("ID Pagamento") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("Data incasso") }

        );
        return row;
    }

}