#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Net.Http;

using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.AddOns;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Strategies.ZTraderStg;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class GStrategyBaseEx : Strategy
	{
		//protected GIndicatorBase indicatorProxy;
		
		#region Init Functions
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				string ud_dir = NinjaTrader.Core.Globals.UserDataDir;
				Print(this.Name + " set defaults called, NinjaTrader.Core.Globals.UserDataDir=" + ud_dir);
				SetInitParams();
				//	AddPlot(new Stroke(Brushes.Orange, 2), PlotStyle.TriangleRight, "CustomPlot1");
				//	AddLine(Brushes.Orange, 1, "CustomLine1");
				//	AddPlot(new Stroke(Brushes.Orange, 2), PlotStyle.HLine, "CustomPlot2");
			}
			else if (State == State.Configure)
			{				
				//CurrentTrade.OnCurPositionUpdate += this.OnPositionUpdate;
				//InitTradeMgmt();
				//AddDataSeries("@SPX500", Data.BarsPeriodType.Minute, 1, Data.MarketDataType.Last);
			}
			else if (State == State.DataLoaded)
			{
				//CurrentTrade = new CurrentTradeBase(this);
				IndicatorProxyEx = GIndicatorProxyEx(this);//1, Account.Name);
				//GUtils.UpdateProperties(this, ReadCmdPara(), IndicatorProxy);
				//ReadCmdParaObj();
				//	CurrentTrade.InstStrategy = ;
				//tradeSignal = new TradeSignal();
				//CancelAccountOrders();
				//Account.CancelAllOrders(Instrument);
				//Account.Flatten(new List<Instrument>{Instrument});
			}
			else if (State == State.Terminated) {
//				if(IndicatorProxy != null) {
//					IndicatorProxy.Log2Disk = true;
//					IndicatorProxy.PrintLog(true, true, CurrentBar + ":" + this.Name + " terminated!");
//				}
			}
		}
		
		public void SetInitParams() {
			Print(this.Name + " Set initParams called....");			
			Description									= @"GS Z-Trader base;";
			Name										= "GSZTraderBaseEx";
			//Account.All.FirstOrDefault(a => a.Name == "Sim101");
			//this.accName								= Account.Name;
			Calculate									= Calculate.OnPriceChange;
			//This property does not work for unmanaged order entry;
			EntriesPerDirection							= 2;
			//QuantityType = QuantityType.DefaultQuantity;
			SetOrderQuantity							= SetOrderQuantity.DefaultQuantity;
			DefaultQuantity								= 5;			
			EntryHandling								= EntryHandling.AllEntries;
			//SyncAccountPosition = true;
			IsExitOnSessionCloseStrategy				= true;
			// Triggers the exit on close function 30 seconds prior to session end			
			ExitOnSessionCloseSeconds					= 30;
			IsFillLimitOnTouch							= true;
			MaximumBarsLookBack							= MaximumBarsLookBack.Infinite;
			OrderFillResolution							= OrderFillResolution.Standard;
			Slippage									= 0;
			StartBehavior								= StartBehavior.WaitUntilFlatSynchronizeAccount;
			TimeInForce									= TimeInForce.Day;
			TraceOrders									= true;
			RealtimeErrorHandling						= RealtimeErrorHandling.IgnoreAllErrors;
			StopTargetHandling							= StopTargetHandling.ByStrategyPosition;
			BarsRequiredToTrade							= 100;
			// Disable this property for performance gains in Strategy Analyzer optimizations
			// See the Help Guide for additional information
			IsInstantiatedOnEachOptimizationIteration	= true;
			WaitForOcoClosingBracket					= false;
			
			//CustomColor1					= Brushes.Orange;
//			StartH	= DateTime.Parse("08:25", System.Globalization.CultureInfo.InvariantCulture);
			// Use Unmanaged order methods
        	IsUnmanaged = true;
//			AlgoMode = AlgoModeType.Trading;
		}
		#endregion
			
		#region Utilities Functions
		public bool IsLiveTrading() {
			if(State == State.Realtime)
				return true;
			else return false;
		}
		
//		public void SetPrintOut(int i) {
//			PrintOut = IndicatorProxy.PrintOut + i;
//		}
		public string GetCmdDir() {
			List<string> names = new List<string>(){"CmdPathRoot"};
			Dictionary<string,object> dic =	GConfig.GetConfigItems(GConfig.MainConfigFile, names);
			object dir = null;
			dic.TryGetValue("CmdPathRoot", out dir);
			return GConfig.GetTemplatesDir() + dir.ToString() + Path.DirectorySeparatorChar;
		}

		public string GetCTXFilePath() {
			List<string> names = new List<string>(){"CTXFileName","MenuFileName"};
			Dictionary<string,object> dic =	GConfig.GetConfigItems(GConfig.MainConfigFile, names);
			object name = null;
			//dic.TryGetValue("CmdPathRoot", out dir);
			dic.TryGetValue("CTXFileName", out name);
			string path = GetCmdDir() + name.ToString();
			Print("GetCTXFilePath=" + path);
			return path;
		}
/*
		public void ReadRestfulJson() {
			HttpClient http = new HttpClient();
			http.DefaultRequestHeaders.Add(RequestConstants.UserAgent, RequestConstants.UserAgentValue);
			string baseUrl = "https://restcountries.eu/rest/v2/name/";

			string queryFilter = "?fields=name;capital;currencies";

			//Console.WriteLine("Enter your country name:");

			//string searchTerm = Console.ReadLine();

			//string url = baseUrl + searchTerm + queryFilter;
			string url = "https://api.github.com/repos/restsharp/restsharp/releases";
			HttpResponseMessage response = http.GetAsync(new Uri(url)).Result;

			string responseBody = response.Content.ReadAsStringAsync().Result;
		
			List<GitHubRelease> paraDict = GConfig.LoadJsonStr2Obj<List<GitHubRelease>>(responseBody);
			Print(String.Format("ele.Key={0}, ele.Value.ToString()={1}", paraDict.GetType().ToString(), paraDict.Count));
					foreach(GitHubRelease tctx in paraDict) {
						Print(String.Format("name={0}, pub_at={1}",
						tctx.Name, tctx.Published_At));
					}
			Print(String.Format("ReadRestfulJson={0}", responseBody));
			
			//var countries = JsonConvert.DeserializeObject>(responseBody);
		}
		*/
		#endregion

		public virtual void AlertTradeSignal(TradeSignal tsig, string caption){}

		#region Money Mgmt Functions
		
		public virtual bool IsProfitFactorValid(double risk, double reward, double pfMin, double pfMax) {
			bool is_valid = false;
			if(risk > 0 && reward > 0 && pfMin <= reward/risk && pfMax >= reward/risk)
				is_valid = true;
			return is_valid;
		}
		public double CheckPerformance()
		{			
			double pl = SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit;//Performance.AllTrades.TradesPerformance.Currency.CumProfit;
			double plrt = SystemPerformance.RealTimeTrades.TradesPerformance.Currency.CumProfit;//Performance.RealtimeTrades.TradesPerformance.Currency.CumProfit;
			IndicatorProxyEx.PrintLog(true, IsLiveTrading(), 
				CurrentBar + "-" + this.accName + ": Cum all PnL= " + pl + ", Cum runtime PnL= " + plrt);
			if (IndicatorProxyEx.IsLastBarOnChart() > 0 && SystemPerformance.AllTrades.Count > 0)
			{
			    foreach (Trade myTrade in SystemPerformance.AllTrades)
			    {
			    	if (myTrade.Entry.MarketPosition == MarketPosition.Long)
			        	IndicatorProxyEx.PrintLog(true, IsLiveTrading(), 
						String.Format("#{0}, ProfitCurrency={1}", myTrade.TradeNumber, myTrade.ProfitCurrency));
			    }
				IndicatorProxyEx.PrintLog(true, IsLiveTrading(), 
					String.Format("There are {0} trades, NetProfit={1}",
					SystemPerformance.AllTrades.Count, SystemPerformance.AllTrades.TradesPerformance.NetProfit));
			}
			return plrt;
		}
		#endregion
		
		#region Position Mgmt Functions
		public int HasPosition() {
			IndicatorProxyEx.TraceMessage(this.Name, 0);
			int pos = 0;
			if(IsLiveTrading()) {
				//if(PositionAccount != null)
				pos = PositionAccount.Quantity;
			}
			else //if(Position != null)
				pos = Position.Quantity;
			return pos;
		}
		
		public double GetAvgPrice() {
			MasterInstrument maIns = Bars.Instrument.MasterInstrument;
			if(IsLiveTrading())
				return maIns.RoundToTickSize(PositionAccount.AveragePrice);
			else
				return maIns.RoundToTickSize(Position.AveragePrice);
		}
				
		public MarketPosition GetMarketPosition() {
			if(IsLiveTrading())
				return PositionAccount.MarketPosition;
			else
				return Position.MarketPosition;
		}
		
		public double GetUnrealizedPnL() {
			if(IsLiveTrading())
				return PositionAccount.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0]);
			else return
				Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0]);
		}
		
		public PositionStatus GetPositionStatus(int prevPos) {
			int curPos = HasPosition();
			if(curPos == 0) {
				if(prevPos != 0)
					return PositionStatus.Liquidate;
				else return PositionStatus.Flat;
			} else {
				if(prevPos == 0)
					return PositionStatus.NewEstablished;
				else if(prevPos == curPos)
					return PositionStatus.Hold;
				else if(Math.Abs(prevPos) < Math.Abs(curPos))
					return PositionStatus.ScaledIn;
				else if(Math.Abs(prevPos) > Math.Abs(curPos))
					return PositionStatus.ScaledOut;
			}
			
			return PositionStatus.UnKnown;
		}
		#endregion

		#region Properties
//		[Browsable(false), XmlIgnore()]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
//		public GIndicatorProxyEx IndicatorProxyEx
//        {
//            get;set;
//        }
		#endregion

		#region Variables for Properties
		
		private string accName = "Sim101";
//		private AlgoModeType algoMode = AlgoModeType.Trading;
		private bool backTest = true; //if it runs for backtesting;
		private bool liveModelUpdate = false; //if it keeps model updated bar by bar
		private string liveUpdateURL = "https://thetradingbook.com/modelupdate"; //model updated URL
		private int printOut = 1; // Default setting for PrintOut
		
		#endregion

	}
}
