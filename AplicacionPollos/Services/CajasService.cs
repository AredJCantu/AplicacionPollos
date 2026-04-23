using AplicacionPollos.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace AplicacionPollos.Services
{
    public class CajasService
    {
        private readonly HttpClient _httpClient;

        public CajasService()
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://localhost:7018/")
            };
        }

        public async Task<List<CajasModel>> GetCajasByDateAsync(DateTime fecha) 
        {
            return await _httpClient.GetFromJsonAsync<List<CajasModel>>($"cajas/{fecha}") ?? new();
        }

        public async Task<bool> SaveCajasAsync(CajasModel caja) 
        {
            var respuesta = _httpClient.PostAsJsonAsync("productos", caja);
            return respuesta.IsCompletedSuccessfully;
        }

        public async Task<bool> SaveAnomaliaAsync(CajasModel caja)
        {
            var respuesta = _httpClient.PostAsJsonAsync("anomalia", caja);
            return respuesta.IsCompletedSuccessfully;
        }
    }
}
