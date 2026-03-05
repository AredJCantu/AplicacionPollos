using AplicacionPollos.ViewModels;

namespace AplicacionPollos.Views;

public partial class AgregarCajaView : ContentPage
{
	public AgregarCajaView()
	{
		InitializeComponent();
	}
	CajasViewModel contexto = new();

    private void Entry_Completed(object sender, EventArgs e)
    {
		var codigo=txtCodigo.Text.Trim();
		if (string.IsNullOrWhiteSpace(codigo))
			return;
		// traer el rango de peso y peso del vm
		contexto.CalcularCaja(codigo);
		txtRango.Text = contexto.rango_Peso;
		txtPeso.Text = contexto.Peso;

    }
}