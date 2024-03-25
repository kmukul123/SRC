namespace CoffeeBrowser.Library.Services;

public interface ICoffeeService
{
    Task<IEnumerable<Coffee>?> LoadCoffeesAsync();
}
