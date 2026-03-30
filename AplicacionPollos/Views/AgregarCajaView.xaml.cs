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
    private bool _procesandoEscaneo = false;
    private CajasViewModel contexto;

    // --- Constructor y Ciclo de Vida ---
    public AgregarCajaView(CajasViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        barcodeReader.Options = new ZXing.Net.Maui.BarcodeReaderOptions
        {
            AutoRotate = true,
            Multiple = true
        };

        contexto = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RequestCameraPermission();
        await Task.Delay(100);
        txtCodigo.Focus();
    }

    // --- Gestión del Escáner y Permisos ---
    private async Task RequestCameraPermission()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permisos", "Se requiere permiso de cámara para escanear códigos de barras", "Aceptar");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error solicitando permiso de cámara: {ex.Message}", "Aceptar");
        }
    }

    private void barcodeReader_BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {
        if (_procesandoEscaneo) return;

        var first = e.Results.FirstOrDefault();
        if (first is null || string.IsNullOrWhiteSpace(first.Value))
            return;

        _procesandoEscaneo = true;
        string codigoLeido = first.Value.Trim();

        Dispatcher.Dispatch(async () =>
        {
            try
            {
                if (codigoLeido.Length < 20)
                {
                    await DisplayAlert("Error", "Código de barras muy corto. Mínimo 20 caracteres", "Aceptar");
                    return;
                }
                if (codigoLeido.Length > 50)
                {
                    await DisplayAlert("Error", "Código de barras muy largo. Máximo 50 caracteres", "Aceptar");
                    return;
                }

                contexto.Agregar(codigoLeido);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al procesar el código: {ex.Message}", "Aceptar");
            }
        });

        // Retraso fuera del hilo principal para no trabar el UI
        Task.Run(async () =>
        {
            await Task.Delay(1500);
            _procesandoEscaneo = false;
        });
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
                txtCodigo.IsEnabled = false;
                txtRango.IsEnabled = false;
                txtPeso.IsEnabled = false;
                BtnAceptar.Text = "Agregar";
                BtnAceptar.Command = contexto.AgregarCommand;
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
            txtCodigo.IsEnabled = false;
            txtRango.IsEnabled = false;
            txtPeso.IsEnabled = false;
            BtnAceptar.Text = "Agregar";
            BtnAceptar.Command = contexto.AgregarCommand;
        });

        var objSwipe = (SwipeItem)sender;
        CajasModel caja_Parametro = (CajasModel)objSwipe.CommandParameter;

        bool res = await DisplayAlert("Confirmación", "¿Está seguro de eliminar este elemento?", "Confirmar", "Cancelar");
        if (res)
            contexto.Eliminar(caja_Parametro);
    }
}