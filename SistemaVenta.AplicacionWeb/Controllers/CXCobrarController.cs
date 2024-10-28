using Microsoft.AspNetCore.Mvc;
using SistemaVenta.Entity;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using System.Linq;
using SistemaVenta.DAL.DBContext;

public class CXCobrarController : Controller
{
    private readonly DBVENTAContext _context;

    public CXCobrarController(DBVENTAContext context)
    {
        _context = context;
    }

    // Método para la vista principal
    public IActionResult Index()
    {
        var facturas = (from f in _context.Facturas
                        join c in _context.Clientes on f.ClienteID equals c.ClienteId
                        select new FacturaViewModel
                        {
                            FacturaID = f.FacturaID,
                            NumeroIdentificacion = c.NumeroIdentificacion,
                            NombreCliente = c.Nombre + " " + c.Apellidos,
                            MontoTotal = f.MontoTotal,
                            Abono = f.Abono ?? 0,
                            Estado = f.Estado,
                            Pendiente = f.Pendiente // Aquí obtenemos el campo calculado
                        }).ToList();

        ViewData["TotalGenerado"] = facturas.Sum(f => f.MontoTotal);
        ViewData["TotalPendiente"] = facturas.Sum(f => f.Pendiente);

        return View(facturas);
    }

    // Método para buscar facturas según filtro
    [HttpPost]
    public IActionResult Buscar(string numeroFactura, string cliente, DateTime? desde, DateTime? hasta)
    {
        var facturas = (from f in _context.Facturas
                        join c in _context.Clientes on f.ClienteID equals c.ClienteId
                        where (string.IsNullOrEmpty(numeroFactura) || f.FacturaID.ToString() == numeroFactura)
                            && (string.IsNullOrEmpty(cliente) || c.Nombre.Contains(cliente))
                            && (!desde.HasValue || f.FechaEmision >= desde)
                            && (!hasta.HasValue || f.FechaEmision <= hasta)
                        select new FacturaViewModel
                        {
                            FacturaID = f.FacturaID,
                            NumeroIdentificacion = c.NumeroIdentificacion,
                            NombreCliente = c.Nombre + " " + c.Apellidos,
                            MontoTotal = f.MontoTotal,
                            Abono = f.Abono ?? 0,
                            Estado = f.Estado,
                            Pendiente = f.Pendiente // También aquí obtenemos el campo calculado
                        }).ToList();

        ViewData["TotalGenerado"] = facturas.Sum(f => f.MontoTotal);
        ViewData["TotalPendiente"] = facturas.Sum(f => f.Pendiente);

        return View("Index", facturas);
    }

    // Método para abonar a la factura
    [HttpPost]
    public IActionResult Abonar(List<int> selectedFacturas, Dictionary<int, decimal> abonos)
    {
        if (selectedFacturas == null || !selectedFacturas.Any())
        {
            ModelState.AddModelError("", "Debe seleccionar al menos una factura.");
            return RedirectToAction("Index");
        }

        foreach (var facturaId in selectedFacturas)
        {
            var factura = _context.Facturas.FirstOrDefault(f => f.FacturaID == facturaId);
            if (factura != null && abonos.ContainsKey(facturaId))
            {
                decimal abono = abonos[facturaId];

                // Validar que el abono no exceda el monto total pendiente
                decimal nuevoAbono = (factura.Abono ?? 0m) + abono;
                if (nuevoAbono > factura.MontoTotal)
                {
                    ModelState.AddModelError("", $"El abono no puede exceder el monto total para la factura {facturaId}.");
                    continue;
                }

                factura.Abono = nuevoAbono;

                // Actualizar el estado de la factura
                factura.Estado = nuevoAbono >= factura.MontoTotal ? "Pagado" : "Pendiente";
            }
        }

        // Guardar los cambios en la base de datos
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

}
