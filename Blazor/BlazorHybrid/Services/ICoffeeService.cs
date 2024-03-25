namespace BlazorHybrid.Services;

interface ICoffeeService
{
    Task<IEnumerable<Coffee>?> LoadCoffeesAsync();
}
