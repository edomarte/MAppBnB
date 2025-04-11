using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MAppBnB;
using MAppBnB.Data;
using System.Threading.Channels;
using PdfSharp.Snippets;
using DocumentFormat.OpenXml.Math;
using Microsoft.IdentityModel.Tokens;

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
            if (!_context.Accommodation.IsNullOrEmpty())
            {
                ViewBag.AccommodationList = await _context.Accommodation.ToListAsync();
            }
            return View(new FinancialReportsDetailsViewModel());
        }

        // POST: FinancialReports/Details/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(FinancialReportsDetailsViewModel fr)
        {
            if (_context.Accommodation.IsNullOrEmpty())
            {
                ModelState.AddModelError("", "No accommodation found. Please add accommodation first.");
                return View(new FinancialReportsDetailsViewModel()); //TODO: to test
            }
            else
            {
                ViewBag.AccommodationList = await _context.Accommodation.ToListAsync();
            }

            if (!fr.DateFrom.HasValue)
            {
                ModelState.AddModelError("DateFrom", "Date From must have a value.");
                return View(new FinancialReportsDetailsViewModel());
            }

            if (!fr.DateTo.HasValue)
            {
                ModelState.AddModelError("DateTo", "Date To must have a value.");
                return View(new FinancialReportsDetailsViewModel());
            }

            if (fr.DateFrom.Value.Date > fr.DateTo.Value.Date)
            {
                ModelState.AddModelError("DateFrom", "Date From must be earlier than Date To.");
                return View(new FinancialReportsDetailsViewModel());
            }


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
                                 NightsBooked = g.Sum(x => (x.b.CheckOutDateTime - x.b.CheckinDateTime).TotalDays),
                                 TotalBookings = g.Count(),
                                 TotalRevenue = g.Sum(x => x.b.Price - x.b.Discount),
                                 AfterFeeRevenue = g.Sum(x => (x.b.Price - x.b.Discount) - ((x.b.Price - x.b.Discount) * x.bc.Fee))
                             })
                             .ToList(); // Materialize the result


                if (result.Count == 0)
                {
                    ModelState.AddModelError("", "No bookings found for the selected accommodation and date range.");
                    return View(new FinancialReportsDetailsViewModel());
                }
                
                fr.FinancialsByChannels = new List<FinancialsByChannel>();

                // Create a new instance of FinancialsByChannel for the total
                FinancialsByChannel allChannels= new FinancialsByChannel();
                allChannels.id = 0;
                allChannels.ChannelName = "All Channels";
                allChannels.TotNights = 0;
                allChannels.TotBookings = 0;
                allChannels.GrossRevenue = 0;
                allChannels.NetRevenue = 0;

                foreach (var chanl in result)
                {
                    FinancialsByChannel fbc = new FinancialsByChannel();
                    fbc.id = _context.BookChannel.FirstOrDefault(x => x.id == chanl.ChannelID).id;
                    fbc.ChannelName = _context.BookChannel.FirstOrDefault(x => x.id == chanl.ChannelID).Name;
                    fbc.TotNights = Convert.ToInt32(chanl.NightsBooked);
                    allChannels.TotNights += Convert.ToInt32(chanl.NightsBooked);
                    fbc.TotBookings = chanl.TotalBookings;
                    allChannels.TotBookings += chanl.TotalBookings;
                    fbc.GrossRevenue = chanl.TotalRevenue;
                    allChannels.GrossRevenue += chanl.TotalRevenue;
                    fbc.NetRevenue = chanl.AfterFeeRevenue;
                    allChannels.NetRevenue += chanl.AfterFeeRevenue;
                    fr.FinancialsByChannels.Add(fbc);
                }
                fr.FinancialsByChannels.Add(allChannels);
            }

            return View(fr);
        }

    }
}
