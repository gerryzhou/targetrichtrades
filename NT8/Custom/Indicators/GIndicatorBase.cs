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
	public class GIndicatorBase : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Base indicator for GI;";
				Name										= "GIndicatorBase";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIndicatorBase[] cacheGIndicatorBase;
		public GIndicatorBase GIndicatorBase()
		{
			return GIndicatorBase(Input);
		}

		public GIndicatorBase GIndicatorBase(ISeries<double> input)
		{
			if (cacheGIndicatorBase != null)
				for (int idx = 0; idx < cacheGIndicatorBase.Length; idx++)
					if (cacheGIndicatorBase[idx] != null &&  cacheGIndicatorBase[idx].EqualsInput(input))
						return cacheGIndicatorBase[idx];
			return CacheIndicator<GIndicatorBase>(new GIndicatorBase(), input, ref cacheGIndicatorBase);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIndicatorBase GIndicatorBase()
		{
			return indicator.GIndicatorBase(Input);
		}

		public Indicators.GIndicatorBase GIndicatorBase(ISeries<double> input )
		{
			return indicator.GIndicatorBase(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIndicatorBase GIndicatorBase()
		{
			return indicator.GIndicatorBase(Input);
		}

		public Indicators.GIndicatorBase GIndicatorBase(ISeries<double> input )
		{
			return indicator.GIndicatorBase(input);
		}
	}
}

#endregion
