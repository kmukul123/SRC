using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Realms;
using Realms.Sync;
using TodoRealm.Models;

namespace TodoRealm.VM;

public partial class DashBoardVM : BaseVM
{
    private Realm realm;
    private PartitionSyncConfiguration config;

    public DashBoardVM()
    {
        todoList = new ObservableCollection<Todo>();
        EmptyText = "Please add a new TODO to get started";
    }

    [ObservableProperty]
    ObservableCollection<Todo> todoList;
    
    [ObservableProperty]
    string emptyText;

    [ObservableProperty]
    string todoEntryText;

    [ObservableProperty]
    bool isRefreshing;

    public async Task InitializeRealm()
    {
        config = new(App.RealmApp.CurrentUser.Id, App.RealmApp.CurrentUser);
        realm = Realm.GetInstance(config);

        GetTodos();
        if (todoList.Count == 0)
        {
            EmptyText = "Loading Projects...";
            await Task.Delay(2000);
            GetTodos();
        }
    }

    [RelayCommand]
    public async Task GetTodos()
    {
        IsRefreshing = true;
        IsBusy = true;
        try
        {
            var tList = realm.All<Todo>().Reverse().OrderBy(x => x.Completed);
            TodoList = new ObservableCollection<Todo>(tList);
        } catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayPromptAsync("Error", ex.Message);
        }
    }

    [RelayCommand]
    async Task EditTodo(Todo td)
    {
        var newStr = await App.Current.MainPage.DisplayPromptAsync("Edit", td.Name);
        if (string.IsNullOrWhiteSpace(newStr)) return;

        try
        {
            realm.Write(() => {
                var foundTodo = realm.Find<Todo>(td.Id);
                foundTodo.Name = newStr;
            });
        } catch (Exception ex) {
            await Application.Current.MainPage.DisplayPromptAsync("Error", ex.Message);
        }
    }

}
