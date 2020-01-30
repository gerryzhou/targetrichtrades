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
	/// Shows the OHLC of the previous week
	/// </summary>
	public class GIPriorWeekOHLC : Indicator
	{
		private double weeklyOpen 		= 0;
		private double weeklyHigh 		= 0;
		private double weeklyLow 		= 0;
		private double weeklyClose 		= 0;
		
		private double prWeeklyOpen 	= 0;
		private double prWeeklyHigh 	= 0;
		private double prWeeklyLow 		= 0;
		private double prWeeklyClose 	= 0;
		
		DateTime newWeek = DateTime.MinValue;
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"Shows the OHLC of the previous week";
				Name						= "GIPriorWeekOHLC";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= true;
				DisplayInDataBox			= true;
				DrawOnPricePanel			= true;
				DrawHorizontalGridLines		= true;
				DrawVerticalGridLines		= true;
				PaintPriceMarkers			= true;
				ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive	= true;
				ShowClose					= true;
				ShowLow						= true;
				ShowHigh					= true;
				ShowOpen					= true;				
				
				AddPlot(new Stroke(Brushes.Orange, 		1), PlotStyle.Dot, "PriorWeekOpen");
				AddPlot(new Stroke(Brushes.Green, 		1), PlotStyle.Dot, "PriorWeekHigh");
				AddPlot(new Stroke(Brushes.Red,			1),	PlotStyle.Dot, "PriorWeekLow");
				AddPlot(new Stroke(Brushes.Firebrick,	1),	PlotStyle.Dot, "PriorWeekClose");
			}
		}

		protected override void OnBarUpdate()
		{
			if (!Bars.BarsType.IsIntraday)
			{
				Draw.TextFixed(this, "error1", "PriorWeekOHLC only works on intraday interval", TextPosition.BottomRight);
				return;
			}
			
			if (newWeek < Time[0])
			{
				prWeeklyOpen 	= weeklyOpen;
				prWeeklyHigh 	= weeklyHigh;
				prWeeklyLow 	= weeklyLow;
				prWeeklyClose 	= weeklyClose;
				
				weeklyOpen 		= Open[0];
				weeklyHigh 		= High[0];
				weeklyLow 		= Low[0];
				weeklyClose 	= Close[0];
					
				newWeek = Time[0].Date.AddDays(7 - (int)Time[0].DayOfWeek);
			}
			
			if (prWeeklyOpen != 0)
			{
			
				if (ShowOpen)	PriorWeekOpen[0] 	= prWeeklyOpen;
				if (ShowHigh)	PriorWeekHigh[0] 	= prWeeklyHigh;
				if (ShowLow)	PriorWeekLow[0] 	= prWeeklyLow;
				if (ShowClose)	PriorWeekClose[0] 	= prWeeklyClose;
			}
			
			weeklyHigh 		= Math.Max(High[0], weeklyHigh);
			weeklyLow 		= Math.Min(Low[0], weeklyLow);
			weeklyClose 	= Close[0];
		}
		
		#region Properties
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> PriorWeekOpen
        {
            get { return Values[0]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> PriorWeekHigh
        {
            get { return Values[1]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> PriorWeekLow
        {
            get { return Values[2]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> PriorWeekClose
        {
            get { return Values[3]; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show Weekly Close", GroupName = "NinjaScriptParameters", Order = 0)]
		public bool ShowClose
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "Show Weekly High", GroupName = "NinjaScriptParameters", Order = 1)]
		public bool ShowHigh
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "Show Weekly Low", GroupName = "NinjaScriptParameters", Order = 2)]
		public bool ShowLow
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "Show Weekly Open", GroupName = "NinjaScriptParameters", Order = 3)]
		public bool ShowOpen
		{ get; set; }		
        #endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIPriorWeekOHLC[] cacheGIPriorWeekOHLC;
		public GIPriorWeekOHLC GIPriorWeekOHLC()
		{
			return GIPriorWeekOHLC(Input);
		}

		public GIPriorWeekOHLC GIPriorWeekOHLC(ISeries<double> input)
		{
			if (cacheGIPriorWeekOHLC != null)
				for (int idx = 0; idx < cacheGIPriorWeekOHLC.Length; idx++)
					if (cacheGIPriorWeekOHLC[idx] != null &&  cacheGIPriorWeekOHLC[idx].EqualsInput(input))
						return cacheGIPriorWeekOHLC[idx];
			return CacheIndicator<GIPriorWeekOHLC>(new GIPriorWeekOHLC(), input, ref cacheGIPriorWeekOHLC);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIPriorWeekOHLC GIPriorWeekOHLC()
		{
			return indicator.GIPriorWeekOHLC(Input);
		}

		public Indicators.GIPriorWeekOHLC GIPriorWeekOHLC(ISeries<double> input )
		{
			return indicator.GIPriorWeekOHLC(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIPriorWeekOHLC GIPriorWeekOHLC()
		{
			return indicator.GIPriorWeekOHLC(Input);
		}

		public Indicators.GIPriorWeekOHLC GIPriorWeekOHLC(ISeries<double> input )
		{
			return indicator.GIPriorWeekOHLC(input);
		}
	}
}

#endregion
