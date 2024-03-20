using StocksWatch.Page.HistoricalData;

namespace StocksWatch.Page;

public partial class HistoricalDataPage : ContentPage
{
	public HistoricalDataPage(HistoricalDataVM viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
        Title = viewModel.Item.Ticker;
    }
}