using System;
using System.Collections.Generic;
using System.Text;
using AplicacionPollos.Models;
using ClosedXML.Excel;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace AplicacionPollos.Services
{
    public class ImprimirExcel
    {
        public async Task CrearYAbrirExcel(List<CajasModel> Cajas)
        {
            try
            {
                // Crear un nuevo workbook
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Inventario de Cajas");

                // Agregar encabezados
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Temp ID";
                worksheet.Cell(1, 3).Value = "Número de Lote";
                worksheet.Cell(1, 4).Value = "GTIN";
                worksheet.Cell(1, 5).Value = "Código de Barras";
                worksheet.Cell(1, 6).Value = "Rango Peso";
                worksheet.Cell(1, 7).Value = "Peso";
                worksheet.Cell(1, 8).Value = "Número de Piezas";

                // Aplicar formato a los encabezados
                var headerRange = worksheet.Range("A1:H1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Agregar datos
                int row = 2;
                foreach (var caja in Cajas)
                {
                    worksheet.Cell(row, 1).Value = caja.id;
                    worksheet.Cell(row, 2).Value = caja.temp_id;
                    worksheet.Cell(row, 3).Value = caja.numero_lote;
                    worksheet.Cell(row, 4).Value = caja.GTIN;
                    worksheet.Cell(row, 5).Value = caja.codigo_barras;
                    worksheet.Cell(row, 6).Value = caja.rango_peso;
                    worksheet.Cell(row, 7).Value = caja.peso;
                    worksheet.Cell(row, 8).Value = caja.numero_piezas ?? 0;
                    row++;
                }

                // Ajustar el ancho de las columnas
                worksheet.Columns().AdjustToContents();

                // Crear el nombre del archivo con fecha y hora
                string fileName = $"Inventario_Cajas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

                // Guardar el archivo
                workbook.SaveAs(filePath);

                // Abrir el archivo
                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath)
                });
            }
            catch (Exception ex)
            {
                // Manejar errores
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo crear el archivo Excel: {ex.Message}", "OK");
            }
        }
    }
}
