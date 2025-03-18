using System.Data;
using System.Threading.Channels;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using MAppBnB;
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

    public static string GenerateExcelFinancialReport(List<Booking> bookings, BookChannel channel, List<Person> mainPersons, List<Room> rooms, Accommodation accommodation, string dateFrom, string dateTo, Configuration configuration, List<int> guestsNums) 
    {
        string fileName = accommodation.Name + "_" + channel.Name + "_" + dateFrom + "_" + dateTo;
        string reportPath = "..\\DocumentTemplates\\Report_" + fileName + ".xlsx"; //TODO: add Channel name, accommodation name, datefrom, dateto to path

        File.Copy("..\\DocumentTemplates\\Report.xlsx", "..\\DocumentTemplates\\" + reportPath, true);
        using (SpreadsheetDocument doc = SpreadsheetDocument.Open("..\\DocumentTemplates\\" + reportPath, true))
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
                    Row firstRow = sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex == 0);
                    firstRow = addCellsToHeaderRow(firstRow, accommodation, channel, configuration);
                    for (int i=0; i<bookings.Count;i++)
                    {
                        sheetData.Append(addCellsToRow(firstRow,bookings[i],guestsNums[i],mainPersons[i],rooms[i],accommodation));
                    }

                    addLastSumCellsToRow(bookings.Count);
                    worksheetPart.Worksheet.Save();
                }
            }
            doc.Save();
        }
        return reportPath;
    }

    private static Row addCellsToHeaderRow(Row firstRow, Accommodation accommodation, BookChannel channel, Configuration configuration)
    {
        Cell cellTownFee = firstRow.Elements<Cell>().FirstOrDefault(c => c.CellReference == "O1");
        Cell cellIVAVendite = firstRow.Elements<Cell>().FirstOrDefault(c => c.CellReference == "Q1");
        Cell cellChannelFee = firstRow.Elements<Cell>().FirstOrDefault(c => c.CellReference == "R1");
        Cell cellCommissioneBancaria = firstRow.Elements<Cell>().FirstOrDefault(c => c.CellReference == "S1");
        Cell cellIVACommissioni = firstRow.Elements<Cell>().FirstOrDefault(c => c.CellReference == "U1");
        Cell cellCedolareSecca = firstRow.Elements<Cell>().FirstOrDefault(c => c.CellReference == "X1");

        cellTownFee.CellValue = new CellValue(accommodation.TownFee.Value);
        cellIVAVendite.CellValue = new CellValue(configuration.IVAVendite);
        cellChannelFee.CellValue = new CellValue(channel.Fee.Value);
        cellCommissioneBancaria.CellValue = new CellValue(configuration.CommissioneBancaria);
        cellIVACommissioni.CellValue = new CellValue(configuration.IVACommissioni);
        cellCedolareSecca.CellValue = new CellValue(configuration.CedolareSecca);
        /*
            firstRow.Append(
                   new Cell() { CellReference="O1", DataType = CellValues.Number, CellValue = new CellValue(accommodation.TownFee.Value)},
                   new Cell() { CellReference="Q1", DataType = CellValues.Number, CellValue = new CellValue(configuration.IVAVendite)},
                   new Cell() { CellReference="R1", DataType = CellValues.Number, CellValue = new CellValue(channel.Fee.Value)},
                   new Cell() { CellReference="S1", DataType = CellValues.Number, CellValue = new CellValue(configuration.CommissioneBancaria)},
                   new Cell() { CellReference="U1", DataType = CellValues.Number, CellValue = new CellValue(configuration.IVACommissioni)},
                   new Cell() { CellReference="X1", DataType = CellValues.Number, CellValue = new CellValue(configuration.CedolareSecca)}
                );*/
        return firstRow;
    }

    private static Row addCellsToRow(Row header, Booking b, int guestsNum, Person mainPerson, Room room, Accommodation accommodation) //TODO: other objects needed
    {
        Row row = new Row();
        double nightsNum = (b.CheckOutDateTime.Value.Date - b.CheckinDateTime.Value.Date).TotalDays;
        double grossLessDiscount = Convert.ToDouble(b.Price.Value - b.Discount.Value + accommodation.CleaningFee.Value);
        double TownFee = Convert.ToDouble(header.Elements<Cell>().FirstOrDefault(c => c.CellReference == "O1").CellValue);
        double ivaVendite = Convert.ToDouble(header.Elements<Cell>().FirstOrDefault(c => c.CellReference == "Q1").CellValue);
        double ivaCommissioni = Convert.ToDouble(header.Elements<Cell>().FirstOrDefault(c => c.CellReference == "U1").CellValue);
        double channelFee = Convert.ToDouble(header.Elements<Cell>().FirstOrDefault(c => c.CellReference == "R1").CellValue);
        double bankCommission = Convert.ToDouble(header.Elements<Cell>().FirstOrDefault(c => c.CellReference == "S1").CellValue);
        double fixedTax = Convert.ToDouble(header.Elements<Cell>().FirstOrDefault(c => c.CellReference == "X1").CellValue);//Cedolare secca

        double totalFees = (channelFee * nightsNum) + (bankCommission * grossLessDiscount);
        double ivaCommissioniValue = totalFees * ivaCommissioni;
        double ivaVenditeValue = grossLessDiscount * ivaVendite;
        double grossTotalPlusExtra = grossLessDiscount + Convert.ToDouble(accommodation.CleaningFee.Value);
        double netBeforeFixedTax=grossTotalPlusExtra - (totalFees + ivaCommissioniValue);
        double fixedTaxValue = (grossTotalPlusExtra - ivaVenditeValue) * fixedTax;

        //TODO: completare sotto
        row.Append(
            new Cell() { DataType = CellValues.String, CellValue = new CellValue(b.CheckinDateTime.Value.Date.ToString("dd-MM-yyyy") ?? string.Empty) },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue($"{mainPerson.Name} {mainPerson.Surname}") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue(room.Name) },
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(Convert.ToDouble(b.Price - b.Discount) / nightsNum) },
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(nightsNum) },
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(guestsNum) },
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue("#ospiti esenti") },
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(nightsNum) },
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(b.Price.Value) },
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(b.Discount.Value / b.Price.Value) },
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(b.Discount.Value) },
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(b.Price.Value - b.Discount.Value) },
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(accommodation.CleaningFee.Value) },
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(grossTotalPlusExtra) },//"Lordo scontato + extra (commissioni)"
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(nightsNum * TownFee) },//Tassa di soggiorno
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(grossLessDiscount) },//Lordo scontato + extra
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(ivaVenditeValue) },//IVA vendite
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(channelFee * nightsNum) },//Commissione
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(bankCommission * grossLessDiscount) }, //Commissione Bancaria
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(totalFees) }, //Totale commissioni
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(ivaCommissioniValue) },//IVA su commissioni
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(totalFees + ivaCommissioniValue) },//Totale Costi
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(netBeforeFixedTax) },//Totale netto lordo cedolare
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(fixedTaxValue) },//Cedolare secca
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue(netBeforeFixedTax-fixedTaxValue) },
            new Cell() { DataType = CellValues.Number, CellValue = new CellValue("Fattura costi") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("ID Pagamento") },
            new Cell() { DataType = CellValues.Date, CellValue = new CellValue(Convert.ToDateTime(b.PaymentDate.Value).Date.ToString("dd-MM-yyyy")) }

        );
        return row;
    }

    private static Row addLastSumCellsToRow(int lastRowIndex)
    {
        Row row = new Row();


        row.Append(
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("") },

            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(D3:D{lastRowIndex})" } },
            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(E3:E{lastRowIndex})" } },
            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(F3:F{lastRowIndex})" } },
            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(G3:G{lastRowIndex})" } },
            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(H3:H{lastRowIndex})" } },
            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(I3:I{lastRowIndex})" } },
            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(J3:J{lastRowIndex})" } },
            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(K3:K{lastRowIndex})" } },
            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(L3:L{lastRowIndex})" } },
            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(M3:M{lastRowIndex})" } },
            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(N3:N{lastRowIndex})" } },
            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(O3:O{lastRowIndex})" } },
            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(P3:P{lastRowIndex})" } },
            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(Q3:Q{lastRowIndex})" } },
            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(R3:R{lastRowIndex})" } },
            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(S3:S{lastRowIndex})" } },
            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(T3:T{lastRowIndex})" } },
            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(U3:U{lastRowIndex})" } },
            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(V3:V{lastRowIndex})" } },
            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(W3:W{lastRowIndex})" } },
            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(X3:X{lastRowIndex})" } },
            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(Y3:Y{lastRowIndex})" } },
            new Cell() { DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(Z3:Z{lastRowIndex})" } },

            new Cell() { DataType = CellValues.String, CellValue = new CellValue("") },
            new Cell() { DataType = CellValues.String, CellValue = new CellValue("") }

        );
        return row;
    }

}