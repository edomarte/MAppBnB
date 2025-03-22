using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MAppBnB.Data;
using SignalRChat.Hubs;
namespace MAppBnB.Controllers
{
    public class BookingController : Controller
    {
        private readonly MappBnBContext _context;

        public BookingController(MappBnBContext context)
        {
            _context = context;
        }

        // GET: Booking
        public async Task<IActionResult> Index()
        {
            return View(await _context.Booking.ToListAsync());
        }

        // GET: Booking/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .FirstOrDefaultAsync(m => m.id == id);
            if (booking == null)
            {
                return NotFound();
            }

            var peopleOnBooking = await _context.BookingPerson.Where(x => x.BookingID == id).ToListAsync();
            var detailsPeopleOnBooking = await _context.Person
                .Where(p => peopleOnBooking.Select(b => b.PersonID).Contains(p.id))
                .ToListAsync();
            ViewBag.PeopleOnBooking = detailsPeopleOnBooking;

            return View(booking);
        }

        // GET: Booking/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new PersonBookingViewModel
            {
                Booking = new Booking(),
                PersonIDs = ""
            };

            ViewBag.AccommodationList = await _context.Accommodation.ToListAsync();
            ViewBag.ChannelList = await _context.BookChannel.ToListAsync();
            ViewBag.RoomAlreadyBooked = false;
            return View(viewModel);
        }

        // POST: Booking/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(PersonBookingViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingBooking = await _context.Booking.FirstOrDefaultAsync(x => x.RoomID == model.Booking.RoomID
                                                                                && x.CheckinDateTime.Value.Date < model.Booking.CheckOutDateTime.Value.Date
                                                                                && x.CheckOutDateTime.Value.Date > model.Booking.CheckinDateTime.Value.Date);
                if (existingBooking != null)
                {
                    ViewBag.AccommodationList = await _context.Accommodation.ToListAsync();
                    ViewBag.ChannelList = await _context.BookChannel.ToListAsync();
                    ViewBag.RoomAlreadyBooked = true;
                    return View(model);
                }

                _context.Add(model.Booking);
                await _context.SaveChangesAsync();
                var booking = await _context.Booking.FirstOrDefaultAsync(x => x.CheckinDateTime == model.Booking.CheckinDateTime && x.RoomID == model.Booking.RoomID);


                foreach (var personID in model.PersonIDs.Split(","))
                {
                    if (personID == "") continue;

                    var personBooking = new BookingPerson
                    {
                        BookingID = booking.id,
                        PersonID = int.Parse(personID)
                    };
                    _context.Add(personBooking);

                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Booking/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viewModel = new PersonBookingViewModel
            {
                Booking = new Booking(),
                PersonIDs = "",
                PeopleInBooking = new List<PersonRoleNames>()
            };

            viewModel.Booking = await _context.Booking.FirstOrDefaultAsync(x => x.id == id);
            var bookingPersons = await _context.BookingPerson.Where(x => x.BookingID == id).ToListAsync();
            foreach (var bookingPerson in bookingPersons)
            {
                var person = await _context.Person.FirstOrDefaultAsync(x => x.id == bookingPerson.PersonID);
                viewModel.PeopleInBooking.Add(new PersonRoleNames(){Person=person, RoleName=_context.TipoAlloggiato.FirstOrDefault(x => x.Codice == person.RoleRelation).Descrizione});
            }

            if (viewModel == null)
            {
                return NotFound();
            }

            ViewBag.AccommodationList = await _context.Accommodation.ToListAsync();
            ViewBag.ChannelList = await _context.BookChannel.ToListAsync();
            return View(viewModel);
        }

        // POST: Booking/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PersonBookingViewModel model)
        {
            if (id != model.Booking.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model.Booking);
                    await _context.SaveChangesAsync();

                    // TODO: find a linq way to extract only the ids in the string that are not already in the model.
                    //var currentbookingPersons = await _context.BookingPerson.Where(x => x.BookingID == model.Booking.id).ToListAsync(); 

                    var bpInList = new List<BookingPerson>();
                    foreach (var personID in model.PersonIDs.Split(","))
                    {
                        if (personID == "") continue;

                        var personBooking = new BookingPerson
                        {
                            BookingID = model.Booking.id,
                            PersonID = int.Parse(personID)
                        };
                        bpInList.Add(personBooking);

                        if (!_context.BookingPerson.Any(x => x.BookingID == model.Booking.id && x.PersonID == int.Parse(personID)))
                        {
                            _context.BookingPerson.Add(personBooking);
                        }

                    }
                    var allPersonInBooking = _context.BookingPerson.Where(x => x.BookingID == id).ToList();
                    var bp2beDeleted = allPersonInBooking.Except(bpInList, new BookingPersonComparer()).ToList();
                    _context.BookingPerson.RemoveRange(bp2beDeleted);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(model.Booking.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Booking/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .FirstOrDefaultAsync(m => m.id == id);
            if (booking == null)
            {
                return NotFound();
            }

            var peopleOnBooking = await _context.BookingPerson.Where(x => x.BookingID == id).ToListAsync();
            var detailsPeopleOnBooking = await _context.Person
                .Where(p => peopleOnBooking.Select(b => b.PersonID).Contains(p.id))
                .ToListAsync();
            ViewBag.PeopleOnBooking = detailsPeopleOnBooking;

            return View(booking);
        }

        // POST: Booking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking != null)
            {
                _context.Booking.Remove(booking);
            }

            _context.BookingPerson.RemoveRange(_context.BookingPerson.Where(x => x.BookingID == id));

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.id == id);
        }
    }
}
