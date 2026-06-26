using AplicacionPollos.Models;
using SQLite;
using AplicacionPollos.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AplicacionPollos.Repositories
{
    public class GestionadorCajas
    {
        SQLiteConnection context;
        ImprimirExcel excel = new();
        //private readonly CajasService _cajasService = new();

        public List<string> Errores = new();

        public GestionadorCajas()
        {
            var connection = FileSystem.AppDataDirectory + "/Cajas.db3";
            context = new SQLiteConnection(connection);
            context.CreateTable<CajasModel>();
            context.CreateTable<AnomaliaCaja>();
        }

        //-- Create --
        public bool AgregarCaja(IEnumerable<CajasModel> cajas)
        {
            Errores = new List<string>();

            foreach (var caja in cajas)
            {
                if (string.IsNullOrWhiteSpace(caja.codigo_barras))
                {
                    Errores.Add($"La caja con temp_id {caja.temp_id} tiene un código de barras vacío.");
                    continue;
                }
                if (caja.peso <= 0)
                {
                    Errores.Add($"La caja con temp_id {caja.temp_id} tiene un peso inválido.");
                    continue;
                }
                if (caja.numero_lote <= 0)
                {
                    Errores.Add($"La caja con temp_id {caja.temp_id} tiene un número de lote inválido.");
                    continue;
                }
                if (caja.rango_peso < 0)
                {
                    Errores.Add($"La caja con temp_id {caja.temp_id} tiene un rango de peso inválido.");
                    continue;
                }
                // ✅ Asignar la fecha de inserción si no está asignada
                if (caja.inserted_at == default(DateTime))
                {
                    caja.inserted_at = DateTime.Now;
                }
                context.Insert(caja);
            }

            return true;
        }

        public bool AgregarAnomalia(IEnumerable<AnomaliaCaja> cajas)
        {
            Errores = new List<string>();

            foreach (var caja in cajas)
            {
                if (string.IsNullOrWhiteSpace(caja.codigo_barras))
                {
                    Errores.Add($"La caja con temp_id {caja.temp_id} tiene un código de barras vacío.");
                    continue;
                }
                if (caja.peso <= 0)
                {
                    Errores.Add($"La caja con temp_id {caja.temp_id} tiene un peso inválido.");
                    continue;
                }
                if (caja.numero_lote <= 0)
                {
                    Errores.Add($"La caja con temp_id {caja.temp_id} tiene un número de lote inválido.");
                    continue;
                }
                if (caja.rango_peso < 0)
                {
                    Errores.Add($"La caja con temp_id {caja.temp_id} tiene un rango de peso inválido.");
                    continue;
                }
                // ✅ Asignar la fecha de inserción si no está asignada
                if (caja.inserted_at == default(DateTime))
                {
                    caja.inserted_at = DateTime.Now;
                }
                context.Insert(caja);
            }

            return true;
        }

        // --- Read ---
        public List<CajasModel> GetReporte(DateTime date)
        {
            try
            {
                DateTime fecha = date.Date;                          // Inicio del día: 2026-04-06 00:00:00
                DateTime fechaMañana = fecha.AddDays(1);             // Inicio del siguiente día: 2026-04-07 00:00:00

                // ✅ Ejecutar en memoria después de traer de DB (SQLite-net tiene limitaciones)
                var cajas = context.Table<CajasModel>().ToList();    // Traer todas las cajas
                var resultado = cajas.Where(x => x.inserted_at.Date == fecha.Date).ToList();  // Filtrar por fecha

                return resultado;
            }
            catch (Exception ex)
            {
                Errores.Add($"Error al obtener reporte: {ex.Message}");
                return new List<CajasModel>();
            }
        }

        public async Task ImprimirExcel(DateTime date)
        {
            await excel.CrearYAbrirExcel(GetReporte(date));
        }
    }
}