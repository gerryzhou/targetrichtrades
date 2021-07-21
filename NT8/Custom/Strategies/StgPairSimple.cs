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
				IsFillLimitOnTouch							= false;
				TraceOrders									= false;
				BarsRequiredToTrade							= 128;
				IsUnmanaged									= false;
				OrderFillResolution							= OrderFillResolution.Standard;
				EntriesPerDirection							= 2;
				EntryHandling								= EntryHandling.AllEntries;
				DefaultQuantity								= 100;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
//				MM_ProfitFactorMax							= 1;
//				MM_ProfitFactorMin							= 0;
//				TG_TradeEndH								= 10;
//				TG_TradeEndM								= 45;
				TG_OpenStartH								= 8;
				TG_OpenStartM								= 30;
				MktPosition1								= MarketPosition.Long;
				MktPosition2								= MarketPosition.Long;
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
				giPairPctSpd = GIPairPctSpd(RocPeriod, SecondSymbol, ChartMinutes, CapRatio1, CapRatio2, PctChgSpdThresholdEn, PctChgSpdThresholdEx);
				
				// Add RSI and ADX indicators to the chart for display
				// This only displays the indicators for the primary Bars object (main instrument) on the chart
//				AddChartIndicator(rsi);
//				AddChartIndicator(adx);
//				AddChartIndicator(giPctSpd);
				AddChartIndicator(giPairPctSpd);
				
				giPairPctSpd.RaiseIndicatorEvent += OnTradeByPairPctSpd;
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

        void OnTradeByPairPctSpd(object sender, IndicatorEventArgs e) {
			IndicatorSignal isig = e.IndSignal;
			Print(String.Format("{0}:OnTradeByPairPctSpd triggerred {1} Bip{2}: PlotPctSpd={3}, PctChgSpdThresholdEn={4}",
			CurrentBars[BarsInProgress], isig.SignalName, BarsInProgress, giPairPctSpd.PlotPctSpd[0], giPairPctSpd.PctChgSpdThresholdEn));
			if(IsTradingTime(IndicatorProxy.GetTimeByHM(TG_OpenStartH, TG_OpenStartM, true)) && giPairPctSpd.PlotPctSpd[0] <= PctChgSpdThresholdEn)
				OnEntryPositions(e);
			else OnExitPositions(e);
			/*
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
			} */
			
		}
		
		void OnExitPositions(IndicatorEventArgs e) {

			int quant1 = base.GetTradeQuantity(0, CapRatio1);
			int quant2 = base.GetTradeQuantity(1, CapRatio2);
			Print(String.Format("{0}:OnExitPositions quant1={1}, quant2={2}: PlotPctSpd={3}, PctChgSpdThresholdEx={4}",
				CurrentBars[BarsInProgress], quant1, quant2, giPairPctSpd.PlotPctSpd[0], giPairPctSpd.PctChgSpdThresholdEx));
			//Exit positions for both legs
			if(GetMarketPosition(0) == MarketPosition.Long){
				ExitLong(0, quant1, "", giPairPctSpd.SignalName_EntryLongLeg1);
			}
			if(GetMarketPosition(1) == MarketPosition.Long){
				ExitLong(1, quant2, "", giPairPctSpd.SignalName_EntryLongLeg2);
			}
			if(GetMarketPosition(0) == MarketPosition.Short){
				ExitShort(0, quant1, "", giPairPctSpd.SignalName_EntryShortLeg1);
			}
			if(GetMarketPosition(1) == MarketPosition.Short){
				ExitShort(1, quant2, "", giPairPctSpd.SignalName_EntryShortLeg2);
			}

			/*
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
			//New entry with no poistions for both legs
			if(HasPosition(0) <= 0 && HasPosition(1) <= 0) {
				int quant1 = base.GetTradeQuantity(0, CapRatio1);
				int quant2 = base.GetTradeQuantity(1, CapRatio2);
				Print(String.Format("{0}:OnEntryPositions quant1={1}, quant2={2}: PlotPctSpd={3}, PctChgSpdThresholdEn={4}",
				CurrentBars[BarsInProgress], quant1, quant2, giPairPctSpd.PlotPctSpd[0], giPairPctSpd.PctChgSpdThresholdEn));
				if(MktPosition1 == MarketPosition.Long) {
					EnterLong(0, quant1, giPairPctSpd.SignalName_EntryLongLeg1);
				} else if(MktPosition1 == MarketPosition.Short) {
					EnterShort(0, quant1, giPairPctSpd.SignalName_EntryShortLeg1);
				}
				
				if(MktPosition2 == MarketPosition.Long) {
					EnterLong(1, quant2, giPairPctSpd.SignalName_EntryLongLeg2);
				} else if(MktPosition2 == MarketPosition.Short) {
					EnterShort(1, quant2, giPairPctSpd.SignalName_EntryShortLeg2);
				}
			}
		}

		public override bool IsTradingTime(int session_start) {
			//Bars.Session.GetNextBeginEnd(DateTime time, out DateTime sessionBegin, out DateTime sessionEnd)
			int time_start = IndicatorProxy.GetTimeByHM(TG_TradeStartH, TG_TradeStartM, true);
			int time_end = IndicatorProxy.GetTimeByHM(TG_TradeEndH, TG_TradeEndM, true);
			int time_now = ToTime(Time[0]);
			bool isTime= false;
			if (time_now >= session_start && time_now >= time_start && time_now <= time_end) {
				isTime = true;
			}
			Print(String.Format("{0}: time_now={1}, session_start={2}, time_start={3}, time_end={4}, isTime={5}",
			CurrentBar, time_now, session_start, time_start, time_end, isTime));
			return isTime;
		}
		#endregion

		/// <summary>
		/// CapRatio: ES:RTY=1.7:1, NQ:RTY=2.1:1, NQ:ES=1.25:1
		/// </summary>		
//		public override int GetTradeQuantity(int idx, double ratio) {
//			switch(idx) {
//				case 0: return base.GetTradeQuantity(idx, CapRatio1); // ratio>0? (int)ratio*5 : 5;
//				case 1: return base.GetTradeQuantity(idx, CapRatio2); //ratio>0? (int)ratio*4 :4;
//				default: return -1;
//			}
//		}
		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RocPeriod", Description="Rate of chage period", Order=0, GroupName=GPS_CUSTOM_PARAMS)]
		public int RocPeriod
		{ 	get{
				return rocPeriod;
			}
			set{
				rocPeriod = value;
			}
		}

		[NinjaScriptProperty]		
		[Display(Name="SecondSymbol", Description="The second symbol of the pair", Order=1, GroupName=GPS_CUSTOM_PARAMS)]
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
		[Display(Name="ChartMinutes", Description="Minutes for the chart", Order=2, GroupName=GPS_CUSTOM_PARAMS)]
		public int ChartMinutes
		{ 	get{
				return chartMinutes;
			}
			set{
				chartMinutes = value;
			}
		}
		
		[NinjaScriptProperty]
		//[Range(1, double.MaxValue)]
		[Display(Name="MktPosition1", Description="Long or short for the leg1 entry", Order=3, GroupName=GPS_CUSTOM_PARAMS)]
		public MarketPosition MktPosition1
		{ 	get; set;
		}
		
		[NinjaScriptProperty]
		//[Range(1, double.MaxValue)]
		[Display(Name="MktPosition2", Description="Long or short for the leg2 entry", Order=4, GroupName=GPS_CUSTOM_PARAMS)]
		public MarketPosition MktPosition2
		{ 	get; set;
		}
		
		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="CapRatio1", Description="CapRatio of first leg", Order=5, GroupName=GPS_CUSTOM_PARAMS)]
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
		[Display(Name="CapRatio2", Description="CapRatio of 2nd leg", Order=6, GroupName=GPS_CUSTOM_PARAMS)]
		public double CapRatio2
		{ 	get{
				return capRatio2;
			}
			set{
				capRatio2 = value;
			}
		}
		
		[NinjaScriptProperty]
		[Range(double.MinValue, double.MaxValue)]
		[Display(Name="PctChgSpdThresholdEn", Description="PctChgSpd Threshold to entry", Order=7, GroupName=GPS_CUSTOM_PARAMS)]
		public double PctChgSpdThresholdEn
		{ 	get{
				return pctChgSpdThresholdEn;
			}
			set{
				pctChgSpdThresholdEn = value;
			}
		}
		
		[NinjaScriptProperty]
		[Range(double.MinValue, double.MaxValue)]
		[Display(Name="PctChgSpdThresholdEx", Description="PctChgSpd Threshold to exit", Order=8, GroupName=GPS_CUSTOM_PARAMS)]
		public double PctChgSpdThresholdEx
		{ 	get{
				return pctChgSpdThresholdEx;
			}
			set{
				pctChgSpdThresholdEx = value;
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
		private double capRatio1 = 1.25;
		private double capRatio2 = 1;
		private double pctChgSpdThresholdEn = -2.3;
		private double pctChgSpdThresholdEx = 2.5;
		private string secondSymbol = "SPY";
		private int chartMinutes = 4;
		#endregion
	}
}
