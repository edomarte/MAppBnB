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
    public class PersonController : Controller
    {
        private readonly MappBnBContext _context;

        public PersonController(MappBnBContext context)
        {
            _context = context;
        }

        // GET: Person
        public async Task<IActionResult> Index()
        {
            var documents = await _context.Document.ToListAsync();
            ViewData["DocumentList"] = documents;
            return View(await _context.Person.ToListAsync());
        }

        // GET: Person/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = await _context.Person
                .FirstOrDefaultAsync(m => m.id == id);
            if (person == null)
            {
                return NotFound();
            }

            var document = await _context.Document.FirstOrDefaultAsync(x => x.id == person.DocumentID);

            var viewModel = new PersonDocumentViewModel
            {
                Person = person,
                Document = document
            };

            return View(viewModel);
        }

        // GET: Person/Create
        public IActionResult Create()
        {
            var viewModel = new PersonDocumentViewModel
            {
                Person = new Person(),
                Document = new Document()
            };

            return View(viewModel);
        }

        // POST: Person/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PersonDocumentViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.PdfCopyPath != null && model.PdfCopyPath.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.PdfCopyPath.CopyToAsync(memoryStream);

                        model.Document.PdfCopy = memoryStream.ToArray();
                    }
                }
                _context.Add(model.Document);
                await _context.SaveChangesAsync();
                var document = await _context.Document.FirstOrDefaultAsync(x => x.SerialNumber == model.Document.SerialNumber && x.IssuingCountry == model.Document.IssuingCountry);
                model.Person.DocumentID = document.id;
                _context.Add(model.Person);
                await _context.SaveChangesAsync();
                model.Document.PersonID = model.Person.id;
                _context.Update(model.Document);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Person/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = await _context.Person.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }
            var document = await _context.Document.FirstOrDefaultAsync(x => x.id == person.DocumentID);

            var viewModel = new PersonDocumentViewModel
            {
                Person = person,
                Document = document
            };
            return View(viewModel);
        }

        // POST: Person/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PersonDocumentViewModel model)
        {
            if (id != model.Person.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (model.PdfCopyPath != null && model.PdfCopyPath.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.PdfCopyPath.CopyToAsync(memoryStream);

                        model.Document.PdfCopy = memoryStream.ToArray();
                    }
                }
                try
                {
                    if (model.Document.SerialNumber != null)
                        if (model.Document.id == null)
                        {
                            model.Document.PersonID = model.Person.id;
                            _context.Add(model.Document);
                            await _context.SaveChangesAsync();
                            var document = await _context.Document.FirstOrDefaultAsync(x => x.SerialNumber == model.Document.SerialNumber && x.IssuingCountry == model.Document.IssuingCountry);
                            model.Person.DocumentID = document.id;
                        }
                        else
                        {
                            _context.Update(model.Document);
                            await _context.SaveChangesAsync();
                        }
                    _context.Update(model.Person);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonExists(model.Person.id))
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
            return View(model);
        }

        // GET: Person/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = await _context.Person
                .FirstOrDefaultAsync(m => m.id == id);
            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        // POST: Person/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var person = await _context.Person.FindAsync(id);
            if (person != null)
            {
                _context.Person.Remove(person);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PersonExists(int id)
        {
            return _context.Person.Any(e => e.id == id);
        }
    }
}
