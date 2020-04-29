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
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class GISnR : GIndicatorBase
	{
		private Series<double> CustmSeries;

		private GIGetHighLowByTimeRange getHLByTimeRange;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Support and resistance indicator;";
				Name										= "GISnR";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				ShowOpenHL									= true;
				ShowLastdayClose							= true;
				ShowLastdayHL								= true;
				ShowTodayOpen								= true;
				TM_OpenStartH = 8;
				TM_OpenStartM = 30;
				TM_OpenEndH = 9;
				TM_OpenEndM = 30;				

				AddPlot(new Stroke(Brushes.Green, 1), PlotStyle.Dot, "OverNightSpt");
				AddPlot(new Stroke(Brushes.Red, 1), PlotStyle.Dot, "OverNightRst");
				AddPlot(new Stroke(Brushes.DarkGreen, 1), PlotStyle.Hash, "OpenSpt");
				AddPlot(new Stroke(Brushes.DarkRed, 1), PlotStyle.Hash, "OpenRst");
				AddPlot(new Stroke(Brushes.ForestGreen, DashStyleHelper.Dash, 1), PlotStyle.Square, "LastDaySpt");
				AddPlot(new Stroke(Brushes.LightCoral, 	DashStyleHelper.Dash, 1), PlotStyle.Square, "LastDayRst");
				AddPlot(new Stroke(Brushes.Gold, 		DashStyleHelper.Dash, 2), PlotStyle.Square, "LastDayClose");
				AddPlot(new Stroke(Brushes.Blue, 		DashStyleHelper.Dash, 2), PlotStyle.Square, "TodayOpen");
			}
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Day, 1);
				//AddDataSeries(BarsPeriodType.Minute, 5);
			}
			else if (State == State.DataLoaded)
			{
				CustmSeries = new Series<double>(this);
				getHLByTimeRange = GIGetHighLowByTimeRange(High, TM_OpenStartH, TM_OpenStartM, TM_OpenEndH, TM_OpenEndM);
				for (int i = 0; i < TradingHours.Sessions.Count; i++)
				{
				  Print(String.Format("Session {0}: {1} at {2} to {3} at {4}", i, TradingHours.Sessions[i].BeginDay, TradingHours.Sessions[i].BeginTime,
				    TradingHours.Sessions[i].EndDay, TradingHours.Sessions[i].EndTime));
				}
			}
		}

		protected override void OnBarUpdate()
		{
			if (Bars.IsFirstBarOfSession)
    			Print(string.Format("Bar number {0} was the first bar processed of the session at {1}.",
				CurrentBar, Time[0]));
			  
			GIFibonacci fib = GIFibonacci(1);

			Print(CurrentBar + ":BarsInProgress, Bars.GetDayBar(1)=" + BarsInProgress + "," + Bars.GetDayBar(1));
		    if (BarsInProgress != 0) return;
		
			if(Bars.GetDayBar(1) != null) {
//			    Print(string.Format( "{0}: day[1] Hi={1}, Lo={2}, Close={3}, day[0] high={4}, low={5}, open={6}",
//					CurrentBar, Bars.GetDayBar(1).High, Bars.GetDayBar(1).Low, Bars.GetDayBar(1).Close,
//					CurrentDayOHL().CurrentHigh[0], CurrentDayOHL().CurrentLow[0], CurrentDayOHL().CurrentOpen[0]));
				if(ShowLastdayHL) {
					LastDaySpt[0] = Bars.GetDayBar(1) == null? double.MinValue:Bars.GetDayBar(1).Low;//Values[4][0] = Bars.GetDayBar(1).Low;
					LastDayRst[0] = Bars.GetDayBar(1) == null? double.MaxValue:Bars.GetDayBar(1).High;//Values[5][0] = Bars.GetDayBar(1).High;
					CheckLastDayHLEvent();
				}
			
				if(ShowLastdayClose)
					LastDayClose[0]	= Bars.GetDayBar(1).Close;
				
				if(ShowTodayOpen) {
					if(IsStartTimeBar(GetTimeByHM(TM_OpenStartH, TM_OpenStartM, true), ToTime(Time[0]), ToTime(Time[1])))
						today_open = Open[0];
					if(today_open > 0)
						TodayOpen[0] = today_open; //CurrentDayOHL().CurrentOpen[0];
				}
				
				if(ShowOvernightHL) {
					if(GetOvernightHigh() > 0) {
						OverNightRst[0] = overnight_hi;
					}
					if(GetOvernightLow() > 0) {
						OverNightSpt[0] = overnight_lo;
					}
				}

				getHLByTimeRange.Update();
				//Print(CurrentBar + ":" + ShowOpenHL + "," + getHLByTimeRange.SnRRange + "," + getHLByTimeRange.SnRRanges);
				if(ShowOpenHL && getHLByTimeRange.SnRRange != null) {
					OpenRst[0] = getHLByTimeRange.SnRRange.Resistance.SnRPrice;
					OpenSpt[0] = getHLByTimeRange.SnRRange.Support.SnRPrice;
				}
			}			
			
				
			//double Rst = GIGetHighLowByTimeRange(High, 8, 30, 9, 30).HighestHigh[0];
			//double Spt = GIGetHighLowByTimeRange(Low, 8, 30, 9, 30).LowestLow[0];
			
		    // Go long if we have three up bars on all bars objects 
		    //if (Close[0] > Open[0] && Closes[1][0] > Opens[1][0] && Closes[2][0] > Opens[2][0])
//			if(CurrentBar > 10*BarsRequiredToPlot && Bars.IsFirstBarOfSession)
//		        Print(CurrentBar + ":[" + High[0] + "," + Low[0] + "]--[" + Highs[1][1] + "," + Lows[1][1] + "]");
		}
		
		public double GetPrevOpenHigh() {
			double hi = 0;
			return hi;
		}

		public double GetPrevOpenLow() {
			double lo = 0;
			return lo;
		}
		
		public double GetOvernightHigh() {
			if (GetTimeDiffByHM(TM_OpenStartH, TM_OpenStartM, Time[0].Hour, Time[0].Minute) >= 0 &&
				GetTimeDiffByHM(TM_OpenStartH, TM_OpenStartM, Time[1].Hour, Time[1].Minute) <= 0) {
				overnight_hi = CurrentDayOHL().CurrentHigh[0];
			}
				
//				if(Time[1].Hour < OpenStartH && Time[0].Hour >= OpenStartH) {
//					Price_Spt_LD = PriorDayOHLC().PriorLow[0]; //Bars.GetDayBar(1).Low;
//					Price_Rst_LD = PriorDayOHLC().PriorHigh[0]; //Bars.GetDayBar(1).High;
//				}
//				else {
//					Price_Spt_LD = CurrentDayOHL().CurrentLow[0];
//					Price_Rst_LD = CurrentDayOHL().CurrentHigh[0];
//				}

//				Price_Spt_TD = CurrentDayOHL().CurrentLow[0];
//				Price_Rst_TD = CurrentDayOHL().CurrentHigh[0];
				
			return overnight_hi;
		}

		public double GetOvernightLow() {
			if (GetTimeDiffByHM(TM_OpenStartH, TM_OpenStartM, Time[0].Hour, Time[0].Minute) >= 0 &&
				GetTimeDiffByHM(TM_OpenStartH, TM_OpenStartM, Time[1].Hour, Time[1].Minute) <= 0) {
				overnight_lo = CurrentDayOHL().CurrentLow[0];
			}
			return overnight_lo;
		}
		
		public void CheckLastDayHLEvent() {
			IndicatorSignal isig = new IndicatorSignal();
			//if(CurrentBar < 300)
				Print(String.Format("{0}:Close={1},LastDaySpt={2},LastDayRst={3}",
				CurrentBar, Close[0], LastDaySpt[0], LastDayRst[0]));
			if(Close[0] < LastDaySpt[0]) {
				isig.BreakoutDir = BreakoutDirection.Down;
				isig.SignalName = SignalName_BreakoutLastDLow;
			} else if(Close[0] > LastDayRst[0]) {
				isig.BreakoutDir = BreakoutDirection.Up;
				isig.SignalName = SignalName_BreakoutLastDHigh;
			} else
				return;
			
			isig.BarNo = CurrentBar;
			isig.IndicatorSignalType = SignalType.SimplePriceAction;
			IndicatorEventArgs ievt = new IndicatorEventArgs(this.GetType().Name, " CheckLastDayHLEvent: ");
			ievt.IndSignal = isig;
			//FireEvent(ievt);
			OnRaiseCustomEvent(ievt);
		}
		
		#region Properties
		private const int ODI_ShowOvernightHL = 1;
		private const int ODI_ShowOpenHL = 2;
		private const int ODI_ShowLastdayHL = 3;
		private const int ODI_ShowLastdayClose = 4;
		private const int ODI_ShowTodayOpen = 5;
		private const int ODI_TimeOpen = 6;
		private const int ODI_TimeClose = 7;
		
		[Description("Show Overnight HL")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ShowOvernightHL", Order=ODI_ShowOvernightHL, GroupName=GPI_CUSTOM_PARAMS)]
		public bool ShowOvernightHL
		{ get; set; }
		
		[Description("Show Open HL")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ShowOpenHL", Order=ODI_ShowOpenHL, GroupName=GPI_CUSTOM_PARAMS)]
		public bool ShowOpenHL
		{ get; set; }
		
		[Description("Show Lastday HL")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ShowLastdayHL", Order=ODI_ShowLastdayHL, GroupName=GPI_CUSTOM_PARAMS)]
		public bool ShowLastdayHL
		{ get; set; }

		[Description("Show Lastday Close")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ShowLastdayClose", Order=ODI_ShowLastdayClose, GroupName=GPI_CUSTOM_PARAMS)]
		public bool ShowLastdayClose
		{ get; set; }
		
		[Description("Show Today Open")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ShowTodayOpen", Order=ODI_ShowTodayOpen, GroupName=GPI_CUSTOM_PARAMS)]
		public bool ShowTodayOpen
		{ get; set; }

		[Description("Time for Open")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="TimeOpen", Order=ODI_TimeOpen, GroupName=GPI_CUSTOM_PARAMS)]
		public int TimeOpen
		{ get; set; }
		
		[Description("Time for Close")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="TimeClose", Order=ODI_TimeClose, GroupName=GPI_CUSTOM_PARAMS)]
		public int TimeClose
		{ get; set; }		
//		[Description("Hour of opening start")]
// 		[Range(0, 23), NinjaScriptProperty]		
//		[Display(Name="OpenStartH", Order=0, GroupName="Timming")]
//		public int OpenStartH
//		{ get; set; }

//		[Description("Minute of opening start")]
//		[Range(0, 59), NinjaScriptProperty]
//		[Display(Name="OpenStartM", Order=1, GroupName="Timming")]
//		public int OpenStartM
//		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> OverNightSpt
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> OverNightRst
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> OpenSpt
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> OpenRst
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LastDaySpt
		{
			get { return Values[4]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LastDayRst
		{
			get { return Values[5]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LastDayClose
		{
			get { return Values[6]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TodayOpen
		{
			get { return Values[7]; }
		}
		
		private double overnight_hi;
		private double overnight_lo;
		private double today_open;
		
		#endregion
		
		#region
		[Browsable(false), XmlIgnore]
		public string SignalName_BreakoutLastDLow
		{
			get { return "BreakoutLastDLow";}
		}

		[Browsable(false), XmlIgnore]
		public string SignalName_BreakoutLastDHigh
		{
			get { return "BreakoutLastDHigh";}
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GISnR[] cacheGISnR;
		public GISnR GISnR(bool showOvernightHL, bool showOpenHL, bool showLastdayHL, bool showLastdayClose, bool showTodayOpen, int timeOpen, int timeClose)
		{
			return GISnR(Input, showOvernightHL, showOpenHL, showLastdayHL, showLastdayClose, showTodayOpen, timeOpen, timeClose);
		}

		public GISnR GISnR(ISeries<double> input, bool showOvernightHL, bool showOpenHL, bool showLastdayHL, bool showLastdayClose, bool showTodayOpen, int timeOpen, int timeClose)
		{
			if (cacheGISnR != null)
				for (int idx = 0; idx < cacheGISnR.Length; idx++)
					if (cacheGISnR[idx] != null && cacheGISnR[idx].ShowOvernightHL == showOvernightHL && cacheGISnR[idx].ShowOpenHL == showOpenHL && cacheGISnR[idx].ShowLastdayHL == showLastdayHL && cacheGISnR[idx].ShowLastdayClose == showLastdayClose && cacheGISnR[idx].ShowTodayOpen == showTodayOpen && cacheGISnR[idx].TimeOpen == timeOpen && cacheGISnR[idx].TimeClose == timeClose && cacheGISnR[idx].EqualsInput(input))
						return cacheGISnR[idx];
			return CacheIndicator<GISnR>(new GISnR(){ ShowOvernightHL = showOvernightHL, ShowOpenHL = showOpenHL, ShowLastdayHL = showLastdayHL, ShowLastdayClose = showLastdayClose, ShowTodayOpen = showTodayOpen, TimeOpen = timeOpen, TimeClose = timeClose }, input, ref cacheGISnR);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GISnR GISnR(bool showOvernightHL, bool showOpenHL, bool showLastdayHL, bool showLastdayClose, bool showTodayOpen, int timeOpen, int timeClose)
		{
			return indicator.GISnR(Input, showOvernightHL, showOpenHL, showLastdayHL, showLastdayClose, showTodayOpen, timeOpen, timeClose);
		}

		public Indicators.GISnR GISnR(ISeries<double> input , bool showOvernightHL, bool showOpenHL, bool showLastdayHL, bool showLastdayClose, bool showTodayOpen, int timeOpen, int timeClose)
		{
			return indicator.GISnR(input, showOvernightHL, showOpenHL, showLastdayHL, showLastdayClose, showTodayOpen, timeOpen, timeClose);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GISnR GISnR(bool showOvernightHL, bool showOpenHL, bool showLastdayHL, bool showLastdayClose, bool showTodayOpen, int timeOpen, int timeClose)
		{
			return indicator.GISnR(Input, showOvernightHL, showOpenHL, showLastdayHL, showLastdayClose, showTodayOpen, timeOpen, timeClose);
		}

		public Indicators.GISnR GISnR(ISeries<double> input , bool showOvernightHL, bool showOpenHL, bool showLastdayHL, bool showLastdayClose, bool showTodayOpen, int timeOpen, int timeClose)
		{
			return indicator.GISnR(input, showOvernightHL, showOpenHL, showLastdayHL, showLastdayClose, showTodayOpen, timeOpen, timeClose);
		}
	}
}

#endregion
