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
    public class BookingPersonController : Controller
    {
        private readonly MappBnBContext _context;

        public BookingPersonController(MappBnBContext context)
        {
            _context = context;
        }

        // GET: BookingPerson
        public async Task<IActionResult> Index()
        {
            return View(await _context.BookingPerson.ToListAsync());
        }

        // GET: BookingPerson/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookingPerson = await _context.BookingPerson
                .FirstOrDefaultAsync(m => m.id == id);
            if (bookingPerson == null)
            {
                return NotFound();
            }

            return View(bookingPerson);
        }

        // GET: BookingPerson/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BookingPerson/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,BookingID,PersonID")] BookingPerson bookingPerson)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bookingPerson);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bookingPerson);
        }

        // GET: BookingPerson/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookingPerson = await _context.BookingPerson.FindAsync(id);
            if (bookingPerson == null)
            {
                return NotFound();
            }
            return View(bookingPerson);
        }

        // POST: BookingPerson/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,BookingID,PersonID")] BookingPerson bookingPerson)
        {
            if (id != bookingPerson.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bookingPerson);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingPersonExists(bookingPerson.id))
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
            return View(bookingPerson);
        }

        // GET: BookingPerson/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookingPerson = await _context.BookingPerson
                .FirstOrDefaultAsync(m => m.id == id);
            if (bookingPerson == null)
            {
                return NotFound();
            }

            return View(bookingPerson);
        }

        // POST: BookingPerson/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bookingPerson = await _context.BookingPerson.FindAsync(id);
            if (bookingPerson != null)
            {
                _context.BookingPerson.Remove(bookingPerson);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingPersonExists(int id)
        {
            return _context.BookingPerson.Any(e => e.id == id);
        }
    }
}
