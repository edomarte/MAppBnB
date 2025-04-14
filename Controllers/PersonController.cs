using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        #region ASP.NET Core actions
        // GET: Person/Index; Show data on all the persons in the database within the index page.
        public async Task<IActionResult> Index()
        {
            // Get all persons and their documents, excluding the one with RoleRelation 99 (the host).
            // Use GroupJoin to join Person and Document tables, and SelectMany to flatten the result.
            var query = _context.Person
                        .GroupJoin(
                            _context.Document,
                            p => p.DocumentID,
                            d => d.id,
                            (p, docs) => new { Person = p, Documents = docs.DefaultIfEmpty() } // DefaultIfEmpty() is used to include persons without documents.
                        )
                        .SelectMany(
                            x => x.Documents,
                            (x, d) => new PersonDocumentViewModel
                            {
                                Person = x.Person,
                                Document = d
                            }
                        )
                        .Where(x => x.Person.RoleRelation != 99)
                        .ToList();

            // Add BirthcountryName and RoleName for each person in the list.
            var personList = await getFieldsNamesAsync(query);
            // Return the view with the list of persons.
            return View(personList);
        }


        // GET: Person/Details/; Shows the details of a person.
        public async Task<IActionResult> Details(int? id)
        {
            // Gather all the information about the person and their document.
            PersonDocumentViewModel viewModel = await showIndividualPersonAsync(id);
            // If the person is not found, return a 404 error.
            if (viewModel == null)
            {
                return NotFound();
            }
            // Return the view with the person details.
            return View(viewModel);
        }


        // GET: Person/Create; Shows the form to create a new person.
        public IActionResult Create()
        {
            // Create a new PersonDocumentViewModel object to hold the person and document data.
            var viewModel = new PersonDocumentViewModel
            {
                Person = new Person(),
                Document = new Document()
            };

            // Populate the ViewBag with the necessary data for the dropdowns in the view.
            ViewBag.Stati = _context.Stati.ToList();
            ViewBag.TipoAlloggiato = _context.TipoAlloggiato.ToList();
            ViewBag.TipoDocumento = _context.TipoDocumento.ToList();

            // Return the view with the empty viewModel.
            return View(viewModel);
        }

        // POST: Person/Create; Saves the new person to the database.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PersonDocumentViewModel model)
        {
            // Check if the model state is valid.
            if (ModelState.IsValid)
            {
                // Check if the modelview contains a document.
                if (model.Document != null)
                {
                    // Check if all document filds are filled.
                    if (model.Document.SerialNumber != null && model.Document.DocumentType != null && model.Document.IssuingCountry != null)
                    {
                        // Add the PDF document to the object (if present).
                        addPdfToDbAsync(model);
                        // Add the document to the database and connect it to the person.
                        _context.Add(model.Document);
                        await _context.SaveChangesAsync();
                        model.Person.DocumentID = model.Document.id;
                    }
                    else
                    {
                        // If the document fields are not filled, add an error to the model state and return to the view.
                        ModelState.AddModelError("", "All document fields are required.");
                        // Populate the ViewBag with the necessary data for the dropdowns in the view.
                        ViewBag.Stati = _context.Stati.ToList();
                        ViewBag.TipoAlloggiato = _context.TipoAlloggiato.ToList();
                        ViewBag.TipoDocumento = _context.TipoDocumento.ToList();
                        return View(model);
                    }
                }
                // Add the person to the database.
                _context.Add(model.Person);
                await _context.SaveChangesAsync();

                // Redirect to the index page after saving the person.
                return RedirectToAction(nameof(Index));
            }
            // Populate the ViewBag with the necessary data for the dropdowns in the view.
            ViewBag.Stati = _context.Stati.ToList();
            ViewBag.TipoAlloggiato = _context.TipoAlloggiato.ToList();
            ViewBag.TipoDocumento = _context.TipoDocumento.ToList();
            // Return the view with the model containing validation errors.
            return View(model);
        }

        // GET: Person/Edit; Shows the form to edit a person.
        public async Task<IActionResult> Edit(int? id)
        {
            // Check if the id is null and return a 404 error if it is.
            if (id == null)
            {
                return NotFound();
            }
            // Find the person in the database using the id. Return Error 404 if not found.
            var person = await _context.Person.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }
            // Find the document associated with the person.
            var document = await _context.Document.FirstOrDefaultAsync(x => x.id == person.DocumentID);

            // Create a new PersonDocumentViewModel object to hold the person and document data.
            var viewModel = new PersonDocumentViewModel
            {
                Person = person,
                Document = document
            };

            // Populate the ViewBag with the necessary data for the dropdowns in the view.
            ViewBag.Stati = _context.Stati.ToList();
            ViewBag.TipoAlloggiato = _context.TipoAlloggiato.ToList();
            ViewBag.TipoDocumento = _context.TipoDocumento.ToList();

            // Return the view with the populated viewmodel.
            return View(viewModel);
        }

        // POST: Person/Edit; Saves the edited person to the database.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PersonDocumentViewModel model)
        {
            // Check if the id is valid and matches the person id in the model.
            if (id != model.Person.id)
            {
                return NotFound();
            }
            // Check if the model state is valid.
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if the modelview contains a document.
                    if (model.Document.id != null)
                    {
                        // Check if the person is a secondary guest (FAMILIARE, MEMBRO GRUPPO).
                        if (model.Person.RoleRelation == 19 || model.Person.RoleRelation == 20) // Secondary guest (FAMILIARE, MEMBRO GRUPPO)
                        {
                            // If it is and the document is not null, remove the document from the database (Secondary guests have no document); they were created as main guests first.
                            model.Person.DocumentID = null;
                            _context.Remove(model.Document);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            // If it is not a secondary guest, check if all document fields are filled.
                            if (model.Document.SerialNumber != null && model.Document.DocumentType != null && model.Document.IssuingCountry != null)
                            {
                                // If all fields are filled, add the PDF document to the object (if present).
                                addPdfToDbAsync(model);

                                // Check if the document is new (id is null) or existing (id is not null). Add or update the document to the database accordingly.
                                if (model.Document.id == null)
                                {
                                    _context.Add(model.Document);
                                    await _context.SaveChangesAsync();
                                    // Update the person with the new document id.
                                    model.Person.DocumentID = model.Document.id;
                                }
                                else
                                {
                                    _context.Update(model.Document);
                                    await _context.SaveChangesAsync();
                                }

                            }
                            else
                            {
                                // If the document fields are not filled, add an error to the model state and return to the view.
                                ModelState.AddModelError("", "All document fields are required.");
                                ViewBag.Stati = _context.Stati.ToList();
                                ViewBag.TipoAlloggiato = _context.TipoAlloggiato.ToList();
                                ViewBag.TipoDocumento = _context.TipoDocumento.ToList();
                                return View(model);
                            }

                        }

                    }
                    // Update the person.
                    _context.Update(model.Person);
                    await _context.SaveChangesAsync();
                }
                // Handle concurrency exception.
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
                // Redirect to the Person index page.
                return RedirectToAction(nameof(Index));
            }
            // If the modelState is invalid, return the view with the model data and the populated viewbag for the dropdowns.
            ViewBag.Stati = _context.Stati.ToList();
            ViewBag.TipoAlloggiato = _context.TipoAlloggiato.ToList();
            ViewBag.TipoDocumento = _context.TipoDocumento.ToList();
            return View(model);
        }

        // GET: Person/Delete; Shows the form to delete a person.
        public async Task<IActionResult> Delete(int? id)
        {
            // Check if the id is null and return a 404 error if it is.
            if (id == null)
            {
                return NotFound();
            }
            // Check if the person is already in a booking (junction table BookingPerson).
            bool isPersonInBooking = _context.BookingPerson.Any(x => x.PersonID == id);
            // Get the person data from the database.
            Person person = await _context.Person.FindAsync(id);
            // If the person is in a booking, show an error message and redirect to the index page (cannot be deleted).
            if (isPersonInBooking)
            {
                // TempData is used to store data that can be accessed in the next request.
                TempData["Error"] = person.Name + " " + person.Surname + " is already in a booking. You cannot delete it.";
                return RedirectToAction(nameof(Index));
            }
            // Add the person data to the viewbag to show in the delete confirmation view.
            PersonDocumentViewModel viewModel = await showIndividualPersonAsync(id);

            // Return the view with the person data.
            return View(viewModel);
        }

        // POST: Person/Delete; Deletes the person from the database.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Find the person in the database.
            var person = await _context.Person.FindAsync(id);
            // If the person exists, remove the associated document (if any) and the person itself from the database. If not, throw a 404 error.
            if (person != null)
            {
                if (person.DocumentID != null)
                {
                    var document = await _context.Document.FirstOrDefaultAsync(x => x.id == person.DocumentID);
                    _context.Document.Remove(document);
                }
                _context.Person.Remove(person);
            }
            else
            {
                return NotFound();
            }

            // Save the changes to the database.
            await _context.SaveChangesAsync();
            // Redirect to the index page after deleting the person.
            return RedirectToAction(nameof(Index));
        }
        #endregion


        #region Helper methods
        // Check if a person with the given id exists in the database.
        private bool PersonExists(int id)
        {
            return _context.Person.Any(e => e.id == id);
        }

        // Add the PDF document to the model if it exists. 
        protected async Task addPdfToDbAsync(PersonDocumentViewModel model)
        {
            if (model.PdfCopyPath != null && model.PdfCopyPath.Length > 0)
            {
                // MemoryStream is used to convert the file to a byte array.
                using (var memoryStream = new MemoryStream())
                {
                    // Copy the file to the model 'PdfCopyPath' property.
                    await model.PdfCopyPath.CopyToAsync(memoryStream);
                    // Convert the MemoryStream to a byte array and assign it to the Document's PdfCopy property.
                    model.Document.PdfCopy = memoryStream.ToArray();
                }
            }
        }

        // Get the description of the fields from the database and assign them to the viewmodel.
        private async Task<List<PersonDocumentViewModel>> getFieldsNamesAsync(List<PersonDocumentViewModel> query)
        {
            // Loop through each person in the query.
            foreach (PersonDocumentViewModel pdv in query)
            {
                // Get the description of the fields from the database and assign them to the viewmodel.
                pdv.BirthCountryName = _context.Stati.Find(pdv.Person.BirthCountry).Descrizione;
                pdv.RoleName = _context.TipoAlloggiato.Find(pdv.Person.RoleRelation).Descrizione;
                // If the birth country is Italy, get the description from the towns table (Comuni).
                if (pdv.BirthCountryName.Equals("ITALIA"))
                {
                    pdv.BirthPlaceName = _context.Comuni.Find(pdv.Person.BirthPlace).Descrizione;
                }
                else
                {
                    // If the birth country is not Italy, assign the birth place directly to the viewmodel (birthPlace==birthCountry).
                    pdv.BirthPlaceName = pdv.Person.BirthPlace;
                }

            }
            // Return the updated viewmodel with the descriptions.
            return query;
        }

        // Get the details of a person and their document from the database. Used to convert codes to descriptions.
        private async Task<PersonDocumentViewModel> showIndividualPersonAsync(int? id)
        {
            // Check if the id is null and return null if it is.
            if (id == null)
            {
                return null;
            }
            // Find the person in the database using the id. Return null if not found.
            var person = await _context.Person
                .FirstOrDefaultAsync(m => m.id == id);
            if (person == null)
            {
                return null;
            }
            // Find the document associated with the person.
            var document = await _context.Document.FirstOrDefaultAsync(x => x.id == person.DocumentID);

            // Create a new PersonDocumentViewModel object to hold the person and document data.
            var viewModel = new PersonDocumentViewModel
            {
                Person = person,
                Document = document
            };

            // Find description for the following fields depending on country. Add them to the ViewBag to show in the view.
            string birthCountry = _context.Stati.FindAsync(person.BirthCountry).Result.Descrizione ?? "";
            ViewBag.BirthCountry = birthCountry;
            ViewBag.BirthProvince = person.BirthProvince ?? "";
            ViewBag.RoleRelation = _context.TipoAlloggiato.FindAsync(person.RoleRelation).Result.Descrizione ?? "";

            // If the birth country is Italy, get the description from the towns table (Comuni).
            if (birthCountry.Equals("ITALIA"))
            {
                ViewBag.BirthPlace = _context.Comuni.FindAsync(person.BirthPlace).Result.Descrizione ?? "";
            }
            else
            {
                // If the birth country is not Italy, assign the birth place directly to the viewmodel (birthPlace==birthCountry).
                ViewBag.BirthPlace = person.BirthPlace;
            }

            // If document is null, set default values for DocumentType and IssuingCountry.
            if (document == null)
            {
                ViewBag.DocumentType = "";
                ViewBag.IssuingCountry = "";
            }
            else
            {
                // If document is not null, get the description from the database and assign it to the viewmodel.
                ViewBag.DocumentType = _context.TipoDocumento.FindAsync(document.DocumentType).Result.Descrizione ?? "";

                // If the issuing country code starts with 4, it means it's a town in Italy (Comuni).
                if (document.IssuingCountry[0] == '4')
                { // All Towns id starts with 4.
                    ViewBag.IssuingCountry = _context.Comuni.FindAsync(document.IssuingCountry).Result.Descrizione ?? "";
                }
                else
                {
                    // If the issuing country code does not start with 4, get the description from the states (Stati) table.
                    ViewBag.IssuingCountry = _context.Stati.FindAsync(document.IssuingCountry).Result.Descrizione ?? "";
                }
            }

            // Get the Sex description from the enumerator according to the value in the database.
            ViewBag.Sex = Enum.GetName(typeof(Sex), person.Sex) ?? "";
            // Return the viewmodel with the person and document data.
            return viewModel;
        }
        #endregion
    }
}