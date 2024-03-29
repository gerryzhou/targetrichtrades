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
		public const int BipQQQ = 1;
		public const int BipIWM = 2;
		
		public const int BipSpyLn = 3;		
		public const int BipSpySt = 4;
		public const int BipQQQLn = 5;
		public const int BipQQQSt = 6;		
		public const int BipIWMLn = 7;
		public const int BipIWMSt = 8;
		
		public const int RocFraction = 2;
		
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
		private SMA		sma;
		private StdDev	stdDev;		
//		private Series<double> sumSeries;
//		private Series<double> smaSeries;
//		private Series<double> stdDevSeries;
		
		//private Bollinger bol;
//		private SMA	smaClose1;
//		private SMA	smaClose2;
//		private SMA	smaPctChgRatio;
		List<DailyMaxMin> listRocHiLoMaxMin;
		
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
				RocPeriod									= 10;				
				RocScale									= 10000;
				BasePriceType								= 0;
				NoTrade										= 0;
//				RocHighBip									= -1;
//				RocMidBip									= -1;
//				RocLowBip									= -1;
				NumStdDevUp									= 1.6;
				NumStdDevDown								= 2.6;
				NumStdDevUpMin								= 0.5;
				NumStdDevDownMin							= 0.5;
				ChartMinutes								= 8;
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
				
				AddPlot(new Stroke(Brushes.Black), PlotStyle.Dot, "SPY");
				AddPlot(new Stroke(Brushes.Orange), PlotStyle.Dot, "QQQ");
				AddPlot(new Stroke(Brushes.LightSkyBlue), PlotStyle.Dot, "IWM");
				AddPlot(new Stroke(Brushes.Magenta), PlotStyle.Dot, "MxMiSpread");
				
				AddPlot(Brushes.Gray, "Mean");
				AddPlot(Brushes.Red, "UpperBB");
				AddPlot(Brushes.Green, "LowerBB");
				
				AddPlot(Brushes.Blue, "SpdCoVar");
				//AddLine(Brushes.Blue, 0, "IWM");
			}
			else if (State == State.Configure)
			{
				//AddDataSeries("NQ 06-20", Data.BarsPeriodType.Minute, 13, Data.MarketDataType.Last);
				//AddDataSeries("RTY 06-20", Data.BarsPeriodType.Minute, 13, Data.MarketDataType.Last);
				//AddDataSeries("NRGU", Data.BarsPeriodType.Minute, 1, Data.MarketDataType.Last);
				AddDataSeries(QQQSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
				AddDataSeries(IWMSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
				if(NoTrade > 0) {
					AddDataSeries(SpyLnSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
					AddDataSeries(SpyStSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
					
					AddDataSeries(QQQLnSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
					AddDataSeries(QQQStSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
					
					AddDataSeries(IWMLnSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
					AddDataSeries(IWMStSymbol, Data.BarsPeriodType.Minute, ChartMinutes, Data.MarketDataType.Last);
				}
				RocHighBip = new Series<int>(this);
				RocMidBip = new Series<int>(this);
				RocLowBip = new Series<int>(this);
				//bol		= Bollinger(RocHiLoSpread, 1.6, RocPeriod);
			}
			else if (State == State.DataLoaded)
			{			
//				RocChg = new Series<double>(this);			
				Print(String.Format("{0}: DataLoaded...BarsArray.Length={1}", 
					this.GetType().Name, BarsArray.Length));
				SmaSpy = SMA(BarsArray[BipSpy], RocPeriod);
				SmaQQQ = SMA(BarsArray[BipQQQ], RocPeriod);
				SmaIWM = SMA(BarsArray[BipIWM], RocPeriod);
				
				sma		= SMA(RocHiLoSpread, RocPeriod);
				stdDev	= StdDev(RocHiLoSpread, RocPeriod);
//				GI_BBSqz = GIBBSqz(RocHiLoSpread, 2, 20, 10, 4);
				
				RocHiLoSpdMaxMin = new DailyMaxMin();
				listRocHiLoMaxMin = new List<DailyMaxMin>();
				DailyRocHiLoMidCross = new DailyCross<int>();
//				RocHiLoSpreadMax = double.MinValue;
//				RocHiLoSpreadMin = double.MaxValue;
//				smaClose1 = SMA(Closes[0], 5);
//				smaClose2 = SMA(Closes[1], 5);
//				smaPctChgRatio = SMA(RocChg, 50);
				//stdDev = StdDev(PlotPctSpd, 7);
				//AddChartIndicator(stdDev);
			}
		}

		protected override void OnBarUpdate()
		{
//			Print(String.Format("{0}:[{1}] GISQRSpd called BarsRequiredToPlot={2},", 
//			CurrentBars[BarsInProgress], BarsInProgress, BarsRequiredToPlot));
			if(CurrentBars[BarsInProgress] < BarsRequiredToPlot)
				return;
			//double BaseSpy=0, BaseQQQ=0, BaseIWM=0;

			SetBasePrice();
			SetRoc();
			GetRocHiLoMidBip();
			SetRocSpreadHiLo();
//			GI_BBSqz.Update();
//			MiddleBB[0] = GI_BBSqz.SmaVal[0];
//			UpperBB[0] = GI_BBSqz.UpperBB[0];
//			LowerBB[0] = GI_BBSqz.LowerBB[0];
			if(NoTrade > 0)
				CheckTradeEvent();
//			PctChgArr[BarsInProgress] = GetPctChg(BarsInProgress);
//			SetPctChgSpread();
			//if(PrintOut > 1)
			//	PrintPctChgSpd();
			if(IsLastBarOnChart(BipIWM) > 0 && BarsInProgress == BipIWM) {
//				PrintDailyMaxMinList(this.listRocHiLoMaxMin);
//				DailyRocHiLoMidCross.PrintCrosses();
			}
		}
		
		/// <summary>
		/// Set the Bip for long, short entrys 
		/// </summary>
		private void SetLongShortBips(bool reversal) {
			if(BarsInProgress != BipIWM) return;
			switch(RocHighBip[0]) {
				case BipSpy:
					ShortBip = reversal ? BipSpySt : BipSpyLn;
					break;
				case BipQQQ:
					ShortBip = reversal ? BipQQQSt : BipQQQLn;
					break;
				case BipIWM:
					ShortBip = reversal ? BipIWMSt : BipIWMLn;
					break;
				default:
					if(reversal) ShortBip = -1;
					else LongBip = -1;
					break;
			}
			switch(RocLowBip[0]) {
				case BipSpy:
					LongBip = reversal ? BipSpyLn : BipSpySt;
					break;
				case BipQQQ:
					LongBip = reversal ? BipQQQLn : BipQQQSt;
					break;
				case BipIWM:
					LongBip = reversal ? BipIWMLn : BipIWMSt;
					break;
				default:
					if(reversal) LongBip = -1;
					else ShortBip = -1;
					break;
			}
			switch(RocMidBip[0]) {
				case BipSpy:
					MidLongBip = reversal ? BipSpyLn : BipSpySt;
					MidShortBip = reversal ? BipSpySt : BipSpyLn;
					break;
				case BipQQQ:
					MidLongBip = reversal ? BipQQQLn : BipQQQSt;
					MidShortBip = reversal ? BipQQQSt : BipQQQLn;
					break;
				case BipIWM:
					MidLongBip = reversal ? BipIWMLn : BipIWMSt;
					MidShortBip = reversal ? BipIWMSt : BipIWMLn;
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
//			Print(string.Format("{0}:SetLongShortBips bip={1}, LongBip={2}, ShortBip={3}, MidLongBip={4}, MidShortBip={5}, Time={6:yyyyMMdd-HHmm}", 
//					CurrentBars[BarsInProgress], BarsInProgress,
//					LongBip, ShortBip, MidLongBip, MidShortBip,
//					Times[BarsInProgress][0]));
		}
		
		/// <summary>
		/// Get the Bip for High Roc, Low Roc and Mid Roc 
		/// </summary>
		private void GetRocHiLoMidBip() {
			if(BarsInProgress != BipIWM) return;
			
			double mx = Math.Max(RocSpy[0], Math.Max(RocQQQ[0], RocIWM[0]));			
			double mi = Math.Min(RocSpy[0], Math.Min(RocQQQ[0], RocIWM[0]));
			RocHighBip[0] = (mx==RocSpy[0])? BipSpy : ((mx==RocQQQ[0])? BipQQQ:BipIWM);
			RocLowBip[0] = (mi==RocSpy[0])? BipSpy : ((mi==RocQQQ[0])? BipQQQ:BipIWM);
			RocMidBip[0] = 3 - RocHighBip[0] - RocLowBip[0];
//			Print(string.Format("{0}: bip={1}, RocHighBip={2}, RocLowBip={3}, RocMidBip={4}, RocSpy={5}, RocQQQ={6}, RocIWM={7}, Time={8:yyyyMMdd-HHmm}", 
//					CurrentBars[BarsInProgress], BarsInProgress,
//					RocHighBip, RocLowBip, RocMidBip,
//					RocSpy[0], RocQQQ[0], RocIWM[0],
//					Times[BarsInProgress][0]));
		}
		
		private void SetBasePrice() {
			if(BarsArray[BarsInProgress].IsFirstBarOfSession) {				
//				RocHiLoSpreadMax = double.MinValue;
//				RocHiLoSpreadMin = double.MaxValue;
				if(BarsInProgress == BipIWM) {
					this.listRocHiLoMaxMin.Add(RocHiLoSpdMaxMin);
					RocHiLoSpdMaxMin = new DailyMaxMin();
				}
				switch(BarsInProgress) {
					case BipSpy:
						BaseSpy = BasePriceType==1? Opens[BarsInProgress][0] : SmaSpy[0];
						break;
					case BipQQQ:
						BaseQQQ = BasePriceType==1? Opens[BarsInProgress][0] : SmaQQQ[0]; 
						break;
					case BipIWM:
						BaseIWM = BasePriceType==1? Opens[BarsInProgress][0] : SmaIWM[0]; 
						break;
					case BipSpyLn: //Can only use open for quantity calculation, so en=ex for quantity;
						BaseLnSpy = Opens[BarsInProgress][0]; 
						break;
					case BipSpySt:
						BaseStSpy = Opens[BarsInProgress][0]; 
						break;
					case BipQQQLn:
						BaseLnQQQ = Opens[BarsInProgress][0]; 
						break;
					case BipQQQSt:
						BaseStQQQ = Opens[BarsInProgress][0]; 
						break;
					case BipIWMLn:
						BaseLnIWM = Opens[BarsInProgress][0]; 
						break;
					case BipIWMSt:
						BaseStIWM = Opens[BarsInProgress][0];
						break;
					default:
						break;
				}
			}
		}
		
		private void SetRoc() {
			switch(BarsInProgress) {
				case BipSpy:
					RocSpy[0] = GetNormalizedRocPrice(Closes[BarsInProgress][0], BaseSpy, RocScale, RocFraction);
					if(IsFutures(BarsArray[BarsInProgress].Instrument)) {
						Print(BarsInProgress + ":" 
						+ BarsArray[BarsInProgress].Instrument.MasterInstrument.Name
						+ " PointValue=" 
						+ BarsArray[BarsInProgress].Instrument.MasterInstrument.PointValue);
					}
					break;
				case BipQQQ:
					RocQQQ[0] = GetNormalizedRocPrice(Closes[BarsInProgress][0], BaseQQQ, RocScale, RocFraction);
					if(IsFutures(BarsArray[BarsInProgress].Instrument)) {
						Print(BarsInProgress + ":" 
						+ BarsArray[BarsInProgress].Instrument.MasterInstrument.Name
						+ " PointValue=" 
						+ BarsArray[BarsInProgress].Instrument.MasterInstrument.PointValue);
					}
					break;
				case BipIWM:
					RocIWM[0] = GetNormalizedRocPrice(Closes[BarsInProgress][0], BaseIWM, RocScale, RocFraction);					
					if(IsFutures(BarsArray[BarsInProgress].Instrument)) {
						Print(BarsInProgress + ":" 
						+ BarsArray[BarsInProgress].Instrument.MasterInstrument.Name
						+ " PointValue=" 
						+ BarsArray[BarsInProgress].Instrument.MasterInstrument.PointValue);
					}
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
			if(BarsInProgress != BipIWM) return false;
			
			if(RocHighBip[0] >= 0 && RocHighBip[0] < 3 
				&& RocLowBip[0] >= 0 && RocLowBip[0] < 3 
				&& RocMidBip[0] >= 0 && RocMidBip[0] < 3) {
				double mx = GetRocByBip(RocHighBip[0]);
				double mi = GetRocByBip(RocLowBip[0]);
				double md = GetRocByBip(RocMidBip[0]);
				RocHighSpread = mx - md;
				RocLowSpread = mi - md;
				RocHiLoSpread[0] = mx - mi;
				double sma0		= sma[0];
				double stdDev0	= stdDev[0];
				UpperBB[0]		= sma0 + NumStdDevUp * stdDev0;
				MiddleBB[0]		= sma0;
				LowerBB[0]		= sma0 - NumStdDevDown * stdDev0;
				//use CHOP to try the bandWidth % value over MaxBand
				SpdCoVar[0]		= 10*(UpperBB[0]-LowerBB[0])/RocHiLoSpread[0];
				CheckRocHiLoSpdMaxMin();
				CheckRocHiLoMidCross();
//				Print(string.Format("{0}: bip={1}, RocHighBip={2}, RocLowBip={3}, RocMidBip={4}, RocHighSpread={5}, RocLowSpread={6},RocHiLoSpread[0]={7}, sma0={8}, stdDev0={9}, Time={10:yyyyMMdd-HHmm}", 
//					CurrentBars[BarsInProgress], BarsInProgress,
//					RocHighBip, RocLowBip, RocMidBip,
//					RocHighSpread, RocLowSpread, RocHiLoSpread[0],
//					sma0, stdDev0,
//					Times[BarsInProgress][0]));
				return true;
			}
			else return false;
		}
		
		private double GetRocByBip(int bip) {
			double roc = (bip == BipSpy) ? RocSpy[0] : ((bip == BipQQQ) ? RocQQQ[0] : RocIWM[0]);
			return roc;
		}
		
		private void CheckRocHiLoSpdMaxMin() {
			if(RocHiLoSpread[0] > RocHiLoSpdMaxMin.DailyMax) {
				RocHiLoSpdMaxMin.DailyMax = RocHiLoSpread[0];
				RocHiLoSpdMaxMin.DailyMaxTime = Times[BarsInProgress][0];
			}
			if(RocHiLoSpread[0] < RocHiLoSpdMaxMin.DailyMin) {
				RocHiLoSpdMaxMin.DailyMin = RocHiLoSpread[0];
				RocHiLoSpdMaxMin.DailyMinTime = Times[BarsInProgress][0];
			}
		}
		
		private void CheckRocHiLoMidCross() {
			if(RocHighBip[0] >= 0 && RocHighBip[0] < 3 
				&& RocLowBip[0] >= 0 && RocLowBip[0] < 3 
				&& RocMidBip[0] >= 0 && RocMidBip[0] < 3
				&& RocHighBip[1] >= 0 && RocHighBip[1] < 3 
				&& RocLowBip[1] >= 0 && RocLowBip[1] < 3 
				&& RocMidBip[1] >= 0 && RocMidBip[1] < 3) {
					//Corss over detected
					if(RocLowBip[0] != RocLowBip[1] 
						&& !BarsArray[BarsInProgress].IsFirstBarOfSession) {
						Cross<int> crs = new Cross<int>(RocLowBip[1], -1);
						if(RocLowBip[1] == RocMidBip[0]) { //Cross over mid
							crs.CrossType = 1;
							crs.CrossTime = Times[RocLowBip[1]][0];
						}
						else if(RocLowBip[1] == RocHighBip[0]) { //Cross over high
							crs.CrossType = 2;
							crs.CrossTime = Times[RocLowBip[1]][0];
						}
						DailyRocHiLoMidCross.AddCross(crs);
					}
					//Corss below detected
					if(RocHighBip[0] != RocHighBip[1]
						&& !BarsArray[BarsInProgress].IsFirstBarOfSession) {
						Cross<int> crs = new Cross<int>(RocHighBip[1], -1);
						if(RocHighBip[1] == RocMidBip[0]) { //Cross below mid
							crs.CrossType = -1;
							crs.CrossTime = Times[RocHighBip[1]][0];
						}
						else if(RocHighBip[1] == RocLowBip[0]) { //Cross blow low
							crs.CrossType = -2;
							crs.CrossTime = Times[RocHighBip[1]][0];
						}
						DailyRocHiLoMidCross.AddCross(crs);
					}
				}
		}
		
		#region Bollinger Band Functions
		public bool IsSpreadFlat() {
			bool isFlat = false;
			if(IsSpreadMiddleUp() || IsSpreadMiddleDown())
				isFlat = true;
			return isFlat;
		}
		
		public bool IsSpreadMiddleUp() {
			bool isMu = false;
			//if(RocHiLoSpread[0] < UpperMin[0] && RocHiLoSpread[0] >= MiddleBB[0])
//			if(RocHiLoSpread[0] < UpperBB[0] && RocHiLoSpread[0] >= MiddleBB[0])
//				isMu = true;
			return isMu;
		}

		public bool IsSpreadMiddleDown() {
			bool isMd = false;
//			if(RocHiLoSpread[0] > LowerMin[0] && RocHiLoSpread[0] <= MiddleBB[0])
//			if(RocHiLoSpread[0] > LowerBB[0] && RocHiLoSpread[0] <= MiddleBB[0])
//				isMd = true;
			return isMd;
		}
		
		public bool IsSpreadUpBand() {
			bool isUb = false;
//			if(RocHiLoSpread[0] >= UpperMin[0] && RocHiLoSpread[0] < UpperBB[0])
//			if(RocHiLoSpread[0] >= MiddleBB[0] && RocHiLoSpread[0] < UpperBB[0])
//				isUb = true;
			return isUb;
		}

		public bool IsSpreadLowBand() {
			bool isLb = false;
//			if(RocHiLoSpread[0] <= LowerMin[0] && RocHiLoSpread[0] > LowerBB[0])
//			if(RocHiLoSpread[0] <= MiddleBB[0] && RocHiLoSpread[0] > LowerBB[0])
//				isLb = true;
			return isLb;
		}
				
		public bool IsSpreadBreakout() {
			bool isBk = false;
//			if(RocHiLoSpread[0] >= UpperBB[0])
//				isBk = true;
			return isBk;
		}
		
		public bool IsSpreadBreakdown() {
			bool isBd = false;
//			if(RocHiLoSpread[0] <= LowerBB[0])
//				isBd = true;
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
		#endregion
		
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
			if(BarsInProgress == BipIWM) {
				//&& SetRocSpreadHiLo()) {
//				Print(String.Format("{0}:CheckTradeEvent En Bip{1}: PctSpd={2}, MaxBip={3}, MinBip={4}",
//				CurrentBars[BarsInProgress], BarsInProgress, PlotPctSpd[0], PctChgMaxBip, PctChgMinBip));
				
				IndicatorSignal isig = new IndicatorSignal();
				Direction dir = new Direction();
				SignalAction sigAct = new SignalAction();
				PairSpread<int> prSpdHi = new PairSpread<int>();
				PairSpread<int> prSpdLo = new PairSpread<int>();
				if(IsSpreadBreakdown()) {
					dir.TrendDir = TrendDirection.Up;
					isig.SignalName = SignalName_BelowStdDev;
					SetLongShortBips(true);
				}
//				else if(Spread[0] <= LowerMin[0]) {
//					dir.TrendDir = TrendDirection.Up;
//					isig.SignalName = SignalName_BelowStdDevMin;
//				}
				else if(IsSpreadBreakout()) {
					dir.TrendDir = TrendDirection.Down;
					isig.SignalName = SignalName_AboveStdDev;
					SetLongShortBips(true);
				}
				
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
				//isig.SignalName = SignalName_EntrySQRSpread;
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
		
		public void PrintDailyMaxMinList(List<DailyMaxMin> list) {
			foreach(DailyMaxMin dmm in list) {
				Print(string.Format("{0:0.0000}\t{1:yyyyMMdd}\t{1:HHmm}\t{2:0.0000}\t{3:yyyyMMdd}\t{3:HHmm}", dmm.DailyMax, dmm.DailyMaxTime, dmm.DailyMin, dmm.DailyMinTime));
			}
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
		
		/// <summary>
		/// The spread of High Roc and Low Roc
		/// </summary>
		[Browsable(false), XmlIgnore()]
		public Series<double> RocHiLoSpread
		{
			get { return Values[3]; }
		}
		
		[Browsable(false), XmlIgnore()]
		public Series<double> MiddleBB
		{
			get { return Values[4]; }
		}
		
		[Browsable(false), XmlIgnore()]
		public Series<double> UpperBB
		{
			get { return Values[5]; }
		}
		
		[Browsable(false), XmlIgnore()]
		public Series<double> LowerBB
		{
			get { return Values[6]; }
		}
		
		[Browsable(false), XmlIgnore()]
		public Series<double> SpdCoVar
		{
			get { return Values[7]; }
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
	
		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NumStdDevUp", GroupName="Parameters", Order = 18)]
		public double NumStdDevUp
		{ get; set; }

		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NumStdDevDown", GroupName="Parameters", Order = 19)]
		public double NumStdDevDown
		{ get; set; }
		
		[Range(-2, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NumStdDevUpMin", GroupName="Parameters", Order = 20)]
		public double NumStdDevUpMin
		{ get; set; }

		[Range(-2, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NumStdDevDownMin", GroupName="Parameters", Order = 21)]
		public double NumStdDevDownMin
		{ get; set; }
		
		/// <summary>
		/// Base Price using SMA or Open
		/// 0=SMA, 1=Open
		/// </summary>
		[Range(0, 1), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BasePriceType", GroupName="Parameters", Order = 22)]
		public int BasePriceType
		{ get; set; }
		
		/// <summary>
		/// Include the trade symbols or not
		/// 0=Exclude, 1=Include
		/// </summary>
		[Range(0, 1), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NoTrade", GroupName="Parameters", Order = 23)]
		public int NoTrade
		{ get; set; }
		
		[Browsable(false), XmlIgnore]
		public double BaseSpy
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public double BaseLnSpy
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public double BaseStSpy
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public double BaseQQQ
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public double BaseLnQQQ
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public double BaseStQQQ
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public double BaseIWM
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public double BaseLnIWM
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public double BaseStIWM
		{
			get;set;
		}
		
		
		[Browsable(false), XmlIgnore]
		public SMA SmaSpy
		{
			get;set;
		}
		
		
		[Browsable(false), XmlIgnore]
		public SMA SmaQQQ
		{
			get;set;
		}
		
		
		[Browsable(false), XmlIgnore]
		public SMA SmaIWM
		{
			get;set;
		}
		
		/// <summary>
		/// The bip of highest Roc
		/// </summary>
		[Browsable(false), XmlIgnore]
		public Series<int> RocHighBip
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public Series<int> RocMidBip
		{
			get;set;
		}

		[Browsable(false), XmlIgnore]
		public Series<int> RocLowBip
		{
			get;set;
		}

		/// <summary>
		/// The spread between RocHigh and RocMid
		/// </summary>
		[Browsable(false), XmlIgnore]
		public double RocHighSpread
		{
			get;set;
		}
		
		/// <summary>
		/// The spread between RocLow and RocMid
		/// </summary>
		[Browsable(false), XmlIgnore]
		public double RocLowSpread
		{
			get;set;
		}
		
//		[Browsable(false), XmlIgnore]
//		public GIBBSqz GI_BBSqz
//		{
//			get;set;
//		}
		
		/// <summary>
		/// The daily max/min values of RocHiLoSpread
		/// </summary>
		[Browsable(false), XmlIgnore]
		public DailyMaxMin RocHiLoSpdMaxMin
		{
			get;set;
		}
		
		/// <summary>
		/// The daily cross from Roc High/Low to Roc Mid
		/// </summary>
		[Browsable(false), XmlIgnore]
		public DailyCross<int> DailyRocHiLoMidCross
		{
			get;set;
		}
		
//		/// <summary>
//		/// The time of max daily value of RocHiLoSpread
//		/// </summary>
//		[Browsable(false), XmlIgnore]
//		public DateTime RocHiLoSpdMaxTime
//		{
//			get;set;
//		}
		
//		/// <summary>
//		/// The min daily value of RocHiLoSpread
//		/// </summary>
//		[Browsable(false), XmlIgnore]
//		public double RocHiLoSpreadMin
//		{
//			get;set;
//		}

//		/// <summary>
//		/// The time of min daily value of RocHiLoSpread
//		/// </summary>
//		[Browsable(false), XmlIgnore]
//		public DateTime RocHiLoSpdMinTime
//		{
//			get;set;
//		}

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
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GISQRSpd[] cacheGISQRSpd;
		public GISQRSpd GISQRSpd(int rocPeriod, int chartMinutes, string spySymbol, string qQQSymbol, string iWMSymbol, string spyLnSymbol, int spyLnSymbolRatio, string spyStSymbol, int spyStSymbolRatio, string qQQLnSymbol, int qQQLnSymbolRatio, string qQQStSymbol, int qQQStSymbolRatio, string iWMLnSymbol, int iWMLnSymbolRatio, string iWMStSymbol, int iWMStSymbolRatio, int tradeBaseSymbol, int rocScale, double numStdDevUp, double numStdDevDown, double numStdDevUpMin, double numStdDevDownMin, int basePriceType, int noTrade)
		{
			return GISQRSpd(Input, rocPeriod, chartMinutes, spySymbol, qQQSymbol, iWMSymbol, spyLnSymbol, spyLnSymbolRatio, spyStSymbol, spyStSymbolRatio, qQQLnSymbol, qQQLnSymbolRatio, qQQStSymbol, qQQStSymbolRatio, iWMLnSymbol, iWMLnSymbolRatio, iWMStSymbol, iWMStSymbolRatio, tradeBaseSymbol, rocScale, numStdDevUp, numStdDevDown, numStdDevUpMin, numStdDevDownMin, basePriceType, noTrade);
		}

		public GISQRSpd GISQRSpd(ISeries<double> input, int rocPeriod, int chartMinutes, string spySymbol, string qQQSymbol, string iWMSymbol, string spyLnSymbol, int spyLnSymbolRatio, string spyStSymbol, int spyStSymbolRatio, string qQQLnSymbol, int qQQLnSymbolRatio, string qQQStSymbol, int qQQStSymbolRatio, string iWMLnSymbol, int iWMLnSymbolRatio, string iWMStSymbol, int iWMStSymbolRatio, int tradeBaseSymbol, int rocScale, double numStdDevUp, double numStdDevDown, double numStdDevUpMin, double numStdDevDownMin, int basePriceType, int noTrade)
		{
			if (cacheGISQRSpd != null)
				for (int idx = 0; idx < cacheGISQRSpd.Length; idx++)
					if (cacheGISQRSpd[idx] != null && cacheGISQRSpd[idx].RocPeriod == rocPeriod && cacheGISQRSpd[idx].ChartMinutes == chartMinutes && cacheGISQRSpd[idx].SpySymbol == spySymbol && cacheGISQRSpd[idx].QQQSymbol == qQQSymbol && cacheGISQRSpd[idx].IWMSymbol == iWMSymbol && cacheGISQRSpd[idx].SpyLnSymbol == spyLnSymbol && cacheGISQRSpd[idx].SpyLnSymbolRatio == spyLnSymbolRatio && cacheGISQRSpd[idx].SpyStSymbol == spyStSymbol && cacheGISQRSpd[idx].SpyStSymbolRatio == spyStSymbolRatio && cacheGISQRSpd[idx].QQQLnSymbol == qQQLnSymbol && cacheGISQRSpd[idx].QQQLnSymbolRatio == qQQLnSymbolRatio && cacheGISQRSpd[idx].QQQStSymbol == qQQStSymbol && cacheGISQRSpd[idx].QQQStSymbolRatio == qQQStSymbolRatio && cacheGISQRSpd[idx].IWMLnSymbol == iWMLnSymbol && cacheGISQRSpd[idx].IWMLnSymbolRatio == iWMLnSymbolRatio && cacheGISQRSpd[idx].IWMStSymbol == iWMStSymbol && cacheGISQRSpd[idx].IWMStSymbolRatio == iWMStSymbolRatio && cacheGISQRSpd[idx].TradeBaseSymbol == tradeBaseSymbol && cacheGISQRSpd[idx].RocScale == rocScale && cacheGISQRSpd[idx].NumStdDevUp == numStdDevUp && cacheGISQRSpd[idx].NumStdDevDown == numStdDevDown && cacheGISQRSpd[idx].NumStdDevUpMin == numStdDevUpMin && cacheGISQRSpd[idx].NumStdDevDownMin == numStdDevDownMin && cacheGISQRSpd[idx].BasePriceType == basePriceType && cacheGISQRSpd[idx].NoTrade == noTrade && cacheGISQRSpd[idx].EqualsInput(input))
						return cacheGISQRSpd[idx];
			return CacheIndicator<GISQRSpd>(new GISQRSpd(){ RocPeriod = rocPeriod, ChartMinutes = chartMinutes, SpySymbol = spySymbol, QQQSymbol = qQQSymbol, IWMSymbol = iWMSymbol, SpyLnSymbol = spyLnSymbol, SpyLnSymbolRatio = spyLnSymbolRatio, SpyStSymbol = spyStSymbol, SpyStSymbolRatio = spyStSymbolRatio, QQQLnSymbol = qQQLnSymbol, QQQLnSymbolRatio = qQQLnSymbolRatio, QQQStSymbol = qQQStSymbol, QQQStSymbolRatio = qQQStSymbolRatio, IWMLnSymbol = iWMLnSymbol, IWMLnSymbolRatio = iWMLnSymbolRatio, IWMStSymbol = iWMStSymbol, IWMStSymbolRatio = iWMStSymbolRatio, TradeBaseSymbol = tradeBaseSymbol, RocScale = rocScale, NumStdDevUp = numStdDevUp, NumStdDevDown = numStdDevDown, NumStdDevUpMin = numStdDevUpMin, NumStdDevDownMin = numStdDevDownMin, BasePriceType = basePriceType, NoTrade = noTrade }, input, ref cacheGISQRSpd);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GISQRSpd GISQRSpd(int rocPeriod, int chartMinutes, string spySymbol, string qQQSymbol, string iWMSymbol, string spyLnSymbol, int spyLnSymbolRatio, string spyStSymbol, int spyStSymbolRatio, string qQQLnSymbol, int qQQLnSymbolRatio, string qQQStSymbol, int qQQStSymbolRatio, string iWMLnSymbol, int iWMLnSymbolRatio, string iWMStSymbol, int iWMStSymbolRatio, int tradeBaseSymbol, int rocScale, double numStdDevUp, double numStdDevDown, double numStdDevUpMin, double numStdDevDownMin, int basePriceType, int noTrade)
		{
			return indicator.GISQRSpd(Input, rocPeriod, chartMinutes, spySymbol, qQQSymbol, iWMSymbol, spyLnSymbol, spyLnSymbolRatio, spyStSymbol, spyStSymbolRatio, qQQLnSymbol, qQQLnSymbolRatio, qQQStSymbol, qQQStSymbolRatio, iWMLnSymbol, iWMLnSymbolRatio, iWMStSymbol, iWMStSymbolRatio, tradeBaseSymbol, rocScale, numStdDevUp, numStdDevDown, numStdDevUpMin, numStdDevDownMin, basePriceType, noTrade);
		}

		public Indicators.GISQRSpd GISQRSpd(ISeries<double> input , int rocPeriod, int chartMinutes, string spySymbol, string qQQSymbol, string iWMSymbol, string spyLnSymbol, int spyLnSymbolRatio, string spyStSymbol, int spyStSymbolRatio, string qQQLnSymbol, int qQQLnSymbolRatio, string qQQStSymbol, int qQQStSymbolRatio, string iWMLnSymbol, int iWMLnSymbolRatio, string iWMStSymbol, int iWMStSymbolRatio, int tradeBaseSymbol, int rocScale, double numStdDevUp, double numStdDevDown, double numStdDevUpMin, double numStdDevDownMin, int basePriceType, int noTrade)
		{
			return indicator.GISQRSpd(input, rocPeriod, chartMinutes, spySymbol, qQQSymbol, iWMSymbol, spyLnSymbol, spyLnSymbolRatio, spyStSymbol, spyStSymbolRatio, qQQLnSymbol, qQQLnSymbolRatio, qQQStSymbol, qQQStSymbolRatio, iWMLnSymbol, iWMLnSymbolRatio, iWMStSymbol, iWMStSymbolRatio, tradeBaseSymbol, rocScale, numStdDevUp, numStdDevDown, numStdDevUpMin, numStdDevDownMin, basePriceType, noTrade);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GISQRSpd GISQRSpd(int rocPeriod, int chartMinutes, string spySymbol, string qQQSymbol, string iWMSymbol, string spyLnSymbol, int spyLnSymbolRatio, string spyStSymbol, int spyStSymbolRatio, string qQQLnSymbol, int qQQLnSymbolRatio, string qQQStSymbol, int qQQStSymbolRatio, string iWMLnSymbol, int iWMLnSymbolRatio, string iWMStSymbol, int iWMStSymbolRatio, int tradeBaseSymbol, int rocScale, double numStdDevUp, double numStdDevDown, double numStdDevUpMin, double numStdDevDownMin, int basePriceType, int noTrade)
		{
			return indicator.GISQRSpd(Input, rocPeriod, chartMinutes, spySymbol, qQQSymbol, iWMSymbol, spyLnSymbol, spyLnSymbolRatio, spyStSymbol, spyStSymbolRatio, qQQLnSymbol, qQQLnSymbolRatio, qQQStSymbol, qQQStSymbolRatio, iWMLnSymbol, iWMLnSymbolRatio, iWMStSymbol, iWMStSymbolRatio, tradeBaseSymbol, rocScale, numStdDevUp, numStdDevDown, numStdDevUpMin, numStdDevDownMin, basePriceType, noTrade);
		}

		public Indicators.GISQRSpd GISQRSpd(ISeries<double> input , int rocPeriod, int chartMinutes, string spySymbol, string qQQSymbol, string iWMSymbol, string spyLnSymbol, int spyLnSymbolRatio, string spyStSymbol, int spyStSymbolRatio, string qQQLnSymbol, int qQQLnSymbolRatio, string qQQStSymbol, int qQQStSymbolRatio, string iWMLnSymbol, int iWMLnSymbolRatio, string iWMStSymbol, int iWMStSymbolRatio, int tradeBaseSymbol, int rocScale, double numStdDevUp, double numStdDevDown, double numStdDevUpMin, double numStdDevDownMin, int basePriceType, int noTrade)
		{
			return indicator.GISQRSpd(input, rocPeriod, chartMinutes, spySymbol, qQQSymbol, iWMSymbol, spyLnSymbol, spyLnSymbolRatio, spyStSymbol, spyStSymbolRatio, qQQLnSymbol, qQQLnSymbolRatio, qQQStSymbol, qQQStSymbolRatio, iWMLnSymbol, iWMLnSymbolRatio, iWMStSymbol, iWMStSymbolRatio, tradeBaseSymbol, rocScale, numStdDevUp, numStdDevDown, numStdDevUpMin, numStdDevDownMin, basePriceType, noTrade);
		}
	}
}

#endregion
