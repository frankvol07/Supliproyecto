using Microsoft.AspNetCore.Mvc;

using AutoMapper;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using SistemaVenta.DAL.DBContext;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    [Authorize]
    public class ReporteController : Controller
    {

        private readonly IMapper _mapper;
        private readonly IVentaService _ventaServicio;
        private readonly DBVENTAContext _context;


        public ReporteController(IMapper mapper, IVentaService ventaServicio, DBVENTAContext context)
        {
            _mapper = mapper;
            _ventaServicio = ventaServicio;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ReporteVenta(string fechaInicio, string fechaFin)
        {
            List<VMReporteVenta> vmLista = _mapper.Map<List<VMReporteVenta>>( await _ventaServicio.Reporte(fechaInicio, fechaFin));
            return StatusCode(StatusCodes.Status200OK, new { data = vmLista });

        }


    }
}
