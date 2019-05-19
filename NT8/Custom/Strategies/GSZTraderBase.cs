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
	[Gui.CategoryOrder("CustomParams", 1)] // display "CP" first
	[Gui.CategoryOrder("GStrategy", 2)] // then "GStrategy"
	[Gui.CategoryOrder("MoneyMgmt", 3)] // then "MM"
	[Gui.CategoryOrder("TradeMgmt", 4)] // and then "TM"
	[Gui.CategoryOrder("Trigger", 5)] // and finally "TG"
	
	public partial class GSZTraderBase : Strategy
	{
		//protected GIndicatorBase indicatorProxy;
		protected IndicatorSignal indicatorSignal;
		protected TradeObj tradeObj;
		
		private Series<double> CustomDatsSeries1;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				SetInitParams();
//				AddPlot(new Stroke(Brushes.Orange, 2), PlotStyle.TriangleRight, "CustomPlot1");
//				AddLine(Brushes.Orange, 1, "CustomLine1");
//				AddPlot(new Stroke(Brushes.Orange, 2), PlotStyle.HLine, "CustomPlot2");
			}
			else if (State == State.Configure)
			{
				//InitTradeMgmt();
				//AddDataSeries("@SPX500", Data.BarsPeriodType.Minute, 1, Data.MarketDataType.Last);
			}
			else if (State == State.DataLoaded)
			{
				indicatorProxy = GIndicatorProxy(1, Account.Name);
				indicatorSignal = new IndicatorSignal();
				//CustomDatsSeries1 = new Series<double>(this);
			}
		}
		
		public void SetInitParams() {
			Print("GSZTraderBase Set initParams called....");
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
			RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
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
			//CustomPrc1					= 1;
			// Use Unmanaged order methods
        	IsUnmanaged = true;
			AlgoMode = AlgoModeType.Trading;
		}
		
		public bool IsLiveTrading() {
			if(State == State.Realtime)
				return true;
			else return false;
		}
		
		protected override void OnAccountItemUpdate(Cbi.Account account, Cbi.AccountItem accountItem, double value)
		{
			
		}

		protected override void OnConnectionStatusUpdate(ConnectionStatusEventArgs connectionStatusUpdate)
		{
			
		}

		protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
		{
			
		}

		protected override void OnMarketDepth(MarketDepthEventArgs marketDepthUpdate)
		{
			
		}

		protected override void OnBarUpdate()
		{
			//Print(CurrentBar.ToString() + " -- GSZTraderBase - Add your custom strategy logic here.");
			int bsx = BarsSinceExitExecution(0, "", 0);
			int bse = BarsSinceEntryExecution(0, "", 0);
			SetPrintOut(-1);
			
			//Print(CurrentBar + ":" + this.Name + " OnBarUpdate, BarsSinceExit, BarsSinceEntry=" + bsx + "," + bse);
			indicatorProxy.TraceMessage(this.Name, PrintOut);
			indicatorProxy.Update();
			indicatorProxy.TraceMessage(this.Name, PrintOut);
			CheckPerformance();
			//double gap = GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Cyan).GetCurZZGap();
			//bool isReversalBar = true;//CurrentBar>BarsRequired?false:GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Cyan).IsReversalBar();
			indicatorProxy.TraceMessage(this.Name, PrintOut);
			GetIndicatorSignal();
//			CheckNewEntryTrade();
//			PutTrade();
			switch(AlgoMode) {
				case AlgoModeType.Liquidate: //liquidate
					indicatorProxy.TraceMessage(this.Name, PrintOut);
					CloseAllPositions();
					break;
				case AlgoModeType.Trading: //trading
					//PutTrade(); break;
					indicatorProxy.TraceMessage(this.Name, PrintOut);
					if(hasPosition() != 0) {
						CheckExitTrade();
					}
					else if(NewOrderAllowed())
					{
						indicatorProxy.TraceMessage(this.Name, PrintOut);
						CheckNewEntryTrade();
						
						indicatorProxy.TraceMessage(this.Name, PrintOut);
						PutTrade();
					}
					break;
				case AlgoModeType.CancelOrders: //cancel order
					indicatorProxy.TraceMessage(this.Name, PrintOut);
					CancelAllOrders();
					break;
				case AlgoModeType.StopTrading: //stop trading
					indicatorProxy.TraceMessage(this.Name, PrintOut);
					indicatorProxy.PrintLog(true, !backTest, CurrentBar + "- Stop trading cmd:" + indicatorProxy.Get24HDateTime(Time[0]));
					break;
			}
		}

		public void SetPrintOut(int i) {
			PrintOut = indicatorProxy.PrintOut + i;
		}
		
		#region Properties
		
		[Description("Account Name")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "AccName", GroupName = "GStrategy", Order = 0)]		
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
		[Display(ResourceType = typeof(Custom.Resource), Name = "AlgoMode", GroupName = "GStrategy", Order = 1)]			
		public AlgoModeType AlgoMode
        {
            get { return algoMode; }
            set { algoMode = value; }
        }

        [Description("BackTesting mode or not")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BackTest", GroupName = "GStrategy", Order = 2)]		
        public bool BackTest
        {
            get { return backTest; }
            set { backTest = value; }
        }
		
        [Description("Print out level: large # print out more")]
		[Range(-5, 5), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PrintOut", GroupName = "GStrategy", Order = 3)]		
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

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="StartH", Description="StartH", Order=4, GroupName="GStrategy")]
		public DateTime StartH
		{ get; set; }

//		[NinjaScriptProperty]
//		[Range(1, double.MaxValue)]
//		[Display(Name="CustomPrc1", Description="prc-1", Order=3, GroupName="Parameters")]
//		public double CustomPrc1
//		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CustomPlot1
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
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
