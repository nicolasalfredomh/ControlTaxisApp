using ControlTaxisApp.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

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
        options.LoginPath = "/Acceso/Index";
    });

var app = builder.Build();

// --- 4. CREACIÓN AUTOMÁTICA DE LA BASE DE DATOS ---
// Esto asegura que si el archivo .db no existe en la carpeta /app/data, se cree al iniciar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ControlTaxisContext>();
    db.Database.EnsureCreated();
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

app.Run();