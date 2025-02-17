using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciTrader.Model
{
    public class YearMonth
    {
        public string ProductCode { get; set; }
        public string YearMonthCode { get; set; }
        private List<StockSymbol> _symbolList { get; set; }

        public YearMonth()
        {
            _symbolList = new List<StockSymbol>();
        }

        public List<StockSymbol> GetSymbolList()
        {
            return new List<StockSymbol>(_symbolList);
        }

        public void AddSymbol(StockSymbol item)
        {
            if (item != null && !string.IsNullOrWhiteSpace(item.SymbolCode))
            {
                // Add item only if it doesn't already exist in the list
                if (!_symbolList.Any(i => i.SymbolCode == item.SymbolCode))
                {
                    _symbolList.Add(item);
                }
            }
        }

        public StockSymbol GetLatestStockSymbol()
        {
            if (_symbolList.Count == 0) return null;
            // Assuming latest means the most recently added item
            return _symbolList.LastOrDefault();
        }
    }

}
