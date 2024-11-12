using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.Entity
{
    [Table("ComprobantesFiscales")]
    public class ComprobanteFiscal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Tipo { get; set; }

        [Required]
        [StringLength(20)]
        public string NCF_Desde { get; set; }

        [Required]
        [StringLength(20)]
        public string NCF_Hasta { get; set; }

        [Required]
        [StringLength(20)]
        public string NCF_Actual { get; set; }

        [Required]
        public int NCF_Restan { get; set; }

        [Required]
        public DateTime Fecha_Vto { get; set; }
    }
}
