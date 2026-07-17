
using ControlTaxisApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

public class VehiculoService
{
    private readonly ControlTaxisContext _context; // Cambia 'TuDbContext' por el nombre real de tu clase de base de datos

    public VehiculoService(ControlTaxisContext context)
    {
        _context = context;
    }

    public IQueryable<int> GetVehiculosUsuario(string userId)
    {
        return _context.Vehiculos
                       .Where(v => v.UsuarioId == userId)
                       .Select(v => v.Id);
    }
}