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
	public class StgSample01 : GStrategyBase
	{		
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				Print(this.Name + " set defaults called....");
				Description									= @"The sample strategy for GSZTrader.";
				Name										= "StgSample01";
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
				AddChartIndicator(indicatorProxy);
				SetPrintOut(1);
			}
			else if (State == State.Configure)
			{
				Print(this.Name + " set Configure called....");
				tradeObj = new TradeObj(this);				
			}
		}

		/// <summary>
		/// 
		/// </summary>
		protected override void OnBarUpdate()
		{
			base.OnBarUpdate();
			indicatorProxy.TraceMessage(this.Name, PrintOut);
		}
		
		public override TradeSignal GetTradeSignal() {
			indicatorProxy.TraceMessage(this.Name, PrintOut);
			TradeSignal trdSignal = new TradeSignal();
			Direction dir = new Direction();
			
			if(Close[0] > High[1] && Close[0] > High[2])
				dir.TrendDir = TrendDirection.Up;

			if(Close[0] < Low[1] && Close[0] < Low[2])
				dir.TrendDir = TrendDirection.Down;
			trdSignal.TrendDir = dir;
			
			this.tradeSignal = trdSignal;
			return trdSignal;
		}
		
		public override TradeObj CheckNewEntryTrade() {
			indicatorProxy.TraceMessage(this.Name, PrintOut);
			tradeObj.InitNewEntryTrade();
			if(tradeSignal != null) {
				if(tradeSignal.TrendDir.TrendDir == TrendDirection.Down)
				{
					indicatorProxy.TraceMessage(this.Name, PrintOut);
					tradeObj.tradeDirection = TradingDirection.Down;
				}
				else if(tradeSignal.TrendDir.TrendDir == TrendDirection.Up)
				{
					indicatorProxy.TraceMessage(this.Name, PrintOut);
					tradeObj.tradeDirection = TradingDirection.Up;
				}
				
				tradeObj.tradeStyle = TradingStyle.TrendFollowing;
				
			} else {
				tradeObj.SetTradeType(TradeType.NoTrade);
			}
			return tradeObj;
		}
		
		public override void PutTrade(){
			indicatorProxy.TraceMessage(this.Name, PrintOut);
			if(tradeObj.GetTradeType() == TradeType.Entry) {
				indicatorProxy.PrintLog(true, IsLiveTrading(), "PutTrade tradeObj.stopLossAmt=" + tradeObj.stopLossAmt + "," + MM_StopLossAmt);
				if(tradeObj.tradeDirection == TradingDirection.Down) {
					indicatorProxy.PrintLog(true, IsLiveTrading(), "PutTrade Down OrderSignalName=" + tradeObj.entrySignalName);
					tradeObj.enLimitPrice = Close[0];
					NewShortLimitOrderUM(OrderSignalName.EntryShortLmt.ToString());
				}
				else if(tradeObj.tradeDirection == TradingDirection.Up) {
					indicatorProxy.PrintLog(true, IsLiveTrading(), "PutTrade Up OrderSignalName=" + tradeObj.entrySignalName);
					tradeObj.enLimitPrice = Close[0];
					NewLongLimitOrderUM(OrderSignalName.EntryLongLmt.ToString());
					//EnterLongLimit(Low[0]-5, OrderSignalName.EntryLong.ToString());
				}				
			}
		}
	}
}
