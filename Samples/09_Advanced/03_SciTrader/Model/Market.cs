using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciTrader.Model
{
    public class Market
    {
        private string _name;
        private string _marketCode;
        private string _marketRegion;
		private List<Product> _ProductList;

		public string MarketRegion
		{
			get { return _marketRegion; }
			set { _marketRegion = value; }
		}

        public Market()
        {
            _ProductList = new List<Product>();
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string MarketCode
        {
            get { return _marketCode; }
            set { _marketCode = value; }
        }

        public Product AddProduct(string code)
        {
            var product = new Product { ProductCode = code };
            _ProductList.Add(product);
            return product;
        }

        public Product FindProduct(string code)
        {
            return _ProductList.Find(p => p.ProductCode == code);
        }

        public Product FindAddProduct(string code)
        {
            var product = FindProduct(code);
            if (product == null)
            {
                product = AddProduct(code);
            }
            return product;
        }

        public List<Product> GetProductList()
        {
            return _ProductList;
        }
    }
}
