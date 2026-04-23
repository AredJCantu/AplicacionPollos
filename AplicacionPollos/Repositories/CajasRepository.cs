using AplicacionPollos.Models;
using AplicacionPollos.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AplicacionPollos.Repositories
{
    public class GestionadorCajas
    {
        ImprimirExcel excel = new();
        private readonly CajasService _cajasService = new();
        public List<string> Errores = new();

        //-- Create --
        public async Task<bool> AgregarCaja(IEnumerable<CajasModel> cajas)
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

                await _cajasService.SaveCajasAsync(caja);
            }

            return true;
        }

        public async Task<bool> AgregarAnomalia(IEnumerable<CajasModel> cajas)
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

                await _cajasService.SaveAnomaliaAsync(caja);
            }

            return true;
        }

        // --- Read ---
        public async Task<List<CajasModel>> GetReporte(DateTime date)
        {
            var cajas = await _cajasService.GetCajasByDateAsync(date);
            return cajas;
        }

        public async Task ImprimirExcel(DateTime date)
        {
            await excel.CrearYAbrirExcel(await GetReporte(date));
        }
    }
}