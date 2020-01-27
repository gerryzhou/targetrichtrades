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
	/// Sample strategy fro GSZTrader:
	/// 1) OnStateChange();
	/// 2) OnBarUpdate();
	/// 3) GetIndicatorSignal();
	/// 4) GetTradeSignal();
	/// 5) CheckExitTrade() or CheckNewEntryTrade();
	/// 6) PutTrade();
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
	/// 
	/// </summary>
	public class StgSampleTRT : GStrategyBase
	{
		#region Variables
		private GISMI giSMI;
		private GIAwesomeOscillator awOscillator;
		private GIKAMA giKAMA;
		private GIPbSAR giPbSAR;
		private int barNo_EnInflection = -1;
		
		private double c0 = 0, hi3 = Double.MaxValue, lo3 = Double.MinValue;
		
		#endregion
		
		#region Init Functions
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				Print(this.Name + " SetDefaults called....");
				Description									= @"The sample strategy for GSZTrader.";
				Name										= "StgSampleTRT";
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
				Print(this.Name + " Set DataLoaded called....");				
				AddChartIndicator(IndicatorProxy);
				SetPrintOut(1);
				IndicatorProxy.LoadSpvPRList(SpvDailyPatternES.spvPRDayES);
				IndicatorProxy.AddPriceActionTypeAllowed(PriceActionType.DnWide);
				
				giSMI = GISMI(EMAPeriod1, EMAPeriod2, Range, SMITMAPeriod, SMICrossLevel);//(3, 5, 5, 8);
				awOscillator = GIAwesomeOscillator(FastPeriod, SlowPeriod, Smooth, MovingAvgType.SMA, false);//(5, 34, 5, MovingAvgType.SMA);
				giKAMA = GIKAMA(FastKAMA, PeriodKAMA, SlowKAMA);
				giPbSAR = GIPbSAR(AccPbSAR, AccMaxPbSAR, AccStepPbSAR);
				
				AddChartIndicator(giSMI);
				AddChartIndicator(awOscillator);
				AddChartIndicator(giKAMA);
				AddChartIndicator(giPbSAR);
				Print("GISMI called:" + "EMAPeriod1=" + EMAPeriod1 + "EMAPeriod2=" + EMAPeriod2 + "Range=" + Range + "SMITMAPeriod=" + SMITMAPeriod);
			}
			else if (State == State.Configure)
			{
				Print(this.Name + " Set Configure called.... CurrentTrade=" + CurrentTrade);
			}
		}

		/// <summary>
		/// Call the base as default implementation
		/// </summary>
		protected override void OnBarUpdate()
		{
			try {
				base.OnBarUpdate();
				IndicatorProxy.TraceMessage(this.Name, PrintOut);
				Print(String.Format("{0}: Stg={1}, GSZTrader={2}", CurrentBar, CurrentTrade.InstStrategy, IndicatorProxy.GSZTrader));
			} catch (Exception ex) {
				IndicatorProxy.Log2Disk = true;
				IndicatorProxy.PrintLog(true, true, "Exception: " + ex.StackTrace);
			}
		}
		#endregion

		#region Signal Functions
		public override bool CheckNewEntrySignals(){
			giSMI.Update();
			giPbSAR.Update();
			Print(CurrentBar + ":CheckNewEntrySignals called -----------" + giSMI.LastInflection);

			if(NewTradeAllowed())
				IndicatorProxy.PrintLog(true, IsLiveTrading(), String.Format("{0}: CheckNewEntrySignals called....NewOrderAllowed", CurrentBar));
			else {
				IndicatorProxy.PrintLog(true, IsLiveTrading(), String.Format("{0}: CheckNewEntrySignals called....NewOrderNotAllowed", CurrentBar));
				return false;
			}
			
			IndicatorSignal indSig = giSMI.GetLastIndicatorSignalByName(CurrentBar, giSMI.SignalName_Inflection);
			
			if(indSig != null && indSig.SignalAction != null)
				Print(CurrentBar + ":stg-Last " + giSMI.SignalName_Inflection + "=" + indSig.BarNo + "," + indSig.SignalAction.SignalActionType.ToString());

			IndicatorSignal indSigCrs = giSMI.GetLastIndicatorSignalByName(CurrentBar, giSMI.SignalName_LineCross);
			
			if(indSigCrs != null && indSigCrs.SignalAction != null)
				Print(CurrentBar + ":stg-Last " + giSMI.SignalName_LineCross + "=" + indSigCrs.BarNo + "," + indSigCrs.SignalAction.SignalActionType.ToString());
			
			if(PatternMatched()) {
				Direction dir = GetDirection(giPbSAR);
				TradeSignal enSig = new TradeSignal();			
				enSig.BarNo = CurrentBar;
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
				Print(CurrentBar + ":PatternMatched-- " + enSig.Action.ToString() + "," + enSig.SignalSource.ToString());
				return true;
			} else
				return false;
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
			giPbSAR.Update();
			Direction dir = GetDirection(giPbSAR);			
			TradeSignal slSig = new TradeSignal();
			slSig.BarNo = CurrentBar;
			slSig.SignalType = TradeSignalType.StopLoss;
			slSig.Order_Type = OrderType.Market;
			slSig.SignalSource = TradeSignalSource.Indicator;
			slSig.OrderCalculationMode = CalculationMode.Price;
			slSig.Quantity = 1;
			if(dir.TrendDir==TrendDirection.Up) {
				slSig.Action = OrderAction.Sell;
				slSig.StopPrice = GetStopLossPrice(SupportResistanceType.Support);
			}
			else if(dir.TrendDir==TrendDirection.Down) {
				slSig.Action = OrderAction.Buy;
				slSig.StopPrice = GetStopLossPrice(SupportResistanceType.Resistance);
			}
			IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				String.Format("{0}:CheckStopLossSignal slSig.StopPrice={1}", CurrentBar, slSig.StopPrice));
			
			AddTradeSignal(CurrentBar, IndicatorTradeSignals, slSig);
			return true;
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

			giPbSAR.Update();
			Direction dir = GetDirection(giPbSAR);
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
		
		/// ====Unused====
		//public override bool CheckTradeSignals() {
		public bool CheckTradeSignals1() {
			IndicatorProxy.TraceMessage(this.Name, PrintOut);
			List<TradeSignal> sigList = new List<TradeSignal>();
			TradeSignal trdSignal = new TradeSignal();
			Direction dir = giKAMA.GetDirection();// new Direction();
			PatternMatched();
			c0 = Close[0];
			
//			Print(CurrentBar + ":"
//			+ ";c0=" + c0
//			+ ";hi3=" + hi3
//			+ ";lo3=" + lo3
//			+ ";BarsLookback=" + BarsLookback);
			
//			if(c0 > hi3)
//				dir.TrendDir = TrendDirection.Up;

//			if(c0 < lo3)
//				dir.TrendDir = TrendDirection.Down;
//			trdSignal.TrendDir = dir;
			
//			this.AddTradeSignal(CurrentBar, trdSignal);
//			hi3 = GetHighestPrice(BarsLookback, true);
//			lo3 = GetLowestPrice(BarsLookback, true);
			
			return false;
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
			//Print("CurrentBar, barsMaxLastCross, barsAgoMaxPbSAREn,=" + CurrentBar + "," + barsAgoMaxPbSAREn + "," + barsSinceLastCross);
//			if (giParabSAR.IsSpvAllowed4PAT(curBarPriceAction.paType) && barsSinceLastCross < barsAgoMaxPbSAREn) 
//				return true;
//			else return false;
			PriceAction pa = IndicatorProxy.GetPriceAction(Time[0]);
			IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":"
				+ ";ToShortDateString=" + Time[0].ToString()
				+ ";paType=" + pa.paType.ToString()
				+ ";maxDownTicks=" + pa.maxDownTicks
				);
			SignalActionType sat = giSMI.IsLastBarInflection();
			Direction dir = GetDirection(giPbSAR);

			if((sat == SignalActionType.InflectionDn && dir.TrendDir == TrendDirection.Up) && giSMI.IsPullBack(-SMITmaUp, -SMITmaLow) ||
				(sat == SignalActionType.InflectionUp && dir.TrendDir == TrendDirection.Down && giSMI.IsPullBack(SMITmaLow, SMITmaUp)))
				return true;
			else
				return false;
			//barsAgoMaxPbSAREn Bars Since PbSAR reversal. Enter the amount of the bars ago maximum for PbSAR entry allowed
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
			
			SetStopLossSignal(ta);
			SetProfitTargetSignal(ta);			
			CurrentTrade.TradeAction = ta;
			
			IndicatorProxy.PrintLog(true, IsLiveTrading(),
				String.Format("{0}:SetExitTradeAction after set ta, StopLossPrice={1}, ProfitTargetPrice={2}",
				CurrentBar, ta.StopLossPrice, ta.ProfitTargetPrice));
			return true;
		}
		
		// ====unused====
		public override bool CheckNewEntryTrade() {
			IndicatorProxy.PrintLog(true, IsLiveTrading(), "====CheckNewEntryTrade()===" + this.Name);
			IndicatorProxy.TraceMessage(this.Name, PrintOut);
			CurrentTrade.InitNewEntryTrade();
			SetTradeAction();
//			if(GetTradeSignal(CurrentBar) != null) {
//				if(GetTradeSignal(CurrentBar).TrendDir.TrendDir == TrendDirection.Down)
//				{
//					IndicatorProxy.TraceMessage(this.Name, PrintOut);
//					CurrentTrade.tradeDirection = TradingDirection.Down;
//				}
//				else if(GetTradeSignal(CurrentBar).TrendDir.TrendDir == TrendDirection.Up)
//				{
//					IndicatorProxy.TraceMessage(this.Name, PrintOut);
//					CurrentTrade.tradeDirection = TradingDirection.Up;
//				}
				
//				CurrentTrade.tradeStyle = TradingStyle.TrendFollowing;
				
//			} else {
//				CurrentTrade.CurrentTradeType = TradeType.NoTrade);
//			}
			return false;
		}
		
		public override bool NewTradeAllowed() {
			IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				String.Format("{0}: NewOrderAllowed called....CurrentTrade.PosQuantity={1}, HasPosition={2}, TM_MaxOpenPosition={3}",
			CurrentBar, CurrentTrade.PosQuantity, HasPosition(), TM_MaxOpenPosition));
			if(CurrentTrade.PosQuantity < TM_MaxOpenPosition)
				return true;
			else 
				return false;
		}		
		#endregion
		
		#region Indicator Functions
		public override Direction GetDirection(GIndicatorBase indicator) {
			
			IndicatorSignal lnSig = indicator.GetLastIndicatorSignalByName(CurrentBar, giPbSAR.SignalName_Long);
			IndicatorSignal stSig = indicator.GetLastIndicatorSignalByName(CurrentBar, giPbSAR.SignalName_Short);
			IndicatorSignal revlnSig = indicator.GetLastIndicatorSignalByName(CurrentBar, giPbSAR.SignalName_RevLong);
			IndicatorSignal revstSig = indicator.GetLastIndicatorSignalByName(CurrentBar, giPbSAR.SignalName_RevShort);
			
			int lnSigBarNo = lnSig == null? -1:lnSig.BarNo;
			int stSigBarNo = stSig == null? -1:stSig.BarNo;
			int revlnSigBarNo = revlnSig == null? -1:revlnSig.BarNo;
			int revstSigBarNo = revstSig == null? -1:revstSig.BarNo;
			Direction dir = indicator.GetDirection();
			
			Print(CurrentBar + ":Dir=" + dir.TrendDir.ToString() + ",LastLn=" + lnSigBarNo + ", LastSt=" + stSigBarNo + ", LastRevLn=" + revlnSigBarNo + ", LastRevSt=" + revstSigBarNo);

//			if(revlnSigBarNo == CurrentBar || revstSigBarNo == CurrentBar)
//				dir.TrendDir = TrendDirection.UnKnown;
//			else if (stSigBarNo == CurrentBar)
//				dir.TrendDir = TrendDirection.Down;
//			else if (lnSigBarNo == CurrentBar)
//				dir.TrendDir = TrendDirection.Up;
			return dir;
		}
		
		public override double GetStopLossPrice(SupportResistanceType srt) {
			double prc = 0, prcLH = 0, prcInfl = 0;
			int infBarNo = -1;
			IndicatorSignal indSig = null;
			switch(srt) {
				case SupportResistanceType.Support:
					prcLH = GetLowestPrice(Bars_Lookback, true);
					indSig = giSMI.GetLastIndicatorSignalByActionType(CurrentBar-1, SignalActionType.InflectionDn);
					break;
				case SupportResistanceType.Resistance:
					prcLH = GetHighestPrice(Bars_Lookback, true);
					indSig = giSMI.GetLastIndicatorSignalByActionType(CurrentBar-1, SignalActionType.InflectionUp);
					break;
			}
			if(indSig != null)
				infBarNo = indSig.BarNo;
			if(infBarNo > 0) {
				if (srt == SupportResistanceType.Resistance)
					prcInfl = High[CurrentBar-infBarNo];
				else if (srt == SupportResistanceType.Support)
					prcInfl = Low[CurrentBar-infBarNo];
			}
			
			prc = GetValidStopLossPrice(new List<double>{prcLH, prcInfl}, MM_SLPriceGapPref);
			if(prc <= 0)
				prc = GetValidStopLossPrice(Close[0]);
			IndicatorProxy.PrintLog(true, IsLiveTrading(),
				String.Format("{0}: GetStopLossPrice={1}, infBarNo={2}", CurrentBar, prc, infBarNo));
			return prc;
		}
		
		public override double GetProfitTargetPrice(SupportResistanceType srt) {
			double prc = 0, prcLH = 0, prcInfl = 0;
			int infBarNo = -1;
			IndicatorSignal indSig = null;
			switch(srt) {
				case SupportResistanceType.Support:
					indSig = giSMI.GetLastIndicatorSignalByActionType(CurrentBar-1, SignalActionType.InflectionDn);
					prcLH = GetLowestPrice(Bars_Lookback, true);
					break;
				case SupportResistanceType.Resistance:
					indSig = giSMI.GetLastIndicatorSignalByActionType(CurrentBar-1, SignalActionType.InflectionUp);
					prcLH = GetHighestPrice(Bars_Lookback, true);
					break;
			}

			if(indSig != null)
				infBarNo = indSig.BarNo;

			if(infBarNo > 0) {
				if (srt == SupportResistanceType.Resistance)
					prcInfl = High[CurrentBar-infBarNo];
				else if (srt == SupportResistanceType.Support)
					prcInfl = Low[CurrentBar-infBarNo];
			}
			
			prc = GetValidProfitTargetPrice(new List<double>{prcLH, prcInfl}, MM_PTPriceGapPref);
			if(prc <= 0)
				prc = GetValidProfitTargetPrice(Close[0]);
			IndicatorProxy.PrintLog(true, IsLiveTrading(),
				String.Format("{0}: GetProfitTargetPrice={1}, infBarNo={2}", CurrentBar, prc, infBarNo));
			return prc;
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
		
		private int cp_EnBarsBeforeInflection = 2;
				
		private int barsLookback = 15;
		
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
}
