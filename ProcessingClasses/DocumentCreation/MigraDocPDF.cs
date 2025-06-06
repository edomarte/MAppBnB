using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

public class MigraDocPDF
{
    // Converts a Word document (.docx) to a MigraDoc Document and saves it as a PDF
    public static MigraDoc.DocumentObjectModel.Document ConvertWordToPdf(string docPath, string pdfPath)
    {
        var doc = new MigraDoc.DocumentObjectModel.Document();
        var section = doc.AddSection();
        section.PageSetup.PageFormat = PageFormat.A4; //A4 page
        doc.Styles["Normal"].Font.Name = "DejaVu Sans"; // <- Add this!

        // Open the Word document in read-only mode
        using (var wordDoc = WordprocessingDocument.Open(docPath, false))
        {
            var body = wordDoc.MainDocumentPart.Document.Body;
            ApplyPageSetup(wordDoc, section); // Apply page margins from Word to MigraDoc

            // Process each paragraph in the Word document
            foreach (var para in body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
            {
                // Add a paragraph
                var migraPara = section.AddParagraph();
                ApplyParagraphFormatting(para, migraPara, section); // Apply paragraph-level formatting

                // Process each run (text span) within the paragraph
                foreach (var run in para.Elements<Run>())
                {
                    // Skip MERGEFIELD placeholders (Avoid duplication)
                    if (!run.InnerText.Contains("MERGEFIELD"))
                    {
                        // Add formatted text
                        var text = migraPara.AddFormattedText(run.InnerText);
                        ApplyTextFormatting(run, text); // Apply text-level formatting
                    }
                }
            }
        }

        // Render the PDF from the MigraDoc document
        PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true);
        pdfRenderer.Document = doc;
        pdfRenderer.RenderDocument();

        // Save the generated PDF
        pdfRenderer.PdfDocument.Save(pdfPath);
        // Return the file.
        return doc;
    }

    // Applies page margins from the Word document to the MigraDoc section
    static void ApplyPageSetup(WordprocessingDocument wordDoc, Section section)
    {
        // Set margins to 1 inch by default, or use the values from Word if available
        var margins = wordDoc.MainDocumentPart.Document.Body.GetFirstChild<SectionProperties>()?.GetFirstChild<PageMargin>();
        if (margins != null)
        {
            section.PageSetup.LeftMargin = Unit.FromPoint(margins.Left / 20);
            section.PageSetup.RightMargin = Unit.FromPoint(margins.Right / 20);
            section.PageSetup.TopMargin = Unit.FromPoint(margins.Top / 20);
            section.PageSetup.BottomMargin = Unit.FromPoint(margins.Bottom / 20);
        }
        else
        {
            section.PageSetup.LeftMargin = Unit.FromInch(1);   // Default 1 inch margin
            section.PageSetup.RightMargin = Unit.FromInch(1);  // Default 1 inch margin
            section.PageSetup.TopMargin = Unit.FromInch(1);    // Default 1 inch margin
            section.PageSetup.BottomMargin = Unit.FromInch(1); // Default 1 inch margin
        }
    }

    // Applies formatting from a Word paragraph to a MigraDoc paragraph
    static void ApplyParagraphFormatting(DocumentFormat.OpenXml.Wordprocessing.Paragraph para, MigraDoc.DocumentObjectModel.Paragraph migraPara, Section section)
    {
        // Get the paragraph properties.
        var props = para.ParagraphProperties;
        if (props != null)
        {
            // Apply justification (alignments)
            if (props.Justification != null)
            {
                if (props.Justification.Val.Value == JustificationValues.Center)
                {
                    migraPara.Format.Alignment = ParagraphAlignment.Center;
                }
                else if (props.Justification.Val.Value == JustificationValues.Right)
                {
                    migraPara.Format.Alignment = ParagraphAlignment.Right;
                }
                else if (props.Justification.Val.Value == JustificationValues.Both)
                {
                    migraPara.Format.Alignment = ParagraphAlignment.Justify;
                }
                else
                {
                    migraPara.Format.Alignment = ParagraphAlignment.Left;
                }
            }

            // Apply spacing (before, after, and line spacing)
            if (props.SpacingBetweenLines != null)
            {
                migraPara.Format.SpaceBefore = Unit.FromPoint(props.SpacingBetweenLines.Before != null ? Convert.ToDouble(props.SpacingBetweenLines.Before) : 0);
                migraPara.Format.SpaceAfter = Unit.FromPoint(props.SpacingBetweenLines.After != null ? Convert.ToDouble(props.SpacingBetweenLines.After) : 0);
                migraPara.Format.LineSpacing = Unit.FromPoint(props.SpacingBetweenLines.Line != null ? Convert.ToDouble(props.SpacingBetweenLines.Line) : 0);
            }

            // Apply indentation (respect page width and margins)
            if (props.Indentation != null)
            {
                 double firstLineIndent = props.Indentation.FirstLine != null ? Convert.ToDouble(props.Indentation.FirstLine) : 0;

                // Ensure the left indent doesn't exceed the available space (set to 0 because indentation is already applied at document level apart from first line).
                migraPara.Format.LeftIndent = Unit.FromPoint(0);
                migraPara.Format.RightIndent = Unit.FromPoint(0);
                migraPara.Format.FirstLineIndent = Unit.FromPoint(firstLineIndent);
            }

            // Apply tab stops
            if (props.Tabs != null)
            {
                foreach (var tab in props.Tabs.Elements<DocumentFormat.OpenXml.Wordprocessing.TabStop>())
                {
                    // For each tab, align according to its property.
                    TabAlignment align;
                    if (tab.Val == TabStopValues.Center)
                    {
                        align = TabAlignment.Center;
                    }
                    else if (tab.Val == TabStopValues.Right)
                    {
                        align = TabAlignment.Right;
                    }
                    else
                    {
                        align = TabAlignment.Left;
                    }
                    // Add a tab stop.
                    migraPara.Format.TabStops.AddTabStop(Unit.FromPoint(tab.Position), align);
                }
            }

            // Apply shading (background color)
            if (props.Shading != null && !string.IsNullOrEmpty(props.Shading.Fill))
            {
                migraPara.Format.Shading.Color = ConvertHexToColor(props.Shading.Fill);
            }

            // Apply paragraph borders
            if (props.ParagraphBorders != null)
            {
                var border = props.ParagraphBorders.TopBorder;
                if (border != null && border.Color != null)
                {
                    migraPara.Format.Borders.Top.Color = ConvertHexToColor(border.Color);
                }
            }
        }
    }

    // Applies character formatting from a Word Run to a MigraDoc FormattedText object
    static void ApplyTextFormatting(Run run, FormattedText text)
    {
        var props = run.RunProperties;
        if (props != null)
        {
            text.Bold = props.Bold != null;
            text.Italic = props.Italic != null;
            text.Underline = props.Underline != null ? MigraDoc.DocumentObjectModel.Underline.Single : MigraDoc.DocumentObjectModel.Underline.None;
            text.Size = props.FontSize != null ? Convert.ToDouble(props.FontSize.Val) / 2 : 12;

            if (props.Color != null && !string.IsNullOrEmpty(props.Color.Val))
            {
                text.Color = ConvertHexToColor(props.Color.Val);
            }

            if (props.RunFonts != null && props.RunFonts.Ascii != null)
            {
                text.Font.Name = props.RunFonts.Ascii?.Value ?? "Times New Roman";
            }
        }
    }

    // Converts a hex color code (e.g., "FF0000") to a MigraDoc Color
    static MigraDoc.DocumentObjectModel.Color ConvertHexToColor(string hex)
    {
        if (hex.Length == 6)
        {
            return new MigraDoc.DocumentObjectModel.Color(
                Convert.ToByte(hex.Substring(0, 2), 16),
                Convert.ToByte(hex.Substring(2, 2), 16),
                Convert.ToByte(hex.Substring(4, 2), 16)
            );
        }
        return Colors.Black;
    }
}
