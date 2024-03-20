using System;
using System.Collections.Generic;

namespace StocksWatch.Page.WatchList;

public class WatchListVM
{
    public IList<CollectionItemViewModel> Items { get; set; }

    public WatchListVM()
    {
        Items = new List<CollectionItemViewModel>();
        foreach (Symbol symbol in Data.Symbols)
        {
            //var symbolViewModel = new CollectionItemViewModel(
            //    Ticker: symbol.Ticker,
            //    CompanyName: symbol.Name,
            //    Change: symbol.Prices[0].Close - symbol.Prices[1].Close,
            //    ChangePercent: symbol.Prices[0].Close / symbol.Prices[1].Close - 1,
            //    Date: symbol.Prices[0].Date,
            //    ClosePrice: symbol.Prices[0].Close
            //);
            var symbolViewModel = new CollectionItemViewModel()
            {
                Ticker = symbol.Ticker,
                CompanyName = symbol.Name,
                Change = symbol.Prices[0].Close - symbol.Prices[1].Close,
                ChangePercent = symbol.Prices[0].Close / symbol.Prices[1].Close - 1,
                Date = symbol.Prices[0].Date,
                ClosePrice = symbol.Prices[0].Close
            };
            Items.Add(symbolViewModel);
        }
    }
}