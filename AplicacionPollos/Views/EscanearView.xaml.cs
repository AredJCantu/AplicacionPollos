using AplicacionPollos.ViewModels;

namespace AplicacionPollos.Views;

public partial class EscanearView : ContentPage
{
    CajasViewModel contexto = new();
	public EscanearView()
	{
		InitializeComponent();
        barcodeReader.Options = new ZXing.Net.Maui.BarcodeReaderOptions
        {
            AutoRotate = true,
            Multiple = true
        };
    }
    private void barcodeReader_BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {
        var first = e.Results.FirstOrDefault();
        if (first is null)
            return;
        Dispatcher.DispatchAsync(async () =>
        {
            txtCodigoEscaneado.Text=first.Value;
            await Task.Delay(500);
        });
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        contexto.EscanearQR(txtCodigoEscaneado.Text);
    }
}