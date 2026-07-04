using Microsoft.EntityFrameworkCore;
using ControlTaxisApp.Models;
using OfficeOpenXml; // Asegúrate de incluir este namespace

var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURACIÓN PARA EPPLUS 5.8.14 ---
// Esta configuración es necesaria para versiones 5.x de EPPlus
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// 1. REGISTRO DEL CONTEXTO DE LA BASE DE DATOS
//builder.Services.AddDbContext<ControlTaxisContext>(options =>
//options.UseSqlServer("Server=Daniel\\SQLEXPRESS;Database=ControlTaxisDB;Trusted_Connection=True;TrustServerCertificate=True"));

builder.Services.AddDbContext<ControlTaxisContext>(options =>
    options.UseSqlite("Data Source=ControlTaxis.db"));

// 2. AGREGAR SERVICIOS PARA CONTROLADORES Y VISTAS (MVC)
builder.Services.AddControllersWithViews();

// 3. CONFIGURACIÓN DE LA AUTENTICACIÓN POR COOKIES
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Acceso/Index";
    });

var app = builder.Build();

// 4. CONFIGURACIÓN DEL ENTORNO DE EJECUCIÓN (Pipeline HTTP)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 5. RUTA POR DEFECTO
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Login}/{id?}");

app.Run();