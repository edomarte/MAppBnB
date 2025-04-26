// Required namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MAppBnB;
using MAppBnB.Data;

namespace MAppBnB.Controllers
{
    // MVC Controller to manage Accommodation entities
    public class AccommodationController : Controller
    {
        // Database context to access the data
        private readonly MappBnBContext _context;

        // Constructor that injects the database context
        public AccommodationController(MappBnBContext context)
        {
            _context = context;
        }

        // GET: Accommodation
        // Returns a view with the list of all accommodations
        public async Task<IActionResult> Index()
        {   
            var accommodations=await _context.Accommodation.ToListAsync();
            var accommodationNames= getCountryProvinceCityNames(accommodations);
            return View(accommodationNames);
        }

        private List<AccommodationAccommodationNames>  getCountryProvinceCityNames(List<Accommodation> accommodations)
        {
            List<AccommodationAccommodationNames> accommodationNames = new List<AccommodationAccommodationNames>();
            foreach (var accommodation in accommodations)
            {
                var country = _context.Stati.FindAsync(accommodation.Country).Result.Descrizione ?? "";
                var province = _context.Province.FindAsync(accommodation.Province).Result.Descrizione ?? "";
                var city = country.Equals("ITALIA") ? _context.Comuni.FindAsync(accommodation.City).Result.Descrizione ?? "" : "ESTERO";

                accommodationNames.Add(new AccommodationAccommodationNames
                {
                    Accommodation = accommodation,
                    CountryName = country,
                    ProvinceName = province,
                    CityName = city
                });
            }
            return accommodationNames;
        }

        // GET: Accommodation/Details/5
        // Displays the details of a single accommodation based on ID
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // If ID is null, return 404
            }

            var accommodation = await _context.Accommodation
                .FirstOrDefaultAsync(m => m.id == id); // Fetch accommodation by ID

            if (accommodation == null)
            {
                return NotFound(); // Return 404 if not found
            }

            showIndividualAccommodation(accommodation);

            
            return View(accommodation); // Display details view
        }

        private void showIndividualAccommodation(Accommodation accommodation)
        {
            string country = _context.Stati.FindAsync(accommodation.Country).Result.Descrizione ?? "";
            ViewBag.Country = country;
            ViewBag.Province = _context.Province.FindAsync(accommodation.Province).Result.Descrizione ?? "";

            // If the country is Italy, get the description from the towns table (Comuni).
            if (country.Equals("ITALIA"))
            {
                ViewBag.City = _context.Comuni.FindAsync(accommodation.City).Result.Descrizione ?? "";
            }
            else
            {
                // If the country is not Italy, assign the City "ES".
                ViewBag.City = "ESTERO";
            }
        }

        // GET: Accommodation/Create
        // Displays the form to create a new accommodation
        public IActionResult Create()
        {
            // Set flag for "Gestione Appartamenti" (apartment management for police integration)
            if (_context.Configuration.FirstOrDefault() != null)
                ViewBag.IsGestioneAppartamenti = _context.Configuration.FirstOrDefault().IsGestioneAppartamenti;
            else
                ViewBag.IsGestioneAppartamenti = false;
            
            // Populate the ViewBag with the necessary data for the dropdowns in the view.
            ViewBag.Stati = _context.Stati.Where(x=> x.Descrizione == "Italia").ToList(); // Get the ID of the Italian state
            return View();
        }

        // POST: Accommodation/Create
        // Handles the submission of the create form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Accommodation accommodation)
        {
            if (ModelState.IsValid) // Check if form input is valid
            {
                _context.Add(accommodation); // Add accommodation to context
                await _context.SaveChangesAsync(); // Save changes to DB
                return RedirectToAction(nameof(Index)); // Redirect to list
            }

            // If model state is invalid, return form with existing input and flag
            ViewBag.IsGestioneAppartamenti = _context.Configuration.FirstOrDefault()?.IsGestioneAppartamenti ?? false;
            return View(accommodation);
        }

        // GET: Accommodation/Edit/5
        // Displays the form to edit an existing accommodation
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Return 404 if no ID provided
            }

            var accommodation = await _context.Accommodation.FindAsync(id);
            if (accommodation == null)
            {
                return NotFound(); // Return 404 if not found
            }

            // Set flag for "Gestione Appartamenti"
            if (_context.Configuration.FirstOrDefault() != null)
                ViewBag.IsGestioneAppartamenti = _context.Configuration.FirstOrDefault().IsGestioneAppartamenti;
            else
                ViewBag.IsGestioneAppartamenti = false;

            ViewBag.Stati = _context.Stati.Where(x=> x.Descrizione=="ITALIA").ToList(); // Get the ID of the Italian state
            return View(accommodation); // Return the edit form
        }

        // POST: Accommodation/Edit/5
        // Handles the submission of the edit form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,Name,Address,PostCode,City,Province,Country,Floor,UnitApartment,phone_prefix,phone_number,email,Website,CIN,CIR,CleaningFee,TownFee,AWIDAppartamento")] Accommodation accommodation)
        {
            if (id != accommodation.id)
            {
                return NotFound(); // Return 404 if ID mismatch
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(accommodation); // Update entity
                    await _context.SaveChangesAsync(); // Save to DB
                }
                catch (DbUpdateConcurrencyException) // Handle concurrency conflict
                {
                    if (!AccommodationExists(accommodation.id))
                    {
                        return NotFound(); // Return 404 if not found
                    }
                    else
                    {
                        throw; // Re-throw if unknown issue
                    }
                }
                return RedirectToAction(nameof(Index)); // Redirect to list
            }
            return View(accommodation); // If invalid, return form
        }

        // GET: Accommodation/Delete/5
        // Displays confirmation for deleting an accommodation
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Return 404 if no ID provided
            }

            var accommodation = await _context.Accommodation
                .FirstOrDefaultAsync(m => m.id == id); // Fetch entity

            if (accommodation == null)
            {
                return NotFound(); // Return 404 if not found
            }

            // Check if the accommodation is already in a booking.
            bool isAccommodationInBooking = _context.Booking.Any(x => x.AccommodationID == id);
            // If the person is in a booking, show an error message and redirect to the index page (cannot be deleted).
            if (isAccommodationInBooking)
            {
                // TempData is used to store data that can be accessed in the next request.
                TempData["Error"] = accommodation.Name + " is already in a booking. You cannot delete it.";
                return RedirectToAction(nameof(Index));
            }

             // Check if the accommodation is already associated to a room.
            bool isAccommodationInRoom = _context.Room.Any(x => x.AccommodationId == id);
            // If the accommodation is is already associated to a room, show an error message and redirect to the index page (cannot be deleted).
            if (isAccommodationInRoom)
            {
                // TempData is used to store data that can be accessed in the next request.
                TempData["Error"] = accommodation.Name + " is already associated to a room. You cannot delete it.";
                return RedirectToAction(nameof(Index));
            }

            showIndividualAccommodation(accommodation); // Show individual accommodation details
            return View(accommodation); // Show delete confirmation
        }

        // POST: Accommodation/Delete/5
        // Handles the confirmed deletion of an accommodation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var accommodation = await _context.Accommodation.FindAsync(id);
            if (accommodation != null)
            {
                _context.Accommodation.Remove(accommodation); // Remove entity
            }

            await _context.SaveChangesAsync(); // Save changes to DB
            return RedirectToAction(nameof(Index)); // Redirect to list
        }

        // Helper method to check if an accommodation with a specific ID exists
        private bool AccommodationExists(int id)
        {
            return _context.Accommodation.Any(e => e.id == id);
        }
    }
}
