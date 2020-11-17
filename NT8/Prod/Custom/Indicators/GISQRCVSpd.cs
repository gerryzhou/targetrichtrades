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
	/// Use CV=StdDev/MA as normalized price;
	/// CHOP index = 100 * LOG10( SUM(ATR(1), n) / ( MaxHi(n) - MinLo(n) ) ) / LOG10(n)
	/// </summary>
	public class GISQRCVSpd : GIndicatorBase
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
		
		public const int CoVarFraction = 2;
		
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
				Description									= @"SPY, QQQ and IWM Pair scalper using CV as price";
				Name										= "GISQRCVSpd";
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
				CoVarPeriod									= 5;				
				CoVarScale									= 10000;
//				CoVarHighBip									= -1;
//				CoVarMidBip									= -1;
//				CoVarLowBip									= -1;
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
				
				CoVarHighBip = new Series<int>(this);
				CoVarMidBip = new Series<int>(this);
				CoVarLowBip = new Series<int>(this);
				//bol		= Bollinger(CoVarHiLoSpread, 1.6, CoVarPeriod);
			}
			else if (State == State.DataLoaded)
			{			
//				RocChg = new Series<double>(this);			
				Print(String.Format("{0}: DataLoaded...BarsArray.Length={1}", 
					this.GetType().Name, BarsArray.Length));
				SmaSpy = SMA(BarsArray[BipSpy], CoVarPeriod);
				StdDevSpy = StdDev(BarsArray[BipSpy], CoVarPeriod);
				//CoVarSpy = StdDevSpy/SmaSpy;
				SmaQQQ = SMA(BarsArray[BipQQQ], CoVarPeriod);
				StdDevQQQ = StdDev(BarsArray[BipQQQ], CoVarPeriod);
				SmaIWM = SMA(BarsArray[BipIWM], CoVarPeriod);
				StdDevIWM = StdDev(BarsArray[BipIWM], CoVarPeriod);
				
				sma		= SMA(CoVarHiLoSpread, CoVarPeriod);
				stdDev	= StdDev(CoVarHiLoSpread, CoVarPeriod);
				
				CoVarHiLoSpdMaxMin = new DailyMaxMin();
				listRocHiLoMaxMin = new List<DailyMaxMin>();
				DailyCoVarHiLoMidCross = new DailyCross<int>();
//				CoVarHiLoSpreadMax = double.MinValue;
//				CoVarHiLoSpreadMin = double.MaxValue;
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
			//double openSpy=0, openQQQ=0, openIWM=0;
			//SetOpenPrice();
			SetCoVar();
			//GetRocHiLoMidBip();
			//SetRocSpreadHiLo();
			//CheckTradeEvent();
//			PctChgArr[BarsInProgress] = GetPctChg(BarsInProgress);
//			SetPctChgSpread();
			//if(PrintOut > 1)
			//	PrintPctChgSpd();
			if(IsLastBarOnChart(BipIWM) > 0 && BarsInProgress == BipIWM) {
//				PrintDailyMaxMinList(this.listRocHiLoMaxMin);
//				DailyCoVarHiLoMidCross.PrintCrosses();
			}
		}
		
		/// <summary>
		/// Set the Bip for long, short entrys 
		/// </summary>
		private void SetLongShortBips(bool reversal) {
			if(BarsInProgress != BipIWM) return;
			switch(CoVarHighBip[0]) {
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
			switch(CoVarLowBip[0]) {
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
			switch(CoVarMidBip[0]) {
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
			
//			double mx = Math.Max(CoVarSpy[0], Math.Max(CoVarQQQ[0], CoVarIWM[0]));			
//			double mi = Math.Min(CoVarSpy[0], Math.Min(CoVarQQQ[0], CoVarIWM[0]));
//			CoVarHighBip = (mx==CoVarSpy[0])? BipSpy : ((mx==CoVarQQQ[0])? BipQQQ:BipIWM);
//			CoVarLowBip = (mi==CoVarSpy[0])? BipSpy : ((mi==CoVarQQQ[0])? BipQQQ:BipIWM);
//			CoVarMidBip = 3 - CoVarHighBip - CoVarLowBip;
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
			
			double mx = Math.Max(CoVarSpy[0], Math.Max(CoVarQQQ[0], CoVarIWM[0]));			
			double mi = Math.Min(CoVarSpy[0], Math.Min(CoVarQQQ[0], CoVarIWM[0]));
			CoVarHighBip[0] = (mx==CoVarSpy[0])? BipSpy : ((mx==CoVarQQQ[0])? BipQQQ:BipIWM);
			CoVarLowBip[0] = (mi==CoVarSpy[0])? BipSpy : ((mi==CoVarQQQ[0])? BipQQQ:BipIWM);
			CoVarMidBip[0] = 3 - CoVarHighBip[0] - CoVarLowBip[0];
//			Print(string.Format("{0}: bip={1}, CoVarHighBip={2}, CoVarLowBip={3}, CoVarMidBip={4}, CoVarSpy={5}, CoVarQQQ={6}, CoVarIWM={7}, Time={8:yyyyMMdd-HHmm}", 
//					CurrentBars[BarsInProgress], BarsInProgress,
//					CoVarHighBip, CoVarLowBip, CoVarMidBip,
//					CoVarSpy[0], CoVarQQQ[0], CoVarIWM[0],
//					Times[BarsInProgress][0]));
		}
		
		private void SetOpenPrice() {
			if(BarsArray[BarsInProgress].IsFirstBarOfSession) {				
//				CoVarHiLoSpreadMax = double.MinValue;
//				CoVarHiLoSpreadMin = double.MaxValue;
				if(BarsInProgress == BipIWM) {
					this.listRocHiLoMaxMin.Add(CoVarHiLoSpdMaxMin);
					CoVarHiLoSpdMaxMin = new DailyMaxMin();
				}
				switch(BarsInProgress) {
					case BipSpy:
//						OpenSpy = Opens[BarsInProgress][0]; 
						break;
					case BipQQQ:
//						OpenQQQ = Opens[BarsInProgress][0]; 
						break;
					case BipIWM:
//						OpenIWM = Opens[BarsInProgress][0]; 
						break;
					case BipSpyLn:
//						OpenLnSpy = Opens[BarsInProgress][0]; 
						break;
					case BipSpySt:
//						OpenStSpy = Opens[BarsInProgress][0]; 
						break;
					case BipQQQLn:
//						OpenLnQQQ = Opens[BarsInProgress][0]; 
						break;
					case BipQQQSt:
//						OpenStQQQ = Opens[BarsInProgress][0]; 
						break;
					case BipIWMLn:
//						OpenLnIWM = Opens[BarsInProgress][0]; 
						break;
					case BipIWMSt:
//						OpenStIWM = Opens[BarsInProgress][0];
						break;
					default:
						break;
				}
			}
		}
		
		private void SetCoVar() {
			switch(BarsInProgress) {
				case BipSpy:
					CoVarSpy[0] = GetCoVar(SmaSpy[0], StdDevSpy[0], CoVarScale, CoVarFraction);
					Print(string.Format("{0}:SetCoVar bip={1}, CoVarSpy[0]={2}, Time={3:yyyyMMdd-HHmm}", 
					CurrentBars[BarsInProgress], BarsInProgress, CoVarSpy[0], Times[BarsInProgress][0]));
					break;
				case BipQQQ:
					CoVarQQQ[0] = GetCoVar(SmaQQQ[0], StdDevQQQ[0], CoVarScale, CoVarFraction);
					Print(string.Format("{0}:SetCoVar bip={1}, CoVarQQQ[0]={2}, Time={3:yyyyMMdd-HHmm}", 
					CurrentBars[BarsInProgress], BarsInProgress, CoVarQQQ[0], Times[BarsInProgress][0]));
					break;
				case BipIWM:
					CoVarIWM[0] = GetCoVar(SmaIWM[0], StdDevIWM[0], CoVarScale, CoVarFraction);
					Print(string.Format("{0}:SetCoVar bip={1}, CoVarIWM[0]={2}, Time={3:yyyyMMdd-HHmm}", 
					CurrentBars[BarsInProgress], BarsInProgress, CoVarIWM[0], Times[BarsInProgress][0]));
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
			
			if(CoVarHighBip[0] >= 0 && CoVarHighBip[0] < 3 
				&& CoVarLowBip[0] >= 0 && CoVarLowBip[0] < 3 
				&& CoVarMidBip[0] >= 0 && CoVarMidBip[0] < 3) {
				double mx = GetRocByBip(CoVarHighBip[0]);
				double mi = GetRocByBip(CoVarLowBip[0]);
				double md = GetRocByBip(CoVarMidBip[0]);
				RocHighSpread = mx - md;
				RocLowSpread = mi - md;
				CoVarHiLoSpread[0] = mx - mi;
				double sma0		= sma[0];
				double stdDev0	= stdDev[0];
				UpperBB[0]		= sma0 + NumStdDevUp * stdDev0;
				MiddleBB[0]		= sma0;
				LowerBB[0]		= sma0 - NumStdDevDown * stdDev0;
				CheckCoVarHiLoSpdMaxMin();
				CheckRocHiLoMidCross();
//				Print(string.Format("{0}: bip={1}, CoVarHighBip={2}, CoVarLowBip={3}, CoVarMidBip={4}, RocHighSpread={5}, RocLowSpread={6},CoVarHiLoSpread[0]={7}, sma0={8}, stdDev0={9}, Time={10:yyyyMMdd-HHmm}", 
//					CurrentBars[BarsInProgress], BarsInProgress,
//					CoVarHighBip, CoVarLowBip, CoVarMidBip,
//					RocHighSpread, RocLowSpread, CoVarHiLoSpread[0],
//					sma0, stdDev0,
//					Times[BarsInProgress][0]));
				return true;
			}
			else return false;
		}
		
		private double GetRocByBip(int bip) {
			double roc = (bip == BipSpy) ? CoVarSpy[0] : ((bip == BipQQQ) ? CoVarQQQ[0] : CoVarIWM[0]);
			return roc;
		}
		
		private void CheckCoVarHiLoSpdMaxMin() {
			if(CoVarHiLoSpread[0] > CoVarHiLoSpdMaxMin.DailyMax) {
				CoVarHiLoSpdMaxMin.DailyMax = CoVarHiLoSpread[0];
				CoVarHiLoSpdMaxMin.DailyMaxTime = Times[BarsInProgress][0];
			}
			if(CoVarHiLoSpread[0] < CoVarHiLoSpdMaxMin.DailyMin) {
				CoVarHiLoSpdMaxMin.DailyMin = CoVarHiLoSpread[0];
				CoVarHiLoSpdMaxMin.DailyMinTime = Times[BarsInProgress][0];
			}
		}
		
		private void CheckRocHiLoMidCross() {
			if(CoVarHighBip[0] >= 0 && CoVarHighBip[0] < 3 
				&& CoVarLowBip[0] >= 0 && CoVarLowBip[0] < 3 
				&& CoVarMidBip[0] >= 0 && CoVarMidBip[0] < 3
				&& CoVarHighBip[1] >= 0 && CoVarHighBip[1] < 3 
				&& CoVarLowBip[1] >= 0 && CoVarLowBip[1] < 3 
				&& CoVarMidBip[1] >= 0 && CoVarMidBip[1] < 3) {
					//Corss over detected
					if(CoVarLowBip[0] != CoVarLowBip[1] 
						&& !BarsArray[BarsInProgress].IsFirstBarOfSession) {
						Cross<int> crs = new Cross<int>(CoVarLowBip[1], -1);
						if(CoVarLowBip[1] == CoVarMidBip[0]) { //Cross over mid
							crs.CrossType = 1;
							crs.CrossTime = Times[CoVarLowBip[1]][0];
						}
						else if(CoVarLowBip[1] == CoVarHighBip[0]) { //Cross over high
							crs.CrossType = 2;
							crs.CrossTime = Times[CoVarLowBip[1]][0];
						}
						DailyCoVarHiLoMidCross.AddCross(crs);
					}
					//Corss below detected
					if(CoVarHighBip[0] != CoVarHighBip[1]
						&& !BarsArray[BarsInProgress].IsFirstBarOfSession) {
						Cross<int> crs = new Cross<int>(CoVarHighBip[1], -1);
						if(CoVarHighBip[1] == CoVarMidBip[0]) { //Cross below mid
							crs.CrossType = -1;
							crs.CrossTime = Times[CoVarHighBip[1]][0];
						}
						else if(CoVarHighBip[1] == CoVarLowBip[0]) { //Cross blow low
							crs.CrossType = -2;
							crs.CrossTime = Times[CoVarHighBip[1]][0];
						}
						DailyCoVarHiLoMidCross.AddCross(crs);
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
			//if(CoVarHiLoSpread[0] < UpperMin[0] && CoVarHiLoSpread[0] >= MiddleBB[0])
			if(CoVarHiLoSpread[0] < UpperBB[0] && CoVarHiLoSpread[0] >= MiddleBB[0])
				isMu = true;
			return isMu;
		}

		public bool IsSpreadMiddleDown() {
			bool isMd = false;
//			if(CoVarHiLoSpread[0] > LowerMin[0] && CoVarHiLoSpread[0] <= MiddleBB[0])
			if(CoVarHiLoSpread[0] > LowerBB[0] && CoVarHiLoSpread[0] <= MiddleBB[0])
				isMd = true;
			return isMd;
		}
		
		public bool IsSpreadUpBand() {
			bool isUb = false;
//			if(CoVarHiLoSpread[0] >= UpperMin[0] && CoVarHiLoSpread[0] < UpperBB[0])
			if(CoVarHiLoSpread[0] >= MiddleBB[0] && CoVarHiLoSpread[0] < UpperBB[0])
				isUb = true;
			return isUb;
		}

		public bool IsSpreadLowBand() {
			bool isLb = false;
//			if(CoVarHiLoSpread[0] <= LowerMin[0] && CoVarHiLoSpread[0] > LowerBB[0])
			if(CoVarHiLoSpread[0] <= MiddleBB[0] && CoVarHiLoSpread[0] > LowerBB[0])
				isLb = true;
			return isLb;
		}
				
		public bool IsSpreadBreakout() {
			bool isBk = false;
			if(CoVarHiLoSpread[0] >= UpperBB[0])
				isBk = true;
			return isBk;
		}
		
		public bool IsSpreadBreakdown() {
			bool isBd = false;
			if(CoVarHiLoSpread[0] <= LowerBB[0])
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
		public Series<double> CoVarSpy
		{
			get { return Values[0]; }
		}

		[Browsable(false), XmlIgnore()]
		public Series<double> CoVarQQQ
		{
			get { return Values[1]; }
		}
		
		[Browsable(false), XmlIgnore()]
		public Series<double> CoVarIWM
		{
			get { return Values[2]; }
		}
		
		/// <summary>
		/// The spread of High CoVar and Low CoVar
		/// </summary>
		[Browsable(false), XmlIgnore()]
		public Series<double> CoVarHiLoSpread
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
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="CoVarPeriod", Description="CoVar period", Order=0, GroupName="Parameters")]
		public int CoVarPeriod
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
		[Display(Name="CoVarScale", Description="Fold for Roc", Order=17, GroupName="Parameters")]
		public int CoVarScale
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
		
		[Browsable(false), XmlIgnore]
		public SMA SmaSpy
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public StdDev StdDevSpy
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public SMA SmaQQQ
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public StdDev StdDevQQQ
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public SMA SmaIWM
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public StdDev StdDevIWM
		{
			get;set;
		}
		
		/// <summary>
		/// The bip of highest Roc
		/// </summary>
		[Browsable(false), XmlIgnore]
		public Series<int> CoVarHighBip
		{
			get;set;
		}
		
		[Browsable(false), XmlIgnore]
		public Series<int> CoVarMidBip
		{
			get;set;
		}

		[Browsable(false), XmlIgnore]
		public Series<int> CoVarLowBip
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
		
		/// <summary>
		/// The daily max/min values of CoVarHiLoSpread
		/// </summary>
		[Browsable(false), XmlIgnore]
		public DailyMaxMin CoVarHiLoSpdMaxMin
		{
			get;set;
		}
		
		/// <summary>
		/// The daily cross from Roc High/Low to Roc Mid
		/// </summary>
		[Browsable(false), XmlIgnore]
		public DailyCross<int> DailyCoVarHiLoMidCross
		{
			get;set;
		}
		
//		/// <summary>
//		/// The time of max daily value of CoVarHiLoSpread
//		/// </summary>
//		[Browsable(false), XmlIgnore]
//		public DateTime RocHiLoSpdMaxTime
//		{
//			get;set;
//		}
		
//		/// <summary>
//		/// The min daily value of CoVarHiLoSpread
//		/// </summary>
//		[Browsable(false), XmlIgnore]
//		public double CoVarHiLoSpreadMin
//		{
//			get;set;
//		}

//		/// <summary>
//		/// The time of min daily value of CoVarHiLoSpread
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
		private GISQRCVSpd[] cacheGISQRCVSpd;
		public GISQRCVSpd GISQRCVSpd(int coVarPeriod, int chartMinutes, string spySymbol, string qQQSymbol, string iWMSymbol, string spyLnSymbol, int spyLnSymbolRatio, string spyStSymbol, int spyStSymbolRatio, string qQQLnSymbol, int qQQLnSymbolRatio, string qQQStSymbol, int qQQStSymbolRatio, string iWMLnSymbol, int iWMLnSymbolRatio, string iWMStSymbol, int iWMStSymbolRatio, int tradeBaseSymbol, int coVarScale, double numStdDevUp, double numStdDevDown, double numStdDevUpMin, double numStdDevDownMin)
		{
			return GISQRCVSpd(Input, coVarPeriod, chartMinutes, spySymbol, qQQSymbol, iWMSymbol, spyLnSymbol, spyLnSymbolRatio, spyStSymbol, spyStSymbolRatio, qQQLnSymbol, qQQLnSymbolRatio, qQQStSymbol, qQQStSymbolRatio, iWMLnSymbol, iWMLnSymbolRatio, iWMStSymbol, iWMStSymbolRatio, tradeBaseSymbol, coVarScale, numStdDevUp, numStdDevDown, numStdDevUpMin, numStdDevDownMin);
		}

		public GISQRCVSpd GISQRCVSpd(ISeries<double> input, int coVarPeriod, int chartMinutes, string spySymbol, string qQQSymbol, string iWMSymbol, string spyLnSymbol, int spyLnSymbolRatio, string spyStSymbol, int spyStSymbolRatio, string qQQLnSymbol, int qQQLnSymbolRatio, string qQQStSymbol, int qQQStSymbolRatio, string iWMLnSymbol, int iWMLnSymbolRatio, string iWMStSymbol, int iWMStSymbolRatio, int tradeBaseSymbol, int coVarScale, double numStdDevUp, double numStdDevDown, double numStdDevUpMin, double numStdDevDownMin)
		{
			if (cacheGISQRCVSpd != null)
				for (int idx = 0; idx < cacheGISQRCVSpd.Length; idx++)
					if (cacheGISQRCVSpd[idx] != null && cacheGISQRCVSpd[idx].CoVarPeriod == coVarPeriod && cacheGISQRCVSpd[idx].ChartMinutes == chartMinutes && cacheGISQRCVSpd[idx].SpySymbol == spySymbol && cacheGISQRCVSpd[idx].QQQSymbol == qQQSymbol && cacheGISQRCVSpd[idx].IWMSymbol == iWMSymbol && cacheGISQRCVSpd[idx].SpyLnSymbol == spyLnSymbol && cacheGISQRCVSpd[idx].SpyLnSymbolRatio == spyLnSymbolRatio && cacheGISQRCVSpd[idx].SpyStSymbol == spyStSymbol && cacheGISQRCVSpd[idx].SpyStSymbolRatio == spyStSymbolRatio && cacheGISQRCVSpd[idx].QQQLnSymbol == qQQLnSymbol && cacheGISQRCVSpd[idx].QQQLnSymbolRatio == qQQLnSymbolRatio && cacheGISQRCVSpd[idx].QQQStSymbol == qQQStSymbol && cacheGISQRCVSpd[idx].QQQStSymbolRatio == qQQStSymbolRatio && cacheGISQRCVSpd[idx].IWMLnSymbol == iWMLnSymbol && cacheGISQRCVSpd[idx].IWMLnSymbolRatio == iWMLnSymbolRatio && cacheGISQRCVSpd[idx].IWMStSymbol == iWMStSymbol && cacheGISQRCVSpd[idx].IWMStSymbolRatio == iWMStSymbolRatio && cacheGISQRCVSpd[idx].TradeBaseSymbol == tradeBaseSymbol && cacheGISQRCVSpd[idx].CoVarScale == coVarScale && cacheGISQRCVSpd[idx].NumStdDevUp == numStdDevUp && cacheGISQRCVSpd[idx].NumStdDevDown == numStdDevDown && cacheGISQRCVSpd[idx].NumStdDevUpMin == numStdDevUpMin && cacheGISQRCVSpd[idx].NumStdDevDownMin == numStdDevDownMin && cacheGISQRCVSpd[idx].EqualsInput(input))
						return cacheGISQRCVSpd[idx];
			return CacheIndicator<GISQRCVSpd>(new GISQRCVSpd(){ CoVarPeriod = coVarPeriod, ChartMinutes = chartMinutes, SpySymbol = spySymbol, QQQSymbol = qQQSymbol, IWMSymbol = iWMSymbol, SpyLnSymbol = spyLnSymbol, SpyLnSymbolRatio = spyLnSymbolRatio, SpyStSymbol = spyStSymbol, SpyStSymbolRatio = spyStSymbolRatio, QQQLnSymbol = qQQLnSymbol, QQQLnSymbolRatio = qQQLnSymbolRatio, QQQStSymbol = qQQStSymbol, QQQStSymbolRatio = qQQStSymbolRatio, IWMLnSymbol = iWMLnSymbol, IWMLnSymbolRatio = iWMLnSymbolRatio, IWMStSymbol = iWMStSymbol, IWMStSymbolRatio = iWMStSymbolRatio, TradeBaseSymbol = tradeBaseSymbol, CoVarScale = coVarScale, NumStdDevUp = numStdDevUp, NumStdDevDown = numStdDevDown, NumStdDevUpMin = numStdDevUpMin, NumStdDevDownMin = numStdDevDownMin }, input, ref cacheGISQRCVSpd);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GISQRCVSpd GISQRCVSpd(int coVarPeriod, int chartMinutes, string spySymbol, string qQQSymbol, string iWMSymbol, string spyLnSymbol, int spyLnSymbolRatio, string spyStSymbol, int spyStSymbolRatio, string qQQLnSymbol, int qQQLnSymbolRatio, string qQQStSymbol, int qQQStSymbolRatio, string iWMLnSymbol, int iWMLnSymbolRatio, string iWMStSymbol, int iWMStSymbolRatio, int tradeBaseSymbol, int coVarScale, double numStdDevUp, double numStdDevDown, double numStdDevUpMin, double numStdDevDownMin)
		{
			return indicator.GISQRCVSpd(Input, coVarPeriod, chartMinutes, spySymbol, qQQSymbol, iWMSymbol, spyLnSymbol, spyLnSymbolRatio, spyStSymbol, spyStSymbolRatio, qQQLnSymbol, qQQLnSymbolRatio, qQQStSymbol, qQQStSymbolRatio, iWMLnSymbol, iWMLnSymbolRatio, iWMStSymbol, iWMStSymbolRatio, tradeBaseSymbol, coVarScale, numStdDevUp, numStdDevDown, numStdDevUpMin, numStdDevDownMin);
		}

		public Indicators.GISQRCVSpd GISQRCVSpd(ISeries<double> input , int coVarPeriod, int chartMinutes, string spySymbol, string qQQSymbol, string iWMSymbol, string spyLnSymbol, int spyLnSymbolRatio, string spyStSymbol, int spyStSymbolRatio, string qQQLnSymbol, int qQQLnSymbolRatio, string qQQStSymbol, int qQQStSymbolRatio, string iWMLnSymbol, int iWMLnSymbolRatio, string iWMStSymbol, int iWMStSymbolRatio, int tradeBaseSymbol, int coVarScale, double numStdDevUp, double numStdDevDown, double numStdDevUpMin, double numStdDevDownMin)
		{
			return indicator.GISQRCVSpd(input, coVarPeriod, chartMinutes, spySymbol, qQQSymbol, iWMSymbol, spyLnSymbol, spyLnSymbolRatio, spyStSymbol, spyStSymbolRatio, qQQLnSymbol, qQQLnSymbolRatio, qQQStSymbol, qQQStSymbolRatio, iWMLnSymbol, iWMLnSymbolRatio, iWMStSymbol, iWMStSymbolRatio, tradeBaseSymbol, coVarScale, numStdDevUp, numStdDevDown, numStdDevUpMin, numStdDevDownMin);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GISQRCVSpd GISQRCVSpd(int coVarPeriod, int chartMinutes, string spySymbol, string qQQSymbol, string iWMSymbol, string spyLnSymbol, int spyLnSymbolRatio, string spyStSymbol, int spyStSymbolRatio, string qQQLnSymbol, int qQQLnSymbolRatio, string qQQStSymbol, int qQQStSymbolRatio, string iWMLnSymbol, int iWMLnSymbolRatio, string iWMStSymbol, int iWMStSymbolRatio, int tradeBaseSymbol, int coVarScale, double numStdDevUp, double numStdDevDown, double numStdDevUpMin, double numStdDevDownMin)
		{
			return indicator.GISQRCVSpd(Input, coVarPeriod, chartMinutes, spySymbol, qQQSymbol, iWMSymbol, spyLnSymbol, spyLnSymbolRatio, spyStSymbol, spyStSymbolRatio, qQQLnSymbol, qQQLnSymbolRatio, qQQStSymbol, qQQStSymbolRatio, iWMLnSymbol, iWMLnSymbolRatio, iWMStSymbol, iWMStSymbolRatio, tradeBaseSymbol, coVarScale, numStdDevUp, numStdDevDown, numStdDevUpMin, numStdDevDownMin);
		}

		public Indicators.GISQRCVSpd GISQRCVSpd(ISeries<double> input , int coVarPeriod, int chartMinutes, string spySymbol, string qQQSymbol, string iWMSymbol, string spyLnSymbol, int spyLnSymbolRatio, string spyStSymbol, int spyStSymbolRatio, string qQQLnSymbol, int qQQLnSymbolRatio, string qQQStSymbol, int qQQStSymbolRatio, string iWMLnSymbol, int iWMLnSymbolRatio, string iWMStSymbol, int iWMStSymbolRatio, int tradeBaseSymbol, int coVarScale, double numStdDevUp, double numStdDevDown, double numStdDevUpMin, double numStdDevDownMin)
		{
			return indicator.GISQRCVSpd(input, coVarPeriod, chartMinutes, spySymbol, qQQSymbol, iWMSymbol, spyLnSymbol, spyLnSymbolRatio, spyStSymbol, spyStSymbolRatio, qQQLnSymbol, qQQLnSymbolRatio, qQQStSymbol, qQQStSymbolRatio, iWMLnSymbol, iWMLnSymbolRatio, iWMStSymbol, iWMStSymbolRatio, tradeBaseSymbol, coVarScale, numStdDevUp, numStdDevDown, numStdDevUpMin, numStdDevDownMin);
		}
	}
}

#endregion
