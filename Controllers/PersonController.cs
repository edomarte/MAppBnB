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
    //TODO: se host, non farlo vedere nella lista delle persone. Qui e nell'hub.
    //TODO: se è capofamiglia, capogruppo, o ospite singolo, documento obbligatorio
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
            var query = _context.Person
                        .Join(_context.Document,
                            p => p.DocumentID,
                            d => d.id,
                            (p, d) => new PersonDocumentViewModel
                            {
                                Person = p,  // Booking properties
                                Document = d
                            })
                            .ToList();

            return View(query);
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

            // Find description for the following fields depending on country.
            string birthCountry = _context.Stati.FindAsync(person.BirthCountry).Result.Descrizione ?? "";
            ViewBag.BirthCountry = birthCountry;
            ViewBag.BirthProvince = person.BirthProvince ?? "";
            ViewBag.DocumentType = _context.TipoDocumento.FindAsync(document.DocumentType).Result.Descrizione ?? "";
            ViewBag.RoleRelation = _context.TipoAlloggiato.FindAsync(person.RoleRelation).Result.Descrizione ?? "";
            if (birthCountry.Equals("ITALIA"))
            {
                ViewBag.BirthPlace = _context.Comuni.FindAsync(person.BirthPlace).Result.Descrizione ?? "";
                ViewBag.IssuingCountry = _context.Comuni.FindAsync(document.IssuingCountry).Result.Descrizione ?? "";

            }
            else
            {
                ViewBag.BirthPlace = person.BirthPlace;
                ViewBag.IssuingCountry = _context.Stati.FindAsync(document.IssuingCountry).Result.Descrizione ?? "";
            }


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

            ViewBag.Stati = _context.Stati.ToList();
            ViewBag.TipoAlloggiato = _context.TipoAlloggiato.ToList();
            ViewBag.TipoDocumento = _context.TipoDocumento.ToList();

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
                if (model.Document != null)
                {
                    addPdfToDbAsync(model);
                    _context.Add(model.Document);
                    await _context.SaveChangesAsync();
                    var document = await _context.Document.FirstOrDefaultAsync(x => x.SerialNumber == model.Document.SerialNumber && x.IssuingCountry == model.Document.IssuingCountry);
                    model.Person.DocumentID = document.id;
                }

                _context.Add(model.Person);
                await _context.SaveChangesAsync();

                if (model.Document != null)
                {
                    model.Document.PersonID = model.Person.id;
                    _context.Update(model.Document);
                    await _context.SaveChangesAsync();
                }

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

            ViewBag.Stati = _context.Stati.ToList();

            ViewBag.TipoAlloggiato = _context.TipoAlloggiato.ToList();
            ViewBag.TipoDocumento = _context.TipoDocumento.ToList();

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
                try
                {
                    if (model.Document.SerialNumber != null)
                        addPdfToDbAsync(model);
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

            ViewBag.Stati = _context.Stati.ToList();
            ViewBag.TipoAlloggiato = _context.TipoAlloggiato.ToList();
            ViewBag.TipoDocumento = _context.TipoDocumento.ToList();
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

            var document = await _context.Document.FirstOrDefaultAsync(x => x.id == person.DocumentID);

            var viewModel = new PersonDocumentViewModel
            {
                Person = person,
                Document = document
            };

            // Find description for the following fields depending on country.
            string birthCountry = _context.Stati.FindAsync(person.BirthCountry).Result.Descrizione ?? "";
            if (birthCountry.Equals("ITALIA"))
            {
                ViewBag.BirthPlace = _context.Comuni.FindAsync(person.BirthPlace).Result.Descrizione ?? "";
                ViewBag.IssuingCountry = _context.Comuni.FindAsync(document.IssuingCountry).Result.Descrizione ?? "";

            }
            else
            {
                ViewBag.BirthPlace = person.BirthPlace;
                ViewBag.IssuingCountry = _context.Stati.FindAsync(document.IssuingCountry).Result.Descrizione ?? "";
            }
            ViewBag.BirthCountry = birthCountry;
            ViewBag.BirthProvince = person.BirthProvince ?? "";
            ViewBag.DocumentType = _context.TipoDocumento.FindAsync(document.DocumentType).Result.Descrizione ?? "";
            ViewBag.RoleRelation = _context.TipoAlloggiato.FindAsync(person.RoleRelation).Result.Descrizione ?? "";

            return View(viewModel);
        }

        // POST: Person/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var person = await _context.Person.FindAsync(id);
            if (person != null)
            {
                if (person.DocumentID != null)
                {
                    var document = await _context.Document.FirstOrDefaultAsync(x => x.id == person.DocumentID);
                    _context.Document.Remove(document);
                }
                _context.Person.Remove(person);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PersonExists(int id)
        {
            return _context.Person.Any(e => e.id == id);
        }

        protected async Task addPdfToDbAsync(PersonDocumentViewModel model)
        {
            if (model.PdfCopyPath != null && model.PdfCopyPath.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await model.PdfCopyPath.CopyToAsync(memoryStream);

                    model.Document.PdfCopy = memoryStream.ToArray();
                }
            }
        }
    }
}
