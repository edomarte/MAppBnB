using Microsoft.EntityFrameworkCore;
using MAppBnB.Data;
using SignalRChat.Hubs;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<MappBnBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'MappBnBContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Make keys persistent to work on different Docker containers
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/var/dpkeys"))
    .SetApplicationName("MAppBnB");

// Imposta la cultura globale come InvariantCulture
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHub<PersonSearchHub>("/personSearchHub");
app.MapHub<RoomSelectorHub>("/roomSelectorHub");
app.MapHub<CreateDocumentsHub>("/createDocumentsHub");
app.MapHub<DocumentTransmissionHub>("/docsTransmissionHub");
app.MapHub<CalendarHub>("/calendarHub");
app.MapHub<CountryProvincePlaceSelectorHub>("/countryProvincePlaceSelectorHub");

app.Run();