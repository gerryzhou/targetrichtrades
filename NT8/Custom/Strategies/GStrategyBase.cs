#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Strategies.ZTraderStg;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class GStrategyBase : Strategy
	{
		//protected GIndicatorBase indicatorProxy;
		private Series<double> CustomDatsSeries1;
		
		#region Init Functions
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				string ud_dir = NinjaTrader.Core.Globals.UserDataDir;
				Print(this.Name + "set defaults called, NinjaTrader.Core.Globals.UserDataDir=" + ud_dir);
				SetInitParams();
//				AddPlot(new Stroke(Brushes.Orange, 2), PlotStyle.TriangleRight, "CustomPlot1");
//				AddLine(Brushes.Orange, 1, "CustomLine1");
//				AddPlot(new Stroke(Brushes.Orange, 2), PlotStyle.HLine, "CustomPlot2");
			}
			else if (State == State.Configure)
			{				
				//CurrentTrade.OnCurPositionUpdate += this.OnPositionUpdate;
				//InitTradeMgmt();
				//AddDataSeries("@SPX500", Data.BarsPeriodType.Minute, 1, Data.MarketDataType.Last);
			}
			else if (State == State.DataLoaded)
			{
				IndicatorProxy = GIndicatorProxy(this);//1, Account.Name);
				CurrentTrade = new CurrentTradeBase(this);
				
//				CurrentTrade.InstStrategy = ;
				//tradeSignal = new TradeSignal();
				CancelAccountOrders();
				//Account.CancelAllOrders(Instrument);
				//Account.Flatten(new List<Instrument>{Instrument});
				ReadCmdPara();
			}
			else if (State == State.Terminated) {
				if(IndicatorProxy != null) {
					IndicatorProxy.Log2Disk = true;
					IndicatorProxy.PrintLog(true, true, CurrentBar + ":" + this.Name + " terminated!");
				}
			}
		}
		
		public void SetInitParams() {
			Print(this.Name + " Set initParams called....");			
			Description									= @"GS Z-Trader base;";
			Name										= "GSZTraderBase";
			Calculate									= Calculate.OnPriceChange;
			EntriesPerDirection							= 1;
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
			//QuantityType = QuantityType.DefaultQuantity;
			SetOrderQuantity = SetOrderQuantity.DefaultQuantity;
			DefaultQuantity = 1;
			WaitForOcoClosingBracket = false;
			
			//CustomColor1					= Brushes.Orange;
			StartH						= DateTime.Parse("08:25", System.Globalization.CultureInfo.InvariantCulture);
			// Use Unmanaged order methods
        	IsUnmanaged = true;
			AlgoMode = AlgoModeType.Trading;
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
		private int printOut = 1; // Default setting for PrintOut
		
		#endregion

	}
}
