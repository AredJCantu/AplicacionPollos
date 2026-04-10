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

        // Reemplazamos SQLite con una lista en memoria y un contador de IDs
        private List<CajasModel> _cajas;
        private int _siguienteId;

        public List<string> Errores;

        public GestionadorCajas()
        {
            _cajas = new List<CajasModel>();
            _siguienteId = 1; // Inicializamos el ID simulando el Autoincrement
        }

        // --- Create ---

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
            if (caja.rango_peso < 0)
            {
                Errores.Add("El rango de peso es obligatorio.");
            }
            if (Errores.Count > 0)
            {
                return false;
            }

            // Simulamos la inserción y el auto-incremento del ID
            caja.id = _siguienteId++;
            _cajas.Add(caja);

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
                if (caja.rango_peso < 0)
                {
                    Errores.Add($"La caja con temp_id {caja.temp_id} tiene un rango de peso inválido.");
                    continue;
                }

                // Asignar ID e insertar en la lista
                caja.id = _siguienteId++;
                _cajas.Add(caja);
                cajasAgregadas.Add(caja);
            }
        }

        // --- Read ---

        public IEnumerable<CajasModel> GetAll()
        {
            return _cajas.OrderBy(c => c.id);
        }

        public CajasModel GetById(int id)
        {
            return _cajas.FirstOrDefault(c => c.id == id);
        }

        public IEnumerable<CajasModel> GetByLote(int numeroLote)
        {
            return _cajas.Where(c => c.numero_lote == numeroLote).OrderBy(c => c.id);
        }

        public IEnumerable<CajasModel> GetByRangoPeso(byte rangoPeso)
        {
            return _cajas.Where(c => c.rango_peso == rangoPeso).OrderBy(c => c.id);
        }

        // --- Update ---

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
            if (caja.rango_peso < 0)
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

            // Al actualizar las propiedades de 'c', automáticamente se refleja en la lista
            // porque 'c' es una referencia al objeto almacenado en _cajas.
            c.codigo_barras = caja.codigo_barras;
            c.numero_lote = caja.numero_lote;
            c.rango_peso = caja.rango_peso;
            c.peso = caja.peso;

            return true;
        }

        // --- Delete ---

        public bool EliminarCaja(int id)
        {
            Errores = new List<string>();
            CajasModel caja = GetById(id);

            if (caja == null)
            {
                Errores.Add("La caja no existe.");
                return false;
            }

            _cajas.Remove(caja);
            return true;
        }

        public void EliminarTodasLasCajas()
        {
            _cajas.Clear();
            _siguienteId = 1; // Opcional: Reiniciar el contador de IDs si vacías la lista
        }

        public async Task ImprimirExcel()
        {
            await excel.CrearYAbrirExcel(GetAll().ToList());
        }
    }
}