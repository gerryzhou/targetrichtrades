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
	/// Shows the OHLC of the previous week/month
	/// </summary>
	public class GISnRPriorWM : GIndicatorBase
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
				Description					= @"Shows the OHLC of the previous week/month";
				Name						= "GISnRPriorWeekMonth";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= true;
				DisplayInDataBox			= true;
				DrawOnPricePanel			= true;
				DrawHorizontalGridLines		= true;
				DrawVerticalGridLines		= true;
				PaintPriceMarkers			= true;
				ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive	= true;
				ShowWkClose					= true;
				ShowWkLow					= false;
				ShowWkHigh					= false;
				ShowWkOpen					= false;				
				ShowMoClose					= true;
				ShowMoLow					= false;
				ShowMoHigh					= false;
				ShowMoOpen					= false;
				
				AddPlot(new Stroke(Brushes.Orange, 	DashStyleHelper.Dash,	2), PlotStyle.Dot, "PriorWeekOpen");
				AddPlot(new Stroke(Brushes.Green, 	DashStyleHelper.Dash,	2), PlotStyle.Dot, "PriorWeekHigh");
				AddPlot(new Stroke(Brushes.Red,		DashStyleHelper.Dash,	2),	PlotStyle.Dot, "PriorWeekLow");
				AddPlot(new Stroke(Brushes.Blue,	DashStyleHelper.Dash,	2),	PlotStyle.Dot, "PriorWeekClose");
				
				AddPlot(new Stroke(Brushes.Orange,	DashStyleHelper.Dash,	3),	PlotStyle.Cross, "PriorMonthOpen");
				AddPlot(new Stroke(Brushes.Green,	DashStyleHelper.Dash,	3),	PlotStyle.Cross, "PriorMonthHigh");
				AddPlot(new Stroke(Brushes.Red,		DashStyleHelper.Dash,	3),	PlotStyle.Cross, "PriorMonthLow");
				AddPlot(new Stroke(Brushes.Blue, 	DashStyleHelper.Dash,	3),	PlotStyle.Cross, "PriorMonthClose");
			}
		}

		protected override void OnBarUpdate()
		{
			if (!Bars.BarsType.IsIntraday)
			{
				Draw.TextFixed(this, "error1", "PriorWMOHLC only works on intraday interval", TextPosition.BottomRight);
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
			
				if (ShowWkOpen)	PriorWeekOpen[0] 	= prWeeklyOpen;
				if (ShowWkHigh)	PriorWeekHigh[0] 	= prWeeklyHigh;
				if (ShowWkLow)	PriorWeekLow[0] 	= prWeeklyLow;
				if (ShowWkClose)	PriorWeekClose[0] 	= prWeeklyClose;
			}
			
			weeklyHigh 		= Math.Max(High[0], weeklyHigh);
			weeklyLow 		= Math.Min(Low[0], weeklyLow);
			weeklyClose 	= Close[0];
			
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
			
				if (ShowMoOpen)	PriorMonthOpen[0] = prMonthlyOpen;
				if (ShowMoHigh)	PriorMonthHigh[0] = prMonthlyHigh;
				if (ShowMoLow)	PriorMonthLow[0] = prMonthlyLow;
				if (ShowMoClose)	PriorMonthClose[0] = prMonthlyClose;
			}
			
			monthlyHigh		= Math.Max(High[0], monthlyHigh);
			monthlyLow		= Math.Min(Low[0], monthlyLow);
			monthlyClose	= Close[0];
		}
		
		#region Properties
		private const int ODI_ShowWkClose = 0;
		private const int ODI_ShowWkHigh = 1;
		private const int ODI_ShowWkLow = 2;
		private const int ODI_ShowWkOpen = 3;
		private const int ODI_ShowMoClose = 4;
		private const int ODI_ShowMoHigh = 5;
		private const int ODI_ShowMoLow = 6;
		private const int ODI_ShowMoOpen = 7;
		
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
		
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> PriorMonthOpen
        {
            get { return Values[4]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> PriorMonthHigh
        {
            get { return Values[5]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> PriorMonthLow
        {
            get { return Values[6]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> PriorMonthClose
        {
            get { return Values[7]; }
        }
		
		[Description("Show Weekly Close")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ShowWkClose", Order=ODI_ShowWkClose, GroupName=GPI_CUSTOM_PARAMS)]		
		public bool ShowWkClose
		{ get; set; }

		[Description("Show Weekly High")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ShowWkHigh", Order=ODI_ShowWkHigh, GroupName=GPI_CUSTOM_PARAMS)]		
		public bool ShowWkHigh
		{ get; set; }

		[Description("Show Weekly Low")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ShowWkLow", Order=ODI_ShowWkLow, GroupName=GPI_CUSTOM_PARAMS)]		
		public bool ShowWkLow
		{ get; set; }

		[Description("Show Weekly Open")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ShowWkOpen", Order=ODI_ShowWkOpen, GroupName=GPI_CUSTOM_PARAMS)]		
		public bool ShowWkOpen
		{ get; set; }
		
		[Description("Show Monthly Close")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ShowMoClose", Order=ODI_ShowMoClose, GroupName=GPI_CUSTOM_PARAMS)]		
		public bool ShowMoClose
		{ get; set; }

		[Description("Show Monthly High")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ShowMoHigh", Order=ODI_ShowMoHigh, GroupName=GPI_CUSTOM_PARAMS)]		
		public bool ShowMoHigh
		{ get; set; }

		[Description("Show Monthly Low")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ShowMoLow", Order=ODI_ShowMoLow, GroupName=GPI_CUSTOM_PARAMS)]		
		public bool ShowMoLow
		{ get; set; }

		[Description("Show Monthly Open")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ShowWkClose", Order=ODI_ShowMoOpen, GroupName=GPI_CUSTOM_PARAMS)]		
		public bool ShowMoOpen
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GISnRPriorWM[] cacheGISnRPriorWM;
		public GISnRPriorWM GISnRPriorWM(bool showWkClose, bool showWkHigh, bool showWkLow, bool showWkOpen, bool showMoClose, bool showMoHigh, bool showMoLow, bool showMoOpen)
		{
			return GISnRPriorWM(Input, showWkClose, showWkHigh, showWkLow, showWkOpen, showMoClose, showMoHigh, showMoLow, showMoOpen);
		}

		public GISnRPriorWM GISnRPriorWM(ISeries<double> input, bool showWkClose, bool showWkHigh, bool showWkLow, bool showWkOpen, bool showMoClose, bool showMoHigh, bool showMoLow, bool showMoOpen)
		{
			if (cacheGISnRPriorWM != null)
				for (int idx = 0; idx < cacheGISnRPriorWM.Length; idx++)
					if (cacheGISnRPriorWM[idx] != null && cacheGISnRPriorWM[idx].ShowWkClose == showWkClose && cacheGISnRPriorWM[idx].ShowWkHigh == showWkHigh && cacheGISnRPriorWM[idx].ShowWkLow == showWkLow && cacheGISnRPriorWM[idx].ShowWkOpen == showWkOpen && cacheGISnRPriorWM[idx].ShowMoClose == showMoClose && cacheGISnRPriorWM[idx].ShowMoHigh == showMoHigh && cacheGISnRPriorWM[idx].ShowMoLow == showMoLow && cacheGISnRPriorWM[idx].ShowMoOpen == showMoOpen && cacheGISnRPriorWM[idx].EqualsInput(input))
						return cacheGISnRPriorWM[idx];
			return CacheIndicator<GISnRPriorWM>(new GISnRPriorWM(){ ShowWkClose = showWkClose, ShowWkHigh = showWkHigh, ShowWkLow = showWkLow, ShowWkOpen = showWkOpen, ShowMoClose = showMoClose, ShowMoHigh = showMoHigh, ShowMoLow = showMoLow, ShowMoOpen = showMoOpen }, input, ref cacheGISnRPriorWM);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GISnRPriorWM GISnRPriorWM(bool showWkClose, bool showWkHigh, bool showWkLow, bool showWkOpen, bool showMoClose, bool showMoHigh, bool showMoLow, bool showMoOpen)
		{
			return indicator.GISnRPriorWM(Input, showWkClose, showWkHigh, showWkLow, showWkOpen, showMoClose, showMoHigh, showMoLow, showMoOpen);
		}

		public Indicators.GISnRPriorWM GISnRPriorWM(ISeries<double> input , bool showWkClose, bool showWkHigh, bool showWkLow, bool showWkOpen, bool showMoClose, bool showMoHigh, bool showMoLow, bool showMoOpen)
		{
			return indicator.GISnRPriorWM(input, showWkClose, showWkHigh, showWkLow, showWkOpen, showMoClose, showMoHigh, showMoLow, showMoOpen);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GISnRPriorWM GISnRPriorWM(bool showWkClose, bool showWkHigh, bool showWkLow, bool showWkOpen, bool showMoClose, bool showMoHigh, bool showMoLow, bool showMoOpen)
		{
			return indicator.GISnRPriorWM(Input, showWkClose, showWkHigh, showWkLow, showWkOpen, showMoClose, showMoHigh, showMoLow, showMoOpen);
		}

		public Indicators.GISnRPriorWM GISnRPriorWM(ISeries<double> input , bool showWkClose, bool showWkHigh, bool showWkLow, bool showWkOpen, bool showMoClose, bool showMoHigh, bool showMoLow, bool showMoOpen)
		{
			return indicator.GISnRPriorWM(input, showWkClose, showWkHigh, showWkLow, showWkOpen, showMoClose, showMoHigh, showMoLow, showMoOpen);
		}
	}
}

#endregion
