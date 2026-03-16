using AplicacionPollos.Models;
using AplicacionPollos.ViewModels;

namespace AplicacionPollos.Views;

public partial class InventarioView : ContentPage
{
	private SwipeView _AbiertoActualmente;
	CajasViewModel contexto;

	public InventarioView(CajasViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
		contexto = viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		contexto?.verInventario();
	}

	private void SwipeView_SwipeStarted(object sender, SwipeStartedEventArgs e)
	{
		var swipeViewActual = sender as SwipeView;
		if (_AbiertoActualmente != null && _AbiertoActualmente != swipeViewActual)
		{
			_AbiertoActualmente.Close();
		}
		_AbiertoActualmente = swipeViewActual;
	}

	private async void SwipeItem_Clicked(object sender, EventArgs e)
	{
		await DisplayAlert("Información", "Función de edición desde inventario no implementada aún", "Aceptar");
	}

	private async void Eliminar_Clicked(object sender, EventArgs e)
	{
		var objSwipe = (SwipeItem)sender;
		CajasModel caja_Parametro = (CajasModel)objSwipe.CommandParameter;
		bool res = await DisplayAlert("Confirmación", "¿Está seguro de eliminar esta caja de la base de datos?", "Confirmar", "Cancelar");
		if (res)
		{
			contexto.EliminarDesdeBD(caja_Parametro.id);
		}
	}

	private async void BtnVolver_Clicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//Agregar_Caja");
		contexto.ListaCajas.Clear();
    }
}