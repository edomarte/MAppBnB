// Import necessary namespaces
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MAppBnB.Data;

namespace MAppBnB.Controllers
{
    public class BookingController : Controller
    {
        private readonly MappBnBContext _context;

        public BookingController(MappBnBContext context)
        {
            _context = context;
        }

        #region ASP.NET Core MVC actions
        // GET: Booking (Lista prenotazioni). Pass the information to the Index Booking page.
        public async Task<IActionResult> Index()
        {
            // Get bookings from database context including names associated with the ids for accommodation, room, and bookchannel and store them in the appropriate viewmodel.
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
                                Booking = bar.b,
                                AccommodationName = bar.AccommodationName,
                                RoomName = bar.RoomName,
                                BookChannelName = bc.Name
                            })
                            .ToList();

            return View(query);
        }

        // GET: Booking/Details/; Shows details of a single booking. Pass data to the Booking details view.
        public async Task<IActionResult> Details(int? id)
        {
            // Check if booking id is not null. If null, returns error 404.
            if (id == null) return NotFound();

            // Get booking associated to id parameter. If null, returns error 404.
            var booking = await _context.Booking.FirstOrDefaultAsync(m => m.id == id);
            if (booking == null) return NotFound();

            // Get people associated to this booking on the BookingPerson junction table and get details of them from the Person table.
            var peopleOnBooking = await _context.BookingPerson.Where(x => x.BookingID == id).ToListAsync();
            var detailsPeopleOnBooking = await _context.Person
                .Where(p => peopleOnBooking.Select(b => b.PersonID).Contains(p.id))
                .ToListAsync();

            // Get supporting information for the view.
            // Get the names of the accommodation, room, and channel associated with this booking.
            ViewBag.PeopleOnBooking = detailsPeopleOnBooking;
            ViewBag.ChannelName = _context.BookChannel.Find(booking.ChannelID)?.Name;
            ViewBag.AccommodationName = _context.Accommodation.Find(booking.AccommodationID)?.Name;
            ViewBag.RoomName = _context.Room.Find(booking.RoomID)?.Name;

            return View(booking);
        }

        // GET: Booking/Create. Shows the form to create a new booking. Pass data to the Booking creation view.
        public async Task<IActionResult> Create()
        {
            // Create a new instance of PersonBookingViewModel to hold the booking information. Discount is set to 0 by default. 
            var viewModel = new PersonBookingViewModel
            {
                Booking = new Booking { Discount = 0 },
                PersonIDs = ""
            };

            // Get the information needed to populate the dropdowns in the view.
            ViewBag.AccommodationList = await _context.Accommodation.ToListAsync();
            ViewBag.ChannelList = await _context.BookChannel.ToListAsync();
            ViewBag.RoomAlreadyBooked = false;

            return View(viewModel);
        }

        // POST: Booking/Create. Handles form submission to create a new booking.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(PersonBookingViewModel model)
        {
            // If the modelstate is valid (no validation errors in the form).
            if (ModelState.IsValid)
            {
                // Verify if the room is already booked for the selected dates.
                var existingBooking = await _context.Booking.FirstOrDefaultAsync(x =>
                    x.RoomID == model.Booking.RoomID &&
                    x.CheckinDateTime.Date < model.Booking.CheckOutDateTime.Date &&
                    x.CheckOutDateTime.Date > model.Booking.CheckinDateTime.Date);

                // If the room is already booked, set the RoomAlreadyBooked flag to true and return the view with the model.
                if (existingBooking != null)
                {
                    // Get the information needed to populate the dropdowns in the view.
                    ViewBag.AccommodationList = await _context.Accommodation.ToListAsync();
                    ViewBag.ChannelList = await _context.BookChannel.ToListAsync();
                    ViewBag.RoomAlreadyBooked = true;
                    return View(model);
                }

                // Save booking in the database
                _context.Add(model.Booking);
                await _context.SaveChangesAsync();

                // Add people to the booking. Input received from the View is a comma-separated string of person IDs.
                foreach (var personID in model.PersonIDs.Split(","))
                {
                    // Skip empty IDs (the string closes with a comma, so last element is empty).
                    if (personID == "") continue;

                    // Add the BookingPerson object to the junction table.
                    var personBooking = new BookingPerson
                    {
                        BookingID = model.Booking.id,
                        PersonID = int.Parse(personID)
                    };
                    _context.Add(personBooking);
                }
                // Save changes to the database.
                await _context.SaveChangesAsync();
                // Redirect the user to the Index action of the controller.
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // If model not valid, still populate the PeopleInBooking field of the model to retain the people added in the form (otherwise, page refresh and people selection to redo).
                model.PeopleInBooking = addPersonsToPeopleInBookingAsync(model.PersonIDs.Split(",")).Result;
            }

            // Get the information needed to populate the dropdowns in the view.
            ViewBag.AccommodationList = await _context.Accommodation.ToListAsync();
            ViewBag.ChannelList = await _context.BookChannel.ToListAsync();
            ViewBag.RoomAlreadyBooked = false;

            // Return the view with the model.
            return View(model);
        }

        // GET: Booking/Edit/. Show the form to edit a booking. Pass data to the Booking edit view.
        public async Task<IActionResult> Edit(int? id)
        {
            // If the id parameter is null, show error 404.
            if (id == null) return NotFound();

            // Get the booking from the database given the id. If not found, show error 404.
            var booking = await _context.Booking.FirstOrDefaultAsync(x => x.id == id);
            if (booking == null) return NotFound();

            // Create the viewmodel and initialize empty fields.
            var viewModel = new PersonBookingViewModel
            {
                Booking = booking,
                // Necessary for the post action.
                PersonIDs = "",
                // Necessary for the view to show the people associated with this booking.
                PeopleInBooking = new List<PersonRoleNames>()
            };

            // Get the list of people associated with this booking and add them to the viewmodel.
            viewModel = addPeopleToPeopleInBookingFromDBAsync(viewModel, id).Result;

            // Get the information needed to populate the dropdowns in the view.
            ViewBag.AccommodationList = await _context.Accommodation.ToListAsync();
            ViewBag.ChannelList = await _context.BookChannel.ToListAsync();

            // Send the data to the View.
            return View(viewModel);
        }



        // POST: Booking/Edit. Handles form submission to edit a booking.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PersonBookingViewModel model)
        {
            // If the Booking id is not the same as the id parameter, show error 404.
            if (id != model.Booking.id) return NotFound();

            // If the modelstate is valid (no validation errors in the form).
            if (ModelState.IsValid)
            {
                try
                {
                    // Update the booking details on the database
                    _context.Update(model.Booking);
                    await _context.SaveChangesAsync();

                    // Get the list of people in the booking selected in the View by the user. Add the ones in the user input but not in the junction table.
                    // Input received from the View is a comma-separated string of person IDs.
                    var bpInList = addPersonsToPeopleInBooking(model.Booking.id, model.PersonIDs.Split(","));
                    // Get the list of people in the booking from the BookingPerson junction table.
                    var allPersonInBooking = _context.BookingPerson.Where(x => x.BookingID == id).ToList();
                    // Get the list of people in the booking that are in the junction table but not in the user input. 
                    var bp2beDeleted = allPersonInBooking.Except(bpInList, new BookingPersonComparer()).ToList();
                    //Delete them from the junction table. Save the changes in the database.
                    _context.BookingPerson.RemoveRange(bp2beDeleted);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // If the booking was deleted by another user, show error 404.
                    if (!BookingExists(model.Booking.id)) return NotFound();
                    throw;
                }
            }

            // Populate the PeopleInBooking field of the model to retain the people added in the form (otherwise, page refresh and people selection to redo).
            model.PeopleInBooking = addPersonsToPeopleInBookingAsync(model.PersonIDs.Split(",")).Result;
            // Get the information needed to populate the dropdowns in the view.
            ViewBag.AccommodationList = await _context.Accommodation.ToListAsync();
            ViewBag.ChannelList = await _context.BookChannel.ToListAsync();
            ViewBag.RoomAlreadyBooked = false;

            // Return the current page with the updated model.
            return View(model);
        }

        // GET: Booking/Delete/5. Shows the confirmation page for deleting a booking.
        public async Task<IActionResult> Delete(int? id)
        {
            // If no booking correspond to the parameter id, show error 404.
            if (id == null) return NotFound();

            // Get data on the booking from the database. If not found, show error 404.
            var booking = await _context.Booking.FirstOrDefaultAsync(m => m.id == id);
            if (booking == null) return NotFound();

            // Get the list of people associated with this booking from the BookingPerson junction table, and the details of each person. 
            var peopleOnBooking = await _context.BookingPerson.Where(x => x.BookingID == id).ToListAsync();
            var detailsPeopleOnBooking = await _context.Person
                .Where(p => peopleOnBooking.Select(b => b.PersonID).Contains(p.id))
                .ToListAsync();

            // Populate ViewBag with the details of the booking and the people associated with it.
            ViewBag.PeopleOnBooking = detailsPeopleOnBooking;
            ViewBag.ChannelName = _context.BookChannel.Find(booking.ChannelID)?.Name;
            ViewBag.AccommodationName = _context.Accommodation.Find(booking.AccommodationID)?.Name;
            ViewBag.RoomName = _context.Room.Find(booking.RoomID)?.Name;

            // Return the view with the data.
            return View(booking);
        }

        // POST: Booking/DeleteConfirmed. Perform the delete action of the booking.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Get the booking from the database given the id. If not found, show error 404.
            var booking = await _context.Booking.FindAsync(id);
            if (booking != null)
            {
                _context.Booking.Remove(booking);
            }

            // Eliminate the records associated with this booking from the BookingPerson junction table. Save the changes on the database
            _context.BookingPerson.RemoveRange(_context.BookingPerson.Where(x => x.BookingID == id));
            await _context.SaveChangesAsync();

            // Redirect the user to the Index action of the controller.
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Support functions

        // Check if a booking with the given id exists in the database.
        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.id == id);
        }

        // Add people associated with this booking to the viewmodel and to the junction table BookingPerson. 
        // This is necessary for the view to show the people associated with this booking.
        private List<BookingPerson> addPersonsToPeopleInBooking(int bookingId, string[] personIDs)
        {
            List<BookingPerson> bpInList = new List<BookingPerson>();
            // For each person id in the list.
            foreach (var personID in personIDs)
            {
                // Skip empty IDs (the string closes with a comma, so last element is empty).
                if (personID == "") continue;

                // Create instance of BookingPerson and valorize it.
                var personBooking = new BookingPerson
                {
                    BookingID = bookingId,
                    PersonID = int.Parse(personID)
                };
                // Add the BookingPerson object to the list bpInList.
                bpInList.Add(personBooking);

                // If the BookingPerson object is not already in the junction table, add it to the database.
                if (!_context.BookingPerson.Any(x => x.BookingID == bookingId && x.PersonID == int.Parse(personID)))
                {
                    _context.BookingPerson.Add(personBooking);
                }
            }
            //Return the bpInList list.
            return bpInList;
        }

        // Add people from an array of strings with person IDs to a list of PersonRoleNames.
        private async Task<List<PersonRoleNames>> addPersonsToPeopleInBookingAsync(string[] personIDs)
        {
            List<PersonRoleNames> lpib = new List<PersonRoleNames>();
            // For each person id in the list.
            foreach (string person in personIDs)
            {
                // Skip empty IDs (the string closes with a comma, so last element is empty).
                if (person == "") continue;
                // Find the person details and the role name in the database.
                var personDetails = await _context.Person.FirstOrDefaultAsync(x => x.id == Convert.ToInt32(person));
                var role = _context.TipoAlloggiato.Find(personDetails.RoleRelation)?.Descrizione;
                // Add a new PersonRoleNames object to the list lpib.
                lpib.Add(new PersonRoleNames { Person = personDetails, RoleName = role });
            }
            // Return the list of PersonRoleNames objects.
            return lpib;
        }

        // Add people associated with this booking to the viewmodel. This is necessary for the view to show the people associated with this booking.
        private async Task<PersonBookingViewModel> addPeopleToPeopleInBookingFromDBAsync(PersonBookingViewModel viewModel, int? id)
        {
            // Get the list of people associated with this booking from the BookingPerson junction table.
            var bookingPersons = await _context.BookingPerson.Where(x => x.BookingID == id).ToListAsync();
            // For each person in the list, get the details from the Person table and the name of their role from the TipoAlloggiato table and add them to the viewmodel both in PersonsIDs and in PeopleinBooking.
            foreach (var bookingPerson in bookingPersons)
            {
                // Find the person details and the role name in the database.
                var person = await _context.Person.FirstOrDefaultAsync(x => x.id == bookingPerson.PersonID);
                var role = _context.TipoAlloggiato.FirstOrDefault(x => x.Codice == person.RoleRelation)?.Descrizione;
                // Add a new PersonRoleNames object to the list lpibPeopleInBooking list in the model.
                viewModel.PeopleInBooking.Add(new PersonRoleNames { Person = person, RoleName = role });
                // Add a new PersonID string to the PersonID field of the model.
                viewModel.PersonIDs += $"{bookingPerson.PersonID},";
            }
            // Return the viewmodel.
            return viewModel;
        }
        #endregion
    }
}
