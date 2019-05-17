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
	public partial class GSZTraderBase : Strategy
	{		
		#region Trade Mgmt Functions
		
		public virtual void InitTradeMgmt() {
//			if(tradeObj != null) {
//				SetProfitTarget(CalculationMode.Currency, tradeObj.profitTargetAmt);
//	            SetStopLoss(CalculationMode.Currency, tradeObj.stopLossAmt);
//			} else {
//				SetProfitTarget(CalculationMode.Currency, MM_ProfitTargetAmt);
//	            SetStopLoss(CalculationMode.Currency, MM_StopLossAmt);
//			}
		}
		
		public virtual void PutTrade() {
			Print(CurrentBar + "::PutTrade()--" + this.ToString());
		}
		
		/// <summary>
		/// Check performance first, 
		/// then check signal to determine special exit: 
		/// liquidate, reverse, or scale in/out;
		/// </summary>
		/// <returns></returns>
		public virtual TradeObj CheckExitTrade() {
			int prtLevel = 1;
			//tradeObj.SetTradeType(TradeType.Exit);
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			ChangeSLPT();
			//if(Position.MarketPosition == MarketPosition.Flat) return null;
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			CheckExitTradeBySignal();
			
			return tradeObj;
		}
		
		/// <summary>
		/// Check exit trader from signal instead of by money management policy
		/// </summary>
		/// <returns></returns>
		public virtual void CheckExitTradeBySignal() {
			if(indicatorSignal == null) return;
			Print(CurrentBar + ":CheckExitTradeBySignal"
			+ ";indicatorSignal.ReversalDir=" + indicatorSignal.ReversalDir.ToString()
			+ ";Position.MarketPosition=" + Position.MarketPosition
			);
			if((indicatorSignal.ReversalDir == Reversal.Up && Position.MarketPosition == MarketPosition.Short) ||
				(indicatorSignal.ReversalDir == Reversal.Down && Position.MarketPosition == MarketPosition.Long)) {
				tradeObj.SetTradeType(TradeType.Liquidate);
				CloseAllPositions();
				CancelExitOrders();
			} else {

			}			
		}
		
		/// <summary>
		/// Check if new entry trade could be generated
		/// </summary>
		/// <returns></returns>
		public virtual TradeObj CheckNewEntryTrade() {
//			if(NewOrderAllowed()) {
//			}
			Print(CurrentBar + "::CheckNewEntryTrade()--" + this.ToString());
			return null;
		}
	
		public virtual bool CheckEnOrder(double cur_gap)
        {
            double min_en = -1;

            if (tradeObj.BracketOrder.EntryOrder != null && tradeObj.BracketOrder.EntryOrder.OrderState == OrderState.Working)
            {
                min_en = indicatorProxy.GetMinutesDiff(tradeObj.BracketOrder.EntryOrder.Time, Time[0]);// DateTime.Now);
                //if ( IsTwoBarReversal(cur_gap, TickSize, enCounterPBBars) || (barsHoldEnOrd > 0 && barsSinceEnOrd >= barsHoldEnOrd) || ( minutesChkEnOrder > 0 &&  min_en >= minutesChkEnOrder))
				if ( (tradeObj.barsHoldEnOrd > 0 && tradeObj.barsSinceEnOrd >= tradeObj.barsHoldEnOrd) || ( tradeObj.minutesChkEnOrder > 0 &&  min_en >= tradeObj.minutesChkEnOrder))	
                {
                    CancelOrder(tradeObj.BracketOrder.EntryOrder);
                    //giParabSAR.PrintLog(true, !backTest, log_file, "Order cancelled for " + AccName + ":" + barsSinceEnOrd + "/" + min_en + " bars/mins elapsed--" + BracketOrder.EntryOrder.ToString());
					return true;
                }
				else {
					//giParabSAR.PrintLog(true, !backTest, log_file, "Order working for " + AccName + ":" + barsSinceEnOrd + "/" + min_en + " bars/mins elapsed--" + BracketOrder.EntryOrder.ToString());
					tradeObj.barsSinceEnOrd++;
				}
            }
            return false;
        }
		
		public virtual double GetEnLongPrice() {
			double prc = -1;
			switch(tradeObj.BracketOrder.EnOrderType) {
				case OrderType.Limit:
					prc = (tradeObj.enTrailing && tradeObj.enCounterPBBars>0) ? Close[0]-tradeObj.enOffsetPnts :  Low[0]-tradeObj.enOffsetPnts;
					break;
				case OrderType.StopMarket:
					prc = tradeObj.enStopPrice;
					break;
				case OrderType.StopLimit:
					prc = tradeObj.enStopPrice;
					break;
			}
			return prc;
		}
		
		public virtual bool NewOrderAllowed()
		{
			if(BarsInProgress !=0) return false;
			
			Print(MethodBase.GetCurrentMethod().ToString() + " - 1");
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
			if((backTest && IsLiveTrading()) || (!backTest && !IsLiveTrading())) {
				//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + "[backTest,Historical]=" + backTest + "," + Historical + "- NewOrderAllowed=false - " + Get24HDateTime(Time[0]));
				return false;
			}
			
			
			//indicatorProxy.TraceMessage(this.Name, prtLevel);
			if(!backTest && (plrt <= tradeObj.dailyLossLmt || pnl_daily <= tradeObj.dailyLossLmt))
			{
				if(PrintOut > -1) {
					Print(CurrentBar + "-" + AccName + ": dailyLossLmt reached = " + pnl_daily + "," + plrt);
				}
				return false;
			}
			
			
			//indicatorProxy.TraceMessage(this.Name, prtLevel);
			if (backTest && pnl_daily <= tradeObj.dailyLossLmt) {
				if(PrintOut > -3) {
					Print(CurrentBar + "-" + AccName + ": dailyLossLmt reached = " + pnl_daily + "," + plrt);
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ": backTest dailyLossLmt reached = " + pnl);
				}
				return false;				
			}
		
			
			//indicatorProxy.TraceMessage(this.Name, prtLevel);
			if (IsTradingTime(170000) && Position.Quantity == 0)
			{
				if (tradeObj.BracketOrder.EntryOrder == null || tradeObj.BracketOrder.EntryOrder.OrderState != OrderState.Working || tradeObj.enTrailing)
				{					
					if(bsx < 0 || bsx > tradeObj.barsSincePTSL) //if(bsx == -1 || bsx > tradeObj.barsSincePtSl)
					{
						Print(CurrentBar + "-bsx,bse,tradeObj.barsSincePtSl=" + bsx + "," + bse + "," + tradeObj.barsSincePTSL);
						//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + "- NewOrderAllowed=true - " + Get24HDateTime(Time[0]));
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
			tradeObj.entrySignalName = GetNewEnOrderSignalName(OrderSignalName.EntryLongLmt.ToString());
			Print(CurrentBar + ":NewLongLimitOrderUM"
			+";tradeObj.entrySignalName=" + tradeObj.entrySignalName);
			SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.Limit, 
			tradeObj.quantity, tradeObj.enLimitPrice, 0, "", tradeObj.entrySignalName);
			
			tradeObj.barsSinceEnOrd = 0;
		}
		/// <summary>
		/// Submit unmanaged short limit order, set the order object in OnOrderUpdate handler
		/// </summary>
		/// <param name="msg"></param>
		public virtual void NewShortLimitOrderUM(string msg)
		{
			tradeObj.entrySignalName = GetNewEnOrderSignalName(OrderSignalName.EntryShortLmt.ToString());
			Print(CurrentBar + ":NewShortLimitOrderUM"
			+";tradeObj.entrySignalName=" + tradeObj.entrySignalName);
			SubmitOrderUnmanaged(0, OrderAction.SellShort, OrderType.Limit, tradeObj.quantity, tradeObj.enLimitPrice, 0, "", tradeObj.entrySignalName);
			
			//double prc = (tradeObj.enTrailing && tradeObj.enCounterPBBars>0) ? Close[0]+tradeObj.enOffsetPnts : High[0]+tradeObj.enOffsetPnts;
			
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
			if(tradeObj.BracketOrder.EntryOrder == null) {
//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg + ", EnterShortLimit called short price=" + prc + "--" + Get24HDateTime(Time[0]));			
			}
			else if (tradeObj.BracketOrder.EntryOrder.OrderState == OrderState.Working) {
//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterShortLimit updated short price (old, new)=(" + BracketOrder.EntryOrder.LimitPrice + "," + prc + ") -- " + Get24HDateTime(Time[0]));		
				//CancelOrder(tradeObj.BracketOrder.EntryOrder);
				//BracketOrder.EntryOrder = EnterShortLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			}
			//tradeObj.BracketOrder.EntryOrder = EnterShortLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			tradeObj.barsSinceEnOrd = 0;
		}
		
		public virtual void NewLongLimitOrder(string msg, double zzGap, double curGap)
		{
			double prc = (tradeObj.enTrailing && tradeObj.enCounterPBBars>0) ? Close[0]-tradeObj.enOffsetPnts :  Low[0]-tradeObj.enOffsetPnts;
			
			if(tradeObj.BracketOrder.EntryOrder == null) {
				tradeObj.BracketOrder.EntryOrder = EnterLongLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterLongLimit called buy price= " + prc + " -- " + Get24HDateTime(Time[0]));
			}
			else if (tradeObj.BracketOrder.EntryOrder.OrderState == OrderState.Working) {
//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterLongLimit updated buy price (old, new)=(" + BracketOrder.EntryOrder.LimitPrice + "," + prc + ") -- " + Get24HDateTime(Time[0]));
				CancelOrder(tradeObj.BracketOrder.EntryOrder);
				tradeObj.BracketOrder.EntryOrder = EnterLongLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			}
			tradeObj.barsSinceEnOrd = 0;
		}

		public virtual void NewEntryLongOrder()
		{
			double prc = GetEnLongPrice();//(MM_EnTrailing && tradeObj.enCounterPBBars>0) ? Close[0]-tradeObj.enOffsetPnts :  Low[0]-tradeObj.enOffsetPnts;
			
			if (tradeObj.BracketOrder.EntryOrder.OrderState == OrderState.Working) {
//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterLongLimit updated buy price (old, new)=(" + BracketOrder.EntryOrder.LimitPrice + "," + prc + ") -- " + Get24HDateTime(Time[0]));
				//CancelOrder(tradeObj.BracketOrder.EntryOrder);
				CancelEntryOrders();
				//tradeObj.BracketOrder.EntryOrder = EnterLongLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			}
			if(tradeObj.BracketOrder.EntryOrder == null) {
				if(tradeObj.BracketOrder.EnOrderType == OrderType.StopMarket) {
					tradeObj.BracketOrder.EntryOrder = EnterLongStopMarket(DefaultQuantity, prc, OrderSignalName.EntryLongStopMkt.ToString());
				}
				else if(tradeObj.BracketOrder.EnOrderType == OrderType.Limit) {
					tradeObj.BracketOrder.EntryOrder = EnterLongLimit(DefaultQuantity, prc, OrderSignalName.EntryLongLmt.ToString());
				}
//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterLongLimit called buy price= " + prc + " -- " + Get24HDateTime(Time[0]));
			}
			tradeObj.barsSinceEnOrd = 0;
		}
		
		#region Exit Order functions
		public virtual void SetProfitTargetOrder(string sigName) {
			if(IsUnmanaged) {
				SetProfitTargetOrderUM();
				return;
			}
			Print(CurrentBar + ":SetProfitTargetOrder-" 
			+ sigName + "-" + tradeObj.PTCalculationMode 
			+ "-profitTargetAmt=" + tradeObj.profitTargetAmt
			+ "-profitTargetTic" + tradeObj.profitTargetTic
			+ "-profitTargetPrice" + tradeObj.profitTargetPrice + "-avg=" + Position.AveragePrice);
			try{
			switch(tradeObj.PTCalculationMode) {
				case CalculationMode.Currency :
					SetProfitTarget(sigName, CalculationMode.Currency, tradeObj.profitTargetAmt);
					break;
				case CalculationMode.Price :
					SetProfitTarget(sigName, CalculationMode.Price, tradeObj.profitTargetPrice);
					break;
				case CalculationMode.Ticks :
					SetProfitTarget(sigName, CalculationMode.Ticks, tradeObj.profitTargetTic);
					break;
				default: 
					SetProfitTarget(sigName, CalculationMode.Currency, tradeObj.profitTargetAmt);
					break;
			}
			} catch(Exception ex) {
				throw new Exception("Ex SetProfitTarget:" + ex.Message);
			}
		}

		public virtual void SetProfitTargetOrderUM() {
			Print(CurrentBar + ":SetProfitTargetOrderUM"
			+ ";tradeObj.entrySignalName=" + tradeObj.entrySignalName
			+ ";tradeObj.ocoID=" + tradeObj.ocoID
			+ ";GetExitOrderAction()=" + GetExitOrderAction().ToString()
			+ ";profitTargetAmt=" + tradeObj.profitTargetAmt
			+ ";profitTargetTic=" + tradeObj.profitTargetTic
			+ ";profitTargetPrice=" + tradeObj.profitTargetPrice
			+ ";avgPrc=" + Position.AveragePrice
			+ ";Position.Quantity=" + Position.Quantity);
			Order ptOrder = tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder;
			
			try{
				if(ptOrder == null || !ptOrder.Oco.Equals(tradeObj.ocoID)) {
					SubmitOrderUnmanaged(0, GetExitOrderAction(), OrderType.Limit, tradeObj.quantity,
					tradeObj.profitTargetPrice, 0, tradeObj.ocoID, tradeObj.profitTargetSignalName);
				}
				else if(ptOrder != null && ptOrder.Oco.Equals(tradeObj.ocoID)
					&& ptOrder.LimitPrice != tradeObj.profitTargetPrice) {
					ChangeOrder(ptOrder, ptOrder.Quantity, tradeObj.profitTargetPrice, 0);
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
			Print(CurrentBar + ":SetStopLossOrder"
			+ ";tradeObj.entrySignalName=" + tradeObj.entrySignalName
			+ ";tradeObj.ocoID=" + tradeObj.ocoID
			+ ";tradeObj.SLCalculationMode=" + tradeObj.SLCalculationMode
			+ ";stopLossAmt=" + tradeObj.stopLossAmt
			+ ";stopLossTic=" + tradeObj.stopLossTic
			+ ";stopLossPrice=" + tradeObj.stopLossPrice
			+ ";avgPrc=" + Position.AveragePrice);
			try {
			switch(tradeObj.SLCalculationMode) {
				case CalculationMode.Currency :
					SetStopLoss(sigName, CalculationMode.Currency, tradeObj.stopLossAmt, true);
					break;
				case CalculationMode.Price :
					SetStopLoss(sigName, CalculationMode.Price, tradeObj.stopLossPrice, true);
					break;
				case CalculationMode.Ticks :
					SetStopLoss(sigName, CalculationMode.Ticks, tradeObj.stopLossTic, true);
					break;
				default: 
					SetStopLoss(sigName, CalculationMode.Currency, tradeObj.stopLossAmt, true);
					break;
			}
			} catch(Exception ex) {
				throw new Exception("Ex SetStopLossOrder:" + ex.Message);
			}
		}
		
		public virtual void SetStopLossOrderUM() {
			Print(CurrentBar + ":SetStopLossOrderUM" 
			+ ";tradeObj.entrySignalName=" + tradeObj.entrySignalName
			+ ";tradeObj.ocoID=" + tradeObj.ocoID
			+ ";GetExitOrderAction()=" + GetExitOrderAction().ToString()			
			+ ";tradeObj.SLCalculationMode=" + tradeObj.SLCalculationMode
			+ ";stopLossAmt=" + tradeObj.stopLossAmt
			+ ";stopLossTic=" + tradeObj.stopLossTic
			+ ";stopLossPrice=" + tradeObj.stopLossPrice
			+ ";avgPrc=" + Position.AveragePrice
			+ ";Position.Quantity=" + Position.Quantity);
			
			Order slOrder = tradeObj.BracketOrder.OCOOrder.StopLossOrder;
			try{
				if(slOrder == null || !slOrder.Oco.Equals(tradeObj.ocoID)) {
					SubmitOrderUnmanaged(0, GetExitOrderAction(), OrderType.StopMarket, tradeObj.quantity,
					0, tradeObj.stopLossPrice, tradeObj.ocoID, tradeObj.stopLossSignalName);
				}
				else if(slOrder != null && slOrder.Oco.Equals(tradeObj.ocoID)
					&& slOrder.StopPrice != tradeObj.stopLossPrice) {
					ChangeOrder(slOrder, slOrder.Quantity, 0, tradeObj.stopLossPrice);
				}
			} catch(Exception ex) {
				throw new Exception("Ex SetStopLossOrderUM:" + ex.Message);
			}
		}

		/// <summary>
		/// Setup breakeven order
		/// </summary>
		public virtual void SetBreakEvenOrder(double avgPrc) {			
			Order stopOrder = tradeObj.BracketOrder.OCOOrder.StopLossOrder;
			string tif = stopOrder == null?
				"N/A":stopOrder.TimeInForce.ToString()+",IsLiveUntilCancelled="+stopOrder.IsLiveUntilCancelled;
			tradeObj.stopLossAmt = 0;
			tradeObj.stopLossPrice = avgPrc;
			
			if(IsUnmanaged) {
				indicatorProxy.PrintLog(true, !backTest, 
					AccName + "-Setup SL BreakevenUM posAvgPrc=" + avgPrc + "," + tif);				
				SetBreakEvenOrderUM(avgPrc);
				return;
			}
			indicatorProxy.PrintLog(true, !backTest, 
				AccName + "-Setup SL Breakeven posAvgPrc=" + avgPrc + "," + tif);
			SetStopLossOrder(tradeObj.entrySignalName.ToString());
		}
		
		/// <summary>
		/// Setup breakeven order for unmanaged approach
		/// </summary>
		public virtual void SetBreakEvenOrderUM(double slPrc) {
			Order stopOrder = tradeObj.BracketOrder.OCOOrder.StopLossOrder;
			if (stopOrder != null && stopOrder.StopPrice != slPrc)
        		ChangeOrder(stopOrder, stopOrder.Quantity, 0, slPrc);
		}
		
		public virtual void SetTrailingStopLossOrder(string sigName) {
			CancelExitOCO();
			
			if(IsUnmanaged) {
				SetTrailingStopLossOrderUM();
				return;
			}
			
			Print(CurrentBar + ":SetTrailingStopLossOrder-" 
			+ sigName + "-" + tradeObj.TLSLCalculationMode 
			+ "-trailingSLAmt=" + tradeObj.trailingSLAmt
			+ "-trailingSLTic=" + tradeObj.trailingSLTic + "-avg=" + Position.AveragePrice);
			try {
				switch(tradeObj.TLSLCalculationMode) {
					case CalculationMode.Ticks :
						SetTrailStop(sigName, CalculationMode.Ticks, tradeObj.trailingSLTic, true);
						break;
					case CalculationMode.Percent :
						SetTrailStop(sigName, CalculationMode.Percent, tradeObj.trailingSLAmt, true);
						break;
					default: 
						SetTrailStop(sigName, CalculationMode.Ticks, tradeObj.trailingSLTic, true);
						break;
				}
			} catch(Exception ex) {
				throw new Exception("Ex SetTrailingStopLossOrder:" + ex.Message);
			}
		}

		public virtual void SetTrailingStopLossOrderUM() {
			Print(CurrentBar + ":SetTrailingStopLossOrderUM"
			+ ";trailingSLSignalName=" + tradeObj.trailingSLSignalName
			+ ";TLSLCalculationMode=" + tradeObj.TLSLCalculationMode
			+ ";trailingPTTic=" + tradeObj.trailingPTTic
			+ ";trailingSLAmt=" + tradeObj.trailingSLAmt
			+ ";trailingSLTic=" + tradeObj.trailingSLTic 
			+ ";GetAvgPrice=" + GetAvgPrice()
			+ ";Position.Quantity=" + Position.Quantity);

			Order tlslOrder = tradeObj.TrailingSLOrder.TLSLOrder;
			bool isPrcValid = isTLSLPriceValid();
			try{
				if(tlslOrder == null || !tlslOrder.Name.Equals(tradeObj.trailingSLSignalName)) {
					if(isPrcValid)
						SubmitOrderUnmanaged(0, GetExitOrderAction(), OrderType.StopMarket, Position.Quantity,
						0, tradeObj.trailingSLPrice, String.Empty, tradeObj.trailingSLSignalName);
					else {
						CloseAllPositions();
					}
				}
				else if(tlslOrder != null && tlslOrder.Name.Equals(tradeObj.trailingSLSignalName)
					&& tlslOrder.StopPrice != tradeObj.trailingSLPrice
					&& isPrcValid) {
					ChangeOrder(tlslOrder, tlslOrder.Quantity, 0, tradeObj.trailingSLPrice);
				}
				if(!isPrcValid) {
					throw new Exception("isTLSLPriceValid false"
					+ ";trailingSLPrice=" + tradeObj.trailingSLPrice
					+ ";Close[0]=" + Close[0]);
				}
			} catch(Exception ex) {
				throw new Exception("Ex SetTrailingStopLossOrderUM:" + ex.Message);
			}
		}
		
		public virtual void SetSimpleExitOCO(string sigName) {
			if(!isOcoPriceValid())
				throw new Exception("Invalid OCO price:"
				+ "stopLossPrice=" + tradeObj.stopLossPrice
				+ ";profitTargetPrice=" + tradeObj.profitTargetPrice);
			if(IsUnmanaged) {
				SetSimpleExitOCOUM();
				return;
			}
			int prtLevel = 0;
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			Print(CurrentBar + ": SetSimpleExitOCO-" 
			+ sigName + "-avg=" + Position.AveragePrice);
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			tradeObj.stopLossAmt = MM_StopLossAmt;
			tradeObj.profitTargetAmt = MM_ProfitTargetAmt;
			tradeObj.SLCalculationMode = CalculationMode.Currency;
			tradeObj.PTCalculationMode = CalculationMode.Currency;
			SetStopLossOrder(sigName);
			SetProfitTargetOrder(sigName);
		}
		
		/// <summary>
		/// Set OCO for exit;
		/// </summary>
		public virtual void SetSimpleExitOCOUM() {
			int prtLevel = 0;
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			Print(CurrentBar + ": SetExitOcoUM;" 
			+ ";avgPrc=" + Position.AveragePrice
			+ ";ocoID=" + tradeObj.ocoID
			+ ";Position.Quantity=" + Position.Quantity
			);
			Order slOrd = tradeObj.BracketOrder.OCOOrder.StopLossOrder;
			Order ptOrd = tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder;
			if(tradeObj.ocoID == null || (slOrd == null && ptOrd == null) 
				|| (slOrd.OrderState != OrderState.Working && ptOrd.OrderState != OrderState.Working))
				tradeObj.ocoID = GetNewOCOId();
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			SetProfitTargetOrderUM();
			SetStopLossOrderUM();
		}		
		//SetParabolicStop(string fromEntrySignal, CalculationMode mode, double value, bool isSimulatedStop, double acceleration, double accelerationMax, double accelerationStep)
		#endregion
		
		public virtual bool CloseAllPositions() 
		{
			//giParabSAR.PrintLog(true, !backTest, log_file, "CloseAllPosition called");
			if(Position.MarketPosition == MarketPosition.Long) {
				if(IsUnmanaged)
					SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Market, Position.Quantity);
				else
					ExitLong();
			}
			if(Position.MarketPosition == MarketPosition.Short) {
				if(IsUnmanaged)
					SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Market, Position.Quantity);
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
			if(tradeObj.BracketOrder.EntryOrder != null) {
				CancelOrder(tradeObj.BracketOrder.EntryOrder);
				tradeObj.BracketOrder.EntryOrder = null;
			}
			return true;
		}
		
		public virtual bool CancelExitOrders()
		{
			indicatorProxy.PrintLog(true, !backTest, CurrentBar + "- CancelExitOrders called");
			CancelExitOCO();
			CancelTrailingStopLoss();
			return true;
		}
		
		public virtual bool CancelExitOCO()
		{
			//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "- CancelAllOrders called");
			if(tradeObj.BracketOrder.OCOOrder.StopLossOrder != null)
				CancelOrder(tradeObj.BracketOrder.OCOOrder.StopLossOrder);
			if(tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder != null)
				CancelOrder(tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder);		
			return true;
		}

		public virtual bool CancelTrailingStopLoss()
		{
			if(tradeObj.TrailingSLOrder.TLSLOrder != null)
				CancelOrder(tradeObj.TrailingSLOrder.TLSLOrder);
			return true;
		}		
		#endregion
		
		#region Event Handlers
		
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
			if(BarsInProgress !=0) return;
			
			Print(CurrentBar + ":OnExecutionUpdate-IsUnmanaged=" + IsUnmanaged);
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
			
			Print(CurrentBar + ":OnExecutionUpdateMG-quant,mktPos,prc, AvgPrc, BarsSinceEx,BarsSinceEn=" 
			+ quantity + "," + marketPosition + "," + price + "," + Position.AveragePrice + ","
			+ bsx + "," + bse
			+ ",SL=" + tradeObj.stopLossPrice
			+ ",Ordername=" + GetOrderName(execution.Order.Name));
			
			// Remember to check the underlying IOrder object for null before trying to access its properties
			if (execution.Order != null && execution.Order.OrderState == OrderState.Filled) {
				if(Position.Quantity != 0) {
					//SetEntryOrder(OrderSignalName.EntryShort, execution.Order);					
					CalProfitTargetAmt(price, tradeObj.profitFactor);
					CalExitOcoPrice(GetAvgPrice(), tradeObj.profitFactor);
					SetSimpleExitOCOUM();

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
			
			Print(CurrentBar + ":OnExecutionUpdateUM"
			+ ";price=" + price
			+ ";Ordername=" + GetOrderName(execution.Order.Name)
			+ ";entrySignalName=" + tradeObj.entrySignalName
			+ ";ocoID=" + tradeObj.ocoID
			+ ";OCO=" + execution.Order.Oco			
			+ ";bsx=" + bsx
			+ ";bse=" + bse
			+ ";quant=" + quantity
			+ ";mktPos=" + marketPosition);
			
			// Remember to check the underlying IOrder object for null before trying to access its properties
			if (execution.Order != null) {
				string oName = GetOrderName(execution.Order.Name);
				OrderState oState = execution.Order.OrderState;
				if(Position.Quantity != 0) {
					if(oState == OrderState.Filled 
						&& oName.Equals(tradeObj.entrySignalName)) {
					//SetEntryOrder(OrderSignalName.EntryShort, execution.Order);
						tradeObj.BracketOrder.EntryOrder = execution.Order;
						tradeObj.TrailingSLOrder.EntryOrder = execution.Order;
						CalProfitTargetAmt(price, tradeObj.profitFactor);
						CalExitOcoPrice(GetAvgPrice(), tradeObj.profitFactor);
						SetSimpleExitOCOUM();
					}
				}
				else if (oName.Equals(OrderSignalName.Exitonsessionclose.ToString())) {
					CancelExitOrders();
				}
			}
		}

		protected override void OnOrderUpdate(Cbi.Order order, double limitPrice, double stopPrice, 
			int quantity, int filled, double averageFillPrice, 
			Cbi.OrderState orderState, DateTime time, Cbi.ErrorCode error, string comment)
		{
			if(BarsInProgress !=0) return;
			
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
			
			Print(CurrentBar + ":OnOrderUpdateMG name-" + GetOrderName(order.Name) + "-" + order.FromEntrySignal + ";" + order.OrderTypeString
			+ ";" + order.OrderState.ToString() + ";" + order.OrderAction.ToString()
			+ ";SP=" + order.StopPrice + ";LP=" + order.LimitPrice
			+ "; BarsSinceExit, BarsSinceEntry=" + bsx + "," + bse);
			
			GetBracketOrderSubType(order);
			
			indicatorProxy.TraceMessage(this.Name, prtLevel);
		    if (tradeObj.BracketOrder.EntryOrder != null && tradeObj.BracketOrder.EntryOrder == order)
		    {
				//giParabSAR.PrintLog(true, !backTest, log_file, order.ToString() + "--" + order.OrderState);
		        if (order.OrderState == OrderState.Cancelled || 
					order.OrderState == OrderState.Filled || 
					order.OrderState == OrderState.Rejected || 
					order.OrderState == OrderState.Unknown)
				{
					tradeObj.barsSinceEnOrd = 0;
					tradeObj.BracketOrder.EntryOrder = null;
				}
		    }
			
			indicatorProxy.TraceMessage(this.Name, prtLevel);
		    if (//tradeObj.BracketOrder.OCOOrder != null &&
				tradeObj.BracketOrder.OCOOrder.StopLossOrder != null &&
				tradeObj.BracketOrder.OCOOrder.StopLossOrder == order)
		    {
				//giParabSAR.PrintLog(true, !backTest, log_file, order.ToString() + "--" + order.OrderState);
		        if (order.OrderState == OrderState.Cancelled || 
					order.OrderState == OrderState.Filled || 
					order.OrderState == OrderState.Rejected)
				{
					tradeObj.BracketOrder.OCOOrder.StopLossOrder = null;
				}
		    }
			
			indicatorProxy.TraceMessage(this.Name, prtLevel);
		    if (//tradeObj.BracketOrder.OCOOrder != null &&
				tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder != null &&
				tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder == order)
		    {
				//giParabSAR.PrintLog(true, !backTest, log_file, order.ToString() + "--" + order.OrderState);
		        if (order.OrderState == OrderState.Cancelled || 
					order.OrderState == OrderState.Filled || 
					order.OrderState == OrderState.Rejected)
				{
					tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder = null;
				}
		    }
			
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			if (order.OrderState == OrderState.Working){// || order.OrderType == OrderType.StopMarket) {
				if(GetOrderName(order.Name) == tradeObj.entrySignalName) //OrderSignalName.EntryLong.ToString() ||
					//GetOrderName(order.Name) == OrderSignalName.EntryShort.ToString()) 
				{
					indicatorProxy.PrintLog(true, !BackTest, "Entry Order Name=" + GetOrderName(order.Name));
					tradeObj.BracketOrder.EntryOrder = order;
				}
				if(GetOrderName(order.Name) == OrderSignalName.Profittarget.ToString()) {
					indicatorProxy.PrintLog(true, !BackTest, "order.Name == OrderSignalName.Profittarget");
					tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder = order;
				}				
				//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + order.ToString());
			}
			
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			if(GetOrderName(order.Name) == OrderSignalName.Stoploss.ToString() &&
				(order.OrderState == OrderState.Accepted || order.OrderState == OrderState.Working)) {
				tradeObj.BracketOrder.OCOOrder.StopLossOrder = order;
			}
			
//			if(tradeObj.BracketOrder.OCOOrder.StopLossOrder == null && tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder == null)
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
			
			Print(CurrentBar + ":OnOrderUpdateUM name=" + oName
			+ ";Type=" + order.OrderTypeString
			+ ";SP=" + order.StopPrice 
			+ ";LP=" + order.LimitPrice			
			+ ";State=" + orderState.ToString()
			+ ";Action=" + order.OrderAction.ToString()
			+ ";ocoID=" + tradeObj.ocoID
			+ ";OCO=" + order.Oco
			+ ";BarsSinceExit=" + bsx
			+ ";BarsSinceEntry=" + bse
			+ ";FromEntrySignal=" + order.FromEntrySignal);
			
			GetBracketOrderSubType(order);
						
			indicatorProxy.TraceMessage(this.Name, prtLevel);
		    if (tradeObj.BracketOrder.EntryOrder != null && tradeObj.BracketOrder.EntryOrder == order)
		    {
				if (orderState == OrderState.Cancelled ||
					//order.OrderState == OrderState.Filled ||
					orderState == OrderState.Rejected ||
					orderState == OrderState.Unknown)
				{
					tradeObj.barsSinceEnOrd = 0;
					tradeObj.BracketOrder.EntryOrder = null;
				}
		    }
			
			indicatorProxy.TraceMessage(this.Name, prtLevel);
		    if (tradeObj.TrailingSLOrder.EntryOrder != null && tradeObj.TrailingSLOrder.EntryOrder == order)
		    {
				if (orderState == OrderState.Cancelled || 
					orderState == OrderState.Rejected || 
					orderState == OrderState.Unknown)
				{
					tradeObj.barsSinceEnOrd = 0;
					tradeObj.TrailingSLOrder.EntryOrder = null;
				}
		    }
			
			indicatorProxy.TraceMessage(this.Name, prtLevel);
		    if (tradeObj.BracketOrder.OCOOrder.StopLossOrder != null &&
				tradeObj.BracketOrder.OCOOrder.StopLossOrder == order)
		    {				
		        if (orderState == OrderState.Cancelled || 
					orderState == OrderState.Filled || 
					orderState == OrderState.Rejected)
				{
					tradeObj.BracketOrder.OCOOrder.StopLossOrder = null;
					ClearOrderName(oName);
				}
		    }
			
			indicatorProxy.TraceMessage(this.Name, prtLevel);
		    if (tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder != null &&
				tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder == order)
		    {
				if (orderState == OrderState.Cancelled || 
					orderState == OrderState.Filled || 
					orderState == OrderState.Rejected)
				{
					tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder = null;
					ClearOrderName(oName);
				}
		    }

			indicatorProxy.TraceMessage(this.Name, prtLevel);
		    if ( tradeObj.TrailingSLOrder.TLSLOrder != null && tradeObj.TrailingSLOrder.TLSLOrder == order)
		    {				
		        if (orderState == OrderState.Cancelled || 
					orderState == OrderState.Filled || 
					orderState == OrderState.Rejected)
				{
					tradeObj.TrailingSLOrder.TLSLOrder = null;
					ClearOrderName(oName);
				}
		    }
			
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			if (orderState == OrderState.Working || orderState == OrderState.Accepted) {
				if(oName.Equals(tradeObj.entrySignalName)) {
					indicatorProxy.PrintLog(true, !BackTest, "Entry Order Name=" + oName);
					tradeObj.BracketOrder.EntryOrder = order;
					tradeObj.TrailingSLOrder.EntryOrder = order;
				}
				
				if(order.Oco != null && order.Oco.Equals(tradeObj.ocoID)){
					indicatorProxy.PrintLog(true, !BackTest, "Exit order.Name=" + oName);
					if(oName.Equals(tradeObj.profitTargetSignalName)) {						
						tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder = order;
					}
					if(oName.Equals(tradeObj.stopLossSignalName)) {						
						tradeObj.BracketOrder.OCOOrder.StopLossOrder = order;
					}
				}
				
				if(oName.Equals(tradeObj.trailingSLSignalName)) {
					indicatorProxy.PrintLog(true, !BackTest, "TLSL Order Name=" + oName);
					tradeObj.TrailingSLOrder.TLSLOrder = order;
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
			tradeObj.entrySignalName = nameTag + "-" + timeStr;
			SetNewOcoOrderSignalName(nameTag, timeStr);
			SetNewTLSLOrderSignalName(nameTag, timeStr);
			return tradeObj.entrySignalName;
		}
				
		/// <summary>
		/// Bracket order Signal name with timestamp:
		/// EntryShort-SL-201905312359888
		/// </summary>
		/// <param name="nameTag"></param>
		/// <returns></returns>
		public void SetNewOcoOrderSignalName(string nameTag, string timeStr) {
			tradeObj.stopLossSignalName = nameTag + "-SL-" + timeStr;
			tradeObj.profitTargetSignalName = nameTag + "-PT-" + timeStr;			
		}

		/// <summary>
		/// TrailingSL order Signal name with timestamp:
		/// EntryShort-TLSL-201905312359888
		/// </summary>
		/// <param name="nameTag"></param>
		/// <returns></returns>
		public void SetNewTLSLOrderSignalName(string nameTag, string timeStr) {
			tradeObj.trailingSLSignalName = nameTag + "-TLSL-" + timeStr;
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
			else if(oName.Equals(tradeObj.entrySignalName)) tradeObj.entrySignalName = String.Empty;
			else if(oName.Equals(tradeObj.stopLossSignalName)) {
				tradeObj.stopLossSignalName = String.Empty;
				tradeObj.ocoID = String.Empty;
			}
			else if(oName.Equals(tradeObj.profitTargetSignalName)) {
				tradeObj.profitTargetSignalName = String.Empty;
				tradeObj.ocoID = String.Empty;
			}
			else if(oName.Equals(tradeObj.trailingSLSignalName)) tradeObj.trailingSLSignalName = String.Empty;
		}
		
		/// <summary>
		/// Get the subType of the bracket order: entry, SL or PT
		/// </summary>
		/// <param name="order"></param>
		/// <returns></returns>
		public BracketOrderSubType GetBracketOrderSubType(Order order) {
			BracketOrderSubType bost = BracketOrderSubType.UnKnown;
			if(order != null) {
				indicatorProxy.PrintLog(true, !BackTest, "GetBracketOrderSubType:" +
				order.Name + "," + order.OrderTypeString);
			}
			return bost;
		}
		
		public OrderAction GetExitOrderAction() {
			OrderAction oAct;
			if(Position.MarketPosition==MarketPosition.Short)
				oAct = OrderAction.BuyToCover;
			else if(Position.MarketPosition==MarketPosition.Long)
				oAct = OrderAction.Sell;
			else throw new Exception("GetExitOrderAction error MarketPosition=" + Position.MarketPosition);
			return oAct;
		}
		
		#endregion

		#region TM Properties
		
		//Only types which can be Xml Serialized should be marked as a NinjaScriptAttribute,
		//otherwise you may run into errors when persisting values in various scenarios 
		//(e.g., saving workspace, or running Strategy Optimizations). 
        [Description("Short, Long or both directions for entry")]
 		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TradingDirection", GroupName = "TradeMgmt", Order = 0)]		
        public TradingDirection TM_TradingDirection
        {
            get { return tm_TradingDirection; }
            set { tm_TradingDirection = value; }
        }

        [Description("Trading style: trend following, counter trend, scalp")]
 		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TradingStyle", GroupName = "TradeMgmt", Order = 1)]
        public TradingStyle TM_TradingStyle
        {
            get { return tm_TradingStyle; }
            set { tm_TradingStyle = value; }
        }
		
		[Description("Use trailing entry every bar")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EnTrailing", GroupName = "TradeMgmt", Order = 2)]
        public bool TM_EnTrailing
        {
            get{return tm_EnTrailing;}
            set{tm_EnTrailing = value;}
        }
		
        [Description("Offeset points for limit price entry")]
		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EnOffsetPnts", GroupName = "TradeMgmt", Order = 3)]
        public double TM_EnOffsetPnts
        {
            get{return tm_EnOffsetPnts;}
            set{tm_EnOffsetPnts = Math.Max(0, value);}
        }
		
        [Description("How long to check entry order filled or not")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MinutesChkEnOrder", GroupName = "TradeMgmt", Order = 4)]
        public int TM_MinutesChkEnOrder
        {
            get{return tm_MinutesChkEnOrder;}
            set{tm_MinutesChkEnOrder = Math.Max(0, value);}
        }
		
        [Description("How long to check P&L")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MinutesChkPnL", GroupName = "TradeMgmt", Order = 5)]
        public int TM_MinutesChkPnL
        {
            get{return tm_MinutesChkPnL;}
            set{tm_MinutesChkPnL = Math.Max(0, value);}
        }
		
		[Description("Bar count before checking P&L")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarsToCheckPL", GroupName = "TradeMgmt", Order = 6)]
        public int TM_BarsToCheckPnL
        {
            get{return tm_BarsToCheckPnL;}
            set{tm_BarsToCheckPnL = Math.Max(1, value);}
        }
		
        [Description("How many bars to hold entry order before cancel it")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarsHoldEnOrd", GroupName = "TradeMgmt", Order = 7)]
        public int TM_BarsHoldEnOrd
        {
            get{return tm_BarsHoldEnOrd;}
            set{tm_BarsHoldEnOrd = Math.Max(1, value);}
        }
		
        [Description("Bar count for en order counter pullback")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EnCounterPBBars", GroupName = "TradeMgmt", Order = 8)]
        public int TM_EnCounterPBBars
        {
            get{return tm_EnCounterPBBars;}
            set{tm_EnCounterPBBars = Math.Max(1, value);}
        }
				
		[Description("Bar count since last filled PT or SL")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarsSincePTSL", GroupName = "TradeMgmt", Order = 9)]
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
