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
		#region Money Mgmt Functions
		
		protected double CalProfitTargetAmt(double price, double profitFactor) {
			switch(tradeObj.SLCalculationMode) {
				case CalculationMode.Currency:
					if(profitFactor == 0)
						tradeObj.profitTargetAmt = MM_ProfitTargetAmt;
					else
						tradeObj.profitTargetAmt = 
							profitFactor*tradeObj.stopLossAmt;
					break;
				case CalculationMode.Price:
					if(profitFactor == 0)
						tradeObj.profitTargetAmt = MM_ProfitTargetAmt;			
					else
						tradeObj.profitTargetAmt = 
							profitFactor*Math.Abs(price-tradeObj.stopLossPrice)*Instrument.MasterInstrument.PointValue;
					break;
			}			
			return 0;
		}
		
		protected bool ChangeSLPT()
		{
//			int bse = BarsSinceEntry();
//			double timeSinceEn = -1;
//			if(bse > 0) {
//				timeSinceEn = indicatorProxy.GetMinutesDiff(Time[0], Time[bse]);
//			}
			
			double pl = Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0]);//.GetProfitLoss(Close[0], PerformanceUnit.Currency);
			
			if(Position.Quantity == 0)
				indicatorProxy.PrintLog(true, !backTest, 
					AccName + "- ChangeSLPT=0: (PnL, posAvgPrc, MM_BreakEvenAmt)=(" + pl + "," + Position.AveragePrice + "," + MM_BreakEvenAmt + ")");
			else
				indicatorProxy.PrintLog(true, !backTest, 
					AccName + "- ChangeSLPT<>0: (PnL, posAvgPrc, MM_BreakEvenAmt)=(" + pl + "," + Position.AveragePrice + "," + MM_BreakEvenAmt + ")");
			
			// If not flat print out unrealized PnL
    		if (Position.MarketPosition != MarketPosition.Flat) 
			{
         		//giParabSAR.PrintLog(true, !backTest, log_file, AccName + "- Open PnL: " + pl);
				//int nChkPnL = (int)(timeSinceEn/minutesChkPnL);
				double curPTTics = -1;
				double slPrc = tradeObj.BracketOrder.OCOOrder.StopLossOrder == null ?
					Position.AveragePrice : tradeObj.BracketOrder.OCOOrder.StopLossOrder.StopPrice;
				
				if(MM_PTTrailing && pl >= 12.5*(tradeObj.trailingPTTic - 2*MM_ProfitTgtIncTic))
				{
					tradeObj.trailingPTTic = tradeObj.trailingPTTic + MM_ProfitTgtIncTic;
					if(tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder != null) {
						curPTTics = Math.Abs(tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder.LimitPrice - Position.AveragePrice)/TickSize;
					}
					//giParabSAR.PrintLog(true, !backTest, log_file, AccName + "- update PT: PnL=" + pl + ",(trailingPTTic, curPTTics, $Amt, $Amt_cur)=(" + trailingPTTic + "," + curPTTics + "," + 12.5*trailingPTTic + "," + 12.5*curPTTics + ")");
					if(tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder == null || tradeObj.trailingPTTic > curPTTics)
						SetProfitTarget(CalculationMode.Ticks, tradeObj.trailingPTTic);
				}
				indicatorProxy.PrintLog(true, !backTest, 
					AccName + "- SL Breakeven: (PnL, posAvgPrc, MM_BreakEvenAmt)=(" + pl + "," + Position.AveragePrice + "," + MM_BreakEvenAmt + ")");
				
				if(pl >= MM_BreakEvenAmt) { //setup breakeven order
					indicatorProxy.PrintLog(true, !backTest, 
						AccName + "- setup SL Breakeven: (PnL, posAvgPrc)=(" + pl + "," + Position.AveragePrice + ")");
					slPrc = Position.AveragePrice;
					//SetStopLoss(CalculationMode.Currency, 0);
					if(tradeObj.BracketOrder.OCOOrder.StopLossOrder != null) {
						indicatorProxy.PrintLog(true, !backTest, 
							AccName + "- Setup SL Breakeven Price=" + slPrc + "," + tradeObj.BracketOrder.OCOOrder.StopLossOrder.TimeInForce.ToString());
//						ChangeOrder(tradeObj.BracketOrder.OCOOrder.StopLossOrder, 
//						tradeObj.BracketOrder.OCOOrder.StopLossOrder.Quantity, 0, slPrc);
						SetStopLossOrder(tradeObj.BracketOrder.OCOOrder.StopLossOrder.FromEntrySignal);						
					}
				}
				
				if(MM_SLTrailing) { // trailing max and min profits then converted to trailing stop after over the max
					if(tradeObj.trailingSLTic > tradeObj.profitLockMaxTic && pl >= 12.5*(tradeObj.trailingSLTic + 2*tradeObj.profitTgtIncTic)) {
						tradeObj.trailingSLTic = tradeObj.trailingSLTic + tradeObj.profitTgtIncTic;
						if(Position.MarketPosition == MarketPosition.Long)
							slPrc = Position.AveragePrice+TickSize*tradeObj.trailingSLTic;
						if(Position.MarketPosition == MarketPosition.Short)
							slPrc = Position.AveragePrice-TickSize*tradeObj.trailingSLTic;
						Print(AccName + "- update SL over Max: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= (" + MM_SLTrailing + "," + tradeObj.trailingSLTic + "," + slPrc + ")");						
					}
					if(tradeObj.trailingSLTic > MM_ProfitLockMaxTic && pl >= 12.5*(tradeObj.trailingSLTic + 2*MM_ProfitTgtIncTic)) {
						tradeObj.trailingSLTic = tradeObj.trailingSLTic + MM_ProfitTgtIncTic;
						if(tradeObj.BracketOrder.OCOOrder.StopLossOrder != null)
							CancelOrder(tradeObj.BracketOrder.OCOOrder.StopLossOrder);
						if(tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder != null)
							CancelOrder(tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder);
						SetTrailStop(CalculationMode.Currency, MM_TrailingStopLossAmt);
						indicatorProxy.PrintLog(true, !backTest,
							AccName + "- SetTrailStop over SL Max: PnL=" + pl +
							"(slTrailing, trailingSLTic, slPrc)= (" + tradeObj.slTrailing + "," + tradeObj.trailingSLTic + "," + slPrc + ")");						
					}
					else if(pl >= 12.5*(MM_ProfitLockMaxTic + 2*MM_ProfitTgtIncTic)) { // lock max profits
						tradeObj.trailingSLTic = tradeObj.trailingSLTic + MM_ProfitTgtIncTic;
						if(Position.MarketPosition == MarketPosition.Long)
							slPrc = tradeObj.trailingSLTic > MM_ProfitLockMaxTic ? Position.AveragePrice+TickSize*tradeObj.trailingSLTic : Position.AveragePrice+TickSize*MM_ProfitLockMaxTic;
						if(Position.MarketPosition == MarketPosition.Short)
							slPrc = tradeObj.trailingSLTic > MM_ProfitLockMaxTic ? Position.AveragePrice-TickSize*tradeObj.trailingSLTic :  Position.AveragePrice-TickSize*MM_ProfitLockMaxTic;
						indicatorProxy.PrintLog(true, !backTest,
							AccName + "- update SL Max: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= ("
							+ tradeObj.slTrailing + "," + tradeObj.trailingSLTic + "," + slPrc + ")");
						//SetStopLoss(CalculationMode.Price, slPrc);
					}
					else if(pl >= 12.5*(MM_ProfitLockMinTic + 2*MM_ProfitTgtIncTic)) { //lock min profits
						tradeObj.trailingSLTic = tradeObj.trailingSLTic + MM_ProfitTgtIncTic;
						if(Position.MarketPosition == MarketPosition.Long)
							slPrc = Position.AveragePrice+TickSize*MM_ProfitLockMinTic;
						if(Position.MarketPosition == MarketPosition.Short)
							slPrc = Position.AveragePrice-TickSize*MM_ProfitLockMinTic;
						indicatorProxy.PrintLog(true, !backTest, 
							AccName + "- update SL Min: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= ("
							+ tradeObj.slTrailing + "," + tradeObj.trailingSLTic + "," + slPrc + ")");
						//SetStopLoss(CalculationMode.Price, slPrc);
					}
				}
				if(tradeObj.BracketOrder.OCOOrder.StopLossOrder == null || 
					(Position.MarketPosition == MarketPosition.Long && slPrc > tradeObj.BracketOrder.OCOOrder.StopLossOrder.StopPrice) ||
					(Position.MarketPosition == MarketPosition.Short && slPrc < tradeObj.BracketOrder.OCOOrder.StopLossOrder.StopPrice)) 
				{
					//SetStopLoss(CalculationMode.Price, slPrc);
				}
			} else {
				//InitTradeMgmt();
			}

			return false;
		}
		
		public double CheckAccPnL() {
			double pnl = 0;//GetAccountValue(AccountItem.RealizedProfitLoss);
			//Print(CurrentBar + "-" + AccName + ": GetAccountValue(AccountItem.RealizedProfitLoss)= " + pnl + " -- " + Time[0].ToString());
			return pnl;
		}
		
		public double CheckAccCumProfit() {
			///Performance.RealtimeTrades.TradesPerformance.Currency.CumProfit;
			//Print(CurrentBar + "-" + AccName + ": Cum runtime PnL= " + plrt);
			if(IsLiveTrading()) {
				return SystemPerformance.RealTimeTrades.TradesPerformance.Currency.CumProfit;
			}
			else {
				return SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit;
			}
		}
		
		public double CheckPerformance()
		{
			double pl = SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit;//Performance.AllTrades.TradesPerformance.Currency.CumProfit;
			double plrt = SystemPerformance.RealTimeTrades.TradesPerformance.Currency.CumProfit;//Performance.RealtimeTrades.TradesPerformance.Currency.CumProfit;
			indicatorProxy.PrintLog(true, !backTest, CurrentBar + "-" + AccName + ": Cum all PnL= " + pl + ", Cum runtime PnL= " + plrt);
			return plrt;
		}
		
		public double CheckPnlByDay(int year, int month, int day) {
			double pnl = 0;
			TradeCollection tc = null;
			DateTime dayKey = new DateTime(year, month, day);//(Time[0].Year,Time[0].Month,Time[0].Day);
			Print(CurrentBar + "-CheckPnlByDay AllTrades(dayKey, ByDay.Count)=" + dayKey + "," + SystemPerformance.AllTrades.ByDay.Count
			+ "RealTimeTrades ByDay.Count=" + SystemPerformance.RealTimeTrades.ByDay.Count);
			if(IsLiveTrading()) {
				if(SystemPerformance.RealTimeTrades.ByDay.Keys.Contains(dayKey))
					tc = (TradeCollection)SystemPerformance.RealTimeTrades.ByDay[dayKey];
			} else {
				if(SystemPerformance.AllTrades.ByDay.Keys.Contains(dayKey))
					tc = (TradeCollection)SystemPerformance.AllTrades.ByDay[dayKey];//Performance.AllTrades.ByDay[dayKey];
			}
			
			if(tc != null) {
				pnl = tc.TradesPerformance.Currency.CumProfit;
				Print(CurrentBar + "-CheckPnlByDay: Count, IsLiveTrading, pnl=" + tc.Count + "," + IsLiveTrading().ToString() + "," + pnl);
			}
			return pnl;
		}
		
		#endregion
		
		#region
		
		#endregion
		
		#region Event Handlers
		
		protected override void OnPositionUpdate(Cbi.Position position, double averagePrice, 
			int quantity, Cbi.MarketPosition marketPosition)
		{			
			int bsx = BarsSinceExitExecution(0, "", 0);
			int bse = BarsSinceEntryExecution(0, "", 0);
			
			Print(CurrentBar + ":OnPositionUpdate- quantity, marketPosition, BarsSinceExit, BarsSinceEntry=" 
			+ quantity + "," + marketPosition + ","
			+ bsx + "," + bse);
			//Print(position.ToString() + "--MarketPosition=" + position.MarketPosition);
			if (position.MarketPosition == MarketPosition.Flat)
			{
				tradeObj.trailingPTTic = MM_ProfitTargetAmt/12.5;
				tradeObj.trailingSLTic = MM_StopLossAmt/12.5;
			} else {
//				SetBracketOrder.OCOOrder.ProfitTargetOrder(OrderSignalName.EntryShort.ToString());
//				SetBracketOrder.OCOOrder.StopLossOrder(OrderSignalName.EntryShort.ToString());
			}
		}
		
		#endregion
		
		#region MM Properties
        [Description("Money amount of profit target")]
 		[Range(0, double.MaxValue), NinjaScriptProperty]
		//[DefaultValueAttribute(300)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitTgtAmt", GroupName = "MoneyMgmt", Order = 0)]	
        public double MM_ProfitTargetAmt
        {
            get{return mm_ProfitTargetAmt;}// { return tradeObj==null? 50 : tradeObj.profitTargetAmt; }
            set{mm_ProfitTargetAmt = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.profitTargetAmt = Math.Max(0, value); }
        }

        [Description("Money amount for profit target increasement")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitTgtIncTic", GroupName = "MoneyMgmt", Order = 1)]	
        public int MM_ProfitTgtIncTic
        {
            get{return mm_ProfitTgtIncTic;}// { return tradeObj==null? 0 : tradeObj.profitTgtIncTic; }
            set{mm_ProfitTgtIncTic = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.profitTgtIncTic = Math.Max(0, value); }
        }
		
        [Description("Tick amount for min profit locking")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitLockMinTic", GroupName = "MoneyMgmt", Order = 2)]	
        public int MM_ProfitLockMinTic
        {
            get{return mm_ProfitLockMinTic;}// { return tradeObj==null? 0 : tradeObj.profitLockMinTic; }
            set{mm_ProfitLockMinTic = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.profitLockMinTic = Math.Max(0, value); }
        }

		[Description("Tick amount for max profit locking")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitLockMaxTic", GroupName = "MoneyMgmt", Order = 3)]	
        public int MM_ProfitLockMaxTic
        {
            get{return mm_ProfitLockMaxTic;}// { return tradeObj==null? 0 : tradeObj.profitLockMaxTic; }
            set{mm_ProfitLockMaxTic = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.profitLockMaxTic = Math.Max(0, value); }
        }
		
        [Description("Money amount of stop loss")]
 		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "StopLossAmt", GroupName = "MoneyMgmt", Order = 4)]	
        public double MM_StopLossAmt
        {
            get{return mm_StopLossAmt;}// { return tradeObj==null? 30 : tradeObj.stopLossAmt; }
            set{mm_StopLossAmt = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.stopLossAmt = Math.Max(0, value); }
        }
		
        [Description("Money amount of trailing stop loss")]
 		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TrailingStopLossAmt", GroupName = "MoneyMgmt", Order = 5)]	
        public double MM_TrailingStopLossAmt
        {
            get{return mm_TrailingStopLossAmt;}// { return tradeObj==null? 0 : tradeObj.trailingSLAmt; }
            set{mm_TrailingStopLossAmt = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.trailingSLAmt = Math.Max(0, value); }
        }
		
		[Description("Money amount for stop loss increasement")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "StopLossIncTic", GroupName = "MoneyMgmt", Order = 6)]	
        public int MM_StopLossIncTic
        {
            get{return mm_StopLossIncTic;}// { return tradeObj==null? 0 : tradeObj.stopLossIncTic; }
            set{mm_StopLossIncTic = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.stopLossIncTic = Math.Max(0, value); }
        }
		
        [Description("Break Even amount")]
 		[Range(0, double.MaxValue), NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BreakEvenAmt", GroupName = "MoneyMgmt", Order = 7)]	
        public double MM_BreakEvenAmt
        {
            get{return mm_BreakEvenAmt;}// { return tradeObj==null? 0 : tradeObj.breakEvenAmt; }
            set{mm_BreakEvenAmt = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.breakEvenAmt = Math.Max(0, value); }
        }

		[Description("Daily Loss Limit amount")]
 		[Range(double.MinValue, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "DailyLossLmt", GroupName = "MoneyMgmt", Order = 8)]	
        public double MM_DailyLossLmt
        {
            get{return mm_DailyLossLmt;}// { return tradeObj==null? 0 : tradeObj.dailyLossLmt; }
            set{mm_DailyLossLmt = Math.Min(-100, value);}// { if(tradeObj!=null) tradeObj.dailyLossLmt = Math.Min(-100, value); }
        }

		[Description("Profit Factor")]
 		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitFactor", GroupName = "MoneyMgmt", Order = 9)]	
        public double MM_ProfitFactor
        {
            get{return mm_ProfitFactor;}// { return tradeObj==null? 0 : tradeObj.dailyLossLmt; }
            set{mm_ProfitFactor = Math.Max(0, value);}// { if(tradeObj!=null) tradeObj.dailyLossLmt = Math.Min(-100, value); }
        }
		
		[Description("Use trailing entry every bar")]
 		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EnTrailing", GroupName = "MoneyMgmt", Order = 10)]	
        public bool MM_EnTrailing
        {
            get{return mm_EnTrailing;}// { return tradeObj==null? false : tradeObj.enTrailing; }
            set{mm_EnTrailing = value;}// { if(tradeObj!=null) tradeObj.enTrailing = value; }
        }
		
		[Description("Use trailing profit target every bar")]
 		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PTTrailing", GroupName = "MoneyMgmt", Order = 11)]	
        public bool MM_PTTrailing
        {
            get{return mm_PTTrailing;}// { return tradeObj==null? false : tradeObj.ptTrailing; }
            set{mm_PTTrailing = value;}// { if(tradeObj!=null) tradeObj.ptTrailing = value; }
        }
		
		[Description("Use trailing stop loss every bar")]
 		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SLTrailing", GroupName = "MoneyMgmt", Order = 12)]	
        public bool MM_SLTrailing
        {
            get{return mm_SLTrailing;}// { return tradeObj==null? false : tradeObj.slTrailing; }
            set{mm_SLTrailing = value;}// { if(tradeObj!=null) tradeObj.slTrailing = value; }
        }

		[Description("Calculation mode for profit target")]
 		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PTCalculationMode", GroupName = "MoneyMgmt", Order = 13)]	
        public CalculationMode MM_PTCalculationMode
        {
            get{return mm_PTCalculationMode;}// { return tradeObj==null? false : tradeObj.slTrailing; }
            set{mm_PTCalculationMode = value;}// { if(tradeObj!=null) tradeObj.slTrailing = value; }
        }

		[Description("Calculation mode for stop loss")]
 		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SLCalculationMode", GroupName = "MoneyMgmt", Order = 14)]	
        public CalculationMode MM_SLCalculationMode
        {
            get{return mm_SLCalculationMode;}// { return tradeObj==null? false : tradeObj.slTrailing; }
            set{mm_SLCalculationMode = value;}// { if(tradeObj!=null) tradeObj.slTrailing = value; }
        }

		[Description("Calculation mode for break even")]
 		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BECalculationMode", GroupName = "MoneyMgmt", Order = 15)]	
        public CalculationMode MM_BECalculationMode
        {
            get{return mm_BECalculationMode;}// { return tradeObj==null? false : tradeObj.slTrailing; }
            set{mm_BECalculationMode = value;}// { if(tradeObj!=null) tradeObj.slTrailing = value; }
        }
		
		#endregion
		
		#region Variables for Properties		
		
		private double mm_ProfitTargetAmt = 500;
		private int mm_ProfitTgtIncTic = 8;
		private int mm_ProfitLockMinTic = 16;
		private int mm_ProfitLockMaxTic = 40;
		private double mm_StopLossAmt = 300;
		private double mm_TrailingStopLossAmt = 16;
		private int mm_StopLossIncTic = 8;
		private double mm_BreakEvenAmt = 150;
		private double mm_DailyLossLmt = -200;
		private double mm_ProfitFactor = 1.5;
		private bool mm_EnTrailing = true;
		private bool mm_PTTrailing = false;
		private bool mm_SLTrailing = false;
		private CalculationMode mm_PTCalculationMode = CalculationMode.Currency;
		private CalculationMode mm_SLCalculationMode = CalculationMode.Currency;	
		private CalculationMode mm_BECalculationMode = CalculationMode.Currency;
		
		#endregion
	}
}
