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
    public class BookChannelController : Controller
    {
        private readonly MappBnBContext _context;

        public BookChannelController(MappBnBContext context)
        {
            _context = context;
        }

        // GET: BookChannel
        public async Task<IActionResult> Index()
        {
            return View(await _context.BookChannel.ToListAsync());
        }

        // GET: BookChannel/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookChannel = await _context.BookChannel
                .FirstOrDefaultAsync(m => m.id == id);
            if (bookChannel == null)
            {
                return NotFound();
            }

            return View(bookChannel);
        }

        // GET: BookChannel/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BookChannel/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,Name,Fee")] BookChannel bookChannel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bookChannel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bookChannel);
        }

        // GET: BookChannel/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookChannel = await _context.BookChannel.FindAsync(id);
            if (bookChannel == null)
            {
                return NotFound();
            }
            return View(bookChannel);
        }

        // POST: BookChannel/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,Name,Fee")] BookChannel bookChannel)
        {
            if (id != bookChannel.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bookChannel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookChannelExists(bookChannel.id))
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
            return View(bookChannel);
        }

        // GET: BookChannel/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookChannel = await _context.BookChannel
                .FirstOrDefaultAsync(m => m.id == id);
            if (bookChannel == null)
            {
                return NotFound();
            }

            return View(bookChannel);
        }

        // POST: BookChannel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bookChannel = await _context.BookChannel.FindAsync(id);
            if (bookChannel != null)
            {
                _context.BookChannel.Remove(bookChannel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookChannelExists(int id)
        {
            return _context.BookChannel.Any(e => e.id == id);
        }
    }
}
