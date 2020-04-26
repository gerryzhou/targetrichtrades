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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class GIVWAP : GIndicatorBase
	{
		double	iCumVolume			= 0;
		double	iCumTypicalVolume	= 0;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"Volume Weighted Average Price";
				Name								= "GIVWAP";
				Calculate							= Calculate.OnBarClose;
				IsOverlay							= true;
				DisplayInDataBox					= true;
				DrawOnPricePanel					= true;
				DrawHorizontalGridLines				= true;
				DrawVerticalGridLines				= true;
				PaintPriceMarkers					= true;
				ScaleJustification					= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive			= true;
				AddPlot(Brushes.Black, "PlotVWAP");
			}
		}

		protected override void OnBarUpdate()
		{
			if (Bars.IsFirstBarOfSession)
			{
				iCumVolume = VOL()[0];
				iCumTypicalVolume = VOL()[0] * ((High[0] + Low[0] + Close[0]) / 3);
			}
			else
			{
				iCumVolume = iCumVolume + VOL()[0];
				iCumTypicalVolume = iCumTypicalVolume + (VOL()[0] * ((High[0] + Low[0] + Close[0]) / 3));
			}

			PlotVWAP[0] = (iCumTypicalVolume / iCumVolume);
		}

		#region Properties

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PlotVWAP
		{
			get { return Values[0]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIVWAP[] cacheGIVWAP;
		public GIVWAP GIVWAP()
		{
			return GIVWAP(Input);
		}

		public GIVWAP GIVWAP(ISeries<double> input)
		{
			if (cacheGIVWAP != null)
				for (int idx = 0; idx < cacheGIVWAP.Length; idx++)
					if (cacheGIVWAP[idx] != null &&  cacheGIVWAP[idx].EqualsInput(input))
						return cacheGIVWAP[idx];
			return CacheIndicator<GIVWAP>(new GIVWAP(), input, ref cacheGIVWAP);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIVWAP GIVWAP()
		{
			return indicator.GIVWAP(Input);
		}

		public Indicators.GIVWAP GIVWAP(ISeries<double> input )
		{
			return indicator.GIVWAP(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIVWAP GIVWAP()
		{
			return indicator.GIVWAP(Input);
		}

		public Indicators.GIVWAP GIVWAP(ISeries<double> input )
		{
			return indicator.GIVWAP(input);
		}
	}
}

#endregion
