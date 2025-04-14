// import namespaces
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MAppBnB.Data;
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

        // GET: Document. Index page for Financial Reports
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // If there are accommodations, populate the ViewBag with the list of accommodations
            if (!_context.Accommodation.IsNullOrEmpty())
            {
                ViewBag.AccommodationList = await _context.Accommodation.ToListAsync();
            }else{
                ModelState.AddModelError("", "No accommodation found. Please add accommodation first.");
                return View(new FinancialReportsDetailsViewModel());
            }
            // Return a new instance of FinancialReportsDetailsViewModel to the view
            return View(new FinancialReportsDetailsViewModel());
        }

        // POST: FinancialReports/Index. Handles the form submission for financial reports and returns the results.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(FinancialReportsDetailsViewModel fr)
        {
            // If there are no accommodations, return a general model error and a new instance of FinancialReportsDetailsViewModel
            if (_context.Accommodation.IsNullOrEmpty())
            {
                ModelState.AddModelError("", "No accommodation found. Please add accommodation first.");
                return View(new FinancialReportsDetailsViewModel());
            }
            else
            {
                // Else, populate the ViewBag with the list of accommodations
                ViewBag.AccommodationList = await _context.Accommodation.ToListAsync();
            }

            // If datefrom or dateto is not provided, add model errors and return a new instance of FinancialReportsDetailsViewModel
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

            // If the datefrom is greater than dateto, add model error and return a new instance of FinancialReportsDetailsViewModel
            if (fr.DateFrom.Value.Date > fr.DateTo.Value.Date)
            {
                ModelState.AddModelError("DateFrom", "Date From must be earlier than Date To.");
                return View(new FinancialReportsDetailsViewModel());
            }

            // If the viewmodel is valorized
            if (fr != null)
            {
                // If the accommodation ID is not provided, return NotFound
                if (fr.AccommodationID == null)
                {
                    return NotFound();
                }

                // Fetch the necessary data on booking and book channel given the accommodation and the dates.
                var bookings = _context.Booking
                                 .Join(_context.BookChannel, b => b.ChannelID, bc => bc.id, (b, bc) => new { b, bc })
                                 .Where(x => x.b.AccommodationID == fr.AccommodationID
                                            && x.b.CheckinDateTime > fr.DateFrom
                                            && x.b.CheckinDateTime < fr.DateTo)
                                 .ToList(); // Fetches data into memory

                // Now, process the create new objects grouped by BookChannel, with the information needed given the result of the join.
                var result = bookings
                             .GroupBy(x => x.b.ChannelID)
                             .Select(g => new FinancialReportResult()
                             {
                                 ChannelID = g.Key,
                                 NightsBooked = g.Sum(x => (x.b.CheckOutDateTime - x.b.CheckinDateTime).TotalDays),
                                 TotalBookings = g.Count(),
                                 TotalRevenue = g.Sum(x => x.b.Price - x.b.Discount),
                                 AfterFeeRevenue = g.Sum(x => x.b.Price - x.b.Discount - ((x.b.Price - x.b.Discount) * x.bc.Fee))
                             })
                             .ToList(); // Materialize the result

                // If the result is empty, add a model error and return a new instance of FinancialReportsDetailsViewModel
                if (result.Count == 0)
                {
                    ModelState.AddModelError("", "No bookings found for the selected accommodation and date range.");
                    return View(new FinancialReportsDetailsViewModel());
                }
                // Populate the FinancialReportsDetailsViewModel with the results.
                fr=addFinancialByChannelsToReport(fr, result);
            }
            // Return the populated FinancialReportsDetailsViewModel to the view
            return View(fr);
        }

        // Method to add financials by channels to the report
        private FinancialReportsDetailsViewModel addFinancialByChannelsToReport(FinancialReportsDetailsViewModel fr, List<FinancialReportResult> result)
        {       
                // Initialize the FinancialsByChannels property of the FinancialReportsDetailsViewModel
                fr.FinancialsByChannels = new List<FinancialsByChannel>();

                // Create a new instance of FinancialsByChannel for the allChannels line and populate it.
                FinancialsByChannel allChannels= new FinancialsByChannel();
                allChannels.id = 0;
                allChannels.ChannelName = "All Channels";
                allChannels.TotNights = 0;
                allChannels.TotBookings = 0;
                allChannels.GrossRevenue = 0;
                allChannels.NetRevenue = 0;

                // Iterate through the result and populate the FinancialsByChannels property with the data. Add the values for the individual channels to the allChannels object.
                foreach (FinancialReportResult chanl in result)
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
                // Add the allChannel object at the end of the field FinancialsByChannels.
                fr.FinancialsByChannels.Add(allChannels);

                return fr;
        }
    }

// Class to represent the financial report result.
    internal class FinancialReportResult
    {
        public FinancialReportResult()
        {
        }

        public int? ChannelID { get; set; }
        public double NightsBooked { get; set; }
        public int TotalBookings { get; set; }
        public decimal? TotalRevenue { get; set; }
        public decimal? AfterFeeRevenue { get; set; }
    }
}
