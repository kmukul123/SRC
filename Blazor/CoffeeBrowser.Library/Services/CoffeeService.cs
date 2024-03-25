using System.Net.Http.Json;

namespace CoffeeBrowser.Library.Services;

public class CoffeeService : ICoffeeService
{
    private readonly HttpClient _httpClient = new();
    public async Task<IEnumerable<Coffee>?> LoadCoffeesAsync()
    {
        var coffees = await _httpClient.GetFromJsonAsync<IEnumerable<Coffee>>(
            "https://www.thomasclaudiushuber.com/pluralsight/coffees.json");
        //var coffees = new[]
        //{
        //    new Coffee("Cappuccino", "Coffee with milk foram"),
        //    new Coffee("Doppio", "Double Expressor"),
        //};
        //await Task.Delay(500);
        return coffees;
    }
}
