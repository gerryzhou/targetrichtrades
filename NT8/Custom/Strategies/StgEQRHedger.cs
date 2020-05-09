//
// Copyright (C) 2019, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Indicators.PriceActions;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds strategies in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Strategies
{
	/// <summary>
	/// Hedge by itself: one trend following, the other is counter trend;
	/// </summary>
	public class StgEQRHedger : GStrategyBase
	{
		private RSI rsi;
		private ADX adx;
		private ADX adx1;

		private GIPctSpd giPctSpd;

		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				Description	= "Hedge ES, NQ, and RTY";
				Name		= "StgEQRHedger";
				// This strategy has been designed to take advantage of performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				Calculate									= Calculate.OnBarClose;
				//IsFillLimitOnTouch							= false;
				TraceOrders									= false;
				BarsRequiredToTrade							= 22;
				IsUnmanaged									= false;
				OrderFillResolution							= OrderFillResolution.Standard;
				EntriesPerDirection							= 1;
				DefaultQuantity								= 5;
				MM_ProfitFactorMax							= 1;
				MM_ProfitFactorMin							= 0;
				//IsInstantiatedOnEachOptimizationIteration = false;
			}
			else if (State == State.Configure)
			{
				// Add an MSFT 1 minute Bars object to the strategy
				//AddDataSeries("NQ 06-20", Data.BarsPeriodType.Minute, 13);
				AddDataSeries("NQ 06-20", Data.BarsPeriodType.Minute, 13);
				AddDataSeries("RTY 06-20", Data.BarsPeriodType.Minute, 13);
				SetOrderQuantity = SetOrderQuantity.Strategy; // calculate orders based off default size
				// Sets a 20 tick trailing stop for an open position
				//SetTrailStop(CalculationMode.Ticks, 200);
			}
			else if (State == State.DataLoaded)
			{
				rsi = RSI(14, 1);
				adx = ADX(14);
				giPctSpd = GIPctSpd(8);
				// Add RSI and ADX indicators to the chart for display
				// This only displays the indicators for the primary Bars object (main instrument) on the chart
				AddChartIndicator(rsi);
				AddChartIndicator(adx);
				AddChartIndicator(giPctSpd);
				
				giPctSpd.RaiseIndicatorEvent += OnTradeByPctSpd;
				SetPrintOut(1);
				Print(String.Format("{0}: IsUnmanaged={1}", this.GetType().Name, IsUnmanaged));
				Print(String.Format("{0}: DataLoaded...BarsArray.Length={1}", this.GetType().Name, BarsArray.Length));
			}			
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < BarsRequiredToTrade)
				return;
			giPctSpd.Update();
			IndicatorProxy.Update();
//			if(BarsInProgress == BarsArray.Length-1)
//				OnTradeByPctSpd();
			// Note: Bars are added to the BarsArray and can be accessed via an index value
			// E.G. BarsArray[1] ---> Accesses the 1 minute Bars added above
			if (adx1 == null)
				adx1 = ADX(BarsArray[1], 14);

			// OnBarUpdate() will be called on incoming tick events on all Bars objects added to the strategy
			// We only want to process events on our primary Bars object (main instrument) (index = 0) which
			// is set when adding the strategy to a chart
			if (BarsInProgress != 0)
				return;
			if (CurrentBars[0] < 0 || CurrentBars[1] < 0)
				return;

			// Checks if the 14 period ADX on both instruments are trending (above a value of 30)
			if (adx[0] > 30 && adx1[0] > 30)
			{
				// If RSI crosses above a value of 30 then enter a long position via a limit order
				if (CrossAbove(rsi, 30, 1))
				{
					// Draws a square 1 tick above the high of the bar identifying when a limit order is issued
					Draw.Square(this, "My Square" + CurrentBar, false, 0, High[0] + TickSize, Brushes.DodgerBlue);

					// Enter a long position via a limit order at the current ask price
					//EnterLongLimit(GetCurrentAsk(), "RSI");					
					//EnterLong(1);
//					EnterLong(0, 1, "RSI");
//					EnterShort(1, 1, "RSI");
//					EnterShort(2, 1, "RSI");
				}
			}

			// Any open long position will exit if RSI crosses below a value of 75
			// This is in addition to the trail stop set in the OnStateChange() method under State.Configure
			if (CrossBelow(rsi, 75, 1)) {
				//ExitLong();
//				ExitLong(0, 1, "ExitRSI", "RSI");
//				ExitShort(1, 1, "ExitRSI", "RSI");
//				ExitShort(2, 1, "ExitRSI", "RSI");
			}
		}
		#region Indicator Event Handler
        // Define what actions to take when the event is raised.
        void OnTradeByPctSpd(object sender, IndicatorEventArgs e) {
			IndicatorSignal isig = e.IndSignal;
			Print(String.Format("{0}:OnTradeByPctSpd triggerred {1} Bip{2}: PctSpd={3}, MaxBip={4}, MinBip={5}",
			CurrentBars[BarsInProgress], isig.SignalName, BarsInProgress, giPctSpd.PlotPctSpd[0], giPctSpd.PctChgMaxBip, giPctSpd.PctChgMinBip));
			int q_max = GetTradeQuantity(giPctSpd.PctChgMaxBip, this.MM_ProfitFactorMax);
			int q_min = GetTradeQuantity(giPctSpd.PctChgMinBip, this.MM_ProfitFactorMin);
			
			//exit at 9:40 am ct
			if(isig.SignalName == giPctSpd.SignalName_ExitForOpen) {
				Print(String.Format("{0}:OnTradeByPctSpd Ex Bip={1}: MaxBip={2}, PosMax={3},  MinBip={4}, PosMin={5}", 
				CurrentBars[BarsInProgress], BarsInProgress, giPctSpd.PctChgMaxBip, Positions[giPctSpd.PctChgMaxBip], giPctSpd.PctChgMinBip, Positions[giPctSpd.PctChgMinBip]));
				
				if(Positions[giPctSpd.PctChgMaxBip].MarketPosition == MarketPosition.Long) {
					Print(String.Format("{0}:OnTradeByPctSpd ExLn Bip={1}: MaxBipQuant={2}, MinBipQuant={3}", 
					CurrentBars[BarsInProgress], BarsInProgress,
					q_max, q_min));
					ExitLong(giPctSpd.PctChgMaxBip, q_max, "GIExLn", String.Empty);
					ExitShort(giPctSpd.PctChgMinBip, q_min, "GIExSt", String.Empty);
				}
				//else if(isig.TrendDir.TrendDir == TrendDirection.Down) {
				else if(Positions[giPctSpd.PctChgMaxBip].MarketPosition == MarketPosition.Short) {
					Print(String.Format("{0}:OnTradeByPctSpd ExSt Bip={1}: MaxBipQuant={2}, MinBipQuant={3}", 
					CurrentBars[BarsInProgress], BarsInProgress,
					q_max, q_min));
					ExitShort(giPctSpd.PctChgMaxBip, q_max, "GIExSt", String.Empty);
					ExitLong(giPctSpd.PctChgMinBip, q_min, "GIExLn", String.Empty);
				}

			} else { //entry at 9:02 am ct
				Print(String.Format("{0}:OnTradeByPctSpd En Bip={1}: PctSpd={2}, MaxBip={3}, MinBip={4}", 
				CurrentBar, BarsInProgress, giPctSpd.PlotPctSpd[0], giPctSpd.PctChgMaxBip, giPctSpd.PctChgMinBip));
				if(isig.TrendDir.TrendDir == TrendDirection.Up) {
					IndicatorProxy.PrintLog(true, IsLiveTrading(),String.Format("{0}:{1} Ln Bip={2}: PctSpd={3}, MaxBipQuant={4}, MinBipQuant={5}", 
					CurrentBars[BarsInProgress], e.Message, BarsInProgress, giPctSpd.PlotPctSpd[0],
					q_max, q_min));
					EnterLong(giPctSpd.PctChgMaxBip, q_max, "GIPctSpd");
					EnterShort(giPctSpd.PctChgMinBip, q_min, "GIPctSpd");
				}
				else if(isig.TrendDir.TrendDir == TrendDirection.Down) {
					IndicatorProxy.PrintLog(true, IsLiveTrading(),String.Format("{0}:{1} St Bip={2}: PctSpd={3}, MaxBipQuant={4}, MinBipQuant={5}", 
					CurrentBars[BarsInProgress], e.Message, BarsInProgress, giPctSpd.PlotPctSpd[0],
					q_max, q_min));
					EnterShort(giPctSpd.PctChgMaxBip, q_max, "GIPctSpd");
					EnterLong(giPctSpd.PctChgMinBip, q_min, "GIPctSpd");
				}
			}
			
		}
		
		/// <summary>
		/// CapRatio: ES:RTY=1.7:1, NQ:RTY=2.1:1, NQ:ES=1.25:1
		/// </summary>		
		private int GetTradeQuantity(int idx, double ratio) {
			switch(idx) {
				case 0: return ratio>0? (int)ratio*5 : 5;
				case 1: return ratio>0? (int)ratio*4 :4;
				case 2: return ratio>0? (int)ratio*8 :8;
				default: return -1;
			}
		}
		
		#endregion
	}
}
