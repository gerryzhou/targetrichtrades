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
		private Series<double> volwpr;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Indicator for volume;";
				Name										= "GIVOL";
				Calculate									= Calculate.OnEachTick;
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
				AddPlot(new Stroke(Brushes.Magenta, 2), PlotStyle.Dot, "VolBurst");
				AddPlot(new Stroke(Brushes.Yellow, 2), PlotStyle.Line, "VolWPR");
				AddLine(Brushes.DarkGray, 1, "ZeroLine");
			}
			else if (State == State.Configure)
			{
				volwpr = new Series<double>(this);
			}
			else if (State == State.DataLoaded)
			{
				//smaVolume	= new Series<double>(this);
				smaVol = SMA(Volume, VolPeriod);
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
			if(CurrentBar > VolPeriod) {
				//double v = SMA(Volume, 3)[0];
				if(smaVol[2] > 0) {
					if(smaVol[0] > 3*smaVol[1])
						VolBurst[0] = 90;//*(High[0] + 2);
					else if(smaVol[1] > 3*smaVol[0])
						VolBurst[0] = 10;//Low[0] - 2;
				//Vol = v;
				}
			}
			if(CurrentBar > VolWPRPeriod) {
				VolWPR[0] = GetVolWPR(VolWPRPeriod);
			}
		}

		#region Properties

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> VolBurst
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> VolWPR
		{
			get { return Values[1]; }
		}
		
		//[Browsable(false)]
		[XmlIgnore]
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "VolPeriod", GroupName = GP_CUSTOM_PARAMS, Order = OD_VolPeriod)]
		public int VolPeriod
		{ 
			get {return volPeriod;}
			set {volPeriod = value;}
		}

		//[Browsable(false)]
		[XmlIgnore]
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "VolWPRPeriod", GroupName = GP_CUSTOM_PARAMS, Order = OD_VolWPRPeriod)]
		public int VolWPRPeriod
		{ 
			get {return volwprPeriod;}
			set {volwprPeriod = value;}
		}

		private int volPeriod = 3;		
		private int volwprPeriod = 30;
		
		private const int OD_VolPeriod = 1;
		private const int OD_VolWPRPeriod = 2;
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIVOL[] cacheGIVOL;
		public GIVOL GIVOL(int volPeriod, int volWPRPeriod)
		{
			return GIVOL(Input, volPeriod, volWPRPeriod);
		}

		public GIVOL GIVOL(ISeries<double> input, int volPeriod, int volWPRPeriod)
		{
			if (cacheGIVOL != null)
				for (int idx = 0; idx < cacheGIVOL.Length; idx++)
					if (cacheGIVOL[idx] != null && cacheGIVOL[idx].VolPeriod == volPeriod && cacheGIVOL[idx].VolWPRPeriod == volWPRPeriod && cacheGIVOL[idx].EqualsInput(input))
						return cacheGIVOL[idx];
			return CacheIndicator<GIVOL>(new GIVOL(){ VolPeriod = volPeriod, VolWPRPeriod = volWPRPeriod }, input, ref cacheGIVOL);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIVOL GIVOL(int volPeriod, int volWPRPeriod)
		{
			return indicator.GIVOL(Input, volPeriod, volWPRPeriod);
		}

		public Indicators.GIVOL GIVOL(ISeries<double> input , int volPeriod, int volWPRPeriod)
		{
			return indicator.GIVOL(input, volPeriod, volWPRPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIVOL GIVOL(int volPeriod, int volWPRPeriod)
		{
			return indicator.GIVOL(Input, volPeriod, volWPRPeriod);
		}

		public Indicators.GIVOL GIVOL(ISeries<double> input , int volPeriod, int volWPRPeriod)
		{
			return indicator.GIVOL(input, volPeriod, volWPRPeriod);
		}
	}
}

#endregion
