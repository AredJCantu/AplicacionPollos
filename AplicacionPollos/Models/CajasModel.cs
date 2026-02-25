using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AplicacionPollos.Models
{
    public class CajasModel
    {
        public int id { get; set; }
        public int numero_lote { get; set; }
        public byte rango_peso { get; set; }
        public decimal peso { get; set; }
    }
}
