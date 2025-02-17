using Ecng.Common;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using NLog;
using SciChart.Charting3D.PointMarkers;
using SciTrader.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using WebSocketSharp;

namespace SciTrader.Model
{
	public class SymbolManager
	{
		// Singleton pattern to ensure a single instance
		private static readonly Lazy<SymbolManager> _instance = new Lazy<SymbolManager>(() => new SymbolManager());
		public static SymbolManager Instance => _instance.Value;

		// Dictionary to store StockSymbols
		private readonly Dictionary<string, StockSymbol> _Symbols;

		// HashSet to store product codes
		private readonly HashSet<string> _FavoriteProductSet;
		private readonly List<string> _marketRegion = new List<string> { "Home", "Foreign" };
		private List<Market> _marketList;
		private Dictionary<string, string> _marketCodeToMarketNameTable;

		private readonly Dictionary<string, Product> _products;

		List<OptionItem> optionItems = new List<OptionItem>();
		List<FutureItem> futureItems = new List<FutureItem>();

		public List<OptionItem> OptionItems { get { return optionItems; } }
		public List<FutureItem> FutureItems { get { return futureItems; } }

		private List<HomeFuture> _homeFutures = new List<HomeFuture>();
		public System.Collections.Generic.List<SciTrader.Model.HomeFuture> HomeFutures
		{
			get { return _homeFutures; }
			set { _homeFutures = value; }
		}
		private List<HomeOption> _homeOptions = new List<HomeOption>();
		public System.Collections.Generic.List<SciTrader.Model.HomeOption> HomeOptions
		{
			get { return _homeOptions; }
			set { _homeOptions = value; }
		}
		// Constructor
		public SymbolManager()
		{
			_Symbols = new Dictionary<string, StockSymbol>();
			_FavoriteProductSet = new HashSet<string>();
			_products = new Dictionary<string, Product>();
			InitProductSet(); // Initialize the product set
			_marketList = new List<Market>();
			_marketCodeToMarketNameTable = new Dictionary<string, string>();
			InitMarketTable();
		}

		// 000 - 099: Foreign Market
		// 100 - 199: Home Market
		// 200 - 299: ETF Market
		public void InitMarketTable()
		{
			Market market;


			market = new Market
			{
				MarketRegion = "Home",
				Name = "국내선물",
				MarketCode = "101"
			};
			_marketList.Add(market);
			_marketCodeToMarketNameTable["101"] = "국내선물";

			Product product = market.AddProduct("101");
			product.MarketName = market.Name;

			HomeFuture homeFuture = new HomeFuture
			{
				FutureName = "지수선물",
				ProductCode = "101F",
				Product = product
			};
			_homeFutures.Add(homeFuture);

			product = market.AddProduct("105");
			product.MarketName = market.Name;
			homeFuture = new HomeFuture
			{
				FutureName = "미니선물",
				ProductCode = "105F",
				Product = product
			};
			_homeFutures.Add(homeFuture);

			product = market.AddProduct("106");
			product.MarketName = market.Name;
			homeFuture = new HomeFuture
			{
				FutureName = "코닥선물",
				ProductCode = "106F",
				Product = product
			};
			_homeFutures.Add(homeFuture);

			product = market.AddProduct("167");
			product.MarketName = market.Name;
			homeFuture = new HomeFuture
			{
				FutureName = "국채선물",
				ProductCode = "167F",
				Product = product
			};
			_homeFutures.Add(homeFuture);

			product = market.AddProduct("175");
			product.MarketName = market.Name;
			homeFuture = new HomeFuture
			{
				FutureName = "달러선물",
				ProductCode = "175F",
				Product = product
			};
			_homeFutures.Add(homeFuture);

			market = new Market
			{
				MarketRegion = "Home",
				Name = "국내옵션",
				MarketCode = "102"
			};
			_marketList.Add(market);
			_marketCodeToMarketNameTable["102"] = "국내옵션";
			HomeOption homeOption = new HomeOption
			{
				OptionName = "코스피옵션",
				ProductCode = "101O",
			};
			product = market.AddProduct("201");
			product.MarketName = market.Name;
			homeOption.CallProduct = product;
			product = market.AddProduct("301");
			product.MarketName = market.Name;
			homeOption.PutProduct = product;
			HomeOptions.Add(homeOption);

			homeOption = new HomeOption
			{
				OptionName = "미니코스피옵션",
				ProductCode = "105O",
			};
			product = market.AddProduct("205");
			product.MarketName = market.Name;
			homeOption.CallProduct = product;
			product = market.AddProduct("305");
			product.MarketName = market.Name;
			homeOption.PutProduct = product;
			HomeOptions.Add(homeOption);

			homeOption = new HomeOption
			{
				OptionName = "코스피위클리옵션[T]",
				ProductCode = "109O",
			};
			product = market.AddProduct("209");
			product.MarketName = market.Name;
			homeOption.CallProduct = product;
			product = market.AddProduct("309");
			product.MarketName = market.Name;
			homeOption.PutProduct = product;
			HomeOptions.Add(homeOption);

			homeOption = new HomeOption
			{
				OptionName = "코스피위클리옵션[M]",
				ProductCode = "1AFO",
			};
			product = market.AddProduct("2AF");
			product.MarketName = market.Name;
			homeOption.CallProduct = product;
			product = market.AddProduct("3AF");
			product.MarketName = market.Name;
			homeOption.PutProduct = product;
			HomeOptions.Add(homeOption);

			homeOption = new HomeOption
			{
				OptionName = "코스닥옵션",
				ProductCode = "106O",
			};
			product = market.AddProduct("206");
			product.MarketName = market.Name;
			homeOption.CallProduct = product;
			product = market.AddProduct("306");
			product.MarketName = market.Name;
			homeOption.PutProduct = product;
			HomeOptions.Add(homeOption);

			market = new Market
			{
				MarketRegion = "Foreign",
				Name = "지수",
				MarketCode = "001"
			};
			_marketList.Add(market);
			_marketCodeToMarketNameTable["001"] = "지수";

			market = new Market
			{
				MarketRegion = "Foreign",
				Name = "통화",
				MarketCode = "002"
			};
			_marketList.Add(market);
			_marketCodeToMarketNameTable["002"] = "통화";

			market = new Market
			{
				MarketRegion = "Foreign",
				Name = "금리",
				MarketCode = "003"
			};
			_marketList.Add(market);
			_marketCodeToMarketNameTable["003"] = "금리";

			market = new Market
			{
				MarketRegion = "Foreign",
				Name = "농축산",
				MarketCode = "004"
			};
			_marketList.Add(market);
			_marketCodeToMarketNameTable["004"] = "농축산";

			market = new Market
			{
				MarketRegion = "Foreign",
				Name = "귀금속",
				MarketCode = "005"
			};
			_marketList.Add(market);
			_marketCodeToMarketNameTable["005"] = "귀금속";

			market = new Market
			{
				MarketRegion = "Foreign",
				Name = "에너지",
				MarketCode = "006"
			};
			_marketList.Add(market);
			_marketCodeToMarketNameTable["006"] = "에너지";

			market = new Market
			{
				MarketRegion = "Foreign",
				Name = "비철금속",
				MarketCode = "007"
			};
			_marketList.Add(market);
			_marketCodeToMarketNameTable["007"] = "비철금속";
		}

		public void InitHomeMarket()
		{
			Market market;
			market = new Market
			{
				MarketRegion = "Home",
				Name = "지수",
				MarketCode = "001"
			};
			_marketList.Add(market);
			_marketCodeToMarketNameTable["001"] = "지수";
		}

		// Method to save a StockSymbol
		public void SaveSymbol(StockSymbol item)
		{
			if (item == null || string.IsNullOrWhiteSpace(item.SymbolCode))
			{
				throw new ArgumentException("Symbol or SymbolCode cannot be null or empty");
			}

			_Symbols[item.SymbolCode] = item;
		}

		// Method to read a StockSymbol by its SymbolCode
		public StockSymbol ReadSymbol(string itemCode)
		{
			if (string.IsNullOrWhiteSpace(itemCode))
			{
				throw new ArgumentException("SymbolCode cannot be null or empty");
			}

			if (_Symbols.TryGetValue(itemCode, out StockSymbol item))
			{
				return item;
			}

			return null;
		}

		// Method to delete a StockSymbol by its SymbolCode
		public bool DeleteSymbol(string itemCode)
		{
			if (string.IsNullOrWhiteSpace(itemCode))
			{
				throw new ArgumentException("SymbolCode cannot be null or empty");
			}

			return _Symbols.Remove(itemCode);
		}

		// Method to find a StockSymbol by its SymbolCode
		public StockSymbol FindSymbol(string itemCode)
		{
			return ReadSymbol(itemCode);
		}

		// Method to get all StockSymbols
		public IEnumerable<StockSymbol> GetAllSymbols()
		{
			return _Symbols.Values;
		}

		// Method to read StockSymbols from a file
		public void ReadForeignSymbolFile(string subDirectory, string fileName)
		{
			try
			{
				// Get the application path
				string appPath = AppDomain.CurrentDomain.BaseDirectory;

				// Combine the path to the subdirectory and file name
				string filePath = Path.Combine(appPath, subDirectory, fileName);

				// Specify the euc-kr encoding
				int euckrCodePage = 51949;  // euc-kr code page
				System.Text.Encoding eucKr = System.Text.Encoding.GetEncoding(euckrCodePage);

				// Read the file line by line
				using (StreamReader sr = new StreamReader(filePath, eucKr))
				{
					string line;
					int lineNum = 0;
					while ((line = sr.ReadLine()) != null)
					{
						byte[] lineBytes = eucKr.GetBytes(line);

						string symbolCode = eucKr.GetString(lineBytes, 0, 32).Trim();
						string exchangeName = eucKr.GetString(lineBytes, 32, 5).Trim();
						string indexCode = eucKr.GetString(lineBytes, 37, 4).Trim();
						string productCode = eucKr.GetString(lineBytes, 41, 5).Trim();
						string exchNo = eucKr.GetString(lineBytes, 46, 5).Trim();
						string pdesz = eucKr.GetString(lineBytes, 51, 5).Trim();
						string rdesz = eucKr.GetString(lineBytes, 56, 5).Trim();
						string ctrtSize = eucKr.GetString(lineBytes, 61, 20).Trim();
						string tickSize = eucKr.GetString(lineBytes, 81, 20).Trim();
						string tickValue = eucKr.GetString(lineBytes, 101, 20).Trim();
						string mltiPler = eucKr.GetString(lineBytes, 121, 20).Trim();
						string dispDigit = eucKr.GetString(lineBytes, 141, 10).Trim();
						string symbolNameEn = eucKr.GetString(lineBytes, 151, 32).Trim();
						string symbolNameKr = eucKr.GetString(lineBytes, 183, 32).Trim();
						string nearSeq = eucKr.GetString(lineBytes, 215, 1).Trim();
						string statTp = eucKr.GetString(lineBytes, 216, 1).Trim();
						string lockDt = eucKr.GetString(lineBytes, 217, 8).Trim();
						string tradFrDt = eucKr.GetString(lineBytes, 225, 8).Trim();
						string lastDate = eucKr.GetString(lineBytes, 233, 8).Trim();
						string exprDt = eucKr.GetString(lineBytes, 241, 8).Trim();
						string remnCnt = eucKr.GetString(lineBytes, 249, 4).Trim();
						string hogaMthd = eucKr.GetString(lineBytes, 253, 30).Trim();
						string minMaxRt = eucKr.GetString(lineBytes, 283, 6).Trim();
						string baseP = eucKr.GetString(lineBytes, 289, 20).Trim();
						string maxP = eucKr.GetString(lineBytes, 309, 20).Trim();
						string minP = eucKr.GetString(lineBytes, 329, 20).Trim();
						string trstMgn = eucKr.GetString(lineBytes, 349, 20).Trim();
						string mntMgn = eucKr.GetString(lineBytes, 369, 20).Trim();
						string crcCd = eucKr.GetString(lineBytes, 389, 3).Trim();
						string baseCrcCd = eucKr.GetString(lineBytes, 392, 3).Trim();
						string counterCrcCd = eucKr.GetString(lineBytes, 395, 3).Trim();
						string pipCost = eucKr.GetString(lineBytes, 398, 20).Trim();
						string buyInt = eucKr.GetString(lineBytes, 418, 20).Trim();
						string sellInt = eucKr.GetString(lineBytes, 438, 20).Trim();
						string roundLots = eucKr.GetString(lineBytes, 458, 6).Trim();
						string scaleChiper = eucKr.GetString(lineBytes, 464, 10).Trim();
						string decimalchiper = eucKr.GetString(lineBytes, 474, 5).Trim();
						string jnilVolume = eucKr.GetString(lineBytes, 479, 10).Trim();
						try
						{
							Product product = FindProduct(productCode);

							StockSymbol symbol = new StockSymbol
							{
								SymbolType = "Foreign",
								SymbolCode = symbolCode,
								NameKr = symbolNameKr,
								NameEn = symbolNameEn,
								Decimal = Convert.ToInt32(pdesz),
								Multiplier = Convert.ToDouble(mltiPler),
								ContractSize = Convert.ToDouble(ctrtSize),
								TickValue = Convert.ToDouble(tickValue),
								TickSize = Convert.ToDouble(tickSize),
								ExpireDay = exprDt,
								PredayVolume = Convert.ToInt32(jnilVolume)
							};

							SaveSymbol(symbol);
							if (product != null)
							{
								product.AddSymbol(symbol);
								product.AddToYearMonth(1, symbol.SymbolCode, symbol);
								Debug.WriteLine(lineNum++);
							}
						}
						catch (Exception ex)
						{
							LogManager.Log($"Line : {lineNum} logged at {symbolCode}");
							MessageBox.Show($"Error iterating Symbol file: {ex.Message}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error reading Symbol file: {ex.Message}");
			}
		}


		public void ReadForeignMarketFile(string subDirectory, string fileName)
		{
			try
			{
				// Get the application path
				string appPath = AppDomain.CurrentDomain.BaseDirectory;
				LogManager.Log($"Application path: {appPath}");

				// Combine the path to the subdirectory and file name
				string filePath = Path.Combine(appPath, subDirectory, fileName);
				LogManager.Log($"Full file path: {filePath}");

				// Check if the file exists
				if (!File.Exists(filePath))
				{
					LogManager.Log($"The file '{filePath}' does not exist.");
					MessageBox.Show($"Error: The file '{filePath}' does not exist.");
					return;
				}
				// Specify the euc-kr encoding
				int euckrCodePage = 51949;  // euc-kr code page
				Encoding eucKr = Encoding.GetEncoding(euckrCodePage);

				// Read the file line by line
				using (StreamReader sr = new StreamReader(filePath, eucKr))
				{
					string line;
					int lineNum = 0;
					while ((line = sr.ReadLine()) != null)
					{
						try
						{
							byte[] lineBytes = eucKr.GetBytes(line);

							string market_type = eucKr.GetString(lineBytes, 0, 20).Trim();
							string exchange = eucKr.GetString(lineBytes, 20, 5).Trim();
							string pmCode = eucKr.GetString(lineBytes, 25, 3).Trim();
							string enName = eucKr.GetString(lineBytes, 28, 50).Trim();
							string name = eucKr.GetString(lineBytes, 78, 50).Trim();

							Market market = AddMarket(market_type);
							LogManager.Log($"Successfully added product at line {lineNum}");
							Product product = market.FindAddProduct(pmCode);
							try
							{
								product.MarketName = market_type;
								product.Exchange = exchange;
								product.Name = enName;
								product.NameKr = name;
								LogManager.Log("Product added successfully.");
								AddProduct(product);
								lineNum++;
								LogManager.Log($"Successfully added product at line {lineNum}");
								Debug.WriteLine(lineNum);
							}
							catch (Exception ex)
							{
								LogManager.Log($"Exception while adding product at line {lineNum}: {ex.Message}");
							}
						}
						catch (Exception ex)
						{
							LogManager.Log($"Exception while processing line {lineNum}: {ex.Message}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				LogManager.Log($"Error reading Market file: {ex.Message}");
				MessageBox.Show($"Error reading Market file: {ex.Message}");
			}
		}

		public void ReadHomeSymbolMasterFile(string subDirectory, string fileName)
		{
			try
			{
				// Get the application path
				string appPath = AppDomain.CurrentDomain.BaseDirectory;
				LogManager.Log($"Application path: {appPath}");

				// Combine the path to the subdirectory and file name
				string filePath = Path.Combine(appPath, subDirectory, fileName);
				LogManager.Log($"Full file path: {filePath}");

				// Check if the file exists
				if (!File.Exists(filePath))
				{
					LogManager.Log($"The file '{filePath}' does not exist.");
					MessageBox.Show($"Error: The file '{filePath}' does not exist.");
					return;
				}
				// Specify the euc-kr encoding
				int euckrCodePage = 51949;  // euc-kr code page
				Encoding eucKr = Encoding.GetEncoding(euckrCodePage);

				// Read the file line by line
				using (StreamReader sr = new StreamReader(filePath, eucKr))
				{
					string line;
					int lineNum = 0;
					while ((line = sr.ReadLine()) != null)
					{
						try
						{
							if (lineNum == 1625)
							{
								LogManager.Log($"paused at line {lineNum}");
							}
							byte[] lineBytes = eucKr.GetBytes(line);

							string symbolCode = eucKr.GetString(lineBytes, 1, 8).Trim();
							string productCode = eucKr.GetString(lineBytes, 1, 3).Trim();
							string fullCode = eucKr.GetString(lineBytes, 9, 12).Trim();
							string symbolNameKr = eucKr.GetString(lineBytes, 21, 30).Trim();
							string symbolNameEn = eucKr.GetString(lineBytes, 51, 30).Trim();

							string remainDays = eucKr.GetString(lineBytes, 81, 5).Trim();
							string lastTradeDay = eucKr.GetString(lineBytes, 86, 8).Trim();
							string highLimitPrice = eucKr.GetString(lineBytes, 94, 12).Trim();
							string lowLimitPrice = eucKr.GetString(lineBytes, 106, 12).Trim();
							string preDayClose = eucKr.GetString(lineBytes, 118, 12).Trim();
							string standardPrice = eucKr.GetString(lineBytes, 130, 12).Trim();
							string strike = eucKr.GetString(lineBytes, 142, 17).Trim();

							string atmType = eucKr.GetString(lineBytes, 159, 1).Trim();
							string recentMonth = eucKr.GetString(lineBytes, 160, 1).Trim();
							string expireDate = eucKr.GetString(lineBytes, 161, 8).Trim();

							StockSymbol symbol = new StockSymbol
							{
								SymbolType = "Home",
								SymbolCode = symbolCode,
								NameKr = symbolNameKr,
								NameEn = symbolNameEn,
								ProductCode = productCode,
								FullCode = fullCode,
								MarketName = symbolCode[0] == '1' ? "국내선물" : "국내옵션",
								StandardPrice = standardPrice,
								Strike = strike,
								AtmType = Convert.ToInt32(atmType),
								Exchange = "KRX",
								ExpireDay = expireDate,
								RemainDays = Convert.ToInt32(remainDays),
								RecentMonth =  char.IsDigit(recentMonth[0]) ? Convert.ToInt32(recentMonth.ToString(), 16) : 0,
								StartTime = "084500",
								EndTime = "154500",
								Currency = "KRW"
							};

							SetHomeProductInfo(symbol);
							SaveSymbol(symbol);
							AddToYearMonth(symbol);
							lineNum++;
							LogManager.Log($"Successfully added symbol at line {lineNum}");
							Debug.WriteLine(lineNum);
						}
						catch (Exception ex)
						{
							LogManager.Log($"Exception while processing line {lineNum}: {ex.Message}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				LogManager.Log($"Error reading Market file: {ex.Message}");
				MessageBox.Show($"Error reading Market file: {ex.Message}");
			}
		}

		public void AddToYearMonth(StockSymbol symbol)
		{
			string productCode = symbol.ProductCode;
			string symbolNameEn = symbol.NameEn;
			string marketName = symbol.MarketName;

			if (productCode == "101")  // KOSPI 200 F 202303
			{
				string year = symbolNameEn.Substring(12, 4);
				string month = symbolNameEn.Substring(16, 2);
				string yearMonthName = $"{year}-{month}";
				Product product = FindProduct(marketName, productCode);
				if (product == null)
					return;
				YearMonth yearMonth = product.AddYearMonth(yearMonthName);
				yearMonth.AddSymbol(symbol);
			}
			else if (productCode == "105")  // MINI KOSPI F 202303
			{
				string year = symbolNameEn.Substring(13, 4);
				string month = symbolNameEn.Substring(17, 2);
				string yearMonthName = $"{year}-{month}";
				Product product = FindProduct(marketName, productCode);
				if (product == null)
					return;
				YearMonth yearMonth = product.AddYearMonth(yearMonthName);
				yearMonth.AddSymbol(symbol);
			}
			else if (productCode == "106")  // KOSDAQ150 F 202303
			{
				string year = symbolNameEn.Substring(12, 4);
				string month = symbolNameEn.Substring(16, 2);
				string yearMonthName = $"{year}-{month}";
				Product product = FindProduct(marketName, productCode);
				if (product == null)
					return;
				YearMonth yearMonth = product.AddYearMonth(yearMonthName);
				yearMonth.AddSymbol(symbol);
			}
			else if (productCode == "167")  // KTB10      F 202303
			{
				string year = symbolNameEn.Substring(13, 4);
				string month = symbolNameEn.Substring(17, 2);
				string yearMonthName = $"{year}-{month}";
				Product product = FindProduct(marketName, productCode);
				if (product == null)
					return;
				YearMonth yearMonth = product.AddYearMonth(yearMonthName);
				yearMonth.AddSymbol(symbol);
			}
			else if (productCode == "175")  // USD F 202303
			{
				string year = symbolNameEn.Substring(6, 4);
				string month = symbolNameEn.Substring(10, 2);
				string yearMonthName = $"{year}-{month}";
				Product product = FindProduct(marketName, productCode);
				if (product == null)
					return;
				YearMonth yearMonth = product.AddYearMonth(yearMonthName);
				yearMonth.AddSymbol(symbol);
			}
			else if (productCode == "201")  // KOSPI 200 C 202303 160.0 
			{
				string year = symbolNameEn.Substring(12, 4);
				string month = symbolNameEn.Substring(16, 2);
				string yearMonthName = $"{year}-{month}";
				Product product = FindProduct(marketName, productCode);
				if (product == null)
					return;
				YearMonth yearMonth = product.AddYearMonth(yearMonthName);
				yearMonth.AddSymbol(symbol);
			}
			else if (productCode == "301")  // KOSPI 200 P 202303 340.0
			{
				string year = symbolNameEn.Substring(12, 4);
				string month = symbolNameEn.Substring(16, 2);
				string yearMonthName = $"{year}-{month}";
				Product product = FindProduct(marketName, productCode);
				if (product == null)
					return;
				YearMonth yearMonth = product.AddYearMonth(yearMonthName);
				yearMonth.AddSymbol(symbol);
			}
			else if (productCode == "205")  // MINI KOSPI C 202303 200.0 
			{
				string year = symbolNameEn.Substring(13, 4);
				string month = symbolNameEn.Substring(17, 2);
				string yearMonthName = $"{year}-{month}";
				Product product = FindProduct(marketName, productCode);
				if (product == null)
					return;
				YearMonth yearMonth = product.AddYearMonth(yearMonthName);
				yearMonth.AddSymbol(symbol);
			}
			else if (productCode == "305")  // MINI KOSPI P 202303 200.0
			{
				string year = symbolNameEn.Substring(13, 4);
				string month = symbolNameEn.Substring(17, 2);
				string yearMonthName = $"{year}-{month}";
				Product product = FindProduct(marketName, productCode);
				if (product == null)
					return;
				YearMonth yearMonth = product.AddYearMonth(yearMonthName);
				yearMonth.AddSymbol(symbol);
			}
			else if (productCode == "209")  // KOSPI WEEKLY C 2303W1 275.0
			{
				string year = symbolNameEn.Substring(15, 2);
				string month = symbolNameEn.Substring(17, 2);
				string week = symbolNameEn.Substring(19, 2);
				string yearMonthName = $"20{year}-{month}-{week}-T";
				Product product = FindProduct(marketName, productCode);
				if (product == null)
					return;
				YearMonth yearMonth = product.AddYearMonth(yearMonthName);
				yearMonth.AddSymbol(symbol);
			}
			else if (productCode == "309")  // KOSPI WEEKLY P 2303W1 337.5
			{
				string year = symbolNameEn.Substring(15, 2);
				string month = symbolNameEn.Substring(17, 2);
				string week = symbolNameEn.Substring(19, 2);
				string yearMonthName = $"20{year}-{month}-{week}-T";
				Product product = FindProduct(marketName, productCode);
				if (product == null)
					return;
				YearMonth yearMonth = product.AddYearMonth(yearMonthName);
				yearMonth.AddSymbol(symbol);
			}
			else if (productCode == "2AF")  // KOSPI WEEKLY M C 2308W1 302.5
			{
				string year = symbolNameEn.Substring(17, 2);
				string month = symbolNameEn.Substring(19, 2);
				string week = symbolNameEn.Substring(21, 2);
				string yearMonthName = $"20{year}-{month}-{week}-M";
				Product product = FindProduct(marketName, productCode);
				if (product == null)
					return;
				YearMonth yearMonth = product.AddYearMonth(yearMonthName);
				yearMonth.AddSymbol(symbol);
			}
			else if (productCode == "3AF")  // KOSPI WEEKLY P 2303W1 337.5
			{
				string year = symbolNameEn.Substring(17, 2);
				string month = symbolNameEn.Substring(19, 2);
				string week = symbolNameEn.Substring(21, 2);
				string yearMonthName = $"20{year}-{month}-{week}-M";
				Product product = FindProduct(marketName, productCode);
				if (product == null)
					return;
				YearMonth yearMonth = product.AddYearMonth(yearMonthName);
				yearMonth.AddSymbol(symbol);
			}
			else if (productCode == "206")  // KOSDAQ150 C 202303 1,275
			{
				string year = symbolNameEn.Substring(12, 4);
				string month = symbolNameEn.Substring(16, 2);
				string yearMonthName = $"{year}-{month}";
				Product product = FindProduct(marketName, productCode);
				if (product == null)
					return;
				YearMonth yearMonth = product.AddYearMonth(yearMonthName);
				yearMonth.AddSymbol(symbol);
			}
			else if (productCode == "306")  // KOSDAQ150 P 202303 1,275
			{
				string year = symbolNameEn.Substring(12, 4);
				string month = symbolNameEn.Substring(16, 2);
				string yearMonthName = $"{year}-{month}";
				Product product = FindProduct(marketName, productCode);
				if (product == null)
					return;
				YearMonth yearMonth = product.AddYearMonth(yearMonthName);
				yearMonth.AddSymbol(symbol);
			}
		}

		void SetHomeProductInfo(StockSymbol symbol)
		{
			Product product = FindProduct(symbol.ProductCode);
			if (product == null)
			{
				LogManager.Log($"Product not found for symbol {symbol.SymbolCode}");
				return;
			}
			if (double.TryParse(product.TickSize, NumberStyles.Any, CultureInfo.InvariantCulture, out double ticksize_))
			{
				symbol.TickSize = ticksize_;
			}
			else
			{
				Console.WriteLine("Conversion failed.");
			}

			symbol.TickValue = product.TickValue;
			symbol.Decimal = product.Decimal;
			symbol.Multiplier = product.Multiplier;

			Console.WriteLine("SetHomeProductInfo succeeded.");
		}

		Market FindHomeMarketByProductCode(string productCode)
		{
			if (productCode.IsNullOrEmpty()) return null;
			if (productCode[0] == '1')
			{
				return FindMarket("국내선물");
			}
			else if ((productCode[0] == '2') || (productCode[0] == '3'))
			{
				return FindMarket("국내옵션");
			}

			return null;
		}

		public void ReadHomeProductFile(string subDirectory, string fileName)
		{
			try
			{
				// Get the application path
				string appPath = AppDomain.CurrentDomain.BaseDirectory;
				LogManager.Log($"Application path: {appPath}");

				// Combine the path to the subdirectory and file name
				string filePath = Path.Combine(appPath, subDirectory, fileName);
				LogManager.Log($"Full file path: {filePath}");

				// Check if the file exists
				if (!File.Exists(filePath))
				{
					LogManager.Log($"The file '{filePath}' does not exist.");
					MessageBox.Show($"Error: The file '{filePath}' does not exist.");
					return;
				}
				// Specify the euc-kr encoding
				int euckrCodePage = 51949;  // euc-kr code page
				Encoding eucKr = Encoding.GetEncoding(euckrCodePage);

				// Read the file line by line
				using (StreamReader sr = new StreamReader(filePath, eucKr))
				{
					string line;
					int lineNum = 0;
					while ((line = sr.ReadLine()) != null)
					{
						try
						{
							byte[] lineBytes = eucKr.GetBytes(line);

							string productCode = eucKr.GetString(lineBytes, 0, 8).Trim();
							string productDecimal = eucKr.GetString(lineBytes, 8, 2).Trim();
							string tickSize = eucKr.GetString(lineBytes, 10, 5).Trim();
							string tickValue = eucKr.GetString(lineBytes, 15, 5).Trim();
							string multiplier = eucKr.GetString(lineBytes, 20, 10).Trim();
							string productNameEn = eucKr.GetString(lineBytes, 30, 30).Trim();

							Market market = FindHomeMarketByProductCode(productCode);
							if (market == null) continue;
							Product product = market.FindAddProduct(productCode);
							try
							{
								if (int.TryParse(productCode, out int product_decimal))
								{
									product.Decimal = product_decimal;
								}
								else
								{
									LogManager.Log($"product_decimal Conversion failed {lineNum}");
								}
								product.Name = productNameEn;
								product.NameKr = productNameEn;
								product.TickSize = tickSize;

								if (int.TryParse(tickValue, out int tick_value))
								{
									product.TickValue = tick_value;
								}
								else
								{
									LogManager.Log($"tick_value Conversion failed {lineNum}");
								}


								if (int.TryParse(multiplier, out int multiplier_))
								{
									product.Multiplier = multiplier_;
								}
								else
								{
									LogManager.Log($"multiplier Conversion failed {lineNum}");
								}

								AddProduct(product);
								lineNum++;
								LogManager.Log($"Successfully added product at line {lineNum}");
								Debug.WriteLine(lineNum);
							}
							catch (Exception ex)
							{
								LogManager.Log($"Exception while adding product at line {lineNum}: {ex.Message}");
							}
						}
						catch (Exception ex)
						{
							LogManager.Log($"Exception while processing line {lineNum}: {ex.Message}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				LogManager.Log($"Error reading Market file: {ex.Message}");
				MessageBox.Show($"Error reading Market file: {ex.Message}");
			}
		}

		public Product FindProduct(string productCode)
		{
			if (string.IsNullOrWhiteSpace(productCode))
			{
				throw new ArgumentException("Product code cannot be null or empty", nameof(productCode));
			}

			if (_products.TryGetValue(productCode, out var product))
			{
				return product;
			}
			else
			{
				// Debugging statement for missing key
				Console.WriteLine($"Product with ProductCode {productCode} not found in dictionary.");
				return null;
			}
		}

		public void AddProduct(Product product)
		{
			if (product == null || string.IsNullOrWhiteSpace(product.ProductCode))
			{
				throw new ArgumentException("Product or ProductCode cannot be null or empty");
			}

			try
			{
				// Debugging statement to check the ProductCode
				Console.WriteLine($"Adding product with ProductCode: {product.ProductCode}");
				_products[product.ProductCode] = product;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error adding product: {ex.Message}");
				throw;
			}
		}


		Market AddMarket(string name)
		{
			Market found_market = FindMarket(name);
			if (found_market != null)
				return found_market;

			Market market = new Market();
			market.Name = name;
			_marketList.Add(market);
			return market;
		}

		public Market FindMarket(string marketName)
		{
			return _marketList.FirstOrDefault(market => market.Name == marketName);
		}

		// Function to get items with a specific prefix and sort by SymbolCode in ascending order
		public List<StockSymbol> GetSymbolsByProductCode(string productCodePrefix)
		{
			return _Symbols.Values
						 .Where(item => item.SymbolCode.StartsWith(productCodePrefix))
						 .OrderBy(item => item.SymbolCode)
						 .ToList();
		}

		public StockSymbol GetLatestSymbolByProductCode(string productCodePrefix)
		{
			if (!_products.ContainsKey(productCodePrefix))
				return null;
			Product product;
			_products.TryGetValue(productCodePrefix, out product);
			if (product == null) return null;
			var yearMonth = product.GetRecentYearMonth();
			if (yearMonth == null) return null;
			var item = yearMonth.GetLatestStockSymbol();
			return item;
		}

		public List<StockSymbol> GetFavoriteSymbolsByProductCode()
		{
			List<StockSymbol> itemList = new List<StockSymbol>();
			foreach (var item in _FavoriteProductSet)
			{
				var stockSymbol = GetLatestSymbolByProductCode(item);
				if (stockSymbol == null) continue;
				itemList.Add(stockSymbol);
			}
			return itemList;
		}

		public StockSymbol GetFirstFavoriteSymbolByProductCode()
		{
			List<StockSymbol> itemList = new List<StockSymbol>();
			foreach (var item in _FavoriteProductSet)
			{
				var stockSymbol = GetLatestSymbolByProductCode(item);
				if (stockSymbol == null) continue;
				itemList.Add(stockSymbol);
			}
			if (itemList.Count == 0) return null;
			return itemList[0];
		}

		// Function to initialize the product set
		public void InitProductSet()
		{
			_FavoriteProductSet.Add("NQ");
			_FavoriteProductSet.Add("MNQ");
			_FavoriteProductSet.Add("CL");
			_FavoriteProductSet.Add("HSI");
		}

		public Product FindProduct(string marketName, string productCode)
		{
			if (string.IsNullOrWhiteSpace(marketName) || string.IsNullOrWhiteSpace(productCode))
			{
				throw new ArgumentException("MarketName or ProductCode cannot be null or empty");
			}
			Market market = FindMarket(marketName);
			if (market == null)
			{
				return null;
			}
			return market.FindProduct(productCode);

		}
	}
}