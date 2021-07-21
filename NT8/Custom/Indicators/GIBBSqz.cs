// 
// Copyright (C) 2015, NinjaTrader LLC <www.ninjatrader.com>.
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
	/// Bollinger Bands are plotted at standard deviation levels above and below a moving average. 
	/// Since standard deviation is a measure of volatility, the bands are self-adjusting: 
	/// widening during volatile markets and contracting during calmer periods.
	/// JohnCarter default: period 20, BB 2, KC 1.5; 
	/// </summary>
	public class GIBBSqz : GIndicatorBase
	{
		/// <summary>
		/// BB params
		/// </summary>
//		private SMA		sma;
		private StdDev	stdDev;
		
		/// <summary>
		/// KC params
		/// </summary>
//		private Series<double>		bb_width;
//		private	SMA					smaDiff;
//		private	SMA					smaTypical;
		
		/// <summary>
		/// TTM params
		/// </summary>
		//private Series<int>			barsSinceSqueeze;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "Bollinger Band + CoVar as Squeeze";
				Name						= "GIBBSqz";
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;

				//BB
				NumStdDev					= 2;
				Period_BB					= 20;
				AddPlot(Brushes.Magenta, "UpperBB");
				//AddPlot(Brushes.LightGray, "Middle band");
				AddPlot(Brushes.Magenta, "LowerBB");

				//CoVar
				CoVarBk						= 10;
				CoVarSqz					= 4;
				//AddPlot(Brushes.DarkGray,	"Midline");
				AddPlot(Brushes.Blue,		"CoVarBB");
			}
			else if (State == State.Configure)
			{
				//BB
				SmaVal	= SMA(Period_BB);
				stdDev	= StdDev(Period_BB);
				
				//KC
//				bb_width			= new Series<double>(this);
//				smaDiff				= SMA(bb_width, Period_KC);
//				smaTypical			= SMA(Typical, Period_KC);
				
				//TTM
				BarsSinceSqueeze	= new Series<int>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			//BB
			double sma0		= SmaVal[0];
			//double stdDev0	= stdDev[0];
			double stdDev_width = NumStdDev * stdDev[0];

			UpperBB[0]		= sma0 + stdDev_width;
			//Middle[0]		= sma0;
			LowerBB[0]		= sma0 - stdDev_width;
			
			//BB width
			double bb_width	= 2*stdDev_width;
			
			CoVar[0]		= 100*bb_width/sma0;

//			double middle	= smaTypical[0];
//			double kc_offset	= smaDiff[0] * KCOffset;

//			double kc_upper	= middle + kc_offset;
//			double kc_lower	= middle - kc_offset;

			//Midline[0]		= middle;
//			UpperKC[0]		= kc_upper;
//			LowerKC[0]		= kc_lower;
			
			if(IsSqueezed()) {
				Print(String.Format("{0}: true BarsSinceSqueeze={1}", CurrentBar, BarsSinceSqueeze[0]));
			} else
				Print(String.Format("{0}: false BarsSinceSqueeze={1}", CurrentBar, BarsSinceSqueeze[0]));
		}
		
		public bool IsSqueezed() {
			bool isSz = false;
			if(CoVar[0] > CoVarSqz) {
				isSz = false;
				BarsSinceSqueeze[0] = 0;
			} else {
				isSz = true;
				BarsSinceSqueeze[0] = BarsSinceSqueeze[1] + 1;
			}
			return isSz;
		}

		#region Properties
		/// <summary>
		/// BB params
		/// </summary>
		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NumStdDev", GroupName = "NinjaScriptParameters", Order = 0)]
		public double NumStdDev
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period_BB", GroupName = "NinjaScriptParameters", Order = 1)]
		public int Period_BB
		{ get; set; }

		[Browsable(false), XmlIgnore()]
		public Series<double> UpperBB
		{
			get { return Values[0]; }
		}
		
		[Browsable(false), XmlIgnore()]
		public Series<double> LowerBB
		{
			get { return Values[1]; }
		}
		/*
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Middle
		{
			get { return Values[1]; }
		} */
		
		/// <summary>
		/// CoVar breakout or squeeze
		/// </summary>
		[Range(0.01, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "CoVarBk", GroupName = "NinjaScriptParameters", Order = 2)]
		public double CoVarBk
		{ get; set; }

		[Range(0.01, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "CoVarSqz", GroupName = "NinjaScriptParameters", Order = 3)]
		public double CoVarSqz
		{ get; set; }
		
		[Browsable(false), XmlIgnore()]
		public Series<double> CoVar
		{
			get { return Values[2]; }
		}
		
//		[Browsable(false), XmlIgnore()]
//		public Series<double> LowerKC
//		{
//			get { return Values[3]; }
//		}

		/*
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Midline
		{
			get { return Values[0]; }
		}
		*/

		[Browsable(false), XmlIgnore()]
		public SMA SmaVal
		{
			get;set;
		}
		
		//Bar count since last squeeze ended
		[Browsable(false), XmlIgnore()]
		public Series<int> BarsSinceSqueeze
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIBBSqz[] cacheGIBBSqz;
		public GIBBSqz GIBBSqz(double numStdDev, int period_BB, double coVarBk, double coVarSqz)
		{
			return GIBBSqz(Input, numStdDev, period_BB, coVarBk, coVarSqz);
		}

		public GIBBSqz GIBBSqz(ISeries<double> input, double numStdDev, int period_BB, double coVarBk, double coVarSqz)
		{
			if (cacheGIBBSqz != null)
				for (int idx = 0; idx < cacheGIBBSqz.Length; idx++)
					if (cacheGIBBSqz[idx] != null && cacheGIBBSqz[idx].NumStdDev == numStdDev && cacheGIBBSqz[idx].Period_BB == period_BB && cacheGIBBSqz[idx].CoVarBk == coVarBk && cacheGIBBSqz[idx].CoVarSqz == coVarSqz && cacheGIBBSqz[idx].EqualsInput(input))
						return cacheGIBBSqz[idx];
			return CacheIndicator<GIBBSqz>(new GIBBSqz(){ NumStdDev = numStdDev, Period_BB = period_BB, CoVarBk = coVarBk, CoVarSqz = coVarSqz }, input, ref cacheGIBBSqz);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIBBSqz GIBBSqz(double numStdDev, int period_BB, double coVarBk, double coVarSqz)
		{
			return indicator.GIBBSqz(Input, numStdDev, period_BB, coVarBk, coVarSqz);
		}

		public Indicators.GIBBSqz GIBBSqz(ISeries<double> input , double numStdDev, int period_BB, double coVarBk, double coVarSqz)
		{
			return indicator.GIBBSqz(input, numStdDev, period_BB, coVarBk, coVarSqz);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIBBSqz GIBBSqz(double numStdDev, int period_BB, double coVarBk, double coVarSqz)
		{
			return indicator.GIBBSqz(Input, numStdDev, period_BB, coVarBk, coVarSqz);
		}

		public Indicators.GIBBSqz GIBBSqz(ISeries<double> input , double numStdDev, int period_BB, double coVarBk, double coVarSqz)
		{
			return indicator.GIBBSqz(input, numStdDev, period_BB, coVarBk, coVarSqz);
		}
	}
}

#endregion
