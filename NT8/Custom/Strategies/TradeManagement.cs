#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using System.Reflection;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Strategies.ZTraderStg;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.AddOns;

#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class GStrategyBase : Strategy
	{
		//protected CurrentTrade CurrentTrade;
		
		#region Trade Mgmt Functions
		
		public virtual void InitTradeMgmt() {
//			if(CurrentTrade != null) {
//				SetProfitTarget(CalculationMode.Currency, CurrentTrade.profitTargetAmt);
//	            SetStopLoss(CalculationMode.Currency, CurrentTrade.stopLossAmt);
//			} else {
//				SetProfitTarget(CalculationMode.Currency, MM_ProfitTargetAmt);
//	            SetStopLoss(CalculationMode.Currency, MM_StopLossAmt);
//			}
		}
		
		public virtual void PutTrade() {
			indicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + 
				"::PutTrade()--" + this.ToString());
			switch(CurrentTrade.CurrentTradeType) {
				case TradeType.Entry:
					PutEntryTrade();
					break;
				case TradeType.Exit:
					PutExitTrade();
					break;
				case TradeType.Liquidate:
					PutLiquidateTrade();
					break;
			}
		}
		
		public virtual void PutEntryTrade() {
			if(CurrentTrade.TradeAction == null || 
				CurrentTrade.TradeAction.EntrySignal == null ||
				CurrentTrade.TradeAction.EntrySignal.BarNo != CurrentBar) return;
			
			switch(CurrentTrade.TradeAction.TradeActionType) {
				case TradeActionType.EntrySimple:
					NewEntrySimpleOrder();
					break;
			}
		}
		
		public virtual void PutExitTrade() {
			if(CurrentTrade.TradeAction == null) return;
			
			switch(CurrentTrade.TradeAction.TradeActionType) {
				case TradeActionType.ExitOCO:
					break;
				case TradeActionType.ExitTrailingSL:
					break;
				case TradeActionType.UnKnown:
					break;
			}
		}
		
		public virtual void PutLiquidateTrade() {
		}
		
		/// <summary>
		/// Check performance first, 
		/// then check signal to determine special exit: 
		/// liquidate, reverse, or scale in/out;
		/// </summary>
		/// <returns></returns>
		public virtual CurrentTrade CheckExitTrade() {
			int prtLevel = 1;
			//CurrentTrade.SetTradeType(TradeType.Exit);
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			ChangeSLPT();
			//if(Position.MarketPosition == MarketPosition.Flat) return null;
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			CheckExitTradeBySignal();
			
			return CurrentTrade;
		}
		
		/// <summary>
		/// Check exit trader from signal instead of by money management policy
		/// </summary>
		/// <returns></returns>
		public virtual void CheckExitTradeBySignal() {
			if(GetTradeSignal(CurrentBar) == null) return;
//			indicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":CheckExitTradeBySignal"
//			+ ";indicatorSignal.ReversalDir=" + GetTradeSignal(CurrentBar).ReversalDir.ToString()
//			+ ";Position.MarketPosition=" + GetMarketPosition()
//			);
//			if((GetTradeSignal(CurrentBar).ReversalDir == Reversal.Up && GetMarketPosition() == MarketPosition.Short) ||
//				(GetTradeSignal(CurrentBar).ReversalDir == Reversal.Down && GetMarketPosition() == MarketPosition.Long)) {
//				CurrentTrade.SetTradeType(TradeType.Liquidate);
//				CloseAllPositions();
//				CancelExitOrders();
//			} else {

//			}			
		}
		
		/// <summary>
		/// Check if new entry trade could be generated
		/// </summary>
		/// <returns></returns>
		public virtual CurrentTrade CheckNewEntryTrade() {
			indicatorProxy.PrintLog(true, IsLiveTrading(), 
				CurrentBar + "::=======Virtual CheckNewEntryTrade()===========" + this.ToString());
			return null;
		}

		public virtual bool CheckEnOrder(double cur_gap)
        {
            double min_en = -1;

            if (CurrentTrade.BracketOrder.EntryOrder != null && CurrentTrade.BracketOrder.EntryOrder.OrderState == OrderState.Working)
            {
                min_en = indicatorProxy.GetMinutesDiff(CurrentTrade.BracketOrder.EntryOrder.Time, Time[0]);// DateTime.Now);
                //if ( IsTwoBarReversal(cur_gap, TickSize, enCounterPBBars) || (barsHoldEnOrd > 0 && barsSinceEnOrd >= barsHoldEnOrd) || ( minutesChkEnOrder > 0 &&  min_en >= minutesChkEnOrder))
				if ( (CurrentTrade.barsHoldEnOrd > 0 && CurrentTrade.barsSinceEnOrd >= CurrentTrade.barsHoldEnOrd) || ( CurrentTrade.minutesChkEnOrder > 0 &&  min_en >= CurrentTrade.minutesChkEnOrder))	
                {
                    CancelOrder(CurrentTrade.BracketOrder.EntryOrder);
                    //giParabSAR.PrintLog(true, !backTest, log_file, "Order cancelled for " + AccName + ":" + barsSinceEnOrd + "/" + min_en + " bars/mins elapsed--" + BracketOrder.EntryOrder.ToString());
					return true;
                }
				else {
					//giParabSAR.PrintLog(true, !backTest, log_file, "Order working for " + AccName + ":" + barsSinceEnOrd + "/" + min_en + " bars/mins elapsed--" + BracketOrder.EntryOrder.ToString());
					CurrentTrade.barsSinceEnOrd++;
				}
            }
            return false;
        }
		
		public virtual double GetEnLongPrice() {
			double prc = -1;
			switch(CurrentTrade.BracketOrder.EnOrderType) {
				case OrderType.Limit:
					prc = (CurrentTrade.enTrailing && CurrentTrade.enCounterPBBars>0) ? Close[0]-CurrentTrade.enOffsetPnts :  Low[0]-CurrentTrade.enOffsetPnts;
					break;
				case OrderType.StopMarket:
					prc = CurrentTrade.TradeAction.EntryPrice;
					break;
				case OrderType.StopLimit:
					prc = CurrentTrade.TradeAction.EntryPrice;
					break;
			}
			return prc;
		}
		
		public virtual bool NewOrderAllowed()
		{
			if(BarsInProgress !=0) return false;
			
			SetPrintOut(-1);
			indicatorProxy.TraceMessage(this.Name, PrintOut);
			//Print(MethodBase.GetCurrentMethod().ToString() + " called");
			//indicatorProxy.TraceMessage(this.Name, prtLevel);
			int bsx = BarsSinceExitExecution(0, "", 0);
			int bse = BarsSinceEntryExecution(0, "", 0);		
			//Print(CurrentBar + ":" + this.Name + " NewOrderAllowed, BarsSinceExit, BarsSinceEntry=" + bsx + "," + bse);
			
			//indicatorProxy.TraceMessage(this.Name, prtLevel);
			double pnl = CheckAccPnL();//GetAccountValue(AccountItem.RealizedProfitLoss);
			
			//indicatorProxy.TraceMessage(this.Name, prtLevel);
			double plrt = CheckAccCumProfit();
			//DateTime dayKey = new DateTime(Time[0].Year,Time[0].Month,Time[0].Day);
			
			//indicatorProxy.TraceMessage(this.Name, prtLevel);
			double pnl_daily = CheckPnlByDay(Time[0].Year, Time[0].Month, Time[0].Day);
			
			
			//indicatorProxy.TraceMessage(this.Name, prtLevel);
			if(PrintOut > -1) {
				//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":(RealizedProfitLoss,RealtimeTrades.CumProfit)=(" + pnl + "," + plrt + ")--" + Get24HDateTime(Time[0]));	
				if(SystemPerformance.AllTrades.ByDay.Count == 2) {
					//giParabSAR.PrintLog(true, !backTest, log_file, "Performance.AllTrades.TradesPerformance.Currency.CumProfit is: " + Performance.AllTrades.TradesPerformance.Currency.CumProfit);
					//giParabSAR.PrintLog(true, !backTest, log_file, "Performance.AllTrades.ByDay[dayKey].TradesPerformance.Currency.CumProfit is: " + pnl);
				}
			}
			
			
			//indicatorProxy.TraceMessage(this.Name, prtLevel);
			if((BackTest && IsLiveTrading()) || (!BackTest && !IsLiveTrading())) {
				//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + "[backTest,Historical]=" + backTest + "," + Historical + "- NewOrderAllowed=false - " + Get24HDateTime(Time[0]));
				return false;
			}
			
			
			//indicatorProxy.TraceMessage(this.Name, prtLevel);
			if(IsLiveTrading() && (plrt <= CurrentTrade.dailyLossLmt || pnl_daily <= CurrentTrade.dailyLossLmt))
			{
				indicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + "-" + AccName 
				+ ": dailyLossLmt reached = " + pnl_daily + "," + plrt
				+ "::" + this.Name);
				
				return false;
			}
			
			
			//indicatorProxy.TraceMessage(this.Name, prtLevel);
			if (!IsLiveTrading() && pnl_daily <= CurrentTrade.dailyLossLmt) 
			{
				indicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + "-" + AccName 
				+ ": dailyLossLmt reached = " + pnl_daily + "," + plrt
				+ "::" + this.Name);
				
				return false;				
			}
		
			
			//indicatorProxy.TraceMessage(this.Name, prtLevel);
			if (IsTradingTime(170000) && HasPosition() == 0)
			{
				if (CurrentTrade.BracketOrder.EntryOrder == null || CurrentTrade.BracketOrder.EntryOrder.OrderState != OrderState.Working || CurrentTrade.enTrailing)
				{					
					if(bsx < 0 || bsx > CurrentTrade.barsSincePTSL) //if(bsx == -1 || bsx > CurrentTrade.barsSincePtSl)
					{
						indicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar 
						+ "-bsx,bse,CurrentTrade.barsSincePtSl=" + bsx + "," + bse + "," + CurrentTrade.barsSincePTSL
						+ "::" + this.Name);
						return true;
					} //else
						//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + "-NewOrderAllowed=false-[bsx,barsSincePtSl]" + bsx + "," + barsSincePtSl + " - " + Get24HDateTime(Time[0]));
				} //else
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + "-NewOrderAllowed=false-[BracketOrder.EntryOrder.OrderState,BracketOrder.EntryOrder.OrderType]" + BracketOrder.EntryOrder.OrderState + "," + BracketOrder.EntryOrder.OrderType + " - " + Get24HDateTime(Time[0]));
			}
			
			return false;
		}
	
		/// <summary>
		/// Submit unmanaged long limit order, set the order object in OnOrderUpdate handler
		/// </summary>
		/// <param name="msg"></param>
		public virtual void NewLongLimitOrderUM(string msg)
		{
			CurrentTrade.TradeAction.EntrySignal.SignalName = GetNewEnOrderSignalName(OrderSignalName.EntryLongLmt.ToString());
			indicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":NewLongLimitOrderUM"
			+";CurrentTrade.TradeAction.EntrySignal.SignalName=" + CurrentTrade.TradeAction.EntrySignal.SignalName);
			SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.Limit, 
			CurrentTrade.quantity, CurrentTrade.TradeAction.EntryPrice, 0, "", CurrentTrade.TradeAction.EntrySignal.SignalName);
			
			CurrentTrade.barsSinceEnOrd = 0;
		}
		/// <summary>
		/// Submit unmanaged short limit order, set the order object in OnOrderUpdate handler
		/// </summary>
		/// <param name="msg"></param>
		public virtual void NewShortLimitOrderUM(string msg)
		{
			CurrentTrade.TradeAction.EntrySignal.SignalName = GetNewEnOrderSignalName(OrderSignalName.EntryShortLmt.ToString());
			indicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":NewShortLimitOrderUM"
			+";CurrentTrade.TradeAction.EntrySignal.SignalName=" + CurrentTrade.TradeAction.EntrySignal.SignalName);
			SubmitOrderUnmanaged(0, OrderAction.SellShort, OrderType.Limit,
			CurrentTrade.quantity, CurrentTrade.TradeAction.EntryPrice, 0, "", CurrentTrade.TradeAction.EntrySignal.SignalName);
			
			//double prc = (CurrentTrade.enTrailing && CurrentTrade.enCounterPBBars>0) ? Close[0]+CurrentTrade.enOffsetPnts : High[0]+CurrentTrade.enOffsetPnts;
			
//			double curGap = giParabSAR.GetCurGap();
//			double todaySAR = giParabSAR.GetTodaySAR();		
//			double prevSAR = giParabSAR.GetPrevSAR();		
//			int reverseBar = giParabSAR.GetReverseBar();
//			int last_reverseBar = giParabSAR.GetLastReverseBar(CurrentBar);		
//			double reverseValue = giParabSAR.GetReverseValue();
//		
//			if(TG_PrintOut > 1) {
//				string logText = CurrentBar + "-" + AccName + 
//				":PutOrder-(curGap,todaySAR,prevSAR,zzGap,reverseBar,last_reverseBar,reverseValue)= " 
//				+ curGap + "," + todaySAR + "," + prevSAR + "," + zzGap + "," + reverseBar + "," + last_reverseBar + "," + reverseValue ;
//				giParabSAR.PrintLog(true, !backTest, log_file, logText);
//			}
			//enCounterPBBars
			if(CurrentTrade.BracketOrder.EntryOrder == null) {
//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg + ", EnterShortLimit called short price=" + prc + "--" + Get24HDateTime(Time[0]));			
			}
			else if (CurrentTrade.BracketOrder.EntryOrder.OrderState == OrderState.Working) {
//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterShortLimit updated short price (old, new)=(" + BracketOrder.EntryOrder.LimitPrice + "," + prc + ") -- " + Get24HDateTime(Time[0]));		
				//CancelOrder(CurrentTrade.BracketOrder.EntryOrder);
				//BracketOrder.EntryOrder = EnterShortLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			}
			//CurrentTrade.BracketOrder.EntryOrder = EnterShortLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			CurrentTrade.barsSinceEnOrd = 0;
		}
		
		public virtual void NewLongLimitOrder(string msg)//, double zzGap, double curGap)
		{
			double prc = (CurrentTrade.enTrailing && CurrentTrade.enCounterPBBars>0) ? Close[0]-CurrentTrade.enOffsetPnts :  Low[0]-CurrentTrade.enOffsetPnts;
			
			if(CurrentTrade.BracketOrder.EntryOrder == null) {
				CurrentTrade.BracketOrder.EntryOrder = EnterLongLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterLongLimit called buy price= " + prc + " -- " + Get24HDateTime(Time[0]));
			}
			else if (CurrentTrade.BracketOrder.EntryOrder.OrderState == OrderState.Working) {
//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterLongLimit updated buy price (old, new)=(" + BracketOrder.EntryOrder.LimitPrice + "," + prc + ") -- " + Get24HDateTime(Time[0]));
				CancelOrder(CurrentTrade.BracketOrder.EntryOrder);
				CurrentTrade.BracketOrder.EntryOrder = EnterLongLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			}
			CurrentTrade.barsSinceEnOrd = 0;
		}

		public virtual void NewEntryLongOrder()
		{
			double prc = GetEnLongPrice();//(MM_EnTrailing && CurrentTrade.enCounterPBBars>0) ? Close[0]-CurrentTrade.enOffsetPnts :  Low[0]-CurrentTrade.enOffsetPnts;
			
			if (CurrentTrade.BracketOrder.EntryOrder.OrderState == OrderState.Working) {
//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterLongLimit updated buy price (old, new)=(" + BracketOrder.EntryOrder.LimitPrice + "," + prc + ") -- " + Get24HDateTime(Time[0]));
				//CancelOrder(CurrentTrade.BracketOrder.EntryOrder);
				CancelEntryOrders();
				//CurrentTrade.BracketOrder.EntryOrder = EnterLongLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			}
			if(CurrentTrade.BracketOrder.EntryOrder == null) {
				if(CurrentTrade.BracketOrder.EnOrderType == OrderType.StopMarket) {
					CurrentTrade.BracketOrder.EntryOrder = EnterLongStopMarket(DefaultQuantity, prc, OrderSignalName.EntryLongStopMkt.ToString());
				}
				else if(CurrentTrade.BracketOrder.EnOrderType == OrderType.Limit) {
					CurrentTrade.BracketOrder.EntryOrder = EnterLongLimit(DefaultQuantity, prc, OrderSignalName.EntryLongLmt.ToString());
				}
//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterLongLimit called buy price= " + prc + " -- " + Get24HDateTime(Time[0]));
			}
			CurrentTrade.barsSinceEnOrd = 0;
		}
		
		#region Entry Order functions
		public virtual void NewEntrySimpleOrder() {
			if (CurrentTrade.BracketOrder.EntryOrder !=null &&
				CurrentTrade.BracketOrder.EntryOrder.OrderState == OrderState.Working) {
					CancelEntryOrders();
			}
			if(IsUnmanaged) {
				NewEntrySimpleOrderUM();
			} else {
				NewEntrySimpleOrderMG();
			}
			CurrentTrade.barsSinceEnOrd = 0;
		}
		
		public virtual void NewEntrySimpleOrderMG() {
		}
		
		public virtual void NewEntrySimpleOrderUM() {
			CurrentTrade.TradeAction.EntrySignal.SignalName = GetNewEnOrderSignalName(OrderSignalName.EntryLongLmt.ToString());
			indicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":NewLongLimitOrderUM"
			+";CurrentTrade.TradeAction.EntrySignal.SignalName=" + CurrentTrade.TradeAction.EntrySignal.SignalName);
			SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.Limit, 
			CurrentTrade.quantity, CurrentTrade.TradeAction.EntryPrice, 0, "", CurrentTrade.TradeAction.EntrySignal.SignalName);
		}
		
		#endregion Entry Order functions
		
		#region Exit Order functions
		public virtual void SetProfitTargetOrder(string sigName) {
			if(IsUnmanaged) {
				SetProfitTargetOrderUM();
				return;
			}
			indicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":SetProfitTargetOrder-" 
			+ sigName + "-" + CurrentTrade.PTCalculationMode 
			+ "-profitTargetAmt=" + CurrentTrade.profitTargetAmt
			+ "-profitTargetTic" + CurrentTrade.profitTargetTic
			+ "-profitTargetPrice" + CurrentTrade.TradeAction.ProfitTargetPrice + "-avg=" + GetAvgPrice());
			try{
			switch(CurrentTrade.PTCalculationMode) {
				case CalculationMode.Currency :
					SetProfitTarget(sigName, CalculationMode.Currency, CurrentTrade.profitTargetAmt);
					break;
				case CalculationMode.Price :
					SetProfitTarget(sigName, CalculationMode.Price, CurrentTrade.TradeAction.ProfitTargetPrice);
					break;
				case CalculationMode.Ticks :
					SetProfitTarget(sigName, CalculationMode.Ticks, CurrentTrade.profitTargetTic);
					break;
				default: 
					SetProfitTarget(sigName, CalculationMode.Currency, CurrentTrade.profitTargetAmt);
					break;
			}
			} catch(Exception ex) {
				throw new Exception("Ex SetProfitTarget:" + ex.Message);
			}
		}

		public virtual void SetProfitTargetOrderUM() {
			indicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":SetProfitTargetOrderUM"
			+ ";CurrentTrade.TradeAction.EntrySignal.SignalName=" + CurrentTrade.TradeAction.EntrySignal.SignalName
			+ ";CurrentTrade.ocoID=" + CurrentTrade.ocoID
			+ ";GetExitOrderAction()=" + GetExitOrderAction().ToString()
			+ ";profitTargetAmt=" + CurrentTrade.profitTargetAmt
			+ ";profitTargetTic=" + CurrentTrade.profitTargetTic
			+ ";profitTargetPrice=" + CurrentTrade.TradeAction.ProfitTargetPrice
			+ ";avgPrc=" + GetAvgPrice()
			+ ";Position.Quantity=" + HasPosition());
			Order ptOrder = CurrentTrade.BracketOrder.OCOOrder.ProfitTargetOrder;
			
			try{
				if(ptOrder == null || !ptOrder.Oco.Equals(CurrentTrade.ocoID)) {
					SubmitOrderUnmanaged(0, GetExitOrderAction(), OrderType.Limit, CurrentTrade.quantity,
					CurrentTrade.TradeAction.ProfitTargetPrice, 0, CurrentTrade.ocoID, CurrentTrade.TradeAction.ProfitTargetSignal.SignalName);
				}
				else if(ptOrder != null && ptOrder.Oco.Equals(CurrentTrade.ocoID)
					&& ptOrder.LimitPrice != CurrentTrade.TradeAction.ProfitTargetPrice) {
					ChangeOrder(ptOrder, ptOrder.Quantity, CurrentTrade.TradeAction.ProfitTargetPrice, 0);
				}
			} catch(Exception ex) {
				throw new Exception("Ex SetProfitTargetUM:" + ex.Message);
			}
		}
		
		public virtual void SetStopLossOrder(string sigName) {
			if(IsUnmanaged) {
				SetStopLossOrderUM();
				return;
			}
			indicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":SetStopLossOrder"
			+ ";CurrentTrade.TradeAction.EntrySignal.SignalName=" + CurrentTrade.TradeAction.EntrySignal.SignalName
			+ ";CurrentTrade.ocoID=" + CurrentTrade.ocoID
			+ ";CurrentTrade.SLCalculationMode=" + CurrentTrade.SLCalculationMode
			+ ";stopLossAmt=" + CurrentTrade.stopLossAmt
			+ ";stopLossTic=" + CurrentTrade.stopLossTic
			+ ";stopLossPrice=" + CurrentTrade.TradeAction.StopLossPrice
			+ ";avgPrc=" + GetAvgPrice());
			try {
			switch(CurrentTrade.SLCalculationMode) {
				case CalculationMode.Currency :
					SetStopLoss(sigName, CalculationMode.Currency, CurrentTrade.stopLossAmt, true);
					break;
				case CalculationMode.Price :
					SetStopLoss(sigName, CalculationMode.Price, CurrentTrade.TradeAction.StopLossPrice, true);
					break;
				case CalculationMode.Ticks :
					SetStopLoss(sigName, CalculationMode.Ticks, CurrentTrade.stopLossTic, true);
					break;
				default: 
					SetStopLoss(sigName, CalculationMode.Currency, CurrentTrade.stopLossAmt, true);
					break;
			}
			} catch(Exception ex) {
				throw new Exception("Ex SetStopLossOrder:" + ex.Message);
			}
		}
		
		public virtual void SetStopLossOrderUM() {
			indicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":SetStopLossOrderUM" 
			+ ";CurrentTrade.TradeAction.EntrySignal.SignalName=" + CurrentTrade.TradeAction.EntrySignal.SignalName
			+ ";CurrentTrade.ocoID=" + CurrentTrade.ocoID
			+ ";GetExitOrderAction()=" + GetExitOrderAction().ToString()			
			+ ";CurrentTrade.SLCalculationMode=" + CurrentTrade.SLCalculationMode
			+ ";stopLossAmt=" + CurrentTrade.stopLossAmt
			+ ";stopLossTic=" + CurrentTrade.stopLossTic
			+ ";stopLossPrice=" + CurrentTrade.TradeAction.StopLossPrice
			+ ";avgPrc=" + GetAvgPrice()
			+ ";Position.Quantity=" + HasPosition());
			
			Order slOrder = CurrentTrade.BracketOrder.OCOOrder.StopLossOrder;
			try{
				if(slOrder == null || !slOrder.Oco.Equals(CurrentTrade.ocoID)) {
					SubmitOrderUnmanaged(0, GetExitOrderAction(), OrderType.StopMarket, CurrentTrade.quantity,
					0, CurrentTrade.TradeAction.StopLossPrice, CurrentTrade.ocoID, CurrentTrade.TradeAction.StopLossSignal.SignalName);
				}
				else if(slOrder != null && slOrder.Oco.Equals(CurrentTrade.ocoID)
					&& slOrder.StopPrice != CurrentTrade.TradeAction.StopLossPrice) {
					ChangeOrder(slOrder, slOrder.Quantity, 0, CurrentTrade.TradeAction.StopLossPrice);
				}
			} catch(Exception ex) {
				throw new Exception("Ex SetStopLossOrderUM:" + ex.Message);
			}
		}

		/// <summary>
		/// Setup breakeven order
		/// </summary>
		public virtual void SetBreakEvenOrder(double avgPrc) {			
			Order stopOrder = CurrentTrade.BracketOrder.OCOOrder.StopLossOrder;
			string tif = stopOrder == null?
				"N/A":stopOrder.TimeInForce.ToString()+",IsLiveUntilCancelled="+stopOrder.IsLiveUntilCancelled;
			CurrentTrade.stopLossAmt = 0;
			CurrentTrade.TradeAction.StopLossPrice = avgPrc;
			
			if(IsUnmanaged) {
				indicatorProxy.PrintLog(true, IsLiveTrading(), 
					AccName + "-Setup SL BreakevenUM posAvgPrc=" + avgPrc + "," + tif);				
				SetBreakEvenOrderUM(avgPrc);
				return;
			}
			indicatorProxy.PrintLog(true, IsLiveTrading(), 
				AccName + "-Setup SL Breakeven posAvgPrc=" + avgPrc + "," + tif);
			SetStopLossOrder(CurrentTrade.TradeAction.EntrySignal.SignalName.ToString());
		}
		
		/// <summary>
		/// Setup breakeven order for unmanaged approach
		/// </summary>
		public virtual void SetBreakEvenOrderUM(double slPrc) {
			Order stopOrder = CurrentTrade.BracketOrder.OCOOrder.StopLossOrder;
			if (stopOrder != null && stopOrder.StopPrice != slPrc)
        		ChangeOrder(stopOrder, stopOrder.Quantity, 0, slPrc);
		}
		
		public virtual void SetTrailingStopLossOrder(string sigName) {
			CancelExitOCO();
			
			if(IsUnmanaged) {
				SetTrailingStopLossOrderUM();
				return;
			}
			
			indicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":SetTrailingStopLossOrder-" 
			+ sigName + "-" + CurrentTrade.TLSLCalculationMode 
			+ "-trailingSLAmt=" + CurrentTrade.trailingSLAmt
			+ "-trailingSLTic=" + CurrentTrade.trailingSLTic + "-avg=" + GetAvgPrice());
			try {
				switch(CurrentTrade.TLSLCalculationMode) {
					case CalculationMode.Ticks :
						SetTrailStop(sigName, CalculationMode.Ticks, CurrentTrade.trailingSLTic, true);
						break;
					case CalculationMode.Percent :
						SetTrailStop(sigName, CalculationMode.Percent, CurrentTrade.trailingSLAmt, true);
						break;
					default: 
						SetTrailStop(sigName, CalculationMode.Ticks, CurrentTrade.trailingSLTic, true);
						break;
				}
			} catch(Exception ex) {
				throw new Exception("Ex SetTrailingStopLossOrder:" + ex.Message);
			}
		}

		public virtual void SetTrailingStopLossOrderUM() {
			indicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":SetTrailingStopLossOrderUM"
			+ ";trailingSLSignalName=" + CurrentTrade.TradeAction.StopLossSignal.SignalName
			+ ";TLSLCalculationMode=" + CurrentTrade.TLSLCalculationMode
			+ ";trailingPTTic=" + CurrentTrade.TradeAction.TrailingProfitTargetTics
			+ ";trailingSLAmt=" + CurrentTrade.trailingSLAmt
			+ ";trailingSLTic=" + CurrentTrade.trailingSLTic 
			+ ";GetAvgPrice=" + GetAvgPrice()
			+ ";Position.Quantity=" + HasPosition());

			Order tlslOrder = CurrentTrade.TrailingSLOrder.TLSLOrder;
			bool isPrcValid = isTLSLPriceValid();
			try{
				if(tlslOrder == null || !tlslOrder.Name.Equals(CurrentTrade.TradeAction.StopLossSignal.SignalName)) {
					if(isPrcValid)
						SubmitOrderUnmanaged(0, GetExitOrderAction(), OrderType.StopMarket, HasPosition(),
						0, CurrentTrade.TradeAction.StopLossPrice, String.Empty, CurrentTrade.TradeAction.StopLossSignal.SignalName);
					else {
						CloseAllPositions();
					}
				}
				else if(tlslOrder != null && tlslOrder.Name.Equals(CurrentTrade.TradeAction.StopLossSignal.SignalName)
					&& tlslOrder.StopPrice != CurrentTrade.TradeAction.StopLossPrice
					&& isPrcValid) {
					ChangeOrder(tlslOrder, tlslOrder.Quantity, 0, CurrentTrade.TradeAction.StopLossPrice);
				}
				if(!isPrcValid) {
					throw new Exception("isTLSLPriceValid false"
					+ ";trailingSLPrice=" + CurrentTrade.TradeAction.StopLossPrice
					+ ";Close[0]=" + Close[0]);
				}
			} catch(Exception ex) {
				throw new Exception("Ex SetTrailingStopLossOrderUM:" + ex.Message);
			}
		}
		
		public virtual void SetSimpleExitOCO(string sigName) {
			if(!isOcoPriceValid())
				throw new Exception("Invalid OCO price:"
				+ "stopLossPrice=" + CurrentTrade.TradeAction.StopLossPrice
				+ ";profitTargetPrice=" + CurrentTrade.TradeAction.ProfitTargetPrice);
			if(IsUnmanaged) {
				SetSimpleExitOCOUM();
				return;
			}
			int prtLevel = 0;
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			indicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ": SetSimpleExitOCO-" 
			+ sigName + "-avg=" + GetAvgPrice());
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			CurrentTrade.stopLossAmt = MM_StopLossAmt;
			CurrentTrade.profitTargetAmt = MM_ProfitTargetAmt;
			CurrentTrade.SLCalculationMode = CalculationMode.Currency;
			CurrentTrade.PTCalculationMode = CalculationMode.Currency;
			SetStopLossOrder(sigName);
			SetProfitTargetOrder(sigName);
		}
		
		/// <summary>
		/// Set OCO for exit;
		/// </summary>
		public virtual void SetSimpleExitOCOUM() {
			int prtLevel = 0;
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			indicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ": SetExitOcoUM;" 
			+ ";avgPrc=" + GetAvgPrice()
			+ ";ocoID=" + CurrentTrade.ocoID
			+ ";Position.Quantity=" + HasPosition()
			);
			Order slOrd = CurrentTrade.BracketOrder.OCOOrder.StopLossOrder;
			Order ptOrd = CurrentTrade.BracketOrder.OCOOrder.ProfitTargetOrder;
			if(CurrentTrade.ocoID == null || (slOrd == null && ptOrd == null) 
				|| (slOrd.OrderState != OrderState.Working && ptOrd.OrderState != OrderState.Working))
				CurrentTrade.ocoID = GetNewOCOId();
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			SetProfitTargetOrderUM();
			SetStopLossOrderUM();
		}		
		//SetParabolicStop(string fromEntrySignal, CalculationMode mode, double value, bool isSimulatedStop, double acceleration, double accelerationMax, double accelerationStep)
		#endregion
		
		public virtual bool CloseAllPositions() 
		{
			//giParabSAR.PrintLog(true, !backTest, log_file, "CloseAllPosition called");
			if(GetMarketPosition() == MarketPosition.Long) {
				if(IsUnmanaged)
					SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Market, HasPosition());
				else
					ExitLong();
			}
			if(GetMarketPosition() == MarketPosition.Short) {
				if(IsUnmanaged)
					SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Market, HasPosition());
				else				
					ExitShort();
			}
			return true;
		}
		
		public virtual bool CancelAllOrders()
		{
			CancelExitOrders();
			CancelEntryOrders();			
			return true;
		}

		public virtual bool CancelEntryOrders()
		{
			if(CurrentTrade.BracketOrder.EntryOrder != null) {
				CancelOrder(CurrentTrade.BracketOrder.EntryOrder);
				CurrentTrade.BracketOrder.EntryOrder = null;
			}
			return true;
		}
		
		public virtual bool CancelExitOrders()
		{
			indicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + "- CancelExitOrders called");
			CancelExitOCO();
			CancelTrailingStopLoss();
			return true;
		}
		
		public virtual bool CancelExitOCO()
		{
			//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "- CancelAllOrders called");
			if(CurrentTrade.BracketOrder.OCOOrder.StopLossOrder != null)
				CancelOrder(CurrentTrade.BracketOrder.OCOOrder.StopLossOrder);
			if(CurrentTrade.BracketOrder.OCOOrder.ProfitTargetOrder != null)
				CancelOrder(CurrentTrade.BracketOrder.OCOOrder.ProfitTargetOrder);		
			return true;
		}

		public virtual bool CancelTrailingStopLoss()
		{
			if(CurrentTrade.TrailingSLOrder.TLSLOrder != null)
				CancelOrder(CurrentTrade.TrailingSLOrder.TLSLOrder);
			return true;
		}
		
		public virtual bool CancelAccountOrders() {
			if(Account.Orders != null) {
				foreach(Order ord in Account.Orders) {
					if(ord.OrderState == OrderState.Working ||
						ord.OrderState == OrderState.Accepted)
					Account.Cancel(new List<Order>{ord});
				}
			}
			return true;
		}
		#endregion
		
		#region Event Handlers
		
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
			if(BarsInProgress !=0) return;
			indicatorProxy.Log2Disk = true;
			Print(CurrentBar + ":OnExecutionUpdate"
			+ ";IsUnmanaged=" + IsUnmanaged
			+ ";IsLiveTrading=" + IsLiveTrading()
			+ ";GetMarketPosition=" + GetMarketPosition()
			+ ";marketPosition=" + marketPosition
			+ ";quantity=" + quantity
			+ ";HasPosition=" + HasPosition()
			+ ";GetAvgPrice=" + GetAvgPrice()
			+ ";price=" + price);
			if(IsUnmanaged) 
				OnExecutionUpdateUM(execution, executionId, price, quantity, marketPosition, orderId, time);
			else
				OnExecutionUpdateMG(execution, executionId, price, quantity, marketPosition, orderId, time);
		}
		
		/// <summary>
		/// For managed orders approach
		/// </summary>
		public virtual void OnExecutionUpdateMG(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
			if(BarsInProgress !=0) return;
			
			int bsx = BarsSinceExitExecution(0, "", 0);
			int bse = BarsSinceEntryExecution(0, "", 0);
			
			indicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":OnExecutionUpdateMG-quant,mktPos,prc, AvgPrc, BarsSinceEx,BarsSinceEn=" 
			+ quantity + "," + marketPosition + "," + price + "," + GetAvgPrice() + ","
			+ bsx + "," + bse
			+ ",SL=" + CurrentTrade.TradeAction.StopLossPrice
			+ ",Ordername=" + GetOrderName(execution.Order.Name));
			
			// Remember to check the underlying IOrder object for null before trying to access its properties
			if (execution.Order != null && execution.Order.OrderState == OrderState.Filled) {
				if(HasPosition() != 0) {
					//SetEntryOrder(OrderSignalName.EntryShort, execution.Order);					
					CalProfitTargetAmt(price, CurrentTrade.profitFactor);
					CalExitOcoPrice(GetAvgPrice(), CurrentTrade.profitFactor);
					SetSimpleExitOCO(CurrentTrade.TradeAction.EntrySignal.SignalName);

					//SetProfitTargetOrder(OrderSignalName.EntryShort.ToString());
					//SetStopLossOrder(OrderSignalName.EntryShort.ToString());
				}
				//if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + " Exe=" + execution.Name + ",Price=" + execution.Price + "," + execution.Time.ToShortTimeString());
				//if(drawTxt) {
				//	IText it = DrawText(CurrentBar.ToString()+Time[0].ToShortTimeString(), Time[0].ToString().Substring(10)+"\r\n"+execution.Name+":"+execution.Price, 0, execution.Price, Color.Red);
				//	it.Locked = false;
				//}
			}
		}
		
		/// <summary>
		/// For un-managed orders approach
		/// </summary>
		public virtual void OnExecutionUpdateUM(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
			if(BarsInProgress !=0) return;			
			
			int bsx = BarsSinceExitExecution(0, "", 0);
			int bse = BarsSinceEntryExecution(0, "", 0);
			
			indicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":OnExecutionUpdateUM"
			+ ";price=" + price
			+ ";Ordername=" + GetOrderName(execution.Order.Name)
			+ ";entrySignalName=" + CurrentTrade.TradeAction.EntrySignal.SignalName
			+ ";ocoID=" + CurrentTrade.ocoID
			+ ";OCO=" + execution.Order.Oco			
			+ ";bsx=" + bsx
			+ ";bse=" + bse
			+ ";quant=" + quantity
			+ ";HasPosition=" + HasPosition()
			+ ";mktPos=" + marketPosition);
			
			// Remember to check the underlying IOrder object for null before trying to access its properties
			if (execution.Order != null) {
				string oName = GetOrderName(execution.Order.Name);
				OrderState oState = execution.Order.OrderState;
				if(marketPosition != MarketPosition.Flat) { //(HasPosition() != 0) {
					if(oState == OrderState.Filled 
						&& oName.Equals(CurrentTrade.TradeAction.EntrySignal.SignalName)) {
					//SetEntryOrder(OrderSignalName.EntryShort, execution.Order);
						CurrentTrade.BracketOrder.EntryOrder = execution.Order;
						CurrentTrade.TrailingSLOrder.EntryOrder = execution.Order;
					}
				}
				else if (oName.Equals(OrderSignalName.ExitOnSessionClose.ToString())) {
					CancelExitOrders();
				}
			}
		}

		protected override void OnOrderUpdate(Cbi.Order order, double limitPrice, double stopPrice, 
			int quantity, int filled, double averageFillPrice, 
			Cbi.OrderState orderState, DateTime time, Cbi.ErrorCode error, string comment)
		{
			if(BarsInProgress !=0) return;
			indicatorProxy.Log2Disk = true;
			
			Print(CurrentBar + ":OnOrderUpdate IsUnmanaged=" + IsUnmanaged);
			
			if(IsUnmanaged)
				OnOrderUpdateUM(order, limitPrice, stopPrice, quantity, filled, 
				averageFillPrice, orderState, time, error, comment);
			else
				OnOrderUpdateMG(order, limitPrice, stopPrice, quantity, filled, 
				averageFillPrice, orderState, time, error, comment);	
		}
		
		/// <summary>
		/// For managed orders approach
		/// </summary>
		public virtual void OnOrderUpdateMG(Cbi.Order order, double limitPrice, double stopPrice, 
			int quantity, int filled, double averageFillPrice, 
			Cbi.OrderState orderState, DateTime time, Cbi.ErrorCode error, string comment)
		{
			if(BarsInProgress !=0) return;
			
			int bsx = BarsSinceExitExecution(0, "", 0);
			int bse = BarsSinceEntryExecution(0, "", 0);
			int prtLevel = 0;
			
			indicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":OnOrderUpdateMG name-" + GetOrderName(order.Name) + "-" + order.FromEntrySignal + ";" + order.OrderTypeString
			+ ";" + order.OrderState.ToString() + ";" + order.OrderAction.ToString()
			+ ";SP=" + order.StopPrice + ";LP=" + order.LimitPrice
			+ "; BarsSinceExit, BarsSinceEntry=" + bsx + "," + bse);
			
			GetBracketOrderSubType(order);
			
			indicatorProxy.TraceMessage(this.Name, prtLevel);
		    if (CurrentTrade.BracketOrder.EntryOrder != null && CurrentTrade.BracketOrder.EntryOrder == order)
		    {
				//giParabSAR.PrintLog(true, !backTest, log_file, order.ToString() + "--" + order.OrderState);
		        if (order.OrderState == OrderState.Cancelled || 
					order.OrderState == OrderState.Filled || 
					order.OrderState == OrderState.Rejected || 
					order.OrderState == OrderState.Unknown)
				{
					CurrentTrade.barsSinceEnOrd = 0;
					CurrentTrade.BracketOrder.EntryOrder = null;
				}
		    }
			
			indicatorProxy.TraceMessage(this.Name, prtLevel);
		    if (//CurrentTrade.BracketOrder.OCOOrder != null &&
				CurrentTrade.BracketOrder.OCOOrder.StopLossOrder != null &&
				CurrentTrade.BracketOrder.OCOOrder.StopLossOrder == order)
		    {
				//giParabSAR.PrintLog(true, !backTest, log_file, order.ToString() + "--" + order.OrderState);
		        if (order.OrderState == OrderState.Cancelled || 
					order.OrderState == OrderState.Filled || 
					order.OrderState == OrderState.Rejected)
				{
					CurrentTrade.BracketOrder.OCOOrder.StopLossOrder = null;
				}
		    }
			
			indicatorProxy.TraceMessage(this.Name, prtLevel);
		    if (//CurrentTrade.BracketOrder.OCOOrder != null &&
				CurrentTrade.BracketOrder.OCOOrder.ProfitTargetOrder != null &&
				CurrentTrade.BracketOrder.OCOOrder.ProfitTargetOrder == order)
		    {
				//giParabSAR.PrintLog(true, !backTest, log_file, order.ToString() + "--" + order.OrderState);
		        if (order.OrderState == OrderState.Cancelled || 
					order.OrderState == OrderState.Filled || 
					order.OrderState == OrderState.Rejected)
				{
					CurrentTrade.BracketOrder.OCOOrder.ProfitTargetOrder = null;
				}
		    }
			
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			if (order.OrderState == OrderState.Working){// || order.OrderType == OrderType.StopMarket) {
				if(GetOrderName(order.Name) == CurrentTrade.TradeAction.EntrySignal.SignalName) //OrderSignalName.EntryLong.ToString() ||
					//GetOrderName(order.Name) == OrderSignalName.EntryShort.ToString()) 
				{
					indicatorProxy.PrintLog(true, IsLiveTrading(), "Entry Order Name=" + GetOrderName(order.Name));
					CurrentTrade.BracketOrder.EntryOrder = order;
				}
				if(GetOrderName(order.Name) == OrderSignalName.ProfitTarget.ToString()) {
					indicatorProxy.PrintLog(true, IsLiveTrading(), "order.Name == OrderSignalName.Profittarget");
					CurrentTrade.BracketOrder.OCOOrder.ProfitTargetOrder = order;
				}				
				//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + order.ToString());
			}
			
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			if(GetOrderName(order.Name) == OrderSignalName.StopLoss.ToString() &&
				(order.OrderState == OrderState.Accepted || order.OrderState == OrderState.Working)) {
				CurrentTrade.BracketOrder.OCOOrder.StopLossOrder = order;
			}
			
//			if(CurrentTrade.BracketOrder.OCOOrder.StopLossOrder == null && CurrentTrade.BracketOrder.OCOOrder.ProfitTargetOrder == null)
				//InitTradeMgmt();			
		}
		
		/// <summary>
		/// For un-managed orders approach
		/// </summary>
		public virtual void OnOrderUpdateUM(Cbi.Order order, double limitPrice, double stopPrice, 
			int quantity, int filled, double averageFillPrice, 
			Cbi.OrderState orderState, DateTime time, Cbi.ErrorCode error, string comment)
		{
			if(BarsInProgress !=0) return;
			
			int bsx = BarsSinceExitExecution(0, "", 0);
			int bse = BarsSinceEntryExecution(0, "", 0);
			string oName = GetOrderName(order.Name);
			int prtLevel = 0;
			
			indicatorProxy.PrintLog(true, IsLiveTrading(),CurrentBar + ":OnOrderUpdateUM name=" + oName
			+ ";Type=" + order.OrderTypeString
			+ ";SP=" + order.StopPrice 
			+ ";LP=" + order.LimitPrice			
			+ ";State=" + orderState.ToString()
			+ ";Action=" + order.OrderAction.ToString()
			+ ";ocoID=" + CurrentTrade.ocoID
			+ ";OCO=" + order.Oco
			+ ";BarsSinceExit=" + bsx
			+ ";BarsSinceEntry=" + bse
			+ ";FromEntrySignal=" + order.FromEntrySignal);
			
			GetBracketOrderSubType(order);
						
			indicatorProxy.TraceMessage(this.Name, prtLevel);
		    if (CurrentTrade.BracketOrder.EntryOrder != null && CurrentTrade.BracketOrder.EntryOrder == order)
		    {
				if (orderState == OrderState.Cancelled ||
					//order.OrderState == OrderState.Filled ||
					orderState == OrderState.Rejected ||
					orderState == OrderState.Unknown)
				{
					CurrentTrade.barsSinceEnOrd = 0;
					CurrentTrade.BracketOrder.EntryOrder = null;
				}
		    }
			
			indicatorProxy.TraceMessage(this.Name, prtLevel);
		    if (CurrentTrade.TrailingSLOrder.EntryOrder != null && CurrentTrade.TrailingSLOrder.EntryOrder == order)
		    {
				if (orderState == OrderState.Cancelled || 
					orderState == OrderState.Rejected || 
					orderState == OrderState.Unknown)
				{
					CurrentTrade.barsSinceEnOrd = 0;
					CurrentTrade.TrailingSLOrder.EntryOrder = null;
				}
		    }
			
			indicatorProxy.TraceMessage(this.Name, prtLevel);
		    if (CurrentTrade.BracketOrder.OCOOrder.StopLossOrder != null &&
				CurrentTrade.BracketOrder.OCOOrder.StopLossOrder == order)
		    {				
		        if (orderState == OrderState.Cancelled || 
					orderState == OrderState.Filled || 
					orderState == OrderState.Rejected)
				{
					CurrentTrade.BracketOrder.OCOOrder.StopLossOrder = null;
					ClearOrderName(oName);
				}
		    }
			
			indicatorProxy.TraceMessage(this.Name, prtLevel);
		    if (CurrentTrade.BracketOrder.OCOOrder.ProfitTargetOrder != null &&
				CurrentTrade.BracketOrder.OCOOrder.ProfitTargetOrder == order)
		    {
				if (orderState == OrderState.Cancelled || 
					orderState == OrderState.Filled || 
					orderState == OrderState.Rejected)
				{
					CurrentTrade.BracketOrder.OCOOrder.ProfitTargetOrder = null;
					ClearOrderName(oName);
				}
		    }

			indicatorProxy.TraceMessage(this.Name, prtLevel);
		    if ( CurrentTrade.TrailingSLOrder.TLSLOrder != null && CurrentTrade.TrailingSLOrder.TLSLOrder == order)
		    {				
		        if (orderState == OrderState.Cancelled || 
					orderState == OrderState.Filled || 
					orderState == OrderState.Rejected)
				{
					CurrentTrade.TrailingSLOrder.TLSLOrder = null;
					ClearOrderName(oName);
				}
		    }
			
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			if (orderState == OrderState.Working || orderState == OrderState.Accepted) {
				if(oName.Equals(CurrentTrade.TradeAction.EntrySignal.SignalName)) {
					indicatorProxy.PrintLog(true, IsLiveTrading(), "Entry Order Name=" + oName);
					CurrentTrade.BracketOrder.EntryOrder = order;
					CurrentTrade.TrailingSLOrder.EntryOrder = order;
				}
				
				if(order.Oco != null && order.Oco.Equals(CurrentTrade.ocoID)){
					indicatorProxy.PrintLog(true, IsLiveTrading(), "Exit order.Name=" + oName);
					if(oName.Equals(CurrentTrade.TradeAction.ProfitTargetSignal.SignalName)) {						
						CurrentTrade.BracketOrder.OCOOrder.ProfitTargetOrder = order;
					}
					if(oName.Equals(CurrentTrade.TradeAction.StopLossSignal.SignalName)) {						
						CurrentTrade.BracketOrder.OCOOrder.StopLossOrder = order;
					}
				}
				
				if(oName.Equals(CurrentTrade.TradeAction.StopLossSignal.SignalName)) {
					indicatorProxy.PrintLog(true, IsLiveTrading(), "TLSL Order Name=" + oName);
					CurrentTrade.TrailingSLOrder.TLSLOrder = order;
				}
			}
		}
		
		#endregion
		
		#region Order utilities functions
		
		/// <summary>
		/// Entry order Signal name with timestamp:
		/// EntryShort-201905312359888
		/// </summary>
		/// <param name="nameTag"></param>
		/// <returns></returns>
		public string GetNewEnOrderSignalName(string nameTag) {
			string timeStr = GetBarTimestampStr(0);
			CurrentTrade.TradeAction.EntrySignal.SignalName = nameTag + "-" + timeStr;
			SetNewOcoOrderSignalName(nameTag, timeStr);
			SetNewTLSLOrderSignalName(nameTag, timeStr);
			return CurrentTrade.TradeAction.EntrySignal.SignalName;
		}
				
		/// <summary>
		/// Bracket order Signal name with timestamp:
		/// EntryShort-SL-201905312359888
		/// </summary>
		/// <param name="nameTag"></param>
		/// <returns></returns>
		public void SetNewOcoOrderSignalName(string nameTag, string timeStr) {
			CurrentTrade.TradeAction.StopLossSignal.SignalName = nameTag + "-SL-" + timeStr;
			CurrentTrade.TradeAction.ProfitTargetSignal.SignalName = nameTag + "-PT-" + timeStr;			
		}

		/// <summary>
		/// TrailingSL order Signal name with timestamp:
		/// EntryShort-TLSL-201905312359888
		/// </summary>
		/// <param name="nameTag"></param>
		/// <returns></returns>
		public void SetNewTLSLOrderSignalName(string nameTag, string timeStr) {
			CurrentTrade.TradeAction.StopLossSignal.SignalName = nameTag + "-TLSL-" + timeStr;
		}
		
		/// <summary>
		/// New OCO ID with timestamp:
		/// OCO-201905312359888
		/// </summary>
		/// <returns></returns>
		public string GetNewOCOId() {
			string sName = "OCO-" + GetBarTimestampStr(0);
			return sName;
		}
		
		//Remove whitespaces from order name
		public string GetOrderName(string orderName) {
			return Regex.Replace(orderName, @"\s+", "");
		}
		
		//Clear order name
		public void ClearOrderName(string oName) {
			if(oName == null) return;
			else if(oName.Equals(CurrentTrade.TradeAction.EntrySignal.SignalName)) CurrentTrade.TradeAction.EntrySignal.SignalName = String.Empty;
			else if(oName.Equals(CurrentTrade.TradeAction.StopLossSignal.SignalName)) {
				CurrentTrade.TradeAction.StopLossSignal.SignalName = String.Empty;
				CurrentTrade.ocoID = String.Empty;
			}
			else if(oName.Equals(CurrentTrade.TradeAction.ProfitTargetSignal.SignalName)) {
				CurrentTrade.TradeAction.ProfitTargetSignal.SignalName = String.Empty;
				CurrentTrade.ocoID = String.Empty;
			}
			else if(oName.Equals(CurrentTrade.TradeAction.StopLossSignal.SignalName)) CurrentTrade.TradeAction.StopLossSignal.SignalName = String.Empty;
		}
		
		/// <summary>
		/// Get the subType of the bracket order: entry, SL or PT
		/// </summary>
		/// <param name="order"></param>
		/// <returns></returns>
		public BracketOrderSubType GetBracketOrderSubType(Order order) {
			BracketOrderSubType bost = BracketOrderSubType.UnKnown;
			if(order != null) {
				indicatorProxy.PrintLog(true, IsLiveTrading(), "GetBracketOrderSubType:" +
				order.Name + "," + order.OrderTypeString);
			}
			return bost;
		}
		
		public OrderAction GetExitOrderAction() {
			OrderAction oAct;
			if(GetMarketPosition()==MarketPosition.Short)
				oAct = OrderAction.BuyToCover;
			else if(GetMarketPosition()==MarketPosition.Long)
				oAct = OrderAction.Sell;
			else throw new Exception("GetExitOrderAction error MarketPosition=" + GetMarketPosition());
			return oAct;
		}
		
		#endregion

		#region TM Properties
		
		[XmlIgnore, Browsable(false)]
		public CurrentTrade CurrentTrade
		{
			get; set;
		}
		
		//Only types which can be Xml Serialized should be marked as a NinjaScriptAttribute,
		//otherwise you may run into errors when persisting values in various scenarios 
		//(e.g., saving workspace, or running Strategy Optimizations). 
        [Description("Short, Long or both directions for entry")]
 		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TradingDirection", GroupName = GPS_TRADE_MGMT, Order = ODG_TradingDirection)]		
        public TradingDirection TM_TradingDirection
        {
            get { return tm_TradingDirection; }
            set { tm_TradingDirection = value; }
        }

        [Description("Trading style: trend following, counter trend, scalp")]
 		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TradingStyle", GroupName = GPS_TRADE_MGMT, Order = ODG_TradingStyle)]
        public TradingStyle TM_TradingStyle
        {
            get { return tm_TradingStyle; }
            set { tm_TradingStyle = value; }
        }
		
		[Description("Use trailing entry every bar")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EnTrailing", GroupName = GPS_TRADE_MGMT, Order = ODG_EnTrailing)]
        public bool TM_EnTrailing
        {
            get{return tm_EnTrailing;}
            set{tm_EnTrailing = value;}
        }
		
        [Description("Offeset points for limit price entry")]
		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EnOffsetPnts", GroupName = GPS_TRADE_MGMT, Order = ODG_EnOffsetPnts)]
        public double TM_EnOffsetPnts
        {
            get{return tm_EnOffsetPnts;}
            set{tm_EnOffsetPnts = Math.Max(0, value);}
        }
		
        [Description("How long to check entry order filled or not")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MinutesChkEnOrder", GroupName = GPS_TRADE_MGMT, Order = ODG_MinutesChkEnOrder)]
        public int TM_MinutesChkEnOrder
        {
            get{return tm_MinutesChkEnOrder;}
            set{tm_MinutesChkEnOrder = Math.Max(0, value);}
        }
		
        [Description("How long to check P&L")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MinutesChkPnL", GroupName = GPS_TRADE_MGMT, Order = ODG_MinutesChkPnL)]
        public int TM_MinutesChkPnL
        {
            get{return tm_MinutesChkPnL;}
            set{tm_MinutesChkPnL = Math.Max(0, value);}
        }
		
		[Description("Bar count before checking P&L")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarsToCheckPL", GroupName = GPS_TRADE_MGMT, Order = ODG_BarsToCheckPL)]
        public int TM_BarsToCheckPnL
        {
            get{return tm_BarsToCheckPnL;}
            set{tm_BarsToCheckPnL = Math.Max(1, value);}
        }
		
        [Description("How many bars to hold entry order before cancel it")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarsHoldEnOrd", GroupName = GPS_TRADE_MGMT, Order = ODG_BarsHoldEnOrd)]
        public int TM_BarsHoldEnOrd
        {
            get{return tm_BarsHoldEnOrd;}
            set{tm_BarsHoldEnOrd = Math.Max(1, value);}
        }
		
        [Description("Bar count for en order counter pullback")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EnCounterPBBars", GroupName = GPS_TRADE_MGMT, Order = ODG_EnCounterPBBars)]
        public int TM_EnCounterPBBars
        {
            get{return tm_EnCounterPBBars;}
            set{tm_EnCounterPBBars = Math.Max(1, value);}
        }
				
		[Description("Bar count since last filled PT or SL")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarsSincePTSL", GroupName = GPS_TRADE_MGMT, Order = ODG_BarsSincePTSL)]
        public int TM_BarsSincePTSL
        {
            get{return tm_BarsSincePtSl;}
            set{tm_BarsSincePtSl = Math.Max(1, value);}
        }
		
		#endregion

		#region Variables for Properties
		
		private TradingDirection tm_TradingDirection = TradingDirection.Both; // -1=short; 0-both; 1=long;
		private TradingStyle tm_TradingStyle = TradingStyle.TrendFollowing; // -1=counter trend; 1=trend following;				
		private bool tm_EnTrailing = false;
		private double tm_EnOffsetPnts = 1;
		private int tm_MinutesChkEnOrder = 10;
		private int tm_MinutesChkPnL = 30;
		private int tm_BarsToCheckPnL = 2;
		private int tm_BarsHoldEnOrd = 3;
		private int tm_EnCounterPBBars = 2;
		private int tm_BarsSincePtSl = 1;
		
		#endregion
	}
}
