using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MAppBnB;
using MAppBnB.Data;
using System.Threading.Channels;
using PdfSharp.Snippets;
using DocumentFormat.OpenXml.Math;

namespace MAppBnB.Controllers
{
    public class FinancialReportsController : Controller
    {
        private readonly MappBnBContext _context;

        public FinancialReportsController(MappBnBContext context)
        {
            _context = context;
        }

        // GET: Document
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var accomodationName = await _context.Accommodation.ToListAsync();
            ViewData["AccommodationList"] = accomodationName;
            return View(new FinancialReportsDetailsViewModel());
        }

        // POST: FinancialReports/Details/5
        [HttpPost]
        public async Task<IActionResult> Index(FinancialReportsDetailsViewModel fr)
        {
            var accomodationName = await _context.Accommodation.ToListAsync();
            ViewData["AccommodationList"] = accomodationName;

            if (fr != null)
            {


                if (fr.AccommodationID == null)
                {
                    return NotFound();
                }



                // First, fetch the necessary data into memory
                var bookings = _context.Booking
                                 .Join(_context.BookChannel, b => b.ChannelID, bc => bc.id, (b, bc) => new { b, bc })
                                 .Where(x => x.b.AccommodationID == fr.AccommodationID
                                            && x.b.CheckinDateTime > fr.DateFrom
                                            && x.b.CheckinDateTime < fr.DateTo)
                                 .ToList(); // Fetches data into memory

                // Now, process the aggregation in-memory
                var result = bookings
                             .GroupBy(x => x.b.ChannelID)
                             .Select(g => new
                             {
                                 ChannelID = g.Key,
                                 NightsBooked = g.Sum(x => (x.b.CheckOutDateTime - x.b.CheckinDateTime).Value.TotalDays),
                                 TotalBookings = g.Count(),
                                 TotalRevenue = g.Sum(x => x.b.Price - x.b.Discount),
                                 AfterFeeRevenue = g.Sum(x => (x.b.Price - x.b.Discount) - ((x.b.Price - x.b.Discount) * x.bc.Fee))
                             })
                             .ToList(); // Materialize the result


                if (result == null)
                {
                    return NotFound();
                }

                fr.FinancialsByChannels = new List<FinancialsByChannel>();

                foreach (var chanl in result)
                {
                    FinancialsByChannel fbc = new FinancialsByChannel();
                    fbc.id = _context.BookChannel.FirstOrDefault(x => x.id == chanl.ChannelID).id;
                    fbc.ChannelName = _context.BookChannel.FirstOrDefault(x => x.id == chanl.ChannelID).Name;
                    fbc.TotNights = Convert.ToInt32(chanl.NightsBooked);
                    fbc.TotBookings = chanl.TotalBookings;
                    fbc.GrossRevenue = chanl.TotalRevenue;
                    fbc.NetRevenue = chanl.AfterFeeRevenue;
                    fr.FinancialsByChannels.Add(fbc);
                }
            }

            return View(fr);
        }

        // GET: FinancialReport/DowloadFile
        public async Task<IActionResult> DownloadFile(int? id)
        {
            //TODO:
            var document = await _context.Document.FirstOrDefaultAsync(m => m.id == id);

            return File(document.PdfCopy, "application/pdf", document.SerialNumber + ".pdf");
        }
    }
}
