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
        Dictionary<string, byte> categorias = new() {
            { "1254", 3 },
            { "1255", 4 },
            { "1256", 5},
            { "1257", 6}
        };
        CajasRepository contexto = new();
        public ObservableCollection<CajasModel> ListaCajas { get; set; }
        //-----------------------
        public CajasModel? CajaModel { get; set; } = new();
        public List<string> ListaErrores { get; set; } = new();
        public Vistas VistaActual { get; set; }
        public ICommand AgregarCommand { get; set; }
        public ICommand EliminarCommand { get; set; }
        public ICommand VerEditarCommand { get; set; }
        public ICommand EditarCommand { get; set; } /* No creo que sea necesario, es imposible que se requiera editar a no ser que exista
                                                    *  error humano al momento de introducir manualmente el código de barras. (Eliminar de ser necesario) */
        public string contadorCajas { get { return "Cajas: " + ListaCajas.Count(); } }
        public string rango_Peso { get; set; }
        public string Peso { get; set; }
        public ICommand CambiarVistaCommand { get; set; }
        //TODO: propiedad del repositorio

        public event PropertyChangedEventHandler? PropertyChanged;
        public CajasViewModel()
        {
            AgregarCommand = new RelayCommand(Agregar);
            EliminarCommand = new RelayCommand(Eliminar);
            EditarCommand = new RelayCommand(Editar);
            CambiarVistaCommand = new RelayCommand<Vistas>(CambiarVista);
            VerEditarCommand = new RelayCommand<CajasModel>(VerEditar);
            //foreach (var caja in ObtenerCajas().Result)
            //{
            //    ListaCajas?.Add(caja);
            //}
            ListaCajas = new() {
                { new CajasModel() { id = 1, rango_peso = 3, numero_lote = 1254, peso=23.56m } },
                { new CajasModel() { id = 2, rango_peso = 4, numero_lote = 1255, peso=26.6m } },
                { new CajasModel() { id = 3, rango_peso = 5, numero_lote = 1256, peso=20.2m } },
                { new CajasModel() { id = 4, rango_peso = 6, numero_lote = 1257, peso=27.79m } }
            };
        }

        private void VerEditar(CajasModel? model)
        {
            if (model!=null)
            {
                VistaActual = Vistas.Editar;
                CambiarVista(VistaActual);
                CajaModel = new() {
                    id=model.id,
                peso=model.peso,
                rango_peso=model.rango_peso,
                numero_lote=model.numero_lote
                };
                ListaErrores.Clear();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            }
        }

        private void ValidarEntrada(CajasModel? cajaModel)
        {
            if (cajaModel == null) return;

            if (cajaModel.peso < 0) ListaErrores.Add("El campo 'Peso neto' no puede ser menor o igual que 0. Revise la etiqueta.");
            if (cajaModel.numero_lote < 0) ListaErrores.Add("El campo 'No. Lote' no fue reconocido, revise el código de barras introducido.");

            //TODO: Validar el código de barras, si no será imposible reconocer los datos. (preferiblemente utilizando RegEx)
        }

        public async Task<List<CajasModel>> ObtenerCajas()
        {
            return await contexto.ObtenerCajasAsync();
        }

        public void Agregar() 
        {
            ValidarEntrada(CajaModel);
            //TODO: instrucción para agregarlo a la base de datos, recargar datos.
            CajaModel = null; //Para evitar que se haga referencia a ella justo despues de argegar sin que el usuario la haya seleccionado. Otra opcion es quitarla de Agregar e incluirla en CambiarVista.
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
            if (CajaModel != null)
            {
                ValidarEntrada(CajaModel);
            }
            //TODO: Instrucción para editarlo en la base de datos (repositorio)
            CambiarVista(Vistas.Agregar);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VistaActual)));
        }

        private void CambiarVista(Vistas vista)
        {
            switch (vista)
            {
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
                    Shell.Current.GoToAsync("//Editar_Caja");
                    VistaActual = Vistas.Editar;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VistaActual)));
                    break;
            }
            //TODO: En caso de seguir usando la vista "Dialogo" modificar este metodo para tomar en cuenta esa vista también.

            ListaErrores.Clear();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaErrores)));
        }
        public void CalcularCaja(string codigo)
        {
                rango_Peso = categorias[codigo.Substring(2, 4)].ToString();
                Peso = codigo.Substring(12, 4);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }
}
