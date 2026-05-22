using Meditation.Services;
using Meditation.ViewModels;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;

namespace Meditation
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Services
            builder.Services.AddSingleton<IDispatcher>(services => Application.Current?.Dispatcher ?? throw new InvalidOperationException("Dispatcher not found"));
            builder.Services.AddSingleton<IAudioService, AudioService>();
            builder.Services.AddSingleton<CountdownTimerService>();

            // ViewModels
            builder.Services.AddTransient<MainViewModel>();

            // Pages
            builder.Services.AddTransient<MainPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
