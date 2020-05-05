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
				//IsInstantiatedOnEachOptimizationIteration = false;
			}
			else if (State == State.Configure)
			{
				// Add an MSFT 1 minute Bars object to the strategy
				//AddDataSeries("NQ 06-20", Data.BarsPeriodType.Minute, 13);
				AddDataSeries("NQ 06-20", Data.BarsPeriodType.Minute, 13);
				AddDataSeries("RTY 06-20", Data.BarsPeriodType.Minute, 13);
				// Sets a 20 tick trailing stop for an open position
				SetTrailStop(CalculationMode.Ticks, 20);
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
				Print(String.Format("{0}: DataLoaded...BarsArray.Length={1}", this.GetType().Name, BarsArray.Length));
			}			
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < BarsRequiredToTrade)
				return;

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
					Print(String.Format("{0}: IsUnmanaged={1}", CurrentBar, IsUnmanaged));
					//EnterLong(1);
					EnterLong(0, 1, "RSI");
					EnterShort(1, 1, "RSI");
					EnterShort(2, 1, "RSI");
				}
			}

			// Any open long position will exit if RSI crosses below a value of 75
			// This is in addition to the trail stop set in the OnStateChange() method under State.Configure
			if (CrossBelow(rsi, 75, 1)) {
				//ExitLong();
				ExitLong(0, 1, "ExitRSI", "RSI");
				ExitShort(1, 1, "ExitRSI", "RSI");
				ExitShort(2, 1, "ExitRSI", "RSI");
			}
		}
	}
}
