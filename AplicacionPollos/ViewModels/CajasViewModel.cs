using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AplicacionPollos.Models;
using CommunityToolkit.Mvvm.Input;
using AplicacionPollos.Repositories;

namespace AplicacionPollos.ViewModels
{
    public enum Vistas //Si quieren usar un string, diganme y lo cambio a un string.
    {
        Agregar,
        Editar,
        Eliminar,
        Principal,
        Dialogo //Para mostrar mensajes de error, a no ser que tengan otra idea para mostrarlos.
    }
    public class CajasViewModel : INotifyPropertyChanged
    {
        public CajasViewModel()
        {
            AgregarCommand = new RelayCommand(Agregar);
            EliminarCommand = new RelayCommand(Eliminar);
            EditarCommand = new RelayCommand(Editar);
            CambiarVistaCommand = new RelayCommand<Vistas>(CambiarVista);

            foreach (var caja in ObtenerCajas().Result) 
            {
                ListaCajas?.Add(caja);
            }
        }
        
        public async Task<List<CajasModel>> ObtenerCajas()
        {
            return await contexto.ObtenerCajasAsync();
        }

        CajasRepository contexto = new();
        public ObservableCollection<CajasModel> ListaCajas { get; set; }
        //-----------------------
        public CajasModel? CajaModel { get; set; }
        public List<string> ListaErrores { get; set; } = new();
        public Vistas VistaActual { get; set; }
        public ICommand AgregarCommand { get; set; }
        public string contadorCajas { get { return "Cajas: "+ListaCajas.Count(); } }
        public ICommand EliminarCommand { get; set; }
        public ICommand EditarCommand { get; set; } /* No creo que sea necesario, es imposible que se requiera editar a no ser que exista
                                                    *  error humano al momento de introducir manualmente el código de barras. (Eliminar de ser necesario) */
        public ICommand CambiarVistaCommand { get; set; }
        //TODO: propiedad del repositorio

        public event PropertyChangedEventHandler? PropertyChanged;

        private void ValidarEntrada(CajasModel? cajaModel)
        {
            if (cajaModel == null) return;

            if (cajaModel.peso < 0) ListaErrores.Add("El campo 'Peso neto' no puede ser menor o igual que 0. Revise la etiqueta.");
            if (cajaModel.numero_lote < 0) ListaErrores.Add("El campo 'No. Lote' no fue reconocido, revise el código de barras introducido.");

            //TODO: Validar el código de barras, si no será imposible reconocer los datos. (preferiblemente utilizando RegEx)
        }

        public void Agregar() 
        {
            ValidarEntrada(CajaModel);
            //TODO: instrucción para agregarlo a la base de datos, recargar datos.
            CajaModel = null; //Para evitar que se haga referencia a ella justo despues de argegar sin que el usuario la haya seleccionado. Otra opcion es quitarla de Agregar e incluirla en CambiarVista.
            CambiarVista(Vistas.Principal);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CajaModel)));
        }

        public void Eliminar() 
        {
            //TODO: Instrucción para eliminarla de la base de datos (repositorio)
            CambiarVista(Vistas.Principal);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VistaActual)));
        }

        public void Editar() 
        {
            //TODO: Instrucción para editarlo en la base de datos (repositorio)
            CambiarVista(Vistas.Principal);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VistaActual)));
        }

        private void CambiarVista(Vistas vista)
        {
            switch (vista)
            {
                case Vistas.Principal:
                    VistaActual = Vistas.Principal;
                    Shell.Current.GoToAsync("//Menu_Inicio");
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VistaActual)));
                    break;
                case Vistas.Agregar:
                    VistaActual = Vistas.Agregar;
                    CajaModel = new CajasModel();
                    Shell.Current.GoToAsync("//Agregar_Caja");
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VistaActual)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CajaModel)));
                    break;
                case Vistas.Eliminar:
                    if (CajaModel == null)
                    {
                        ListaErrores.Add("Seleccione una caja para eliminar."); /* Podemos también ignorar esta validación y simplemente no hacer nada cuando no haya nada seleccionado. 
                                                                                 * Pero nos arriesgamos a que un usuario piense que el programa no funciona al intentar eliminar una entrada nula. */
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ListaErrores)));
                        return;
                    }

                    VistaActual = Vistas.Eliminar;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VistaActual)));
                    break;
                case Vistas.Editar:
                    if (CajaModel == null)
                    {
                        ListaErrores.Add("Seleccione una caja para modificar la información.");
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ListaErrores)));
                        return;
                    }

                    //TODO: Crear un clon para editarlo en lugar de editar la caja original.

                    VistaActual = Vistas.Editar;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VistaActual)));
                    break;
            }
            //TODO: En caso de seguir usando la vista "Dialogo" modificar este metodo para tomar en cuenta esa vista también.

            ListaErrores.Clear();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaErrores)));
        }
    }
}
