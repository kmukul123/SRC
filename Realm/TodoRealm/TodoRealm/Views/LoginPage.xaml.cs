using TodoRealm.VM;

namespace TodoRealm.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginVM loginVM)
	{
		InitializeComponent();
		BindingContext = loginVM;
	}
}