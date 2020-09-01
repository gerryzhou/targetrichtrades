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
namespace NinjaTrader.NinjaScript.Indicators.TTM
{
	public class TTMWave : Indicator
	{
		// This is an approximation of the TTM C-Wave using a very slow macd histogram.
		// The A-Wave is also approximated, and is used in determining trend color, but is not plotted.
		// These EMA lengths have no particular signficance beyond this combination gives a reasonably close visual appoximation to the actual C-Wave (from TOS).
		//
            private int aWaveFastLen = 8;	//  A Wave Fast EMA Length
            private int aWaveSlowLen = 377; //  A Wave Slow EMA Length
            private int cWaveFastLen = 144; //  C Wave Fast EMA Length
            private int cWaveSlowLen = 377; //  C Wave Slow EMA Length
            private int signalLen = 9; 		//  Smoothing Length
		
			private MACD aWave_;
			private MACD cWave_;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"TTM C-Wave - this is an approximation of the TTM C Wave.";
				Name						= "TTMWave";
				
				Calculate					= Calculate.OnPriceChange;
				IsOverlay					= false;
				DisplayInDataBox			= true;
				DrawOnPricePanel			= false;
				DrawHorizontalGridLines		= false;
				DrawVerticalGridLines		= false;
				PaintPriceMarkers			= true;
				ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive	= true;

				TrendingBrush				= Brushes.Aqua;
				NoTrendBrush				= Brushes.Blue;

				AddPlot(new Stroke(NoTrendBrush, 5), PlotStyle.Bar, "CWave");
				AddLine(Brushes.Silver, 0, "ZeroLine");
			}
			else if (State == State.Configure)
			{
				aWave_ = MACD(aWaveFastLen, aWaveSlowLen, signalLen);
				cWave_ = MACD(cWaveFastLen, cWaveSlowLen, signalLen);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < cWaveSlowLen) return;
			
			Value[0] = cWave_[0];
			
			if ((cWave_[0] > 0) && (aWave_[0] > 0))
			{
				PlotBrushes[0][0] = TrendingBrush;
			}
			else if ((cWave_[0] < 0) && (aWave_[0] < 0))
			{
				PlotBrushes[0][0] = TrendingBrush;
			}
		}

		#region Properties
		[XmlIgnore]
		[Display(Name="TrendingBrush", Description="Trend Color", Order=1, GroupName="Colors")]
		public Brush TrendingBrush
		{ get; set; }

		[Browsable(false)]
		public string TrendingBrushSerializable
		{
			get { return Serialize.BrushToString(TrendingBrush); }
			set { TrendingBrush = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="NoTrendBrush", Description="Not Trending Color", Order=2, GroupName="Colors")]
		public Brush NoTrendBrush
		{ get; set; }

		[Browsable(false)]
		public string NoTrendBrushSerializable
		{
			get { return Serialize.BrushToString(NoTrendBrush); }
			set { NoTrendBrush = Serialize.StringToBrush(value); }
		}			

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CWave
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
		private TTM.TTMWave[] cacheTTMWave;
		public TTM.TTMWave TTMWave()
		{
			return TTMWave(Input);
		}

		public TTM.TTMWave TTMWave(ISeries<double> input)
		{
			if (cacheTTMWave != null)
				for (int idx = 0; idx < cacheTTMWave.Length; idx++)
					if (cacheTTMWave[idx] != null &&  cacheTTMWave[idx].EqualsInput(input))
						return cacheTTMWave[idx];
			return CacheIndicator<TTM.TTMWave>(new TTM.TTMWave(), input, ref cacheTTMWave);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TTM.TTMWave TTMWave()
		{
			return indicator.TTMWave(Input);
		}

		public Indicators.TTM.TTMWave TTMWave(ISeries<double> input )
		{
			return indicator.TTMWave(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TTM.TTMWave TTMWave()
		{
			return indicator.TTMWave(Input);
		}

		public Indicators.TTM.TTMWave TTMWave(ISeries<double> input )
		{
			return indicator.TTMWave(input);
		}
	}
}

#endregion
