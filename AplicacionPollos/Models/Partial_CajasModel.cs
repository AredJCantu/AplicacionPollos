using System;
using System.Collections.Generic;
using System.Text;

namespace AplicacionPollos.Models
{
    partial class CajasModel
    {
        public string codigo_barras { get; set; } = string.Empty;
        public int temp_id { get; set; } //Para identificarlos en la lista
    }
}
