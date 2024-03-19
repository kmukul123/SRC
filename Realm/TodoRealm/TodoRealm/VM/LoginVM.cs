using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Realms.Sync;

namespace TodoRealm.VM
{
    public partial class LoginVM : BaseVM
    {
        public LoginVM() {
#if DEBUG
            EmailText = "user@test.com";
            PasswordText = "testPassword";
#endif
        }

        [ObservableProperty]
        string emailText;

        [ObservableProperty]
        string passwordText;

        public async void StartDashBoard()
        {
            await Shell.Current.GoToAsync("///Main");
        }

        [RelayCommand]
        async Task CreateAccount()
        {
            try
            {
                await App.RealmApp.EmailPasswordAuth.RegisterUserAsync(EmailText, PasswordText);
                await Login();
            } catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error creating account",
                    $"{ex.Message}", "OK");

            }
        }

        [RelayCommand]
        async Task Login()
        {
            try
            {
                var cred = Credentials.EmailPassword(EmailText, passwordText);
                var user = await App.RealmApp.LogInAsync(cred);
                if (user != null)
                {
                    await Shell.Current.GoToAsync("///Main");
                    EmailText = string.Empty;
                    passwordText = string.Empty;
                } else
                {
                    await Application.Current.MainPage.DisplayAlert("Error logging in",
                        "message", "OK");
                }
            } catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Exception logging in",
                        $"{ex.Message}", "OK");
            }
        }
    }
}
