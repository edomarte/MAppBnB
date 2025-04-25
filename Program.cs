using Microsoft.EntityFrameworkCore;
using MAppBnB.Data;
using SignalRChat.Hubs;
using System.Globalization;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Configura il contesto DbContext utilizzando SQL Server e la stringa di connessione definita in appsettings.json
builder.Services.AddDbContext<MappBnBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Connection string 'MappBnBContext' not found.")));

// Aggiunge i servizi MVC (controller + views) all'app
builder.Services.AddControllersWithViews();

// Aggiunge il supporto per SignalR per la comunicazione in tempo reale
builder.Services.AddSignalR();

// Imposta l'host per ascoltare tutte le interfacce sulla porta 5000
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Configura la protezione dei dati per rendere le chiavi persistenti tra container Docker diversi
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/var/dpkeys")) // Percorso in cui salvare le chiavi
    .SetApplicationName("MAppBnB"); // Nome dell'app usato per isolare le chiavi

// Clone the InvariantCulture to create a custom culture
var customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();

// Set date format
customCulture.DateTimeFormat.ShortDatePattern = "dd-MM-yyyy";
customCulture.DateTimeFormat.LongDatePattern = "dd-MM-yyyy"; // Optional, for long format too

// Set currency symbol to Euro
customCulture.NumberFormat.CurrencySymbol = "€";
customCulture.NumberFormat.CurrencyDecimalDigits = 2;
customCulture.NumberFormat.CurrencyDecimalSeparator = ",";
customCulture.NumberFormat.CurrencyGroupSeparator = ".";

// Apply globally
CultureInfo.DefaultThreadCurrentCulture = customCulture;
CultureInfo.DefaultThreadCurrentUICulture = customCulture;

var app = builder.Build();

// Configura la pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    // In produzione, usa un gestore di errori generico
    app.UseExceptionHandler("/Home/Error");
    // Abilita HSTS per migliorare la sicurezza HTTPS
    app.UseHsts();
}

// Forza il reindirizzamento da HTTP a HTTPS
app.UseHttpsRedirection();

// Abilita il routing per le richieste HTTP
app.UseRouting();

// Abilita l'autorizzazione (anche se non viene specificata autenticazione in questo file)
app.UseAuthorization();

// Mappa eventuali asset statici (metodo personalizzato, presumibilmente definito altrove)
app.MapStaticAssets();

// Definisce la route predefinita per i controller MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets(); // Includi asset statici nella route MVC (metodo custom)

// Mappa gli hub SignalR alle rispettive rotte WebSocket
app.MapHub<PersonSearchHub>("/personSearchHub");
app.MapHub<RoomSelectorHub>("/roomSelectorHub");
app.MapHub<CreateDocumentsHub>("/createDocumentsHub");
app.MapHub<DocumentTransmissionHub>("/docsTransmissionHub");
app.MapHub<CalendarHub>("/calendarHub");
app.MapHub<CountryProvincePlaceSelectorHub>("/countryProvincePlaceSelectorHub");

// Avvia l'applicazione
app.Run();
