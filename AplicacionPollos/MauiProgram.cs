using AplicacionPollos.ViewModels;
using AplicacionPollos.Views;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
using ZXing.Net.Maui.Controls;
namespace AplicacionPollos
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseBarcodeReader()
                .AddAudio()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "Material");
                });

            // Registrar el ViewModel como Singleton para compartir la misma instancia
            builder.Services.AddSingleton<CajasViewModel>();

            // Registrar las Views
            builder.Services.AddTransient<AgregarCajaView>();
            builder.Services.AddTransient<InventarioView>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
