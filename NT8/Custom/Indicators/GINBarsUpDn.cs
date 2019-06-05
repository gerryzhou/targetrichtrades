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
	public class GINBarsUpDn : GIndicatorBase
	{		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"N bars up and down;";
				Name										= "GINBarsUpDn";
				IsOverlay									= true;
				DrawOnPricePanel							= true;
				BarCount					= 3;
				BarUp						= true;
				HigherHigh					= true;
				HigherLow					= true;
				BarDown						= true;
				LowerHigh					= true;
				LowerLow					= true;				
				IsSuspendedWhileInactive	= true;

				AddPlot(new Stroke(Brushes.DarkOrange, 2), PlotStyle.TriangleDown, "NBarsUp");
				AddPlot(new Stroke(Brushes.DarkCyan, 2), PlotStyle.TriangleUp, "NBarsDn");				
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
				bool gotUpBars = false;
				bool gotDnBars = false;

				for (int i = 0; i < BarCount + 1; i++)
				{
					if (i == BarCount)
					{
						gotUpBars = true;
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

				for (int i = 0; i < BarCount + 1; i++)
				{
					if (i == BarCount)
					{
						gotDnBars = true;
						break;
					}

					if (!(Close[i] < Close[i + 1]))
						break;

					if (BarDown && !(Close[i] < Open[i]))
						break;

					if (LowerHigh && !(High[i] < High[i + 1]))
						break;

					if (LowerLow && !(Low[i] < Low[i + 1]))
						break;
				}
				
				if(gotUpBars)
					Values[0][0] = High[0] + 2;

				if(gotDnBars)
					Values[1][0] = Low[0] - 2;
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
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarDown", GroupName = "NinjaScriptParameters", Order = 1)]
		public bool BarDown
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "LowerHigh", GroupName = "NinjaScriptParameters", Order = 2)]
		public bool LowerHigh
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "LowerLow", GroupName = "NinjaScriptParameters", Order = 3)]
		public bool LowerLow
		{ get; set; }		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GINBarsUpDn[] cacheGINBarsUpDn;
		public GINBarsUpDn GINBarsUpDn(int barCount, bool barUp, bool higherHigh, bool higherLow, bool barDown, bool lowerHigh, bool lowerLow)
		{
			return GINBarsUpDn(Input, barCount, barUp, higherHigh, higherLow, barDown, lowerHigh, lowerLow);
		}

		public GINBarsUpDn GINBarsUpDn(ISeries<double> input, int barCount, bool barUp, bool higherHigh, bool higherLow, bool barDown, bool lowerHigh, bool lowerLow)
		{
			if (cacheGINBarsUpDn != null)
				for (int idx = 0; idx < cacheGINBarsUpDn.Length; idx++)
					if (cacheGINBarsUpDn[idx] != null && cacheGINBarsUpDn[idx].BarCount == barCount && cacheGINBarsUpDn[idx].BarUp == barUp && cacheGINBarsUpDn[idx].HigherHigh == higherHigh && cacheGINBarsUpDn[idx].HigherLow == higherLow && cacheGINBarsUpDn[idx].BarDown == barDown && cacheGINBarsUpDn[idx].LowerHigh == lowerHigh && cacheGINBarsUpDn[idx].LowerLow == lowerLow && cacheGINBarsUpDn[idx].EqualsInput(input))
						return cacheGINBarsUpDn[idx];
			return CacheIndicator<GINBarsUpDn>(new GINBarsUpDn(){ BarCount = barCount, BarUp = barUp, HigherHigh = higherHigh, HigherLow = higherLow, BarDown = barDown, LowerHigh = lowerHigh, LowerLow = lowerLow }, input, ref cacheGINBarsUpDn);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GINBarsUpDn GINBarsUpDn(int barCount, bool barUp, bool higherHigh, bool higherLow, bool barDown, bool lowerHigh, bool lowerLow)
		{
			return indicator.GINBarsUpDn(Input, barCount, barUp, higherHigh, higherLow, barDown, lowerHigh, lowerLow);
		}

		public Indicators.GINBarsUpDn GINBarsUpDn(ISeries<double> input , int barCount, bool barUp, bool higherHigh, bool higherLow, bool barDown, bool lowerHigh, bool lowerLow)
		{
			return indicator.GINBarsUpDn(input, barCount, barUp, higherHigh, higherLow, barDown, lowerHigh, lowerLow);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GINBarsUpDn GINBarsUpDn(int barCount, bool barUp, bool higherHigh, bool higherLow, bool barDown, bool lowerHigh, bool lowerLow)
		{
			return indicator.GINBarsUpDn(Input, barCount, barUp, higherHigh, higherLow, barDown, lowerHigh, lowerLow);
		}

		public Indicators.GINBarsUpDn GINBarsUpDn(ISeries<double> input , int barCount, bool barUp, bool higherHigh, bool higherLow, bool barDown, bool lowerHigh, bool lowerLow)
		{
			return indicator.GINBarsUpDn(input, barCount, barUp, higherHigh, higherLow, barDown, lowerHigh, lowerLow);
		}
	}
}

#endregion
