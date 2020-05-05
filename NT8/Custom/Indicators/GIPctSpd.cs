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
	public class GIPctSpd : Indicator
	{
		private Series<double> PctSpd;
		private Series<double> RocSpd;
		private PriorDayOHLC lastDayOHLC;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Calculate %chg spread for the underlining data series, pick the most %chg and least %chg instrument to calculate the spread;";
				Name										= "GIPctSpd";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				RocPeriod					= 8;
				AddPlot(Brushes.Aqua, "PlotPctSpd");
				AddPlot(Brushes.Orange, "PlotRocSpd");
				AddLine(Brushes.Blue, 0, "LineZero");
			}
			else if (State == State.Configure)
			{
				AddDataSeries("NQ 06-20", Data.BarsPeriodType.Minute, 13, Data.MarketDataType.Last);
				AddDataSeries("RTY 06-20", Data.BarsPeriodType.Minute, 13, Data.MarketDataType.Last);
			}
			else if (State == State.DataLoaded)
			{				
				PctSpd = new Series<double>(this);
				RocSpd = new Series<double>(this);
				Print(String.Format("{0}: DataLoaded...BarsArray.Length={1}", this.GetType().Name, BarsArray.Length));
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar < BarsRequiredToPlot)
				return;
			lastDayOHLC = PriorDayOHLC(BarsArray[BarsArray.Length-1]);
			Print(String.Format("{0}: {1} PriorClose={2}", CurrentBar, this.GetType().Name, lastDayOHLC.PriorClose[0]));
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RocPeriod", Description="Rate of chage period", Order=1, GroupName="Parameters")]
		public int RocPeriod
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PlotPctSpd
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PlotRocSpd
		{
			get { return Values[1]; }
		}

		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIPctSpd[] cacheGIPctSpd;
		public GIPctSpd GIPctSpd(int rocPeriod)
		{
			return GIPctSpd(Input, rocPeriod);
		}

		public GIPctSpd GIPctSpd(ISeries<double> input, int rocPeriod)
		{
			if (cacheGIPctSpd != null)
				for (int idx = 0; idx < cacheGIPctSpd.Length; idx++)
					if (cacheGIPctSpd[idx] != null && cacheGIPctSpd[idx].RocPeriod == rocPeriod && cacheGIPctSpd[idx].EqualsInput(input))
						return cacheGIPctSpd[idx];
			return CacheIndicator<GIPctSpd>(new GIPctSpd(){ RocPeriod = rocPeriod }, input, ref cacheGIPctSpd);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIPctSpd GIPctSpd(int rocPeriod)
		{
			return indicator.GIPctSpd(Input, rocPeriod);
		}

		public Indicators.GIPctSpd GIPctSpd(ISeries<double> input , int rocPeriod)
		{
			return indicator.GIPctSpd(input, rocPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIPctSpd GIPctSpd(int rocPeriod)
		{
			return indicator.GIPctSpd(Input, rocPeriod);
		}

		public Indicators.GIPctSpd GIPctSpd(ISeries<double> input , int rocPeriod)
		{
			return indicator.GIPctSpd(input, rocPeriod);
		}
	}
}

#endregion
