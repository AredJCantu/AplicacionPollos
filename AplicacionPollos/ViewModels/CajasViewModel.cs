using AplicacionPollos.Models;
using AplicacionPollos.Repositories;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.Audio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AplicacionPollos.ViewModels
{
    public enum Estandar
    {
        Empiezan_Por_2,
        Pilgrim,
        Ninguno
    }
    public class CajasViewModel : INotifyPropertyChanged
    {
        // --- Campos Privados ---
        private readonly Dictionary<string, byte> categorias = new() {
            { "1254", 3 },
            { "1255", 4 },
            { "1256", 5},
            { "1257", 6},
            { "8631", 0 },
            { "8609", 0 },
            { "8629", 0 },
            { "40372",10 }
        };
        Dictionary<int, Estandar> Estandares = new()
        {
            {0, Estandar.Empiezan_Por_2 },
            {1, Estandar.Pilgrim },
        };
        private GestionadorCajas contexto = new();

        // --- Eventos ---
        public event PropertyChangedEventHandler? PropertyChanged;

        // --- Propiedades de Estado y UI ---
        public bool fondoHabilitado { get; set; }=true;
        public bool EsAnomalia { get; set; } = false;
        public bool Menu { get; set; }=false;
        public bool VerEliminar { get; set; }=false;
        public bool EditarEntrys { get; set; } = false;
        public bool VistaMensaje { get; set; } = false;
        public string MensajeAlerta { get; set; } = string.Empty;
        public string contadorCajas => "Cajas: " + ListaCajas.Count;

        // --- Colecciones y Modelos ---
        public List<string> ListaErrores { get; set; } = new();
        public ObservableCollection<CajasModel> ListaCajas { get; set; } = new();
        public ObservableCollection<CajasModel> ListaCajasCompleta { get; set; } = new();
        public CajasModel? CajaModel { get; set; } = new();
        public CajasModel? CajaAnomalia { get; set; }
        public List<string> Patrones { get; set; } = new() //regex
        {
            @"^27(\d{4})(\d{4})\d{2}(\d{5})\d{7}A$", //no se de que empresa es, pero es el primer patrón
            @"^0{4}(\d{5})\d{2}(\d{5})\d{15}"        //Pilgrim
        };

        // --- Comandos (ICommand) ---
        public ICommand GuardarCommand { get; set; }
        public ICommand EditarCommand { get; set; }
        public ICommand EliminarCommand { get; set; }
        public ICommand VerEditarCommand { get; set; }
        public ICommand CambiarVistaCommand { get; set; }
        public ICommand ImprimirReporteCommand { get; set; }
        public ICommand VerEliminarCommand { get; set; }
        public ICommand VerMenuCommand { get; set; }
        public ICommand CerrarMenuCommand { get; set; }
        public ICommand OkCommand { get; set; }
        public ICommand VerAgregarManualCommand { get; set; }
        public ICommand VolverCommand { get; set; }

        // --- Constructor ---
        public CajasViewModel()
        {
            GuardarCommand = new RelayCommand<string>(Guardar);
            EditarCommand = new RelayCommand(Editar);
            VerEliminarCommand = new RelayCommand(VerEliminarMenu);
            EliminarCommand =new RelayCommand<CajasModel>(Eliminar);
            VerEditarCommand = new RelayCommand<CajasModel>(VerEditar);
            ImprimirReporteCommand = new RelayCommand(ImprimirReporte);
            OkCommand = new RelayCommand(Ok);
            VerMenuCommand = new RelayCommand(VerMenu);
            CerrarMenuCommand = new RelayCommand(CerrarMenu);
            VerAgregarManualCommand = new RelayCommand(VerAgregarManual);
            VolverCommand=new RelayCommand(Volver);
            contexto.EliminarTodasLasCajas();
        }

        private void Volver()
        {
            EsAnomalia = false;
            fondoHabilitado = true;
            CajaModel = new();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        private void VerAgregarManual()
        {
            EsAnomalia= true;
            fondoHabilitado = false;
            Ok();
            CajaAnomalia = new();
            CajaAnomalia.codigo_barras = CajaModel.codigo_barras;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        private void VerEliminarMenu()
        {
            VerEliminar = true;
            Menu = false;
            fondoHabilitado = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(fondoHabilitado)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Menu)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VerEliminar)));
        }

        private void Guardar(string codigo_barras)
        {
            // Por precaución, si el CommandParameter llega vacío, tomamos el del Binding
            if (string.IsNullOrWhiteSpace(codigo_barras))
            {
                codigo_barras = CajaModel?.codigo_barras ?? "";
            }
            if (EditarEntrys)
            {
                Editar();
                EditarEntrys= false;
                CajaModel = new();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CajaModel)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditarEntrys)));
            }
            else
            {
                Agregar(codigo_barras);
                if(EsAnomalia)
                    AgregarAnomalia();
            }
        }

        public void CerrarMenu()
        {
            Menu = false;
            fondoHabilitado = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(fondoHabilitado)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Menu)));
        }

        private void VerMenu()
        {
            Menu = true;
            fondoHabilitado = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(fondoHabilitado)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Menu)));
        }

        // --- Métodos de Acción (Ejecutados por Comandos) ---
        public async void Agregar(string codigo_barras)
        {
            if (string.IsNullOrWhiteSpace(codigo_barras) || ListaCajas.Any(x => x.codigo_barras == codigo_barras))
                return;
            CajasModel cajaParaLista = new();
            int temp_id = ListaCajas.LastOrDefault() == null ? 1 : ListaCajas.LastOrDefault().temp_id + 1;
            cajaParaLista.temp_id = temp_id;
            cajaParaLista.codigo_barras = codigo_barras;
            if (EsAnomalia) {
                cajaParaLista.GTIN=CajaAnomalia.GTIN;
                cajaParaLista.numero_lote=CajaAnomalia.numero_lote;
                cajaParaLista.numero_piezas=CajaAnomalia.numero_piezas;
                cajaParaLista.peso=CajaAnomalia.peso;
                cajaParaLista.rango_peso=CajaAnomalia.rango_peso;
                ListaCajas.Add(cajaParaLista);
                EditarEntrys = false;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditarEntrys)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(contadorCajas)));
                try
                {
                    var stream = await FileSystem.OpenAppPackageFileAsync("beep.mp3");
                    var reproductor = AudioManager.Current.CreatePlayer(stream);
                    reproductor.Play();
                }
                catch
                {
                    //fakiu rango de peso
                }
                EsAnomalia= false;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EsAnomalia)));
            } else {

                bool parseoExitoso = ParsearCodigoDeBarras(codigo_barras, cajaParaLista);
                if (!parseoExitoso) return;

                ListaCajas.Add(cajaParaLista);
                EditarEntrys = false;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditarEntrys)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(contadorCajas)));

                try
                {
                    var stream = await FileSystem.OpenAppPackageFileAsync("beep.mp3");
                    var reproductor = AudioManager.Current.CreatePlayer(stream);
                    reproductor.Play();
                }
                catch
                {
                    //fakiu rango de peso
                }
            }
            CajaModel = new()
            {
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

        public void Editar()
        {
            if (CajaModel == null) return;
            int indice = CajaModel.temp_id;
            if (indice <= 0) return;

            CajasModel cajaOriginal = ListaCajas[indice - 1];
            bool codigoBarrasChangio = cajaOriginal.codigo_barras != CajaModel.codigo_barras;

            if (codigoBarrasChangio)
            {
                if (ListaCajas.Any(x => x.codigo_barras == CajaModel.codigo_barras && x.temp_id != CajaModel.temp_id))
                    return;

                if (!ParsearCodigoDeBarras(CajaModel.codigo_barras, CajaModel))
                    return;
            }

            ListaCajas[indice - 1] = CajaModel;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaCajas)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CajaModel)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(contadorCajas)));
        }

        public void Eliminar(CajasModel copia)
        {
            EditarEntrys = false;
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(EditarEntrys)));
            ListaCajas.Remove(copia);
            CajaModel = new();
            Ok();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CajaModel)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(contadorCajas)));
        }

        public void EnviarDatos()
        {
            if (ListaCajas.Count == 0)
            {
                ListaErrores.Add("No hay cajas que enviar");
                ActualizarMensajeUI();
                return;
            }

            contexto.AgregarCajas(ListaCajas);

            if (ListaErrores.Count > 0)
            {
                ActualizarMensajeUI();
            }
            else
            {
                MensajeAlerta = "Datos enviados correctamente";
                VistaMensaje = true;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MensajeAlerta)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VistaMensaje)));

                ListaCajas.Clear();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(contadorCajas)));
                CajaModel = new();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CajaModel)));
            }
        }

        // --- Desuso ---
        //public void EliminarDesdeBD(int id)
        //{
        //    bool resultado = contexto.EliminarCaja(id);
        //    if (resultado)
        //    {
        //        var cajaAEliminar = ListaCajas.FirstOrDefault(c => c.id == id);
        //        if (cajaAEliminar != null)
        //        {
        //            ListaCajas.Remove(cajaAEliminar);
        //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaCajas)));
        //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(contadorCajas)));
        //        }
        //    }
        //    else
        //    {
        //        ListaErrores.AddRange(contexto.Errores);
        //        ActualizarMensajeUI();
        //    }
        //}

        public async void ImprimirReporte()
        {
            await contexto.ImprimirExcel();
        }

        public void Ok()
        {
            if (VerEliminar) {
                VerEliminar = false;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VerEliminar)));
                fondoHabilitado = true;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(fondoHabilitado)));
            } else {
                VistaMensaje = false;
                ListaErrores.Clear();
                MensajeAlerta = string.Empty;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VistaMensaje)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MensajeAlerta)));
                fondoHabilitado = true;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(fondoHabilitado)));
            }
        }

        // --- Métodos de Navegación y Vistas ---
        private void VerEditar(CajasModel? model)
        {
            fondoHabilitado = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(fondoHabilitado)));
            if (model != null)
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
                Menu= false;
                EditarEntrys = true;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditarEntrys)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Menu)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CajaModel)));
            }
        }

        

        // --- Lógica de Negocio (Parseo y Validaciones) ---
        public bool ParsearCodigoDeBarras(string codigo_barras, CajasModel cajaParaLista)
        {
            try
            {
                 switch (ValidarCodigoBarras(codigo_barras))
                {
                    case Estandar.Empiezan_Por_2:
                        // Validar longitud mínima y extraer subcadenas de forma segura
                        if (!TryParseSubstring(codigo_barras, 2, 4, out var gtin) ||
                            !TryParseSubstring(codigo_barras, 6, 4, out var lote_str) ||
                            !TryParseSubstring(codigo_barras, 11, 2, out var piezas_str) ||
                            !TryParseSubstring(codigo_barras, 12, 4, out var peso_str))
                        {
                            ListaErrores.Add("ERROR BCR_02: Formato de código inválido.");
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaErrores)));
                            return false;
                        }

                        if (!int.TryParse(lote_str, out var numero_lote) ||
                            !int.TryParse(piezas_str, out var numero_piezas) ||
                            !decimal.TryParse(peso_str, out var peso_valor))
                        {
                            ListaErrores.Add("ERROR BCR_03: No se pudieron convertir los valores numéricos.");
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaErrores)));
                            return false;
                        }

                        if (!categorias.ContainsKey(gtin))
                        {
                            ListaErrores.Add($"ERROR BCR_04: GTIN '{gtin}' no encontrado en categorías.");
                            ActualizarMensajeUI();
                            return false;
                        }
                        //Esta agregando
                        if (EditarEntrys==false)
                        {
                            cajaParaLista.GTIN = gtin;
                            cajaParaLista.numero_lote = numero_lote;
                            cajaParaLista.numero_piezas = numero_piezas;
                            cajaParaLista.peso = peso_valor / 100m;
                            cajaParaLista.rango_peso = categorias[gtin];
                        }
                        break;

                    case Estandar.Pilgrim:
                        //TODO: Identificar donde viene el número de piezas, o si es un producto estandarizado y no tiene variación en la cantidad de piezas.
                        if (!TryParseSubstring(codigo_barras, 3, 5, out var gtin_pilgrim) ||
                            !TryParseSubstring(codigo_barras, 24, 7, out var lote_pilgrim_str) ||
                            !TryParseSubstring(codigo_barras, 11, 5, out var peso_pilgrim_str))
                        {
                            ListaErrores.Add("ERROR BCR_05: Formato de código inválido para estándar Pilgrim.");
                            ActualizarMensajeUI();
                            return false;
                        }

                        if (!int.TryParse(lote_pilgrim_str, out var numero_lote_pilgrim) ||
                            !decimal.TryParse(peso_pilgrim_str, out var peso_valor_pilgrim))
                        {
                            ListaErrores.Add("ERROR BCR_06: No se pudieron parsear los valores numéricos del estándar Pilgrim.");
                            ActualizarMensajeUI();
                            return false;
                        }
                        //Esta agregando
                        if (EditarEntrys==false)
                        {
                            cajaParaLista.GTIN = gtin_pilgrim;
                            cajaParaLista.numero_lote = numero_lote_pilgrim;
                            cajaParaLista.peso = peso_valor_pilgrim / 100m;
                        }

                        break;

                    default:
                        ListaErrores.Add("ERROR BCR_01: Código de barras no identificado.");
                        ActualizarMensajeUI();
                        Vibration.Default.Vibrate(1000);
                        HabilitarEntrys();
                        return false;
                }
            }
            catch (Exception ex)
            {
                ListaErrores.Add($"Error procesando código: {ex.Message}");
                // -- Sonido de error --
                ActualizarMensajeUI();
                return false;
            }

            return true;
        }
        private void AgregarAnomalia()
        {
            //Usar la api para enviar los datos a la base de datos de Mysql
        }
        private void HabilitarEntrys()
        {
            ActualizarMensajeUI();
            EditarEntrys = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditarEntrys)));
        }

        private Estandar ValidarCodigoBarras(string codigo_barras)
        {
            //TODO: Agregar los demás estándares
            if (string.IsNullOrWhiteSpace(codigo_barras)) return Estandar.Ninguno;

            for (int i = 0; i <= Patrones.Count; i++)
            {
                if (Regex.IsMatch(codigo_barras, Patrones[i]))
                {
                    return Estandares[i];
                }
            }

            return Estandar.Ninguno;
        }

        private bool TryParseSubstring(string source, int startIndex, int length, out string result)
        {
            result = string.Empty;
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

        private async void ActualizarMensajeUI()
        {
            try
            {
                var stream = await FileSystem.OpenAppPackageFileAsync("Error.mp3");
                var reproductor = AudioManager.Current.CreatePlayer(stream);
                reproductor.Play();
                Vibration.Default.Vibrate(1000);
            }
            catch
            {
            }
            MensajeAlerta = string.Join("\n", ListaErrores);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MensajeAlerta)));
            VistaMensaje = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VistaMensaje)));
            fondoHabilitado = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(fondoHabilitado)));
        }
    }
}