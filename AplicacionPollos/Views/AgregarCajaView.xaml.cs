using AplicacionPollos.Models;
using AplicacionPollos.ViewModels;
using Microsoft.Maui.Controls;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System;
using ZXing;

namespace AplicacionPollos.Views;

public partial class AgregarCajaView : ContentPage
{
    // --- Campos Privados ---
    private SwipeView _AbiertoActualmente;
    private CajasViewModel contexto;

    // --- Constructor y Ciclo de Vida ---
    public AgregarCajaView(CajasViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        contexto = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(100);
        txtCodigo.IsEnabled = true;
        txtCodigo.Focus();
    }

    // --- Gestión del Escáner Físico ---
    // Este evento reemplaza al barcodeReader_BarcodesDetected.
    // Se dispara cuando la terminal termina de leer el código y manda un "Enter".
    private void txtCodigo_Completed(object sender, EventArgs e)
    {
        string codigoLeido = txtCodigo.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(codigoLeido))
        {
            txtCodigo.Focus();
            return;
        }
        try
        {
            // Mandamos el código al ViewModel
            contexto.Agregar(codigoLeido);
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Error al procesar el código: {ex.Message}", "Aceptar");
        }

        // Automáticamente preparamos la pantalla para la siguiente caja de pollo
        PrepararParaSiguienteEscaneo();
    }

    // Método auxiliar para limpiar y re-enfocar rápido
    private void PrepararParaSiguienteEscaneo()
    {
        txtCodigo.Text = string.Empty;
        Dispatcher.Dispatch(() => txtCodigo.Focus());
    }

    // --- Eventos de la Interfaz Gráfica (UI) ---
    private void SwipeView_SwipeStarted(object sender, SwipeStartedEventArgs e)
    {
        var swipeViewActual = sender as SwipeView;
        if (_AbiertoActualmente != null && _AbiertoActualmente != swipeViewActual)
        {
            _AbiertoActualmente.Close();
        }
        _AbiertoActualmente = swipeViewActual;
    }

    private void SwipeItem_Clicked(object sender, EventArgs e)
    {
        Dispatcher.Dispatch(() =>
        {
            txtCodigo.IsEnabled = true;
            txtCodigo.Focus();
            txtRango.IsEnabled = true;
            txtPeso.IsEnabled = true;
            BtnAceptar.Text = "Editar";
            BtnAceptar.Command = contexto.EditarCommand;
        });
    }

    private void BtnAceptar_Clicked(object sender, EventArgs e)
    {
        if (BtnAceptar.Command == contexto.EditarCommand)
        {
            Dispatcher.Dispatch(() =>
            {
                // Regresamos el Entry del código a habilitado para seguir escaneando
                txtCodigo.IsEnabled = true;
                txtRango.IsEnabled = false;
                txtPeso.IsEnabled = false;
                BtnAceptar.Text = "Agregar";
                BtnAceptar.Command = contexto.AgregarCommand;

                PrepararParaSiguienteEscaneo();
            });
        }
    }

    private void Enviar_Datos_Clicked(object sender, EventArgs e)
    {
        contexto.EnviarDatos();
    }

    private async void Eliminar_Clicked(object sender, EventArgs e)
    {
        Dispatcher.Dispatch(() => {
            txtCodigo.IsEnabled = true;
            txtRango.IsEnabled = false;
            txtPeso.IsEnabled = false;
            BtnAceptar.Text = "Agregar";
            BtnAceptar.Command = contexto.AgregarCommand;
        });

        var objSwipe = (SwipeItem)sender;
        CajasModel caja_Parametro = (CajasModel)objSwipe.CommandParameter;

        bool res = await DisplayAlert("Confirmación", "¿Está seguro de eliminar este elemento?", "Confirmar", "Cancelar");
        if (res)
        {
            contexto.Eliminar(caja_Parametro);
            PrepararParaSiguienteEscaneo();
        }
        else
        {
            PrepararParaSiguienteEscaneo();
        }
    }
}
