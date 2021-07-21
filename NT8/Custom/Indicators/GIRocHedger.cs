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
	/// The ROC (Rate-of-Change) indicator displays the percent change between the current price and the price x-time periods ago.
	/// </summary>
	public class GIRocHedger : GIndicatorBase
	{
		private EMA baseEMA;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionROC;
				Name						= "GIRocHedger";
				IsSuspendedWhileInactive	= true;
				Period						= 8;

				AddLine(Brushes.DarkGray,	0,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorZeroLine);
				AddPlot(Brushes.Red,		NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameROC);
			} else if (State == State.DataLoaded)
			{
				baseEMA = EMA(Period);				
			}
		}

		protected override void OnBarUpdate()
		{
			double inputPeriod = Input[Math.Min(CurrentBar, Period)];
			
			if (inputPeriod <= 0)
				return;
			
			Value[0] = ((Input[0] - inputPeriod) / inputPeriod) * 100;
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIRocHedger[] cacheGIRocHedger;
		public GIRocHedger GIRocHedger(int period)
		{
			return GIRocHedger(Input, period);
		}

		public GIRocHedger GIRocHedger(ISeries<double> input, int period)
		{
			if (cacheGIRocHedger != null)
				for (int idx = 0; idx < cacheGIRocHedger.Length; idx++)
					if (cacheGIRocHedger[idx] != null && cacheGIRocHedger[idx].Period == period && cacheGIRocHedger[idx].EqualsInput(input))
						return cacheGIRocHedger[idx];
			return CacheIndicator<GIRocHedger>(new GIRocHedger(){ Period = period }, input, ref cacheGIRocHedger);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIRocHedger GIRocHedger(int period)
		{
			return indicator.GIRocHedger(Input, period);
		}

		public Indicators.GIRocHedger GIRocHedger(ISeries<double> input , int period)
		{
			return indicator.GIRocHedger(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIRocHedger GIRocHedger(int period)
		{
			return indicator.GIRocHedger(Input, period);
		}

		public Indicators.GIRocHedger GIRocHedger(ISeries<double> input , int period)
		{
			return indicator.GIRocHedger(input, period);
		}
	}
}

#endregion
