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

                context.Insert(cajas);
            }

            return true;
        }

        // --- Read ---
        public List<CajasModel> GetReporte(DateTime date)
        {
            DateTime fecha = date.Date;
            DateTime fechaMañana = fecha.AddDays(1);
            return context.Table<CajasModel>().Where(x => x.inserted_at >= fecha && x.inserted_at <= fechaMañana).ToList();
        }

        public async Task ImprimirExcel(DateTime date)
        {
            await excel.CrearYAbrirExcel(GetReporte(date));
        }
    }
}