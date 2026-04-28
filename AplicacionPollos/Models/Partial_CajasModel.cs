using System;
using SQLite;
using System.Collections.Generic;
using System.Text;

namespace AplicacionPollos.Models
{
    partial class CajasModel
    {
        [NotNull]
        public string codigo_barras { get; set; } = string.Empty;
        [NotNull]
        public DateTime inserted_at { get; set; }
        public int temp_id { get; set; } //Para identificarlos en la lista
    }
}
