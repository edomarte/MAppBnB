using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MAppBnB.Data;
using SignalRChat.Hubs;
using DocumentFormat.OpenXml.Office2010.Excel;
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
            var query = _context.Booking
                        .Join(_context.Accommodation,
                            b => b.AccommodationID,
                            a => a.id,
                            (b, a) => new { b, AccommodationName = a.Name })
                        .Join(_context.Room,
                            ba => ba.b.RoomID,
                            r => r.id,
                            (ba, r) => new { ba.b, ba.AccommodationName, RoomName = r.Name })
                        .Join(_context.BookChannel,
                            bar => bar.b.ChannelID,
                            bc => bc.id,
                            (bar, bc) => new BookingAccommodationRoomChannelViewModel
                            {
                                Booking=bar.b,  // Booking properties
                                AccommodationName=bar.AccommodationName,
                                RoomName=bar.RoomName,
                                BookChannelName = bc.Name
                            })
                            .ToList();

            return View(query);
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
            ViewBag.ChannelName = _context.BookChannel.Find(booking.ChannelID).Name;
            ViewBag.AccommodationName = _context.Accommodation.Find(booking.AccommodationID).Name;
            ViewBag.RoomName = _context.Room.Find(booking.RoomID).Name;

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
                                                                                && x.CheckinDateTime.Date < model.Booking.CheckOutDateTime.Date
                                                                                && x.CheckOutDateTime.Date > model.Booking.CheckinDateTime.Date);
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
            else
            {
                model.PeopleInBooking = addPersonsToPeopleInBookingAsync(model, model.PersonIDs.Split(",")).Result;
            }
            ViewBag.AccommodationList = await _context.Accommodation.ToListAsync();
            ViewBag.ChannelList = await _context.BookChannel.ToListAsync();
            ViewBag.RoomAlreadyBooked = false;
            return View(model);
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
                viewModel.PeopleInBooking.Add(new PersonRoleNames() { Person = person, RoleName = _context.TipoAlloggiato.FirstOrDefault(x => x.Codice == person.RoleRelation).Descrizione });
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

                    var bpInList = addPersonsToPeopleInBooking(model.Booking.id, model.PersonIDs.Split(","));

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
            }
            else
            {
                model.PeopleInBooking = addPersonsToPeopleInBookingAsync(model, model.PersonIDs.Split(",")).Result;
            }
            ViewBag.AccommodationList = await _context.Accommodation.ToListAsync();
            ViewBag.ChannelList = await _context.BookChannel.ToListAsync();
            ViewBag.RoomAlreadyBooked = false;
            return View(model);
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
            
            ViewBag.ChannelName = _context.BookChannel.Find(booking.ChannelID).Name;
            ViewBag.AccommodationName = _context.Accommodation.Find(booking.AccommodationID).Name;
            ViewBag.RoomName = _context.Room.Find(booking.RoomID).Name;

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

        private List<BookingPerson> addPersonsToPeopleInBooking(int bookingId, string[] personIDs)
        {
            List<BookingPerson> bpInList = new List<BookingPerson>();
            foreach (var personID in personIDs)
            {
                if (personID == "") continue;

                var personBooking = new BookingPerson
                {
                    BookingID = bookingId,
                    PersonID = int.Parse(personID)
                };
                bpInList.Add(personBooking);

                if (!_context.BookingPerson.Any(x => x.BookingID == bookingId && x.PersonID == int.Parse(personID)))
                {
                    _context.BookingPerson.Add(personBooking);
                }

            }
            return bpInList;
        }

        private async Task<List<PersonRoleNames>> addPersonsToPeopleInBookingAsync(PersonBookingViewModel model, string[] personIDs)
        {
            List<PersonRoleNames> lpib = new List<PersonRoleNames>();
            foreach (string person in personIDs)
            {
                if (person == "") continue;
                var personDetails = await _context.Person.FirstOrDefaultAsync(x => x.id == Convert.ToInt32(person));
                lpib.Add(new PersonRoleNames() { Person = personDetails, RoleName = _context.TipoAlloggiato.Find(personDetails.RoleRelation).Descrizione });
            }
            return lpib;
        }
    }
}
