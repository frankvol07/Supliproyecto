using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;
using System.Globalization;

namespace SistemaVenta.BLL.Implementacion
{
    public class DashBoardService : IDashBoardService
    {
        private readonly IVentaRepository _repositorioVenta;
        private readonly IGenericRepository<DetalleVenta> _repositorioDetalleVenta;
        private readonly IGenericRepository<Categoria> _repositorioCategoria;
        private readonly IGenericRepository<Producto> _repositorioProducto;

        // Propiedad privada para manejar el rango de tiempo
        private string _rango = "semanal";

        public DashBoardService(
            IVentaRepository repositorioVenta,
            IGenericRepository<DetalleVenta> repositorioDetalleVenta,
            IGenericRepository<Categoria> repositorioCategoria,
            IGenericRepository<Producto> repositorioProducto
            )
        {
            _repositorioVenta = repositorioVenta;
            _repositorioDetalleVenta = repositorioDetalleVenta;
            _repositorioCategoria = repositorioCategoria;
            _repositorioProducto = repositorioProducto;
        }

        public void SetRango(string rango)
        {
            if (new[] { "diaria", "semanal", "mensual", "anual" }.Contains(rango.ToLower()))
            {
                _rango = rango.ToLower();
            }
            else
            {
                throw new ArgumentException("Rango no válido");
            }
        }

        private (DateTime startDate, DateTime endDate) GetDateRange()
        {
            DateTime today = DateTime.Now.Date;

            return _rango switch
            {
                "diaria" => (today, today),
                "semanal" => (today.AddDays(-7), today),
                "mensual" => (new DateTime(today.Year, today.Month, 1), today),
                "anual" => (new DateTime(today.Year, 1, 1), today),
                _ => throw new ArgumentException("Rango no válido")
            };
        }

        public async Task<int> TotalVentasUltimaSemana()
        {
            try
            {
                var (startDate, endDate) = GetDateRange();
                IQueryable<Venta> query = await _repositorioVenta.Consultar(v => v.FechaRegistro.Value.Date >= startDate && v.FechaRegistro.Value.Date <= endDate);
                return query.Count();
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> TotalIngresosUltimaSemana()
        {
            try
            {
                var (startDate, endDate) = GetDateRange();
                IQueryable<Venta> query = await _repositorioVenta.Consultar(v => v.FechaRegistro.Value.Date >= startDate && v.FechaRegistro.Value.Date <= endDate);

                decimal resultado = query
                    .Select(v => v.Total)
                    .Sum(v => v.Value);

                return Convert.ToString(resultado, new CultureInfo("es-PE"));
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> TotalProductos()
        {
            try
            {
                IQueryable<Producto> query = await _repositorioProducto.Consultar();
                return query.Count();
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> TotalCategorias()
        {
            try
            {
                IQueryable<Categoria> query = await _repositorioCategoria.Consultar();
                return query.Count();
            }
            catch
            {
                throw;
            }
        }

        public async Task<Dictionary<string, int>> VentasUltimaSemana()
        {
            try
            {
                var (startDate, endDate) = GetDateRange();
                IQueryable<Venta> query = await _repositorioVenta.Consultar(v => v.FechaRegistro.Value.Date >= startDate && v.FechaRegistro.Value.Date <= endDate);

                Dictionary<string, int> resultado = query
                    .GroupBy(v => v.FechaRegistro.Value.Date).OrderByDescending(g => g.Key)
                    .Select(dv => new { fecha = dv.Key.ToString("dd/MM/yyyy"), total = dv.Count() })
                    .ToDictionary(keySelector: r => r.fecha, elementSelector: r => r.total);

                return resultado;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Dictionary<string, int>> ProductosTopUltimaSemana()
        {
            try
            {
                var (startDate, endDate) = GetDateRange();
                IQueryable<DetalleVenta> query = await _repositorioDetalleVenta.Consultar();

                Dictionary<string, int> resultado = query
                    .Include(v => v.IdVentaNavigation)
                    .Where(dv => dv.IdVentaNavigation.FechaRegistro.Value.Date >= startDate && dv.IdVentaNavigation.FechaRegistro.Value.Date <= endDate)
                    .GroupBy(dv => dv.DescripcionProducto).OrderByDescending(g => g.Count())
                    .Select(dv => new { producto = dv.Key, total = dv.Count() }).Take(4)
                    .ToDictionary(keySelector: r => r.producto, elementSelector: r => r.total);

                return resultado;
            }
            catch
            {
                throw;
            }
        }

    }
}
