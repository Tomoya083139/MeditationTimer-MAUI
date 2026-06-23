using Meditation.Services;
using Meditation.ViewModels;
using Meditation.Views;
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
            builder.Services.AddSingleton<IDiaryService, DiaryService>();   // 日記・記録サービス

            // ViewModels
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<CalendarViewModel>();             // カレンダー VM

            // Pages
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<CalendarPage>();                  // カレンダーページ

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
