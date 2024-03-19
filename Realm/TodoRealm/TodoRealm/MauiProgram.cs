using Microsoft.Extensions.Logging;
using TodoRealm.Views;
using TodoRealm.VM;

namespace TodoRealm
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

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<DashBoard>();
            builder.Services.AddSingleton<DashBoardVM>();
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<LoginVM>();

            return builder.Build();
        }
    }
}
