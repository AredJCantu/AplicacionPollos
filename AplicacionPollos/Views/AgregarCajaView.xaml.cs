using AplicacionPollos.ViewModels;
using System.ComponentModel;
using ZXing;

namespace AplicacionPollos.Views;

public partial class AgregarCajaView : ContentPage
{
    private SwipeView _AbiertoActualmente;
    Dictionary<string, byte> categorias = new() {
            { "1254", 3 },
            { "1255", 4 },
            { "1256", 5},
            { "1257", 6}
        };
    string rango_Peso = "";
    string Peso = "";
    CajasViewModel contexto = new();
    public AgregarCajaView()
	{
		InitializeComponent();
        barcodeReader.Options = new ZXing.Net.Maui.BarcodeReaderOptions
        {
            AutoRotate = true,
            Multiple = true
        };
    }
	
    private void Entry_Completed(object sender, EventArgs e)
    {
        ActualizarTextBox();
    }
    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        ActualizarTextBox();
    }

    private void ActualizarTextBox()
    {
        if(txtCodigo is not null && txtCodigo.Text is not null)
        {
            var codigo = txtCodigo.Text.Trim();
            if (string.IsNullOrWhiteSpace(codigo))
                return;
            // traer el rango de peso y peso del vm
            CalcularCaja(codigo);
            txtRango.Text = rango_Peso;
            txtPeso.Text = Peso;
        }
    }

    private void txtCodigo_Unfocused(object sender, FocusEventArgs e)
    {
        if (txtCodigo.Text != null)
        {
            var codigo = txtCodigo.Text.Trim();
		
            if (string.IsNullOrWhiteSpace(codigo) && codigo.Length <= 24)
                return;
            // traer el rango de peso y peso del vm
            CalcularCaja(codigo);
            txtRango.Text = rango_Peso;
            txtPeso.Text = Peso;
        }
    }
    public void CalcularCaja(string codigo)
    {
        rango_Peso = categorias[codigo.Substring(2, 4)].ToString();
        string p = codigo.Substring(12, 4);
        Peso = p.Insert(2,".");
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
            txtCodigo.Text = string.Empty;
            txtCodigo.Text = first.Value;
            barcodeReader.IsDetecting = true;
        });
        ActualizarTextBox();
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