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
	/// SPY: SH LS, SPXL L,
	/// QQQ: SQQQ S, PSQ L,
	/// IWM: TNA L, TZA S,
	/// </summary>
	public class GISQRSpd : GIndicatorBase
	{
		public const int BipSpy = 0;
		public const int BipSpyLn = 3;
		public const int BipSpySt = 4;
		public const int BipQQQ = 1;
		public const int BipQQQLn = 5;
		public const int BipQQQSt = 6;
		public const int BipIWM = 2;
		public const int BipIWMLn = 7;
		public const int BipIWMSt = 8;
		
		public const int RocFraction = 8;
		
//		private Series<double> PctSpd;
//		private Series<double> RocChg;
//		private PriorDayOHLC lastDayOHLC;
//		private double[] PctChgArr = new double[]{-101, -101};
//		private double PctChgSpdMin, PctChgSpdMax;
		
		//PctChgSpdWideCount = PctChgSpdEnCount  +PctChgSpdExCount
		//PctChgSpdNarrowCount = PctChgSpdCount - PctChgSpdWideCount
		//En<0, Ex>0
//		private double PctChgSpdCount = 0, PctChgSpdEnCount = 0, PctChgSpdExCount = 0;
//		private double PctChgSpdEnSum, PctChgSpdExSum;
		//int PctChgMaxBip=-1, PctChgMinBip=-1;
		private StdDev stdDev;
//		private SMA	smaClose1;
//		private SMA	smaClose2;
//		private SMA	smaPctChgRatio;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"SPY, QQQ and IWM Pair scalper";
				Name										= "GISQRSpd";
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
				RocScale									= 10000;
				RocHighBip									= -1;
				RocMidBip									= -1;
				RocLowBip									= -1;
				ChartMinutes								= 4;
				TM_OpenStartH								= 11;
				TM_OpenStartM								= 15;
				TM_OpenEndH									= 12;
				TM_OpenEndM									= 45;
				TM_ClosingH									= 13;
				TM_ClosingM									= 15;
				//PctChgMaxBip								= -1;
				//PctChgMinBip								= -1;
				BarsRequiredToPlot							= 128;
				MaximumBarsLookBack							= MaximumBarsLookBack.Infinite;
				
				QQQSymbol									= "QQQ";
				IWMSymbol									= "IWM";
				SpyLnSymbol									= "SPXL";
				SpyStSymbol									= "SH";
				QQQLnSymbol									= "QLD";
				QQQStSymbol									= "SQQQ";
				IWMLnSymbol									= "TNA";
				IWMStSymbol									= "TZA";
				SpyLnSymbolRatio							= 3;//"SPXL";
				SpyStSymbolRatio							= 1;//"SH";
				QQQLnSymbolRatio							= 2;//"QLD";
				QQQStSymbolRatio							= 3;//"SQQQ";
				IWMLnSymbolRatio							= 3;//"TNA";
				IWMStSymbolRatio							= 3;//"TZA";
				
				AddPlot(Brushes.Red, "SPY");
				AddPlot(Brushes.Orange, "QQQ");
				AddPlot(Brushes.Blue, "IWM");
				//AddLine(Brushes.Blue, 0, "IWM");
			}
			else if (State == State.Configure)
			{
				//AddDataSeries("NQ 06-20", Data.BarsPeriodType.Minute, 13, Data.MarketDataType.Last);
				//AddDataSeries("RTY 06-20", Data.BarsPeriodType.Minute, 13, Data.MarketDataType.Last);
				//AddDataSeries("NRGU", Data.BarsPeriodType.Minute, 1, Data.MarketDataType.Last);
				AddDataSeries(QQQSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
				AddDataSeries(IWMSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
				
				AddDataSeries(SpyLnSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
				AddDataSeries(SpyStSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
				
				AddDataSeries(QQQLnSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
				AddDataSeries(QQQStSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
				
				AddDataSeries(IWMLnSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
				AddDataSeries(IWMStSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
			}
			else if (State == State.DataLoaded)
			{			
//				RocChg = new Series<double>(this);			
				Print(String.Format("{0}: DataLoaded...BarsArray.Length={1}", 
					this.GetType().Name, BarsArray.Length));
//				smaClose1 = SMA(Closes[0], 5);
//				smaClose2 = SMA(Closes[1], 5);
//				smaPctChgRatio = SMA(RocChg, 50);
				//stdDev = StdDev(PlotPctSpd, 7);
				//AddChartIndicator(stdDev);
			}
		}

		protected override void OnBarUpdate()
		{
			//Print(String.Format("{0}:[{1}] BarsRequiredToPlot={2},", CurrentBars[BarsInProgress], BarsInProgress, BarsRequiredToPlot));
			if(CurrentBars[BarsInProgress] < BarsRequiredToPlot)
				return;
			//double openSpy=0, openQQQ=0, openIWM=0;
			SetOpenPrice();
			SetRoc();
			GetRocHiLoMidBip();
			SetLongShortBips();
			CheckTradeEvent();
//			PctChgArr[BarsInProgress] = GetPctChg(BarsInProgress);
//			SetPctChgSpread();
			//if(PrintOut > 1)
			//	PrintPctChgSpd();
		}
		
		/// <summary>
		/// Set the Bip for long, short entrys 
		/// </summary>
		private void SetLongShortBips() {
			if(BarsInProgress != BipIWM) return;
			switch(RocHighBip) {
				case BipSpy:
					ShortBip = BipSpySt;
					break;
				case BipQQQ:
					ShortBip = BipQQQSt;
					break;
				case BipIWM:
					ShortBip = BipIWMSt;
					break;
				default:
					ShortBip = -1;
					break;
			}
			switch(RocLowBip) {
				case BipSpy:
					LongBip = BipSpyLn;
					break;
				case BipQQQ:
					LongBip = BipQQQLn;
					break;
				case BipIWM:
					LongBip = BipIWMLn;
					break;
				default:
					LongBip = -1;
					break;
			}
			switch(RocMidBip) {
				case BipSpy:
					MidLongBip = BipSpyLn;
					MidShortBip = BipSpySt;
					break;
				case BipQQQ:
					MidLongBip = BipQQQLn;
					MidShortBip = BipQQQSt;
					break;
				case BipIWM:
					MidLongBip = BipIWMLn;
					MidShortBip = BipIWMSt;
					break;
				default:
					MidLongBip = -1;
					MidShortBip = -1;
					break;
			}
			
//			double mx = Math.Max(RocSpy[0], Math.Max(RocQQQ[0], RocIWM[0]));			
//			double mi = Math.Min(RocSpy[0], Math.Min(RocQQQ[0], RocIWM[0]));
//			RocHighBip = (mx==RocSpy[0])? BipSpy : ((mx==RocQQQ[0])? BipQQQ:BipIWM);
//			RocLowBip = (mi==RocSpy[0])? BipSpy : ((mi==RocQQQ[0])? BipQQQ:BipIWM);
//			RocMidBip = 3 - RocHighBip - RocLowBip;
			Print(string.Format("{0}:SetLongShortBips bip={1}, LongBip={2}, ShortBip={3}, MidLongBip={4}, MidShortBip={5}, Time={6:yyyyMMdd-HHmm}", 
					CurrentBars[BarsInProgress], BarsInProgress,
					LongBip, ShortBip, MidLongBip, MidShortBip,
					Times[BarsInProgress][0]));
		}
		
		/// <summary>
		/// Get the Bip for High Roc, Low Roc and Mid Roc 
		/// </summary>
		private void GetRocHiLoMidBip() {
			if(BarsInProgress != BipIWM) return;
			
			double mx = Math.Max(RocSpy[0], Math.Max(RocQQQ[0], RocIWM[0]));			
			double mi = Math.Min(RocSpy[0], Math.Min(RocQQQ[0], RocIWM[0]));
			RocHighBip = (mx==RocSpy[0])? BipSpy : ((mx==RocQQQ[0])? BipQQQ:BipIWM);
			RocLowBip = (mi==RocSpy[0])? BipSpy : ((mi==RocQQQ[0])? BipQQQ:BipIWM);
			RocMidBip = 3 - RocHighBip - RocLowBip;
			Print(string.Format("{0}: bip={1}, RocHighBip={2}, RocLowBip={3}, RocMidBip={4}, RocSpy={5}, RocQQQ={6}, RocIWM={7}, Time={8:yyyyMMdd-HHmm}", 
					CurrentBars[BarsInProgress], BarsInProgress,
					RocHighBip, RocLowBip, RocMidBip,
					RocSpy[0], RocQQQ[0], RocIWM[0],
					Times[BarsInProgress][0]));
		}
		
		private void SetOpenPrice() {
			if(BarsArray[BarsInProgress].IsFirstBarOfSession) {
				switch(BarsInProgress) {
					case BipSpy:
						OpenSpy = Opens[BarsInProgress][0]; 
						break;
					case BipQQQ:
						OpenQQQ = Opens[BarsInProgress][0]; 
						break;
					case BipIWM:
						OpenIWM = Opens[BarsInProgress][0]; 
						break;
					case BipSpyLn:
						OpenLnSpy = Opens[BarsInProgress][0]; 
						break;
					case BipSpySt:
						OpenStSpy = Opens[BarsInProgress][0]; 
						break;
					case BipQQQLn:
						OpenLnQQQ = Opens[BarsInProgress][0]; 
						break;
					case BipQQQSt:
						OpenStQQQ = Opens[BarsInProgress][0]; 
						break;
					case BipIWMLn:
						OpenLnIWM = Opens[BarsInProgress][0]; 
						break;
					case BipIWMSt:
						OpenStIWM = Opens[BarsInProgress][0];
						break;
					default:
						break;
				}
			}
		}
		
		private void SetRoc() {
			switch(BarsInProgress) {
				case BipSpy:
					RocSpy[0] = GetNormalizedRocPrice(Closes[BarsInProgress][0], OpenSpy, RocScale, RocFraction);
					break;
				case BipQQQ:
					RocQQQ[0] = GetNormalizedRocPrice(Closes[BarsInProgress][0], OpenQQQ, RocScale, RocFraction);
					break;
				case BipIWM:
					RocIWM[0] = GetNormalizedRocPrice(Closes[BarsInProgress][0], OpenIWM, RocScale, RocFraction);					
					break;					
				default:
					break;
			}			
		}
		
		/// <summary>
		/// Set high RocSpread = HighSpd - MidSpd
		/// Set low RocSpread = LowSpd - MidSpd
		/// </summary>
		private bool SetRocSpreadHiLo() {
			if(RocHighBip >= 0 && RocHighBip < 3 
				&& RocLowBip >= 0 && RocLowBip < 3 
				&& RocMidBip >= 0 && RocMidBip < 3) {
				double mx = (RocHighBip == 0) ? RocSpy[0] : ((RocHighBip == 1) ? RocQQQ[0] : RocIWM[0]);
				double mi = (RocLowBip == 0) ? RocSpy[0] : ((RocLowBip == 1) ? RocQQQ[0] : RocIWM[0]);
				double md = (RocMidBip == 0) ? RocSpy[0] : ((RocMidBip == 1) ? RocQQQ[0] : RocIWM[0]);
				RocHighSpread = mx - md;
				RocLowSpread = mi - md;
				Print(string.Format("{0}: bip={1}, RocHighBip={2}, RocLowBip={3}, RocMidBip={4}, RocHighSpread={5}, RocLowSpread={6}, Time={7:yyyyMMdd-HHmm}", 
					CurrentBars[BarsInProgress], BarsInProgress,
					RocHighBip, RocLowBip, RocMidBip,
					RocHighSpread, RocLowSpread,
					Times[BarsInProgress][0]));
				return true;
			}
			else return false;
		}
		
		private void FireThresholdEvent(double spd) {
			IndicatorSignal isig = new IndicatorSignal();
			//if(CurrentBar < 300)
//				Print(String.Format("{0}:Close={1}, PctChgSpd={2}, PctChgSpdThresholdEn={3}, PctChgSpdThresholdEx={4}",
//				CurrentBar, Close[0], spd, PctChgSpdThresholdEn, PctChgSpdThresholdEx));
//			if(spd <= PctChgSpdThresholdEn) {
//				isig.BreakoutDir = BreakoutDirection.Down;
//				isig.SignalName = SignalName_BreakdownMV;
//			} else if(spd >= PctChgSpdThresholdEx) {
//				isig.BreakoutDir = BreakoutDirection.Up;
//				isig.SignalName = SignalName_BreakoutMV;
//			} else
//				return;
			
			isig.BarNo = CurrentBar;
			isig.IndicatorSignalType = SignalType.SimplePriceAction;
			IndicatorEventArgs ievt = new IndicatorEventArgs(this.GetType().Name, " CheckMeanV: ");
			ievt.IndSignal = isig;
			//FireEvent(ievt);
			OnRaiseIndicatorEvent(ievt);
		}
		
		private void PrintPctChgSpd() {
			if(IsLastBarOnChart() > 0 && BarsInProgress == 0) {
//				for(int i=0; i < CurrentBar-BarsRequiredToPlot; i++) {
//					Print(string.Format("{0:0.00}	{1:0.00}	{2:0.00}	{3}	{4:yyyyMMdd_HHmm}",
//						PlotPctSpd[i], PctChg1[i], PctChg2[i], CurrentBar-i, Times[0][i]));
//				}
//				Print(string.Format("{0}: PctChgSpdMax={1}, PctChgSpdMin={2}, PctChgSpdThresholdEn={3}, PctChgSpdThresholdEx={4}",
//					CurrentBar, PctChgSpdMax, PctChgSpdMin, PctChgSpdThresholdEn, PctChgSpdThresholdEx));
//				double PctChgSpdWideCount = PctChgSpdWideCount = PctChgSpdEnCount  + PctChgSpdExCount;
//				double PctChgSpdNarrowCount = PctChgSpdCount - PctChgSpdWideCount;
//				Print(string.Format("{0}: PctChgSpdEnCount={1}, {2:0.00}%, PctChgSpdExCount={3}, {4:0.00}% PctChgSpdCount={5}",
//					CurrentBar, PctChgSpdEnCount, 100*PctChgSpdEnCount/PctChgSpdCount, PctChgSpdExCount, 100*PctChgSpdExCount/PctChgSpdCount, PctChgSpdCount));
//				Print(string.Format("{0}: PctChgSpdEnAvg={1}, PctChgSpdExAvg={2}",
//					CurrentBar, PctChgSpdEnSum/PctChgSpdEnCount, PctChgSpdExSum/PctChgSpdExCount));
//				Print(string.Format("{0}: PctChgSpdWideCount={1}, {2:0.00}%, PctChgSpdNarrowCount={3}, {4:0.00}% PctChgSpdCount={5}",
//					CurrentBar, PctChgSpdWideCount, 100*PctChgSpdWideCount/PctChgSpdCount, PctChgSpdNarrowCount, 100*PctChgSpdNarrowCount/PctChgSpdCount, PctChgSpdCount));
			}
		}
		
		private void DrawTextValue() {			
			Draw.TextFixed(this, "NinjaScriptInfo", GetLongShortText(), TextPosition.TopLeft,
				Brushes.LimeGreen, new SimpleFont("Arial", 18), Brushes.Transparent, Brushes.Transparent, 0);
		}
		
		private string GetLongShortText() {
			String txt = "N/A";
//			if(PlotPctSpd[0] != null && PctChgMaxBip >= 0 && PctChgMinBip >= 0) {
//				if(PlotPctSpd[0] > 0) {
//					txt = "L " + (PctChgMaxBip+1).ToString() + " : S " + (PctChgMinBip+1).ToString();
//				} else {
//					txt = "S " + (PctChgMaxBip+1).ToString() + " : L " + (PctChgMinBip+1).ToString();
//				}
//			}
			return txt;
		}
		
		public bool IsTradingTime(DateTime dt) {
			int startTime = GetTimeByHM(TM_OpenStartH, TM_OpenStartM, false);
			int endTime = GetTimeByHM(TM_OpenEndH, TM_OpenEndM, false);
			return IsTimeInSpan(dt, startTime, endTime);			
		}
				
		public void CheckTradeEvent() {
			//int en_H = TM_OpenEndH, en_M = TM_OpenEndM, ex_H = TM_ClosingH, ex_M = TM_ClosingM;		

			//entry at 9:02 am ct
			if(BarsInProgress == BipIWM 
				&& IsTradingTime(Times[BipIWM][0])
				&& SetRocSpreadHiLo()) {
//				Print(String.Format("{0}:CheckTradeEvent En Bip{1}: PctSpd={2}, MaxBip={3}, MinBip={4}",
//				CurrentBars[BarsInProgress], BarsInProgress, PlotPctSpd[0], PctChgMaxBip, PctChgMinBip));
				
				IndicatorSignal isig = new IndicatorSignal();
				Direction dir = new Direction();
				SignalAction sigAct = new SignalAction();
				PairSpread<int> prSpdHi = new PairSpread<int>();
				PairSpread<int> prSpdLo = new PairSpread<int>();
				prSpdHi.SpdType = SpreadType.High;
				prSpdHi.SpreadValue = RocHighSpread;
				prSpdHi.Symbol1 = ShortBip;
				prSpdHi.Symbol2 = MidLongBip;
				
				prSpdLo.SpdType = SpreadType.Low;
				prSpdLo.SpreadValue = RocLowSpread;
				prSpdLo.Symbol1 = LongBip;
				prSpdLo.Symbol2 = MidShortBip;
				
				List<PairSpread<int>> pspdList = new List<PairSpread<int>>();
				pspdList.Add(prSpdHi);
				pspdList.Add(prSpdLo);
				sigAct.PairSpds = pspdList;
	//			Print(String.Format("{0}: [{1}] Non-CutoffTime {2}: MaxBip={3}, %Max={4}, MinBip={5}, %Min={6}, %Spd={7}", 
	//				CurrentBar, Time[0], GetLongShortText(),
	//				PctChgMaxBip, PctChgMax[0], PctChgMinBip, PctChgMin[0], PlotPctSpd[0]));

//				if(PctChgMaxBip != PctChgMinBip) {
//					if(PlotPctSpd[0] > 0) {
//						dir.TrendDir = TrendDirection.Up;
//						isig.SignalName = SignalName_EntryOnOpenLong;
//					} else if(PlotPctSpd[0] < 0) {
//						dir.TrendDir = TrendDirection.Down;
//						isig.SignalName = SignalName_EntryOnOpenShort;
//					}
//				} else
//					return;
				
				isig.BarNo = CurrentBars[BarsInProgress];
				isig.TrendDir = dir;
				isig.IndicatorSignalType = SignalType.Spread;
				isig.SignalName = SignalName_EntrySQRSpread;
				isig.SignalAction = sigAct;
				IndicatorEventArgs ievt = new IndicatorEventArgs(this.GetType().Name, String.Format(" [{0}] {1}", Times[BarsInProgress][0], GetLongShortText()));
				ievt.IndSignal = isig;
				
				//FireEvent(ievt);
				OnRaiseIndicatorEvent(ievt);
			}
//			else {// if(IsCutoffTime(BarsInProgress, ex_H, ex_M)) {
////				Print(String.Format("{0}:CheckTradeEvent Ex Bip{1}: PctSpd={2}, MaxBip={3}, MinBip={4}", 
////				CurrentBars[BarsInProgress], BarsInProgress, PlotPctSpd[0], PctChgMaxBip, PctChgMinBip));
					
//				IndicatorSignal isig = new IndicatorSignal();
//				Direction dir = new Direction();
//	//			Print(String.Format("{0}: [{1}] Non-CutoffTime {2}: MaxBip={3}, %Max={4}, MinBip={5}, %Min={6}, %Spd={7}", 
//	//				CurrentBar, Time[0], GetLongShortText(),
//	//				PctChgMaxBip, PctChgMax[0], PctChgMinBip, PctChgMin[0], PlotPctSpd[0]));

//				dir.TrendDir = TrendDirection.UnKnown;
//				isig.SignalName = SignalName_ExitForOpen;
////				if(PlotPctSpd[0] > 0) {
////					dir.TrendDir = TrendDirection.Up;
////					isig.SignalName = SignalName_ExitForOpen;
////				} else if(PlotPctSpd[0] < 0) {
////					dir.TrendDir = TrendDirection.Down;
////					isig.SignalName = SignalName_ExitForOpen;
////				} else
////					return;
				
//				isig.BarNo = CurrentBars[BarsInProgress];
//				isig.TrendDir = dir;
//				isig.IndicatorSignalType = SignalType.SimplePriceAction;
//				IndicatorEventArgs ievt = new IndicatorEventArgs(this.GetType().Name, " CheckTradeEvent Ex: ");
//				ievt.IndSignal = isig;
//				//FireEvent(ievt);
//				OnRaiseIndicatorEvent(ievt);
//			}
		}
		
		#region Properties
		[Browsable(false), XmlIgnore()]
		public Series<double> RocSpy
		{
			get { return Values[0]; }
		}

		[Browsable(false), XmlIgnore()]
		public Series<double> RocQQQ
		{
			get { return Values[1]; }
		}
		
		[Browsable(false), XmlIgnore()]
		public Series<double> RocIWM
		{
			get { return Values[2]; }
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RocPeriod", Description="Rate of chage period", Order=0, GroupName="Parameters")]
		public int RocPeriod
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ChartMinutes", Description="Minutes for the chart", Order=1, GroupName="Parameters")]
		public int ChartMinutes
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="SpySymbol", Description="The spy symbol", Order=2, GroupName="Parameters")]
		public string SpySymbol
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="QQQSymbol", Description="The qqq symbol", Order=2, GroupName="Parameters")]
		public string QQQSymbol
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="IWMSymbol", Description="The iwm symbol", Order=3, GroupName="Parameters")]
		public string IWMSymbol
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="SpyLnSymbol", Description="The long symbol of spy", Order=4, GroupName="Parameters")]
		public string SpyLnSymbol
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SpyLnSymbolRatio", Description="x ratio of the symbol", Order=5, GroupName="Parameters")]
		public int SpyLnSymbolRatio
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="SpyStSymbol", Description="The short symbol of spy", Order=6, GroupName="Parameters")]
		public string SpyStSymbol
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SpyStSymbolRatio", Description="x ratio of the symbol", Order=7, GroupName="Parameters")]
		public int SpyStSymbolRatio
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="QQQLnSymbol", Description="The long symbol of qqq", Order=8, GroupName="Parameters")]
		public string QQQLnSymbol
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="QQQLnSymbolRatio", Description="x ratio of the symbol", Order=9, GroupName="Parameters")]
		public int QQQLnSymbolRatio
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="QQQStSymbol", Description="The short symbol of qqq", Order=10, GroupName="Parameters")]
		public string QQQStSymbol
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="QQQStSymbolRatio", Description="x ratio of the symbol", Order=11, GroupName="Parameters")]
		public int QQQStSymbolRatio
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="IWMLnSymbol", Description="The long symbol of iwm", Order=12, GroupName="Parameters")]
		public string IWMLnSymbol
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="IWMLnSymbolRatio", Description="x ratio of the symbol", Order=13, GroupName="Parameters")]
		public int IWMLnSymbolRatio
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="IWMStSymbol", Description="The short symbol of iwm", Order=14, GroupName="Parameters")]
		public string IWMStSymbol
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="IWMStSymbolRatio", Description="x ratio of the symbol", Order=15, GroupName="Parameters")]
		public int IWMStSymbolRatio
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="TradeBaseSymbol", Description="The base symbol for calculating size", Order=16, GroupName="Parameters")]
		public int TradeBaseSymbol
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RocScale", Description="Fold for Roc", Order=17, GroupName="Parameters")]
		public int RocScale
		{ get; set; }
	
		[Browsable(false), XmlIgnore]
		public double OpenSpy
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public double OpenLnSpy
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public double OpenStSpy
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public double OpenQQQ
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public double OpenLnQQQ
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public double OpenStQQQ
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public double OpenIWM
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public double OpenLnIWM
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public double OpenStIWM
		{
			get;set;
		}
		//The BarsInProgress for PctChgMax and PctChgMin
		[Browsable(false), XmlIgnore]
		public int RocHighBip
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public int RocMidBip
		{
			get;set;
		}

		[Browsable(false), XmlIgnore]
		public int RocLowBip
		{
			get;set;
		}

		[Browsable(false), XmlIgnore]
		public double RocHighSpread
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public double RocLowSpread
		{
			get;set;
		}

		/// <summary>
		/// The symbol bip to put short trade
		/// </summary>
		[Browsable(false), XmlIgnore]
		public int ShortBip
		{
			get;set;
		}
		
		/// <summary>
		/// The symbol bip to put long trade
		/// </summary>
		[Browsable(false), XmlIgnore]
		public int LongBip
		{
			get;set;
		}
		
		/// <summary>
		/// The symbol bip to put long trade as mid position
		/// </summary>
		[Browsable(false), XmlIgnore]
		public int MidLongBip
		{
			get;set;
		}
		
		/// <summary>
		/// The symbol bip to put short trade as mid position
		/// </summary>
		[Browsable(false), XmlIgnore]
		public int MidShortBip
		{
			get;set;
		}
		#endregion
		
		#region Pre Defined parameters
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
		private GISQRSpd[] cacheGISQRSpd;
		public GISQRSpd GISQRSpd(int rocPeriod, int chartMinutes, string spySymbol, string qQQSymbol, string iWMSymbol, string spyLnSymbol, int spyLnSymbolRatio, string spyStSymbol, int spyStSymbolRatio, string qQQLnSymbol, int qQQLnSymbolRatio, string qQQStSymbol, int qQQStSymbolRatio, string iWMLnSymbol, int iWMLnSymbolRatio, string iWMStSymbol, int iWMStSymbolRatio, int tradeBaseSymbol, int rocScale)
		{
			return GISQRSpd(Input, rocPeriod, chartMinutes, spySymbol, qQQSymbol, iWMSymbol, spyLnSymbol, spyLnSymbolRatio, spyStSymbol, spyStSymbolRatio, qQQLnSymbol, qQQLnSymbolRatio, qQQStSymbol, qQQStSymbolRatio, iWMLnSymbol, iWMLnSymbolRatio, iWMStSymbol, iWMStSymbolRatio, tradeBaseSymbol, rocScale);
		}

		public GISQRSpd GISQRSpd(ISeries<double> input, int rocPeriod, int chartMinutes, string spySymbol, string qQQSymbol, string iWMSymbol, string spyLnSymbol, int spyLnSymbolRatio, string spyStSymbol, int spyStSymbolRatio, string qQQLnSymbol, int qQQLnSymbolRatio, string qQQStSymbol, int qQQStSymbolRatio, string iWMLnSymbol, int iWMLnSymbolRatio, string iWMStSymbol, int iWMStSymbolRatio, int tradeBaseSymbol, int rocScale)
		{
			if (cacheGISQRSpd != null)
				for (int idx = 0; idx < cacheGISQRSpd.Length; idx++)
					if (cacheGISQRSpd[idx] != null && cacheGISQRSpd[idx].RocPeriod == rocPeriod && cacheGISQRSpd[idx].ChartMinutes == chartMinutes && cacheGISQRSpd[idx].SpySymbol == spySymbol && cacheGISQRSpd[idx].QQQSymbol == qQQSymbol && cacheGISQRSpd[idx].IWMSymbol == iWMSymbol && cacheGISQRSpd[idx].SpyLnSymbol == spyLnSymbol && cacheGISQRSpd[idx].SpyLnSymbolRatio == spyLnSymbolRatio && cacheGISQRSpd[idx].SpyStSymbol == spyStSymbol && cacheGISQRSpd[idx].SpyStSymbolRatio == spyStSymbolRatio && cacheGISQRSpd[idx].QQQLnSymbol == qQQLnSymbol && cacheGISQRSpd[idx].QQQLnSymbolRatio == qQQLnSymbolRatio && cacheGISQRSpd[idx].QQQStSymbol == qQQStSymbol && cacheGISQRSpd[idx].QQQStSymbolRatio == qQQStSymbolRatio && cacheGISQRSpd[idx].IWMLnSymbol == iWMLnSymbol && cacheGISQRSpd[idx].IWMLnSymbolRatio == iWMLnSymbolRatio && cacheGISQRSpd[idx].IWMStSymbol == iWMStSymbol && cacheGISQRSpd[idx].IWMStSymbolRatio == iWMStSymbolRatio && cacheGISQRSpd[idx].TradeBaseSymbol == tradeBaseSymbol && cacheGISQRSpd[idx].RocScale == rocScale && cacheGISQRSpd[idx].EqualsInput(input))
						return cacheGISQRSpd[idx];
			return CacheIndicator<GISQRSpd>(new GISQRSpd(){ RocPeriod = rocPeriod, ChartMinutes = chartMinutes, SpySymbol = spySymbol, QQQSymbol = qQQSymbol, IWMSymbol = iWMSymbol, SpyLnSymbol = spyLnSymbol, SpyLnSymbolRatio = spyLnSymbolRatio, SpyStSymbol = spyStSymbol, SpyStSymbolRatio = spyStSymbolRatio, QQQLnSymbol = qQQLnSymbol, QQQLnSymbolRatio = qQQLnSymbolRatio, QQQStSymbol = qQQStSymbol, QQQStSymbolRatio = qQQStSymbolRatio, IWMLnSymbol = iWMLnSymbol, IWMLnSymbolRatio = iWMLnSymbolRatio, IWMStSymbol = iWMStSymbol, IWMStSymbolRatio = iWMStSymbolRatio, TradeBaseSymbol = tradeBaseSymbol, RocScale = rocScale }, input, ref cacheGISQRSpd);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GISQRSpd GISQRSpd(int rocPeriod, int chartMinutes, string spySymbol, string qQQSymbol, string iWMSymbol, string spyLnSymbol, int spyLnSymbolRatio, string spyStSymbol, int spyStSymbolRatio, string qQQLnSymbol, int qQQLnSymbolRatio, string qQQStSymbol, int qQQStSymbolRatio, string iWMLnSymbol, int iWMLnSymbolRatio, string iWMStSymbol, int iWMStSymbolRatio, int tradeBaseSymbol, int rocScale)
		{
			return indicator.GISQRSpd(Input, rocPeriod, chartMinutes, spySymbol, qQQSymbol, iWMSymbol, spyLnSymbol, spyLnSymbolRatio, spyStSymbol, spyStSymbolRatio, qQQLnSymbol, qQQLnSymbolRatio, qQQStSymbol, qQQStSymbolRatio, iWMLnSymbol, iWMLnSymbolRatio, iWMStSymbol, iWMStSymbolRatio, tradeBaseSymbol, rocScale);
		}

		public Indicators.GISQRSpd GISQRSpd(ISeries<double> input , int rocPeriod, int chartMinutes, string spySymbol, string qQQSymbol, string iWMSymbol, string spyLnSymbol, int spyLnSymbolRatio, string spyStSymbol, int spyStSymbolRatio, string qQQLnSymbol, int qQQLnSymbolRatio, string qQQStSymbol, int qQQStSymbolRatio, string iWMLnSymbol, int iWMLnSymbolRatio, string iWMStSymbol, int iWMStSymbolRatio, int tradeBaseSymbol, int rocScale)
		{
			return indicator.GISQRSpd(input, rocPeriod, chartMinutes, spySymbol, qQQSymbol, iWMSymbol, spyLnSymbol, spyLnSymbolRatio, spyStSymbol, spyStSymbolRatio, qQQLnSymbol, qQQLnSymbolRatio, qQQStSymbol, qQQStSymbolRatio, iWMLnSymbol, iWMLnSymbolRatio, iWMStSymbol, iWMStSymbolRatio, tradeBaseSymbol, rocScale);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GISQRSpd GISQRSpd(int rocPeriod, int chartMinutes, string spySymbol, string qQQSymbol, string iWMSymbol, string spyLnSymbol, int spyLnSymbolRatio, string spyStSymbol, int spyStSymbolRatio, string qQQLnSymbol, int qQQLnSymbolRatio, string qQQStSymbol, int qQQStSymbolRatio, string iWMLnSymbol, int iWMLnSymbolRatio, string iWMStSymbol, int iWMStSymbolRatio, int tradeBaseSymbol, int rocScale)
		{
			return indicator.GISQRSpd(Input, rocPeriod, chartMinutes, spySymbol, qQQSymbol, iWMSymbol, spyLnSymbol, spyLnSymbolRatio, spyStSymbol, spyStSymbolRatio, qQQLnSymbol, qQQLnSymbolRatio, qQQStSymbol, qQQStSymbolRatio, iWMLnSymbol, iWMLnSymbolRatio, iWMStSymbol, iWMStSymbolRatio, tradeBaseSymbol, rocScale);
		}

		public Indicators.GISQRSpd GISQRSpd(ISeries<double> input , int rocPeriod, int chartMinutes, string spySymbol, string qQQSymbol, string iWMSymbol, string spyLnSymbol, int spyLnSymbolRatio, string spyStSymbol, int spyStSymbolRatio, string qQQLnSymbol, int qQQLnSymbolRatio, string qQQStSymbol, int qQQStSymbolRatio, string iWMLnSymbol, int iWMLnSymbolRatio, string iWMStSymbol, int iWMStSymbolRatio, int tradeBaseSymbol, int rocScale)
		{
			return indicator.GISQRSpd(input, rocPeriod, chartMinutes, spySymbol, qQQSymbol, iWMSymbol, spyLnSymbol, spyLnSymbolRatio, spyStSymbol, spyStSymbolRatio, qQQLnSymbol, qQQLnSymbolRatio, qQQStSymbol, qQQStSymbolRatio, iWMLnSymbol, iWMLnSymbolRatio, iWMStSymbol, iWMStSymbolRatio, tradeBaseSymbol, rocScale);
		}
	}
}

#endregion
