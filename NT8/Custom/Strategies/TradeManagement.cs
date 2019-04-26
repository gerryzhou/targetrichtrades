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
			Print(CurrentBar + "::" + this.ToString());
		}
		
		public virtual TradeObj CheckExitTrade() {
			if(Position.MarketPosition == MarketPosition.Flat) return null;
			else if((indicatorSignal.ReversalDir == Reversal.Up && Position.MarketPosition == MarketPosition.Short) ||
				(indicatorSignal.ReversalDir == Reversal.Down && Position.MarketPosition == MarketPosition.Long)) {
				CloseAllPositions();
				CancelExitOrders();
				tradeObj.SetTradeType(TradeType.Exit);
			} else {
				ChangeSLPT();
				tradeObj.SetTradeType(TradeType.Exit);
			}
			
			return tradeObj;
		}
		
		/// <summary>
		/// Check if new entry trade could be generated
		/// </summary>
		/// <returns></returns>
		public virtual TradeObj CheckNewEntryTrade() {
//			if(NewOrderAllowed()) {
//			}
			return null;
		}
		
		public virtual bool NewOrderAllowed()
		{
			if(BarsInProgress !=0) return false;
			
			Print(MethodBase.GetCurrentMethod().ToString() + " - 1");
			//indicatorProxy.TraceMessage(this.Name);
			int bsx = BarsSinceExitExecution(0, "", 0);
			int bse = BarsSinceEntryExecution(0, "", 0);		
			//Print(CurrentBar + ":" + this.Name + " NewOrderAllowed, BarsSinceExit, BarsSinceEntry=" + bsx + "," + bse);
			
			//indicatorProxy.TraceMessage(this.Name);
			double pnl = CheckAccPnL();//GetAccountValue(AccountItem.RealizedProfitLoss);
			
			//indicatorProxy.TraceMessage(this.Name);
			double plrt = CheckAccCumProfit();
			//DateTime dayKey = new DateTime(Time[0].Year,Time[0].Month,Time[0].Day);
			
			//indicatorProxy.TraceMessage(this.Name);
			double pnl_daily = CheckPnlByDay(Time[0].Year, Time[0].Month, Time[0].Day);
			
			
			//indicatorProxy.TraceMessage(this.Name);
			if(PrintOut > -1) {
				//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":(RealizedProfitLoss,RealtimeTrades.CumProfit)=(" + pnl + "," + plrt + ")--" + Get24HDateTime(Time[0]));	
				if(SystemPerformance.AllTrades.ByDay.Count == 2) {
					//giParabSAR.PrintLog(true, !backTest, log_file, "Performance.AllTrades.TradesPerformance.Currency.CumProfit is: " + Performance.AllTrades.TradesPerformance.Currency.CumProfit);
					//giParabSAR.PrintLog(true, !backTest, log_file, "Performance.AllTrades.ByDay[dayKey].TradesPerformance.Currency.CumProfit is: " + pnl);
				}
			}
			
			
			//indicatorProxy.TraceMessage(this.Name);
			if((backTest && IsLiveTrading()) || (!backTest && !IsLiveTrading())) {
				//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + "[backTest,Historical]=" + backTest + "," + Historical + "- NewOrderAllowed=false - " + Get24HDateTime(Time[0]));
				return false;
			}
			
			
			//indicatorProxy.TraceMessage(this.Name);
			if(!backTest && (plrt <= MM_DailyLossLmt || pnl_daily <= MM_DailyLossLmt))
			{
				if(PrintOut > -1) {
					Print(CurrentBar + "-" + AccName + ": dailyLossLmt reached = " + pnl_daily + "," + plrt);
				}
				return false;
			}
			
			
			//indicatorProxy.TraceMessage(this.Name);
			if (backTest && pnl_daily <= MM_DailyLossLmt) {
				if(PrintOut > -3) {
					Print(CurrentBar + "-" + AccName + ": dailyLossLmt reached = " + pnl_daily + "," + plrt);
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ": backTest dailyLossLmt reached = " + pnl);
				}
				return false;				
			}
		
			
			//indicatorProxy.TraceMessage(this.Name);
			if (IsTradingTime(170000) && Position.Quantity == 0)
			{
				if (tradeObj.BracketOrder.EntryOrder == null || tradeObj.BracketOrder.EntryOrder.OrderState != OrderState.Working || MM_EnTrailing)
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
		
		public virtual void NewShortLimitOrder(string msg, double zzGap, double curGap)
		{
			double prc = (MM_EnTrailing && TM_EnCounterPBBars>0) ? Close[0]+TM_EnOffsetPnts : High[0]+TM_EnOffsetPnts;
			
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
				CancelOrder(tradeObj.BracketOrder.EntryOrder);
				//BracketOrder.EntryOrder = EnterShortLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			}
			tradeObj.BracketOrder.EntryOrder = EnterShortLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			tradeObj.barsSinceEnOrd = 0;
		}
		
		public virtual void NewLongLimitOrder(string msg, double zzGap, double curGap)
		{
			double prc = (MM_EnTrailing && TM_EnCounterPBBars>0) ? Close[0]-TM_EnOffsetPnts :  Low[0]-TM_EnOffsetPnts;
			
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
			double prc = GetEnLongPrice();//(MM_EnTrailing && TM_EnCounterPBBars>0) ? Close[0]-TM_EnOffsetPnts :  Low[0]-TM_EnOffsetPnts;
			
			if (tradeObj.BracketOrder.EntryOrder.OrderState == OrderState.Working) {
//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterLongLimit updated buy price (old, new)=(" + BracketOrder.EntryOrder.LimitPrice + "," + prc + ") -- " + Get24HDateTime(Time[0]));
				//CancelOrder(tradeObj.BracketOrder.EntryOrder);
				CancelEntryOrders();
				//tradeObj.BracketOrder.EntryOrder = EnterLongLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			}
			if(tradeObj.BracketOrder.EntryOrder == null) {
				if(tradeObj.BracketOrder.EnOrderType == OrderType.StopMarket) {
					tradeObj.BracketOrder.EntryOrder = EnterLongStopMarket(DefaultQuantity, prc, OrderSignalName.EntryLong.ToString());
				}
				else if(tradeObj.BracketOrder.EnOrderType == OrderType.Limit) {
					tradeObj.BracketOrder.EntryOrder = EnterLongLimit(DefaultQuantity, prc, OrderSignalName.EntryLong.ToString());
				}
//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterLongLimit called buy price= " + prc + " -- " + Get24HDateTime(Time[0]));
			}
			tradeObj.barsSinceEnOrd = 0;
		}
		
		public virtual void SetProfitTargetOrder(string sigName) {
			Print(CurrentBar + ": SetProfitTargetOrder-" 
			+ sigName + "-" + tradeObj.PTCalculationMode 
			+ "-" + tradeObj.profitTargetAmt
			+ "-" + tradeObj.profitTargetPrice + "-avg=" + Position.AveragePrice);
			try{
			switch(tradeObj.PTCalculationMode) {
				case CalculationMode.Currency :
					SetProfitTarget(sigName, CalculationMode.Currency, tradeObj.profitTargetAmt);
					break;
				case CalculationMode.Price :
					SetProfitTarget(sigName, CalculationMode.Price, tradeObj.profitTargetPrice);
					break;
				default: 
					SetProfitTarget(sigName, CalculationMode.Currency, tradeObj.profitTargetAmt);
					break;
			}
			} catch(Exception ex) {
				Print("Ex SetProfitTarget:" + ex.Message);
			}			
					
		}

		public virtual void SetStopLossOrder(string sigName) {
			Print(CurrentBar + ": SetStopLossOrder-" 
			+ sigName + "-" + tradeObj.SLCalculationMode 
			+ "-" + tradeObj.stopLossAmt
			+ "-" + tradeObj.stopLossPrice + "-avg=" + Position.AveragePrice);
			try {
			switch(tradeObj.SLCalculationMode) {
				case CalculationMode.Currency :
					SetStopLoss(sigName, CalculationMode.Currency, tradeObj.stopLossAmt, true);
					break;
				case CalculationMode.Price :
					SetStopLoss(sigName, CalculationMode.Price, tradeObj.stopLossPrice, true);
					break;
				default: 
					SetStopLoss(sigName, CalculationMode.Currency, tradeObj.stopLossAmt, true);
					break;
			}
			} catch(Exception ex) {
				Print("Ex SetStopLossOrder:" + ex.Message);
			}
		}
		
		public virtual bool CheckEnOrder(double cur_gap)
        {
            double min_en = -1;

            if (tradeObj.BracketOrder.EntryOrder != null && tradeObj.BracketOrder.EntryOrder.OrderState == OrderState.Working)
            {
                min_en = indicatorProxy.GetMinutesDiff(tradeObj.BracketOrder.EntryOrder.Time, Time[0]);// DateTime.Now);
                //if ( IsTwoBarReversal(cur_gap, TickSize, enCounterPBBars) || (barsHoldEnOrd > 0 && barsSinceEnOrd >= barsHoldEnOrd) || ( minutesChkEnOrder > 0 &&  min_en >= minutesChkEnOrder))
				if ( (TM_BarsHoldEnOrd > 0 && tradeObj.barsSinceEnOrd >= TM_BarsHoldEnOrd) || ( TM_MinutesChkEnOrder > 0 &&  min_en >= TM_MinutesChkEnOrder))	
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
		
		public virtual bool CloseAllPositions() 
		{
			//giParabSAR.PrintLog(true, !backTest, log_file, "CloseAllPosition called");
			if(Position.MarketPosition == MarketPosition.Long)
				ExitLong();
			if(Position.MarketPosition == MarketPosition.Short)
				ExitShort();
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
			//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "- CancelAllOrders called");
			if(tradeObj.BracketOrder.EntryOrder != null) {
				CancelOrder(tradeObj.BracketOrder.EntryOrder);
				tradeObj.BracketOrder.EntryOrder = null;
			}
			return true;
		}
		
		public virtual bool CancelExitOrders()
		{
			//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "- CancelAllOrders called");
			if(tradeObj.BracketOrder.OCOOrder.StopLossOrder != null)
				CancelOrder(tradeObj.BracketOrder.OCOOrder.StopLossOrder);
			if(tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder != null)
				CancelOrder(tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder);
			return true;
		}
		
		#endregion
		
		#region Event Handlers
		
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
			if(BarsInProgress !=0) return;
			
			int bsx = BarsSinceExitExecution(0, "", 0);
			int bse = BarsSinceEntryExecution(0, "", 0);
			
			Print(CurrentBar + ":OnExecutionUpdate- quantity, marketPosition, price, BarsSinceExit, BarsSinceEntry=" 
			+ quantity + "," + marketPosition + "," + price + ","
			+ bsx + "," + bse
			+ ",SL=" + tradeObj.stopLossPrice);
			
			// Remember to check the underlying IOrder object for null before trying to access its properties
			if (execution.Order != null && execution.Order.OrderState == OrderState.Filled) {
				if(Position.Quantity != 0) {
					//SetEntryOrder(OrderSignalName.EntryShort, execution.Order);
					CalProfitTargetAmt(price, tradeObj.profitFactor);
					SetProfitTargetOrder(OrderSignalName.EntryShort.ToString());
					SetStopLossOrder(OrderSignalName.EntryShort.ToString());
				}
				//if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + " Exe=" + execution.Name + ",Price=" + execution.Price + "," + execution.Time.ToShortTimeString());
				//if(drawTxt) {
				//	IText it = DrawText(CurrentBar.ToString()+Time[0].ToShortTimeString(), Time[0].ToString().Substring(10)+"\r\n"+execution.Name+":"+execution.Price, 0, execution.Price, Color.Red);
				//	it.Locked = false;
				//}
			}
		}

		protected override void OnOrderUpdate(Cbi.Order order, double limitPrice, double stopPrice, 
			int quantity, int filled, double averageFillPrice, 
			Cbi.OrderState orderState, DateTime time, Cbi.ErrorCode error, string comment)
		{
			if(BarsInProgress !=0) return;
			
			int bsx = BarsSinceExitExecution(0, "", 0);
			int bse = BarsSinceEntryExecution(0, "", 0);
			
			Print(CurrentBar + ":OnOrderUpdate-" + order.Name + "-" + order.FromEntrySignal + ";" + order.OrderTypeString
			+ ";" + order.OrderState.ToString() + ";" + order.OrderAction.ToString()
			+ ";SP=" + order.StopPrice + ";LP=" + order.LimitPrice
			+ "; BarsSinceExit, BarsSinceEntry=" + bsx + "," + bse);
			
			indicatorProxy.TraceMessage(this.Name);
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
			
			indicatorProxy.TraceMessage(this.Name);
		    if (tradeObj.BracketOrder.OCOOrder != null &&
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
			
			indicatorProxy.TraceMessage(this.Name);
		    if (tradeObj.BracketOrder.OCOOrder != null &&
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
			
			indicatorProxy.TraceMessage(this.Name);
			if (order.OrderState == OrderState.Working){// || order.OrderType == OrderType.StopMarket) {
				if(order.Name == OrderSignalName.EntryLong.ToString() || order.Name == OrderSignalName.EntryShort.ToString()) {
					tradeObj.BracketOrder.EntryOrder = order;
				}
				if(order.Name == OrderSignalName.ProfitTarget.ToString()) {
					tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder = order;
				}				
				//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + order.ToString());
			}
			
			indicatorProxy.TraceMessage(this.Name);
			if(order.Name == OrderSignalName.StopLoss.ToString() && (order.OrderState == OrderState.Accepted || order.OrderState == OrderState.Working)) {
				tradeObj.BracketOrder.OCOOrder.StopLossOrder = order;
			}
			
//			if(tradeObj.BracketOrder.OCOOrder.StopLossOrder == null && tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder == null)
				//InitTradeMgmt();			
		}
		
		#endregion

		#region TM Properties
		
        [Description("Short, Long or both directions for entry")]
 		//[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TradingDirection", GroupName = "TradeMgmt", Order = 0)]		
        //[GridCategory("Parameters")]
		[XmlIgnore]
        public TradingDirection TM_TradingDirection
        {
            get { return tm_TradingDirection; }
            set { tm_TradingDirection = value; }
        }

        [Description("Trading style: trend following, counter trend, scalp")]
 		//[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TradingStyle", GroupName = "TradeMgmt", Order = 1)]
		[XmlIgnore]
        public TradingStyle TM_TradingStyle
        {
            get { return tm_TradingStyle; }
            set { tm_TradingStyle = value; }
        }
		
        [Description("Offeset points for limit price entry")]
		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EnOffsetPnts", GroupName = "TradeMgmt", Order = 2)]		
        public double TM_EnOffsetPnts
        {
            get{return tm_EnOffsetPnts;}// { return tradeObj==null? 0 : tradeObj.enOffsetPnts; }
            set{tm_EnOffsetPnts = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.enOffsetPnts = Math.Max(0, value); }
        }
		
        [Description("How long to check entry order filled or not")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MinutesChkEnOrder", GroupName = "TradeMgmt", Order = 4)]	
        public int TM_MinutesChkEnOrder
        {
            get{return tm_MinutesChkEnOrder;}// { return tradeObj==null? 0 : tradeObj.minutesChkEnOrder; }
            set{tm_MinutesChkEnOrder = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.minutesChkEnOrder = Math.Max(0, value); }
        }
		
        [Description("How long to check P&L")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MinutesChkPnL", GroupName = "TradeMgmt", Order = 5)]	
        public int TM_MinutesChkPnL
        {
            get{return tm_MinutesChkPnL;}// { return tradeObj==null? 0 : tradeObj.minutesChkPnL; }
            set{tm_MinutesChkPnL = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.minutesChkPnL = Math.Max(-1, value); }
        }
		
		[Description("Bar count before checking P&L")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarsToCheckPL", GroupName = "TradeMgmt", Order = 6)]	
        public int TM_BarsToCheckPnL
        {
            get{return tm_BarsToCheckPnL;}// { return tradeObj==null? 0 : tradeObj.barsToCheckPL; }
            set{tm_BarsToCheckPnL = Math.Max(1, value);}// { if(tradeObj!=null) tradeObj.barsToCheckPL = Math.Max(1, value); }
        }
		
        [Description("How many bars to hold entry order before cancel it")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarsHoldEnOrd", GroupName = "TradeMgmt", Order = 7)]	
        public int TM_BarsHoldEnOrd
        {
            get{return tm_BarsHoldEnOrd;}// { return tradeObj==null? 0 : tradeObj.barsHoldEnOrd; }
            set{tm_BarsHoldEnOrd = Math.Max(1, value);}// { if(tradeObj!=null) tradeObj.barsHoldEnOrd = Math.Max(1, value); }
        }
		
        [Description("Bar count for en order counter pullback")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EnCounterPBBars", GroupName = "TradeMgmt", Order = 8)]	
        public int TM_EnCounterPBBars
        {
            get{return tm_EnCounterPBBars;}// { return tradeObj==null? 0 : tradeObj.enCounterPBBars; }
            set{tm_EnCounterPBBars = Math.Max(1, value);}// { if(tradeObj!=null) tradeObj.enCounterPBBars = Math.Max(-1, value); }
        }
				
		[Description("Bar count since last filled PT or SL")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarsSincePTSL", GroupName = "TradeMgmt", Order = 9)]	
        public int TM_BarsSincePTSL
        {
            get{return tm_BarsSincePtSl;}// { return tradeObj==null? 1 : tradeObj.barsSincePtSl; }
            set{tm_BarsSincePtSl = Math.Max(1, value);}// { if(tradeObj!=null) tradeObj.barsSincePtSl = Math.Max(1, value); }
        }
		
		#endregion

		#region Variables for Properties
		
		private TradingDirection tm_TradingDirection = TradingDirection.Both; // -1=short; 0-both; 1=long;
		private TradingStyle tm_TradingStyle = TradingStyle.TrendFollowing; // -1=counter trend; 1=trend following;				
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
