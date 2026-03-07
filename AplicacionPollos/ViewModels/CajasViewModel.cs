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
    public enum Estandares 
    {
        Empiezan_Por_2,
        Pilgrim,
        Ninguno //Error
    }
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
        CajasRepository contexto = new();
        public ObservableCollection<CajasModel> ListaCajas { get; set; } = new();
        public CajasModel? CajaModel { get; set; } = new();
        public List<string> ListaErrores { get; set; } = new();
        public Vistas VistaActual { get; set; }
        public ICommand AgregarCommand { get; set; }
        public ICommand EliminarCommand { get; set; }
        public ICommand VerEditarCommand { get; set; }
        public ICommand EditarCommand { get; set; } 
        public string contadorCajas { get { return "Cajas: " + ListaCajas.Count(); } }
        public ICommand CambiarVistaCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        public CajasViewModel()
        {
            AgregarCommand = new RelayCommand<string>(Agregar);
            EliminarCommand = new RelayCommand(Eliminar);
            EditarCommand = new RelayCommand(Editar);
            CambiarVistaCommand = new RelayCommand<Vistas>(CambiarVista);
            VerEditarCommand = new RelayCommand<CajasModel>(VerEditar);
        }

        private void VerEditar(CajasModel? model) //TODO: Evaluar si se necesita.
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

        private Estandares ValidarCodigoBarras(string codigo_barras)
        {
            //TODO: Agregar los demás estándares
            if (string.IsNullOrWhiteSpace(codigo_barras)) return Estandares.Ninguno;

            if (codigo_barras.StartsWith('2') && codigo_barras.Length == 25) 
            {
                return Estandares.Empiezan_Por_2;
            }
            if (codigo_barras.Length > 30 && codigo_barras.StartsWith('0')) 
            {
                return Estandares.Pilgrim;
            }
            return Estandares.Ninguno;
        }

        private async Task<List<CajasModel>> ObtenerCajas() // TODO: Evaluar si se necesita o no
        {
            return await contexto.ObtenerCajasAsync();
        }

        public async void Agregar(string codigo_barras)
        {
            CajaModel = new();
            int temp_id = ListaCajas.LastOrDefault() == null ? 1 : ListaCajas.LastOrDefault().temp_id + 1;
            CajaModel.temp_id = temp_id;
            CajaModel.codigo_barras = codigo_barras;
            if (ListaCajas.Any(x => x.codigo_barras == codigo_barras)) return;
            switch (ValidarCodigoBarras(codigo_barras))
            {
                case Estandares.Empiezan_Por_2:
                    CajaModel.GTIN = codigo_barras.Substring(2, 4);
                    CajaModel.numero_lote = int.Parse(codigo_barras.Substring(6, 4));
                    CajaModel.numero_piezas = int.Parse(codigo_barras.Substring(11, 2));
                    CajaModel.peso = decimal.Parse(codigo_barras.Substring(12, 5)) / 100m; break;
                case Estandares.Pilgrim: //TODO: Identificar donde viene el número de piezas, o si es un producto estandarizado y no tiene variación en la cantidad de piezas.
                    CajaModel.GTIN = codigo_barras.Substring(0, 9);
                    CajaModel.numero_lote = int.Parse(codigo_barras.Substring(23, 10));
                    CajaModel.peso = decimal.Parse(codigo_barras.Substring(11, 5)) / 100m; break;
                default: ListaErrores.Add("ERROR BCR_01: Código de barras no identificado."); break;
            }
            ListaCajas.Add(CajaModel);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CajaModel)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaCajas)));
        }

        public void Eliminar()
        {
            ListaCajas.Remove(CajaModel);
            CambiarVista(Vistas.Agregar);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VistaActual)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaCajas)));
        }

        public void Editar()
        {
            if (CajaModel == null) return;
            int indice = ListaCajas.IndexOf(CajaModel);
            if (ListaCajas.Any(x => x.codigo_barras == CajaModel.codigo_barras) && ListaCajas[indice].temp_id != CajaModel.temp_id) return;
            ListaCajas[indice] = CajaModel;
            CambiarVista(Vistas.Agregar);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VistaActual)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaCajas)));
        }

        private void CambiarVista(Vistas vista)
        {
            switch (vista)
            {
                case Vistas.Agregar:
                    VistaActual = Vistas.Agregar;
                    Shell.Current.GoToAsync("//Agregar_Caja");
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VistaActual)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CajaModel)));
                    break;
                case Vistas.Eliminar:
                    if (CajaModel == null)
                    {
                        ListaErrores.Add("Seleccione una caja para eliminar.");
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaErrores)));
                        return;
                    }

                    VistaActual = Vistas.Eliminar;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VistaActual)));
                    break;
                case Vistas.Editar:
                    if (CajaModel == null)
                    {
                        ListaErrores.Add("Seleccione una caja para modificar la información.");
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaErrores)));
                        return;
                    }

                    CajasModel clon = new()
                    {
                        id = CajaModel.id,
                        GTIN = CajaModel.GTIN,
                        peso = CajaModel.peso,
                        numero_lote = CajaModel.numero_lote,
                        numero_piezas = CajaModel.numero_piezas,
                        codigo_barras = CajaModel.codigo_barras
                    };
                    CajaModel = clon;
                    Shell.Current.GoToAsync("//Editar_Caja");
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
