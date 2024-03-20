namespace StocksWatch.Page;
public class Symbol
{
    public string Ticker { get; set; }
    public string Name { get; set; }
    public IList<StockPrice> Prices { get; set; }
}
