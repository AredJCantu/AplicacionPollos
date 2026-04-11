

namespace AplicacionPollos.Models
{
    public partial class CajasModel
    {
        public int id { get; set; }
        public int numero_lote { get; set; }
        public string GTIN { get; set; } = string.Empty; //Global Trader ID Number
        public byte rango_peso { get; set; } //No lo creo necesario
        public decimal peso { get; set; } = 0;
        public int? numero_piezas { get; set; } //Número de piezas del producto que incluye la caja
    }
}
