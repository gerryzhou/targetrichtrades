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
	public class GIATRRatio : GIndicatorBase
	{
		//private SMA		sma;
		//private StdDev	stdDev;
		private Series<double>		atr1;
		private Series<double>		atr2;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "ATR ratio by Data1 and Data2";
				Name						= "GIATRRatio";
				IsOverlay					= false;
				IsSuspendedWhileInactive	= true;
				Calculate					= Calculate.OnPriceChange;
				
				ATRPeriod						= 20;
				TM_OpenStartH								= 8;
				TM_OpenStartM								= 0;
				TM_OpenEndH									= 8;
				TM_OpenEndM									= 34;
				TM_ClosingH									= 10;
				TM_ClosingM									= 45;

				BarsRequiredToPlot							= 128;
				MaximumBarsLookBack							= MaximumBarsLookBack.Infinite;

				AddPlot(new Stroke(Brushes.Blue), PlotStyle.Dot, "ATRRatio");
				//AddPlot(new Stroke(Brushes.Gold), PlotStyle.Dot, "Mean");
				//AddPlot(Brushes.Red, "Upper band");
				//AddPlot(Brushes.Orange, "Middle band");
				//AddPlot(Brushes.Green, "Lower band");
			}
			else if (State == State.Configure)
			{
				if(ChartMinutes > 0)
					AddDataSeries(SecondSymbol, BarsPeriodType.Minute, ChartMinutes, MarketDataType.Last);
				else 
					AddDataSeries(SecondSymbol, BarsPeriodType.Day, 1, MarketDataType.Last);
				
				//Spread[0] = Closes[0][0] - Closes[1][0];
				//sma		= SMA(Spread, Period);
				//stdDev	= StdDev(Spread, Period);
			}
			else if (State == State.DataLoaded)
			{
				//UpperMin = new Series<double>(this);
				//LowerMin = new Series<double>(this);
				atr1		= new Series<double>(this);
				atr2		= new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			int barRequired = Math.Max(BarsRequiredToPlot,ATRPeriod);
			if(BarsInProgress == 0) {
				double high00	= Highs[0][0];
				double low00	= Lows[0][0];

				if(CurrentBars[0] < barRequired) return;
				else if (CurrentBars[0] == barRequired)
					atr1[0] = high00 - low00;
				else
				{
					double close01		= Closes[0][1];
					double trueRange0	= Math.Max(Math.Abs(low00 - close01), Math.Max(high00 - low00, Math.Abs(high00 - close01)));
					atr1[0]			= ((Math.Min(CurrentBars[0] + 1, ATRPeriod) - 1 ) * atr1[1] + trueRange0) / Math.Min(CurrentBars[0] + 1, ATRPeriod);
				}
			}
			else if(BarsInProgress == 1) {
				double high10	= Highs[1][0];
				double low10	= Lows[1][0];

				if(CurrentBars[1] < barRequired) return;
				else if (CurrentBars[1] == barRequired)
					atr2[0] = high10 - low10;
				else
				{
					double close11		= Closes[1][1];
					double trueRange1	= Math.Max(Math.Abs(low10 - close11), Math.Max(high10 - low10, Math.Abs(high10 - close11)));
					atr2[0]			= ((Math.Min(CurrentBars[1] + 1, ATRPeriod) - 1 ) * atr2[1] + trueRange1) / Math.Min(CurrentBars[1] + 1, ATRPeriod);
				}
			}
			if(CurrentBars[0] > barRequired 
				&& CurrentBars[1] > barRequired) {
				if(BarsInProgress > 0) {
//				Print(string.Format("CurrentBars[BarsInProgress]={0}, BarsInProgress={1}, atr1[0]={2}, atr2[0]={3}",
//					CurrentBars[BarsInProgress], BarsInProgress, atr1[0], atr2[0]));//, Closes[1][0]));
				if(atr1[0] != 0)
					ATRRatio[0] = Math.Round(atr2[0]/atr1[0], 4);
				}
			} else {
				
			}
		}
		
		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ATRRatio
		{
			get { return Values[0]; }
		}

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ATRPeriod", GroupName = "NinjaScriptParameters", Order = 4)]
		public int ATRPeriod
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="SecondSymbol", Description="The second symbol of the pair", GroupName="NinjaScriptParameters", Order=5)]
		public string SecondSymbol
		{ 	get{ return secondSymbol; }
			set{ secondSymbol = value; }
		}
		
		[Range(-1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ChartMinutes", Description="Minutes for the chart", GroupName="NinjaScriptParameters", Order=6)]
		public int ChartMinutes
		{ 	get{ return chartMinutes; }
			set{ chartMinutes = value; }
		}

		#endregion
		
		#region Pre Defined parameters
		private int rocPeriod = 20;	
		private string secondSymbol = "QQQ";
		private int chartMinutes = 4;
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIATRRatio[] cacheGIATRRatio;
		public GIATRRatio GIATRRatio(int aTRPeriod, string secondSymbol, int chartMinutes)
		{
			return GIATRRatio(Input, aTRPeriod, secondSymbol, chartMinutes);
		}

		public GIATRRatio GIATRRatio(ISeries<double> input, int aTRPeriod, string secondSymbol, int chartMinutes)
		{
			if (cacheGIATRRatio != null)
				for (int idx = 0; idx < cacheGIATRRatio.Length; idx++)
					if (cacheGIATRRatio[idx] != null && cacheGIATRRatio[idx].ATRPeriod == aTRPeriod && cacheGIATRRatio[idx].SecondSymbol == secondSymbol && cacheGIATRRatio[idx].ChartMinutes == chartMinutes && cacheGIATRRatio[idx].EqualsInput(input))
						return cacheGIATRRatio[idx];
			return CacheIndicator<GIATRRatio>(new GIATRRatio(){ ATRPeriod = aTRPeriod, SecondSymbol = secondSymbol, ChartMinutes = chartMinutes }, input, ref cacheGIATRRatio);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIATRRatio GIATRRatio(int aTRPeriod, string secondSymbol, int chartMinutes)
		{
			return indicator.GIATRRatio(Input, aTRPeriod, secondSymbol, chartMinutes);
		}

		public Indicators.GIATRRatio GIATRRatio(ISeries<double> input , int aTRPeriod, string secondSymbol, int chartMinutes)
		{
			return indicator.GIATRRatio(input, aTRPeriod, secondSymbol, chartMinutes);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIATRRatio GIATRRatio(int aTRPeriod, string secondSymbol, int chartMinutes)
		{
			return indicator.GIATRRatio(Input, aTRPeriod, secondSymbol, chartMinutes);
		}

		public Indicators.GIATRRatio GIATRRatio(ISeries<double> input , int aTRPeriod, string secondSymbol, int chartMinutes)
		{
			return indicator.GIATRRatio(input, aTRPeriod, secondSymbol, chartMinutes);
		}
	}
}

#endregion
