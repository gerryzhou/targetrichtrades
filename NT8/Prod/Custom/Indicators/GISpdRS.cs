// 
// Copyright (C) 2015, NinjaTrader LLC <www.ninjatrader.com>.
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
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.AddOns;
using NinjaTrader.NinjaScript.AddOns.PriceActions;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Bollinger Bands are plotted at standard deviation levels above and below a moving average. 
	/// Since standard deviation is a measure of volatility, the bands are self-adjusting: 
	/// widening during volatile markets and contracting during calmer periods.
	/// </summary>
	public class GISpdRS : GIndicatorBase
	{
		private SMA		sma;
		private StdDev	stdDev;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "Pair spread by points or ratio";
				Name						= "GISpdRS";
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				NumStdDev					= 1;
				Period						= 20;
				TM_OpenStartH								= 8;
				TM_OpenStartM								= 0;
				TM_OpenEndH									= 9;
				TM_OpenEndM									= 2;
				TM_ClosingH									= 10;
				TM_ClosingM									= 45;

				BarsRequiredToPlot							= 128;
				MaximumBarsLookBack							= MaximumBarsLookBack.Infinite;

				AddPlot(new Stroke(Brushes.Blue), PlotStyle.Dot, "Spread");
				AddPlot(new Stroke(Brushes.Gold), PlotStyle.Dot, "Mean");
				AddPlot(Brushes.Red, "Upper band");
				//AddPlot(Brushes.Orange, "Middle band");
				AddPlot(Brushes.Green, "Lower band");
			}
			else if (State == State.Configure)
			{
				if(ChartMinutes > 0)
					AddDataSeries(SecondSymbol, BarsPeriodType.Minute, ChartMinutes, MarketDataType.Last);
				else 
					AddDataSeries(SecondSymbol, BarsPeriodType.Day, 1, MarketDataType.Last);
				
				sma		= SMA(Spread, Period);
				stdDev	= StdDev(Spread, Period);
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar > BarsRequiredToPlot && BarsInProgress > 0) {
				Spread[0]		= Closes[0][0] - Closes[1][0];
				double sma0		= sma[0];
				double stdDev0	= stdDev[0];
				Upper[0]		= sma0 + NumStdDev * stdDev0;
				Middle[0]		= sma0;
				Lower[0]		= sma0 - NumStdDev * stdDev0;
				CheckTradeEvent();
			}
		}
		
		public void CheckTradeEvent() {
			int en_H = TM_OpenEndH, en_M = TM_OpenEndM, ex_H = TM_ClosingH, ex_M = TM_ClosingM;		
			Print(String.Format("{0}:CheckTradeEvent Bip{1}: en_H={2}, en_M={3}, ex_H={4}, ex_M={5}",
				CurrentBars[BarsInProgress], BarsInProgress, en_H, en_M, ex_H, ex_M));
			
			//entry at 9:02 am ct
			if(IsCutoffTime(BarsInProgress, en_H, en_M)) {
				Print(String.Format("{0}:CheckTradeEvent En Bip{1}: Spread={2}, Upper={3}, Lower={4}, Middle={5}",
				CurrentBars[BarsInProgress], BarsInProgress, Spread[0], Upper[0], Lower[0], Middle[0]));
					
				IndicatorSignal isig = new IndicatorSignal();
				Direction dir = new Direction();

				if(Spread[0] <= Lower[0]) {
					dir.TrendDir = TrendDirection.Up;
					isig.SignalName = SignalName_EntryOnOpenLong;
				} else if(Spread[0] >= Upper[0]) {
					dir.TrendDir = TrendDirection.Down;
					isig.SignalName = SignalName_EntryOnOpenShort;
				}
				
				isig.BarNo = CurrentBars[BarsInProgress];
				isig.TrendDir = dir;
				isig.IndicatorSignalType = SignalType.SimplePriceAction;
				IndicatorEventArgs ievt = new IndicatorEventArgs(this.GetType().Name, String.Format(" [{0}] {1}", Times[BarsInProgress][0], GetLongShortText()));
				ievt.IndSignal = isig;
				//FireEvent(ievt);
				OnRaiseIndicatorEvent(ievt);
			}
			else if(IsCutoffTime(BarsInProgress, ex_H, ex_M)) {
//				Print(String.Format("{0}:CheckTradeEvent Ex Bip{1}: PctSpd={2}, MaxBip={3}, MinBip={4}", 
//				CurrentBars[BarsInProgress], BarsInProgress, PlotPctSpd[0], PctChgMaxBip, PctChgMinBip));
				Print(String.Format("{0}:CheckTradeEvent Ex Bip{1}: Spread={2}, Upper={3}, Lower={4}, Middle={5}",
				CurrentBars[BarsInProgress], BarsInProgress, Spread[0], Upper[0], Lower[0], Middle[0]));
					
				IndicatorSignal isig = new IndicatorSignal();
				Direction dir = new Direction();

				dir.TrendDir = TrendDirection.UnKnown;
				isig.SignalName = SignalName_ExitForOpen;			
				isig.BarNo = CurrentBars[BarsInProgress];
				isig.TrendDir = dir;
				isig.IndicatorSignalType = SignalType.SimplePriceAction;
				IndicatorEventArgs ievt = new IndicatorEventArgs(this.GetType().Name, " CheckTradeEvent Ex: ");
				ievt.IndSignal = isig;
				//FireEvent(ievt);
				OnRaiseIndicatorEvent(ievt);
			}
		}
		
		private string GetLongShortText() {
			String txt = "N/A";
			if(Spread[0] != null) {
				if(Spread[0] < Middle[0]) {
					txt = "L " + Instrument.MasterInstrument.Name + " : S " + SecondSymbol;
				} else {
					txt = "S " + Instrument.MasterInstrument.Name + " : L " + SecondSymbol;
				}
			}
			return txt;
		}
		
		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Spread
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Middle
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Upper
		{
			get { return Values[2]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Lower
		{
			get { return Values[3]; }
		}
		
		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NumStdDev", GroupName = "NinjaScriptParameters", Order = 0)]
		public double NumStdDev
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 1)]
		public int Period
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="SecondSymbol", Description="The second symbol of the pair", Order=2, GroupName="NinjaScriptParameters")]
		public string SecondSymbol
		{ 	get{
				return secondSymbol;
			}
			set{
				secondSymbol = value;
			}
		}
		
		[Range(-1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ChartMinutes", Description="Minutes for the chart", Order=3, GroupName="NinjaScriptParameters")]
		public int ChartMinutes
		{ 	get{
				return chartMinutes;
			}
			set{
				chartMinutes = value;
			}
		}
		#endregion
		
		#region Pre Defined parameters
		private int rocPeriod = 8;
		private double capRatio1 = 1.25;
		private double capRatio2 = 1;		
		private string secondSymbol = "QQQ";
		private int chartMinutes = 4;
		private double pctChgSpdThresholdEn = -2.3;
		private double pctChgSpdThresholdEx = 2.5;
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GISpdRS[] cacheGISpdRS;
		public GISpdRS GISpdRS(double numStdDev, int period, string secondSymbol, int chartMinutes)
		{
			return GISpdRS(Input, numStdDev, period, secondSymbol, chartMinutes);
		}

		public GISpdRS GISpdRS(ISeries<double> input, double numStdDev, int period, string secondSymbol, int chartMinutes)
		{
			if (cacheGISpdRS != null)
				for (int idx = 0; idx < cacheGISpdRS.Length; idx++)
					if (cacheGISpdRS[idx] != null && cacheGISpdRS[idx].NumStdDev == numStdDev && cacheGISpdRS[idx].Period == period && cacheGISpdRS[idx].SecondSymbol == secondSymbol && cacheGISpdRS[idx].ChartMinutes == chartMinutes && cacheGISpdRS[idx].EqualsInput(input))
						return cacheGISpdRS[idx];
			return CacheIndicator<GISpdRS>(new GISpdRS(){ NumStdDev = numStdDev, Period = period, SecondSymbol = secondSymbol, ChartMinutes = chartMinutes }, input, ref cacheGISpdRS);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GISpdRS GISpdRS(double numStdDev, int period, string secondSymbol, int chartMinutes)
		{
			return indicator.GISpdRS(Input, numStdDev, period, secondSymbol, chartMinutes);
		}

		public Indicators.GISpdRS GISpdRS(ISeries<double> input , double numStdDev, int period, string secondSymbol, int chartMinutes)
		{
			return indicator.GISpdRS(input, numStdDev, period, secondSymbol, chartMinutes);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GISpdRS GISpdRS(double numStdDev, int period, string secondSymbol, int chartMinutes)
		{
			return indicator.GISpdRS(Input, numStdDev, period, secondSymbol, chartMinutes);
		}

		public Indicators.GISpdRS GISpdRS(ISeries<double> input , double numStdDev, int period, string secondSymbol, int chartMinutes)
		{
			return indicator.GISpdRS(input, numStdDev, period, secondSymbol, chartMinutes);
		}
	}
}

#endregion
