using AplicacionPollos.Models;
using AplicacionPollos.Repositories;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.Audio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
        Dictionary<string, byte> categorias = new() {
            { "1254", 3 },
            { "1255", 4 },
            { "1256", 5},
            { "1257", 6}
        };
        CajasRepository contexto = new();
        public ObservableCollection<CajasModel> ListaCajas { get; set; } = new();
        public CajasModel? CajaModel { get; set; } = new();
        public List<string> ListaErrores { get; set; } = new();
        public Vistas VistaActual { get; set; }
        public ICommand AgregarCommand { get; set; }
        public ICommand VerEditarCommand { get; set; }
        public ICommand EditarCommand { get; set; } 
        public string contadorCajas { get { return "Cajas: " + ListaCajas.Count(); } }
        public ICommand CambiarVistaCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        public CajasViewModel()
        {
            AgregarCommand = new RelayCommand<string>(Agregar);
            EditarCommand = new RelayCommand(Editar);
            CambiarVistaCommand = new RelayCommand<Vistas>(CambiarVista);
            VerEditarCommand = new RelayCommand<CajasModel>(VerEditar);
        }

        private void VerEditar(CajasModel? model) //TODO: Evaluar si se necesita.
        {
            if (model!=null)
            {
                CajaModel = new CajasModel
                {
                    id = model.id,
                    temp_id = model.temp_id,
                    codigo_barras = model.codigo_barras,
                    GTIN = model.GTIN,
                    numero_lote = model.numero_lote,
                    peso = model.peso,
                    numero_piezas = model.numero_piezas,
                    rango_peso = model.rango_peso
                };
                ListaErrores.Clear();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CajaModel)));
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

        private bool TryParseSubstring(string source, int startIndex, int length, out string result)
        {
            result = null;
            if (source == null || startIndex + length > source.Length)
                return false;

            try
            {
                result = source.Substring(startIndex, length);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<List<CajasModel>> ObtenerCajas() // TODO: Evaluar si se necesita o no
        {
            return await contexto.ObtenerCajasAsync();
        }

        public async void Agregar(string codigo_barras)
        {
            if (string.IsNullOrWhiteSpace(codigo_barras) || ListaCajas.Any(x => x.codigo_barras == codigo_barras))
                return;
            CajasModel cajaParaLista = new();
            int temp_id = ListaCajas.LastOrDefault() == null ? 1 : ListaCajas.LastOrDefault().temp_id + 1;
            cajaParaLista.temp_id = temp_id;
            cajaParaLista.codigo_barras = codigo_barras;
            try {
                switch (ValidarCodigoBarras(codigo_barras))
                {
                    case Estandares.Empiezan_Por_2:
                        // Validar longitud mínima y extraer subcadenas de forma segura
                        if (!TryParseSubstring(codigo_barras, 2, 4, out var gtin) ||
                            !TryParseSubstring(codigo_barras, 6, 4, out var lote_str) ||
                            !TryParseSubstring(codigo_barras, 11, 2, out var piezas_str) ||
                            !TryParseSubstring(codigo_barras, 12, 5, out var peso_str))
                        {
                            ListaErrores.Add("ERROR BCR_02: Formato de código inválido para estándar Empiezan_Por_2.");
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaErrores)));
                            return;
                        }

                        if (!int.TryParse(lote_str, out var numero_lote) ||
                            !int.TryParse(piezas_str, out var numero_piezas) ||
                            !decimal.TryParse(peso_str, out var peso_valor))
                        {
                            ListaErrores.Add("ERROR BCR_03: No se pudieron parsear los valores numéricos.");
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaErrores)));
                            return;
                        }

                        if (!categorias.ContainsKey(gtin))
                        {
                            ListaErrores.Add($"ERROR BCR_04: GTIN '{gtin}' no encontrado en categorías.");
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaErrores)));
                            return;
                        }

                        cajaParaLista.GTIN = gtin;
                        cajaParaLista.numero_lote = numero_lote;
                        cajaParaLista.numero_piezas = numero_piezas;
                        cajaParaLista.peso = peso_valor / 1000m;
                        cajaParaLista.rango_peso = categorias[gtin];
                        break;

                    case Estandares.Pilgrim:
                        //TODO: Identificar donde viene el número de piezas, o si es un producto estandarizado y no tiene variación en la cantidad de piezas.
                        if (!TryParseSubstring(codigo_barras, 0, 9, out var gtin_pilgrim) ||
                            !TryParseSubstring(codigo_barras, 23, 10, out var lote_pilgrim_str) ||
                            !TryParseSubstring(codigo_barras, 11, 5, out var peso_pilgrim_str))
                        {
                            ListaErrores.Add("ERROR BCR_05: Formato de código inválido para estándar Pilgrim.");
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaErrores)));
                            return;
                        }

                        if (!int.TryParse(lote_pilgrim_str, out var numero_lote_pilgrim) ||
                            !decimal.TryParse(peso_pilgrim_str, out var peso_valor_pilgrim))
                        {
                            ListaErrores.Add("ERROR BCR_06: No se pudieron parsear los valores numéricos del estándar Pilgrim.");
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaErrores)));
                            return;
                        }

                        cajaParaLista.GTIN = gtin_pilgrim;
                        cajaParaLista.numero_lote = numero_lote_pilgrim;
                        cajaParaLista.peso = peso_valor_pilgrim / 100m;
                        break;

                    default: 
                        ListaErrores.Add("ERROR BCR_01: Código de barras no identificado."); 
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaErrores)));
                        return;
                }
            }
            catch(Exception ex) {
                ListaErrores.Add($"Error procesando código: {ex.Message}");
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaErrores)));
                return;
            }
            ListaCajas.Add(cajaParaLista);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(contadorCajas)));
            //Reproduccion de sonido beep
            try {
                var stream = await FileSystem.OpenAppPackageFileAsync("beep.mp3");
                var reproductor = AudioManager.Current.CreatePlayer(stream);
                reproductor.Play();
            }
            catch { }


            CajaModel = new() {
                temp_id = cajaParaLista.temp_id,
                codigo_barras = cajaParaLista.codigo_barras,
                GTIN = cajaParaLista.GTIN,
                numero_lote = cajaParaLista.numero_lote,
                peso = cajaParaLista.peso,
                numero_piezas = cajaParaLista.numero_piezas,
                rango_peso = cajaParaLista.rango_peso
            };
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CajaModel)));

        }

        public void Eliminar(CajasModel copia)
        {
            ListaCajas.Remove(copia);
            CajaModel = new();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CajaModel)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(contadorCajas)));
        }

        public void Editar()
        {
            if (CajaModel == null) return;
            int indice = CajaModel.temp_id;
            if(indice<1) return;
            if (ListaCajas.Any(x => x.codigo_barras == CajaModel.codigo_barras) && ListaCajas[indice-1].temp_id != CajaModel.temp_id) return;
            ListaCajas[indice-1] = CajaModel;
            CambiarVista(Vistas.Agregar);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VistaActual)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(contadorCajas)));
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
