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
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Strategies.ZTraderStg;
using NinjaTrader.NinjaScript.DrawingTools;

#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class GSZTraderBase : Strategy
	{
		//private bool backTest = true;
		//private int printOut = -5;
		
		//protected TradeObj tradeObj = null;	
		#region Trigger Trade Functions
		
		public virtual void InitTradeMgmt() {
			SetProfitTarget(CalculationMode.Currency, MM_ProfitTargetAmt);
            SetStopLoss(CalculationMode.Currency, MM_StopLossAmt);
		}
		
		public virtual void PutTrade() {
		}
		
		public virtual TradeObj CheckExitTrade(IndicatorSignal signal) {
			if((signal.ReversalDir == Reversal.Up && Position.MarketPosition == MarketPosition.Short) ||
				(signal.ReversalDir == Reversal.Down && Position.MarketPosition == MarketPosition.Long)) {
				CloseAllPositions();
				CancelExitOrders();
				tradeObj.SetTradeType(TradeType.Exit);
			} else {
				ChangeSLPT();
				tradeObj.SetTradeType(TradeType.Exit);
			}
			
			return tradeObj;
		}
		
		protected virtual TradeObj CheckEntryTrade() {
			if(NewOrderAllowed()) {

			}
			return null;
		}
		
		protected bool NewOrderAllowed()
		{
			int bsx = BarsSinceExitExecution();
			int bse = BarsSinceEntryExecution();		
			Print(CurrentBar + ":" + this.Name + " NewOrderAllowed, BarsSinceExit, BarsSinceEntry=" + bsx + "," + bse);
			
			double pnl = CheckAccPnL();//GetAccountValue(AccountItem.RealizedProfitLoss);
			double plrt = CheckAccCumProfit();
			//DateTime dayKey = new DateTime(Time[0].Year,Time[0].Month,Time[0].Day);
			double pnl_daily = CheckPnlByDay(Time[0].Year,Time[0].Month,Time[0].Day);
			
			if(PrintOut > -1) {
				//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":(RealizedProfitLoss,RealtimeTrades.CumProfit)=(" + pnl + "," + plrt + ")--" + Get24HDateTime(Time[0]));	
				if(SystemPerformance.AllTrades.ByDay.Count == 2) {
					//giParabSAR.PrintLog(true, !backTest, log_file, "Performance.AllTrades.TradesPerformance.Currency.CumProfit is: " + Performance.AllTrades.TradesPerformance.Currency.CumProfit);
					//giParabSAR.PrintLog(true, !backTest, log_file, "Performance.AllTrades.ByDay[dayKey].TradesPerformance.Currency.CumProfit is: " + pnl);
				}
			}
			
			if((backTest && State == State.Realtime) || (!backTest && State != State.Realtime)) {
				//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + "[backTest,Historical]=" + backTest + "," + Historical + "- NewOrderAllowed=false - " + Get24HDateTime(Time[0]));
				return false;
			}
			
			if(!backTest && (plrt <= MM_DailyLossLmt || pnl_daily <= MM_DailyLossLmt))
			{
				if(PrintOut > -1) {
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ": dailyLossLmt reached = " + pnl + "," + plrt);
				}
				return false;
			}
			
			if (backTest && pnl_daily <= MM_DailyLossLmt) {
				if(PrintOut > 3) {
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ": backTest dailyLossLmt reached = " + pnl);
				}
				return false;				
			}
		
			if (IsTradingTime(170000) && Position.Quantity == 0)
			{
				if (tradeObj.entryOrder == null || tradeObj.entryOrder.OrderState != OrderState.Working || MM_EnTrailing)
				{ 
					Print("bsx,bse,tradeObj.barsSincePtSl=" + bsx + "," + bse + "," + tradeObj.barsSincePTSL);
					if(bsx < 0 || bsx > tradeObj.barsSincePTSL) //if(bsx == -1 || bsx > tradeObj.barsSincePtSl)
					{
						//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + "- NewOrderAllowed=true - " + Get24HDateTime(Time[0]));
						return true;
					} //else 
						//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + "-NewOrderAllowed=false-[bsx,barsSincePtSl]" + bsx + "," + barsSincePtSl + " - " + Get24HDateTime(Time[0]));
				} //else
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + "-NewOrderAllowed=false-[entryOrder.OrderState,entryOrder.OrderType]" + entryOrder.OrderState + "," + entryOrder.OrderType + " - " + Get24HDateTime(Time[0]));
			}
			
			return false;
		}
		
		#endregion Trigger Functions
		
		#region Money Mgmt Functions
		protected bool ChangeSLPT()
		{
//			int bse = BarsSinceEntry();
//			double timeSinceEn = -1;
//			if(bse > 0) {
//				timeSinceEn = indicatorProxy.GetMinutesDiff(Time[0], Time[bse]);
//			}
			
			double pl = Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0]);//.GetProfitLoss(Close[0], PerformanceUnit.Currency);
			 // If not flat print out unrealized PnL
    		if (Position.MarketPosition != MarketPosition.Flat) 
			{
         		//giParabSAR.PrintLog(true, !backTest, log_file, AccName + "- Open PnL: " + pl);
				//int nChkPnL = (int)(timeSinceEn/minutesChkPnL);
				double curPTTics = -1;
				double slPrc = tradeObj.stopLossOrder == null ? Position.AveragePrice : tradeObj.stopLossOrder.StopPrice;
				
				if(MM_PTTrailing && pl >= 12.5*(tradeObj.trailingPTTic - 2*MM_ProfitTgtIncTic))
				{
					tradeObj.trailingPTTic = tradeObj.trailingPTTic + MM_ProfitTgtIncTic;
					if(tradeObj.profitTargetOrder != null) {
						curPTTics = Math.Abs(tradeObj.profitTargetOrder.LimitPrice - Position.AveragePrice)/TickSize;
					}
					//giParabSAR.PrintLog(true, !backTest, log_file, AccName + "- update PT: PnL=" + pl + ",(trailingPTTic, curPTTics, $Amt, $Amt_cur)=(" + trailingPTTic + "," + curPTTics + "," + 12.5*trailingPTTic + "," + 12.5*curPTTics + ")");
					if(tradeObj.profitTargetOrder == null || tradeObj.trailingPTTic > curPTTics)
						SetProfitTarget(CalculationMode.Ticks, tradeObj.trailingPTTic);
				}
				
				if(pl >= MM_BreakEvenAmt) { //setup breakeven order
					//giParabSAR.PrintLog(true, !backTest, log_file, AccName + "- setup SL Breakeven: (PnL, posAvgPrc)=(" + pl + "," + Position.AvgPrice + ")");
					slPrc = Position.AveragePrice;
					//SetStopLoss(0);
				}
				
				if(MM_SLTrailing) { // trailing max and min profits then converted to trailing stop after over the max
//					if(trailingSLTic > profitLockMaxTic && pl >= 12.5*(trailingSLTic + 2*profitTgtIncTic)) {
//						trailingSLTic = trailingSLTic + profitTgtIncTic;
//						if(Position.MarketPosition == MarketPosition.Long)
//							slPrc = Position.AvgPrice+TickSize*trailingSLTic;
//						if(Position.MarketPosition == MarketPosition.Short)
//							slPrc = Position.AvgPrice-TickSize*trailingSLTic;
//						Print(AccName + "- update SL over Max: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= (" + slTrailing + "," + trailingSLTic + "," + slPrc + ")");						
//					}
					if(tradeObj.trailingSLTic > MM_ProfitLockMaxTic && pl >= 12.5*(tradeObj.trailingSLTic + 2*MM_ProfitTgtIncTic)) {
						tradeObj.trailingSLTic = tradeObj.trailingSLTic + MM_ProfitTgtIncTic;
						if(tradeObj.stopLossOrder != null)
							CancelOrder(tradeObj.stopLossOrder);
						if(tradeObj.profitTargetOrder != null)
							CancelOrder(tradeObj.profitTargetOrder);
						SetTrailStop(CalculationMode.Currency, MM_TrailingStopLossAmt);
						//giParabSAR.PrintLog(true, !backTest, log_file, AccName + "- SetTrailStop over SL Max: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= (" + slTrailing + "," + trailingSLTic + "," + slPrc + ")");						
					}
					else if(pl >= 12.5*(MM_ProfitLockMaxTic + 2*MM_ProfitTgtIncTic)) { // lock max profits
						tradeObj.trailingSLTic = tradeObj.trailingSLTic + MM_ProfitTgtIncTic;
						if(Position.MarketPosition == MarketPosition.Long)
							slPrc = tradeObj.trailingSLTic > MM_ProfitLockMaxTic ? Position.AveragePrice+TickSize*tradeObj.trailingSLTic : Position.AveragePrice+TickSize*MM_ProfitLockMaxTic;
						if(Position.MarketPosition == MarketPosition.Short)
							slPrc = tradeObj.trailingSLTic > MM_ProfitLockMaxTic ? Position.AveragePrice-TickSize*tradeObj.trailingSLTic :  Position.AveragePrice-TickSize*MM_ProfitLockMaxTic;
						//giParabSAR.PrintLog(true, !backTest, log_file, AccName + "- update SL Max: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= (" + slTrailing + "," + trailingSLTic + "," + slPrc + ")");
						//SetStopLoss(CalculationMode.Price, slPrc);
					}
					else if(pl >= 12.5*(MM_ProfitLockMinTic + 2*MM_ProfitTgtIncTic)) { //lock min profits
						tradeObj.trailingSLTic = tradeObj.trailingSLTic + MM_ProfitTgtIncTic;
						if(Position.MarketPosition == MarketPosition.Long)
							slPrc = Position.AveragePrice+TickSize*MM_ProfitLockMinTic;
						if(Position.MarketPosition == MarketPosition.Short)
							slPrc = Position.AveragePrice-TickSize*MM_ProfitLockMinTic;
						//giParabSAR.PrintLog(true, !backTest, log_file, AccName + "- update SL Min: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= (" + slTrailing + "," + trailingSLTic + "," + slPrc + ")");
						//SetStopLoss(CalculationMode.Price, slPrc);
					}
				}
				if(tradeObj.stopLossOrder == null || 
					(Position.MarketPosition == MarketPosition.Long && slPrc > tradeObj.stopLossOrder.StopPrice) ||
					(Position.MarketPosition == MarketPosition.Short && slPrc < tradeObj.stopLossOrder.StopPrice)) 
				{
					SetStopLoss(CalculationMode.Price, slPrc);
				}
			} else {
				SetStopLoss(CalculationMode.Currency, MM_StopLossAmt);
				SetProfitTarget(CalculationMode.Currency, MM_ProfitTargetAmt);
			}

			return false;
		}
		
		public double CheckAccPnL() {
			double pnl = 0;//GetAccountValue(AccountItem.RealizedProfitLoss);
			//Print(CurrentBar + "-" + AccName + ": GetAccountValue(AccountItem.RealizedProfitLoss)= " + pnl + " -- " + Time[0].ToString());
			return pnl;
		}
		
		public double CheckAccCumProfit() {
			double plrt = SystemPerformance.RealTimeTrades.TradesPerformance.Currency.CumProfit;//Performance.RealtimeTrades.TradesPerformance.Currency.CumProfit;
			//Print(CurrentBar + "-" + AccName + ": Cum runtime PnL= " + plrt);
			return plrt;
		}
		
		public double CheckPerformance()
		{
			double pl = SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit;//Performance.AllTrades.TradesPerformance.Currency.CumProfit;
			double plrt = SystemPerformance.RealTimeTrades.TradesPerformance.Currency.CumProfit;//Performance.RealtimeTrades.TradesPerformance.Currency.CumProfit;
			//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ": Cum all PnL= " + pl + ", Cum runtime PnL= " + plrt);
			return plrt;
		}
		
		public double CheckPnlByDay(int year, int month, int day) {
			double pnl = 0;
			DateTime dayKey = new DateTime(year, month, day);//(Time[0].Year,Time[0].Month,Time[0].Day);
			Print("CheckPnlByDay SystemPerformance.AllTrades dayKey=" + dayKey);
			Print("CheckPnlByDay SystemPerformance.AllTrades ByDay.Count=" + SystemPerformance.AllTrades.ByDay.Count);
			
			if(SystemPerformance.AllTrades.ByDay.Keys.Contains(dayKey)) {
				TradeCollection tc = (TradeCollection)SystemPerformance.AllTrades.ByDay[dayKey];//Performance.AllTrades.ByDay[dayKey]; 
				if(backTest && tc != null) {
					pnl = tc.TradesPerformance.Currency.CumProfit;
					Print("CheckPnlByDay pnl=" + pnl);
				}
			}
			return pnl;
		}
		
		#endregion
		
		#region Trade Mgmt Functions
		protected void NewShortLimitOrder(string msg, double zzGap, double curGap)
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
			if(tradeObj.entryOrder == null) {
//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg + ", EnterShortLimit called short price=" + prc + "--" + Get24HDateTime(Time[0]));			
			}
			else if (tradeObj.entryOrder.OrderState == OrderState.Working) {
//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterShortLimit updated short price (old, new)=(" + entryOrder.LimitPrice + "," + prc + ") -- " + Get24HDateTime(Time[0]));		
				CancelOrder(tradeObj.entryOrder);
				//entryOrder = EnterShortLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			}
			tradeObj.entryOrder = EnterShortLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			tradeObj.barsSinceEnOrd = 0;
		}
		
		protected void NewLongLimitOrder(string msg, double zzGap, double curGap)
		{
			double prc = (MM_EnTrailing && TM_EnCounterPBBars>0) ? Close[0]-TM_EnOffsetPnts :  Low[0]-TM_EnOffsetPnts;
			
			if(tradeObj.entryOrder == null) {
				tradeObj.entryOrder = EnterLongLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterLongLimit called buy price= " + prc + " -- " + Get24HDateTime(Time[0]));
			}
			else if (tradeObj.entryOrder.OrderState == OrderState.Working) {
//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + msg +  ", EnterLongLimit updated buy price (old, new)=(" + entryOrder.LimitPrice + "," + prc + ") -- " + Get24HDateTime(Time[0]));
				CancelOrder(tradeObj.entryOrder);
				tradeObj.entryOrder = EnterLongLimit(0, true, DefaultQuantity, prc, "pbSAREntrySignal");
			}
			tradeObj.barsSinceEnOrd = 0;
		}
		
		public bool CheckEnOrder(double cur_gap)
        {
            double min_en = -1;

            if (tradeObj.entryOrder != null && tradeObj.entryOrder.OrderState == OrderState.Working)
            {
                min_en = indicatorProxy.GetMinutesDiff(tradeObj.entryOrder.Time, Time[0]);// DateTime.Now);
                //if ( IsTwoBarReversal(cur_gap, TickSize, enCounterPBBars) || (barsHoldEnOrd > 0 && barsSinceEnOrd >= barsHoldEnOrd) || ( minutesChkEnOrder > 0 &&  min_en >= minutesChkEnOrder))
				if ( (TM_BarsHoldEnOrd > 0 && tradeObj.barsSinceEnOrd >= TM_BarsHoldEnOrd) || ( TM_MinutesChkEnOrder > 0 &&  min_en >= TM_MinutesChkEnOrder))	
                {
                    CancelOrder(tradeObj.entryOrder);
                    //giParabSAR.PrintLog(true, !backTest, log_file, "Order cancelled for " + AccName + ":" + barsSinceEnOrd + "/" + min_en + " bars/mins elapsed--" + entryOrder.ToString());
					return true;
                }
				else {
					//giParabSAR.PrintLog(true, !backTest, log_file, "Order working for " + AccName + ":" + barsSinceEnOrd + "/" + min_en + " bars/mins elapsed--" + entryOrder.ToString());
					tradeObj.barsSinceEnOrd++;
				}
            }
            return false;
        }
		
		public bool CloseAllPositions() 
		{
			//giParabSAR.PrintLog(true, !backTest, log_file, "CloseAllPosition called");
			if(Position.MarketPosition == MarketPosition.Long)
				ExitLong();
			if(Position.MarketPosition == MarketPosition.Short)
				ExitShort();
			return true;
		}
		
		public bool CancelAllOrders()
		{
			CancelExitOrders();
			CancelEntryOrders();
			return true;
		}

		public bool CancelEntryOrders()
		{
			//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "- CancelAllOrders called");
			if(tradeObj.entryOrder != null)
				CancelOrder(tradeObj.entryOrder);
			return true;
		}
		
		public bool CancelExitOrders()
		{
			//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "- CancelAllOrders called");
			if(tradeObj.stopLossOrder != null)
				CancelOrder(tradeObj.stopLossOrder);
			if(tradeObj.profitTargetOrder != null)
				CancelOrder(tradeObj.profitTargetOrder);
			return true;
		}
		
		#endregion
		
		#region Event Handlers
		
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
			int bsx = BarsSinceExitExecution();
			int bse = BarsSinceEntryExecution();
			
			Print(CurrentBar + ":OnExecutionUpdate called, BarsSinceExit, BarsSinceEntry=" + bsx + "," + bse);
			// Remember to check the underlying IOrder object for null before trying to access its properties
			if (execution.Order != null && execution.Order.OrderState == OrderState.Filled) {
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
			int bsx = BarsSinceExitExecution();
			int bse = BarsSinceEntryExecution();
			
			Print(CurrentBar + ":OnOrderUpdate called, BarsSinceExit, BarsSinceEntry=" + bsx + "," + bse);
			
		    if (tradeObj.entryOrder != null && tradeObj.entryOrder == order)
		    {
				//giParabSAR.PrintLog(true, !backTest, log_file, order.ToString() + "--" + order.OrderState);
		        if (order.OrderState == OrderState.Cancelled || 
					order.OrderState == OrderState.Filled || 
					order.OrderState == OrderState.Rejected || 
					order.OrderState == OrderState.Unknown)
				{
					tradeObj.barsSinceEnOrd = 0;
					tradeObj.entryOrder = null;
				}
		    }
			
			if (order.OrderState == OrderState.Working || order.OrderType == OrderType.StopMarket) {
//				if(TG_PrintOut > -1)
					//giParabSAR.PrintLog(true, !backTest, log_file, CurrentBar + "-" + AccName + ":" + order.ToString());
			}
			
			if(tradeObj.profitTargetOrder == null && order.Name == "Profit target" && order.OrderState == OrderState.Working) {
				tradeObj.profitTargetOrder = order;
			}
			if(tradeObj.stopLossOrder == null && order.Name == "Stop loss" && (order.OrderState == OrderState.Accepted || order.OrderState == OrderState.Working)) {
				tradeObj.stopLossOrder = order;
			}
			
			if( order.OrderState == OrderState.Filled || order.OrderState == OrderState.Cancelled) {
				if(order.Name == "Stop loss")
					tradeObj.stopLossOrder = null;
				if(order.Name == "Profit target")
					tradeObj.profitTargetOrder = null;
			}
		}

		protected override void OnPositionUpdate(Cbi.Position position, double averagePrice, 
			int quantity, Cbi.MarketPosition marketPosition)
		{
			int bsx = BarsSinceExitExecution();
			int bse = BarsSinceEntryExecution();
			
			Print(CurrentBar + ":OnPositionUpdate called, BarsSinceExit, BarsSinceEntry=" + bsx + "," + bse);
			
			Print(CurrentBar + ":OnPositionUpdate MarketPosition, Quantity=" + Position.MarketPosition + "," + Position.Quantity);
			//Print(position.ToString() + "--MarketPosition=" + position.MarketPosition);
			if (position.MarketPosition == MarketPosition.Flat)
			{
				tradeObj.trailingPTTic = MM_ProfitTargetAmt/12.5;
				tradeObj.trailingSLTic = MM_StopLossAmt/12.5;
			}
		}
		
		#endregion

		#region TM Properties
		
        [Description("Short, Long or both direction for entry")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TM_TradeDirection", GroupName = "TradeMgmt", Order = 0)]		
        //[GridCategory("Parameters")]
        public int TM_TradeDirection
        {
            get { return tm_TradeDirection; }
            set { tm_TradeDirection = value; }
        }		

        [Description("Trade style: trend following, counter trend, scalp")]
 		//[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TM_TradeStyle", GroupName = "TradeMgmt", Order = 1)]
		[XmlIgnore]
        public TradingStyle TM_TradeStyle
        {
            get { return tm_TradeStyle; }
            set { tm_TradeStyle = value; }
        }
		
        [Description("Offeset points for limit price entry")]
		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TM_EnOffsetPnts", GroupName = "TradeMgmt", Order = 0)]		
        public double TM_EnOffsetPnts
        {
            get{return tm_EnOffsetPnts;}// { return tradeObj==null? 0 : tradeObj.enOffsetPnts; }
            set{tm_EnOffsetPnts = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.enOffsetPnts = Math.Max(0, value); }
        }
		
        [Description("How long to check entry order filled or not")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TM_MinutesChkEnOrder", GroupName = "TradeMgmt", Order = 1)]	
        public int TM_MinutesChkEnOrder
        {
            get{return tm_MinutesChkEnOrder;}// { return tradeObj==null? 0 : tradeObj.minutesChkEnOrder; }
            set{tm_MinutesChkEnOrder = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.minutesChkEnOrder = Math.Max(0, value); }
        }
		
        [Description("How long to check P&L")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TM_MinutesChkPnL", GroupName = "TradeMgmt", Order = 2)]	
        public int TM_MinutesChkPnL
        {
            get{return tm_MinutesChkPnL;}// { return tradeObj==null? 0 : tradeObj.minutesChkPnL; }
            set{tm_MinutesChkPnL = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.minutesChkPnL = Math.Max(-1, value); }
        }		
		
		[Description("Bar count before checking P&L")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TM_BarsToCheckPL", GroupName = "TradeMgmt", Order = 6)]	
        public int TM_BarsToCheckPnL
        {
            get{return tm_BarsToCheckPnL;}// { return tradeObj==null? 0 : tradeObj.barsToCheckPL; }
            set{tm_BarsToCheckPnL = Math.Max(1, value);}// { if(tradeObj!=null) tradeObj.barsToCheckPL = Math.Max(1, value); }
        }
		
        [Description("Bar count since en order issued")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TM_BarsHoldEnOrd", GroupName = "TradeMgmt", Order = 3)]	
        public int TM_BarsHoldEnOrd
        {
            get{return tm_BarsHoldEnOrd;}// { return tradeObj==null? 0 : tradeObj.barsHoldEnOrd; }
            set{tm_BarsHoldEnOrd = Math.Max(1, value);}// { if(tradeObj!=null) tradeObj.barsHoldEnOrd = Math.Max(1, value); }
        }
		
        [Description("Bar count for en order counter pullback")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TM_EnCounterPBBars", GroupName = "TradeMgmt", Order = 4)]	
        public int TM_EnCounterPBBars
        {
            get{return tm_EnCounterPBBars;}// { return tradeObj==null? 0 : tradeObj.enCounterPBBars; }
            set{tm_EnCounterPBBars = Math.Max(1, value);}// { if(tradeObj!=null) tradeObj.enCounterPBBars = Math.Max(-1, value); }
        }		
				
		[Description("Bar count since last filled PT or SL")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TM_BarsSincePtSl", GroupName = "TradeMgmt", Order = 5)]	
        public int TM_BarsSincePTSL
        {
            get{return tm_BarsSincePtSl;}// { return tradeObj==null? 1 : tradeObj.barsSincePtSl; }
            set{tm_BarsSincePtSl = Math.Max(1, value);}// { if(tradeObj!=null) tradeObj.barsSincePtSl = Math.Max(1, value); }
        }
		
		#endregion
		
		#region MM Properties
        [Description("Money amount of profit target")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		//[DefaultValueAttribute(300)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MM_ProfitTgtAmt", GroupName = "MoneyMgmt", Order = 0)]	
        public double MM_ProfitTargetAmt
        {
            get{return mm_ProfitTargetAmt;}// { return tradeObj==null? 50 : tradeObj.profitTargetAmt; }
            set{mm_ProfitTargetAmt = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.profitTargetAmt = Math.Max(0, value); }
        }

        [Description("Money amount for profit target increasement")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MM_ProfitTgtIncTic", GroupName = "MoneyMgmt", Order = 1)]	
        public double MM_ProfitTgtIncTic
        {
            get{return mm_ProfitTgtIncTic;}// { return tradeObj==null? 0 : tradeObj.profitTgtIncTic; }
            set{mm_ProfitTgtIncTic = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.profitTgtIncTic = Math.Max(0, value); }
        }
		
        [Description("Tick amount for min profit locking")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MM_ProfitLockMinTic", GroupName = "MoneyMgmt", Order = 2)]	
        public double MM_ProfitLockMinTic
        {
            get{return mm_ProfitLockMinTic;}// { return tradeObj==null? 0 : tradeObj.profitLockMinTic; }
            set{mm_ProfitLockMinTic = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.profitLockMinTic = Math.Max(0, value); }
        }

		[Description("Tick amount for max profit locking")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MM_ProfitLockMaxTic", GroupName = "MoneyMgmt", Order = 3)]	
        public double MM_ProfitLockMaxTic
        {
            get{return mm_ProfitLockMaxTic;}// { return tradeObj==null? 0 : tradeObj.profitLockMaxTic; }
            set{mm_ProfitLockMaxTic = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.profitLockMaxTic = Math.Max(0, value); }
        }
		
        [Description("Money amount of stop loss")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MM_StopLossAmt", GroupName = "MoneyMgmt", Order = 4)]	
        public double MM_StopLossAmt
        {
            get{return mm_StopLossAmt;}// { return tradeObj==null? 30 : tradeObj.stopLossAmt; }
            set{mm_StopLossAmt = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.stopLossAmt = Math.Max(0, value); }
        }
		
        [Description("Money amount of trailing stop loss")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MM_TrailingStopLossAmt", GroupName = "MoneyMgmt", Order = 5)]	
        public double MM_TrailingStopLossAmt
        {
            get{return mm_TrailingStopLossAmt;}// { return tradeObj==null? 0 : tradeObj.trailingSLAmt; }
            set{mm_TrailingStopLossAmt = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.trailingSLAmt = Math.Max(0, value); }
        }
		
		[Description("Money amount for stop loss increasement")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MM_StopLossIncTic", GroupName = "MoneyMgmt", Order = 6)]	
        public double MM_StopLossIncTic
        {
            get{return mm_StopLossIncTic;}// { return tradeObj==null? 0 : tradeObj.stopLossIncTic; }
            set{mm_StopLossIncTic = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.stopLossIncTic = Math.Max(0, value); }
        }
		
        [Description("Break Even amount")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MM_BreakEvenAmt", GroupName = "MoneyMgmt", Order = 7)]	
        public double MM_BreakEvenAmt
        {
            get{return mm_BreakEvenAmt;}// { return tradeObj==null? 0 : tradeObj.breakEvenAmt; }
            set{mm_BreakEvenAmt = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.breakEvenAmt = Math.Max(0, value); }
        }

		[Description("Daily Loss Limit amount")]
 		[Range(int.MinValue, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MM_DailyLossLmt", GroupName = "MoneyMgmt", Order = 8)]	
        public double MM_DailyLossLmt
        {
            get{return mm_DailyLossLmt;}// { return tradeObj==null? 0 : tradeObj.dailyLossLmt; }
            set{mm_DailyLossLmt = Math.Min(-100, value);}// { if(tradeObj!=null) tradeObj.dailyLossLmt = Math.Min(-100, value); }
        }
		
		[Description("Use trailing entry every bar")]
 		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MM_EnTrailing", GroupName = "MoneyMgmt", Order = 9)]	
        public bool MM_EnTrailing
        {
            get{return mm_EnTrailing;}// { return tradeObj==null? false : tradeObj.enTrailing; }
            set{mm_EnTrailing = value;}// { if(tradeObj!=null) tradeObj.enTrailing = value; }
        }
		
		[Description("Use trailing profit target every bar")]
 		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MM_PTTrailing", GroupName = "MoneyMgmt", Order = 10)]	
        public bool MM_PTTrailing
        {
            get{return mm_PTTrailing;}// { return tradeObj==null? false : tradeObj.ptTrailing; }
            set{mm_PTTrailing = value;}// { if(tradeObj!=null) tradeObj.ptTrailing = value; }
        }
		
		[Description("Use trailing stop loss every bar")]
 		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MM_SLTrailing", GroupName = "MoneyMgmt", Order = 11)]	
        public bool MM_SLTrailing
        {
            get{return mm_SLTrailing;}// { return tradeObj==null? false : tradeObj.slTrailing; }
            set{mm_SLTrailing = value;}// { if(tradeObj!=null) tradeObj.slTrailing = value; }
        }
		
		#endregion
		
		#region Variables for Properties
		
		private int tm_TradeDirection = 0; // -1=short; 0-both; 1=long;
		private TradingStyle tm_TradeStyle = TradingStyle.TrendFollowing; // -1=counter trend; 1=trend following;				
		private double tm_EnOffsetPnts = 1;
		private int tm_MinutesChkEnOrder = 10;
		private int tm_MinutesChkPnL = 30;
		private int tm_BarsToCheckPnL = 2;
		private int tm_BarsHoldEnOrd = 3;
		private int tm_EnCounterPBBars = 2;
		private int tm_BarsSincePtSl = 1;
		
		private double mm_ProfitTargetAmt = 500;
		private double mm_ProfitTgtIncTic = 8;
		private double mm_ProfitLockMinTic = 16;
		private double mm_ProfitLockMaxTic = 40;
		private double mm_StopLossAmt = 300;
		private double mm_TrailingStopLossAmt = 16;
		private double mm_StopLossIncTic = 8;
		private double mm_BreakEvenAmt = 20;
		private double mm_DailyLossLmt = -200;
		private bool mm_EnTrailing = true;
		private bool mm_PTTrailing = false;
		private bool mm_SLTrailing = false;
		
		#endregion
	}
}
