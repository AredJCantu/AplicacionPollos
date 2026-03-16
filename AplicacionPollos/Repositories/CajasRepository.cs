using AplicacionPollos.Models;
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
            c.numero_id = caja.numero_id;
            c.numero_empleado = caja.numero_empleado;
            c.numero_planta = caja.numero_planta;
            c.numero_piezas = caja.numero_piezas;
            c.id_producto = caja.id_producto;
            c.proveedor = caja.proveedor;

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
    }
}
