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
    public class DocumentController : Controller
    {
        private readonly MappBnBContext _context;

        public DocumentController(MappBnBContext context)
        {
            _context = context;
        }

        // GET: Document
        public async Task<IActionResult> Index()
        {
            return View(await _context.Document.ToListAsync());
        }

        // GET: Document/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Document
                .FirstOrDefaultAsync(m => m.id == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // GET: Document/DowloadFile
        public async Task<IActionResult> DownloadFile(int? id)
        {
            var document = await _context.Document.FirstOrDefaultAsync(m => m.id == id);

            return File(document.PdfCopy, "application/pdf", document.SerialNumber+".pdf");
        }

        // GET: Document/Create
        public IActionResult Create()
        {
            
            return View();
        }

        // POST: Document/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,SerialNumber,IssuedBy,IssuedDate,IssuingCountry,PdfCopy,PersonID")] Document document)
        {
            if (ModelState.IsValid)
            {
                _context.Add(document);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(document);
        }

        

        // GET: Document/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Document.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            return View(document);
        }

        // POST: Document/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,SerialNumber,IssuedBy,IssuedDate,IssuingCountry,PdfCopy,PersonID")] Document document)
        {
            if (id != document.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(document);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocumentExists(document.id))
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
            return View(document);
        }

        // GET: Document/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Document
                .FirstOrDefaultAsync(m => m.id == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // POST: Document/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var document = await _context.Document.FindAsync(id);
            if (document != null)
            {
                _context.Document.Remove(document);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult>DeletePDFCopy(int? id)
        {
            var document = await _context.Document.FirstOrDefaultAsync(m => m.id == id);
            document.PdfCopy = null;
            _context.Update(document);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(PersonController.Edit), "Person", new { id = document.PersonID });
        }

        private bool DocumentExists(int? id)
        {
            return _context.Document.Any(e => e.id == id);
        }
    }
}
