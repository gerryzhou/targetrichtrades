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

//This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Down: Returns a value of 1 when the current close is less than the prior close after penetrating the highest high of the last n bars.
	/// Up: Returns a value of 1 when the current close is greater than the prior close after penetrating the lowest low of the last n bars.
	/// </summary>
	public class GIKeyReversal : GIndicatorBase
	{
		private MAX max;
		private MIN min;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionKeyReversalDown;
				Name						= "GIKeyReversal";
				IsSuspendedWhileInactive	= true;
				PeriodDown					= 5;
				IsOverlay					= true;

				AddPlot(new Stroke(Brushes.Blue, 2), PlotStyle.TriangleDown, "GIKeyReversalDown");

				//AddPlot(Brushes.DodgerBlue, NinjaTrader.Custom.Resource.KeyReversalPlot0);
			}
			else if (State == State.DataLoaded)
				max = MAX(High, PeriodDown);
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < PeriodDown + 1)
				return;

			//Down
			
			if(High[0] > max[1] && Close[0] < Close[1])
				Value[0] = High[0] + Range()[0]/2;
			if(Low[0] < min[1] && Close[0] > Close[1])
				Value[1] = Low[0] - Range()[0]/2;
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PeriodDown", GroupName = "NinjaScriptParameters", Order = 0)]
		public int PeriodDown
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIKeyReversal[] cacheGIKeyReversal;
		public GIKeyReversal GIKeyReversal(int periodDown)
		{
			return GIKeyReversal(Input, periodDown);
		}

		public GIKeyReversal GIKeyReversal(ISeries<double> input, int periodDown)
		{
			if (cacheGIKeyReversal != null)
				for (int idx = 0; idx < cacheGIKeyReversal.Length; idx++)
					if (cacheGIKeyReversal[idx] != null && cacheGIKeyReversal[idx].PeriodDown == periodDown && cacheGIKeyReversal[idx].EqualsInput(input))
						return cacheGIKeyReversal[idx];
			return CacheIndicator<GIKeyReversal>(new GIKeyReversal(){ PeriodDown = periodDown }, input, ref cacheGIKeyReversal);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIKeyReversal GIKeyReversal(int periodDown)
		{
			return indicator.GIKeyReversal(Input, periodDown);
		}

		public Indicators.GIKeyReversal GIKeyReversal(ISeries<double> input , int periodDown)
		{
			return indicator.GIKeyReversal(input, periodDown);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIKeyReversal GIKeyReversal(int periodDown)
		{
			return indicator.GIKeyReversal(Input, periodDown);
		}

		public Indicators.GIKeyReversal GIKeyReversal(ISeries<double> input , int periodDown)
		{
			return indicator.GIKeyReversal(input, periodDown);
		}
	}
}

#endregion
