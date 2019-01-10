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
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;

#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class StgTRT : GSZTraderBase //Strategy
	{
		private GISMI giSMI;
		private GIAwesomeOscillator awOscillator;
		
		protected override void OnStateChange()
		{
			//base.OnStateChange();
			if (State == State.SetDefaults)
			{
				Print("StgTRT set defaults called....");
				Description									= @"Traget Rich Trade.";
				Name										= "StgTRT";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
			}
			else if (State == State.DataLoaded)
			{
				indicatorProxy = new GIndicatorBase();
				giSMI = GISMI(3, 5, 5, 8);
				awOscillator = GIAwesomeOscillator(5, 34, 5, MovingAvgType.SMA);
				//smaSlow = SMA(Slow);

				//giSMI.Plots[0].Brush = Brushes.Blue;
				
				//smaFast.Plots[0].Brush = Brushes.Goldenrod;
				//smaSlow.Plots[0].Brush = Brushes.SeaGreen;

				AddChartIndicator(giSMI);
				AddChartIndicator(awOscillator);
				//AddChartIndicator(smaFast);
				//AddChartIndicator(smaSlow);
			}			
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			base.OnBarUpdate();
			Print(CurrentBar.ToString() + " -- StgTRT - Add your custom strategy logic here.");
			indicatorProxy.Update();
			GetIndicatorSignal();
		}
		
		public override Direction GetDirection(GIndicatorBase ind){
			Print(CurrentBar.ToString() + " -- StgTRT override GetDirection: " + ind.GetType().Name + " called...");
			//ind.Update();
			return ind.GetDirection();
//			Direction dir_gismi = giSMI.GetDirection();
//			Direction dir_awo = awOscillator.GetDirection();
//			return null;
		}
		
		public override IndicatorSignal GetIndicatorSignal() {
			giSMI.Update();
			awOscillator.Update();
			//isolate the last inflection
			int inft = giSMI.GetLastInflection(giSMI.GetInflection(), CurrentBar, TrendDirection.Down);
			
			//lookback to the crossover and if that candle is bearish we isolate the open as resistance;
			// if that candlestick is bullish we isolate the close as resistance
			
			
			//SMI below the moving average or not
			Direction dir_gismi = GetDirection(giSMI);
			
			//awesome oscillator below zero or not
			Direction dir_awo = GetDirection(awOscillator);
			
			return null;
		}
	}
}
