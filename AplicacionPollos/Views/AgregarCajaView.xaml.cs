using AplicacionPollos.ViewModels;

namespace AplicacionPollos.Views;

public partial class AgregarCajaView : ContentPage
{
	public AgregarCajaView()
	{
		InitializeComponent();
	}
	CajasViewModel contexto = new();
    private void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
		string s=e.NewTextValue;
		if (string.IsNullOrWhiteSpace(s) && s.Length == 25) {
			//Traer datos con el regex
			txtRango.Text =contexto.CajaModel.rango_peso.ToString();
			txtPeso.Text= contexto.CajaModel.peso.ToString();
        }
    }
}