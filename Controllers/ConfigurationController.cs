using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MAppBnB;
using MAppBnB.Data;
using Configuration = MAppBnB.Models.Configuration;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace MAppBnB.Controllers
{
    public class ConfigurationController : Controller
    {
        private readonly MappBnBContext _context;

        public ConfigurationController(MappBnBContext context)
        {
            _context = context;
        }

        // GET: Configuration/Index/5
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.Stati = _context.Stati.ToList();
            ViewBag.TipoAlloggiato = _context.TipoAlloggiato.Where(x => x.Codice.Equals("Host")).ToList();
            ViewBag.TipoDocumento = _context.TipoDocumento.ToList();

            //Only one configuration; row manually added to the database Configuration table
            int id = 1;
            var config = await _context.Configuration.FirstOrDefaultAsync(x => x.id == id);

            var viewModel = new PersonDocumentConfigViewModel
            {
                Person = new Person(),
                Document = new Document(),
                Configuration = config
            };

            // If no person associated to Configuration, send empty person and document model to id
            if (config.PersonID == null)
            {
                return View(viewModel);
            }
            else
            {
                var person = await _context.Person.FindAsync(config.PersonID);
                if (person == null)
                {
                    return NotFound();
                }
                var document = await _context.Document.FirstOrDefaultAsync(x => x.id == config.DocumentID);

                if (document != null)
                {
                    viewModel.Document = document;
                }

                viewModel.Person = person;

                // Pass province stored
                var birthProvince = _context.Province
                .Where(p => p.Codice == person.BirthProvince)
                .Select(p => new { p.Codice, p.Descrizione })
                .FirstOrDefault();
                ViewBag.BirthProvince = birthProvince; // Passa il valore alla view
                // Pass birthplace stored
                var birthPlace = _context.Comuni
                .Where(p => p.Codice == person.BirthPlace)
                .Select(p => new { p.Codice, p.Descrizione })
                .FirstOrDefault();
                ViewBag.BirthPlace = birthPlace; // Passa il valore alla view

            }

            return View(viewModel);
        }

        // POST: Person/Index/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(PersonDocumentConfigViewModel model)
        {
            //TODO BirthPlace and BirthProvince are null;
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.Configuration.PersonID == null)
                    {
                        // Add person to db and immediately get it to have its id.
                        _context.Add(model.Person);
                        await _context.SaveChangesAsync();
                        model.Person = await _context.Person.OrderBy(x => x.id).LastAsync();
                        model.Configuration.PersonID = model.Person.id;
                    }
                    else
                    {
                        _context.Update(model.Person);
                        await _context.SaveChangesAsync();
                    }

                    if (model.Document.SerialNumber != null)
                        if (model.Configuration.DocumentID == null)
                        {
                            model.Document.PersonID = model.Person.id;
                            _context.Add(model.Document);
                            await _context.SaveChangesAsync();
                            var document = await _context.Document.FirstOrDefaultAsync(x => x.SerialNumber == model.Document.SerialNumber && x.IssuingCountry == model.Document.IssuingCountry);
                            model.Person.DocumentID = document.id;
                            model.Configuration.DocumentID = document.id;
                        }
                        else
                        {
                            _context.Update(model.Document);
                            await _context.SaveChangesAsync();
                        }
                    _context.Update(model.Configuration);
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
            ViewBag.TipoAlloggiato = _context.TipoAlloggiato.Where(x => x.Codice.Equals("Host")).ToList();
            ViewBag.TipoDocumento = _context.TipoDocumento.ToList();
            return View(model);
        }

        private bool PersonExists(int id)
        {
            return _context.Person.Any(e => e.id == id);
        }
    }
}
