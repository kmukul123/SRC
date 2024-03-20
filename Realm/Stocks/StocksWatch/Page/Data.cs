using System.Reflection;
using System.Text;

namespace StocksWatch.Page;

public static class Data
{
    private static readonly string logicalName = "symbols";

    private static IList<Symbol> symbols;

    public static IList<Symbol> Symbols
    {
        get
        {
            if (symbols == null)
            {
                symbols = GetSymbols();
            }
            return symbols;
        }
    }

    private static IList<Symbol> GetSymbols()
    {
        List<Symbol> symbols = null;
        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(logicalName))
        using (StreamReader reader = new(stream, Encoding.UTF8))
        {
            var json = reader.ReadToEnd();
            symbols = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Symbol>>(json);
        }
        return symbols;
    }
}