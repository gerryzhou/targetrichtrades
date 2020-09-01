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
	/// </summary>
	public class GITTM : GIndicatorBase
	{
		/// <summary>
		/// BB params
		/// </summary>
		private SMA		sma;
		private StdDev	stdDev;
		
		/// <summary>
		/// KC params
		/// </summary>
		private Series<double>		diff;
		private	SMA					smaDiff;
		private	SMA					smaTypical;
		
		/// <summary>
		/// TTM params
		/// </summary>
		//private Series<int>			barsSinceSqueeze;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "TTM = Bollinger+Keltner Channels";
				Name						= "GITTM";
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;

				//BB
				NumStdDev					= 2;
				Period_BB					= 20;
				AddPlot(Brushes.Magenta, "UpperBB");
				//AddPlot(Brushes.LightGray, "Middle band");
				AddPlot(Brushes.Magenta, "LowerBB");

				//KC
				Period_KC					= 10;
				OffsetMultiplier			= 1.5;
				//AddPlot(Brushes.DarkGray,	"Midline");
				AddPlot(Brushes.Blue,		"UpperKC");
				AddPlot(Brushes.Blue,		"LowerKC");
			}
			else if (State == State.Configure)
			{
				//BB
				sma		= SMA(Period_BB);
				stdDev	= StdDev(Period_BB);
				
				//KC
				diff				= new Series<double>(this);
				smaDiff				= SMA(diff, Period_KC);
				smaTypical			= SMA(Typical, Period_KC);
				
				//TTM
				BarsSinceSqueeze	= new Series<int>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			//BB
			double sma0		= sma[0];
			double stdDev0	= stdDev[0];

			UpperBB[0]		= sma0 + NumStdDev * stdDev0;
			//Middle[0]		= sma0;
			LowerBB[0]		= sma0 - NumStdDev * stdDev0;
			
			//KC
			diff[0]			= High[0] - Low[0];

			double middle	= smaTypical[0];
			double offset	= smaDiff[0] * OffsetMultiplier;

			double upper	= middle + offset;
			double lower	= middle - offset;

			//Midline[0]		= middle;
			UpperKC[0]		= upper;
			LowerKC[0]		= lower;
			
			if(IsSqueezed()) {
				Print(String.Format("{0}: true BarsSinceSqueeze={1}", CurrentBar, BarsSinceSqueeze[0]));
			} else
				Print(String.Format("{0}: false BarsSinceSqueeze={1}", CurrentBar, BarsSinceSqueeze[0]));
		}
		
		public bool IsSqueezed() {
			bool isSz = false;
			if(UpperBB[0] < UpperKC[0] && LowerBB[0] > LowerKC[0]) {
				isSz = true;
				BarsSinceSqueeze[0] = 0;
			} else {
				BarsSinceSqueeze[0] = BarsSinceSqueeze[1] + 1;
			}
			return isSz;
		}

		#region Properties
		/// <summary>
		/// BB params
		/// </summary>
		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NumStdDev", GroupName = "NinjaScriptParameters", Order = 0)]
		public double NumStdDev
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period_BB", GroupName = "NinjaScriptParameters", Order = 1)]
		public int Period_BB
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> UpperBB
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
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
		/// KC params
		/// </summary>
		[Range(0.01, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "OffsetMultiplier", GroupName = "NinjaScriptParameters", Order = 2)]
		public double OffsetMultiplier
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period_KC", GroupName = "NinjaScriptParameters", Order = 3)]
		public int Period_KC
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> UpperKC
		{
			get { return Values[2]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> LowerKC
		{
			get { return Values[3]; }
		}

		/*
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Midline
		{
			get { return Values[0]; }
		}
		*/

		/// <summary>
		/// TTM params
		/// </summary>
		
		//Bar count since last squeeze ended
		[Browsable(false)]
		[XmlIgnore()]
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
		private GITTM[] cacheGITTM;
		public GITTM GITTM(double numStdDev, int period_BB, double offsetMultiplier, int period_KC)
		{
			return GITTM(Input, numStdDev, period_BB, offsetMultiplier, period_KC);
		}

		public GITTM GITTM(ISeries<double> input, double numStdDev, int period_BB, double offsetMultiplier, int period_KC)
		{
			if (cacheGITTM != null)
				for (int idx = 0; idx < cacheGITTM.Length; idx++)
					if (cacheGITTM[idx] != null && cacheGITTM[idx].NumStdDev == numStdDev && cacheGITTM[idx].Period_BB == period_BB && cacheGITTM[idx].OffsetMultiplier == offsetMultiplier && cacheGITTM[idx].Period_KC == period_KC && cacheGITTM[idx].EqualsInput(input))
						return cacheGITTM[idx];
			return CacheIndicator<GITTM>(new GITTM(){ NumStdDev = numStdDev, Period_BB = period_BB, OffsetMultiplier = offsetMultiplier, Period_KC = period_KC }, input, ref cacheGITTM);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GITTM GITTM(double numStdDev, int period_BB, double offsetMultiplier, int period_KC)
		{
			return indicator.GITTM(Input, numStdDev, period_BB, offsetMultiplier, period_KC);
		}

		public Indicators.GITTM GITTM(ISeries<double> input , double numStdDev, int period_BB, double offsetMultiplier, int period_KC)
		{
			return indicator.GITTM(input, numStdDev, period_BB, offsetMultiplier, period_KC);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GITTM GITTM(double numStdDev, int period_BB, double offsetMultiplier, int period_KC)
		{
			return indicator.GITTM(Input, numStdDev, period_BB, offsetMultiplier, period_KC);
		}

		public Indicators.GITTM GITTM(ISeries<double> input , double numStdDev, int period_BB, double offsetMultiplier, int period_KC)
		{
			return indicator.GITTM(input, numStdDev, period_BB, offsetMultiplier, period_KC);
		}
	}
}

#endregion
