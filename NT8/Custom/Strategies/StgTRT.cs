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
		private AwesomeOscillator awOscillator;
		
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
				giSMI = GISMI(3, 5, 5, 8);
				awOscillator = AwesomeOscillator(5, 34, 5, MovingAvgType.SMA);
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
			GetDirection();
		}
		
		public override Direction GetDirection(){
			Print(CurrentBar.ToString() + " -- override Direction GetDirection called...");
			return null;
		}
	}
}
