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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Exponential Moving Average. The Exponential Moving Average is an indicator that
	/// shows the average value of a security's price over a period of time. When calculating
	/// a moving average. The EMA applies more weight to recent prices than the SMA.
	/// </summary>
	public class GIHLnBars : GIndicatorBase
	{
//		private double constant1;
//		private double constant2;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "The High/Low of last N bars";
				Name						= "GIHLnBars";
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				Period						= 5;

				AddPlot(Brushes.Goldenrod, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameEMA);
			}
			else if (State == State.Configure)
			{
//				constant1 = 2.0 / (1 + Period);
//				constant2 = 1 - (2.0 / (1 + Period));
			}
		}

		protected override void OnBarUpdate()
		{
			Values[0][0] = (CurrentBar == 0 ? Low[0] : GetLowestPrice(Period, false));
			Values[1][0] = (CurrentBar == 0 ? High[0] : GetHighestPrice(Period, false));
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LowestN
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HighestN
		{
			get { return Values[1]; }
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIHLnBars[] cacheGIHLnBars;
		public GIHLnBars GIHLnBars(int period)
		{
			return GIHLnBars(Input, period);
		}

		public GIHLnBars GIHLnBars(ISeries<double> input, int period)
		{
			if (cacheGIHLnBars != null)
				for (int idx = 0; idx < cacheGIHLnBars.Length; idx++)
					if (cacheGIHLnBars[idx] != null && cacheGIHLnBars[idx].Period == period && cacheGIHLnBars[idx].EqualsInput(input))
						return cacheGIHLnBars[idx];
			return CacheIndicator<GIHLnBars>(new GIHLnBars(){ Period = period }, input, ref cacheGIHLnBars);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIHLnBars GIHLnBars(int period)
		{
			return indicator.GIHLnBars(Input, period);
		}

		public Indicators.GIHLnBars GIHLnBars(ISeries<double> input , int period)
		{
			return indicator.GIHLnBars(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIHLnBars GIHLnBars(int period)
		{
			return indicator.GIHLnBars(Input, period);
		}

		public Indicators.GIHLnBars GIHLnBars(ISeries<double> input , int period)
		{
			return indicator.GIHLnBars(input, period);
		}
	}
}

#endregion
