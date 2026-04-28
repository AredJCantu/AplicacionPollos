using SQLite;


namespace AplicacionPollos.Models
{
    [Table("cajas")]
    public partial class CajasModel
    {
        [PrimaryKey, NotNull, AutoIncrement]
        public int id { get; set; }
        [NotNull]
        public int numero_lote { get; set; }
        [NotNull]
        public string GTIN { get; set; } = string.Empty; //Global Trader ID Number
        [NotNull]
        public byte rango_peso { get; set; } //No lo creo necesario
        [NotNull]
        public decimal peso { get; set; } = 0;
        public int? numero_piezas { get; set; } //Número de piezas del producto que incluye la caja
    }
}
