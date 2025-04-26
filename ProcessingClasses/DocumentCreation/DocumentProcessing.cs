using System.Data;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MAppBnB;
using Configuration = MAppBnB.Models.Configuration;


public class DocumentProcessing
{
    // DataTable for the contract.
    static DataTable contractDt;
    // DataTable for the booking details document.
    static DataTable bookingDetailsDt;
    // DataTable for the pre-checkin document.
    static DataTable preCheckinDt;

    #region Public methods called by the Hub methods.

    // Method for deleting a document given the filePath. Thow an exception if not found.
    public static void DeleteDocument(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception e)
        {
            throw new Exception("Error deleting file: " + e.Message);
        }
    }

    // Method for generating the Microsoft Word contract.
    public static string GenerateContract(PersonBirthPlace mainPerson, DocumentDocumentType document, AccommodationAccommodationNames accommodation, Booking booking, Person host, DocumentDocumentType hostDocument,string hostBirthPlace)
    {
        // Add a datarow to the contract datatable.
        DataRow dr = addRowToContractDt(mainPerson, document, accommodation, booking, host, hostDocument,hostBirthPlace);
        // Populate fields in the word template
        string contractPath = populateFieldsInWordTemplate(booking.id, "Contract", dr);
        // Return the new contract path.
        return contractPath;
    }

    // Method for generating the Microsoft Word booking details document.
    public static string GenerateBookingDetails(List<Person> persons, Booking booking, AccommodationAccommodationNames accommodation, Room room, BookChannel channel)
    {
        // Add a datarow to the booking details datatable.
        DataRow dr = addRowTobookingDetailsDt(persons, booking, accommodation, room, channel);
        // Populate fields in the word template
        string bookingDetailsPath = populateFieldsInWordTemplate(booking.id, "BookingDetails", dr);
        // Return the new document path.
        return bookingDetailsPath;
    }


    // Method for generating the Microsoft Word pre-check-in details document.
    public static string GeneratePreCheckIn(Person mainPerson, AccommodationAccommodationNames accommodation, Booking booking)
    {
        // Add a datarow to the booking details datatable.
        DataRow dr = addRowToPreCheckinDt(mainPerson, accommodation, booking);
        // Populate fields in the word template
        string preCheckinPath = populateFieldsInWordTemplate(booking.id, "Pre-Checkin", dr);
        // Return the new document path.
        return preCheckinPath;
    }

    // Method for generating a PDF version for a document.
    public static string GeneratePDFDocument(string bookingId, string documentType)
    {
        // Get the path of the doc file and for the new pdf file.
        // Path.Combine ensure the path compatibility regardless of the operating system.
        string docPath = Path.Combine(Directory.GetCurrentDirectory(), "DocumentTemplates", documentType + bookingId + ".docx");
        string pdfPath = Path.Combine(Directory.GetCurrentDirectory(), "DocumentTemplates", documentType + bookingId + ".pdf");

        // If the doc file does not exist, throw an exception.
        if (!File.Exists(docPath))
        {
            throw new FileNotFoundException("Error: Generate the " + documentType + " Document first!");
        }

        // Convert the doc file to pdf.
        MigraDocPDF.ConvertWordToPdf(docPath, pdfPath);
        // Return the pdf path.
        return pdfPath;
    }

    // Method for generating the Microsoft Excel Financial report.
    public static string GenerateExcelFinancialReport(List<FinancialReportLine> frlines, BookChannel channel, Accommodation accommodation, string dateFrom, string dateTo, Configuration configuration)
    {
        string fileName = "";
        // If channel is null, set it to the All Channels.
        if (channel is null)
        {
            channel = new BookChannel() { id = -1, Name = "All_Channels" };
        }

        // Create the filename
        fileName = accommodation.Name + "_" + channel.Name + "_" + dateFrom + "_" + dateTo;

        // Create the template and the new report path.
        string templateReportPath = Path.Combine(Directory.GetCurrentDirectory(), "DocumentTemplates", "ReportTemplate.xlsx");
        string reportPath = Path.Combine(Directory.GetCurrentDirectory(), "DocumentTemplates", "Report_" + fileName + ".xlsx");

        // Copy the template into the new file and open it within the using (closes it automatically at the end).
        File.Copy(templateReportPath, reportPath, true);
        using (SpreadsheetDocument doc = SpreadsheetDocument.Open(reportPath, true))
        {
            // If the document is null, return an exception.
            if (doc is null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            // Get the workbook part from the document.
            WorkbookPart workbookPart = doc.WorkbookPart;
            // Get the first sheet from the workbook.
            Sheet sheet = workbookPart.Workbook.Sheets.GetFirstChild<Sheet>();

            
            if (sheet != null)
            {
                // If the sheet is not null, get the worksheet part given the sheet id from the worksheet.
                WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                // Get the sheet Data from the worksheet part.
                SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                if (sheetData != null)
                {
                    // If the sheet data is not null, get the first row of the sheet.
                    Row firstRow = sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex == 1);
                    // Add the header to the first row.
                    firstRow = addCellsToHeaderRow(firstRow, accommodation, channel, configuration);


                    foreach (FinancialReportLine line in frlines)
                    {
                        // For each line (booking) in the list, append a new line to the excel file with the appropriate values.
                        sheetData.Append(addCellsToRow(line.Booking, line.GuestCount, line.MainPerson, line.Room, line.Channel, accommodation, configuration));
                    }

                    // Append the final row containings the sums of the columns to the excel report.
                    sheetData.Append(addLastSumCellsToRow(3 + frlines.Count - 1)); // skip the header (3 rows) and select only the data rows
                    // Save the worksheet.
                    worksheetPart.Worksheet.Save();
                }
            }else{
                // If the sheet is empty, return an exception.
                throw new Exception("Issues with the excel report template.");
            }
        }
        // Return the new report file path.
        return reportPath;
    }

    #endregion

    #region Helper Methods

    // Method for populating the fields in the word template.
    private static string populateFieldsInWordTemplate(int bookingID, string documentType, DataRow dr)
    {
        // Convert the booking id to string.
        string bookingId = bookingID.ToString();

        // Get the template path and the new booking details document path.
        // Path.Combine ensure the path compatibility regardless of the operating system.
        string documentTemplatePath = Path.Combine(Directory.GetCurrentDirectory(), "DocumentTemplates", documentType + "Template.docx");
        string newDocumentPath = Path.Combine(Directory.GetCurrentDirectory(), "DocumentTemplates", documentType + bookingId + ".docx");

        // Copy the contract template to the new contract path.
        File.Copy(documentTemplatePath, newDocumentPath, true);
        using (WordprocessingDocument doc = WordprocessingDocument.Open(newDocumentPath, true))
        {
            // If the document is null, throw an exception.
            if (doc is null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            // Assign a reference to the existing document body.
            var mainDocumentPart = doc.MainDocumentPart.Document.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>().Where(t => t.Text.Contains("«")).ToList();

            // Create a temporary datatable object and valorize it depending on the type of document to generate.
            DataTable dt = null;
            switch (documentType)
            {
                case "Contract":
                    dt = contractDt;
                    break;
                case "Pre-Checkin":
                    dt = preCheckinDt;
                    break;
                case "BookingDetails":
                    dt = bookingDetailsDt;
                    break;
            }

            // If the document is null, throw an exception.
            foreach (DocumentFormat.OpenXml.Wordprocessing.Text placeholder in mainDocumentPart)
            {
                // For each field in the list, remove the markers "«" and "»".
                string columnName = placeholder.Text.Replace("«", "").Replace("»", "");
                // If the data table contains a column with the field name, add the content of the column to the field/placeholder text.
                if (dt.Columns.Contains(columnName))
                {
                    placeholder.Text = dr[columnName].ToString();
                }
            }
            // Save the document.
            doc.Save();
        }
        // Return the new document path.
        return newDocumentPath;
    }

    // Method for adding the cells to the Header Row of the Excel Financial Report.
    private static Row addCellsToHeaderRow(Row firstRow, Accommodation accommodation, BookChannel channel, Configuration configuration)
    {

        // Lambda function for updating the cell value given the cell reference, the value, and the data type of the cell.
        Action<string, string, CellValues> updateCellValue = (cellRef, value, dataType) =>
        {
            // Get the first row cell given the reference.
            Cell cell = firstRow.Elements<Cell>().FirstOrDefault(c => c.CellReference == cellRef);
            // If the cell is not null, add cell value and datatype to it.
            if (cell != null)
            {
                cell.CellValue = new CellValue(value);
                cell.DataType = dataType;
            }
        };

        // Update cell values with null checks and explicit data types
        // For each cell to be populated, the lambda function is called and the appropriate parameters are passed.
        // Channel Name
        updateCellValue("A1", channel.Name, CellValues.String);
        // Accommodation Name
        updateCellValue("B1", accommodation.Name ?? string.Empty, CellValues.String);
        // Town Fee
        updateCellValue("O1", accommodation.TownFee.HasValue ? accommodation.TownFee.Value.ToString() : "0", CellValues.Number);
        // IVA (VAT) on Booking (Vendite)
        updateCellValue("Q1", configuration.IVAVendite.ToString(), CellValues.Number);
        // Channel Fee
        updateCellValue("R1", channel.Fee.ToString(), CellValues.Number);
        // Bank Commission (Commissione Bancaria)
        updateCellValue("S1", configuration.CommissioneBancaria.ToString(), CellValues.Number);
        // IVA (VAT) on Channel Fee (Commissioni)
        updateCellValue("U1", configuration.IVACommissioni.ToString(), CellValues.Number);
        // Flat Tax (Cedolare secca)
        updateCellValue("X1", configuration.CedolareSecca.ToString(), CellValues.Number);

        // Return the populated first row.
        return firstRow;
    }

    // Method for adding cells to an Excel Financial report row.
    private static Row addCellsToRow(Booking b, int guestsNum, Person mainPerson, Room room, BookChannel channel, Accommodation accommodation, Configuration configuration)
    {
        // Create a new row.
        Row row = new Row();
        // Perform the calculations needed to populate the row.
        double nightsNum = (b.CheckOutDateTime.Date - b.CheckinDateTime.Date).TotalDays;
        double grossLessDiscount = Convert.ToDouble(b.Price.Value - b.Discount.Value);
        double TownFee = Convert.ToDouble(accommodation.TownFee.HasValue ? accommodation.TownFee.Value : 0) * nightsNum;
        double ivaVendite = Convert.ToDouble(configuration.IVAVendite);
        double ivaCommissioni = Convert.ToDouble(configuration.IVACommissioni);
        double channelFee = Convert.ToDouble(channel.Fee);
        double bankCommission = Convert.ToDouble(configuration.CommissioneBancaria);
        double fixedTax = Convert.ToDouble(configuration.CedolareSecca);//Cedolare secca

        double totalFees = (channelFee * nightsNum) + (bankCommission * grossLessDiscount);
        double ivaCommissioniValue = totalFees * ivaCommissioni;
        double ivaVenditeValue = grossLessDiscount * ivaVendite;
        double grossTotalPlusExtra = grossLessDiscount + Convert.ToDouble(accommodation.CleaningFee.Value);
        double netBeforeFixedTax = grossTotalPlusExtra - (totalFees + ivaCommissioniValue);
        double fixedTaxValue = (grossTotalPlusExtra - ivaVenditeValue) * fixedTax;

        // Append the new cells with their correct value to the row.
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
            new Cell() { DataType = CellValues.String, CellValue = new CellValue(b.PaymentDate.HasValue ? b.PaymentDate.Value.ToString("dd-MM-yyyy") : "Not paid") }, //Data pagamento
            new Cell() { DataType = CellValues.String, CellValue = new CellValue(channel.Name) } //Booking Channel (Canale)
        );
        // Return the populated row.
        return row;
    }

    // Method for adding the sum cells to a row.
    private static Row addLastSumCellsToRow(int lastRowIndex)
    {
        // Create a new row.
        Row row = new Row();

        // Append the cells with the sum formula to the row in the appropriate position.
        // The sum start from line 3 to exclude the report header.
        row.Append(
            // Importo per notte
            new Cell() { CellReference = $"D{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(D3:D{lastRowIndex})" } },
            // # notti
            new Cell() { CellReference = $"E{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(E3:E{lastRowIndex})" } },
            // # ospiti
            new Cell() { CellReference = $"F{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(F3:F{lastRowIndex})" } },
            // # ospiti esenti
            new Cell() { CellReference = $"G{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(G3:G{lastRowIndex})" } },
            // notti imponibili
            new Cell() { CellReference = $"H{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(H3:H{lastRowIndex})" } },
            // # lordo
            new Cell() { CellReference = $"I{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(I3:I{lastRowIndex})" } },
            // # importo sconto
            new Cell() { CellReference = $"K{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(K3:K{lastRowIndex})" } },
            // # lordo scontato
            new Cell() { CellReference = $"L{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(L3:L{lastRowIndex})" } },
            // extra
            new Cell() { CellReference = $"M{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(M3:M{lastRowIndex})" } },
            // lordo scontato + extra (commissioni)
            new Cell() { CellReference = $"N{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(N3:N{lastRowIndex})" } },
            // imposta soggiorno
            new Cell() { CellReference = $"O{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(O3:O{lastRowIndex})" } },
            // Lordo scontato + extra
            new Cell() { CellReference = $"P{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(P3:P{lastRowIndex})" } },
            // IVA vendite
            new Cell() { CellReference = $"Q{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(Q3:Q{lastRowIndex})" } },
            // Commissione
            new Cell() { CellReference = $"R{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(R3:R{lastRowIndex})" } },
            // Commissione bancaria
            new Cell() { CellReference = $"S{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(S3:S{lastRowIndex})" } },
            // Totale commissioni
            new Cell() { CellReference = $"T{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(T3:T{lastRowIndex})" } },
            // IVA su commissioni
            new Cell() { CellReference = $"U{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(U3:U{lastRowIndex})" } },
            // Totale costi
            new Cell() { CellReference = $"V{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(V3:V{lastRowIndex})" } },
            // Totale netto lordo cedolare
            new Cell() { CellReference = $"W{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(W3:W{lastRowIndex})" } },
            // Cedolare secca
            new Cell() { CellReference = $"X{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(X3:X{lastRowIndex})" } },
            // Totale netto
            new Cell() { CellReference = $"Y{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(Y3:Y{lastRowIndex})" } },
            // Fattura costi
            new Cell() { CellReference = $"Z{lastRowIndex + 1}", DataType = CellValues.Number, CellFormula = new CellFormula() { Text = $"SUM(Z3:Z{lastRowIndex})" } }
        );
        // Return the row.
        return row;
    }

    // Method to add the fields to the contract data table.
    private static void addFieldsToContractDt()
    {
        // Add columns to the contract data table.
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

    // Method to add the row to the contract data table.
    private static DataRow addRowToContractDt(PersonBirthPlace mainPerson, DocumentDocumentType document, AccommodationAccommodationNames accommodationNames, Booking booking, Person host, DocumentDocumentType hostDocument, string hostBirthPlace)
    {
        // Create a new data table object.
        contractDt = new DataTable();
        // Add Fields to contract data table.
        addFieldsToContractDt();
        // Add a new row to the contract data table
        DataRow dr = contractDt.NewRow();

        // Populate all the field created with the appropriate data.
        dr["Name"] = mainPerson.Person.Name;
        dr["Surname"] = mainPerson.Person.Surname;
        dr["BirthPlace"] = mainPerson.BirthPlace;
        dr["BirthProvince"] = mainPerson.Person.BirthProvince;
        dr["BirthDate"] = mainPerson.Person.BirthDate;
        dr["DocumentType"] = document.DocumentType;
        dr["SerialNumber"] = document.Document.SerialNumber;
        //dr["CodFisc"];
        dr["PhonePrefix"] = mainPerson.Person.PhonePrefix;
        dr["PhoneNumber"] = mainPerson.Person.PhoneNumber;
        dr["Email"] = mainPerson.Person.Email;
        dr["Price"] = booking.Price - booking.Discount;
        //dr["PriceInLetters"];
        dr["PaymentDate"] = booking.PaymentDate;
        dr["BookingNightsNum"] = (booking.CheckOutDateTime.Date - booking.CheckinDateTime.Date).TotalDays;
        dr["CheckinDate"] = booking.CheckinDateTime.Date.ToString("dd/MM/yyyy");
        dr["CheckoutDate"] = booking.CheckOutDateTime.Date.ToString("dd/MM/yyyy");
        dr["ContractDate"] = DateTime.Now.Date.ToString("dd/MM/yyyy");
        dr["City"] = accommodationNames.CityName;
        dr["Address"] = accommodationNames.Accommodation.Address;

        dr["HName"] = host.Name;
        dr["HSurname"] = host.Surname;
        dr["HBirthPlace"] = hostBirthPlace;
        dr["HBirthProvince"] = host.BirthProvince;
        dr["HBirthDate"] = host.BirthDate;
        dr["HDocType"] = hostDocument.DocumentType;
        dr["HDocSerialNum"] = hostDocument.Document.SerialNumber;
        //dr["HCodFisc"];
        dr["HPhonePrefix"] = host.PhonePrefix;
        dr["HPhoneNumber"] = host.PhoneNumber;
        dr["HEmail"] = host.Email;

        // Add the populated row to the contract data table.
        contractDt.Rows.Add(dr);
        // Return the row.
        return dr;
    }

    // Method to add the row to the booking details data table.
    private static DataRow addRowTobookingDetailsDt(List<Person> persons, Booking booking, AccommodationAccommodationNames accommodationNames, Room room, BookChannel channel)
    {
        // Create a new data table object.
        bookingDetailsDt = new DataTable();
        // Add Fields to contract data table.
        addFieldstoBookingDetailsDt();
        // Add a new row to the contract data table
        DataRow dr = bookingDetailsDt.NewRow();

        // Populate all the field created with the appropriate data.
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
        dr["CleaningFee"] = accommodationNames.Accommodation.CleaningFee;
        dr["TownFee"] = accommodationNames.Accommodation.TownFee;
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

        // Add the populated row to the contract data table.
        bookingDetailsDt.Rows.Add(dr);
        // Return the row.
        return dr;
    }

    // Method to add the fields to the booking details data table.
    private static void addFieldstoBookingDetailsDt()
    {
        // Add columns to the contract data table.
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

    // Method to add the row to the pre-checkin data table.
    private static DataRow addRowToPreCheckinDt(Person mainPerson, AccommodationAccommodationNames accommodationNames, Booking booking)
    {
        // Create a new data table object.
        preCheckinDt = new DataTable();
        // Add Fields to contract data table.
        addFieldstoPreCheckinDt();
        // Add a new row to the contract data table
        DataRow dr = preCheckinDt.NewRow();

        // Populate all the field created with the appropriate data.
        dr["Name"] = mainPerson.Name;
        dr["AccommodationName"] = accommodationNames.Accommodation.Name;
        dr["City"] = accommodationNames.CityName;
        dr["AccommodationWebsite"] = accommodationNames.Accommodation.Website;
        dr["CheckinDate"] = booking.CheckinDateTime.Date.ToString("dd-MM-yyyy");

        // Add the populated row to the contract data table.
        preCheckinDt.Rows.Add(dr);
        // Return the row.
        return dr;
    }

    // Method to add the fields to the booking details data table.
    private static void addFieldstoPreCheckinDt()
    {
        // Add columns to the contract data table.
        preCheckinDt.Columns.Add("Name");
        preCheckinDt.Columns.Add("AccommodationName");
        preCheckinDt.Columns.Add("City");
        preCheckinDt.Columns.Add("CheckinDate");
        preCheckinDt.Columns.Add("AccommodationWebsite");
    }
    #endregion
}