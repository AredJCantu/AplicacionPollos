using AplicacionPollos.ViewModels;
using System.ComponentModel;
using ZXing;

namespace AplicacionPollos.Views;

public partial class AgregarCajaView : ContentPage
{
	Dictionary<string, byte> categorias = new() {
			{ "1254", 3 },
			{ "1255", 4 },
			{ "1256", 5},
			{ "1257", 6}
		};

	private CajasViewModel contexto => (CajasViewModel)BindingContext;

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
		var codigo=txtCodigo.Text.Trim();
		if (string.IsNullOrWhiteSpace(codigo))
			return;

		CalcularCaja(codigo);
		txtRango.Text = contexto.CajaModel.rango_peso.ToString();
		txtPeso.Text = contexto.CajaModel.peso.ToString();
	}

	private void txtCodigo_Unfocused(object sender, FocusEventArgs e)
	{
		if (txtCodigo.Text != null)
		{
			var codigo = txtCodigo.Text.Trim();

			if (string.IsNullOrWhiteSpace(codigo) || codigo.Length <= 24)
				return;

			CalcularCaja(codigo);
			txtRango.Text = contexto.CajaModel.rango_peso.ToString();
			txtPeso.Text = contexto.CajaModel.peso.ToString();
		}
	}

	public void CalcularCaja(string codigo)
	{
		if (codigo.Length < 16)
			return;

		string codigoCategoria = codigo.Substring(2, 4);

		if (categorias.ContainsKey(codigoCategoria))
		{
			contexto.CajaModel.rango_peso = categorias[codigoCategoria];
		}

		string pesoStr = codigo.Substring(12, 4);
		if (decimal.TryParse(pesoStr, out decimal peso))
		{
			contexto.CajaModel.peso = peso / 100;
		}

		contexto.CajaModel.codigo_barras = codigo;
		contexto.CajaModel.numero_lote = int.Parse(codigoCategoria);
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
		Dispatcher.DispatchAsync(async () =>
		{
			txtCodigo.Text = first.Value;
			await Task.Delay(500);
		});
	}

}