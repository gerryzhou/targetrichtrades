#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.AddOns;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Indicators.PriceActions;
using NinjaTrader.NinjaScript.Indicators.ZTraderPattern;
using NinjaTrader.NinjaScript.Strategies.ZTraderStg;

#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	/// <summary>
	/// Sample strategy for GSZTrader:
	/// 1) OnStateChange();
	/// 2) OnBarUpdate();
	/// 3) GetIndicatorSignal();
	/// 4) GetTradeSignal();
	/// 5) CheckExitTrade() or CheckNewEntryTrade();
	/// 6) PutTrade();
	/// 
	/// New approach for Event fired trades:
	/// Lock the order entry when submit order;
	/// Unlock the order entry when order executed;
	/// or Check tradeActino at the close of each bar;
	/// 
	/// Indicator Combination:
	/// * SnR: daily high/low
	/// * Breakout: morning breakout of the SnR, big bar cross the SnR
	/// * Reversal Pivot: 9:00-11 AM morning session high/low
	/// * Pullback Pivot: left 20+, right 5+, i.e. (20+, 5+)
	/// * Trending pivot: breakout the pullback pivot, create a new (5+, 5+) pivot
	/// Long/Short rules:
	/// * KAMA indicates trend(not helpful); Use ParabolicSAR(0.002,0.1,0.002) as trending indicator; 
	/// * Long: cyan diamond, Short: red diamond;
	/// * Trend-following long/short entry;
	/// * Stop loss: last n (five?) bars hi/lo;
	/// * Profit Target: next support/resistance, cyan/red diamond;
	/// * Breakeven, or KAMA/EMA did not moving towards target in a period of time, exit?
	/// * Use cyan/red diamond find key reversal: 
	/// 	look back n bars, find the highest/lowest bar as KR; It's leading key reversal;
	/// =====================StgProdTRT=====================================================
	/// 1.	Define overall market motion for the recent past – 
	/// Record the lowest low of the last five candles and the highest high at the open of 
	/// the current day - candle ‘n’ (n-5, n-4, n-3, n-2, n-1) – these will be initial 
	/// breakout or breakdown levels for the trend
	/// Note the current 5EMA of candle ‘n’ –
	/// a.	If the close of candle ‘n-5’ is less that the current 5EMA, the trend is positive
	/// with the support edge at the lowest low of (n-5, n-4, n-3, n-2, n-1).  A break of this
	/// low sends us to a YELLOW or WAIT state for a positive trend  [trend detection]
	/// b.	IF the trend is defined as positive, entries will be defined as follows-
	/// (a)	If VWAP is above the daily open, engage at the test of the VWAP long, and add to 
	/// the position any test of the VWAP to max size with 1st target at prior day’s 
	/// high (if it is higher) or the highest high of the n-5, n-4, n-3, n-2, n-1 candles 
	/// -where we take ½ position and trail position by 20 ticks.  [entry signal]
	/// (b)	Automatic stopout – loss of the prior day’s low.  Alternate exit at 20 ticks 
	/// below the 5EMA, or if the VWAP drops below the daily open, or if the price loses the 
	/// low of the measured sequence of n-5, n-4, n-3, n-2, n-1 before any of those.  [exit signal]
	/// 
	/// 2.	The reversal of this would be the short environment.
	/// 
	/// --SPECIAL EVENTS-in either the long or short environment-- 
	/// 3.	If the chart gaps down at the opening tick but is above the low of the prior 5 days, 
	/// there will a countertrend bounce into the moving averages – short at the open of the 
	/// prior 4 hr candle close or long at the opening tick with the stop at the low of the 
	/// prior 4hr minus 6 ticks into the first target of either the VWAP or 5EMA on the 4hr chart
	/// then trail 16 ticks.  This trade can be taken repeatedly as long as the 4hr low of the 
	/// prior candle holds.
	/// 4.	The reverse will hold true on a gap up that is below the high of the prior 5 days, 
	/// there will be a countertrend fade into the moving averages.
	/// </summary>
	public class StgProdTRT : GStrategyBase
	{
		#region Variables
		//private GISMI giSMI;
		//private GIAwesomeOscillator awOscillator;
		private GIEMA giEMA;
		private GIVWAP giVwap;
		//private GIPbSAR giPbSAR;
		private GISnR giSnR;
		//private GISnRPriorWM giSnRPriorWM;
		private GIHLnBars giHLnBars;
		
		private JsonStgTRT ctxTRT;
			
		private int barNo_EnInflection = -1;
		
		private double c0 = 0, hiN = Double.MaxValue, loN = Double.MinValue;
		
		#endregion
		
		#region Init Functions
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				Print(this.Name + " StgProdTRT SetDefaults called....");
				Description									= @"The prod strategy for TRT.";
				Name										= "StgProdTRT";
				Calculate									= Calculate.OnBarClose;
				IsFillLimitOnTouch							= false;
				TraceOrders									= false;
				BarsRequiredToTrade							= 22;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
			}
			else if (State == State.DataLoaded)
			{
				Print(this.Name + " StgProdTRT Set DataLoaded called....");
				AddChartIndicator(IndicatorProxy);
				SetPrintOut(1);
				IndicatorProxy.LoadSpvPRList(SpvDailyPatternES.spvPRDayES);
				IndicatorProxy.AddPriceActionTypeAllowed(PriceActionType.DnWide);
				GetMarketContext();
				GAlert.LoadAlerConfig(IndicatorProxy);
				
				//giSMI = GISMI(EMAPeriod1, EMAPeriod2, Range, SMITMAPeriod, SMICrossLevel);//(3, 5, 5, 8);
				//awOscillator = GIAwesomeOscillator(FastPeriod, SlowPeriod, Smooth, MovingAvgType.SMA, false);//(5, 34, 5, MovingAvgType.SMA);
				giEMA = GIEMA(EMAPeriod1, StoplossTicksEMA);
				giVwap = GIVWAP();
				//giPbSAR = GIPbSAR(AccPbSAR, AccMaxPbSAR, AccStepPbSAR);
				giSnR = GISnR(false, false, true, true, true, this.ctxTRT.TimeOpen, this.ctxTRT.TimeClose);
				//giSnRPriorWM = GISnRPriorWM(true, false, false, false, true, false, false, false);
				giHLnBars = GIHLnBars(5);
				
				giHLnBars.RaiseIndicatorEvent += OnStopLossEvent;
				giHLnBars.RaiseIndicatorEvent += OnProfitTargetEvent;
				giSnR.RaiseIndicatorEvent += OnStopLossEvent;
				giSnR.RaiseIndicatorEvent += OnProfitTargetEvent;
				giVwap.RaiseIndicatorEvent += OnStopLossEvent;
				giEMA.RaiseIndicatorEvent += OnStopLossEvent;
				//this.RaiseStrategyEvent += OnGISnREvent;
				
				//AddChartIndicator(giSMI);
				//AddChartIndicator(awOscillator);
				AddChartIndicator(giEMA);
				AddChartIndicator(giVwap);
				//AddChartIndicator(giPbSAR);
				AddChartIndicator(giSnR);
				//AddChartIndicator(giSnRPriorWM);
//				Print("GISMI called:" + "EMAPeriod1=" + EMAPeriod1 + "EMAPeriod2=" + EMAPeriod2 + "Range=" + Range + "SMITMAPeriod=" + SMITMAPeriod);
//				IndicatorProxy.PrintLog(true, IsLiveTrading(), String.Format("{0}: StgProdTRT GetMarketContext called...", CurrentBar));
			}
			else if (State == State.Configure)
			{
				Print(this.Name + " StgProdTRT Set Configure called.... CurrentTrade=" + CurrentTrade);
				AddDataSeries(Data.BarsPeriodType.Day, 1);
			}
		}

		/// <summary>
		/// Call the base as default implementation
		/// </summary>
		protected override void OnBarUpdate()
		{
			try {
				//GetHiLoNPrice();
				//giHLnBars.Update();
				base.OnBarUpdate();
				CheckPerformance();
//				if(giSnR.IsCutoffTime(BarsInProgress, 11, 20)) {
//				Print(String.Format("{0}:[{1}] IsCutoffTime EnEx Bip{2}: ",
//				CurrentBar, Times[BarsInProgress][0], BarsInProgress));
//			}
				IndicatorProxy.TraceMessage(this.Name, PrintOut);
				Print(String.Format("{0}: Stg={1}, GSZTrader={2}", CurrentBar, CurrentTrade.InstStrategy, IndicatorProxy.GSZTrader));
			} catch (Exception ex) {
				IndicatorProxy.Log2Disk = true;
				IndicatorProxy.PrintLog(true, true, "Exception: " + ex.StackTrace);
			}
		}
		#endregion

		#region Signal Functions
		/// <summary>
		/// Check the time, position, trend,
		/// </summary>
		/// <returns></returns>
		public override bool CheckNewEntrySignals(){
			//giPbSAR.Update();
//			Print(CurrentBar + ":CheckNewEntrySignals called -----------" + giSMI.LastInflection);

			if(NewTradeAllowed())
				IndicatorProxy.PrintLog(true, IsLiveTrading(), String.Format("{0}: CheckNewEntrySignals called....NewOrderAllowed", CurrentBar));
			else {
				IndicatorProxy.PrintLog(true, IsLiveTrading(), String.Format("{0}: CheckNewEntrySignals called....NewOrderNotAllowed", CurrentBar));
				return false;
			}
			
//			IndicatorSignal indSig = giSMI.GetLastIndicatorSignalByName(CurrentBar, giSMI.SignalName_Inflection);
			
//			if(indSig != null && indSig.SignalAction != null)
//				Print(CurrentBar + ":stg-Last " + giSMI.SignalName_Inflection + "=" + indSig.BarNo + "," + indSig.SignalAction.SignalActionType.ToString());

//			IndicatorSignal indSigCrs = giSMI.GetLastIndicatorSignalByName(CurrentBar, giSMI.SignalName_LineCross);
			
//			if(indSigCrs != null && indSigCrs.SignalAction != null)
//				Print(CurrentBar + ":stg-Last " + giSMI.SignalName_LineCross + "=" + indSigCrs.BarNo + "," + indSigCrs.SignalAction.SignalActionType.ToString());
			giEMA.Update();
			giVwap.Update();
			giSnR.Update();
			giHLnBars.Update();
			if(PatternMatched()) {
				Direction dir = GetDirection(giEMA); //GetDirection(giPbSAR);
				TradeSignal enSig = new TradeSignal();			
				enSig.BarNo = CurrentBar;
				//enSig.SignalName = "StgProdTRT-Entry";
				enSig.SignalType = TradeSignalType.Entry;
				enSig.SignalSource = TradeSignalSource.Indicator;
				enSig.OrderCalculationMode = CalculationMode.Price;
				enSig.Quantity = 1;
				
				if(dir.TrendDir==TrendDirection.Up) 
					enSig.Action = OrderAction.Buy;
				else if(dir.TrendDir==TrendDirection.Down) 
					enSig.Action = OrderAction.Sell;
				enSig.Order_Type = OrderType.Market;
				AddTradeSignal(CurrentBar, IndicatorTradeSignals, enSig);
				giHLnBars.RefBarLowestN = CurrentBar;
				giHLnBars.RefBarLowestN = CurrentBar;
				Print(CurrentBar + ":PatternMatched-- " + enSig.Action.ToString() + "," + enSig.SignalSource.ToString());
				AlertTradeSignal(enSig, "Entry Signal Alert");
				return true;
			} else
				return false;
		}
		
		public override bool CheckScaleInSignal(){
			return false;
		}
		
		public override bool CheckScaleOutSignal(){
			return false;
		}
		
		/// <summary>
		/// [entry signal]
		/// (a)	If VWAP is above the daily open, engage at the test of the VWAP long, 
		/// and add to the position any test of the VWAP to max size with 1st target 
		/// at prior day’s high (if it is higher) or the highest high of the n-5, n-4,
		/// n-3, n-2, n-1 candles -where we take ½ position and trail position by 20 ticks.
		/// </summary>
		/// <returns></returns>
		private bool CheckVwapCrossSignal() {
			IndicatorSignal indSig = new IndicatorSignal();
			indSig.SignalAction = new SignalAction();
			indSig.SignalAction.SnR = new SupportResistanceRange<double>();
			
			if(CrossAbove(Close, this.giVwap, 1)) {
				indSig.SignalAction.SignalActionType = SignalActionType.CrossOver;
			}
			else if(CrossBelow(Close, this.giVwap, 1)) {
				indSig.SignalAction.SignalActionType = SignalActionType.CrossUnder;
			} else
				return false;
			
			indSig.SignalName = IndicatorProxy.SignalName_LineCross;
			indSig.SignalAction.SnR.Resistance = Math.Max(this.ctxTRT.R1, this.giSnR.LastDayRst[0]);
			indSig.SignalAction.SnR.Support = Math.Min(this.ctxTRT.S1, this.giSnR.LastDaySpt[0]);
			indSig.BarNo = CurrentBar;
			indSig.IndicatorSignalType = SignalType.SimplePriceAction;
			giVwap.AddIndicatorSignal(CurrentBar, indSig);
			return true;
		}
		
		public override bool CheckStopLossSignal() {
//			List<TradeSignal> indTdSig = GetTradeSignalByType(CurrentBar, IndicatorTradeSignals, TradeSignalType.StopLoss);
//			if(indTdSig != null && indTdSig.Count>0) return false;
			
			TradeAction ta = CurrentTrade.TradeAction;
			if( ta != null && ta.StopLossSignal != null) {
			IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				String.Format("{0}: CheckStopLossSignal called ---------StopLossSignal != null", CurrentBar));
				return false;
			}
			IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				String.Format("{0}: CheckStopLossSignal called ------", CurrentBar));
			giEMA.Update();
			giVwap.Update();
			giSnR.Update();
			giHLnBars.Update();
			MarketPosition mpos = GetMarketPosition();
			if(mpos == MarketPosition.Flat) return false;
			else {
				BreakoutDirection bkd_lastDayHL = CheckLastDayHLBreakout();
				
				if(mpos == MarketPosition.Long) {
				}
				else if (mpos == MarketPosition.Short) {
				}
			}
			
			Direction dir = GetDirection(giEMA); //GetDirection(giPbSAR);
			TradeSignal slSig = new TradeSignal();
			slSig.BarNo = CurrentBar;
			slSig.SignalType = TradeSignalType.StopLoss;
			slSig.Order_Type = OrderType.Market;
			slSig.SignalSource = TradeSignalSource.Indicator;
			slSig.OrderCalculationMode = CalculationMode.Price;
			slSig.Quantity = 1;
			if(dir.TrendDir==TrendDirection.Up) {
				slSig.Action = OrderAction.Sell;
				slSig.StopPrice = GetStopLossPrice(SupportResistanceType.Support, Close[0]);
			}
			else if(dir.TrendDir==TrendDirection.Down) {
				slSig.Action = OrderAction.Buy;
				slSig.StopPrice = GetStopLossPrice(SupportResistanceType.Resistance, Close[0]);
			}
			IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				String.Format("{0}:CheckStopLossSignal slSig.StopPrice={1}", CurrentBar, slSig.StopPrice));
			
			AddTradeSignal(CurrentBar, IndicatorTradeSignals, slSig);
			return true;
		}
		
		/// <summary>
		/// loss of the prior day’s low or breakout prior day's high
		/// </summary>
		/// <returns></returns>
		private BreakoutDirection CheckLastDayHLBreakout() {
			BreakoutDirection bkdir = BreakoutDirection.UnKnown;
			if(Close[0] < giSnR.LastDaySpt[0])
				bkdir = BreakoutDirection.Down;
			if(Close[0] > giSnR.LastDayRst[0])
				bkdir = BreakoutDirection.Up;
			return bkdir;
		}
		
		/// <summary>
		/// exit at 20 ticks below the 5EMA
		/// </summary>
		/// <returns></returns>
		private BreakoutDirection CheckEMABreakout() {
			BreakoutDirection bkdir = BreakoutDirection.UnKnown;
			if(Close[0] < giEMA[0] - StoplossTicksEMA)
				bkdir = BreakoutDirection.Down;
			if(Close[0] > giEMA[0] + StoplossTicksEMA)
				bkdir = BreakoutDirection.Up;
			return bkdir;
		}

		/// <summary>
		/// if the VWAP drops below the daily open
		/// </summary>
		/// <returns></returns>
		private BreakoutDirection CheckVWapOpenDBreakout() {
			BreakoutDirection bkdir = BreakoutDirection.UnKnown;
			if(giVwap[0] < giSnR.TodayOpen[0])
				bkdir = BreakoutDirection.Down;
			if(giVwap[0] > giSnR.TodayOpen[0])
				bkdir = BreakoutDirection.Up;
			return bkdir;
		}
		
		/// <summary>
		/// Check if current close break out the lowest low
		/// or the highest high of last n bars
		/// </summary>
		/// <returns></returns>
		private BreakoutDirection CheckHLnBarsBreakout() {
			BreakoutDirection bkdir = BreakoutDirection.UnKnown;
			if(Close[0] < giHLnBars.LowestN[1])
				bkdir = BreakoutDirection.Down;
			if(Close[0] > giHLnBars.HighestN[1])
				bkdir = BreakoutDirection.Up;
			return bkdir;
		}

		public override bool CheckProfitTargetSignal() {			
//			List<TradeSignal> indTdSig = GetTradeSignalByType(CurrentBar, IndicatorTradeSignals, TradeSignalType.ProfitTarget);
//			if(indTdSig != null && indTdSig.Count>0) return false;

			TradeAction ta = CurrentTrade.TradeAction;
			if(ta != null && ta.ProfitTargetSignal != null) {
				IndicatorProxy.PrintLog(true, IsLiveTrading(), 
					String.Format("{0}: CheckProfitTargetSignal called ----ProfitTargetSignal != null", CurrentBar));
				return false;
			}
			IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				String.Format("{0}: CheckProfitTargetSignal called ----", CurrentBar));

			//giPbSAR.Update();
			Direction dir = GetDirection(giEMA);// GetDirection(giPbSAR);
			TradeSignal ptSig = new TradeSignal();
			ptSig.BarNo = CurrentBar;
			ptSig.SignalType = TradeSignalType.ProfitTarget;
			ptSig.Order_Type = OrderType.Limit;
			ptSig.SignalSource = TradeSignalSource.Indicator;
			ptSig.OrderCalculationMode = CalculationMode.Price;
			ptSig.Quantity = 1;
			if(dir.TrendDir==TrendDirection.Up) {
				ptSig.Action = OrderAction.Sell;
				ptSig.LimitPrice = GetProfitTargetPrice(SupportResistanceType.Resistance);
			}
			else if(dir.TrendDir==TrendDirection.Down) {
				ptSig.Action = OrderAction.Buy;
				ptSig.LimitPrice = GetProfitTargetPrice(SupportResistanceType.Support);
			}
			IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				String.Format("{0}: CheckProfitTargetSignal - ptSig.LimitPrice={1}", CurrentBar, ptSig.LimitPrice));
			
			AddTradeSignal(CurrentBar, IndicatorTradeSignals, ptSig);
			return true;
		}
		
		
		/// <summary>
		/// Set the entry signal for TradeAction, 
		/// with the combination of command, perform/rule, indicators signals;
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		public override void SetEntrySignal(TradeAction action) {
			List<TradeSignal> cmdSig = GetTradeSignalByType(CurrentBar, CommandSignals, TradeSignalType.Entry);
			List<TradeSignal> evtSig = GetTradeSignalByType(CurrentBar, EventSignals, TradeSignalType.Entry);
			List<TradeSignal> indTdSig = GetTradeSignalByType(CurrentBar, IndicatorTradeSignals, TradeSignalType.Entry);
			
			if(cmdSig != null && cmdSig.Count>0)
				action.EntrySignal = cmdSig[0];
			else if (evtSig != null && evtSig.Count>0)
				action.EntrySignal = evtSig[0];
			else if(indTdSig != null && indTdSig.Count>0)
				action.EntrySignal = indTdSig[0];
			//return enSig;
		}
		
		public override void SetScaleInSignal(TradeAction action) {
			
		}
		
		public override void SetScaleOutSignal(TradeAction action) {
			
		}
		
		/// <summary>
		/// Set the stop loss signal for TradeAction, 
		/// with the combination of command, perform/rule, indicators signals;
		/// Initial SL: last inflection, lowest/highest lo/hi back N bars,(tighter preferred),
		/// if both are invalide, take the fixed $/tick amount
		/// Move SL when: Perform/MM rules triggerred, move to breakeven or higher;
		/// To identify the first signal after entry and the rest with existing order:
		/// 
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		public override void SetStopLossSignal(TradeAction action) {
			List<TradeSignal> cmdSig = GetTradeSignalByType(CurrentBar, CommandSignals, TradeSignalType.StopLoss);
			List<TradeSignal> evtSig = GetTradeSignalByType(CurrentBar, EventSignals, TradeSignalType.StopLoss);
			List<TradeSignal> indTdSig = GetTradeSignalByType(CurrentBar, IndicatorTradeSignals, TradeSignalType.StopLoss);
			
			if(cmdSig != null && cmdSig.Count>0)
				action.StopLossSignal = cmdSig[0];
			else if (evtSig != null && evtSig.Count>0)
				action.StopLossSignal = evtSig[0];
			else if(indTdSig != null && indTdSig.Count>0)
				action.StopLossSignal = indTdSig[0];
			IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				String.Format("{0}: action.StopLossSignal=", CurrentBar, action.StopLossSignal));
//			else {
//				TradeSignal slSig = new TradeSignal();
//				slSig.BarNo = CurrentBar;
//				slSig.SignalType = TradeSignalType.StopLoss;
//				slSig.Order_Type = OrderType.Market;
//				slSig.SignalSource = TradeSignalSource.Event;
//				slSig.OrderCalculationMode = CalculationMode.Price;
//				slSig.Quantity = 1;
				
//				if(CurrentTrade.MktPosition == MarketPosition.Long) {
//					slSig.Action = OrderAction.Sell;
//					//slSig.StopPrice = GetStopLossPrice(SupportResistanceType.Support);
//				}
//				else if(CurrentTrade.MktPosition == MarketPosition.Short) {
//					slSig.Action = OrderAction.Buy;
//					//slSig.StopPrice = GetStopLossPrice(SupportResistanceType.Resistance);
//				}
//				else return;
//				action.StopLossSignal = slSig; 
//			}
		}

		/// <summary>
		/// Set the profit target signal for TradeAction, 
		/// with the combination of command, perform/rule, indicators signals;
		/// Initial PT: last inflection, lowest/highest lo/hi back N bars,(wider perferred),
		/// if both invalid, take the fixed $/tick amount
		/// Move target when: perform/MM rules triggerred, move/lock profits;
		/// Identify new or existing PT signal:
		/// 
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		public override void SetProfitTargetSignal(TradeAction action) {
			List<TradeSignal> cmdSig = GetTradeSignalByType(CurrentBar, CommandSignals, TradeSignalType.ProfitTarget);
			List<TradeSignal> evtSig = GetTradeSignalByType(CurrentBar, EventSignals, TradeSignalType.ProfitTarget);
			List<TradeSignal> indTdSig = GetTradeSignalByType(CurrentBar, IndicatorTradeSignals, TradeSignalType.ProfitTarget);
			
			if(cmdSig != null && cmdSig.Count>0)
				action.ProfitTargetSignal = cmdSig[0];
			else if (evtSig != null && evtSig.Count>0)
				action.ProfitTargetSignal = evtSig[0];
			else if(indTdSig != null && indTdSig.Count>0)
				action.ProfitTargetSignal = indTdSig[0];
			IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				String.Format("{0}: action.ProfitTargetSignal=", CurrentBar, action.ProfitTargetSignal));
			//			else {
//				TradeSignal ptSig = new TradeSignal();
//				ptSig.BarNo = CurrentBar;
//				ptSig.SignalType = TradeSignalType.ProfitTarget;
//				ptSig.Order_Type = OrderType.Limit;
//				ptSig.SignalSource = TradeSignalSource.Event;
//				ptSig.OrderCalculationMode = CalculationMode.Price;
//				ptSig.Quantity = 1;
//				if(CurrentTrade.MktPosition == MarketPosition.Long) {
//					ptSig.Action = OrderAction.Sell;
//					//ptSig.LimitPrice = GetProfitTargetPrice(SupportResistanceType.Resistance);
//				}
//				else if(CurrentTrade.MktPosition == MarketPosition.Short) {
//					ptSig.Action = OrderAction.Buy;
//					//ptSig.LimitPrice = GetProfitTargetPrice(SupportResistanceType.Support);
//				}
//				else return;
//				action.ProfitTargetSignal = ptSig;
//			}
		}
		
		protected override bool PatternMatched()
		{
			double offset_sl, offset_pt;
			//Print("CurrentBar, barsMaxLastCross, barsAgoMaxPbSAREn,=" + CurrentBar + "," + barsAgoMaxPbSAREn + "," + barsSinceLastCross);
//			if (giParabSAR.IsSpvAllowed4PAT(curBarPriceAction.paType) && barsSinceLastCross < barsAgoMaxPbSAREn) 
//				return true;
//			else return false;
			PriceAction pa = IndicatorProxy.GetPriceAction(Time[0]);
			IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":"
				+ ";ToShortDateString=" + Time[0].ToString()
				+ ";paType=" + pa.paType.ToString()
				+ ";maxDownTicks=" + pa.voltality
				);
//			SignalActionType sat = giSMI.IsLastBarInflection();

			if(CheckVwapCrossSignal()) {
				Direction dir = GetDirection(giEMA);// GetDirection(giPbSAR);
				Direction dirvwap = GetDirectionVwap();
				if(dir.TrendDir == TrendDirection.Up && dirvwap.TrendDir == TrendDirection.Up) {
//					IndicatorSignal vSig = giVwap.GetIndicatorSignalByActionType(CurrentBar, SignalActionType.CrossUnder);
					offset_sl = GetStopLossOffset(SupportResistanceType.Support, Close[0]);
					offset_pt = GetProfitTargetOffset(SupportResistanceType.Resistance, Close[0]);
					
//					IndicatorProxy.PrintLog(true, IsLiveTrading(),
//						String.Format("{0}:{1} CheckVwapCrossSignal={2},{3}", CurrentBar, vSig.BarNo, vSig.SignalName, vSig.SignalAction.SignalActionType));
					if(this.MM_ProfitFactorMin > 0 && this.MM_ProfitFactorMax > 0)
						return IsProfitFactorValid(offset_sl, offset_pt, this.MM_ProfitFactorMin, this.MM_ProfitFactorMax);
					else
						return true;
				}
				if(dir.TrendDir == TrendDirection.Down && dirvwap.TrendDir == TrendDirection.Down) {
//					IndicatorSignal vSig = giVwap.GetIndicatorSignalByActionType(CurrentBar, SignalActionType.CrossOver);
					offset_sl = GetStopLossOffset(SupportResistanceType.Resistance, Close[0]);
					offset_pt = GetProfitTargetOffset(SupportResistanceType.Support, Close[0]);
//					IndicatorProxy.PrintLog(true, IsLiveTrading(),
//						String.Format("{0}:{1} CheckVwapCrossSignal={2},{3}", CurrentBar, vSig.BarNo, vSig.SignalName, vSig.SignalAction.SignalActionType));
					if(this.MM_ProfitFactorMin > 0 && this.MM_ProfitFactorMax > 0)
						return IsProfitFactorValid(offset_sl, offset_pt, this.MM_ProfitFactorMin, this.MM_ProfitFactorMax);
					else
						return true;
				}
			}
			
//			if((sat == SignalActionType.InflectionDn && dir.TrendDir == TrendDirection.Up) && giSMI.IsPullBack(-SMITmaUp, -SMITmaLow) ||
//				(sat == SignalActionType.InflectionUp && dir.TrendDir == TrendDirection.Down && giSMI.IsPullBack(SMITmaLow, SMITmaUp)))
//				return true;
//			else
				return false;
			//barsAgoMaxPbSAREn Bars Since PbSAR reversal. Enter the amount of the bars ago maximum for PbSAR entry allowed
		}

		public override void AlertTradeSignal(TradeSignal tsig, string caption) {
			AlertMessage altMsg = 
			new AlertMessage(this.Owner, "Alert triggerred!" + Environment.NewLine 
				+ tsig.SignalToStr(), caption);
				//if(CurrentBar == Bars.Count-2) {
			Print(String.Format("{0}: InstallDir ={1}, GAlert.AlertBarsBack={2}", 
				CurrentBar, NinjaTrader.Core.Globals.InstallDir, GAlert.AlertBarsBack));
			if(State != State.Historical || CurrentBar >= Bars.Count-GAlert.AlertBarsBack) {
				
				//Bars.Instrument
				//NinjaTrader.NinjaScript.Alert.AlertCallback(NinjaTrader.Cbi.Instrument.GetInstrument("MSFT"), this, "someId", NinjaTrader.Core.Globals.Now, Priority.High, "message", NinjaTrader.Core.Globals.InstallDir+@"\sounds\Alert1.wav", new SolidColorBrush(Colors.Blue), new SolidColorBrush(Colors.White), 0);
				// Instead of PlaySound()
				//NinjaTrader.NinjaScript.Alert.AlertCallback(Bars.Instrument, this, "someId", NinjaTrader.Core.Globals.Now, Priority.High, "message", NinjaTrader.Core.Globals.InstallDir+@"\sounds\Alert1.wav", new SolidColorBrush(Colors.Blue), new SolidColorBrush(Colors.White), 0);
				GAlert.PlaySoundFile(altMsg, IndicatorProxy);
			}
		}
		
		#endregion
		
		#region Trade Actions
		public override bool SetNewEntryTradeAction() {
			//CheckIndicatorSignals();
			if(NewTradeAllowed())
				IndicatorProxy.PrintLog(true, IsLiveTrading(), String.Format("{0}: SetNewEntryTradeAction called....NewOrderAllowed", CurrentBar));
			else {
				IndicatorProxy.PrintLog(true, IsLiveTrading(), String.Format("{0}: SetNewEntryTradeAction called....NewOrderNotAllowed", CurrentBar));
				return false;
			}
			
			TradeAction ta = new TradeAction();
			ta.BarNo = CurrentBar;
			ta.ActionName = "SampleTRT-Entry";
			ta.ActionType = TradeActionType.Bracket;
			ta.ActionStatus = TradeActionStatus.New;
			SetEntrySignal(ta);
			CurrentTrade.TradeAction = ta;
			IndicatorProxy.PrintLog(true, IsLiveTrading(),
				string.Format("{0}: SetNewEntryTradeAction EnSig={1}, SLSig={2}, PTSig={3}, StopLossPrice={4}, ProfitTargetPrice={5}",
				CurrentBar, ta.EntrySignal, ta.StopLossSignal, ta.ProfitTargetSignal, ta.StopLossPrice, ta.ProfitTargetPrice));
			return true;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns> 
		/// Bug: ocoID duplicated setting SL/PT orders;
		/// Bug: no exit tradeAction set if SL/PT were set and no changes in the coming bars; 
		/// Bug: ta not set after entry filled;
		public override bool SetExitTradeAction() {
			TradeAction ta = CurrentTrade.TradeAction;//GetTradeAction(CurrentBar);
			if(ta == null || //no entry action, SemiAlgo
				(ta.IsEntryAction() && ta.ActionStatus == TradeActionStatus.Executed)) { //entry order filled
				ta = new TradeAction();
				ta.BarNo = CurrentBar;
				ta.ActionName = "SampleTRT-ExitOCO";
				ta.ActionType = TradeActionType.ExitOCO;
				ta.ActionStatus = TradeActionStatus.New;
				//AddTradeAction(CurrentBar, ta);
				IndicatorProxy.PrintLog(true, IsLiveTrading(),
					String.Format("{0}:SetExitTradeAction ta==null StopLossPrice={1}, ProfitTargetPrice={2}",
					CurrentBar, CurrentTrade.TradeAction.StopLossPrice, CurrentTrade.TradeAction.ProfitTargetPrice));
			} else if(ta.IsExitAction()) { //check if the SL/PT needs to be changed or not
				//ta.ActionType = TradeActionType.ExitOCO;
				IndicatorProxy.PrintLog(true, IsLiveTrading(),
					String.Format("{0}:SetExitTradeAction ta!=null StopLossPrice={1}, ProfitTargetPrice={2}",
					CurrentBar, ta.StopLossPrice, ta.ProfitTargetPrice));
				switch(ta.ActionStatus) {
					case TradeActionStatus.New: //
						break;
					case TradeActionStatus.Updated:
						break;
					case TradeActionStatus.Executed:
						break;
					case TradeActionStatus.UnKnown:
						break;						
				}
			}
			
//			SetStopLossSignal(ta);
//			SetProfitTargetSignal(ta);			
//			CurrentTrade.TradeAction = ta;
			
			IndicatorProxy.PrintLog(true, IsLiveTrading(),
				String.Format("{0}:SetExitTradeAction after set ta, StopLossPrice={1}, ProfitTargetPrice={2}",
				CurrentBar, ta.StopLossPrice, ta.ProfitTargetPrice));
			return true;
		}
		
		public override bool NewTradeAllowed() {
			IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				String.Format("{0}: NewOrderAllowed called in StgProdTRT...CurrentTrade.PosQuantity={1}, HasPosition={2}, TM_MaxOpenPosition={3}",
			CurrentBar, CurrentTrade.PosQuantity, HasPosition(), TM_MaxOpenPosition));
			if(CurrentTrade.PosQuantity < TM_MaxOpenPosition)
				return true;
			else 
				return false;
		}		
		#endregion
		
		#region Indicator Functions
		public override Direction GetDirection(GIndicatorBase indicator) {
			
//			IndicatorSignal lnSig = indicator.GetLastIndicatorSignalByName(CurrentBar, giPbSAR.SignalName_Long);
//			IndicatorSignal stSig = indicator.GetLastIndicatorSignalByName(CurrentBar, giPbSAR.SignalName_Short);
//			IndicatorSignal revlnSig = indicator.GetLastIndicatorSignalByName(CurrentBar, giPbSAR.SignalName_RevLong);
//			IndicatorSignal revstSig = indicator.GetLastIndicatorSignalByName(CurrentBar, giPbSAR.SignalName_RevShort);
			
//			int lnSigBarNo = lnSig == null? -1:lnSig.BarNo;
//			int stSigBarNo = stSig == null? -1:stSig.BarNo;
//			int revlnSigBarNo = revlnSig == null? -1:revlnSig.BarNo;
//			int revstSigBarNo = revstSig == null? -1:revstSig.BarNo;
			
			Direction dir = new Direction(); //indicator.GetDirection();
			dir.TrendDir = TrendDirection.UnKnown;
			if(CurrentBar > this.ctxTRT.BarsLookback && CurrentBar > this.ctxTRT.MALength) {
				if(Close[ctxTRT.BarsLookback] < giEMA[0])
					dir.TrendDir = TrendDirection.Up;
				if(Close[ctxTRT.BarsLookback] > giEMA[0])
					dir.TrendDir = TrendDirection.Down;
			}
//			Print(CurrentBar + ":Dir=" + dir.TrendDir.ToString() + ",LastLn=" + lnSigBarNo + ", LastSt=" + stSigBarNo + ", LastRevLn=" + revlnSigBarNo + ", LastRevSt=" + revstSigBarNo);

//			if(revlnSigBarNo == CurrentBar || revstSigBarNo == CurrentBar)
//				dir.TrendDir = TrendDirection.UnKnown;
//			else if (stSigBarNo == CurrentBar)
//				dir.TrendDir = TrendDirection.Down;
//			else if (lnSigBarNo == CurrentBar)
//				dir.TrendDir = TrendDirection.Up;
			return dir;
		}
		
		private Direction GetDirectionVwap() {
			Direction dir = new Direction();
			dir.TrendDir = TrendDirection.UnKnown;
			if(CurrentBar > this.ctxTRT.BarsLookback && CurrentBar > this.ctxTRT.MALength
				&& giSnR.TodayOpen[0] > 0 && giVwap[0] > 0) 
			{				
				if(giVwap[0] > giSnR.TodayOpen[0]) dir.TrendDir = TrendDirection.Up;
				else if(giVwap[0] < giSnR.TodayOpen[0]) dir.TrendDir = TrendDirection.Down;				
			}
			return dir;
		}
		
		public override double GetStopLossOffset(SupportResistanceType srt, double price) {
			double offset = 0;
			offset = Math.Max(offset, giSnR.GetLastDayHLOffset(srt, price));
			offset = Math.Max(offset, giHLnBars.GetNBarsHLOffset(srt, price));
			offset = Math.Max(offset, giVwap.GetVwapOpenDOffset(srt, price));
			offset = Math.Max(offset, giEMA.GetEmaOffset(srt, price));

//			if(indSig != null)
//				infBarNo = indSig.BarNo;
//			if(infBarNo > 0) {
//				if (srt == SupportResistanceType.Resistance)
//					prcInfl = High[CurrentBar-infBarNo];
//				else if (srt == SupportResistanceType.Support)
//					prcInfl = Low[CurrentBar-infBarNo];
//			}
			
//			prc = GetValidStopLossPrice(new List<double>{prcLH, prcInfl}, MM_SLPriceGapPref);
//			if(prc <= 0)
//				prc = GetValidStopLossPrice(Close[0]);
			IndicatorProxy.PrintLog(true, IsLiveTrading(),
				String.Format("{0}: GetStopLossOffset={1}", CurrentBar, offset));
			return offset;
		}
		
		public override double GetProfitTargetOffset(SupportResistanceType srt, double price) {
			double offset = 0;
			offset = Math.Max(offset, giSnR.GetLastDayHLOffset(srt, price));
			offset = Math.Max(offset, giHLnBars.GetNBarsHLOffset(srt, price));
//			offset = Math.Max(offset, giVwap.GetVwapOpenDOffset(srt, price));
//			offset = Math.Max(offset, giEMA.GetEmaOffset(srt, price));

			IndicatorProxy.PrintLog(true, IsLiveTrading(),
				String.Format("{0}: GetProfitTargetOffset={1}, ProfitFactorMin={2}, ProfitFactorMax={3}", CurrentBar, offset, this.MM_ProfitFactorMin, this.MM_ProfitFactorMax));
			return offset;
		}
		
//		public override double GetProfitTargetPrice(SupportResistanceType srt) {
//			double prc = 0, prcLH = 0, prcInfl = 0;
//			int infBarNo = -1;
//			IndicatorSignal indSig = null;
//			switch(srt) {
//				case SupportResistanceType.Support:
//					indSig = giSMI.GetLastIndicatorSignalByActionType(CurrentBar-1, SignalActionType.InflectionDn);
//					prcLH = IndicatorProxy.GetLowestPrice(Bars_Lookback, true);
//					break;
//				case SupportResistanceType.Resistance:
//					indSig = giSMI.GetLastIndicatorSignalByActionType(CurrentBar-1, SignalActionType.InflectionUp);
//					prcLH = IndicatorProxy.GetHighestPrice(Bars_Lookback, true);
//					break;
//			}

//			if(indSig != null)
//				infBarNo = indSig.BarNo;

//			if(infBarNo > 0) {
//				if (srt == SupportResistanceType.Resistance)
//					prcInfl = High[CurrentBar-infBarNo];
//				else if (srt == SupportResistanceType.Support)
//					prcInfl = Low[CurrentBar-infBarNo];
//			}
			
//			prc = GetValidProfitTargetPrice(new List<double>{prcLH, prcInfl}, MM_PTPriceGapPref);
//			if(prc <= 0)
//				prc = GetValidProfitTargetPrice(Close[0]);
//			IndicatorProxy.PrintLog(true, IsLiveTrading(),
//				String.Format("{0}: GetProfitTargetPrice={1}, infBarNo={2}", CurrentBar, prc, infBarNo));
//			return prc;
//		}
		
		/// <summary>
		/// Setup the S1, R1 with Highest high or Lowest low of the last n bars
		/// </summary>
		private void GetHiLoNPrice() {
			if(IndicatorProxy.IsStartTimeBar(this.ctxTRT.TimeStart, ToTime(Time[0])/100, ToTime(Time[1])/100)) {
				this.ctxTRT.S1 = IndicatorProxy.GetLowestPrice(ctxTRT.BarsLookback, false);
				this.ctxTRT.R1 = IndicatorProxy.GetHighestPrice(ctxTRT.BarsLookback, false);
				IndicatorProxy.PrintLog(true, false, 
					string.Format("{0}: is time, S1={1}, R1={2}", CurrentBar, ctxTRT.S1, ctxTRT.R1));
			}
		}
		
		#endregion
		
		#region MarketContext Functions		
		public override void GetMarketContext() {
			ReadCtxTRT();
		}
		
		/// <summary>
		/// Load ctx from Json file
		/// </summary>
		/// <returns></returns>
		public void ReadCtxTRT() {
			ReadRestfulJson();
			List<JsonStgTRT> paraDict = GConfig.LoadJson2Obj<List<JsonStgTRT>>(GetCTXFilePath());
			Print(String.Format("ReadCtxTRT paraDict={0}, paraDict.Count={1}", paraDict, paraDict.Count));
			if(paraDict != null && paraDict.Count > 0) {
				this.ctxTRT = paraDict[0];
				GUtils.DisplayProperties<JsonStgTRT>(ctxTRT, IndicatorProxy);
			}
			foreach(JsonStgTRT ele in paraDict) {
				//Print(String.Format("DateCtx.ele.Key={0}, ele.Value.ToString()={1}", ele.Symbol, ele.Date));
//				if(ele != null && ele.Date != null && ele.TimeCtxs != null) {
//					Print(String.Format("DateCtx.ele.Key={0}, ele.Value.ToString()={1}", ele.Date, ele.TimeCtxs));
//					foreach(TimeCtx tctx in ele.TimeCtxs) {
//						Print(String.Format("ele.Date={0}, TimeCtx.tctx.Time={1}, tctx.ChannelType={2}, tctx.MinUp={3}, tctx.Support={4}",
//						ele.Date, tctx.Time, tctx.ChannelType, tctx.MinUp, tctx.Support));
//					}
//				}				
			}
//			foreach(KeyValuePair<string, List<TimeCTX>> ele in paraDict.cmdMarketContext.ctx_daily.ctx) {
//				//paraMap.Add(ele.Key, ele.Value.ToString());
//				Print(String.Format("ele.Key={0}, ele.Value.ToString()=", ele.Key));
//			}
		}
		#endregion

		#region Indicator Event Handler
        // Define what actions to take when the event is raised.
        void OnStopLossEvent(object sender, IndicatorEventArgs e)
        {
			IndicatorSignal isig = e.IndSignal;
            Print(String.Format("{0}: {1} sent this message: {2}, B#={3}, signame={4}, bkdir={5}", 
			CurrentBar, sender.GetType().Name, 
			e.Message, isig.BarNo, isig.SignalName, isig.BreakoutDir.ToString()));
			
			TradeSignal tsig = new TradeSignal();
			if(GetMarketPosition() == MarketPosition.Short && isig.BreakoutDir==BreakoutDirection.Up) {
				tsig.Action = OrderAction.Buy;
				//ptSig.LimitPrice = GetProfitTargetPrice(SupportResistanceType.Resistance);
			}
			else if(GetMarketPosition() == MarketPosition.Long && isig.BreakoutDir==BreakoutDirection.Down) {
				tsig.Action = OrderAction.Sell;
				//ptSig.LimitPrice = GetProfitTargetPrice(SupportResistanceType.Support);
			} else
				return;
			
			tsig.BarNo = CurrentBar;			
			tsig.SignalType = TradeSignalType.StopLoss;
			tsig.Order_Type = OrderType.Market;
			tsig.SignalSource = TradeSignalSource.Indicator;
			tsig.OrderCalculationMode = CalculationMode.Price;
			tsig.Quantity = CurrentTrade.PosQuantity;

			if(HasPosition() > 0 && (State != State.Historical || CurrentBar >= Bars.Count/2)) {
				//Print(String.Format("{0}: Alert Stop Loss, HasPosition={1}", CurrentBar, HasPosition()));
				AlertTradeSignal(tsig, "Stop loss alert!");
			}
			
			TradeAction ta = new TradeAction();
			ta.BarNo = CurrentBar;
			ta.ActionName = sender.GetType().Name + "-Stoploss";
			ta.ActionType = TradeActionType.ExitSimple;
			ta.ActionStatus = TradeActionStatus.New;
			ta.StopLossSignal = tsig;
			CurrentTrade.TradeAction = ta;
        }

		void OnProfitTargetEvent(object sender, IndicatorEventArgs e)
        {
			IndicatorSignal isig = e.IndSignal;
            Print(String.Format("{0}: {1} sent this message: {2}, B#={3}, signame={4}, bkdir={5}", 
			CurrentBar, sender.GetType().Name, 
			e.Message, isig.BarNo, isig.SignalName, isig.BreakoutDir.ToString()));
			
			TradeSignal tsig = new TradeSignal();
			if(GetMarketPosition() == MarketPosition.Short && isig.BreakoutDir==BreakoutDirection.Down) {
				tsig.Action = OrderAction.Buy;
				//ptSig.LimitPrice = GetProfitTargetPrice(SupportResistanceType.Resistance);
			}
			else if(GetMarketPosition() == MarketPosition.Long && isig.BreakoutDir==BreakoutDirection.Up) {
				tsig.Action = OrderAction.Sell;
				//ptSig.LimitPrice = GetProfitTargetPrice(SupportResistanceType.Support);
			} else
				return;
			
			tsig.BarNo = CurrentBar;			
			tsig.SignalType = TradeSignalType.ProfitTarget;
			tsig.Order_Type = OrderType.Market;
			tsig.SignalSource = TradeSignalSource.Indicator;
			tsig.OrderCalculationMode = CalculationMode.Price;
			tsig.Quantity = CurrentTrade.PosQuantity;

			if(HasPosition() > 0 && (State != State.Historical || CurrentBar >= Bars.Count/1.5)) {
				//Print(String.Format("{0}: Alert Stop Loss, HasPosition={1}", CurrentBar, HasPosition()));
				AlertTradeSignal(tsig, sender.GetType().Name + ": Profit Target alert!");
			}
			
			TradeAction ta = new TradeAction();
			ta.BarNo = CurrentBar;
			ta.ActionName = sender.GetType().Name + "-ProfitTarget";
			ta.ActionType = TradeActionType.ExitSimple;
			ta.ActionStatus = TradeActionStatus.New;
			ta.StopLossSignal = tsig;
			CurrentTrade.TradeAction = ta;
			
		}
		#endregion
		
        #region Custom Properties
		private const int ODG_EnBarsBeforeInflection = 1;
		private const int ODG_BarsLookback = 2;
		private const int ODG_EMAPeriod1 = 3;
		private const int ODG_EMAPeriod2 = 4;
		private const int ODG_Range = 5;
		private const int ODG_SMITMAPeriod = 6;
		private const int ODG_SMICrossLevel = 7;
		private const int ODG_SMITmaUp = 8;
		private const int ODG_SMITmaLow = 9;
		private const int ODG_FastPeriod = 10;
		private const int ODG_SlowPeriod = 11;
		private const int ODG_Smooth = 12;
		private const int ODG_FastKAMA = 13;
		private const int ODG_SlowKAMA = 14;
		private const int ODG_PeriodKAMA = 15;
		private const int ODG_AccPbSAR = 16;
		private const int ODG_AccMaxPbSAR = 17;
		private const int ODG_AccStepPbSAR = 18;
		private const int ODG_StoplossTicksEMA = 19;
		
		[Description("Strategy Client Version")]
		[Browsable(false), XmlIgnore]
        public string StgClientVersion
        {
            get { return "1.0"; }
        }
		
        [Description("Bars count before inflection for entry")]
 		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EnBarsBeforeInflection", GroupName = GPS_CUSTOM_PARAMS, Order = ODG_EnBarsBeforeInflection)]
        public int EnBarsBeforeInflection
        {
            get { return cp_EnBarsBeforeInflection; }
            set { cp_EnBarsBeforeInflection = Math.Max(1, value); }
        }
		
		[Description("Bars lookback period")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarsLookback", GroupName = GPS_CUSTOM_PARAMS, Order = ODG_BarsLookback)]
        public int Bars_Lookback
        {
            get { return barsLookback;}
            set { barsLookback = Math.Max(1, value);}
        }

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="EMAPeriod1(SMI)", Description="1st ema smothing period. ( R )", Order=ODG_EMAPeriod1, GroupName=GPS_CUSTOM_PARAMS)]
		public int EMAPeriod1
		{
			get { return emaperiod1;}
			set { emaperiod1 = Math.Max(1, value);}
		}

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="EMAPeriod2(SMI)", Description="2nd ema smoothing period. ( S )", Order=ODG_EMAPeriod2, GroupName=GPS_CUSTOM_PARAMS)]
		public int EMAPeriod2
		{
			get { return emaperiod2;}
			set { emaperiod2 = Math.Max(1, value);}
		}
		
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Range(SMI)", Description="Range for momentum Calculation ( Q )", Order=ODG_Range, GroupName=GPS_CUSTOM_PARAMS)]
		public int Range
		{
			get { return range;}
			set { range = Math.Max(1, value);}
		}		

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="SMITMAPeriod", Description="SMI TMA smoothing period", Order=ODG_SMITMAPeriod, GroupName=GPS_CUSTOM_PARAMS)]
		public int SMITMAPeriod
		{
			get { return smitmaperiod;}
			set { smitmaperiod = Math.Max(1, value);}
		}

		[Range(0, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="SMICrossLevel", Description="SMI&TMA Cross Level", Order=ODG_SMICrossLevel, GroupName=GPS_CUSTOM_PARAMS)]
		public int SMICrossLevel
		{
			get { return smiCrossLevel;}
			set { smiCrossLevel = Math.Max(0, value);}
		}
		
		[NinjaScriptProperty]
		[Display(Name="SMITmaUp", Description="SMITMA Up Bound", Order=ODG_SMITmaUp, GroupName=GPS_CUSTOM_PARAMS)]
		public int SMITmaUp
		{
			get { return smiTmaUp;}
			set { smiTmaUp = value;}
		}

		[NinjaScriptProperty]
		[Display(Name="SMITmaLow", Description="SMITMA Low Bound", Order=ODG_SMITmaLow, GroupName=GPS_CUSTOM_PARAMS)]
		public int SMITmaLow
		{
			get { return smiTmaLow;}
			set { smiTmaLow = value;}
		}
		
		/// <summary>
		/// </summary>
		//[Description("Period for fast EMA")]
		//[Category("Parameters")]
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "FastPeriod(AO)", GroupName = GPS_CUSTOM_PARAMS, Order = ODG_FastPeriod)]		
		public int FastPeriod
		{
			//get;set;
			get { return fastPeriod; }
			set { fastPeriod = Math.Max(1, value); }
		}

		/// <summary>
		/// </summary>
		//[Description("Period for slow EMA")]
		//[Category("Parameters")]
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SlowPeriod(AO)", GroupName = GPS_CUSTOM_PARAMS, Order = ODG_SlowPeriod)]
		public int SlowPeriod
		{
			get { return slowPeriod; }
			set { slowPeriod = Math.Max(1, value); }
		}

		/// <summary>
		/// </summary>
//		[Description("Period for Smoothing of Signal Line")]
//		[Category("Parameters")]
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Smooth(AO)", GroupName = GPS_CUSTOM_PARAMS, Order = ODG_Smooth)]		
		public int Smooth
		{
			get { return smooth; }
			set { smooth = Math.Max(1, value); }
		}

		[Range(1, 125), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast(KAMA)", GroupName = GPS_CUSTOM_PARAMS, Order = ODG_FastKAMA)]
		public int FastKAMA
		{ 
			get {return fastKAMA;}
			set {fastKAMA = Math.Max(1, value);}
		}

		[Range(5, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period(KAMA)", GroupName = GPS_CUSTOM_PARAMS, Order = ODG_PeriodKAMA)]
		public int PeriodKAMA
		{ 
			get {return periodKAMA;}
			set {periodKAMA = Math.Max(5, value);}
		}

		[Range(1, 125), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Slow(KAMA)", GroupName = GPS_CUSTOM_PARAMS, Order = ODG_SlowKAMA)]
		public int SlowKAMA
		{ 
			get {return slowKAMA;}
			set {slowKAMA = Math.Max(1, value);}
		}

		[Range(0, 1), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Accelerate(PbSAR)", GroupName = GPS_CUSTOM_PARAMS, Order = ODG_AccPbSAR)]
		public double AccPbSAR
		{ 
			get {return accPbSAR;}
			set {accPbSAR = value;}
		}
		
		[Range(0, 1), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "AccelerateMax(PbSAR)", GroupName = GPS_CUSTOM_PARAMS, Order = ODG_AccMaxPbSAR)]
		public double AccMaxPbSAR
		{ 
			get {return accMaxPbSAR;}
			set {accMaxPbSAR = value;}
		}
		
		[Range(0, 1), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "AccelerateStep(PbSAR)", GroupName = GPS_CUSTOM_PARAMS, Order = ODG_AccStepPbSAR)]
		public double AccStepPbSAR
		{ 
			get {return accStepPbSAR;}
			set {accStepPbSAR = value;}
		}
		
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="StoplossTicksEMA", Description="Tick offset amount to stop out by EMA", Order=ODG_StoplossTicksEMA, GroupName=GPS_CUSTOM_PARAMS)]
		public int StoplossTicksEMA
		{
			get { return stoplossTicksEMA;}
			set { stoplossTicksEMA = Math.Max(0, value);}
		}
		
		private int cp_EnBarsBeforeInflection = 2;
				
		private int barsLookback = 15;
		private int stoplossTicksEMA = 20;
		
		//SMI parameters
		private int	range			= 8;
		private int	emaperiod1		= 8;
		private int	emaperiod2		= 8;
		private int smitmaperiod	= 12;
		private int tmaperiod		= 6;
		private int smiCrossLevel	= 50;
		private int smiTmaUp		= 80; //or -80
		private int smiTmaLow		= 45; //or -45
		
		//AWO parameters
		private int fastPeriod 			= 5;
        private int slowPeriod 			= 34;
		private int smooth		 		= 5;
		
		//KAMA parameters
		private int fastKAMA 			= 2;
        private int slowKAMA 			= 10;
		private int periodKAMA	 		= 30;
		
		//PbSAR parameters
		private double accPbSAR			= 0.001;
		private double accMaxPbSAR		= 0.2;
		private double accStepPbSAR		= 0.0015;

		#endregion
	}
	
	public class JsonStgTRT
    {
		public string Symbol{get;set;}
		public string ChartType{get;set;}
		public string Version{get;set;}
		public string SessionId{get;set;}
		public string Date{get;set;}
		public int TimeOpen{get;set;}
		public int TimeClose{get;set;}
		public int TimeStart{get;set;}
		public int TimeEnd{get;set;}
		
		public string ChannelType{get;set;}
		public string TrendDirection{get;set;}
		public string TradingStyle{get;set;}
		public string TradingDirection{get;set;}
		
		public int BarsLookback{get;set;}
		public int DaysLookback{get;set;}
		public int MALength{get;set;}
		
		public int EnTicOffset{get;set;}
		public int ExTrailTics{get;set;}
		public int StoplossTics{get;set;}
		
		public double S1{get;set;}
		public double R1{get;set;}
		public double S2{get;set;}
		public double R2{get;set;}
		public double S3{get;set;}
		public double R3{get;set;}
		public double S4{get;set;}
		public double R4{get;set;}
		public double S5{get;set;}
		public double R5{get;set;}
		
		public double T1{get;set;}
		public double T2{get;set;}

    }
}
