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
	/// <summary>
	/// Shows the OHLC of the previous month
	/// </summary>
	public class GIPriorMonthOHLC : Indicator
	{
		private double monthlyOpen = 0;
		private double monthlyHigh = 0;
		private double monthlyLow = 0;
		private double monthlyClose = 0;
		
		private double prMonthlyOpen = 0;
		private double prMonthlyHigh = 0;
		private double prMonthlyLow = 0;
		private double prMonthlyClose = 0;
		
		DateTime newMonth = DateTime.MinValue;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"Shows the OHLC of the previous month";
				Name						= "GIPriorMonthOHLC";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= true;
				DisplayInDataBox			= true;
				DrawOnPricePanel			= true;
				DrawHorizontalGridLines		= true;
				DrawVerticalGridLines		= true;
				PaintPriceMarkers			= true;
				ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive	= true;
				
				AddPlot(new Stroke(Brushes.Orange,	DashStyleHelper.Dash,	2),	PlotStyle.Square, "PriorMonthOpen");
				AddPlot(new Stroke(Brushes.Green,		2),									PlotStyle.Square, "PriorMonthHigh");
				AddPlot(new Stroke(Brushes.Red,		2),									PlotStyle.Square, "PriorMonthLow");
				AddPlot(new Stroke(Brushes.Firebrick, DashStyleHelper.Dash,	2),	PlotStyle.Square, "PriorMonthClose");
			}
			else if (State == State.Configure)
			{

			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			if (!Bars.BarsType.IsIntraday)
			{
				Draw.TextFixed(this, "err", "PriorMonthOHLC only works on intraday interval", TextPosition.BottomRight);
				return;
			}
			
			if (newMonth < Time[0])
			{
				prMonthlyOpen = monthlyOpen;
				prMonthlyHigh = monthlyHigh;
				prMonthlyLow = monthlyLow;
				prMonthlyClose = monthlyClose;
				
				monthlyOpen = Open[0];
				monthlyHigh = High[0];
				monthlyLow = Low[0];
				monthlyClose = Close[0];
					
				newMonth = Time[0].Date.AddDays(DateTime.DaysInMonth(Time[0].Year, Time[0].Month) - (Time[0].Day - 1));
			}
			
			if (prMonthlyOpen != 0)
			{
			
				PriorMonthOpen[0] = prMonthlyOpen;
				PriorMonthHigh[0] = prMonthlyHigh;
				PriorMonthLow[0] = prMonthlyLow;
				PriorMonthClose[0] = prMonthlyClose;
			}
			
			monthlyHigh = Math.Max(High[0], monthlyHigh);
			monthlyLow = Math.Min(Low[0], monthlyLow);
			monthlyClose = Close[0];
		}
		
		#region Properties
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> PriorMonthOpen
        {
            get { return Values[0]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> PriorMonthHigh
        {
            get { return Values[1]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> PriorMonthLow
        {
            get { return Values[2]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> PriorMonthClose
        {
            get { return Values[3]; }
        }
        #endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIPriorMonthOHLC[] cacheGIPriorMonthOHLC;
		public GIPriorMonthOHLC GIPriorMonthOHLC()
		{
			return GIPriorMonthOHLC(Input);
		}

		public GIPriorMonthOHLC GIPriorMonthOHLC(ISeries<double> input)
		{
			if (cacheGIPriorMonthOHLC != null)
				for (int idx = 0; idx < cacheGIPriorMonthOHLC.Length; idx++)
					if (cacheGIPriorMonthOHLC[idx] != null &&  cacheGIPriorMonthOHLC[idx].EqualsInput(input))
						return cacheGIPriorMonthOHLC[idx];
			return CacheIndicator<GIPriorMonthOHLC>(new GIPriorMonthOHLC(), input, ref cacheGIPriorMonthOHLC);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIPriorMonthOHLC GIPriorMonthOHLC()
		{
			return indicator.GIPriorMonthOHLC(Input);
		}

		public Indicators.GIPriorMonthOHLC GIPriorMonthOHLC(ISeries<double> input )
		{
			return indicator.GIPriorMonthOHLC(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIPriorMonthOHLC GIPriorMonthOHLC()
		{
			return indicator.GIPriorMonthOHLC(Input);
		}

		public Indicators.GIPriorMonthOHLC GIPriorMonthOHLC(ISeries<double> input )
		{
			return indicator.GIPriorMonthOHLC(input);
		}
	}
}

#endregion
