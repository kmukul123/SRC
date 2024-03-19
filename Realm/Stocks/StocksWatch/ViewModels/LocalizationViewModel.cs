namespace StocksWatch.ViewModels;

public partial class LocalizationViewModel : BaseViewModel
{
	public string LocalizedText => StocksWatch.Resources.Strings.AppResources.HelloMessage;
}
