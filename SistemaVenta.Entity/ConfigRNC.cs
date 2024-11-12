using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.Entity
{
    [Table("ConfigRNC")] // Especifica el nombre de la tabla en la base de datos
    public class ConfigRNC
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public bool Valor { get; set; }
    }
}

