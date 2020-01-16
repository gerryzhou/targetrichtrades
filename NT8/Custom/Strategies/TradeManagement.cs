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
//				SetProfitTarget(CalculationMode.Currency, MM_ProfitTargetAmt);
//	            SetStopLoss(CalculationMode.Currency, MM_StopLossAmt);
//			} else {
//				SetProfitTarget(CalculationMode.Currency, MM_ProfitTargetAmt);
//	            SetStopLoss(CalculationMode.Currency, MM_StopLossAmt);
//			}
		}
		
		public virtual void PutTrade() {
			IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + 
				"::PutTrade()--" + this.ToString());			
		}
		
		public virtual void PutEntryTrade() {
			IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				CurrentBar + ": PutEntryTrade called");
			TradeAction ta = CurrentTrade.TradeAction;
			if( ta == null || ta.EntrySignal == null ||
				ta.EntrySignal.BarNo != CurrentBar)
				return;

			switch(ta.TradeActionType) {
				case TradeActionType.EntrySimple:
				case TradeActionType.Bracket:
					NewEntrySimpleOrder();
					break;
			}
		}
		
		public virtual void PutExitTrade() {
			IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				CurrentBar + ": PutExitTrade called");
			TradeAction ta = CurrentTrade.TradeAction;
			if( ta == null )
				return;
			
			switch(ta.TradeActionType) {
				case TradeActionType.ExitOCO:
					SetSimpleExitOCO();
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
		/// Check if new entry trade could be generated
		/// </summary>
		/// <returns></returns>
		public virtual bool CheckNewEntryTrade() {
			IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				CurrentBar + "::=======Virtual CheckNewEntryTrade()===========" + this.ToString());
			return false;
		}

		public virtual bool CheckEnOrder(double cur_gap)
        {
            double min_en = -1;

            if (CurrentTrade.BracketOrder.EntryOrder != null && CurrentTrade.BracketOrder.EntryOrder.OrderState == OrderState.Working)
            {
                min_en = IndicatorProxy.GetMinutesDiff(CurrentTrade.BracketOrder.EntryOrder.Time, Time[0]);// DateTime.Now);
                //if ( IsTwoBarReversal(cur_gap, TickSize, enCounterPBBars) || (barsHoldEnOrd > 0 && barsSinceEnOrd >= barsHoldEnOrd) || ( minutesChkEnOrder > 0 &&  min_en >= minutesChkEnOrder))
				if ((TM_BarsHoldEnOrd > 0 && CurrentTrade.barsSinceEnOrd >= TM_BarsHoldEnOrd) || ( TM_MinutesChkEnOrder > 0 &&  min_en >= TM_MinutesChkEnOrder))	
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
					prc = (TM_EnTrailing && TM_EnCounterPBBars>0) ? Close[0]-TM_EnOffsetPnts :  Low[0]-TM_EnOffsetPnts;
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
		
		public virtual bool NewTradeAllowed()
		{
			if(BarsInProgress !=0) return false;
			
			SetPrintOut(-1);
			IndicatorProxy.TraceMessage(this.Name, PrintOut);
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
			if(IsLiveTrading() && (plrt <= MM_DailyLossLmt || pnl_daily <= MM_DailyLossLmt))
			{
				IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + "-" + AccName 
				+ ": DailyLossLmt reached = " + pnl_daily + "," + plrt
				+ "::" + this.Name);
				
				return false;
			}
			
			
			//indicatorProxy.TraceMessage(this.Name, prtLevel);
			if (!IsLiveTrading() && pnl_daily <= MM_DailyLossLmt) 
			{
				IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + "-" + AccName 
				+ ": DailyLossLmt reached = " + pnl_daily + "," + plrt
				+ "::" + this.Name);
				
				return false;				
			}
		
			
			//indicatorProxy.TraceMessage(this.Name, prtLevel);
			if (IsTradingTime(170000) && HasPosition() == 0)
			{
				if (CurrentTrade.BracketOrder.EntryOrder == null || CurrentTrade.BracketOrder.EntryOrder.OrderState != OrderState.Working || TM_EnTrailing)
				{					
					if(bsx < 0 || bsx > TM_BarsSincePTSL) //if(bsx == -1 || bsx > TM_BarsSincePTSL)
					{
						IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar 
						+ "-bsx,bse,TM_BarsSincePTSL=" + bsx + "," + bse + "," + TM_BarsSincePTSL
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
			//CurrentTrade.TradeAction.EntrySignal.SignalName = GetNewEnOrderSignalName(OrderSignalName.EntryLongLmt.ToString());
			IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":NewLongLimitOrderUM"
			+";CurrentTrade.TradeAction.EntrySignal.SignalName=" + CurrentTrade.TradeAction.EntrySignal.SignalName);
			SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.Limit, 
			CurrentTrade.TradeAction.EntrySignal.Quantity, CurrentTrade.TradeAction.EntryPrice, 0, "", CurrentTrade.TradeAction.EntrySignal.SignalName);
			
			CurrentTrade.barsSinceEnOrd = 0;
		}
		
		/// <summary>
		/// Submit unmanaged short limit order, set the order object in OnOrderUpdate handler
		/// </summary>
		/// <param name="msg"></param>
		public virtual void NewShortLimitOrderUM(string msg)
		{
			//CurrentTrade.TradeAction.EntrySignal.SignalName = GetNewEnOrderSignalName(OrderSignalName.EntryShortLmt.ToString());
			IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":NewShortLimitOrderUM"
			+";CurrentTrade.TradeAction.EntrySignal.SignalName=" + CurrentTrade.TradeAction.EntrySignal.SignalName);
			SubmitOrderUnmanaged(0, OrderAction.SellShort, OrderType.Limit,
			CurrentTrade.TradeAction.EntrySignal.Quantity, CurrentTrade.TradeAction.EntryPrice, 0, "", CurrentTrade.TradeAction.EntrySignal.SignalName);
			
			//double prc = (TM_EnTrailing && TM_EnCounterPBBars>0) ? Close[0]+TM_EnOffsetPnts : High[0]+TM_EnOffsetPnts;
			
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
			double prc = (TM_EnTrailing && TM_EnCounterPBBars>0) ? Close[0]-TM_EnOffsetPnts :  Low[0]-TM_EnOffsetPnts;
			
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
			double prc = GetEnLongPrice();//(MM_EnTrailing && TM_EnCounterPBBars>0) ? Close[0]-TM_EnOffsetPnts :  Low[0]-TM_EnOffsetPnts;
			
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
		#endregion
			
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
		
		/// <summary>
		/// Submit unmanaged entry simple order, set the order object in OnOrderUpdate handler
		/// </summary>
		public virtual void NewEntrySimpleOrderUM() {
			TradeSignal tSig = CurrentTrade.TradeAction.EntrySignal;
			try {
				//tSig.SignalName = GetNewEnOrderSignalName(OrderSignalName.EntryLongLmt.ToString());
				IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":NewEntrySimpleOrderUM"
				+";EntrySignal.SignalName=" + tSig.SignalName
				+";EntrySignal.Action=" + tSig.Action.ToString()
				+";EntrySignal.OrderType=" + tSig.Order_Type.ToString()
				+";EntrySignal.Quantity=" + tSig.Quantity
				+";EntrySignal.OrderCalculationMode=" + tSig.OrderCalculationMode.ToString()
				+";EntrySignal.LimitPrice=" + tSig.LimitPrice
				+";EntrySignal.StopPrice=" + tSig.StopPrice);

				SubmitOrderUnmanaged(0, tSig.Action, tSig.Order_Type, tSig.Quantity,
				tSig.LimitPrice, tSig.StopPrice, "", tSig.SignalName);
				
				CurrentTrade.barsSinceEnOrd = 0;
			} catch(Exception ex) {
				IndicatorProxy.PrintLog(true, IsLiveTrading(), String.Format("{0}:NewEntrySimpleOrderUM EX={1}", CurrentBar, ex.StackTrace));
			}
		}
		
		#endregion
		
		#region Exit Order functions
		
		public virtual void SetProfitTargetOrder(string sigName) {
			if(IsUnmanaged) {
				SetProfitTargetOrderUM();
				return;
			}
			IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":SetProfitTargetOrder-" 
			+ sigName + "-" + MM_PTCalculationMode 
			+ "-profitTargetAmt=" + MM_ProfitTargetAmt
			+ "-profitTargetTic" + MM_ProfitTgtTic
			+ "-profitTargetPrice" + CurrentTrade.TradeAction.ProfitTargetPrice + "-avg=" + GetAvgPrice());
			try{
			switch(MM_PTCalculationMode) {
				case CalculationMode.Currency :
					SetProfitTarget(sigName, CalculationMode.Currency, MM_ProfitTargetAmt);
					break;
				case CalculationMode.Price :
					SetProfitTarget(sigName, CalculationMode.Price, CurrentTrade.TradeAction.ProfitTargetPrice);
					break;
				case CalculationMode.Ticks :
					SetProfitTarget(sigName, CalculationMode.Ticks, MM_ProfitTgtTic);
					break;
				default: 
					SetProfitTarget(sigName, CalculationMode.Currency, MM_ProfitTargetAmt);
					break;
			}
			} catch(Exception ex) {
				throw new Exception("Ex SetProfitTarget:" + ex.Message);
			}
		}

		public virtual void SetProfitTargetOrderUM() {
			TradeAction ta = CurrentTrade.TradeAction;
			if(ta == null || ta.ProfitTargetSignal == null) {
				IndicatorProxy.PrintLog(true, IsLiveTrading(),
					CurrentBar + ":SetProfitTargetOrderUM ta==null or ProfitTargetSignal==null"); 
				return;
			}
			IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":SetProfitTargetOrderUM"
			+ ";CurrentTrade.TradeAction.ProfitTargetSignal.SignalName=" + ta.ProfitTargetSignal.SignalName
			+ ";CurrentTrade.TDID=" + CurrentTrade.TradeID
			+ ";CurrentTrade.OcoID=" + CurrentTrade.OcoID
			+ ";GetExitOrderAction()=" + GetExitOrderAction().ToString()
			+ ";profitTargetAmt=" + MM_ProfitTargetAmt
			+ ";profitTargetTic=" + MM_ProfitTgtTic
			+ ";profitTargetPrice=" + ta.ProfitTargetPrice
			+ ";avgPrc=" + GetAvgPrice()
			+ ";HasPosition()=" + HasPosition());
			Order ptOrder = CurrentTrade.BracketOrder.OCOOrder.ProfitTargetOrder;
			
			try{
				if(ptOrder == null || !ptOrder.Oco.Equals(CurrentTrade.OcoID)) {
//					SubmitOrderUnmanaged(0, GetExitOrderAction(), OrderType.Limit, CurrentTrade.PosQuantity,
//					CurrentTrade.TradeAction.ProfitTargetPrice, 0, CurrentTrade.OcoID, CurrentTrade.TradeAction.ProfitTargetSignal.SignalName);
					SubmitOrderUnmanaged(0, ta.ProfitTargetSignal.Action, OrderType.Limit, CurrentTrade.PosQuantity,
					ta.ProfitTargetPrice, 0, CurrentTrade.OcoID, ta.ProfitTargetSignal.SignalName);
				}
				else if(ptOrder != null && ptOrder.Oco.Equals(CurrentTrade.OcoID)
					&& ptOrder.LimitPrice != ta.ProfitTargetPrice) {
					ChangeOrder(ptOrder, ptOrder.Quantity, ta.ProfitTargetPrice, 0);
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
			IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":SetStopLossOrder"
			+ ";CurrentTrade.TradeAction.EntrySignal.SignalName=" + CurrentTrade.TradeAction.EntrySignal.SignalName
			+ ";CurrentTrade.TDID=" + CurrentTrade.TradeID
			+ ";CurrentTrade.OcoID=" + CurrentTrade.OcoID
			+ ";MM_SLCalculationMode=" + MM_SLCalculationMode
			+ ";stopLossAmt=" + MM_StopLossAmt
			+ ";stopLossTic=" + MM_StopLossTic
			+ ";stopLossPrice=" + CurrentTrade.TradeAction.StopLossPrice
			+ ";avgPrc=" + GetAvgPrice());
			try {
			switch(MM_SLCalculationMode) {
				case CalculationMode.Currency :
					SetStopLoss(sigName, CalculationMode.Currency, MM_StopLossAmt, true);
					break;
				case CalculationMode.Price :
					SetStopLoss(sigName, CalculationMode.Price, CurrentTrade.TradeAction.StopLossPrice, true);
					break;
				case CalculationMode.Ticks :
					SetStopLoss(sigName, CalculationMode.Ticks, MM_StopLossTic, true);
					break;
				default: 
					SetStopLoss(sigName, CalculationMode.Currency, MM_StopLossAmt, true);
					break;
			}
			} catch(Exception ex) {
				throw new Exception("Ex SetStopLossOrder:" + ex.Message);
			}
		}
		
		public virtual void SetStopLossOrderUM() {
			TradeAction ta = CurrentTrade.TradeAction;
			if(ta == null || ta.StopLossSignal == null) {
				IndicatorProxy.PrintLog(true, IsLiveTrading(),
					CurrentBar + ":SetStopLossOrderUM ta==null or StopLossSignal==null"); 
				return;
			}
			IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":SetStopLossOrderUM" 
			+ ";CurrentTrade.TradeAction.StopLossSignal.SignalName=" + ta.StopLossSignal.SignalName
			+ ";CurrentTrade.TDID=" + CurrentTrade.TradeID
			+ ";CurrentTrade.OcoID=" + CurrentTrade.OcoID
			+ ";GetExitOrderAction()=" + GetExitOrderAction().ToString()			
			+ ";MM_SLCalculationMode=" + MM_SLCalculationMode
			+ ";stopLossAmt=" + MM_StopLossAmt
			+ ";stopLossTic=" + MM_StopLossTic
			+ ";stopLossPrice=" + ta.StopLossPrice
			+ ";avgPrc=" + GetAvgPrice()
			+ ";Position.Quantity=" + HasPosition());
			
			Order slOrder = CurrentTrade.BracketOrder.OCOOrder.StopLossOrder;
			try{
				if(slOrder == null || !slOrder.Oco.Equals(CurrentTrade.OcoID)) {
//					SubmitOrderUnmanaged(0, GetExitOrderAction(), OrderType.StopMarket, CurrentTrade.MaxQuantity,
//					0, CurrentTrade.TradeAction.StopLossPrice, CurrentTrade.OcoID, CurrentTrade.TradeAction.StopLossSignal.SignalName);
					SubmitOrderUnmanaged(0, ta.StopLossSignal.Action, OrderType.StopMarket, CurrentTrade.PosQuantity,
					0, ta.StopLossPrice, CurrentTrade.OcoID, ta.StopLossSignal.SignalName);
				}
				else if(slOrder != null && slOrder.Oco.Equals(CurrentTrade.OcoID)
					&& slOrder.StopPrice != ta.StopLossPrice) {
					ChangeOrder(slOrder, slOrder.Quantity, 0, ta.StopLossPrice);
				}
			} catch(Exception ex) {
				throw new Exception("Ex SetStopLossOrderUM:" + ex.Message);
			}
		}
		
		public virtual void SetSimpleExitOCO() {
			TradeAction ta = CurrentTrade.TradeAction;
			try {
			if(!isOcoPriceValid()) {
				IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + "Invalid OCO price:"
				+ "stopLossPrice=" + ta.StopLossPrice
				+ ";profitTargetPrice=" + ta.ProfitTargetPrice);
				
				ta = GetOcoPrice(-1, ta);
				IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + "GetOcoPrice:"
				+ "stopLossPrice=" + ta.StopLossPrice
				+ ";profitTargetPrice=" + ta.ProfitTargetPrice);
			}
			if(IsUnmanaged) {
				SetSimpleExitOCOUM();
				return;
			}
			int prtLevel = 0;
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
			IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ": SetSimpleExitOCO-" 
			+ ta.EntrySignal.SignalName + "-avg=" + GetAvgPrice());
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
			MM_StopLossAmt = MM_StopLossAmt;
			MM_ProfitTargetAmt = MM_ProfitTargetAmt;
			MM_SLCalculationMode = CalculationMode.Currency;
			MM_PTCalculationMode = CalculationMode.Currency;
			SetStopLossOrder(ta.StopLossSignal.SignalName);
			SetProfitTargetOrder(ta.ProfitTargetSignal.SignalName);
			} catch (Exception ex) {
				IndicatorProxy.PrintLog(true, IsLiveTrading(),
					CurrentBar + ": SetSimpleExitOCO Exception - " + ex.Message); 
			}
		}
		
		/// <summary>
		/// Set OCO for exit;
		/// </summary>
		public virtual void SetSimpleExitOCOUM() {
			int prtLevel = 0;
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
			IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ": SetSimpleExitOCOUM;" 
			+ ";avgPrc=" + GetAvgPrice()
			+ ";CurrentTrade.TDID=" + CurrentTrade.TradeID
			+ ";CurrentTrade.OcoID=" + CurrentTrade.OcoID
			+ ";HasPosition()=" + HasPosition()
			);
			Order slOrd = CurrentTrade.BracketOrder.OCOOrder.StopLossOrder;
			Order ptOrd = CurrentTrade.BracketOrder.OCOOrder.ProfitTargetOrder;
//			if(CurrentTrade.OcoID == null || (slOrd == null && ptOrd == null) 
//				|| (slOrd.OrderState != OrderState.Working && ptOrd.OrderState != OrderState.Working))
//				CurrentTrade.OcoID = GetNewOcoID();
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
			SetStopLossOrderUM();
			SetProfitTargetOrderUM();
		}
		
		/// <summary>
		/// Setup breakeven order
		/// </summary>
		public virtual void SetBreakEvenOrder(double avgPrc) {			
			Order stopOrder = CurrentTrade.BracketOrder.OCOOrder.StopLossOrder;
			string tif = stopOrder == null?
				"N/A":stopOrder.TimeInForce.ToString()+",IsLiveUntilCancelled="+stopOrder.IsLiveUntilCancelled;
			MM_StopLossAmt = 0;
			CurrentTrade.TradeAction.StopLossPrice = avgPrc;
			
			if(IsUnmanaged) {
				IndicatorProxy.PrintLog(true, IsLiveTrading(), 
					AccName + "-Setup SL BreakevenUM posAvgPrc=" + avgPrc + "," + tif);				
				SetBreakEvenOrderUM(avgPrc);
				return;
			}
			IndicatorProxy.PrintLog(true, IsLiveTrading(), 
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
			
			IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":SetTrailingStopLossOrder-" 
			+ sigName + "-" + MM_TLSLCalculationMode 
			+ "-trailingSLAmt=" + MM_TrailingStopLossAmt
			+ "-trailingSLTic=" + MM_TrailingStopLossTic + "-avg=" + GetAvgPrice());
			try {
				switch(MM_TLSLCalculationMode) {
					case CalculationMode.Ticks :
						SetTrailStop(sigName, CalculationMode.Ticks, MM_TrailingStopLossTic, true);
						break;
					case CalculationMode.Percent :
						SetTrailStop(sigName, CalculationMode.Percent, MM_TrailingStopLossAmt, true);
						break;
					default: 
						SetTrailStop(sigName, CalculationMode.Ticks, MM_TrailingStopLossTic, true);
						break;
				}
			} catch(Exception ex) {
				throw new Exception("Ex SetTrailingStopLossOrder:" + ex.Message);
			}
		}

		public virtual void SetTrailingStopLossOrderUM() {
			IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":SetTrailingStopLossOrderUM"
			+ ";trailingSLSignalName=" + CurrentTrade.TradeAction.StopLossSignal.SignalName
			+ ";TLSLCalculationMode=" + MM_TLSLCalculationMode
			+ ";trailingPTTic=" + CurrentTrade.TradeAction.TrailingProfitTargetTics
			+ ";trailingSLAmt=" + MM_TrailingStopLossAmt
			+ ";trailingSLTic=" + MM_TrailingStopLossTic 
			+ ";GetAvgPrice=" + GetAvgPrice()
			+ ";Position.Quantity=" + HasPosition());

			Order tlslOrder = CurrentTrade.BracketOrder.TrailingSLOrder.TLSLOrder;
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
		
		//SetParabolicStop(string fromEntrySignal, CalculationMode mode, double value, bool isSimulatedStop, double acceleration, double accelerationMax, double accelerationStep)
		#endregion
		
		#region Cancel & Close Functions
		
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
			IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + "- CancelExitOrders called");
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
			if(CurrentTrade.BracketOrder.TrailingSLOrder.TLSLOrder != null)
				CancelOrder(CurrentTrade.BracketOrder.TrailingSLOrder.TLSLOrder);
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
		/// <summary>
		/// For managed orders approach
		/// </summary>
		public virtual void OnExecutionUpdateMG(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
			if(BarsInProgress !=0) return;
			
			int bsx = BarsSinceExitExecution(0, "", 0);
			int bse = BarsSinceEntryExecution(0, "", 0);
			
			IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":OnExecutionUpdateMG-quant,mktPos,prc, AvgPrc, BarsSinceEx,BarsSinceEn=" 
			+ quantity + "," + marketPosition + "," + price + "," + GetAvgPrice() + ","
			+ bsx + "," + bse
			+ ",SL=" + CurrentTrade.TradeAction.StopLossPrice
			+ ",Ordername=" + GetOrderName(execution.Order.Name));return;
			
			// Remember to check the underlying IOrder object for null before trying to access its properties
			if (execution.Order != null && execution.Order.OrderState == OrderState.Filled) {
				if(HasPosition() != 0) {
					//SetEntryOrder(OrderSignalName.EntryShort, execution.Order);					
					CalProfitTargetAmt(price, MM_ProfitFactor);
					CalExitOcoPrice(GetAvgPrice(), MM_ProfitFactor);
					SetSimpleExitOCO();
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
			
			IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":OnExecutionUpdateUM"
			+ ";price=" + price
			+ ";Ordername=" + GetOrderName(execution.Order.Name)
			+ ";entrySignalName=" + CurrentTrade.TradeAction.EntrySignal.SignalName
			+ ";CurrentTrade.TDID=" + CurrentTrade.TradeID
			+ ";OcoID=" + CurrentTrade.OcoID
			+ ";OCO=" + execution.Order.Oco			
			+ ";bsx=" + bsx
			+ ";bse=" + bse
			+ ";quant=" + quantity
			+ ";HasPosition=" + HasPosition()
			+ ";mktPos=" + marketPosition);return;
			
			// Remember to check the underlying IOrder object for null before trying to access its properties
			if (execution.Order != null) {
				string oName = GetOrderName(execution.Order.Name);
				OrderState oState = execution.Order.OrderState;
				if(marketPosition != MarketPosition.Flat) { //(HasPosition() != 0) {
					if(oState == OrderState.Filled 
						&& oName.Equals(CurrentTrade.TradeAction.EntrySignal.SignalName)) {
					//SetEntryOrder(OrderSignalName.EntryShort, execution.Order);
						CurrentTrade.BracketOrder.EntryOrder = execution.Order;
						CurrentTrade.BracketOrder.TrailingSLOrder.EntryOrder = execution.Order;
					}
				}
				else if (oName.Equals(OrderSignalName.ExitOnSessionClose.ToString())) {
					CancelExitOrders();
				}
			}
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
			
			IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":OnOrderUpdateMG name-" + GetOrderName(order.Name) + "-" + order.FromEntrySignal + ";" + order.OrderTypeString
			+ ";" + order.OrderState.ToString() + ";" + order.OrderAction.ToString()
			+ ";SP=" + order.StopPrice + ";LP=" + order.LimitPrice
			+ "; BarsSinceExit, BarsSinceEntry=" + bsx + "," + bse);
			
			GetBracketOrderSubType(order);
			
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
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
			
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
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
			
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
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
			
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
			if (order.OrderState == OrderState.Working){// || order.OrderType == OrderType.StopMarket) {
				if(GetOrderName(order.Name) == CurrentTrade.TradeAction.EntrySignal.SignalName) //OrderSignalName.EntryLong.ToString() ||
					//GetOrderName(order.Name) == OrderSignalName.EntryShort.ToString()) 
				{
					IndicatorProxy.PrintLog(true, IsLiveTrading(), "Entry Order Name=" + GetOrderName(order.Name));
					CurrentTrade.BracketOrder.EntryOrder = order;
				}
				if(GetOrderName(order.Name) == OrderSignalName.ProfitTarget.ToString()) {
					IndicatorProxy.PrintLog(true, IsLiveTrading(), "order.Name == OrderSignalName.Profittarget");
					CurrentTrade.BracketOrder.OCOOrder.ProfitTargetOrder = order;
				}				
				//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + order.ToString());
			}
			
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
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
			
			IndicatorProxy.PrintLog(true, IsLiveTrading(),
			CurrentBar + ":OnOrderUpdateUM name=" + oName
			+ ";Type=" + order.OrderTypeString
			+ ";SP=" + order.StopPrice 
			+ ";LP=" + order.LimitPrice			
			+ ";State=" + orderState.ToString()
			+ ";Action=" + order.OrderAction.ToString()
			+ ";CurrentTrade.TDID=" + CurrentTrade.TradeID
			+ ";OcoID=" + CurrentTrade.OcoID
			+ ";OCO=" + order.Oco
			+ ";BarsSinceExit=" + bsx
			+ ";BarsSinceEntry=" + bse
			+ ";FromEntrySignal=" + order.FromEntrySignal);
			//return;
			GetBracketOrderSubType(order);
						
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
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
			
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
		    if (CurrentTrade.BracketOrder.TrailingSLOrder.EntryOrder != null && CurrentTrade.BracketOrder.TrailingSLOrder.EntryOrder == order)
		    {
				if (orderState == OrderState.Cancelled || 
					orderState == OrderState.Rejected || 
					orderState == OrderState.Unknown)
				{
					CurrentTrade.barsSinceEnOrd = 0;
					CurrentTrade.BracketOrder.TrailingSLOrder.EntryOrder = null;
				}
		    }
/*			
			//Reset SL order
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
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
			
			//Reset PT order
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
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

			//Reset TLSL order
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
		    if ( CurrentTrade.BracketOrder.TrailingSLOrder.TLSLOrder != null && CurrentTrade.BracketOrder.TrailingSLOrder.TLSLOrder == order)
		    {				
		        if (orderState == OrderState.Cancelled || 
					orderState == OrderState.Filled || 
					orderState == OrderState.Rejected)
				{
					CurrentTrade.BracketOrder.TrailingSLOrder.TLSLOrder = null;
					ClearOrderName(oName);
				}
		    }
			
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
			if (orderState == OrderState.Working || orderState == OrderState.Accepted) {
				//Set En order
				if(oName.Equals(CurrentTrade.TradeAction.EntrySignal.SignalName)) {
					IndicatorProxy.PrintLog(true, IsLiveTrading(), "Entry Order Name=" + oName);
					CurrentTrade.BracketOrder.EntryOrder = order;
					CurrentTrade.BracketOrder.TrailingSLOrder.EntryOrder = order;
				}
				
				//Set SL, PT order
				if(order.Oco != null && order.Oco.Equals(CurrentTrade.OcoID)){
					IndicatorProxy.PrintLog(true, IsLiveTrading(), "Exit order.Name=" + oName);
					if(oName.Equals(CurrentTrade.TradeAction.ProfitTargetSignal.SignalName)) {						
						CurrentTrade.BracketOrder.OCOOrder.ProfitTargetOrder = order;
					}
					if(oName.Equals(CurrentTrade.TradeAction.StopLossSignal.SignalName)) {						
						CurrentTrade.BracketOrder.OCOOrder.StopLossOrder = order;
					}
				}
				
				//Set SLPT order
				if(oName.Equals(CurrentTrade.TradeAction.StopLossSignal.SignalName)) {
					IndicatorProxy.PrintLog(true, IsLiveTrading(), "TLSL Order Name=" + oName);
					CurrentTrade.BracketOrder.TrailingSLOrder.TLSLOrder = order;
				}
			}
			*/
		}
		
		#endregion
		
		#region Order Utilities Functions
		
		/// <summary>
		/// Entry order Signal name with timestamp:
		/// EntryShort-201905312359888
		/// </summary>
		/// <param name="nameTag"></param>
		/// <returns></returns>
		public string GetNewEnOrderSignalName(string nameTag) {
			string timeStr = GetBarTimestampStr(0);
			//CurrentTrade.TradeAction.EntrySignal.SignalName = nameTag + "-" + timeStr;
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
			//CurrentTrade.TradeAction.StopLossSignal.SignalName = nameTag + "-SL-" + timeStr;
			//CurrentTrade.TradeAction.ProfitTargetSignal.SignalName = nameTag + "-PT-" + timeStr;			
		}

		/// <summary>
		/// TrailingSL order Signal name with timestamp:
		/// EntryShort-TLSL-201905312359888
		/// </summary>
		/// <param name="nameTag"></param>
		/// <returns></returns>
		public void SetNewTLSLOrderSignalName(string nameTag, string timeStr) {
			//CurrentTrade.TradeAction.StopLossSignal.SignalName = nameTag + "-TLSL-" + timeStr;
		}
		
		/// <summary>
		/// New OCO ID with timestamp:
		/// OCO-201905312359888
		/// </summary>
		/// <returns></returns>
		public string GetNewOcoID() {
			string sName = "OCO-" + GetBarTimestampStr(0);
			return sName;
		}
		
		//Remove whitespaces from order name
		public string GetOrderName(string orderName) {
			return Regex.Replace(orderName, @"\s+", "");
		}
		
		//Clear order name
		public void ClearOrderName(string oName) {
//			if(oName == null) return;
//			else if(oName.Equals(CurrentTrade.TradeAction.EntrySignal.SignalName)) CurrentTrade.TradeAction.EntrySignal.SignalName = String.Empty;
//			else if(oName.Equals(CurrentTrade.TradeAction.StopLossSignal.SignalName)) {
//				CurrentTrade.TradeAction.StopLossSignal.SignalName = String.Empty;
//				CurrentTrade.OcoID = String.Empty;
//			}
//			else if(oName.Equals(CurrentTrade.TradeAction.ProfitTargetSignal.SignalName)) {
//				CurrentTrade.TradeAction.ProfitTargetSignal.SignalName = String.Empty;
//				CurrentTrade.OcoID = String.Empty;
//			}
//			else if(oName.Equals(CurrentTrade.TradeAction.StopLossSignal.SignalName)) CurrentTrade.TradeAction.StopLossSignal.SignalName = String.Empty;
		}
		
		/// <summary>
		/// Get the subType of the bracket order: entry, SL or PT
		/// </summary>
		/// <param name="order"></param>
		/// <returns></returns>
		public BracketOrderSubType GetBracketOrderSubType(Order order) {
			BracketOrderSubType bost = BracketOrderSubType.UnKnown;
			if(order != null) {
				IndicatorProxy.PrintLog(true, IsLiveTrading(), "GetBracketOrderSubType:" +
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
		public CurrentTradeBase CurrentTrade
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
		
		[Description("Max open position")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MaxOpenPosition", GroupName = GPS_TRADE_MGMT, Order = ODG_MaxOpenPosition)]
        public int TM_MaxOpenPosition
        {
            get{return tm_MaxOpenPosition;}
            set{tm_MaxOpenPosition = Math.Max(1, value);}
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
		private int tm_MaxOpenPosition = 3;
		
		#endregion
	}
}
