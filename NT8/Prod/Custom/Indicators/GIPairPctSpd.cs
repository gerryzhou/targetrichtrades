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
using NinjaTrader.NinjaScript.AddOns;
using NinjaTrader.NinjaScript.AddOns.PriceActions;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Pair pct spread
	/// Pct spread of the pair from last day: for intraday trading
	/// </summary>
	public class GIPairPctSpd : GIndicatorBase
	{
		private Series<double> PctChg1;
		private Series<double> PctChg2;
		private Series<double> RocChg;
		private PriorDayOHLC lastDayOHLC;
		private double[] PctChgArr = new double[]{-101, -101};
		private double PctChgSpdMin, PctChgSpdMax;
		
		private double PctChgSpdWideCount, PctChgSpdNarrowCount, PctChgSpdCount = 0;
		//int PctChgMaxBip=-1, PctChgMinBip=-1;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Calculate %chg spread for the underlining pair";
				Name										= "GIPairPctSpd";
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
				TM_OpenEndH									= 9;
				TM_OpenEndM									= 2;
				TM_ClosingH									= 10;
				TM_ClosingM									= 45;
				PctChgMaxBip								= -1;
				PctChgMinBip								= -1;
				BarsRequiredToPlot							= 128;
				MaximumBarsLookBack							= MaximumBarsLookBack.Infinite;
				AddPlot(Brushes.Red, "PlotPctSpd");
				AddPlot(Brushes.Orange, "PlotRocSpd");
				AddLine(Brushes.Blue, 0, "LineZero");
			}
			else if (State == State.Configure)
			{
				//AddDataSeries("NQ 06-20", Data.BarsPeriodType.Minute, 13, Data.MarketDataType.Last);
				//AddDataSeries("RTY 06-20", Data.BarsPeriodType.Minute, 13, Data.MarketDataType.Last);
				//AddDataSeries("NRGU", Data.BarsPeriodType.Minute, 1, Data.MarketDataType.Last);
				AddDataSeries(SecondSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
			}
			else if (State == State.DataLoaded)
			{
				PctChg1 = new Series<double>(this);
				PctChg2 = new Series<double>(this);
				RocChg = new Series<double>(this);			
				Print(String.Format("{0}: DataLoaded...BarsArray.Length={1}", 
					this.GetType().Name, BarsArray.Length));
			}
		}

		protected override void OnBarUpdate()
		{
			//Print(String.Format("{0}:[{1}] BarsRequiredToPlot={2},", CurrentBars[BarsInProgress], BarsInProgress, BarsRequiredToPlot));
			if(CurrentBars[BarsInProgress] < BarsRequiredToPlot)
				return;
//			if(IsCutoffTime(BarsInProgress, 10, 20)) {
//				Print(String.Format("{0}:[{1}] PutTradeByPctSpd EnEx Bip{2}: PctSpd={3}, MaxBip={4}, MinBip={5}",
//				CurrentBar, Times[BarsInProgress][0], BarsInProgress, PlotPctSpd[0], PctChgMaxBip, PctChgMinBip));
//			}
			PctChgArr[BarsInProgress] = GetPctChg(BarsInProgress);
			SetPctChgSpread();
			if(PrintOut > 1)
				PrintPctChgSpd();
		}
		
		private double GetPctChg(int bip) {
			double chg = -101;
			if(bip < 0)
				return chg;
			
			lastDayOHLC = PriorDayOHLC(BarsArray[BarsInProgress]);
			double cl = Closes[BarsInProgress][0];
			double lcl = PriorDayOHLC(BarsArray[BarsInProgress]).PriorClose[0];
			if(lcl > 0 && cl > 0) {
				chg = Math.Round(100*(cl-lcl)/lcl, 2);
				Print(string.Format("{0}: {1} Chg={2}, Time{0]={3}", 
					CurrentBar, Instruments[BarsInProgress].FullName, chg.ToString(), Times[BarsInProgress][0]));
			}
			else Print(string.Format("{0}:{1}, Close={2}, PriorClose={3}, Time={4}",
				CurrentBar, Instruments[BarsInProgress].FullName, cl, lcl, Times[BarsInProgress][0]));
			if(bip == 0)
				PctChg1[0] = chg;
			if(bip == 1)
				PctChg2[0] = chg;
			return chg;
		}
		
		private void SetPctChgSpread() {
			if(BarsInProgress == 0) return;
			else {
				if(PctChgArr[0] > -101 && PctChgArr[BarsInProgress] > -101) {
					PlotPctSpd[0] = Math.Round(CapRatio1*PctChgArr[0] + CapRatio2*PctChgArr[BarsInProgress], 2);
					PctChgSpdMax = Math.Max(PctChgSpdMax, PlotPctSpd[0]);
					PctChgSpdMin = Math.Min(PctChgSpdMin, PlotPctSpd[0]);
					PctChgSpdCount++;
					if(PlotPctSpd[0] <= PctChgSpdThresholdEn || PlotPctSpd[0] >= PctChgSpdThresholdEx){
						PctChgSpdWideCount++;
						FireThresholdEvent(PlotPctSpd[0]);
					}
					else PctChgSpdNarrowCount++;
				}
				Print(string.Format("{0}: PctChg0={1}, PctChg1={2}, PlotPctSpd={3}, Time={4}",
					CurrentBar, PctChgArr[0], PctChgArr[BarsInProgress], PlotPctSpd[0], Times[BarsInProgress][0]));
				PctChgArr = new double[]{-101, -101};
			}
		}
		
		private void FireThresholdEvent(double spd) {
			IndicatorSignal isig = new IndicatorSignal();
			//if(CurrentBar < 300)
				Print(String.Format("{0}:Close={1}, PctChgSpd={2}, PctChgSpdThresholdEn={3}, PctChgSpdThresholdEx={4}",
				CurrentBar, Close[0], spd, PctChgSpdThresholdEn, PctChgSpdThresholdEx));
			if(spd <= PctChgSpdThresholdEn) {
				isig.BreakoutDir = BreakoutDirection.Down;
				isig.SignalName = SignalName_BreakdownMV;
			} else if(spd >= PctChgSpdThresholdEx) {
				isig.BreakoutDir = BreakoutDirection.Up;
				isig.SignalName = SignalName_BreakoutMV;
			} else
				return;
			
			isig.BarNo = CurrentBar;
			isig.IndicatorSignalType = SignalType.SimplePriceAction;
			IndicatorEventArgs ievt = new IndicatorEventArgs(this.GetType().Name, " CheckMeanV: ");
			ievt.IndSignal = isig;
			//FireEvent(ievt);
			OnRaiseIndicatorEvent(ievt);
		}
		
		private void PrintPctChgSpd() {
			if(IsLastBarOnChart() > 0 && BarsInProgress == 0) {
				Print(string.Format("{0}: PctChgSpdMax={1}, PctChgSpdMin={2}, PctChgSpdThresholdEn={3}", CurrentBar, PctChgSpdMax, PctChgSpdMin, PctChgSpdThresholdEn));
				Print(string.Format("{0}: PctChgSpdWideCount={1}, {2:0.00}%, PctChgSpdNarrowCount={3}, {4:0.00}% PctChgSpdCount={5}",
					CurrentBar, PctChgSpdWideCount, 100*PctChgSpdWideCount/PctChgSpdCount, PctChgSpdNarrowCount, 100*PctChgSpdNarrowCount/PctChgSpdCount, PctChgSpdCount));
				for(int i=0; i < CurrentBar-BarsRequiredToPlot; i++) {
					Print(string.Format("{0:0.00}	{1:0.00}	{2:0.00}	{3}	{4:yyyyMMdd_HHmm}",
						PlotPctSpd[i], PctChg1[i], PctChg2[i], CurrentBar-i, Times[0][i]));
				}
			}
		}
		
		private void DrawTextValue() {			
			Draw.TextFixed(this, "NinjaScriptInfo", GetLongShortText(), TextPosition.TopLeft,
				Brushes.LimeGreen, new SimpleFont("Arial", 18), Brushes.Transparent, Brushes.Transparent, 0);
		}
		
		private string GetLongShortText() {
			String txt = "N/A";
			if(PlotPctSpd[0] != null && PctChgMaxBip >= 0 && PctChgMinBip >= 0) {
				if(PlotPctSpd[0] > 0) {
					txt = "L " + (PctChgMaxBip+1).ToString() + " : S " + (PctChgMinBip+1).ToString();
				} else {
					txt = "S " + (PctChgMaxBip+1).ToString() + " : L " + (PctChgMinBip+1).ToString();
				}
			}
			return txt;
		}
		
		public void CheckTradeEvent() {
			int en_H = TM_OpenEndH, en_M = TM_OpenEndM, ex_H = TM_ClosingH, ex_M = TM_ClosingM;		
			
			//entry at 9:02 am ct
			if(IsCutoffTime(BarsInProgress, en_H, en_M)) {
				Print(String.Format("{0}:CheckTradeEvent En Bip{1}: PctSpd={2}, MaxBip={3}, MinBip={4}",
				CurrentBars[BarsInProgress], BarsInProgress, PlotPctSpd[0], PctChgMaxBip, PctChgMinBip));
					
				IndicatorSignal isig = new IndicatorSignal();
				Direction dir = new Direction();
	//			Print(String.Format("{0}: [{1}] Non-CutoffTime {2}: MaxBip={3}, %Max={4}, MinBip={5}, %Min={6}, %Spd={7}", 
	//				CurrentBar, Time[0], GetLongShortText(),
	//				PctChgMaxBip, PctChgMax[0], PctChgMinBip, PctChgMin[0], PlotPctSpd[0]));

				if(PctChgMaxBip != PctChgMinBip) {
					if(PlotPctSpd[0] > 0) {
						dir.TrendDir = TrendDirection.Up;
						isig.SignalName = SignalName_EntryOnOpenLong;
					} else if(PlotPctSpd[0] < 0) {
						dir.TrendDir = TrendDirection.Down;
						isig.SignalName = SignalName_EntryOnOpenShort;
					}
				} else
					return;
				
				isig.BarNo = CurrentBars[BarsInProgress];
				isig.TrendDir = dir;
				isig.IndicatorSignalType = SignalType.SimplePriceAction;
				IndicatorEventArgs ievt = new IndicatorEventArgs(this.GetType().Name, String.Format(" [{0}] {1}", Times[BarsInProgress][0], GetLongShortText()));
				ievt.IndSignal = isig;
				//FireEvent(ievt);
				OnRaiseIndicatorEvent(ievt);
			}
			else if(IsCutoffTime(BarsInProgress, ex_H, ex_M)) {
				Print(String.Format("{0}:CheckTradeEvent Ex Bip{1}: PctSpd={2}, MaxBip={3}, MinBip={4}", 
				CurrentBars[BarsInProgress], BarsInProgress, PlotPctSpd[0], PctChgMaxBip, PctChgMinBip));
					
				IndicatorSignal isig = new IndicatorSignal();
				Direction dir = new Direction();
	//			Print(String.Format("{0}: [{1}] Non-CutoffTime {2}: MaxBip={3}, %Max={4}, MinBip={5}, %Min={6}, %Spd={7}", 
	//				CurrentBar, Time[0], GetLongShortText(),
	//				PctChgMaxBip, PctChgMax[0], PctChgMinBip, PctChgMin[0], PlotPctSpd[0]));

				dir.TrendDir = TrendDirection.UnKnown;
				isig.SignalName = SignalName_ExitForOpen;
//				if(PlotPctSpd[0] > 0) {
//					dir.TrendDir = TrendDirection.Up;
//					isig.SignalName = SignalName_ExitForOpen;
//				} else if(PlotPctSpd[0] < 0) {
//					dir.TrendDir = TrendDirection.Down;
//					isig.SignalName = SignalName_ExitForOpen;
//				} else
//					return;
				
				isig.BarNo = CurrentBars[BarsInProgress];
				isig.TrendDir = dir;
				isig.IndicatorSignalType = SignalType.SimplePriceAction;
				IndicatorEventArgs ievt = new IndicatorEventArgs(this.GetType().Name, " CheckTradeEvent Ex: ");
				ievt.IndSignal = isig;
				//FireEvent(ievt);
				OnRaiseIndicatorEvent(ievt);
			}
		}
		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RocPeriod", Description="Rate of chage period", Order=0, GroupName="Parameters")]
		public int RocPeriod
		{ 	get{
				return rocPeriod;
			}
			set{
				rocPeriod = value;
			}
		}

		[NinjaScriptProperty]		
		[Display(Name="SecondSymbol", Description="The second symbol of the pair", Order=1, GroupName="Parameters")]
		public string SecondSymbol
		{ 	get{
				return secondSymbol;
			}
			set{
				secondSymbol = value;
			}
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ChartMinutes", Description="Minutes for the chart", Order=2, GroupName="Parameters")]
		public int ChartMinutes
		{ 	get{
				return chartMinutes;
			}
			set{
				chartMinutes = value;
			}
		}
		
		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="CapRatio1", Description="CapRatio of first instrument", Order=3, GroupName="Parameters")]
		public double CapRatio1
		{ 	get{
				return capRatio1;
			}
			set{
				capRatio1 = value;
			}
		}

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="CapRatio2", Description="CapRatio of 2nd instrument", Order=4, GroupName="Parameters")]
		public double CapRatio2
		{ 	get{
				return capRatio2;
			}
			set{
				capRatio2 = value;
			}
		}

		[NinjaScriptProperty]
		[Range(double.MinValue, double.MaxValue)]
		[Display(Name="PctChgSpdThresholdEn", Description="PctChgSpd Threshold to entry", Order=5, GroupName="Parameters")]
		public double PctChgSpdThresholdEn
		{ 	get{
				return pctChgSpdThresholdEn;
			}
			set{
				pctChgSpdThresholdEn = value;
			}
		}
		
		[NinjaScriptProperty]
		[Range(double.MinValue, double.MaxValue)]
		[Display(Name="PctChgSpdThresholdEx", Description="PctChgSpd Threshold to exit", Order=6, GroupName="Parameters")]
		public double PctChgSpdThresholdEx
		{ 	get{
				return pctChgSpdThresholdEx;
			}
			set{
				pctChgSpdThresholdEx = value;
			}
		}

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

		//The BarsInProgress for PctChgMax and PctChgMin
		[Browsable(false)]
		[XmlIgnore]
		public int PctChgMaxBip
		{
			get;set;
		}
		[Browsable(false)]
		[XmlIgnore]
		public int PctChgMinBip
		{
			get;set;
		}
		#endregion
		
		#region Pre Defined parameters
		private int rocPeriod = 8;
		private double capRatio1 = 1;
		private double capRatio2 = 1;		
		private string secondSymbol = "SCO";
		private int chartMinutes = 4;
		private double pctChgSpdThresholdEn = 1;
		private double pctChgSpdThresholdEx = 1;
		#endregion
		
		#region Pre-defined signal name
		[Browsable(false), XmlIgnore]
		public string SignalName_BreakdownMV
		{
			get { return "BreakdownMeanV";}
		}

		[Browsable(false), XmlIgnore]
		public string SignalName_BreakoutMV
		{
			get { return "BreakoutMeanV";}
		}
		
		[Browsable(false), XmlIgnore]
		public string SignalName_EntryLongLeg1
		{
			get { return "EnLnLeg1";}
		}
		
		[Browsable(false), XmlIgnore]
		public string SignalName_EntryLongLeg2
		{
			get { return "EnLnLeg2";}
		}
		
		[Browsable(false), XmlIgnore]
		public string SignalName_EntryShortLeg1
		{
			get { return "EnStLeg1";}
		}
		
		[Browsable(false), XmlIgnore]
		public string SignalName_EntryShortLeg2
		{
			get { return "EnStLeg2";}
		}
		
		[Browsable(false), XmlIgnore]
		public string SignalName_EntryOnOpenLong
		{
			get { return "EntryOnOpenLong";}
		}

		[Browsable(false), XmlIgnore]
		public string SignalName_EntryOnOpenShort
		{
			get { return "EntryOnOpenShort";}
		}

		[Browsable(false), XmlIgnore]
		public string SignalName_ExitForOpen
		{
			get { return "ExitForOpen";}
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIPairPctSpd[] cacheGIPairPctSpd;
		public GIPairPctSpd GIPairPctSpd(int rocPeriod, string secondSymbol, int chartMinutes, double capRatio1, double capRatio2, double pctChgSpdThresholdEn, double pctChgSpdThresholdEx)
		{
			return GIPairPctSpd(Input, rocPeriod, secondSymbol, chartMinutes, capRatio1, capRatio2, pctChgSpdThresholdEn, pctChgSpdThresholdEx);
		}

		public GIPairPctSpd GIPairPctSpd(ISeries<double> input, int rocPeriod, string secondSymbol, int chartMinutes, double capRatio1, double capRatio2, double pctChgSpdThresholdEn, double pctChgSpdThresholdEx)
		{
			if (cacheGIPairPctSpd != null)
				for (int idx = 0; idx < cacheGIPairPctSpd.Length; idx++)
					if (cacheGIPairPctSpd[idx] != null && cacheGIPairPctSpd[idx].RocPeriod == rocPeriod && cacheGIPairPctSpd[idx].SecondSymbol == secondSymbol && cacheGIPairPctSpd[idx].ChartMinutes == chartMinutes && cacheGIPairPctSpd[idx].CapRatio1 == capRatio1 && cacheGIPairPctSpd[idx].CapRatio2 == capRatio2 && cacheGIPairPctSpd[idx].PctChgSpdThresholdEn == pctChgSpdThresholdEn && cacheGIPairPctSpd[idx].PctChgSpdThresholdEx == pctChgSpdThresholdEx && cacheGIPairPctSpd[idx].EqualsInput(input))
						return cacheGIPairPctSpd[idx];
			return CacheIndicator<GIPairPctSpd>(new GIPairPctSpd(){ RocPeriod = rocPeriod, SecondSymbol = secondSymbol, ChartMinutes = chartMinutes, CapRatio1 = capRatio1, CapRatio2 = capRatio2, PctChgSpdThresholdEn = pctChgSpdThresholdEn, PctChgSpdThresholdEx = pctChgSpdThresholdEx }, input, ref cacheGIPairPctSpd);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIPairPctSpd GIPairPctSpd(int rocPeriod, string secondSymbol, int chartMinutes, double capRatio1, double capRatio2, double pctChgSpdThresholdEn, double pctChgSpdThresholdEx)
		{
			return indicator.GIPairPctSpd(Input, rocPeriod, secondSymbol, chartMinutes, capRatio1, capRatio2, pctChgSpdThresholdEn, pctChgSpdThresholdEx);
		}

		public Indicators.GIPairPctSpd GIPairPctSpd(ISeries<double> input , int rocPeriod, string secondSymbol, int chartMinutes, double capRatio1, double capRatio2, double pctChgSpdThresholdEn, double pctChgSpdThresholdEx)
		{
			return indicator.GIPairPctSpd(input, rocPeriod, secondSymbol, chartMinutes, capRatio1, capRatio2, pctChgSpdThresholdEn, pctChgSpdThresholdEx);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIPairPctSpd GIPairPctSpd(int rocPeriod, string secondSymbol, int chartMinutes, double capRatio1, double capRatio2, double pctChgSpdThresholdEn, double pctChgSpdThresholdEx)
		{
			return indicator.GIPairPctSpd(Input, rocPeriod, secondSymbol, chartMinutes, capRatio1, capRatio2, pctChgSpdThresholdEn, pctChgSpdThresholdEx);
		}

		public Indicators.GIPairPctSpd GIPairPctSpd(ISeries<double> input , int rocPeriod, string secondSymbol, int chartMinutes, double capRatio1, double capRatio2, double pctChgSpdThresholdEn, double pctChgSpdThresholdEx)
		{
			return indicator.GIPairPctSpd(input, rocPeriod, secondSymbol, chartMinutes, capRatio1, capRatio2, pctChgSpdThresholdEn, pctChgSpdThresholdEx);
		}
	}
}

#endregion
