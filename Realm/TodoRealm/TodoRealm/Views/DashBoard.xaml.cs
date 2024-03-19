using TodoRealm.VM;

namespace TodoRealm.Views;

public partial class DashBoard : ContentPage
{
	public DashBoard(DashBoardVM dashBoardVM)
	{
		InitializeComponent();
		BindingContext = dashBoardVM;
	}
}