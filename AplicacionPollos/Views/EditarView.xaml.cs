using AplicacionPollos.ViewModels;

namespace AplicacionPollos.Views;

public partial class EditarView : ContentPage
{
	private CajasViewModel contexto => (CajasViewModel)BindingContext;

	public EditarView()
	{
		InitializeComponent();
	}
}