// Import necessary namespaces
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MAppBnB.Data;
// Set Configuration to the MAppBnB.Models namespace to avoid conflicts with other Configuration classes
using Configuration = MAppBnB.Models.Configuration;

namespace MAppBnB.Controllers
{
    public class ConfigurationController : Controller
    {
        private readonly MappBnBContext _context;

        public ConfigurationController(MappBnBContext context)
        {
            _context = context;
        }

        #region ASP.NET Core actions
        // GET: Configuration/Index. Shows the configuration page with person and document details.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Get the information needed to populate the dropdowns in the view.
            ViewBag.Stati = _context.Stati.ToList();
            // Only the Host role must be available on Configuration (99=host)
            ViewBag.TipoAlloggiato = _context.TipoAlloggiato.Where(x => x.Codice.Equals(99)).ToList();
            ViewBag.TipoDocumento = _context.TipoDocumento.ToList();

            //Only one configuration line in the table
            var config = await _context.Configuration.FirstOrDefaultAsync();

            // If no configuration line in the table, create one with default values.
            if (config == null)
            {
                await _context.Configuration.AddAsync(new Configuration());
                await _context.SaveChangesAsync();
                config = await _context.Configuration.FirstOrDefaultAsync();
            }

            // Create a new viewmodel with the default values.
            var viewModel = new PersonDocumentConfigViewModel
            {
                Person = new Person(),
                Document = new Document(),
                Configuration = config
            };

            // If no person associated to Configuration, send empty person and document model to view.
            if (config.PersonID == null)
            {
                return View(viewModel);
            }
            else
            {
                // If a person is associated to Configuration, get the person and document from the database. If person does not exist, return error 404.
                var person = await _context.Person.FindAsync(config.PersonID);
                if (person == null)
                {
                    return NotFound();
                }
                // Get the document associated with the person.
                var document = await _context.Document.FirstOrDefaultAsync(x => x.id == config.DocumentID);
                // If document exists, add it to the viewmodel.
                if (document != null)
                {
                    viewModel.Document = document;
                }
                // Add the person to the viewmodel.
                viewModel.Person = person;

                // Pass province stored to the ViewBag
                var birthProvince = _context.Province
                .FirstOrDefault(p => p.Codice == person.BirthProvince);
                ViewBag.BirthProvince = birthProvince;
                // Pass birthplace stored to the ViewBag
                var birthPlace = _context.Comuni
                .FirstOrDefault(p => p.Codice == person.BirthPlace);
                ViewBag.BirthPlace = birthPlace;
            }
            // Pass the viewmodel to the View.
            return View(viewModel);
        }

        // POST: Person/Index; saves the updated configuration to the database.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(PersonDocumentConfigViewModel model)
        {
            // If the modelstate is valid (no validation errors in the form).
            if (ModelState.IsValid)
            {
                try
                {
                    // If there is a document in the viewmodel.
                    if (model.Document!=null && model.Document.SerialNumber != null)
                    {
                        // If there is no document in the configuration, add the document from the viewmodel to the database.
                        if (model.Configuration.DocumentID == null)
                        {
                            // Add the document to the database.
                            _context.Add(model.Document);
                            await _context.SaveChangesAsync();
                            // Link the document to the person and the configuration
                            model.Person.DocumentID = model.Document.id;
                            model.Configuration.DocumentID = model.Document.id;
                        }
                        else
                        {
                            // If there is already a document in the configuration, update it.
                            _context.Update(model.Document);
                            await _context.SaveChangesAsync();
                        }
                    }

                    // If there is no person in the configuration
                    if (model.Configuration.PersonID == null)
                    {
                        // Add person to the database.
                        _context.Add(model.Person);
                        await _context.SaveChangesAsync();
                        // Add the person id to the configuration
                        model.Configuration.PersonID = model.Person.id;
                    }
                    else
                    {
                        // If there is already a person in the configuration, update it.
                        _context.Update(model.Person);
                        await _context.SaveChangesAsync();
                    }
                   
                    // Update the fields of the configuration not related to person or document in the database.
                    _context.Update(model.Configuration);
                    await _context.SaveChangesAsync();
                }
                // Catch any concurrency exceptions that may occur during the update process.
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
                // Redirect to the Configuration index action after saving the changes.
                return RedirectToAction(nameof(Index));
            }
            // If the modelstate is not valid, return the view with the model to show validation errors.
            // Get the information needed to populate the dropdowns in the view.
            ViewBag.Stati = _context.Stati.ToList();
            ViewBag.TipoAlloggiato = _context.TipoAlloggiato.Where(x => x.Codice.Equals(99)).ToList(); //99=host
            ViewBag.TipoDocumento = _context.TipoDocumento.ToList();
            return View(model);
        }

        #endregion

        #region Support methods
        // Check if the person exists in the database.
        private bool PersonExists(int id)
        {
            return _context.Person.Any(e => e.id == id);
        }
        #endregion
    }
}
