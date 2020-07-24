//
// Copyright (C) 2019, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.Windows.Media;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds strategies in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Strategies
{
	/// <summary>
	/// Hedge by itself: one trend following, the other is counter trend;
	/// </summary>
	public class StgPairSimple : GStrategyBase
	{
		private RSI rsi;
		private RSI rsi1;
		private RSI rsi2;
		private ADX adx;
		private ADX adx1;
		private ADX adx2;

		private GIPctSpd giPctSpd;
		private GIPairPctSpd giPairPctSpd;
		
//		public StgPairSimple () {
//			VendorLicense("TheTradingBook", "StgPairSimple", "thetradingbook.com", "support@tradingbook.com",null);
//		}
		
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				Description	= "Simple Pair Trading";
				Name		= "StgPairSimple";
				// This strategy has been designed to take advantage of performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				Calculate									= Calculate.OnBarClose;
				//IsFillLimitOnTouch							= false;
				TraceOrders									= false;
				BarsRequiredToTrade							= 22;
				IsUnmanaged									= false;
				OrderFillResolution							= OrderFillResolution.Standard;
				EntriesPerDirection							= 1;
				DefaultQuantity								= 1;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
//				MM_ProfitFactorMax							= 1;
//				MM_ProfitFactorMin							= 0;
//				TG_TradeEndH								= 10;
//				TG_TradeEndM								= 45;
				//IsInstantiatedOnEachOptimizationIteration = false;
			}
			else if (State == State.Configure)
			{
				// Add an MSFT 1 minute Bars object to the strategy
				//AddDataSeries("NQ 06-20", Data.BarsPeriodType.Minute, 13);				
				AddDataSeries(SecondSymbol, Data.BarsPeriodType.Minute, ChartMinutes);
				SetOrderQuantity = SetOrderQuantity.Strategy; // calculate orders based off default size
				// Sets a 20 tick trailing stop for an open position
				//SetTrailStop(CalculationMode.Ticks, 200);
			}
			else if (State == State.DataLoaded)
			{
//				rsi = RSI(14, 1);
//				rsi1 = RSI(BarsArray[1], 14, 1);
//				rsi2 = RSI(BarsArray[2], 14, 1);
//				adx = ADX(14);
//				adx1 = ADX(BarsArray[1], 14);
//				adx2 = ADX(BarsArray[2], 14);
				
//				giPctSpd = GIPctSpd(8);
				giPairPctSpd = GIPairPctSpd(RocPeriod, SecondSymbol, ChartMinutes, CapRatio1, CapRatio2, PctChgSpdThreshold);
				
				// Add RSI and ADX indicators to the chart for display
				// This only displays the indicators for the primary Bars object (main instrument) on the chart
//				AddChartIndicator(rsi);
//				AddChartIndicator(adx);
//				AddChartIndicator(giPctSpd);
				AddChartIndicator(giPairPctSpd);
				
//				giPctSpd.RaiseIndicatorEvent += OnTradeByPctSpd;
//				giPctSpd.TM_ClosingH = TG_TradeEndH;
//				giPctSpd.TM_ClosingM = TG_TradeEndM;
				SetPrintOut(1);
				Print(String.Format("{0}: IsUnmanaged={1}", this.GetType().Name, IsUnmanaged));
				Print(String.Format("{0}: DataLoaded...BarsArray.Length={1}", this.GetType().Name, BarsArray.Length));
			}			
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < BarsRequiredToTrade)
				return;
//			giPctSpd.Update();
			giPairPctSpd.Update();
			//IndicatorProxy.Update();
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
			if (BarsInProgress != 0)
				return;
//			if (CurrentBars[0] < 0 || CurrentBars[1] < 0)
//				return;

			// Checks if the 14 period ADX on both instruments are trending (above a value of 30)
//			if (adx[0] > 30 && adx1[0] > 30)
//			{
				// If RSI crosses above a value of 30 then enter a long position via a limit order
//				if (CrossAbove(rsi, 30, 1))
//				{
					// Draws a square 1 tick above the high of the bar identifying when a limit order is issued
//					Draw.Square(this, "My Square" + CurrentBar, false, 0, High[0] + TickSize, Brushes.DodgerBlue);

					// Enter a long position via a limit order at the current ask price
					//EnterLongLimit(GetCurrentAsk(), "RSI");					
					//EnterLong(1);
//					EnterLong(0, 1, "RSI");
//					EnterShort(1, 1, "RSI");
//					EnterShort(2, 1, "RSI");
//				}
//			}

			// Any open long position will exit if RSI crosses below a value of 75
			// This is in addition to the trail stop set in the OnStateChange() method under State.Configure
//			if (CrossBelow(rsi, 75, 1)) {
				//ExitLong();
//				ExitLong(0, 1, "ExitRSI", "RSI");
//				ExitShort(1, 1, "ExitRSI", "RSI");
//				ExitShort(2, 1, "ExitRSI", "RSI");
//			}
		}
		
		#region En/Ex signals
		private bool CheckPnLByBarsSinceEn() { return false;
//			bool call_ex = false;
//			int bse = BarsSinceEntryExecution(giPctSpd.PctChgMaxBip, String.Empty, 0);
//			double ur_pnl = CheckUnrealizedPnLBip(giPctSpd.PctChgMaxBip);
//			if(bse == TM_BarsToCheckPnL && ur_pnl < 0)
//				call_ex = true;
//			return call_ex;
		}
		#endregion
		
		#region Indicator Event Handler
        // Define what actions to take when the event is raised. 
		/*
        void OnTradeByPctSpd(object sender, IndicatorEventArgs e) {
			IndicatorSignal isig = e.IndSignal;
			Print(String.Format("{0}:OnTradeByPctSpd triggerred {1} Bip{2}: PctSpd={3}, MaxBip={4}, MinBip={5}",
			CurrentBars[BarsInProgress], isig.SignalName, BarsInProgress, giPctSpd.PlotPctSpd[0], giPctSpd.PctChgMaxBip, giPctSpd.PctChgMinBip));
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
			
		} */
		
		void OnExitPositions(IndicatorEventArgs e) { /*
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
			}			*/
		}
		
		void OnEntryPositions(IndicatorEventArgs e) {
			
		}

		/// <summary>
		/// CapRatio: ES:RTY=1.7:1, NQ:RTY=2.1:1, NQ:ES=1.25:1
		/// </summary>		
		private int GetTradeQuantity(int idx, double ratio) {
			switch(idx) {
				case 0: return ratio>0? (int)ratio*5 : 5;
				case 1: return ratio>0? (int)ratio*4 :4;
				case 2: return ratio>0? (int)ratio*8 :8;
				default: return -1;
			}
		}
		
		#endregion
				
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RocPeriod", Description="Rate of chage period", Order=0, GroupName="GPS_CUSTOM_PARAMS")]
		public int RocPeriod
		{ 	get{
				return rocPeriod;
			}
			set{
				rocPeriod = value;
			}
		}

		[NinjaScriptProperty]		
		[Display(Name="SecondSymbol", Description="The second symbol of the pair", Order=1, GroupName="GPS_CUSTOM_PARAMS")]
		public string SecondSymbol
		{ 	get{
				return secondSymbol;
			}
			set{
				secondSymbol = value;
			}
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ChartMinutes", Description="Minutes for the chart", Order=2, GroupName="GPS_CUSTOM_PARAMS")]
		public int ChartMinutes
		{ 	get{
				return chartMinutes;
			}
			set{
				chartMinutes = value;
			}
		}
		
		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="CapRatio1", Description="CapRatio of first instrument", Order=3, GroupName="GPS_CUSTOM_PARAMS")]
		public double CapRatio1
		{ 	get{
				return capRatio1;
			}
			set{
				capRatio1 = value;
			}
		}

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="CapRatio2", Description="CapRatio of 2nd instrument", Order=4, GroupName="GPS_CUSTOM_PARAMS")]
		public double CapRatio2
		{ 	get{
				return capRatio2;
			}
			set{
				capRatio2 = value;
			}
		}

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="PctChgSpdThreshold", Description="PctChgSpd Threshold to entry", Order=5, GroupName="GPS_CUSTOM_PARAMS")]
		public double PctChgSpdThreshold
		{ 	get{
				return pctChgSpdThreshold;
			}
			set{
				pctChgSpdThreshold = value;
			}
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PlotPctSpd
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PlotRocSpd
		{
			get { return Values[1]; }
		}

		//The BarsInProgress for PctChgMax and PctChgMin
		[Browsable(false)]
		[XmlIgnore]
		public int PctChgMaxBip
		{
			get;set;
		}
		[Browsable(false)]
		[XmlIgnore]
		public int PctChgMinBip
		{
			get;set;
		}
		#endregion
		
		#region Pre Defined parameters
		private int rocPeriod = 8;
		private double capRatio1 = 1;
		private double capRatio2 = 1;
		private double pctChgSpdThreshold = 1;
		private string secondSymbol = "SCO";
		private int chartMinutes = 4;
		#endregion
	}
}
