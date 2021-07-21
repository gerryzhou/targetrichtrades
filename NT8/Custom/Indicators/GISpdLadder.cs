// 
// Copyright (C) 2015, NinjaTrader LLC <www.ninjatrader.com>.
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
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.AddOns;
using NinjaTrader.NinjaScript.AddOns.PriceActions;
using NinjaTrader.NinjaScript.AddOns.MarketCtx;
using NinjaTrader.NinjaScript.Strategies;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Bollinger Bands are plotted at standard deviation levels above and below a moving average. 
	/// Since standard deviation is a measure of volatility, the bands are self-adjusting: 
	/// widening during volatile markets and contracting during calmer periods.
	/// </summary>
	public class GISpdLadder : GIndicatorProxy
	{
//		private SMA		sma;
//		private StdDev	stdDev;
//		private GIHLnBars HLnBarsNear;
//		private GIHLnBars HLnBarsMiddle;
//		private GIHLnBars HLnBarsFar;
		//private GIATRRatio giAtrRatio;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= "Pair spread ladder";
				Name										= "GISpdLadder";
				IsOverlay									= false;
				IsSuspendedWhileInactive					= true;
				Calculate									= Calculate.OnPriceChange;
				SPKLineThresholdLow							= 3;
				SPKLineThresholdMid							= 50;
				SPKLineThresholdHigh						= 96;
				NumStdDevDownMin							= 0.5;
				PeriodNear									= 10;
				PeriodMiddle								= 30;
				PeriodFar									= 90;
				PairPriceRatio								= 1;
				TM_OpenStartH								= 8;
				TM_OpenStartM								= 0;
				TM_OpenEndH									= 8;
				TM_OpenEndM									= 34;
				TM_ClosingH									= 10;
				TM_ClosingM									= 45;

				BarsRequiredToPlot							= 8;
				MaximumBarsLookBack							= MaximumBarsLookBack.Infinite;

				AddPlot(new Stroke(Brushes.Blue), PlotStyle.Dot, "Spread");
				AddPlot(new Stroke(Brushes.Gold), PlotStyle.Dot, "SparkLineNear");
				AddPlot(new Stroke(Brushes.Green), PlotStyle.Dot, "SparkLineMid");
				//AddPlot(Brushes.Orange, "Middle band");
				AddPlot(new Stroke(Brushes.Magenta), PlotStyle.Dot, "SparkLineFar");
				AddPlot(Brushes.Red, "SparkLineAllTime");
			}
			else if (State == State.Configure)
			{
				if(ChartMinutes > 0)
					AddDataSeries(SecondSymbol, BarsPeriodType.Minute, ChartMinutes, MarketDataType.Last);
				else 
					AddDataSeries(SecondSymbol, BarsPeriodType.Day, 1, MarketDataType.Last);
				
				//Spread[0] = Closes[0][0] - Closes[1][0];
//				sma		= SMA(Spread, MAPeriod);
//				stdDev	= StdDev(Spread, MAPeriod);
//				HLnBarsNear	= GIHLnBars(Spread, 10);
//				HLnBarsMiddle	= GIHLnBars(Spread, 30);
//				HLnBarsFar	= GIHLnBars(Spread, 90);
				//PairATRRatio = GIATRRatio(ATRPeriod, SecondSymbol, ChartMinutes);
				this.CtxPairSpreadDaily = new CtxPairSpdDaily();
			}
			else if (State == State.DataLoaded)
			{
				HiNear = new Series<double>(this);
				LowNear = new Series<double>(this);
				HiFar = new Series<double>(this);
				LowFar = new Series<double>(this);
				HiMiddle = new Series<double>(this);
				LowMiddle = new Series<double>(this);
				HiAllTime = double.MinValue;
				LowAllTime = double.MaxValue;
				//				UpperMin = new Series<double>(this);
//				LowerMin = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			int barsRequired = Math.Max(BarsRequiredToPlot, PeriodNear);
			//PairATRRatio.Update();
			if(CurrentBars[0] > barsRequired
				&& CurrentBars[1] > barsRequired && BarsInProgress > 0) {
//				Print(string.Format("CurrentBars[BarsInProgress]={0}, BarsInProgress={1}, Closes[0][0]={2}, Closes[1][0]=3",
//					CurrentBars[BarsInProgress], BarsInProgress, Closes[0][0]));//, Closes[1][0]));
				Spread[0]		= Math.Round(PairPriceRatio*Closes[0][0] - Closes[1][0], 4);
//				double sma0		= sma[0];
//				double stdDev0	= stdDev[0];
				GetSpreadHiLoBack();
//			if(CurrentBars[0] > PeriodMid
//				&& CurrentBars[1] > PeriodMid && BarsInProgress > 0) {
//					HLnBarsNear.Update();
//					HLnBarsMiddle.Update();
//					HLnBarsFar.Update();
//				}
//				SPKLineNear[0]		= 100*(Spread[0]-HLnBarsNear.LowestN[0])/HLnBarsNear.HiLoSpread[0];
//				SPKLineMiddle[0]	= 100*(Spread[0]-HLnBarsMiddle.LowestN[0])/HLnBarsMiddle.HiLoSpread[0];
//				SPKLineFar[0]		= 100*(Spread[0]-HLnBarsFar.LowestN[0])/HLnBarsFar.HiLoSpread[0];
//				UpperMin[0]		= sma0 + NumStdDevUpMin * stdDev0;
//				Middle[0]		= sma0;
//				Lower[0]		= sma0 - NumStdDevDown * stdDev0;
//				LowerMin[0]		= sma0 - NumStdDevDownMin * stdDev0;				
//				CheckContext();
				CheckTradeEvent();
			}
		}
		
		public void CheckTradeEvent() {
			int en_H = TM_OpenEndH, en_M = TM_OpenEndM, ex_H = TM_ClosingH, ex_M = TM_ClosingM;		
//			Print(String.Format("{0}:CheckTradeEvent Bip{1}: en_H={2}, en_M={3}, ex_H={4}, ex_M={5}",
//				CurrentBars[BarsInProgress], BarsInProgress, en_H, en_M, ex_H, ex_M));

//			if(IsCutoffTime(BarsInProgress, ex_H, ex_M)) {
				if(PrintOut > 1)
					Print(String.Format("{0}:CheckTradeEvent Ex Bip{1}: Spread={2}, HiNear={3}, LowNear={4}, HiMiddle={5}",
					CurrentBars[BarsInProgress], BarsInProgress, Spread[0], HiNear[0], LowNear[0], HiMiddle[0]));
					
//				IndicatorSignal isig = new IndicatorSignal();
//				Direction dir = new Direction();

//				dir.TrendDir = TrendDirection.UnKnown;
//				isig.SignalName = SignalName_ExitForOpen;			
//				isig.BarNo = CurrentBars[BarsInProgress];
//				isig.TrendDir = dir;
//				isig.IndicatorSignalType = SignalType.SimplePriceAction;
//				IndicatorEventArgs ievt = new IndicatorEventArgs(this.GetType().Name, " CheckTradeEvent Ex: ");
//				ievt.IndSignal = isig;				
//				OnRaiseIndicatorEvent(ievt);
//			}
//			else 			//entry at 9:02 am ct, if(IsCutoffTime(BarsInProgress, en_H, en_M)) 
//			{
//				Print(String.Format("{0}:CheckTradeEvent En Bip{1}: Spread={2}, Upper={3}, Lower={4}, Middle={5}",
//				CurrentBars[BarsInProgress], BarsInProgress, Spread[0], Upper[0], Lower[0], Middle[0]));
					
				IndicatorSignal isig = new IndicatorSignal();
				Direction dir = new Direction();

				if(IsSpreadBreakdown()) {
					dir.TrendDir = TrendDirection.Up;
					isig.SignalName = SignalName_BelowStdDev;
				}
//				else if(Spread[0] <= LowerMin[0]) {
//					dir.TrendDir = TrendDirection.Up;
//					isig.SignalName = SignalName_BelowStdDevMin;
//				}
				else if(IsSpreadBreakout()) {
					dir.TrendDir = TrendDirection.Down;
					isig.SignalName = SignalName_AboveStdDev;
				}
				else {
					dir.TrendDir = TrendDirection.UnKnown;
					isig.SignalName = "UnKnown";
				}
//				else if(Spread[0] >= UpperMin[0]) {
//					dir.TrendDir = TrendDirection.Down;
//					isig.SignalName = SignalName_AboveStdDevMin;
//				}
				
				isig.BarNo = CurrentBars[BarsInProgress];
				isig.TrendDir = dir;
				isig.IndicatorSignalType = SignalType.SimplePriceAction;
				IndicatorEventArgs ievt = new IndicatorEventArgs(this.GetType().Name, String.Format(" [{0}] {1}", Times[BarsInProgress][0], GetLongShortText()));
				ievt.IndSignal = isig;
				//FireEvent(ievt);
				OnRaiseIndicatorEvent(ievt);
//			}
		}
		
		public bool IsSpreadFlat() {
			bool isFlat = false;
			if(IsSpreadMiddleUp() || IsSpreadMiddleDown())
				isFlat = true;
			return isFlat;
		}
		
		public bool IsSpreadMiddleUp() {
			bool isMu = false;
//			if(Spread[0] < UpperMin[0] && Spread[0] >= Middle[0])
//				isMu = true;
			return isMu;
		}

		public bool IsSpreadMiddleDown() {
			bool isMd = false;
//			if(Spread[0] > LowerMin[0] && Spread[0] <= Middle[0])
//				isMd = true;
			return isMd;
		}
		
		public bool IsSpreadUpBand() {
			bool isUb = false;
//			if(Spread[0] >= UpperMin[0] && Spread[0] < Upper[0])
//				isUb = true;
			return isUb;
		}

		public bool IsSpreadLowBand() {
			bool isLb = false;
//			if(Spread[0] <= LowerMin[0] && Spread[0] > Lower[0])
//				isLb = true;
			return isLb;
		}
				
		public bool IsSpreadBreakout() {
			bool isBk = false;
			if(SPKLineAllTime[0] >= SPKLineThresholdHigh)
				isBk = true;
			return isBk;
		}
		
		public bool IsSpreadBreakdown() {
			bool isBd = false;
			if(SPKLineAllTime[0] <= SPKLineThresholdLow)
				isBd = true;
			return isBd;
		}
		
		public PositionInBand GetSpreadPosInBand() {
			PositionInBand pib = PositionInBand.UnKnown;
			if(IsSpreadBreakout())
				pib = PositionInBand.BreakoutUp;
			else if(IsSpreadBreakdown())
				pib = PositionInBand.BreakDown;
			else if(IsSpreadUpBand())
				pib = PositionInBand.Upper;
			else if(IsSpreadLowBand())
				pib = PositionInBand.Lower;
			else if(IsSpreadMiddleUp())
				pib = PositionInBand.MiddleUp;
			else if(IsSpreadMiddleDown())
				pib = PositionInBand.MiddleDn;
			return pib;
		}
		
		public TrendDirection GetSpreadTrend() {
			TrendDirection trd = TrendDirection.UnKnown;
			if(Spread[0] > Spread[1])
				trd = TrendDirection.Up;
			else if (Spread[0] < Spread[1])
				trd = TrendDirection.Down;
			return trd;
		}
		
		public void SetSpreadHiLo() {
//			if(Times[0][0].Day != Times[0][1].Day)
//				Print(string.Format("===={0}: New Day Times[0][0]={1}====", CurrentBars[BarsInProgress], Times[0][0]));
//			if(Times[0][0].Day != Times[0][1].Day || PairSpdHigh < Spread[0]) {
//				PairSpdHigh = Spread[0];
//				PairSpdHighTime = GetTimeByDateTime(Times[0][0], false);
//			}
			
//			if(Times[0][0].Day != Times[0][1].Day || PairSpdLow > Spread[0]) {
//				PairSpdLow = Spread[0];
//				PairSpdLowTime = GetTimeByDateTime(Times[0][0], false);
//			}
//			Print(string.Format("{0}: Spread[0]={1}, Spread[1]={2}, PairSpdHigh={3}, PairSpdLow={4}, PairSpdHighTime={5}, PairSpdLowTime={6}",
//				CurrentBars[BarsInProgress], Spread[0], Spread[1], PairSpdHigh, PairSpdLow, PairSpdHighTime, PairSpdLowTime));
		}
		
		public void GetSpreadHiLoBack() {
			try {
					HiNear[0] = GetHighestPrice(PeriodNear, Spread, true);
					LowNear[0] = GetLowestPrice(PeriodNear, Spread, true);
					HiMiddle[0] = GetHighestPrice(PeriodMiddle, Spread, true);
					LowMiddle[0] = GetLowestPrice(PeriodMiddle, Spread, true);				
					HiFar[0] = GetHighestPrice(PeriodFar, Spread, true);
					LowFar[0] = GetLowestPrice(PeriodFar, Spread, true);
					HiAllTime = Math.Max(HiAllTime, HiFar[0]);
					LowAllTime = Math.Min(LowAllTime, LowFar[0]);
					
					double spdNear = HiNear[0] - LowNear[0];
					double spdMid = HiMiddle[0] - LowMiddle[0];
					double spdFar = HiFar[0] - LowFar[0];
					double spdAllTime = HiAllTime - LowAllTime;
					double avgPrice = (Closes[0][0] + Closes[1][0])/2;
					double pctSpdAvgP = Math.Round(100*spdAllTime/avgPrice, 2);
					
					if(spdNear > 0) {
						SPKLineNear[0] = 100*(Spread[0] - LowNear[0])/spdNear;
					}
					if(spdMid > 0) {
						SPKLineMiddle[0] = 100*(Spread[0] - LowMiddle[0])/spdMid;
					}
					if(spdFar > 0) {
						SPKLineFar[0] = 100*(Spread[0] - LowFar[0])/spdFar;
					}
					if(spdAllTime > 0) {
						SPKLineAllTime[0] = 100*(Spread[0] - LowAllTime)/spdAllTime;
					}
					if(PrintOut > 1)
						Print(string.Format("{0}: spdAllTime={1}, avgPrice={2}, pctSpdAvgP={3}%, HiAllTime={4}, LowAllTime={5}",
						CurrentBars[BarsInProgress], spdAllTime, avgPrice, pctSpdAvgP, HiAllTime, LowAllTime));
				} catch(Exception ex) {
					if(PrintOut > 1)
						Print(string.Format("GetSpreadHiLoBack error: ", ex.Message));
			}
		}
		
		public void CheckContext() {
			if(IsLastBarOnChart(BarsInProgress) > 0) {
				WriteCtxParaObj();
				return;
			} else if(BarsPeriod.BarsPeriodType == BarsPeriodType.Day || Times[0][0].Day != Times[0][1].Day){				
				SetPairSpdCtx();
			}
			SetSpreadHiLo();
		}
		
		public void SetPairSpdCtx() {
			string key = BarsPeriod.BarsPeriodType == BarsPeriodType.Day? 
				this.GetDateStrByDateTime(Times[0][0]):this.GetDateStrByDateTime(Times[0][1]);
			CtxPairSpd ctxps = new CtxPairSpd();
			ctxps.Symbol = Instrument.MasterInstrument.Name;
			ctxps.TimeOpen = 830;
			ctxps.TimeClose = 1030;
			ctxps.TimeStart = 830;
			ctxps.TimeEnd = 1430;
			ctxps.PositionInBand = GetSpreadPosInBand().ToString();
			ctxps.TrendDirection = GetSpreadTrend().ToString();
//			ctxps.PairATRRatio = PairATRRatio[0];
//			ctxps.PairSpdHigh = this.PairSpdHigh;
			ctxps.TimeSpdHigh = this.PairSpdHighTime;
//			ctxps.PairSpdLow = this.PairSpdLow;
			ctxps.TimeSpdLow = this.PairSpdLowTime;
			//if()if(giSpdRs.IsSpreadFlat()) ;
			CtxPairSpreadDaily.AddDayCtx(key, new List<CtxPairSpd>{ctxps});
			//Print(string.Format("{0}: SetPairSpdCtx={1}", CurrentBars[BarsInProgress], CtxPairSpreadDaily.DictCtxPairSpd.Count));
//			Print(string.Format("{0}: SetPairSpdCtx Spread[0]={1}, Spread[1]={2}, PairSpdHigh={3}, PairSpdLow={4}, PairSpdHighTime={5}, PairSpdLowTime={6}",
//				CurrentBars[BarsInProgress], Spread[0], Spread[1], PairSpdHigh, PairSpdLow, PairSpdHighTime, PairSpdLowTime));
		}
		
				/// <summary>
		/// Write ctx to Json file
		/// </summary>
		/// <returns></returns>
		public void WriteCtxParaObj() {
			//Print(String.Format("ReadCtxPairSpd paraDict={0}, paraDict.Count={1}", ctxPairSpd, ctxPairSpd.Count));
//			foreach(var ele in ctxPairSpd)
//			{
//				Print(string.Format("DateCtx.ele.Key={0}, ele.Value.ToString()={1}", ele.Key, ele.Value));
//				foreach(CtxPairSpd ctxPS in ele.Value) {
//					Print(string.Format("ctxPS.Symbol={0}, ctxPS.TimeClose={1}", ctxPS.Symbol, ctxPS.TimeClose));
//				}
//			}
			//if(IsInStrategyAnalyzer) 
			//return;
			try{
				string output = GConfig.Dictionary2JsonFile(this.CtxPairSpreadDaily.DictCtxPairSpd, GConfig.GetCTXOutputFilePath());
				Print(string.Format("WriteCtxParaObj={0}", CtxPairSpreadDaily.DictCtxPairSpd.Count));
				foreach(string key in CtxPairSpreadDaily.DictCtxPairSpd.Keys) {
					List<CtxPairSpd> cps = CtxPairSpreadDaily.DictCtxPairSpd[key];
					if(cps != null && cps.Count > 0) {
						CtxPairSpd ctx = cps[0];
						if(PrintOut > 1)
							Print(string.Format("{0}\t{1}\t{2}", key, ctx.TimeSpdHigh, ctx.TimeSpdLow));
					}
				}
			} catch(Exception ex) {
				Print(string.Format("{0}: WriteCtxParaObj exception {1}", CurrentBars[0], ex.Message));
			}
		}
		
		private string GetLongShortText() {
			String txt = "N/A";
			if(Spread[0] != null) {
				if(Spread[0] < 50) {
					txt = "L " + Instrument.MasterInstrument.Name + " : S " + SecondSymbol;
				} else {
					txt = "S " + Instrument.MasterInstrument.Name + " : L " + SecondSymbol;
				}
			}
			return txt;
		}
		
		#region Properties
		[Browsable(false), XmlIgnore()]
		public Series<double> Spread
		{
			get { return Values[0]; }
		}

		[Browsable(false), XmlIgnore()]
		public Series<double> SPKLineNear
		{
			get { return Values[1]; }
		}
		
		[Browsable(false), XmlIgnore()]
		public Series<double> SPKLineMiddle
		{
			get { return Values[2]; }
		}
		
		[Browsable(false), XmlIgnore()]
		public Series<double> SPKLineFar
		{
			get { return Values[3]; }
		}

		[Browsable(false), XmlIgnore()]
		public Series<double> SPKLineAllTime
		{
			get { return Values[4]; }
		}
		
		[Browsable(false), XmlIgnore()]
		public Series<double> HiNear
		{
			get; set;
		}
		
		[Browsable(false), XmlIgnore()]
		public Series<double> LowNear
		{
			get; set;
		}
		
		[Browsable(false), XmlIgnore()]
		public Series<double> HiFar
		{
			get; set;
		}
		
		[Browsable(false), XmlIgnore()]
		public Series<double> LowFar
		{
			get; set;
		}
		
		[Browsable(false), XmlIgnore()]
		public Series<double> HiMiddle
		{
			get; set;
		}
		
		[Browsable(false), XmlIgnore()]
		public Series<double> LowMiddle
		{
			get; set;
		}
		
		[Browsable(false), XmlIgnore()]
		public double HiAllTime
		{
			get; set;
		}
		
		[Browsable(false), XmlIgnore()]
		public double LowAllTime
		{
			get; set;
		}
		
		[Browsable(false), XmlIgnore()]
		public int PairSpdHighTime
		{
			get; set;
		}

		[Browsable(false), XmlIgnore()]
		public int PairSpdLowTime
		{
			get; set;
		}

//		[Browsable(false), XmlIgnore()]
//		public GIATRRatio PairATRRatio
//		{
//			get; set;
//		}
		
		[Browsable(false), XmlIgnore()]
		public double PairPriceRatio
		{
			get; set;
		}
		
		[Browsable(false), XmlIgnore()]
		public CtxPairSpdDaily CtxPairSpreadDaily
		{
			get; set;
		}
			
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="SecondSymbol", Description="The second symbol of the pair", GroupName="NinjaScriptParameters", Order=0)]
		public string SecondSymbol
		{ 	get{ return secondSymbol; }
			set{ secondSymbol = value; }
		}
		
		[Range(-1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ChartMinutes", Description="Minutes for the chart", GroupName="NinjaScriptParameters", Order=1)]
		public int ChartMinutes
		{ 	get{ return chartMinutes; }
			set{ chartMinutes = value; }
		}

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PeriodNear", Description="Period Near", GroupName="NinjaScriptParameters", Order=2)]
		public int PeriodNear
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PeriodMiddle", Description="Period Middle", GroupName="NinjaScriptParameters", Order=3)]
		public int PeriodMiddle
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PeriodFar", Description="Period Far", GroupName="NinjaScriptParameters", Order=4)]
		public int PeriodFar
		{ get; set; }

		[Range(0, 100), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SPKLineThresholdLow", GroupName="NinjaScriptParameters", Order = 5)]
		public double SPKLineThresholdLow
		{ get; set; }

		[Range(0, 100), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SPKLineThresholdMid", GroupName="NinjaScriptParameters", Order = 6)]
		public double SPKLineThresholdMid
		{ get; set; }
		
		[Range(0, 100), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SPKLineThresholdHigh", GroupName="NinjaScriptParameters", Order = 7)]
		public double SPKLineThresholdHigh
		{ get; set; }

		[Range(-2, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NumStdDevDownMin", GroupName="NinjaScriptParameters", Order = 8)]
		public double NumStdDevDownMin
		{ get; set; }
		#endregion
		
		#region Pre Defined parameters
		private double capRatio1 = 1.25;
		private double capRatio2 = 1;		
		private string secondSymbol = "CWENA";
		private int chartMinutes = 0;
		private double pctChgSpdThresholdEn = -2.3;
		private double pctChgSpdThresholdEx = 2.5;
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GISpdLadder[] cacheGISpdLadder;
		public GISpdLadder GISpdLadder(string secondSymbol, int chartMinutes, int periodNear, int periodMiddle, int periodFar, double sPKLineThresholdLow, double sPKLineThresholdMid, double sPKLineThresholdHigh, double numStdDevDownMin)
		{
			return GISpdLadder(Input, secondSymbol, chartMinutes, periodNear, periodMiddle, periodFar, sPKLineThresholdLow, sPKLineThresholdMid, sPKLineThresholdHigh, numStdDevDownMin);
		}

		public GISpdLadder GISpdLadder(ISeries<double> input, string secondSymbol, int chartMinutes, int periodNear, int periodMiddle, int periodFar, double sPKLineThresholdLow, double sPKLineThresholdMid, double sPKLineThresholdHigh, double numStdDevDownMin)
		{
			if (cacheGISpdLadder != null)
				for (int idx = 0; idx < cacheGISpdLadder.Length; idx++)
					if (cacheGISpdLadder[idx] != null && cacheGISpdLadder[idx].SecondSymbol == secondSymbol && cacheGISpdLadder[idx].ChartMinutes == chartMinutes && cacheGISpdLadder[idx].PeriodNear == periodNear && cacheGISpdLadder[idx].PeriodMiddle == periodMiddle && cacheGISpdLadder[idx].PeriodFar == periodFar && cacheGISpdLadder[idx].SPKLineThresholdLow == sPKLineThresholdLow && cacheGISpdLadder[idx].SPKLineThresholdMid == sPKLineThresholdMid && cacheGISpdLadder[idx].SPKLineThresholdHigh == sPKLineThresholdHigh && cacheGISpdLadder[idx].NumStdDevDownMin == numStdDevDownMin && cacheGISpdLadder[idx].EqualsInput(input))
						return cacheGISpdLadder[idx];
			return CacheIndicator<GISpdLadder>(new GISpdLadder(){ SecondSymbol = secondSymbol, ChartMinutes = chartMinutes, PeriodNear = periodNear, PeriodMiddle = periodMiddle, PeriodFar = periodFar, SPKLineThresholdLow = sPKLineThresholdLow, SPKLineThresholdMid = sPKLineThresholdMid, SPKLineThresholdHigh = sPKLineThresholdHigh, NumStdDevDownMin = numStdDevDownMin }, input, ref cacheGISpdLadder);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GISpdLadder GISpdLadder(string secondSymbol, int chartMinutes, int periodNear, int periodMiddle, int periodFar, double sPKLineThresholdLow, double sPKLineThresholdMid, double sPKLineThresholdHigh, double numStdDevDownMin)
		{
			return indicator.GISpdLadder(Input, secondSymbol, chartMinutes, periodNear, periodMiddle, periodFar, sPKLineThresholdLow, sPKLineThresholdMid, sPKLineThresholdHigh, numStdDevDownMin);
		}

		public Indicators.GISpdLadder GISpdLadder(ISeries<double> input , string secondSymbol, int chartMinutes, int periodNear, int periodMiddle, int periodFar, double sPKLineThresholdLow, double sPKLineThresholdMid, double sPKLineThresholdHigh, double numStdDevDownMin)
		{
			return indicator.GISpdLadder(input, secondSymbol, chartMinutes, periodNear, periodMiddle, periodFar, sPKLineThresholdLow, sPKLineThresholdMid, sPKLineThresholdHigh, numStdDevDownMin);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GISpdLadder GISpdLadder(string secondSymbol, int chartMinutes, int periodNear, int periodMiddle, int periodFar, double sPKLineThresholdLow, double sPKLineThresholdMid, double sPKLineThresholdHigh, double numStdDevDownMin)
		{
			return indicator.GISpdLadder(Input, secondSymbol, chartMinutes, periodNear, periodMiddle, periodFar, sPKLineThresholdLow, sPKLineThresholdMid, sPKLineThresholdHigh, numStdDevDownMin);
		}

		public Indicators.GISpdLadder GISpdLadder(ISeries<double> input , string secondSymbol, int chartMinutes, int periodNear, int periodMiddle, int periodFar, double sPKLineThresholdLow, double sPKLineThresholdMid, double sPKLineThresholdHigh, double numStdDevDownMin)
		{
			return indicator.GISpdLadder(input, secondSymbol, chartMinutes, periodNear, periodMiddle, periodFar, sPKLineThresholdLow, sPKLineThresholdMid, sPKLineThresholdHigh, numStdDevDownMin);
		}
	}
}

#endregion
