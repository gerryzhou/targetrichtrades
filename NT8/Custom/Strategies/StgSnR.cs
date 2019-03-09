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
using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.NinjaScript.Strategies.ZTraderStg;

#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class StgSnR : GSZTraderBase
	{
		private GISMI giSMI;
		private GISnR giSnR;
		
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				Description									= @"Strategy based on support and resistance, combined with fibonacci ratios.";
				Name										= "StgSnR";
				Calculate									= Calculate.OnBarClose;
//				EntriesPerDirection							= 1;
//				EntryHandling								= EntryHandling.AllEntries;
//				IsExitOnSessionCloseStrategy				= true;
//				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
//				MaximumBarsLookBack							= MaximumBarsLookBack.Infinite;
//				OrderFillResolution							= OrderFillResolution.Standard;
//				Slippage									= 0;
//				StartBehavior								= StartBehavior.WaitUntilFlat;
//				TimeInForce									= TimeInForce.Day;
				TraceOrders									= false;
//				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
//				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
//				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
//				IsInstantiatedOnEachOptimizationIteration	= true;
				ShowLastDayHL								= true;
				ShowOvernightHL							= true;
				//AddPlot(new Stroke(Brushes.LimeGreen, 2), PlotStyle.Hash, "Spt");
				//AddPlot(new Stroke(Brushes.Tomato, 2), PlotStyle.Hash, "Rst");
			}
			else if (State == State.DataLoaded)
			{
				//indicatorProxy = new GIndicatorBase();
				//indicatorProxy = GIndicatorProxy(1);
				giSMI = GISMI(3, 5, 5, 8);
				giSnR = GISnR(ShowOvernightHL, true, ShowLastDayHL);
				//awOscillator = GIAwesomeOscillator(5, 34, 5, MovingAvgType.SMA);

				AddChartIndicator(giSMI);
				AddChartIndicator(giSnR);
				AddChartIndicator(indicatorProxy);
			}			
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Day, 1);
				//IncludeCommission = true;
				tradeObj = new TradeObj(this);
				tradeObj.barsSincePTSL = TM_BarsSincePTSL;
			}			
		}

		protected override void OnBarUpdate()
		{	
			base.OnBarUpdate();
			giSMI.Update();
			if (BarsInProgress == 1 || BarsInProgress == 2)
   			return;
			giSnR.Update();
			//Add your custom strategy logic here.
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="ShowLastDayHL", Description="Show High/Low of last day", Order=1, GroupName="CustomParams")]
		public bool ShowLastDayHL
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowOvernightHL", Description="Show the overnight High/Low", Order=2, GroupName="CustomParams")]
		public bool ShowOvernightHL
		{ get; set; }

//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> Spt
//		{
//			get { return Values[0]; }
//		}

//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> Rst
//		{
//			get { return Values[1]; }
//		}
		#endregion

	}
}
