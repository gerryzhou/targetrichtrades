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
	/// <summary>
	/// CapRatio: ES:RTY=1.7:1, NQ:RTY=2.1:1, NQ:ES=1.25:1
	/// </summary>
	public class GIPctSpd : GIndicatorBase
	{
		private Series<double> PctChgMax;
		private Series<double> PctChgMin;
		private Series<double> RocChg;
		private PriorDayOHLC lastDayOHLC;
		double PctSpdMax, PctSpdMin;
		//The BarsInProgress for PctChgMax and PctChgMin
		int PctChgMaxBip=-1, PctChgMinBip=-1;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Calculate %chg spread for the underlining data series, pick the most %chg and least %chg instrument to calculate the spread;";
				Name										= "GIPctSpd";
				Calculate									= Calculate.OnPriceChange;
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
				RocPeriod									= 8;
				TM_OpenStartH								= 8;
				TM_OpenStartM								= 0;
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
				PctChgMax = new Series<double>(this);
				PctChgMin = new Series<double>(this);
				RocChg = new Series<double>(this);
				Print(String.Format("{0}: DataLoaded...BarsArray.Length={1}", 
					this.GetType().Name, BarsArray.Length));
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar < BarsRequiredToPlot)
				return;
			//;
			if(BarsInProgress >= 0) {
				int cutoff = GetTimeByHM(TM_OpenStartH, TM_OpenStartM, false);
				int t0 = GetTimeByHM(Time[0].Hour, Time[0].Minute, false);
				int t1 = GetTimeByHM(Time[1].Hour, Time[1].Minute, false);
				if(t0 >= cutoff && t1 < cutoff) {
					Print("Cutoff time=" + cutoff);
				}
				lastDayOHLC = PriorDayOHLC(BarsArray[BarsInProgress]);
				double cl = Closes[BarsInProgress][0];
				double lcl = PriorDayOHLC(BarsArray[BarsInProgress]).PriorClose[0];
				if(lcl > 0) {
					double chg = Math.Round(100*(cl-lcl)/lcl, 2);
					Print("Chg=" + chg.ToString() + ", Time[0]=" + Time[0]);
					if(PctChgMax[0] == null || chg >= PctChgMax[0]) {
						PctChgMax[0] = chg;
						PctChgMaxBip = BarsInProgress;
					}
					if(PctChgMin[0] == null || chg <= PctChgMin[0]) {
						PctChgMin[0] = chg;
						PctChgMinBip = BarsInProgress;
					}
				Print(String.Format("{0}: {1} BarsInProgress={2}, Close[0]={3}, PriorClose={4}, chg={5}, PctChgMaxBip={6}, PctChgMinBip={7}", 
					CurrentBar, this.GetType().Name, BarsInProgress, cl, lcl,
					chg, PctChgMaxBip, PctChgMinBip));
				}
				
				if(PctChgMax[0] != null && PctChgMin[0] != null) {
				//if(BarsInProgress == BarsArray.Length-1)
				PlotPctSpd[0] = (PctChgMax[0] - PctChgMin[0]);
	//				PctChgMaxBip = -1;
	//				PctChgMinBip = -1;
				}
			}
			//else PlotPctSpd[1] = 1;
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
