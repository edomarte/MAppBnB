// Import necessary namespaces
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
    // Controller to manage BookChannel entities
    public class BookChannelController : Controller
    {
        // Database context
        private readonly MappBnBContext _context;

        // Constructor with dependency injection of the context
        public BookChannelController(MappBnBContext context)
        {
            _context = context;
        }

        // GET: BookChannel
        // Shows a list of all BookChannel entries
        public async Task<IActionResult> Index()
        {
            return View(await _context.BookChannel.ToListAsync());
        }

        // GET: BookChannel/Details/5
        // Shows details of a specific BookChannel by ID
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Return 404 if no ID is provided
            }

            var bookChannel = await _context.BookChannel
                .FirstOrDefaultAsync(m => m.id == id); // Search for BookChannel by ID

            if (bookChannel == null)
            {
                return NotFound(); // Return 404 if not found
            }

            return View(bookChannel); // Render the detail view
        }

        // GET: BookChannel/Create
        // Shows the form to create a new BookChannel
        public IActionResult Create()
        {
            return View();
        }

        // POST: BookChannel/Create
        // Handles form submission to create a BookChannel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,Name,Fee")] BookChannel bookChannel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bookChannel); // Add new BookChannel to context
                await _context.SaveChangesAsync(); // Save changes to DB
                return RedirectToAction(nameof(Index)); // Redirect to list
            }

            return View(bookChannel); // Return form with validation errors if invalid
        }

        // GET: BookChannel/Edit/5
        // Displays the edit form for a specific BookChannel
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound(); // 404 if no ID provided
            }

            var bookChannel = await _context.BookChannel.FindAsync(id); // Find by ID
            if (bookChannel == null)
            {
                return NotFound(); // 404 if not found
            }

            return View(bookChannel); // Render edit form
        }

        // POST: BookChannel/Edit/5
        // Handles submission of edit form for a BookChannel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,Name,Fee")] BookChannel bookChannel)
        {
            if (id != bookChannel.id)
            {
                return NotFound(); // 404 if route ID and model ID don't match
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bookChannel); // Update entity in context
                    await _context.SaveChangesAsync(); // Save to DB
                }
                catch (DbUpdateConcurrencyException) // Handle concurrency issue
                {
                    if (!BookChannelExists(bookChannel.id))
                    {
                        return NotFound(); // If the entity was deleted during edit
                    }
                    else
                    {
                        throw; // Rethrow if another issue occurred
                    }
                }
                return RedirectToAction(nameof(Index)); // Redirect to list
            }

            return View(bookChannel); // Return form with errors if invalid
        }

        // GET: BookChannel/Delete/5
        // Displays confirmation page for deleting a BookChannel
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound(); // 404 if no ID provided
            }

            var bookChannel = await _context.BookChannel
                .FirstOrDefaultAsync(m => m.id == id); // Find entity by ID

            if (bookChannel == null)
            {
                return NotFound(); // 404 if not found
            }

            return View(bookChannel); // Show confirmation view
        }

        // POST: BookChannel/Delete/5
        // Handles deletion after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bookChannel = await _context.BookChannel.FindAsync(id);
            if (bookChannel != null)
            {
                _context.BookChannel.Remove(bookChannel); // Remove entity
            }

            await _context.SaveChangesAsync(); // Save changes
            return RedirectToAction(nameof(Index)); // Redirect to list
        }

        // Helper method to check if a BookChannel exists by ID
        private bool BookChannelExists(int id)
        {
            return _context.BookChannel.Any(e => e.id == id);
        }
    }
}
