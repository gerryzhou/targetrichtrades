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
	public class GIVOL : GIndicatorBase
	{
		private double curVol = 0;
		
		private SMA	smaVol;
			
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Indicator for volume;";
				Name										= "GIVOL";
				Calculate									= Calculate.OnEachTick;
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
				AddPlot(new Stroke(Brushes.Magenta, 2), PlotStyle.Dot, "VolBurst");
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{
				//smaVolume	= new Series<double>(this);
				smaVol = SMA(Volume, 3);
			}
			else if (State == State.Historical)
			{
				if (Calculate == Calculate.OnPriceChange)
				{
					Draw.TextFixed(this, "NinjaScriptInfo", string.Format(Custom.Resource.NinjaScriptOnPriceChangeError, Name), TextPosition.BottomRight);
					Log(string.Format(Custom.Resource.NinjaScriptOnPriceChangeError, Name), LogLevel.Error);
				}
			}
		}

		protected override void OnBarUpdate()
		{
			//Instrument.MasterInstrument.InstrumentType == InstrumentType.CryptoCurrency ? Core.Globals.ToCryptocurrencyVolume((long)Volume[0]) : Volume[0];
			if(CurrentBar > 3) {
				//double v = SMA(Volume, 3)[0];
				if(smaVol[2] > 0) {
					if(smaVol[0] > 3*smaVol[1])
						Value[0] = High[0] + 2;
					else if(smaVol[1] > 3*smaVol[0])
						Value[0] = Low[0] - 2;
				//Vol = v;
				}
			}
		}

		#region Properties

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> VolBurst
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
		private GIVOL[] cacheGIVOL;
		public GIVOL GIVOL()
		{
			return GIVOL(Input);
		}

		public GIVOL GIVOL(ISeries<double> input)
		{
			if (cacheGIVOL != null)
				for (int idx = 0; idx < cacheGIVOL.Length; idx++)
					if (cacheGIVOL[idx] != null &&  cacheGIVOL[idx].EqualsInput(input))
						return cacheGIVOL[idx];
			return CacheIndicator<GIVOL>(new GIVOL(), input, ref cacheGIVOL);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIVOL GIVOL()
		{
			return indicator.GIVOL(Input);
		}

		public Indicators.GIVOL GIVOL(ISeries<double> input )
		{
			return indicator.GIVOL(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIVOL GIVOL()
		{
			return indicator.GIVOL(Input);
		}

		public Indicators.GIVOL GIVOL(ISeries<double> input )
		{
			return indicator.GIVOL(input);
		}
	}
}

#endregion
