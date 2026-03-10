using AplicacionPollos.ViewModels;
using System.ComponentModel;
using ZXing;

namespace AplicacionPollos.Views;

public partial class AgregarCajaView : ContentPage
{
    private SwipeView _AbiertoActualmente;
    CajasViewModel contexto; 
    public AgregarCajaView()
	{
		InitializeComponent();
        barcodeReader.Options = new ZXing.Net.Maui.BarcodeReaderOptions
        {
            AutoRotate = true,
            Multiple = true
        };
        contexto=(CajasViewModel)this.BindingContext;
    }
	

    private void ActualizarTextBox()
    {

            var codigo = txtCodigo.Text.Trim();
            if (string.IsNullOrWhiteSpace(codigo))
                return;
            // traer el rango de peso y peso del vm
            contexto.Agregar(codigo);
       
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(100);
        txtCodigo.Focus();
        
    }

    private void barcodeReader_BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {
        var first = e.Results.FirstOrDefault();
        if (first is null)
            return;
        barcodeReader.IsDetecting = false;
        Dispatcher.Dispatch(() =>
        {
            txtCodigo.IsEnabled = true;
            txtCodigo.Focus();
            txtCodigo.Text = first.Value;
            ActualizarTextBox();
            barcodeReader.IsDetecting = true;
            txtCodigo.IsEnabled = false;
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

}