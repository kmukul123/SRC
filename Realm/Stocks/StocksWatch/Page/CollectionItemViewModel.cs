namespace StocksWatch.Page;

//public record CollectionItemViewModel(string Ticker, 
//    string CompanyName, 
//    double ClosePrice, 
//    double Change, 
//    double ChangePercent, 
//    DateTime Date);

public class CollectionItemViewModel
{
    public string Ticker { get; set; }
    public string CompanyName { get; set; }
    public double ClosePrice { get; set; }
    public double Change { get; set; }
    public double ChangePercent { get; set; }
    public DateTime Date { get; set; }
}