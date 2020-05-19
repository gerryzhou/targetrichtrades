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
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Indicators.PriceActions;
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
		//int PctChgMaxBip=-1, PctChgMinBip=-1;
		
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
			if(CurrentBars[BarsInProgress] < BarsRequiredToPlot)
				return;
//			if(IsCutoffTime(BarsInProgress, 10, 20)) {
//				Print(String.Format("{0}:[{1}] PutTradeByPctSpd EnEx Bip{2}: PctSpd={3}, MaxBip={4}, MinBip={5}",
//				CurrentBar, Times[BarsInProgress][0], BarsInProgress, PlotPctSpd[0], PctChgMaxBip, PctChgMinBip));
//			}
			
			//Setup the max and min instruments for the day
			if(IsCutoffTime(BarsInProgress, TM_OpenStartH, TM_OpenStartM)) {
				double chg = Math.Abs(GetPctChg(BarsInProgress));
				if(BarsInProgress == 0) {
					PctChgMax[0] = chg;
					PctChgMaxBip = 0;
					PctChgMin[0] = chg;
					PctChgMinBip = 0;
					Print(String.Format("{0}: {1} BarsInProgress={2}, chg={3}, PctChgMaxBip={4}, PctChgMinBip={5}", 
						CurrentBar, this.GetType().Name, BarsInProgress,
						chg, PctChgMaxBip, PctChgMinBip));
				}
				else {
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
			lastDayOHLC = PriorDayOHLC(BarsArray[BarsInProgress]);
			double cl = Closes[BarsInProgress][0];
			double lcl = PriorDayOHLC(BarsArray[BarsInProgress]).PriorClose[0];
			if(lcl > 0) {
				chg = Math.Round(100*(cl-lcl)/lcl, 2);
				Print(Instruments[BarsInProgress].FullName + " Chg=" + chg.ToString() + ", Time[0]=" + Time[0]);
			}
			else Print(Instruments[BarsInProgress].FullName + " PriorClose=0, Time[0]=" + Time[0]);
			return chg;
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
