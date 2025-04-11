using System.Data;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using MAppBnB;
using SignalRChat.Hubs;
using Configuration = MAppBnB.Models.Configuration;


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

        contractDt.Columns.Add("HName");
        contractDt.Columns.Add("HSurname");
        contractDt.Columns.Add("HBirthPlace");
        contractDt.Columns.Add("HBirthProvince");
        contractDt.Columns.Add("HBirthDate");
        contractDt.Columns.Add("HDocType");
        contractDt.Columns.Add("HDocSerialNum");
        contractDt.Columns.Add("HIssuedBy");
        contractDt.Columns.Add("HIssuedDate");
        //contractDt.Columns.Add("HCodFisc");
        contractDt.Columns.Add("HPhonePrefix");
        contractDt.Columns.Add("HPhoneNumber");
        contractDt.Columns.Add("HEmail");
    }

    public static string GenerateContract(Person mainPerson, MAppBnB.Document document, Accommodation accommodation, Booking booking, Person host, MAppBnB.Document hostDocument)
    {
        DataRow dr = addRowToContractDt(mainPerson, document, accommodation, booking, host, hostDocument);
        string bookingId = booking.id.ToString();

        string contractPath = "..\\MAppBnB\\DocumentTemplates\\Contract" + bookingId + ".docx";

        File.Copy("..\\MAppBnB\\DocumentTemplates\\ContractTemplate.docx", "..\\MAppBnB\\DocumentTemplates\\Contract" + bookingId + ".docx", true);
        using (WordprocessingDocument doc = WordprocessingDocument.Open("..\\MAppBnB\\DocumentTemplates\\Contract" + bookingId + ".docx", true))
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

    private static DataRow addRowToContractDt(Person mainPerson, MAppBnB.Document document, Accommodation accommodation, Booking booking, Person host, MAppBnB.Document hostDocument)
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
        dr["BookingNightsNum"] = (booking.CheckOutDateTime.Date - booking.CheckinDateTime.Date).TotalDays;
        dr["CheckinDate"] = booking.CheckinDateTime.Date.ToString("dd/MM/yyyy");
        dr["CheckoutDate"] = booking.CheckOutDateTime.Date.ToString("dd/MM/yyyy");
        dr["ContractDate"] = DateTime.Now.Date.ToString("dd/MM/yyyy");
        dr["City"] = accommodation.City;
        dr["Address"] = accommodation.Address;

        dr["HName"] = host.Name;
        dr["HSurname"] = host.Surname;
        dr["HBirthPlace"] = host.BirthPlace;
        dr["HBirthProvince"] = host.BirthProvince;
        dr["HBirthDate"] = host.BirthDate;
        dr["HDocType"] = hostDocument.DocumentType;
        dr["HDocSerialNum"] = hostDocument.SerialNumber;
        //dr["HCodFisc"];
        dr["HPhonePrefix"] = host.PhonePrefix;
        dr["HPhoneNumber"] = host.PhoneNumber;
        dr["HEmail"] = host.Email;


        contractDt.Rows.Add(dr);
        return dr;
    }

    public static string GenerateContractPDF(string bookingId)
    {
        string docPath = "..\\MAppBnB\\DocumentTemplates\\Contract" + bookingId + ".docx";
        string pdfPath = "..\\MAppBnB\\DocumentTemplates\\Contract" + bookingId + ".pdf";

        if (!File.Exists(docPath))
        {
            throw new FileNotFoundException("Error: Generate a Contract Document first!");
        }

        MigraDocPDF.ConvertWordToPdf(docPath, pdfPath);
        return pdfPath;
    }

    public static string GeneratePreCheckinPDF(string bookingId)
    {
        string docPath = "..\\MAppBnB\\DocumentTemplates\\Pre-Checkin" + bookingId + ".docx";
        string pdfPath = "..\\MAppBnB\\DocumentTemplates\\Pre-Checkin" + bookingId + ".pdf";

        if (!File.Exists(docPath))
        {
            throw new FileNotFoundException("Error: Generate a Pre Checkin Document first!");
        }
        MigraDocPDF.ConvertWordToPdf(docPath, pdfPath);
        return pdfPath;
    }

    public static string GenerateBookingDetails(List<Person> persons, Booking booking, Accommodation accommodation, Room room, BookChannel channel)
    {
        DataRow dr = addRowTobookingDetailsDt(persons, booking, accommodation, room, channel);
        string bookingId = booking.id.ToString();

        string bookingDetailsPath = "..\\MAppBnB\\DocumentTemplates\\BookingDetails" + bookingId + ".docx";

        File.Copy("..\\MAppBnB\\DocumentTemplates\\BookingDetailsTemplate.docx", "..\\MAppBnB\\DocumentTemplates\\BookingDetails" + bookingId + ".docx", true);
        using (WordprocessingDocument doc = WordprocessingDocument.Open("..\\MAppBnB\\DocumentTemplates\\BookingDetails" + bookingId + ".docx", true))
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
        // Booking details Word template holds max 4 guests.
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
        dr["PreCheckinSent"] = booking.PreCheckinSent;
        dr["ContractSent"] = booking.ContractSent;
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
        bookingDetailsDt.Columns.Add("PreCheckinSent");
        bookingDetailsDt.Columns.Add("ContractSent");
        bookingDetailsDt.Columns.Add("ContractPrinted");
    }

    public static string GeneratePreCheckIn(Person mainPerson, Accommodation accommodation, Booking booking)
    {
        DataRow dr = addRowToPreCheckinDt(mainPerson, accommodation, booking);
        string bookingId = booking.id.ToString();

        string preCheckinPath = "..\\MAppBnB\\DocumentTemplates\\Pre-Checkin" + bookingId + ".docx";

        File.Copy("..\\MAppBnB\\DocumentTemplates\\Pre-CheckinTemplate.docx", "..\\MAppBnB\\DocumentTemplates\\Pre-Checkin" + bookingId + ".docx", true);
        using (WordprocessingDocument doc = WordprocessingDocument.Open("..\\MAppBnB\\DocumentTemplates\\Pre-Checkin" + bookingId + ".docx", true))
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
        dr["CheckinDate"] = booking.CheckinDateTime.Date.ToString("dd-MM-yyyy");

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

    public static string GenerateExcelFinancialReport(List<FinancialReportLine> frlines, BookChannel channel, Accommodation accommodation, string dateFrom, string dateTo, Configuration configuration)
    {
        string fileName = "";
        if (channel is null)
        {
            fileName = accommodation.Name + "_AllChannels_" + dateFrom + "_" + dateTo;

        }
        else
        {
            fileName = accommodation.Name + "_" + channel.Name + "_" + dateFrom + "_" + dateTo;
        }

        string reportPath = "..\\MAppBnB\\DocumentTemplates\\Report_" + fileName + ".xlsx";

        File.Copy("..\\MAppBnB\\DocumentTemplates\\Report.xlsx", reportPath, true);
        using (SpreadsheetDocument doc = SpreadsheetDocument.Open(reportPath, true))
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
                    Row firstRow = sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex == 1);
                    firstRow = addCellsToHeaderRow(firstRow, accommodation, channel, configuration);

                    foreach (FinancialReportLine line in frlines)
                    {

                        sheetData.Append(addCellsToRow(firstRow, line.Booking, line.GuestCount, line.MainPerson, line.Room, accommodation));
                    }

                    sheetData.Append(addLastSumCellsToRow(3 + frlines.Count - 1)); // skip the header (3 rows) and select only the data rows*/
                    worksheetPart.Worksheet.Save();
                }
            }
        }
        return reportPath;
    }

    private static Row addCellsToHeaderRow(Row firstRow, Accommodation accommodation, BookChannel channel, Configuration configuration)
    {

        // Lambda function
        Action<string, string, CellValues> updateCellValue = (cellRef, value, dataType) =>
        {
            Cell cell = firstRow.Elements<Cell>().FirstOrDefault(c => c.CellReference == cellRef);
            if (cell != null)
            {
                cell.CellValue = new CellValue(value);
                cell.DataType = dataType;
            }
        };

        // Update cell values with null checks and explicit data types
        updateCellValue("A1", channel.Name, CellValues.String);
        updateCellValue("B1", accommodation.Name, CellValues.String);
        updateCellValue("O1", accommodation.TownFee.Value.ToString(), CellValues.Number);
        updateCellValue("Q1", configuration.IVAVendite.ToString(), CellValues.Number);
        updateCellValue("R1", channel.Fee.Value.ToString(), CellValues.Number);
        updateCellValue("S1", configuration.CommissioneBancaria.ToString(), CellValues.Number);
        updateCellValue("U1", configuration.IVACommissioni.ToString(), CellValues.Number);
        updateCellValue("X1", configuration.CedolareSecca.ToString(), CellValues.Number);

        return firstRow;
    }

    private static Row addCellsToRow(Row header, Booking b, int guestsNum, Person mainPerson, Room room, Accommodation accommodation)
    {
        Row row = new Row();
        double nightsNum = (b.CheckOutDateTime.Date - b.CheckinDateTime.Date).TotalDays;
        double grossLessDiscount = Convert.ToDouble(b.Price.Value - b.Discount.Value);
        double TownFee = Convert.ToDouble(header.Elements<Cell>().FirstOrDefault(c => c.CellReference == "O1").CellValue.InnerText);
        double ivaVendite = Convert.ToDouble(header.Elements<Cell>().FirstOrDefault(c => c.CellReference == "Q1").CellValue.InnerText);
        double ivaCommissioni = Convert.ToDouble(header.Elements<Cell>().FirstOrDefault(c => c.CellReference == "U1").CellValue.InnerText);
        double channelFee = Convert.ToDouble(header.Elements<Cell>().FirstOrDefault(c => c.CellReference == "R1").CellValue.InnerText);
        double bankCommission = Convert.ToDouble(header.Elements<Cell>().FirstOrDefault(c => c.CellReference == "S1").CellValue.InnerText);
        double fixedTax = Convert.ToDouble(header.Elements<Cell>().FirstOrDefault(c => c.CellReference == "X1").CellValue.InnerText);//Cedolare secca

        double totalFees = (channelFee * nightsNum) + (bankCommission * grossLessDiscount);
        double ivaCommissioniValue = totalFees * ivaCommissioni;
        double ivaVenditeValue = grossLessDiscount * ivaVendite;
        double grossTotalPlusExtra = grossLessDiscount + Convert.ToDouble(accommodation.CleaningFee.Value);
        double netBeforeFixedTax = grossTotalPlusExtra - (totalFees + ivaCommissioniValue);
        double fixedTaxValue = (grossTotalPlusExtra - ivaVenditeValue) * fixedTax;

        row.Append(
            new Cell() { DataType = CellValues.String, CellValue = new CellValue(b.CheckinDateTime.Date.ToString("dd-MM-yyyy")) }, // Data arrivo
            new Cell() { DataType = CellValues.String, CellValue = new CellValue($"{mainPerson.Name} {mainPerson.Surname}") }, // Ospite
            new Cell() { DataType = CellValues.String, CellValue = new CellValue(room.Name) }, // Camera
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(Convert.ToDouble(b.Price - b.Discount) / nightsNum) }, // Importo per notte
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(nightsNum) }, //#notti
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(guestsNum) }, //#ospiti
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(0) }, // #ospiti esenti //TODO: aggiungere checkbox esente o no su Person
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(nightsNum) }, // #notti imponibili
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(b.Price.Value) }, //#lordo
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(b.Discount.Value / b.Price.Value) }, // sconto%
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(b.Discount.Value) }, // importo sconto
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(b.Price.Value - b.Discount.Value) }, // Lordo scontato
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(accommodation.CleaningFee.Value) }, // Extra (cleanin fee)
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(grossTotalPlusExtra) },//"Lordo scontato + extra (commissioni)"
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(nightsNum * TownFee) },//Imposta soggiorno
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(grossLessDiscount) },//Lordo scontato + extra
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(ivaVenditeValue) },//IVA vendite
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(channelFee * nightsNum) },//Commissione
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(bankCommission * grossLessDiscount) }, //Commissione Bancaria
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(totalFees) }, //Totale commissioni
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(ivaCommissioniValue) },//IVA su commissioni
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(totalFees + ivaCommissioniValue) },//Totale Costi
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(netBeforeFixedTax) },//Totale netto lordo cedolare
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(fixedTaxValue) },//Cedolare secca
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(netBeforeFixedTax - fixedTaxValue) },
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(0) },//Fattura costi TODO: add eventually
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("ID Pagamento") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue(b.PaymentDate.HasValue ? b.PaymentDate.Value.ToString("dd-MM-yyyy") : "N/A") } //Data pagamento
        );
        return row;
    }

    private static Row addLastSumCellsToRow(int lastRowIndex)
    {
        Row row = new Row();


        row.Append(
            new Cell() { CellReference = $"D{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(D3:D{lastRowIndex})" } },
            new Cell() { CellReference = $"E{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(E3:E{lastRowIndex})" } },
            new Cell() { CellReference = $"F{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(F3:F{lastRowIndex})" } },
            new Cell() { CellReference = $"G{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(G3:G{lastRowIndex})" } },
            new Cell() { CellReference = $"H{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(H3:H{lastRowIndex})" } },
            new Cell() { CellReference = $"I{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(I3:I{lastRowIndex})" } },
            new Cell() { CellReference = $"J{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(J3:J{lastRowIndex})" } },
            new Cell() { CellReference = $"K{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(K3:K{lastRowIndex})" } },
            new Cell() { CellReference = $"L{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(L3:L{lastRowIndex})" } },
            new Cell() { CellReference = $"M{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(M3:M{lastRowIndex})" } },
            new Cell() { CellReference = $"N{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(N3:N{lastRowIndex})" } },
            new Cell() { CellReference = $"O{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(O3:O{lastRowIndex})" } },
            new Cell() { CellReference = $"P{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(P3:P{lastRowIndex})" } },
            new Cell() { CellReference = $"Q{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(Q3:Q{lastRowIndex})" } },
            new Cell() { CellReference = $"R{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(R3:R{lastRowIndex})" } },
            new Cell() { CellReference = $"S{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(S3:S{lastRowIndex})" } },
            new Cell() { CellReference = $"T{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(T3:T{lastRowIndex})" } },
            new Cell() { CellReference = $"U{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(U3:U{lastRowIndex})" } },
            new Cell() { CellReference = $"V{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(V3:V{lastRowIndex})" } },
            new Cell() { CellReference = $"W{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(W3:W{lastRowIndex})" } },
            new Cell() { CellReference = $"X{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(X3:X{lastRowIndex})" } },
            new Cell() { CellReference = $"Y{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(Y3:Y{lastRowIndex})" } },
            new Cell() { CellReference = $"Z{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(Z3:Z{lastRowIndex})" } }
        );
        return row;
    }

}