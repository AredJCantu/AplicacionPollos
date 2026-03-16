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
    public enum Vistas
    {
        Agregar,
        Editar,
        Eliminar,
        Principal,
        Dialogo
    }
    public class CajasViewModel : INotifyPropertyChanged
    {
        GestionadorCajas contexto = new();
        public ObservableCollection<CajasModel> ListaCajas { get; set; }
        public CajasModel? CajaModel { get; set; } = new();
        public List<string> ListaErrores { get; set; } = new();
        public Vistas VistaActual { get; set; }
        public ICommand AgregarCommand { get; set; }
        public ICommand EliminarCommand { get; set; }
        public ICommand VerEditarCommand { get; set; }
        public ICommand EditarCommand { get; set; }
        public string contadorCajas { get { return "Cajas: " + ListaCajas.Count(); } }
        public ICommand CambiarVistaCommand { get; set; }
        public ICommand CargarCajasCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public CajasViewModel()
        {
            AgregarCommand = new RelayCommand(Agregar);
            EliminarCommand = new RelayCommand(Eliminar);
            EditarCommand = new RelayCommand(Editar);
            CambiarVistaCommand = new RelayCommand<Vistas>(CambiarVista);
            VerEditarCommand = new RelayCommand<CajasModel>(VerEditar);
            CargarCajasCommand = new RelayCommand(CargarCajas);

            ListaCajas = new ObservableCollection<CajasModel>();
            CargarCajas();
        }

        private void CargarCajas()
        {
            ListaCajas.Clear();
            var cajas = contexto.GetAll();
            foreach (var caja in cajas)
            {
                ListaCajas.Add(caja);
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaCajas)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(contadorCajas)));
        }

        private void VerEditar(CajasModel? model)
        {
            if (model != null)
            {
                VistaActual = Vistas.Editar;
                CambiarVista(VistaActual);
                CajaModel = new()
                {
                    id = model.id,
                    peso = model.peso,
                    rango_peso = model.rango_peso,
                    numero_lote = model.numero_lote,
                    codigo_barras = model.codigo_barras,
                    numero_id = model.numero_id,
                    numero_empleado = model.numero_empleado,
                    numero_planta = model.numero_planta,
                    numero_piezas = model.numero_piezas,
                    id_producto = model.id_producto,
                    proveedor = model.proveedor
                };
                ListaErrores.Clear();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            }
        }

        public void Agregar()
        {
            if (CajaModel == null)
            {
                ListaErrores = new List<string> { "No hay datos para agregar." };
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaErrores)));
                return;
            }

            bool resultado = contexto.AgregarCaja(CajaModel);

            if (!resultado)
            {
                ListaErrores = contexto.Errores;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaErrores)));
                return;
            }

            CargarCajas();
            CajaModel = new CajasModel();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CajaModel)));
        }

        public void Eliminar()
        {
            if (CajaModel == null)
            {
                ListaErrores = new List<string> { "Seleccione una caja para eliminar." };
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaErrores)));
                return;
            }

            bool resultado = contexto.EliminarCaja(CajaModel.id);

            if (!resultado)
            {
                ListaErrores = contexto.Errores;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaErrores)));
                return;
            }

            CargarCajas();
            CajaModel = new CajasModel();
            CambiarVista(Vistas.Agregar);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VistaActual)));
        }

        public void Editar()
        {
            if (CajaModel == null)
            {
                ListaErrores = new List<string> { "No hay datos para editar." };
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaErrores)));
                return;
            }

            bool resultado = contexto.ActualizarCaja(CajaModel);

            if (!resultado)
            {
                ListaErrores = contexto.Errores;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaErrores)));
                return;
            }

            CargarCajas();
            CajaModel = new CajasModel();
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
                        ListaErrores.Add("Seleccione una caja para eliminar.");
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

                    Shell.Current.GoToAsync("//Editar_Caja");
                    VistaActual = Vistas.Editar;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VistaActual)));
                    break;
            }

            ListaErrores.Clear();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaErrores)));
        }
    }
}
