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
        txtCodigo.HandlerChanged += TxtCodigo_HandlerChanged;
        txtCodigo.Unfocused += TxtCodigo_Unfocused;
    }
    
    private void TxtCodigo_HandlerChanged(object sender, EventArgs e)
    {
        // Estas directivas #if aseguran que este código solo se compile en Android
#if ANDROID
        if (txtCodigo.Handler?.PlatformView is Android.Widget.EditText nativeEntry)
        {
            // A. Evitar que el teclado virtual aparezca al recibir Focus programáticamente
            nativeEntry.ShowSoftInputOnFocus = false;

            // B. Detectar el toque humano (Touch)
            nativeEntry.Touch += (s, touchEvent) =>
            {
                // Cuando el usuario levanta el dedo de la pantalla (ActionUp)
                if (touchEvent.Event.Action == Android.Views.MotionEventActions.Up)
                {
                    // Permitimos que se abra el teclado
                    nativeEntry.ShowSoftInputOnFocus = true;

                    // Forzamos al sistema operativo a mostrar el teclado virtual
                    var keyboard = (Android.Views.InputMethods.InputMethodManager)nativeEntry.Context.GetSystemService(Android.Content.Context.InputMethodService);
                    keyboard?.ShowSoftInput(nativeEntry, Android.Views.InputMethods.ShowFlags.Implicit);
                }

                // IMPORTANTE: Devolvemos false para que el clic normal del Entry siga funcionando
                touchEvent.Handled = false;
            };
        }
#endif
    }


    private void TxtCodigo_Unfocused(object sender, FocusEventArgs e)
    {
#if ANDROID
        // C. Cuando el Entry pierde el foco (por ejemplo, después de escanear y presionar "Agregar")
        // Volvemos a bloquear el teclado para que en el próximo escaneo no aparezca
        if (txtCodigo.Handler?.PlatformView is Android.Widget.EditText nativeEntry)
        {
            nativeEntry.ShowSoftInputOnFocus = false;
        }
#endif
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
        if (contexto.EditarEntrys == false)
        {
            string codigoLeido = txtCodigo.Text?.Trim() ?? string.Empty;
            contexto.CerrarMenu();
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
    }

    // Método auxiliar para limpiar y re-enfocar rápido
    private void PrepararParaSiguienteEscaneo()
    {
        
        txtCodigo.Text = string.Empty;
        Dispatcher.Dispatch(() => txtCodigo.Focus());
    }

    private void BtnAceptar_Clicked(object sender, EventArgs e)
    {
        PrepararParaSiguienteEscaneo();
    }

    private void Enviar_Datos_Clicked(object sender, EventArgs e)
    {
        contexto.EnviarDatos();
    }
    //aceptar clicket
    private void Button_Clicked(object sender, EventArgs e)
    {
        PrepararParaSiguienteEscaneo();
    }

    private void RegistroManualbtn_Clicked(object sender, EventArgs e)
    {
        txtCodigo_2.Text = contexto.CajaAnomalia.codigo_barras??string.Empty;
        
        Dispatcher.Dispatch(() => txtLote_2.Focus());
    }

    private void GuardarManual_Clicked(object sender, EventArgs e)
    {
        PrepararParaSiguienteEscaneo();
    }

}
