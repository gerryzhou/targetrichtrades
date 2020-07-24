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
using NinjaTrader.NinjaScript.Strategies;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class GStrategyBase : Strategy
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
				CurrentTrade = new CurrentTradeBase(this);
				IndicatorProxy = GIndicatorProxy(this);//1, Account.Name);
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
			Name										= "GSZTraderBase";
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
		
		public void SetPrintOut(int i) {
			PrintOut = IndicatorProxy.PrintOut + i;
		}
//		public string GetCmdDir() {
//			List<string> names = new List<string>(){"CmdPathRoot"};
//			Dictionary<string,object> dic =	GConfig.GetConfigItems(GConfig.MainConfigFile, names);
//			object dir = null;
//			dic.TryGetValue("CmdPathRoot", out dir);
//			return GConfig.GetTemplatesDir() + dir.ToString() + Path.DirectorySeparatorChar;
//		}

//		public string GetCTXFilePath() {
//			List<string> names = new List<string>(){"CTXFileName","MenuFileName"};
//			Dictionary<string,object> dic =	GConfig.GetConfigItems(GConfig.MainConfigFile, names);
//			object name = null;
//			//dic.TryGetValue("CmdPathRoot", out dir);
//			dic.TryGetValue("CTXFileName", out name);
//			string path = GetCmdDir() + name.ToString();
//			Print("GetCTXFilePath=" + path);
//			return path;
//		}
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
		
		#region Position Mgmt Functions
		public int HasPosition() {
			IndicatorProxy.TraceMessage(this.Name, 0);
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
		
		#region Interfaces
		public virtual void AlertTradeSignal(TradeSignal tsig, string caption){}
		#endregion
		
		#region Properties
		[Description("Account Name")]
		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "AccName", GroupName = GPS_GSTRATEGY, Order = ODG_AccName)]		
        public string AccName
        {
            get { return accName; }
            set { accName = value; }
        }
		
		/// <summary>
		/// AlgoMode: 0=liquidate; 
		/// 1=trading; 
		/// 2=semi-algo(manual entry, algo exit);
		/// -1=stop trading(no entry/exit, cancel entry orders and keep the exit order as it is if there has position);
		/// -2=stop trading(no entry/exit, liquidate positions and cancel all entry/exit orders);
		/// </summary>
		/// <returns></returns>
        [Description("Algo mode")]
//		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "AlgoMode", GroupName = GPS_GSTRATEGY, Order = ODG_AlgoMode)]			
		public AlgoModeType AlgoMode
        {
            get { return algoMode; }
            set { algoMode = value; }
        }

        [Description("BackTesting mode or not")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BackTest", GroupName = GPS_GSTRATEGY, Order = ODG_BackTest)]		
        public bool BackTest
        {
            get { return backTest; }
            set { backTest = value; }
        }
		
        [Description("Live update Model")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "LiveModelUpdate", GroupName = GPS_GSTRATEGY, Order = ODG_LiveModelUpdate)]		
        public bool LiveModelUpdate
        {
            get { return liveModelUpdate; }
            set { liveModelUpdate = value; }
        }
		
        [Description("Model update URL")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ModelUpdateURL", GroupName = GPS_GSTRATEGY, Order = ODG_ModelUpdateURL)]		
        public string ModelUpdateURL
        {
            get { return liveUpdateURL; }
            set { liveUpdateURL = value; }
        }
		
        [Description("Print out level: large # print out more")]
		[Range(-5, 5), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PrintOut", GroupName = GPS_GSTRATEGY, Order = ODG_PrintOut)]		
        public int PrintOut
        {
            get { return printOut; }
            set { printOut = Math.Max(-5, value); }
        }
		
//		[NinjaScriptProperty]
//		[XmlIgnore]
//		[Display(Name="CustomColor1", Description="Color-1", Order=1, GroupName="Parameters")]
//		public Brush CustomColor1
//		{ get; set; }

//		[Browsable(false)]
//		public string CustomColor1Serializable
//		{
//			get { return Serialize.BrushToString(CustomColor1); }
//			set { CustomColor1 = Serialize.StringToBrush(value); }
//		}			

		[Description("Time to Start Strategy")]
		[NinjaScriptProperty]
		//[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(ResourceType = typeof(Custom.Resource), Name="StartH", Description="StartH", Order=ODG_StartH, GroupName=GPS_GSTRATEGY)]
		public DateTime StartH
		{ get; set; }

//		[NinjaScriptProperty]
//		[Range(1, double.MaxValue)]
//		[Display(Name="CustomPrc1", Description="prc-1", Order=3, GroupName="Parameters")]
//		public double CustomPrc1
//		{ get; set; }

		[Browsable(false), XmlIgnore]
		public Series<double> CustomPlot1
		{
			get { return Values[0]; }
		}

		[Browsable(false), XmlIgnore]
		public Series<double> CustomPlot2
		{
			get { return Values[1]; }
		}
		#endregion

		#region Variables for Properties
		
		private string accName = "Sim101";
		private AlgoModeType algoMode = AlgoModeType.Trading;
		private bool backTest = true; //if it runs for backtesting;
		private bool liveModelUpdate = false; //if it keeps model updated bar by bar
		private string liveUpdateURL = "https://thetradingbook.com/modelupdate"; //model updated URL
		private int printOut = 1; // Default setting for PrintOut
		
		#endregion

	}
}
