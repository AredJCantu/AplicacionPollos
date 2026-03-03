using AplicacionPollos.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AplicacionPollos.Repositories
{
    public class CajasRepository
    {
        private SQLiteAsyncConnection _db;
        private readonly Task _initTask;

        public CajasRepository()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "cajas.db");
            _db = new SQLiteAsyncConnection(dbPath);
            _initTask = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await _db.CreateTableAsync<CajasModel>();
        }

        private async Task EnsureInitializedAsync()
        {
            await _initTask;
        }

        //agregar
        public async Task<int> AgregarCajaAsync(CajasModel caja)
        {
            await EnsureInitializedAsync();
            return await _db.InsertAsync(caja);
        }

        public async Task<List<CajasModel>> ObtenerCajasAsync()
        {
            await EnsureInitializedAsync();
            return await _db.Table<CajasModel>().ToListAsync();
        }

        public async Task<int> EliminarCajaAsync(CajasModel caja)
        {
            await EnsureInitializedAsync();
            return await _db.DeleteAsync(caja);
        }
        public async Task<int> EditarCajaAsync(CajasModel caja)
        {
            await EnsureInitializedAsync();
            var cajaExistente = await _db.Table<CajasModel>().Where(c => c.id == caja.id).FirstOrDefaultAsync();
            if (cajaExistente != null)
            {
                cajaExistente.peso = caja.peso;
                cajaExistente.numero_lote = caja.numero_lote;
                return await _db.UpdateAsync(cajaExistente);
            }
            return 0; 
        }

    }
}
