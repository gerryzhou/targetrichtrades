//
// Copyright (C) 2015, NinjaTrader LLC <www.ninjatrader.com>
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
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Indicators.PriceActions;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class GIGetHighLowByTimeRange : GIndicatorBase
	{ 
		protected override void OnStateChange()
		{
			
			if (State == State.SetDefaults)
			{
				Description					= @"Determines the highest high and lowest low in a specified time range";
				Name						= "GIGetHighLowByPeriod";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= true;
				DisplayInDataBox			= true;
				DrawOnPricePanel			= true;
				DrawHorizontalGridLines		= true;
				DrawVerticalGridLines		= true;
				PaintPriceMarkers			= true;
				ScaleJustification 			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				
				StartHour					= 8;
				StartMinute					= 30;
				EndHour						= 9;
				EndMinute					= 30;
				AddPlot(Brushes.Green, "HighestHigh");
				AddPlot(Brushes.Red, "LowestLow");
			}
			else if (State == State.DataLoaded)
			{
				snrRanges = new List<SupportResistanceRange<SupportResistanceLine>>();
			}
		}
		
		private DateTime startDateTime;
		private DateTime endDateTime;
		private List<SupportResistanceRange<SupportResistanceLine>> snrRanges;
		private SupportResistanceRange<SupportResistanceLine> snrRange;
		
		protected override void OnBarUpdate()
		{
			// Check to make sure the end time is not earlier than the start time
			if (EndHour < StartHour)
			    return;

			//Do not calculate the high or low value when the ending time of the desired range is less than the current time of the bar being processed
			if (ToTime(EndHour,EndMinute,0) > ToTime(Time[0]))
			    return;   

			// If the stored date time date is not the same date as the bar time date, create a new DateTime object
			if (startDateTime.Date != Time[0].Date)
			{
			    startDateTime = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, StartHour, StartMinute, 0);
			    endDateTime = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, EndHour, EndMinute, 0);
				snrRanges.Add(snrRange);
				snrRange = null;
			}

			// Calculate the number of bars ago for the start and end bars of the specified time range
			int startBarsAgo = Bars.GetBar(startDateTime);
			int endBarsAgo = Bars.GetBar(endDateTime);

			/* Now that we have the start and end bars ago values for the specified time range we can calculate the highest high for this range
			 
			Note: We add 1 to the period range for MAX and MIN to compensate for the difference between "period" logic and "bars ago" logic.
			"Period" logic means exactly how many bars you want to check including the current bar.
			"Bars ago" logic means how many bars we are going to go backwards. The current bar is not counted because on that bar we aren't going back any bars so it would be "bars ago = 0" */
			double highestHigh = MAX(High, endBarsAgo - startBarsAgo + 1)[CurrentBar - endBarsAgo];

			// Now that we have the start and end bars ago values for the specified time range we can calculate the lowest low for this range
			double lowestLow = MIN(Low, endBarsAgo - startBarsAgo + 1)[CurrentBar - endBarsAgo];

			// Set the plot values
			HighestHigh[0] = highestHigh;
			LowestLow[0] = lowestLow;
			
			if(snrRange == null) {
				SupportResistanceLine rstLine = new SupportResistanceLine();
					rstLine.NewSupportResistanceLine(startBarsAgo, endBarsAgo, SupportResistanceType.Resistance, HighestHigh[0]) ;
				SupportResistanceLine sptLine = new SupportResistanceLine();
					sptLine.NewSupportResistanceLine(startBarsAgo, endBarsAgo, SupportResistanceType.Support, LowestLow[0]) ;
				snrRange = NewSupportResistanceRange(rstLine, sptLine);
			}
		}
		
		
		#region Properties
		private const int ODI_StartHour = 1;
		private const int ODI_StartMinute = 2;
		private const int ODI_EndHour = 3;
		private const int ODI_EndMinute = 4;
		
		[Range(0,23)]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="StartHour", Description = "Enter start hour, Military time format 0 - 23", Order=ODI_StartHour, GroupName=GPI_CUSTOM_PARAMS)]
		public int StartHour
		{ get; set; }

		[Range(0, 59)]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="StartMinute", Description = "Enter start minute(s) 0 - 59", Order=ODI_StartMinute, GroupName=GPI_CUSTOM_PARAMS)]
		public int StartMinute
		{ get; set; }

		[Range(0, 23)]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="EndHour", Description = "Enter end hour, Military time format 0 - 23", Order=ODI_EndHour, GroupName=GPI_CUSTOM_PARAMS)]
		public int EndHour
		{ get; set; }

		[Range(0, 59)]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="EndMinute",Description = "Enter end minute(s) 0 - 59", Order=ODI_EndMinute, GroupName=GPI_CUSTOM_PARAMS)]
		public int EndMinute
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HighestHigh
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LowestLow
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public List<SupportResistanceRange<SupportResistanceLine>> SnRRanges
		{
			get { return snrRanges; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public SupportResistanceRange<SupportResistanceLine> SnRRange
		{
			get { return snrRange; }
		}		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIGetHighLowByTimeRange[] cacheGIGetHighLowByTimeRange;
		public GIGetHighLowByTimeRange GIGetHighLowByTimeRange(int startHour, int startMinute, int endHour, int endMinute)
		{
			return GIGetHighLowByTimeRange(Input, startHour, startMinute, endHour, endMinute);
		}

		public GIGetHighLowByTimeRange GIGetHighLowByTimeRange(ISeries<double> input, int startHour, int startMinute, int endHour, int endMinute)
		{
			if (cacheGIGetHighLowByTimeRange != null)
				for (int idx = 0; idx < cacheGIGetHighLowByTimeRange.Length; idx++)
					if (cacheGIGetHighLowByTimeRange[idx] != null && cacheGIGetHighLowByTimeRange[idx].StartHour == startHour && cacheGIGetHighLowByTimeRange[idx].StartMinute == startMinute && cacheGIGetHighLowByTimeRange[idx].EndHour == endHour && cacheGIGetHighLowByTimeRange[idx].EndMinute == endMinute && cacheGIGetHighLowByTimeRange[idx].EqualsInput(input))
						return cacheGIGetHighLowByTimeRange[idx];
			return CacheIndicator<GIGetHighLowByTimeRange>(new GIGetHighLowByTimeRange(){ StartHour = startHour, StartMinute = startMinute, EndHour = endHour, EndMinute = endMinute }, input, ref cacheGIGetHighLowByTimeRange);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIGetHighLowByTimeRange GIGetHighLowByTimeRange(int startHour, int startMinute, int endHour, int endMinute)
		{
			return indicator.GIGetHighLowByTimeRange(Input, startHour, startMinute, endHour, endMinute);
		}

		public Indicators.GIGetHighLowByTimeRange GIGetHighLowByTimeRange(ISeries<double> input , int startHour, int startMinute, int endHour, int endMinute)
		{
			return indicator.GIGetHighLowByTimeRange(input, startHour, startMinute, endHour, endMinute);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIGetHighLowByTimeRange GIGetHighLowByTimeRange(int startHour, int startMinute, int endHour, int endMinute)
		{
			return indicator.GIGetHighLowByTimeRange(Input, startHour, startMinute, endHour, endMinute);
		}

		public Indicators.GIGetHighLowByTimeRange GIGetHighLowByTimeRange(ISeries<double> input , int startHour, int startMinute, int endHour, int endMinute)
		{
			return indicator.GIGetHighLowByTimeRange(input, startHour, startMinute, endHour, endMinute);
		}
	}
}

#endregion
