using AplicacionPollos.Models;
using AplicacionPollos.ViewModels;
using Microsoft.Maui.Controls;
using System.ComponentModel;
using ZXing;

namespace AplicacionPollos.Views;

public partial class AgregarCajaView : ContentPage
{
	private SwipeView _AbiertoActualmente;
	private bool _procesandoEscaneo = false;
	CajasViewModel contexto; 
	public AgregarCajaView()
	{
		InitializeComponent();
		barcodeReader.Options = new ZXing.Net.Maui.BarcodeReaderOptions
		{
			AutoRotate = true,
			Multiple = true
		};
		contexto = (CajasViewModel)this.BindingContext;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RequestCameraPermission();
        await Task.Delay(100);
        txtCodigo.Focus();
    }

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
        Dispatcher.Dispatch(async() =>
        {
            try
            {
                // Validación básica de longitud
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
            catch (Exception ex) {
                await DisplayAlert("Error", $"Error al procesar el código: {ex.Message}", "Aceptar");
            }
            finally
            {
                await Task.Delay(1500);
                _procesandoEscaneo = false;
            }
        });

    }
    //Cerrar el menu swipe cuando otro este avierto
    private void SwipeView_SwipeStarted(object sender, SwipeStartedEventArgs e)
    {
        var swipeViewActual = sender as SwipeView;
        if (_AbiertoActualmente != null && _AbiertoActualmente != swipeViewActual)
        {
            _AbiertoActualmente.Close();
        }
        _AbiertoActualmente = swipeViewActual;
    }
    //Boton UI editar presionado
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
                BtnAceptar.Text= "Agregar";
                BtnAceptar.Command = contexto.AgregarCommand;
            });
        }
    }
    //Enviar los datos a la base de datos
    private async void Enviar_Datos_Clicked(object sender, EventArgs e)
    {
        if (contexto.ListaCajas.Count == 0)
        {
            await DisplayAlert("Error", "No hay cajas para enviar", "Aceptar");
            return;
        }
        await DisplayAlert("Éxito","Datos enviados correctamente","Aceptar");
    }
    //Eliminar botón presionado
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
        bool res = await DisplayAlert("Confirmacion", "¿Está seguro de eliminar este elemento?", "Confirmar", "Cancelar");
        if (res) 
            contexto.Eliminar(caja_Parametro);
    }
}