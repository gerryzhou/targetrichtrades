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
	public class GIFibonacci : GIndicatorBase
	{
		public const double FibR1 = 0.382;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Indicator to apply fibonacci retracement and extensions.";
				Name										= "GIFibonacci";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				Param1					= 1;
				AddLine(Brushes.Orange, 1, "Fib1");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Param1", Description="Param1", Order=1, GroupName=GPI_CUSTOM_PARAMS)]
		public int Param1
		{ get; set; }
		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIFibonacci[] cacheGIFibonacci;
		public GIFibonacci GIFibonacci(int param1)
		{
			return GIFibonacci(Input, param1);
		}

		public GIFibonacci GIFibonacci(ISeries<double> input, int param1)
		{
			if (cacheGIFibonacci != null)
				for (int idx = 0; idx < cacheGIFibonacci.Length; idx++)
					if (cacheGIFibonacci[idx] != null && cacheGIFibonacci[idx].Param1 == param1 && cacheGIFibonacci[idx].EqualsInput(input))
						return cacheGIFibonacci[idx];
			return CacheIndicator<GIFibonacci>(new GIFibonacci(){ Param1 = param1 }, input, ref cacheGIFibonacci);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIFibonacci GIFibonacci(int param1)
		{
			return indicator.GIFibonacci(Input, param1);
		}

		public Indicators.GIFibonacci GIFibonacci(ISeries<double> input , int param1)
		{
			return indicator.GIFibonacci(input, param1);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIFibonacci GIFibonacci(int param1)
		{
			return indicator.GIFibonacci(Input, param1);
		}

		public Indicators.GIFibonacci GIFibonacci(ISeries<double> input , int param1)
		{
			return indicator.GIFibonacci(input, param1);
		}
	}
}

#endregion
