//
// Copyright (C) 2019, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Threading;

using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.Gui.DrawingTools;
using NinjaTrader.Gui.BasicEntry;
using NinjaTrader.NinjaScript.AddOns.PriceActions;

#endregion

//This namespace holds strategies in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Strategies
{
	/// <summary>
	/// Hedge by itself: one trend following, the other is counter trend;
	/// </summary>
	public class StgSQRSpd : GStrategyBase
	{
//		private RSI rsi;
//		private RSI rsi1;
//		private RSI rsi2;
//		private ADX adx;
//		private ADX adx1;
//		private ADX adx2;
		public const int BipSpy = 0;
		public const int BipSpyLn = 3;
		public const int BipSpySt = 4;
		public const int BipQQQ = 1;
		public const int BipQQQLn = 5;
		public const int BipQQQSt = 6;
		public const int BipIWM = 2;
		public const int BipIWMLn = 7;
		public const int BipIWMSt = 8;
		
		private GIPctSpd giPctSpd;
		private GISQRSpd giSQRSpd;
		
		public BasicEntry be;
		
		public GIChartTrader giChartTrader;
		
		public StgSQRSpd () {
			//VendorLicense("TheTradingBook", "StgEQRHedger", "thetradingbook.com", "support@tradingbook.com",null);
			//be = new BasicEntry(this.Instrument);			
//				Thread thread = new Thread(() => {
//				    //int retVal = MyClass.DoX("abc", "def");
//				    // do something with retVal
//					//if(!be.IsVisible)
//					be.Show();//Dialog();
//				});
//				thread.SetApartmentState(ApartmentState.STA);
//				thread.Start();
		}
		
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				Description	= "Double Pair ES, NQ, and RTY, hedged max spread pair with min spread pair";
				Name		= "StgSQRSpd";
				// This strategy has been designed to take advantage of performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				Calculate									= Calculate.OnBarClose;
				//IsFillLimitOnTouch							= false;
				TraceOrders									= false;
				BarsRequiredToTrade							= 128;
				IsUnmanaged									= false;
				OrderFillResolution							= OrderFillResolution.Standard;
				EntriesPerDirection							= 4;
				DefaultQuantity								= 100;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				RocPeriod									= 8;				
				RocScale									= 10000;
				ChartMinutes								= 4;
				MM_ProfitFactorMax							= 1;
				MM_ProfitFactorMin							= 0;
				TG_TradeStartH								= 11;
				TG_TradeStartM								= 15;
				TG_TradeEndH								= 12;
				TG_TradeEndM								= 45;
				SpySymbol									= "SPY";
				QQQSymbol									= "QQQ";
				IWMSymbol									= "IWM";
				SpyLnSymbol									= "SPXL";
				SpyStSymbol									= "SH";
				QQQLnSymbol									= "QLD";
				QQQStSymbol									= "SQQQ";
				IWMLnSymbol									= "TNA";
				IWMStSymbol									= "TZA";
				SpyLnSymbolRatio							= 3;//"SPXL";
				SpyStSymbolRatio							= 1;//"SH";
				QQQLnSymbolRatio							= 2;//"QLD";
				QQQStSymbolRatio							= 3;//"SQQQ";
				IWMLnSymbolRatio							= 3;//"TNA";
				IWMStSymbolRatio							= 3;//"TZA";
				TradeBaseSymbol								= 4;
				//IsInstantiatedOnEachOptimizationIteration = false;
			}
			else if (State == State.Configure)
			{
				AddDataSeries(QQQSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
				AddDataSeries(IWMSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
				
				AddDataSeries(SpyLnSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
				AddDataSeries(SpyStSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
				
				AddDataSeries(QQQLnSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
				AddDataSeries(QQQStSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
				
				AddDataSeries(IWMLnSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
				AddDataSeries(IWMStSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);

				SetOrderQuantity = SetOrderQuantity.Strategy; // calculate orders based off default size
				// Sets a 20 tick trailing stop for an open position
				//SetTrailStop(CalculationMode.Ticks, 200);
			}
			else if (State == State.DataLoaded)
			{				
//				giPctSpd = GIPctSpd(8);
//				giChartTrader = GIChartTrader(this.Input);
				giSQRSpd = GISQRSpd(RocPeriod, ChartMinutes, SpySymbol, QQQSymbol, IWMSymbol, 
				SpyLnSymbol, SpyLnSymbolRatio, SpyStSymbol, SpyStSymbolRatio,
				QQQLnSymbol, QQQLnSymbolRatio, QQQStSymbol, QQQStSymbolRatio,
				IWMLnSymbol, IWMLnSymbolRatio, IWMStSymbol, IWMStSymbolRatio,
				TradeBaseSymbol, RocScale);
				// Add RSI and ADX indicators to the chart for display
				// This only displays the indicators for the primary Bars object (main instrument) on the chart
				AddChartIndicator(giSQRSpd);
				giSQRSpd.TM_OpenStartH = TG_TradeStartH;
				giSQRSpd.TM_OpenStartM = TG_TradeStartM;
				giSQRSpd.TM_OpenEndH = TG_TradeEndH;
				giSQRSpd.TM_OpenEndM = TG_TradeEndM;
				
				giSQRSpd.RaiseIndicatorEvent += OnTradeBySQRSpd;
				
//				giPctSpd.RaiseIndicatorEvent += OnTradeBySQRSpd;
//				giPctSpd.TM_ClosingH = TG_TradeEndH;
//				giPctSpd.TM_ClosingM = TG_TradeEndM;
//				giChartTrader.RaiseIndicatorEvent += OnTradeByChartTrader;
				SetPrintOut(1);
				Print(String.Format("{0}: IsUnmanaged={1}", this.GetType().Name, IsUnmanaged));
				Print(String.Format("{0}: DataLoaded...BarsArray.Length={1}", this.GetType().Name, BarsArray.Length));
				
				//this.Dispatcher.Invoke(() =>
				this.Dispatcher.BeginInvoke(new ThreadStart(() =>
				{
				    //be.Show();
				}));
				
			}			
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < BarsRequiredToTrade)
				return;
			//giPctSpd.Update();
			giSQRSpd.Update();
//			IndicatorProxy.Update();
//			if(CheckPnLByBarsSinceEn())
//				OnExitPositions(null);
//			if(BarsInProgress == BarsArray.Length-1)
//				OnTradeByPctSpd();
			// Note: Bars are added to the BarsArray and can be accessed via an index value
			// E.G. BarsArray[1] ---> Accesses the 1 minute Bars added above
//			if (adx1 == null)
//				adx1 = ADX(BarsArray[1], 14);

			// OnBarUpdate() will be called on incoming tick events on all Bars objects added to the strategy
			// We only want to process events on our primary Bars object (main instrument) (index = 0) which
			// is set when adding the strategy to a chart
//			if (BarsInProgress != 0)
//				return;
//			if (CurrentBars[0] < 0 || CurrentBars[1] < 0)
//				return;
		}
		
		#region En/Ex signals
		private bool CheckPnLByBarsSinceEn() {
			bool call_ex = false;
			int bse = BarsSinceEntryExecution(giPctSpd.PctChgMaxBip, String.Empty, 0);
			double ur_pnl = CheckUnrealizedPnLBip(giPctSpd.PctChgMaxBip);
			if(bse == TM_BarsToCheckPnL && ur_pnl < 0)
				call_ex = true;
			return call_ex;
		}
		#endregion
		
		#region Indicator Event Handler
        // Define what actions to take when the event is raised.
        void OnTradeBySQRSpd(object sender, IndicatorEventArgs e) {
			IndicatorSignal isig = e.IndSignal;
			Print(String.Format("{0}:OnTradeBySQRSpd triggerred {1} Bip={2}: RocHiBip={3}, RocLoBip={4}, RocMidBip={5}",
			CurrentBars[BarsInProgress], isig.SignalName, BarsInProgress, giSQRSpd.RocHighBip, giSQRSpd.RocLowBip, giSQRSpd.RocMidBip));
			
			SignalAction sa = e.IndSignal.SignalAction;
			List<PairSpread<int>> ps = null;
			if(sa != null)
				ps = sa.PairSpds;	
			if(ps != null) {
				foreach(PairSpread<int> p in ps) {
				Print(String.Format("{0}:OnTradeBySQRSpd Bip={1}, Symbol1={2}, Symbol2={3}, SpdType={4}, SpreadValue={5}",
				CurrentBars[BarsInProgress], BarsInProgress,
				p.Symbol1, p.Symbol2, p.SpdType, p.SpreadValue));
				}
			
				int[] idxs = {giSQRSpd.ShortBip, giSQRSpd.MidLongBip, giSQRSpd.LongBip, giSQRSpd.MidShortBip};// {BipSpyLn, BipSpySt, BipQQQLn, BipQQQSt, BipIWMLn, BipIWMSt};
				if(e.IndSignal.SignalName != null && HasPositions(idxs, 4))
					OnExitPositions(e);
				else if(e.IndSignal.SignalName != null && giSQRSpd.IsTradingTime(Times[BarsInProgress][0]))
					OnEntryPositions(e);
			}
			return;
			
			int q_max = GetTradeQuantity(giPctSpd.PctChgMaxBip, this.MM_ProfitFactorMax);
			int q_min = GetTradeQuantity(giPctSpd.PctChgMinBip, this.MM_ProfitFactorMin);
			
			//exit at 9:40 am ct
			if(isig.SignalName == giPctSpd.SignalName_ExitForOpen) {
				Print(String.Format("{0}:OnTradeByPctSpd Ex Bip={1}: MaxBip={2}, PosMax={3},  MinBip={4}, PosMin={5}", 
				CurrentBars[BarsInProgress], BarsInProgress, giPctSpd.PctChgMaxBip, Positions[giPctSpd.PctChgMaxBip], giPctSpd.PctChgMinBip, Positions[giPctSpd.PctChgMinBip]));
				OnExitPositions(e);
			} else { //entry at 9:02 am ct
				Print(String.Format("{0}:OnTradeByPctSpd En Bip={1}: PctSpd={2}, MaxBip={3}, MinBip={4}", 
				CurrentBar, BarsInProgress, giPctSpd.PlotPctSpd[0], giPctSpd.PctChgMaxBip, giPctSpd.PctChgMinBip));
				if(isig.TrendDir.TrendDir == TrendDirection.Up) {
					IndicatorProxy.PrintLog(true, IsLiveTrading(),String.Format("{0}:{1} Ln Bip={2}: PctSpd={3}, MaxBipQuant={4}, MinBipQuant={5}", 
					CurrentBars[BarsInProgress], e.Message, BarsInProgress, giPctSpd.PlotPctSpd[0],
					q_max, q_min));
					EnterLong(giPctSpd.PctChgMaxBip, q_max, "GIPctSpd");
					EnterShort(giPctSpd.PctChgMinBip, q_min, "GIPctSpd");
				}
				else if(isig.TrendDir.TrendDir == TrendDirection.Down) {
					IndicatorProxy.PrintLog(true, IsLiveTrading(),String.Format("{0}:{1} St Bip={2}: PctSpd={3}, MaxBipQuant={4}, MinBipQuant={5}", 
					CurrentBars[BarsInProgress], e.Message, BarsInProgress, giPctSpd.PlotPctSpd[0],
					q_max, q_min));
					EnterShort(giPctSpd.PctChgMaxBip, q_max, "GIPctSpd");
					EnterLong(giPctSpd.PctChgMinBip, q_min, "GIPctSpd");
				}
			}			
		}
		
		void OnExitPositions(IndicatorEventArgs e) {
			int q_spyLn = GetTradeQuantity(BipSpyLn, SpyLnSymbolRatio);
			int q_spySt = GetTradeQuantity(BipSpySt, SpyStSymbolRatio);
			int q_qqqLn = GetTradeQuantity(BipQQQLn, QQQLnSymbolRatio);
			int q_qqqSt = GetTradeQuantity(BipQQQSt, QQQStSymbolRatio);
			int q_iwmLn = GetTradeQuantity(BipIWMLn, IWMLnSymbolRatio);
			int q_iwmSt = GetTradeQuantity(BipIWMSt, IWMStSymbolRatio);
			Print(String.Format("{0}:OnTradeBySQRSpd Exit Bip={1}: q_spyLn={2}, q_spySt={3}, q_qqqLn={4}, q_qqqSt={5}, q_iwmLn={6}, q_iwmSt={7}", 
				CurrentBars[BarsInProgress], BarsInProgress, 
				q_spyLn, q_spySt, q_qqqLn, q_qqqSt, q_iwmLn, q_iwmSt));
			
			for(int i=0; i<Positions.Length; i++) {
				if(Positions[i].MarketPosition == MarketPosition.Long) {
					ExitLong(i, Positions[i].Quantity, "Ex", "");
				}
			}
			return;
			
			int q_max = GetTradeQuantity(giPctSpd.PctChgMaxBip, this.MM_ProfitFactorMax);
			int q_min = GetTradeQuantity(giPctSpd.PctChgMinBip, this.MM_ProfitFactorMin);

			if(Positions[giPctSpd.PctChgMaxBip].MarketPosition == MarketPosition.Long) {
				Print(String.Format("{0}:OnTradeByPctSpd ExLn Bip={1}: BarsSinceEntry={2}, UnrealizedPnL={3}, MaxBipQuant={4}, MinBipQuant={5}", 
				CurrentBars[BarsInProgress], BarsInProgress, 
				BarsSinceEntryExecution(giPctSpd.PctChgMaxBip, String.Empty, 0), CheckUnrealizedPnLBip(giPctSpd.PctChgMaxBip),
				q_max, q_min));
				ExitLong(giPctSpd.PctChgMaxBip, q_max, "GIExLn", String.Empty);
				ExitShort(giPctSpd.PctChgMinBip, q_min, "GIExSt", String.Empty);
			}
			//else if(isig.TrendDir.TrendDir == TrendDirection.Down) {
			else if(Positions[giPctSpd.PctChgMaxBip].MarketPosition == MarketPosition.Short) {
				Print(String.Format("{0}:OnTradeByPctSpd ExSt Bip={1}: BarsSinceEntry={2}, UnrealizedPnL={3}, MaxBipQuant={4}, MinBipQuant={5}", 
				CurrentBars[BarsInProgress], BarsInProgress,
				BarsSinceEntryExecution(giPctSpd.PctChgMaxBip, String.Empty, 0), CheckUnrealizedPnLBip(giPctSpd.PctChgMaxBip),
				q_max, q_min));
				ExitShort(giPctSpd.PctChgMaxBip, q_max, "GIExSt", String.Empty);
				ExitLong(giPctSpd.PctChgMinBip, q_min, "GIExLn", String.Empty);
			}			
		}
		
		void OnEntryPositions(IndicatorEventArgs e) {
			int q_Ln = GetTradeQuantity(giSQRSpd.LongBip, -1);
			int q_St = GetTradeQuantity(giSQRSpd.ShortBip, -1);
			int q_midLn = GetTradeQuantity(giSQRSpd.MidLongBip, -1);
			int q_midSt = GetTradeQuantity(giSQRSpd.MidShortBip, -1);
			Print(String.Format("{0}:OnTradeBySQRSpd Entry Bip={1}: q_Ln={2}, q_St={3}, q_midLn={4}, q_midSt={5}", 
				CurrentBars[BarsInProgress], BarsInProgress, 
				q_Ln, q_St, q_midLn, q_midSt));
			EnterLong(giSQRSpd.LongBip, q_Ln, "GIEnLn");
			EnterLong(giSQRSpd.ShortBip, q_St, "GIEnSt");
			//EnterLong(BipQQQLn, q_qqqLn, "GIEnQQQLn");
			EnterLong(giSQRSpd.MidLongBip, q_midLn, "GIEnMidLn");
			EnterLong(giSQRSpd.MidShortBip, q_midSt, "GIEnMidSt");
			//EnterLong(BipIWMSt, q_iwmSt, "GIEnIWMSt");
		}

		private int[] GetEntryIndexs() {
			int[] idxs = new int[4];
			return idxs;
		}
		/// <summary>
		/// CapRatio: ES:RTY=1.7:1, NQ:RTY=2.1:1, NQ:ES=1.25:1
		/// 
		/// </summary>		
		public override int GetTradeQuantity(int idx, double ratio) {
			double openBase = GetBaseSymbolOpen();
			double open = 0, rt = 0;
			int qnt = 0;
			switch(idx) {
				case BipSpyLn:
					open = giSQRSpd.OpenLnSpy;
					rt = SpyLnSymbolRatio;
					break;
				case BipSpySt:
					open = giSQRSpd.OpenStSpy;
					rt = SpyStSymbolRatio;
					break;
				case BipQQQLn:
					open = giSQRSpd.OpenLnQQQ;
					rt = QQQLnSymbolRatio;
					break;
				case BipQQQSt:
					open = giSQRSpd.OpenStQQQ;
					rt = QQQStSymbolRatio;
					break;
				case BipIWMLn:
					open = giSQRSpd.OpenLnIWM;
					rt = IWMLnSymbolRatio;
					break;
				case BipIWMSt:
					open = giSQRSpd.OpenStIWM;
					rt = IWMStSymbolRatio;
					break;
			}
			if(openBase > 0 && open > 0 && rt > 0)
				qnt = (int)(DefaultQuantity*open/(openBase*rt));
			Print(string.Format("{0}: bip={1}, openBase={2}, open={3}, qnt={4}, DefaultQuantity={5}, ratio={6}",
				CurrentBar, BarsInProgress, openBase, open, qnt, DefaultQuantity, rt));
			return qnt;
		}
		
		private double GetBaseSymbolOpen() {
			switch(TradeBaseSymbol) {
				case BipSpy:
					return giSQRSpd.OpenSpy;
				case BipQQQ:
					return giSQRSpd.OpenQQQ;
				case BipIWM:
					return giSQRSpd.OpenIWM;
				case BipSpyLn:
					return giSQRSpd.OpenLnSpy;
				case BipSpySt:
					return giSQRSpd.OpenStSpy;
				case BipQQQLn:
					return giSQRSpd.OpenLnQQQ;
				case BipQQQSt:
					return giSQRSpd.OpenStQQQ;
				case BipIWMLn:
					return giSQRSpd.OpenLnIWM;
				case BipIWMSt:
					return giSQRSpd.OpenStIWM;
				default:
					return -1;
			}
		}
		
		#endregion

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RocPeriod", Description="Rate of chage period", Order=0, GroupName="Parameters")]
		public int RocPeriod
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ChartMinutes", Description="Minutes for the chart", Order=1, GroupName="Parameters")]
		public int ChartMinutes
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="SpySymbol", Description="The spy symbol", Order=2, GroupName="Parameters")]
		public string SpySymbol
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="QQQSymbol", Description="The qqq symbol", Order=3, GroupName="Parameters")]
		public string QQQSymbol
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="IWMSymbol", Description="The iwm symbol", Order=4, GroupName="Parameters")]
		public string IWMSymbol
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="SpyLnSymbol", Description="The long symbol of spy", Order=5, GroupName="Parameters")]
		public string SpyLnSymbol
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SpyLnSymbolRatio", Description="x ratio of the symbol", Order=6, GroupName="Parameters")]
		public int SpyLnSymbolRatio
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="SpyStSymbol", Description="The short symbol of spy", Order=7, GroupName="Parameters")]
		public string SpyStSymbol
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SpyStSymbolRatio", Description="x ratio of the symbol", Order=8, GroupName="Parameters")]
		public int SpyStSymbolRatio
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="QQQLnSymbol", Description="The long symbol of qqq", Order=9, GroupName="Parameters")]
		public string QQQLnSymbol
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="QQQLnSymbolRatio", Description="x ratio of the symbol", Order=10, GroupName="Parameters")]
		public int QQQLnSymbolRatio
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="QQQStSymbol", Description="The short symbol of qqq", Order=11, GroupName="Parameters")]
		public string QQQStSymbol
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="QQQStSymbolRatio", Description="x ratio of the symbol", Order=12, GroupName="Parameters")]
		public int QQQStSymbolRatio
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="IWMLnSymbol", Description="The long symbol of iwm", Order=13, GroupName="Parameters")]
		public string IWMLnSymbol
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="IWMLnSymbolRatio", Description="x ratio of the symbol", Order=14, GroupName="Parameters")]
		public int IWMLnSymbolRatio
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="IWMStSymbol", Description="The short symbol of iwm", Order=15, GroupName="Parameters")]
		public string IWMStSymbol
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="IWMStSymbolRatio", Description="x ratio of the symbol", Order=16, GroupName="Parameters")]
		public int IWMStSymbolRatio
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="TradeBaseSymbol", Description="The base symbol for calculating size", Order=17, GroupName="Parameters")]
		public int TradeBaseSymbol
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RocScale", Description="Fold for Roc", Order=18, GroupName="Parameters")]
		public int RocScale
		{ get; set; }
		#endregion
	}
}
