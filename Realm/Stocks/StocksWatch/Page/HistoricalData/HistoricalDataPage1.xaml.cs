using StocksWatch.Page.HistoricalData;

namespace StocksWatch.Page;

public partial class HistoricalDataPage1 : ContentPage
{
	public HistoricalDataPage1(HistoricalDataVM viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
        Title = viewModel.Item.Ticker;
    }
}