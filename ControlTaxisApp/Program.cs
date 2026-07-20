using ControlTaxisApp.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<VehiculoService>();
var cultureInfo = new CultureInfo("es-CO");
cultureInfo.NumberFormat.CurrencySymbol = "$"; // Asegura el símbolo de pesos
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
// --- 1. CONFIGURACIÓN DINÁMICA DE SQLite ---
string dbPath;
if (builder.Environment.IsProduction())
{
    // Ruta en el contenedor de Fly.io (donde está el volumen)
    dbPath = "/app/data/ControlTaxis.db";
}
else
{
    // Ruta en tu PC local
    dbPath = "ControlTaxis.db";
}

// REGISTRO ÚNICO DEL CONTEXTO DE BASE DE DATOS
builder.Services.AddDbContext<ControlTaxisContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// --- 2. CONFIGURACIÓN DE EPPLUS ---
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// --- 3. SERVICIOS MVC Y AUTENTICACIÓN ---
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Acceso/Login";
    });

var app = builder.Build();

// --- 4. CREACIÓN AUTOMÁTICA DE LA BASE DE DATOS ---
// Esto asegura que si el archivo .db no existe en la carpeta /app/data, se cree al iniciar
using (var scope = app.Services.CreateScope())
{ 
    var db = scope.ServiceProvider.GetRequiredService<ControlTaxisContext>();
    db.Database.Migrate();

}

// --- 5. PIPELINE HTTP ---
if (!app.Environment.IsProduction()) // Usamos !IsProduction para ver errores en desarrollo
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Login}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ControlTaxisContext>();

    // 1. Limpiamos la tabla para asegurar que siempre esté actualizada
    context.Database.ExecuteSqlRaw("DELETE FROM Festivos");

    // 2. Definimos la lista completa
    var todosLosFestivos = new List<Festivo>
    {
        // Festivos 2025
        new Festivo { Fecha = new DateTime(2025, 1, 1), Nombre = "Año Nuevo" },
        new Festivo { Fecha = new DateTime(2025, 1, 6), Nombre = "Día de los Reyes Magos" },
        new Festivo { Fecha = new DateTime(2025, 3, 24), Nombre = "Día de San José" },
        new Festivo { Fecha = new DateTime(2025, 4, 17), Nombre = "Jueves Santo" },
        new Festivo { Fecha = new DateTime(2025, 4, 18), Nombre = "Viernes Santo" },
        new Festivo { Fecha = new DateTime(2025, 5, 1), Nombre = "Día del Trabajo" },
        new Festivo { Fecha = new DateTime(2025, 6, 2), Nombre = "Día de la Ascensión" },
        new Festivo { Fecha = new DateTime(2025, 6, 23), Nombre = "Corpus Christi" },
        new Festivo { Fecha = new DateTime(2025, 6, 30), Nombre = "Sagrado Corazón" },
        new Festivo { Fecha = new DateTime(2025, 7, 20), Nombre = "Día de la Independencia" },
        new Festivo { Fecha = new DateTime(2025, 8, 7), Nombre = "Batalla de Boyacá" },
        new Festivo { Fecha = new DateTime(2025, 8, 18), Nombre = "Asunción de la Virgen" },
        new Festivo { Fecha = new DateTime(2025, 10, 13), Nombre = "Día de la Raza" },
        new Festivo { Fecha = new DateTime(2025, 11, 3), Nombre = "Todos los Santos" },
        new Festivo { Fecha = new DateTime(2025, 11, 17), Nombre = "Independencia de Cartagena" },
        new Festivo { Fecha = new DateTime(2025, 12, 8), Nombre = "Inmaculada Concepción" },
        new Festivo { Fecha = new DateTime(2025, 12, 25), Nombre = "Navidad" },

        // Festivos 2026
        new Festivo { Fecha = new DateTime(2026, 1, 1), Nombre = "Año Nuevo" },
        new Festivo { Fecha = new DateTime(2026, 1, 12), Nombre = "Día de los Reyes Magos" },
        new Festivo { Fecha = new DateTime(2026, 3, 23), Nombre = "Día de San José" },
        new Festivo { Fecha = new DateTime(2026, 4, 2), Nombre = "Jueves Santo" },
        new Festivo { Fecha = new DateTime(2026, 4, 3), Nombre = "Viernes Santo" },
        new Festivo { Fecha = new DateTime(2026, 5, 1), Nombre = "Día del Trabajo" },
        new Festivo { Fecha = new DateTime(2026, 5, 18), Nombre = "Día de la Ascensión" },
        new Festivo { Fecha = new DateTime(2026, 6, 8), Nombre = "Corpus Christi" },
        new Festivo { Fecha = new DateTime(2026, 6, 15), Nombre = "Sagrado Corazón" },
        new Festivo { Fecha = new DateTime(2026, 7, 3), Nombre = "San Pedro y San Pablo" },
        new Festivo { Fecha = new DateTime(2026, 7, 20), Nombre = "Día de la Independencia" },
        new Festivo { Fecha = new DateTime(2026, 8, 7), Nombre = "Batalla de Boyacá" },
        new Festivo { Fecha = new DateTime(2026, 8, 17), Nombre = "Asunción de la Virgen" },
        new Festivo { Fecha = new DateTime(2026, 10, 12), Nombre = "Día de la Raza" },
        new Festivo { Fecha = new DateTime(2026, 11, 2), Nombre = "Todos los Santos" },
        new Festivo { Fecha = new DateTime(2026, 11, 16), Nombre = "Independencia de Cartagena" },
        new Festivo { Fecha = new DateTime(2026, 12, 7), Nombre = "Día de las Velitas" },
        new Festivo { Fecha = new DateTime(2026, 12, 8), Nombre = "Inmaculada Concepción" },
        new Festivo { Fecha = new DateTime(2026, 12, 25), Nombre = "Navidad" }
    };

    context.Festivos.AddRange(todosLosFestivos);
    context.SaveChanges();
}

app.Run();