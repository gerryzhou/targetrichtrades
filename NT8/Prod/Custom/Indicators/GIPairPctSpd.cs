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
	/// </summary>
	public class GIPairPctSpd : GIndicatorBase
	{
		private Series<double> PctChgMax;
		private Series<double> PctChgMin;
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
				AddPlot(Brushes.Red, "PlotPctSpd");
				AddPlot(Brushes.Orange, "PlotRocSpd");
				AddLine(Brushes.Blue, 0, "LineZero");
			}
			else if (State == State.Configure)
			{
				//AddDataSeries("NQ 06-20", Data.BarsPeriodType.Minute, 13, Data.MarketDataType.Last);
				//AddDataSeries("RTY 06-20", Data.BarsPeriodType.Minute, 13, Data.MarketDataType.Last);
				//AddDataSeries("NRGU", Data.BarsPeriodType.Minute, 1, Data.MarketDataType.Last);
				AddDataSeries(SecondSymbol, Data.BarsPeriodType.Minute, 4, Data.MarketDataType.Last);
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
			//Print(String.Format("{0}:[{1}] BarsRequiredToPlot={2},", CurrentBars[BarsInProgress], BarsInProgress, BarsRequiredToPlot));
			if(CurrentBars[BarsInProgress] < BarsRequiredToPlot)
				return;
//			if(IsCutoffTime(BarsInProgress, 10, 20)) {
//				Print(String.Format("{0}:[{1}] PutTradeByPctSpd EnEx Bip{2}: PctSpd={3}, MaxBip={4}, MinBip={5}",
//				CurrentBar, Times[BarsInProgress][0], BarsInProgress, PlotPctSpd[0], PctChgMaxBip, PctChgMinBip));
//			}
			PctChgArr[BarsInProgress] = GetPctChg(BarsInProgress);
			SetPctChgSpread();
			PrintPctChgSpd();

			return;
			//Setup the max and min instruments for the day
			if(IsCutoffTime(BarsInProgress, TM_OpenStartH, TM_OpenStartM)) {
				double chg = Math.Abs(GetPctChg(BarsInProgress));
				if(BarsInProgress == 0) {
//					PctChgMax[0] = chg;
//					PctChgMaxBip = 0;
//					PctChgMin[0] = chg;
//					PctChgMinBip = 0;
//					Print(String.Format("{0}: {1} BarsInProgress={2}, chg={3}, PctChgMaxBip={4}, PctChgMinBip={5}", 
//						CurrentBar, this.GetType().Name, BarsInProgress,
//						chg, PctChgMaxBip, PctChgMinBip));
				}
				else { return;
					if(chg >= PctChgMax[0]) {
							PctChgMax[0] = chg;
							PctChgMaxBip = BarsInProgress;
						}
					if(chg <= PctChgMin[0]) {
						PctChgMin[0] = chg;
						PctChgMinBip = BarsInProgress;
					}
					Print(String.Format("{0}: {1} Bip={2}, chg={3}, %MaxBip={4}, %MinBip={5}", 
						CurrentBar, this.GetType().Name, BarsInProgress,
						chg, PctChgMaxBip, PctChgMinBip));
				}
				
				if (BarsInProgress == BarsArray.Length-1) {
					Print(String.Format("{0}: [{1}] MaxBip={2}, %Max={3}, MinBip={4}, %Min={5}", 
						CurrentBar, Time[0],
						PctChgMaxBip, PctChgMax[0], PctChgMinBip, PctChgMin[0]));
				}
			}//end of cutoff time
			else{ //Not the cutoff time
				return;
				if(BarsInProgress == PctChgMaxBip)
					PctChgMax[0] = GetPctChg(PctChgMaxBip);
				if(BarsInProgress == PctChgMinBip)
					PctChgMin[0] = GetPctChg(PctChgMinBip);
				
				if(BarsInProgress == BarsArray.Length-1 && PctChgMax[0] > -100 && PctChgMin[0] > -100) {
					PlotPctSpd[0] = (PctChgMax[0] - PctChgMin[0]);
					Print(String.Format("{0}: [{1}] Non-CutoffTime {2}: MaxBip={3}, %Max={4}, MinBip={5}, %Min={6}, %Spd={7}", 
						CurrentBar, Time[0], GetLongShortText(),
						PctChgMaxBip, PctChgMax[0], PctChgMinBip, PctChgMin[0], PlotPctSpd[0]));
					DrawTextValue();
					CheckTradeEvent();
				}
			}
			return;

//			if(BarsInProgress >= 0) {
//				if(IsCutoffTime(BarsInProgress, TM_OpenStartH, TM_OpenStartM)) {
//					double chg = GetPctChg(BarsInProgress);
//					if(chg >= -100) {
//						if(BarsInProgress == 0 || chg >= PctChgMax[0]) {
//							PctChgMax[0] = chg;
//							PctChgMaxBip = BarsInProgress;
//						}
//						if(BarsInProgress == 0 || chg <= PctChgMin[0]) {
//							PctChgMin[0] = chg;
//							PctChgMinBip = BarsInProgress;
//						}						
//						Print(String.Format("{0}: {1} BarsInProgress={2}, chg={3}, PctChgMaxBip={4}, PctChgMinBip={5}", 
//							CurrentBar, this.GetType().Name, BarsInProgress,
//							chg, PctChgMaxBip, PctChgMinBip));
//					} else {
//						throw new Exception(String.Format("{0}: Invalid PctChg for {1}", 
//						CurrentBar, Instruments[BarsInProgress]));
//						return;
//					}
//				}
//				if(PctChgMax[0] != null && PctChgMin[0] != null) {
//				//if(BarsInProgress == BarsArray.Length-1)
//				PlotPctSpd[0] = (PctChgMax[0] - PctChgMin[0]);
	//				PctChgMaxBip = -1;
	//				PctChgMinBip = -1;
//				}
//			}
			//else PlotPctSpd[1] = 1;
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
				Print(Instruments[BarsInProgress].FullName + " Chg=" + chg.ToString() + ", Time[0]=" + Times[BarsInProgress][0]);
			}
			else Print(string.Format("{0}:{1}, Close={2}, PriorClose={3}, Time={4}", CurrentBar, Instruments[BarsInProgress].FullName, cl, lcl, Times[BarsInProgress][0]));
			return chg;
		}
		
		private void SetPctChgSpread() {
			if(BarsInProgress == 0) return;
			else {
				if(PctChgArr[0] > -101 && PctChgArr[BarsInProgress] > -101) {
					PlotPctSpd[0] = CapRatio1*PctChgArr[0] + CapRatio2*PctChgArr[BarsInProgress];
					PctChgSpdMax = Math.Max(PctChgSpdMax, PlotPctSpd[0]);
					PctChgSpdMin = Math.Min(PctChgSpdMin, PlotPctSpd[0]);
					PctChgSpdCount++;
					if(Math.Abs(PlotPctSpd[0]) > PctChgSpdThreshold) PctChgSpdWideCount++;
					else PctChgSpdNarrowCount++;
				}
				Print(string.Format("{0}: PctChg0={1}, PctChg1={2}, PlotPctSpd={3}, Time={4}", CurrentBar, PctChgArr[0], PctChgArr[BarsInProgress], PlotPctSpd[0], Times[BarsInProgress][0]));
				PctChgArr = new double[]{-101, -101};
			}
		}
		
		private void PrintPctChgSpd() {
			if(IsLastBarOnChart() > 0) {
				Print(string.Format("{0}: PctChgSpdMax={1}, PctChgSpdMin={2}, PctChgSpdThreshold={3}", CurrentBar, PctChgSpdMax, PctChgSpdMin, PctChgSpdThreshold));
				Print(string.Format("{0}: PctChgSpdWideCount={1}, {2:0.00}%, PctChgSpdNarrowCount={3}, {4:0.00}% PctChgSpdCount={5}",
					CurrentBar, PctChgSpdWideCount, 100*PctChgSpdWideCount/PctChgSpdCount, PctChgSpdNarrowCount, 100*PctChgSpdNarrowCount/PctChgSpdCount, PctChgSpdCount));
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
		{ get; set; }

		[NinjaScriptProperty]		
		[Display(Name="SecondSymbol", Description="The second symbol of the pair", Order=1, GroupName="Parameters")]
		public string SecondSymbol
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="CapRatio1", Description="CapRatio of first instrument", Order=2, GroupName="Parameters")]
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
		[Display(Name="CapRatio2", Description="CapRatio of 2nd instrument", Order=3, GroupName="Parameters")]
		public double CapRatio2
		{ 	get{
				return capRatio2;
			}
			set{
				capRatio2 = value;
			}
		}

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="PctChgSpdThreshold", Description="PctChgSpd Threshold to entry", Order=4, GroupName="Parameters")]
		public double PctChgSpdThreshold
		{ 	get{
				return pctChgSpdThreshold;
			}
			set{
				pctChgSpdThreshold = value;
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
		
		#region pre-defined parameters
		private double capRatio1 = 1;
		private double capRatio2 = 1;
		private double pctChgSpdThreshold = 1;
		#endregion
		#region Pre-defined signal name
		[Browsable(false), XmlIgnore]
		public string SignalName_EntryOnOpenLong
		{
			get { return "EntryOnOpenLong";}
		}

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
		public GIPairPctSpd GIPairPctSpd(int rocPeriod, string secondSymbol, double capRatio1, double capRatio2, double pctChgSpdThreshold)
		{
			return GIPairPctSpd(Input, rocPeriod, secondSymbol, capRatio1, capRatio2, pctChgSpdThreshold);
		}

		public GIPairPctSpd GIPairPctSpd(ISeries<double> input, int rocPeriod, string secondSymbol, double capRatio1, double capRatio2, double pctChgSpdThreshold)
		{
			if (cacheGIPairPctSpd != null)
				for (int idx = 0; idx < cacheGIPairPctSpd.Length; idx++)
					if (cacheGIPairPctSpd[idx] != null && cacheGIPairPctSpd[idx].RocPeriod == rocPeriod && cacheGIPairPctSpd[idx].SecondSymbol == secondSymbol && cacheGIPairPctSpd[idx].CapRatio1 == capRatio1 && cacheGIPairPctSpd[idx].CapRatio2 == capRatio2 && cacheGIPairPctSpd[idx].PctChgSpdThreshold == pctChgSpdThreshold && cacheGIPairPctSpd[idx].EqualsInput(input))
						return cacheGIPairPctSpd[idx];
			return CacheIndicator<GIPairPctSpd>(new GIPairPctSpd(){ RocPeriod = rocPeriod, SecondSymbol = secondSymbol, CapRatio1 = capRatio1, CapRatio2 = capRatio2, PctChgSpdThreshold = pctChgSpdThreshold }, input, ref cacheGIPairPctSpd);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIPairPctSpd GIPairPctSpd(int rocPeriod, string secondSymbol, double capRatio1, double capRatio2, double pctChgSpdThreshold)
		{
			return indicator.GIPairPctSpd(Input, rocPeriod, secondSymbol, capRatio1, capRatio2, pctChgSpdThreshold);
		}

		public Indicators.GIPairPctSpd GIPairPctSpd(ISeries<double> input , int rocPeriod, string secondSymbol, double capRatio1, double capRatio2, double pctChgSpdThreshold)
		{
			return indicator.GIPairPctSpd(input, rocPeriod, secondSymbol, capRatio1, capRatio2, pctChgSpdThreshold);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIPairPctSpd GIPairPctSpd(int rocPeriod, string secondSymbol, double capRatio1, double capRatio2, double pctChgSpdThreshold)
		{
			return indicator.GIPairPctSpd(Input, rocPeriod, secondSymbol, capRatio1, capRatio2, pctChgSpdThreshold);
		}

		public Indicators.GIPairPctSpd GIPairPctSpd(ISeries<double> input , int rocPeriod, string secondSymbol, double capRatio1, double capRatio2, double pctChgSpdThreshold)
		{
			return indicator.GIPairPctSpd(input, rocPeriod, secondSymbol, capRatio1, capRatio2, pctChgSpdThreshold);
		}
	}
}

#endregion
