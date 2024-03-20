
using DevExpress.Maui;
using StocksWatch.Page;
using StocksWatch.Page.HistoricalData;

namespace StocksWatch;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkitMediaElement()
            .UseMauiCommunityToolkit()
            .UseDevExpress()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("FontAwesome6FreeBrands.otf", "FontAwesomeBrands");
                fonts.AddFont("FontAwesome6FreeRegular.otf", "FontAwesomeRegular");
                fonts.AddFont("FontAwesome6FreeSolid.otf", "FontAwesomeSolid");
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.RegisterServices();

        return builder.Build();
    }

    private static void RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton<MainViewModel>();

        services.AddSingleton<MainPage>();

        services.AddTransient<SampleDataService>();
        //services.AddTransient<ListDetailDetailViewModel>();
        //services.AddTransient<ListDetailDetailPage>();

        services.AddSingleton<ListDetailViewModel>();
        services.AddSingleton<ListDetailPage>();

        services.AddSingleton<MediaElementViewModel>();
        services.AddSingleton<MediaElementPage>();

        services.AddSingleton<WebViewViewModel>();
        services.AddSingleton<WebViewPage>();

        services.AddSingleton<LocalizationViewModel>();
        services.AddSingleton<LocalizationPage>();

        services.AddSingleton<WatchListPage>();
        services.AddSingleton<Page.WatchList.WatchListVM>();

        services.UsePageResolver();
    }

}
