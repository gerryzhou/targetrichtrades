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
	public class GISnR : GIndicatorBase
	{
		private Series<double> CustmSeries;
		private double overnight_hi;
		private double overnight_lo;

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
				//OpenStartH					= 1;
				AddPlot(new Stroke(Brushes.Green, 1), PlotStyle.Dot, "OverNightSpt");
				AddPlot(new Stroke(Brushes.Red, 1), PlotStyle.Dot, "OverNightRst");
				AddPlot(new Stroke(Brushes.DarkGreen, 1), PlotStyle.Hash, "OpenSpt");
				AddPlot(new Stroke(Brushes.DarkRed, 1), PlotStyle.Hash, "OpenRst");
				AddPlot(new Stroke(Brushes.ForestGreen, 1), PlotStyle.Dot, "LastDaySpt");
				AddPlot(new Stroke(Brushes.LightCoral, 1), PlotStyle.Dot, "LastDayRst");
			}
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Day, 1);
			}
			else if (State == State.DataLoaded)
			{				
				CustmSeries = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			//Print(CurrentBar + ":BarsInProgress=" + BarsInProgress);
		    if (BarsInProgress == 1 || BarsInProgress == 2)
   			return;
 
			if(Bars.GetDayBar(1) != null) {
			    Print(CurrentBar + ": day[1],[0]'s high,low are: [" + Bars.GetDayBar(1).High + "," + Bars.GetDayBar(1).Low + "]"
				+ ",[" + CurrentDayOHL().CurrentHigh[0] + "," + CurrentDayOHL().CurrentLow[0] + "]");
				LastDaySpt[0] = Bars.GetDayBar(1).Low;//Values[4][0] = Bars.GetDayBar(1).Low;
				LastDayRst[0] = Bars.GetDayBar(1).High;//Values[5][0] = Bars.GetDayBar(1).High;
				if(GetOvernightHigh() > 0) {
					OverNightRst[0] = overnight_hi;
				}
				if(GetOvernightLow() > 0) {
					OverNightSpt[0] = overnight_lo;
				}				
			}
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
		
		#region Properties
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
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GISnR[] cacheGISnR;
		public GISnR GISnR()
		{
			return GISnR(Input);
		}

		public GISnR GISnR(ISeries<double> input)
		{
			if (cacheGISnR != null)
				for (int idx = 0; idx < cacheGISnR.Length; idx++)
					if (cacheGISnR[idx] != null &&  cacheGISnR[idx].EqualsInput(input))
						return cacheGISnR[idx];
			return CacheIndicator<GISnR>(new GISnR(), input, ref cacheGISnR);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GISnR GISnR()
		{
			return indicator.GISnR(Input);
		}

		public Indicators.GISnR GISnR(ISeries<double> input )
		{
			return indicator.GISnR(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GISnR GISnR()
		{
			return indicator.GISnR(Input);
		}

		public Indicators.GISnR GISnR(ISeries<double> input )
		{
			return indicator.GISnR(input);
		}
	}
}

#endregion
