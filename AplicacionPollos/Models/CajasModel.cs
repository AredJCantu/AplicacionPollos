using SQLite;

namespace AplicacionPollos.Models
{
    public class CajasModel
    {
        //271254486922289100162628A
        public string codigo_barras { get; set; } = string.Empty;

        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public int numero_lote { get; set; }
        public byte rango_peso { get; set; }
        public decimal peso { get; set; } = 0;
        public int? numero_id { get; set; } //Son los últimos 4 digitos del ID de algunos tipos de etiqueta
        public int? numero_empleado { get; set; } //Número del empleado que etiquetó la caja
        public int? numero_planta { get; set; } //Número de la planta de donde proviene la caja
        public int? numero_piezas { get; set; } //Número de piezas del producto que incluye la caja
        public int? id_producto { get; set; } /* Número de ID que utiliza la empresa proveedora para identificar su producto. 
                                              * NOTA: en el código de barras solo vienen los primeros 4 digitos que identifican al producto.*/
        public string? proveedor { get; set; } = string.Empty; /* En caso de que luego podamos identificar el proveedor en base al
                                                              * código de barras (ej. Por el tamaño del código). */
    }
}
