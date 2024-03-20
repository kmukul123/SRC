using StocksWatch.Page.HistoricalData;

namespace StocksWatch.Page;

public partial class HistoricalDataPage2 : ContentPage
{
	public HistoricalDataPage2(HistoricalDataVM viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
        Title = viewModel.Item.Ticker;
    }
}