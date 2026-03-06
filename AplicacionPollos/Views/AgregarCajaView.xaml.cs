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

    private void txtCodigo_Unfocused(object sender, FocusEventArgs e)
    {
        if (txtCodigo.Text != null)
        {
            var codigo = txtCodigo.Text.Trim();
		
            if (string.IsNullOrWhiteSpace(codigo) && codigo.Length <= 24)
                return;
            // traer el rango de peso y peso del vm
            contexto.CalcularCaja(codigo);
            txtRango.Text = contexto.rango_Peso;
            txtPeso.Text = contexto.Peso;
        }
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(100);
        txtCodigo.Focus();
    }
}