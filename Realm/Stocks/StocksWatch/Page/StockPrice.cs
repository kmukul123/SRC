namespace StocksWatch.Page;

//public record StockPrice(DateTime Date, double Open, double Close, double High, double Low, double Volume);


public class StockPrice
{
    public DateTime Date { get; set; }
    public double Open { get; set; }
    public double Close { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public double Volume { get; set; }
}