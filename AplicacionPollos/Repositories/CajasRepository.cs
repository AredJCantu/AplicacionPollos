using AplicacionPollos.Models;
using AplicacionPollos.Services;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AplicacionPollos.Repositories
{
    public class GestionadorCajas
    {
        ImprimirExcel excel = new();
        SQLiteConnection context;
        public List<string> Errores;

        public GestionadorCajas()
        {
            string ruta = FileSystem.AppDataDirectory + "/cajas.db3";
            context = new SQLiteConnection(ruta);
            context.CreateTable<CajasModel>();
        }

        //Create

        public bool AgregarCaja(CajasModel caja)
        {
            Errores = new List<string>();

            if (string.IsNullOrWhiteSpace(caja.codigo_barras))
            {
                Errores.Add("El código de barras es obligatorio.");
            }
            if (caja.peso <= 0)
            {
                Errores.Add("El peso debe ser mayor que 0.");
            }
            if (caja.numero_lote <= 0)
            {
                Errores.Add("El número de lote es obligatorio.");
            }
            if (caja.rango_peso <= 0)
            {
                Errores.Add("El rango de peso es obligatorio.");
            }
            if (Errores.Count > 0)
            {
                return false;
            }

            context.Insert(caja);
            return true;
        }
        public void AgregarCajas(IEnumerable<CajasModel> cajas)
        {
            Errores = new List<string>();
            List<CajasModel> cajasAgregadas = new List<CajasModel>();
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
                if (caja.rango_peso <= 0)
                {
                    Errores.Add($"La caja con temp_id {caja.temp_id} tiene un rango de peso inválido.");
                    continue;
                }
                context.Insert(caja);
                cajasAgregadas.Add(caja);
            }
        }

        //Read
        public IEnumerable<CajasModel> GetAll()
        {
            return context.Table<CajasModel>().OrderBy(c => c.id);
        }

        public CajasModel GetById(int id)
        {
            return context.Table<CajasModel>().FirstOrDefault(c => c.id == id);
        }

        public IEnumerable<CajasModel> GetByLote(int numeroLote)
        {
            return context.Table<CajasModel>().Where(c => c.numero_lote == numeroLote).OrderBy(c => c.id);
        }

        public IEnumerable<CajasModel> GetByRangoPeso(byte rangoPeso)
        {
            return context.Table<CajasModel>().Where(c => c.rango_peso == rangoPeso).OrderBy(c => c.id);
        }

        //Update
        public bool ActualizarCaja(CajasModel caja)
        {
            Errores = new List<string>();

            if (string.IsNullOrWhiteSpace(caja.codigo_barras))
            {
                Errores.Add("El código de barras es obligatorio.");
            }
            if (caja.peso <= 0)
            {
                Errores.Add("El peso debe ser mayor que 0.");
            }
            if (caja.numero_lote <= 0)
            {
                Errores.Add("El número de lote es obligatorio.");
            }
            if (caja.rango_peso <= 0)
            {
                Errores.Add("El rango de peso es obligatorio.");
            }
            if (Errores.Count > 0)
            {
                return false;
            }

            CajasModel c = GetById(caja.id);

            if (c == null)
            {
                Errores.Add("La caja no existe.");
                return false;
            }

            c.codigo_barras = caja.codigo_barras;
            c.numero_lote = caja.numero_lote;
            c.rango_peso = caja.rango_peso;
            c.peso = caja.peso;

            context.Update(c);
            return true;
        }

        //Delete
        public bool EliminarCaja(int id)
        {
            Errores = new List<string>();
            CajasModel caja = GetById(id);

            if (caja == null)
            {
                Errores.Add("La caja no existe.");
                return false;
            }

            context.Delete(caja);
            return true;
        }
        //borrar todas las cajas
        public void EliminarTodasLasCajas()
        {
            context.DeleteAll<CajasModel>();
        }
        public async Task ImprimirExcel()
        {
            await excel.CrearYAbrirExcel(GetAll().ToList());
        }
    }
}
