using AplicacionPollos.ViewModels;

namespace AplicacionPollos.Views;

public partial class EditarView : ContentPage
{
	CajasViewModel contexto = new();
	public EditarView()
	{
		InitializeComponent();

	}
}