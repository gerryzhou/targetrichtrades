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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class GIKeyReversal : GIndicatorBase
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Key reversal up and down;";
				Name										= "GIKeyReversal";
				BarCount					= 3;
				BarUp						= true;
				HigherHigh					= true;
				HigherLow					= true;
				IsSuspendedWhileInactive	= true;

				AddPlot(new Stroke(Brushes.DarkCyan, 2), PlotStyle.TriangleDown, NinjaTrader.Custom.Resource.NinjaScriptIndicatorDiff);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < BarCount)
			{
				return;//Value[0] = 0;
			}
			else
			{
				bool gotBars = false;

				for (int i = 0; i < BarCount + 1; i++)
				{
					if (i == BarCount)
					{
						gotBars = true;
						break;
					}

					if (!(Close[i] > Close[i + 1]))
						break;

					if (BarUp && !(Close[i] > Open[i]))
						break;

					if (HigherHigh && !(High[i] > High[i + 1]))
						break;

					if (HigherLow && !(Low[i] > Low[i + 1]))
						break;
				}

				if(gotBars)
					Value[0] = High[0] + 2; //gotBars ? 1 : 0;
			}
		}


		#region Properties
		[Range(2, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarCount", GroupName = "NinjaScriptParameters", Order = 0)]
		public int BarCount
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarUp", GroupName = "NinjaScriptParameters", Order = 1)]
		public bool BarUp
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "HigherHigh", GroupName = "NinjaScriptParameters", Order = 2)]
		public bool HigherHigh
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "HigherLow", GroupName = "NinjaScriptParameters", Order = 3)]
		public bool HigherLow
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
		public GIKeyReversal GIKeyReversal(int barCount, bool barUp, bool higherHigh, bool higherLow)
		{
			return GIKeyReversal(Input, barCount, barUp, higherHigh, higherLow);
		}

		public GIKeyReversal GIKeyReversal(ISeries<double> input, int barCount, bool barUp, bool higherHigh, bool higherLow)
		{
			if (cacheGIKeyReversal != null)
				for (int idx = 0; idx < cacheGIKeyReversal.Length; idx++)
					if (cacheGIKeyReversal[idx] != null && cacheGIKeyReversal[idx].BarCount == barCount && cacheGIKeyReversal[idx].BarUp == barUp && cacheGIKeyReversal[idx].HigherHigh == higherHigh && cacheGIKeyReversal[idx].HigherLow == higherLow && cacheGIKeyReversal[idx].EqualsInput(input))
						return cacheGIKeyReversal[idx];
			return CacheIndicator<GIKeyReversal>(new GIKeyReversal(){ BarCount = barCount, BarUp = barUp, HigherHigh = higherHigh, HigherLow = higherLow }, input, ref cacheGIKeyReversal);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIKeyReversal GIKeyReversal(int barCount, bool barUp, bool higherHigh, bool higherLow)
		{
			return indicator.GIKeyReversal(Input, barCount, barUp, higherHigh, higherLow);
		}

		public Indicators.GIKeyReversal GIKeyReversal(ISeries<double> input , int barCount, bool barUp, bool higherHigh, bool higherLow)
		{
			return indicator.GIKeyReversal(input, barCount, barUp, higherHigh, higherLow);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIKeyReversal GIKeyReversal(int barCount, bool barUp, bool higherHigh, bool higherLow)
		{
			return indicator.GIKeyReversal(Input, barCount, barUp, higherHigh, higherLow);
		}

		public Indicators.GIKeyReversal GIKeyReversal(ISeries<double> input , int barCount, bool barUp, bool higherHigh, bool higherLow)
		{
			return indicator.GIKeyReversal(input, barCount, barUp, higherHigh, higherLow);
		}
	}
}

#endregion
