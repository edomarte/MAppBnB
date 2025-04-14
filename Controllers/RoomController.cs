using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MAppBnB.Data;

namespace MAppBnB.Controllers
{
    public class RoomController : Controller
    {
        private readonly MappBnBContext _context;

        public RoomController(MappBnBContext context)
        {
            _context = context;
        }
        #region ASPNET MVC CRUD actions

        // GET: Room; Get all rooms with their accommodation names
        public async Task<IActionResult> Index()
        {
            // Using LINQ to join Room and Accommodation tables
            var query = _context.Room
                        .Join(_context.Accommodation,
                            r => r.AccommodationId,
                            a => a.id,
                            (r, a) => new RoomAccommodationViewModel
                            {
                                Room = r,  // Booking properties
                                AccommodationName = a.Name,
                            })
                            .ToList();

            // Return the result to the view.
            return View(query);
        }

        // GET: Room/Details; Get room details by id
        public async Task<IActionResult> Details(int? id)
        {
            // Check if the id is null and return NotFound if it is.
            if (id == null)
            {
                return NotFound();
            }

            // Find the room by id. Return Error 404 if not found.
            var room = await _context.Room
                .FirstOrDefaultAsync(m => m.id == id);
            if (room == null)
            {
                return NotFound();
            }

            // Include the accommodation name in the Viewbag
            ViewBag.AccommodationName = _context.Accommodation.Find(room.AccommodationId);

            // Return the room details to the view.
            return View(room);
        }

        // GET: Room/Create; Show the form to create a new room.
        public async Task<IActionResult> Create()
        {
            // Include the accommodation list in the Viewbag for the dropdown.
            ViewBag.AccommodationList = await _context.Accommodation.ToListAsync();

            return View();
        }

        // POST: Room/Create; Create a new room and save it to the database.
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,Name,Capacity,AccommodationId,BasicPrice")] Room room)
        {
            // Check if the model state is valid before saving to the database.
            if (ModelState.IsValid)
            {
                // Add the new room to the context and save changes.
                _context.Add(room);
                await _context.SaveChangesAsync();
                // Redirect to the Index action after successful creation.
                return RedirectToAction(nameof(Index));
            }
            // If the model state is not valid, return the same view with the room object.
            return View(room);
        }

        // GET: Room/Edit; Get the form to edit a room by id.
        public async Task<IActionResult> Edit(int? id)
        {
            // Check if the id is null and return NotFound if it is.
            if (id == null)
            {
                return NotFound();
            }

            // Include the accommodation list in the Viewbag for the dropdown.
            ViewBag.AccommodationList = await _context.Accommodation.ToListAsync();

            // Find the room by id. Return Error 404 if not found.
            var room = await _context.Room.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            // Return the view with the room object.
            return View(room);
        }

        // POST: Room/Edit/; Edit a room and save changes to the database.
        // To protect from overposting attacks, only the the specific properties to bind are enabled.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,Name,Capacity,AccommodationId,BasicPrice")] Room room)
        {
            // Check if the id is valid and matches the room id. Else, return Error 404.
            if (id != room.id)
            {
                return NotFound();
            }

            // Check if the model state is valid before saving to the database.
            if (ModelState.IsValid)
            {
                try
                {
                    // Update the room in the context and save changes in the database.
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                }
                // Handle concurrency exception.
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Redirect to the Index action after successful edit.
                return RedirectToAction(nameof(Index));
            }
            // If the model state is not valid, return the same view with the room object.
            return View(room);
        }

        // GET: Room/Delete/; Display the confirmation page for deleting a room by id.
        public async Task<IActionResult> Delete(int? id)
        {
            // Check if the id is null and return NotFound if it is.
            if (id == null)
            {
                return NotFound();
            }

            // Find the room by id. Return Error 404 if not found.
            var room = await _context.Room
                .FirstOrDefaultAsync(m => m.id == id);
            if (room == null)
            {
                return NotFound();
            }
            // Include the accommodation list in the Viewbag for the dropdown.
            ViewBag.AccommodationName = _context.Accommodation.Find(room.AccommodationId);
            // Return the view with the room object.
            return View(room);
        }

        // POST: Room/Delete/5; Delete a room by id and save changes to the database.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Check if the room exists in the database. If it does, remove it.
            var room = await _context.Room.FindAsync(id);
            if (room != null)
            {
                _context.Room.Remove(room);
            }

            await _context.SaveChangesAsync();
            // Redirect to the Index action after successful deletion.
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Helper methods
        // Check if a room exists by id.
        private bool RoomExists(int id)
        {
            return _context.Room.Any(e => e.id == id);
        }
        #endregion
    }
}
