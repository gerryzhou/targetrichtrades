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
	/// 4) CheckNewEntryTrade();
	/// 5) PutTrade();
	/// </summary>
	public class StgSample02 : GStrategyBase
	{
		private double c0 = 0, hi3 = Double.MaxValue, lo3 = Double.MinValue;
		
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				Print(this.Name + " set defaults called....");
				Description									= @"The sample strategy for GSZTrader.";
				Name										= "StgSample02";
				Calculate									= Calculate.OnBarClose;
				IsFillLimitOnTouch							= false;
				TraceOrders									= false;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
			}
			else if (State == State.DataLoaded)
			{
				Print(this.Name + " set DataLoaded called....");
				AddChartIndicator(IndicatorProxy);
				SetPrintOut(1);
			}
			else if (State == State.Configure)
			{
				Print(this.Name + " set Configure called....");
				//CurrentTrade = new CurrentTrade(this);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		protected override void OnBarUpdate()
		{
			try {
			base.OnBarUpdate();
			IndicatorProxy.TraceMessage(this.Name, PrintOut);
			} catch (Exception ex) {
				IndicatorProxy.Log2Disk = true;
				IndicatorProxy.PrintLog(true, true, "Exception: " + ex.StackTrace);
			}
		}
		
		public override TradeSignal GetTradeSignal() {
			IndicatorProxy.TraceMessage(this.Name, PrintOut);
			TradeSignal trdSignal = new TradeSignal();
			Direction dir = new Direction();
			
			c0 = Close[0];
			
			Print(CurrentBar + ":"
			+ ";c0=" + c0
			+ ";hi3=" + hi3
			+ ";lo3=" + lo3
			+ ";BarsLookback=" + BarsLookback);
			
			if(c0 > hi3)
				dir.TrendDir = TrendDirection.Up;

			if(c0 < lo3)
				dir.TrendDir = TrendDirection.Down;
//			trdSignal.TrendDir = dir;
			
//			this.AddTradeSignal(CurrentBar, trdSignal);
			hi3 = GetHighestPrice(BarsLookback);
			lo3 = GetLowestPrice(BarsLookback);
			
			return trdSignal;
		}
		
		public override bool CheckNewEntryTrade() {
			IndicatorProxy.TraceMessage(this.Name, PrintOut);
			CurrentTrade.InitNewEntryTrade();
//			if(GetTradeSignal(CurrentBar) != null) {
//				if(GetTradeSignal(CurrentBar).TrendDir.TrendDir == TrendDirection.Down)
//				{
//					indicatorProxy.TraceMessage(this.Name, PrintOut);
//					CurrentTrade.TradeDirection = TradingDirection.Down;
//				}
//				else if(GetTradeSignal(CurrentBar).TrendDir.TrendDir == TrendDirection.Up)
//				{
//					indicatorProxy.TraceMessage(this.Name, PrintOut);
//					CurrentTrade.TradeDirection = TradingDirection.Up;
//				}
				
//				CurrentTrade.tradeStyle = TradingStyle.TrendFollowing;
				
//			} else {
//				CurrentTrade.CurrentTradeType = TradeType.NoTrade;
//			}
			return false;
		}
		
		public override void PutTrade(){
			IndicatorProxy.TraceMessage(this.Name, PrintOut);
			if(CurrentTrade.TradeAction.TradeActionType == TradeActionType.EntrySimple) {
				IndicatorProxy.PrintLog(true, IsLiveTrading(), "PutTrade CurrentTrade.stopLossAmt=" + CurrentTrade.stopLossAmt + "," + MM_StopLossAmt);
				if(CurrentTrade.TradeDirection == TradingDirection.Down) {
					IndicatorProxy.PrintLog(true, IsLiveTrading(), "PutTrade Down OrderSignalName=" + CurrentTrade.TradeAction.EntrySignal.SignalName);
					CurrentTrade.TradeAction.EntryPrice = GetTypicalPrice(0);
					NewShortLimitOrderUM(OrderSignalName.EntryShortLmt.ToString());
				}
				else if(CurrentTrade.TradeDirection == TradingDirection.Up) {
					IndicatorProxy.PrintLog(true, IsLiveTrading(), "PutTrade Up OrderSignalName=" + CurrentTrade.TradeAction.EntrySignal.SignalName);
					CurrentTrade.TradeAction.EntryPrice = GetTypicalPrice(0);
					NewLongLimitOrderUM(OrderSignalName.EntryLongLmt.ToString());
				}				
			}
		}
		
		#region Properties
		[Description("Bars lookback period")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarsLookback", GroupName = "CustomParams", Order = 0)]
        public int BarsLookback
        {
            get { return barsLookback; }
            set { barsLookback = Math.Max(1, value); }
        }
		
		private int barsLookback = 1;// 15;
		
		#endregion
	}
}
