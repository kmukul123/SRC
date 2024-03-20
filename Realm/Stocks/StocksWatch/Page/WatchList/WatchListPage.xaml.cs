using DevExpress.Maui.CollectionView;
using StocksWatch.Page.HistoricalData;
using StocksWatch.Page.WatchList;

namespace StocksWatch.Page;

public partial class WatchListPage : ContentPage
{
	public WatchListPage(WatchListVM viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    void DXCollectionView_Tap(object sender, CollectionViewGestureEventArgs e)
    {
        var symbolViewModel = (CollectionItemViewModel)e.Item;
        var historicalDataViewModel = new HistoricalDataVM(symbolViewModel);
        Navigation.PushAsync(new HistoricalDataPage1(historicalDataViewModel));
    }
}