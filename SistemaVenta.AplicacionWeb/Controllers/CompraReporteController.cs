using Microsoft.AspNetCore.Mvc;
using SistemaVenta.DAL.DBContext;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    public class CompraReporteController : Controller
    {
        private readonly DBVENTAContext _context;

        public CompraReporteController(DBVENTAContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
