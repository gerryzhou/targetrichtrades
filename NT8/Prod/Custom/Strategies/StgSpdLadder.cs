//
// Copyright (C) 2019, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.AddOns;
using NinjaTrader.NinjaScript.AddOns.PriceActions;
using NinjaTrader.NinjaScript.AddOns.MarketCtx;
using NinjaTrader.NinjaScript.Strategies;
#endregion

//This namespace holds strategies in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Strategies
{
	/// <summary>
	/// Trade by the spread: diff or ratio between the two symbols
	/// Factors impact the performance:
	/// Price, volatility, slippage
	/// MA(90), ATR(20); Slippage=0.04% Price or MA
	/// </summary>
	public class StgSpdLadder : GStrategyBase
	{
		//private GIPctSpd giPctSpd;
		private GISpdLadder giSpdLadder;
		
		//private Dictionary<string, List<CtxPairSpd>> ctxPairSpd;		
		private CtxPairSpdDaily ctxPairSpdDaily;
//		public StgPairSimple () {
//			VendorLicense("TheTradingBook", "StgPairSimple", "thetradingbook.com", "support@tradingbook.com",null);
//		}
		
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				Description	= "Pair Trading by Spread Ladders";
				Name		= "StgSpdLadder";
				//Account.Name								= "Sim102";
				SlippageRate								= 0.04; //percent of the price/100
				Calculate									= Calculate.OnPriceChange;
				IsFillLimitOnTouch							= false;
				TraceOrders									= false;
				BarsRequiredToTrade							= 12;
				IsUnmanaged									= false;
				IncludeCommission							= true;
				OrderFillResolution							= OrderFillResolution.Standard;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				DefaultQuantity								= 100;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				IsExitOnSessionCloseStrategy				= false;
				//ExitOnSessionCloseSeconds					= 30;
//				MM_ProfitFactorMax							= 1;
//				MM_ProfitFactorMin							= 0;
//				TG_TradeEndH								= 10;
//				TG_TradeEndM								= 45;
				TG_OpenStartH								= 8;
				TG_OpenStartM								= 30;
				PeriodNear									= 10;
				PeriodMiddle								= 30;
				PeriodFar									= 90;
				MktPosition1								= MarketPosition.Flat;
				MktPosition2								= MarketPosition.Flat;
				SPKLineThresholdLow							= 3;
				SPKLineThresholdMid							= 50;
				SPKLineThresholdHigh						= 96;
				TradeCostRate								= 0.03;
				DaysToHoldPos								= 9;
				PrintOut									= 1;
//				IsInstantiatedOnEachOptimizationIteration = false;
			}
			else if (State == State.Configure)
			{				
				// Add an MSFT 1 minute Bars object to the strategy
				//AddDataSeries("NQ 06-20", Data.BarsPeriodType.Minute, 13);				
				if(ChartMinutes > 0)
					AddDataSeries(SecondSymbol, BarsPeriodType.Minute, ChartMinutes, MarketDataType.Last);
				else 
					AddDataSeries(SecondSymbol, BarsPeriodType.Day, 1, MarketDataType.Last);
				//AddDataSeries(SecondSymbol, BarsPeriodType.Minute, ChartMinutes);
				SetOrderQuantity = SetOrderQuantity.Strategy; // calculate orders based off default size
				// Sets a 20 tick trailing stop for an open position
				//SetTrailStop(CalculationMode.Ticks, 200);
			}
			else if (State == State.DataLoaded)
			{				
				giSpdLadder = GISpdLadder(SecondSymbol, ChartMinutes, PeriodNear, PeriodMiddle, PeriodFar, SPKLineThresholdLow, SPKLineThresholdMid, SPKLineThresholdHigh, 1);// CapRatio1, CapRatio2, PctChgSpdThresholdEn, PctChgSpdThresholdEx);
				AddChartIndicator(giSpdLadder);
				
				giSpdLadder.RaiseIndicatorEvent += OnTradeBySpdLadder;
//				giPctSpd.RaiseIndicatorEvent += OnTradeByPctSpd;
//				giPctSpd.TM_ClosingH = TG_TradeEndH;
//				giPctSpd.TM_ClosingM = TG_TradeEndM;
				SetPrintOut(-1);
				CapRatio2 = 1;
				CapRatio1 = Closes[1][0]/Closes[0][0];
				Print(String.Format("{0}: IsUnmanaged={1}", this.GetType().Name, IsUnmanaged));
				Print(String.Format("{0}: DataLoaded...BarsArray.Length={1}", this.GetType().Name, BarsArray.Length));		
				
				ctxPairSpdDaily = giSpdLadder.CtxPairSpreadDaily;
//				if(BarsPeriod.BarsPeriodType == BarsPeriodType.Day) {
//					//SetMarketContext();
//					ctxPairSpd = new Dictionary<string, List<CtxPairSpd>>();
//				}
				if(BarsPeriod.BarsPeriodType != BarsPeriodType.Day)
					GetMarketContext();
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBars[0] < BarsRequiredToTrade || CurrentBars[1] < BarsRequiredToTrade)
				return;
			giSpdLadder.Update();
//			Print(string.Format("CurrentBars[BarsInProgress]={0}, BarsInProgress={1}, BarsPeriod.BarsPeriodType={2}", 
//				CurrentBars[BarsInProgress], BarsInProgress, BarsPeriod.BarsPeriodType));
//			if(BarsPeriod.BarsPeriodType == BarsPeriodType.Day && BarsInProgress > 0){				
//				if(giSpdRs.IsLastBarOnChart(BarsInProgress) > 0) {
//					WriteCtxParaObj();
//				} else {
//					giSpdRs.SetPairSpdCtx();
//				}
//			} else {
//				GetPairSpdCtx();
//			}
			
//			if(BarsPeriod.BarsPeriodType != BarsPeriodType.Day)
//				GetPairSpdCtx();
		}
		
		#region En/Ex signals
		private bool CheckPnLByBarsSinceEn() { return false;
//			bool call_ex = false;
//			int bse = BarsSinceEntryExecution(giPctSpd.PctChgMaxBip, String.Empty, 0);
//			double ur_pnl = CheckUnrealizedPnLBip(giPctSpd.PctChgMaxBip);
//			if(bse == TM_BarsToCheckPnL && ur_pnl < 0)
//				call_ex = true;
//			return call_ex;
		}
		#endregion
		
		#region Indicator Event Handler
        // Define what actions to take when the event is raised. 

        void OnTradeBySpdLadder(object sender, IndicatorEventArgs e) {
			IndicatorSignal isig = e.IndSignal;
			if(this.PrintOut > 1)
				Print(String.Format("{0}:OnTradeBySpdLadder triggerred {1} Bip{2}: Spread={3}, HiAllTime={4}",
				CurrentBars[BarsInProgress], isig.SignalName, BarsInProgress, giSpdLadder.Spread[0], giSpdLadder.HiAllTime));
			if(e.IndSignal.SignalName != null && HasPairPosition())
				OnExitPositions(e);
			else if(e.IndSignal.SignalName != null && IsTradingTime(IndicatorProxy.GetTimeByHM(TG_OpenStartH, TG_OpenStartM, true)))
				OnEntryPositions(e);
		}
				
		void OnExitPositions(IndicatorEventArgs e) {
			int quant1 = DefaultQuantity;//base.GetTradeQuantity(0, CapRatio1);
			int quant2 = DefaultQuantity;//base.GetTradeQuantity(1, CapRatio2);
			string sig = e.IndSignal.SignalName;
			if(this.PrintOut > 1)
				Print(String.Format("{0}:OnExitPositions quant1={1}, quant2={2}: Spread={3}, HiAllTime={4}, HiNear[0]={5}",
				CurrentBars[BarsInProgress], quant1, quant2, giSpdLadder.Spread[0], giSpdLadder.HiAllTime, giSpdLadder.HiNear[0]));
			if(!IsInStrategyAnalyzer)
				Print(string.Format("{0}: EntryLongLeg1={1}, EntrySTLeg1={2}, En1={3}, EntryLongLeg2={4}, EntrySTLeg2={5}, En2={6}", CurrentBars[BarsInProgress],
				BarsSinceEntryExecution(0, giSpdLadder.SignalName_EntryLongLeg1, 0),
				BarsSinceEntryExecution(0, giSpdLadder.SignalName_EntryShortLeg1, 0),
				BarsSinceEntryExecution(0, "", 0),
				BarsSinceEntryExecution(1, giSpdLadder.SignalName_EntryLongLeg2, 0),
				BarsSinceEntryExecution(1, giSpdLadder.SignalName_EntryShortLeg2, 0),
				BarsSinceEntryExecution(1, "", 0)));
			if(BarsSinceEntryExecution(0, "", 0) < DaysToHoldPos &&				
				BarsSinceEntryExecution(1, "", 0) < DaysToHoldPos) {
				int quant = quant1 + quant2;
				double pnl = GetPairUnrealizedPnL(PerformanceUnit.Currency);
//				double pnl0 = Positions[0].GetUnrealizedProfitLoss(PerformanceUnit.Currency, Closes[0][0]);
//				double pnl1 = Positions[1].GetUnrealizedProfitLoss(PerformanceUnit.Currency, Closes[1][0]);
				if(this.PrintOut > 1)
					Print(String.Format("{0}:OnExitPositions Performance bip={1} quant1={2}, quant2={3}: UnrealizedPnL={4}",
					CurrentBars[BarsInProgress], BarsInProgress, quant1, quant2, pnl));
				if(pnl < TradeCostRate*quant)
					return;
			}
			//Exit positions for both legs
//			if(sig == giSpdLadder.SignalName_AboveStdDev || sig == giSpdLadder.SignalName_AboveStdDevMin) {
				if(GetMarketPosition(0) == MarketPosition.Long) 
					ExitLong(0, quant1, "", giSpdLadder.SignalName_EntryLongLeg1);
				if(GetMarketPosition(1) == MarketPosition.Short) {								
					ExitShort(1, quant2, "", giSpdLadder.SignalName_EntryShortLeg2);
				}
//			}
//			else if(sig == giSpdLadder.SignalName_BelowStdDev || sig == giSpdLadder.SignalName_BelowStdDevMin) {
				if(GetMarketPosition(1) == MarketPosition.Long) 
					ExitLong(1, quant2, "", giSpdLadder.SignalName_EntryLongLeg2);
				if(GetMarketPosition(0) == MarketPosition.Short){					
					ExitShort(0, quant1, "", giSpdLadder.SignalName_EntryShortLeg1);
				}
//			}			
		}
		
		void OnEntryPositions(IndicatorEventArgs e) {
			if(this.PrintOut > 1)
				Print(string.Format("OnEntryPositions CurrentBars={0}, BarsInProgress={1}, Times[][0]={2}",
				CurrentBars[BarsInProgress], BarsInProgress, Times[BarsInProgress][0]));
			//New entry with no poistions for both legs
			if(HasPosition(0) <= 0 && HasPosition(1) <= 0) {
//				string key = ctxPairSpdDaily.KeyLastDay;//giSpdRs.GetDateStrByDateTime(Times[0][0]); //
//				if(key == null) {
//					Print(string.Format("ctxPairSpdDaily.KeyLastDay CurrentBars={0}, BarsInProgress={1}, Times[][0]={2}",
//					CurrentBars[BarsInProgress], BarsInProgress, Times[BarsInProgress][0]));
//					return;
//				}
//				CtxPairSpd ctxps = ctxPairSpdDaily.GetDayCtx(key);
//				if(ctxps == null) return;
//				else {
					int quant1 = DefaultQuantity;//base.GetTradeQuantity(0, CapRatio1);
					int quant2 = DefaultQuantity;//base.GetTradeQuantity(1, CapRatio2);
					Slippage = Math.Round(SlippageRate*Closes[0][0], 4);
					string sig = e.IndSignal.SignalName;
					if(this.PrintOut > 1)
						Print(String.Format("{0}:OnEntryPositions quant1={1}, quant2={2}: Spread={3}, HiNear[0]={4}, LowNear[0]={5}",
						CurrentBars[BarsInProgress], quant1, quant2, giSpdLadder.Spread[0], giSpdLadder.HiNear[0], giSpdLadder.LowNear[0]));
					if(sig == giSpdLadder.SignalName_AboveStdDev) {
//						if(MktPosition1 != MarketPosition.Long && MktPosition2 != MarketPosition.Short) {
							EnterShort(0, quant1, giSpdLadder.SignalName_EntryShortLeg1);
							EnterLong(1, quant2, giSpdLadder.SignalName_EntryLongLeg2);
//						}
					}
					else if (//TrendDirection.Up.ToString().Equals(ctxps.TrendDirection) && PositionInBand.Upper.ToString().Equals(ctxps.PositionInBand) 
						sig == giSpdLadder.SignalName_BelowStdDev) {
//						if(MktPosition1 != MarketPosition.Short && MktPosition2 != MarketPosition.Long) {
							EnterLong(0, quant1, giSpdLadder.SignalName_EntryLongLeg1);
							EnterShort(1, quant2, giSpdLadder.SignalName_EntryShortLeg2);
//						}
					}					
//				}

				
//				if(MktPosition1 == MarketPosition.Long) {
//					EnterLong(0, quant1, giSpdRs.SignalName_EntryLongLeg1);
//				} else if(MktPosition1 == MarketPosition.Short) {
//					EnterShort(0, quant1, giSpdRs.SignalName_EntryShortLeg1);
//				}
				
//				if(MktPosition2 == MarketPosition.Long) {
//					EnterLong(1, quant2, giSpdRs.SignalName_EntryLongLeg2);
//				} else if(MktPosition2 == MarketPosition.Short) {
//					EnterShort(1, quant2, giSpdRs.SignalName_EntryShortLeg2);
//				}
			}
		}

		public override bool IsTradingTime(int session_start) {
			//Bars.Session.GetNextBeginEnd(DateTime time, out DateTime sessionBegin, out DateTime sessionEnd)
			int time_start = IndicatorProxy.GetTimeByHM(TG_TradeStartH, TG_TradeStartM, true);
			int time_end = IndicatorProxy.GetTimeByHM(TG_TradeEndH, TG_TradeEndM, true);
			int time_now = ToTime(Time[0]);
			bool isTime= (BarsPeriod.BarsPeriodType == BarsPeriodType.Day)? true : false;
			if (time_now >= session_start && time_now >= time_start && time_now <= time_end) {
				isTime = true;
			}
//			Print(String.Format("{0}: time_now={1}, session_start={2}, time_start={3}, time_end={4}, isTime={5}",
//			CurrentBar, time_now, session_start, time_start, time_end, isTime));
			return isTime;
		}
		#endregion

		/// <summary>
		/// CapRatio: ES:RTY=1.7:1, NQ:RTY=2.1:1, NQ:ES=1.25:1
		/// </summary>		
//		public override int GetTradeQuantity(int idx, double ratio) {
//			switch(idx) {
//				case 0: return base.GetTradeQuantity(idx, CapRatio1); // ratio>0? (int)ratio*5 : 5;
//				case 1: return base.GetTradeQuantity(idx, CapRatio2); //ratio>0? (int)ratio*4 :4;
//				default: return -1;
//			}
//		}

		/// <summary>
		/// Load ctx from Json file
		/// </summary>
		/// <returns></returns>
		public override void ReadCtxParaObj() {
			//ReadRestfulJson();
			ctxPairSpdDaily.DictCtxPairSpd = GConfig.LoadJson2Obj<Dictionary<string, List<CtxPairSpd>>>(GConfig.GetCTXFilePath());
			//Dictionary<string, object> paraDict = GConfig.LoadJson2Dictionary(GetCTXFilePath());
			//Dictionary<string, object> paraDict = GConfig.LoadJson2Obj<Dictionary<string, object>>(GetCTXFilePath());
			if(this.PrintOut > 1)
				Print(String.Format("ReadCtxPairSpd paraDict={0}, paraDict.Count={1}", ctxPairSpdDaily.DictCtxPairSpd, ctxPairSpdDaily.DictCtxPairSpd.Count));
			//if(paraDict != null && paraDict.Count > 0) {
				//this.ctxPairSpd = paraDict[0];
				//GUtils.DisplayProperties<JsonStgPairSpd>(ctxPairSpd, IndicatorProxy);
			//}
//			foreach(var ele in ctxPairSpdDaily.DictCtxPairSpd)
//			{
				//Print(string.Format("DateCtx.ele.Key={0}, ele.Value.ToString()={1}", ele.Key, ele.Value));
//				foreach(CtxPairSpd ctxPS in ele.Value) {
//					Print(string.Format("ctxPS.Symbol={0}, ctxPS.TimeClose={1}", ctxPS.Symbol, ctxPS.TimeClose));
//				}
//			}
			//foreach(JsonStgPairSpd ele in ctxPairSpd) {
				//GUtils.DisplayProperties<JsonStgPairSpd>(ele, IndicatorProxy);
				//Print(string.Format("DateCtx.ele.Key={0}, ele.Value.ToString()={1}", ele.Date, ele.CtxDaily));
//				if(ele != null && ele.Date != null && ele.TimeCtxs != null) {
//					Print(String.Format("DateCtx.ele.Key={0}, ele.Value.ToString()={1}", ele.Date, ele.TimeCtxs));
//					foreach(TimeCtx tctx in ele.TimeCtxs) {
//						Print(String.Format("ele.Date={0}, TimeCtx.tctx.Time={1}, tctx.ChannelType={2}, tctx.MinUp={3}, tctx.Support={4}",
//						ele.Date, tctx.Time, tctx.ChannelType, tctx.MinUp, tctx.Support));
//					}
//				}				
			//}
//			foreach(KeyValuePair<string, List<TimeCTX>> ele in paraDict.cmdMarketContext.ctx_daily.ctx) {
//				//paraMap.Add(ele.Key, ele.Value.ToString());
//				Print(String.Format("ele.Key={0}, ele.Value.ToString()=", ele.Key));
//			}
		}		
		
		private void GetPairSpdCtx() {
			//Is a new day
			if(Times[0][0].Day != Times[0][1].Day) {
				this.ctxPairSpdDaily.KeyLastDay = giSpdLadder.GetDateStrByDateTime(Times[0][1]);
				CtxPairSpd ctxps = this.ctxPairSpdDaily.GetDayCtx(this.ctxPairSpdDaily.KeyLastDay);
				CapRatio2 = 1;
				CapRatio1 = ctxps==null? Closes[1][0]/Closes[0][0] : ctxps.PairATRRatio;
			}
		}
		
		#region Properties	
		[NinjaScriptProperty]		
		[Display(Name="SecondSymbol", Description="The second symbol of the pair", Order=0, GroupName=GPS_CUSTOM_PARAMS)]
		public string SecondSymbol
		{ 	get{
				return secondSymbol;
			}
			set{
				secondSymbol = value;
			}
		}
		
		[NinjaScriptProperty]
		[Range(-1, int.MaxValue)]
		[Display(Name="ChartMinutes", Description="Minutes for the chart", Order=1, GroupName=GPS_CUSTOM_PARAMS)]
		public int ChartMinutes
		{ 	get{
				return chartMinutes;
			}
			set{
				chartMinutes = value;
			}
		}

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PeriodNear", Description="Period Near", Order=2, GroupName=GPS_CUSTOM_PARAMS)]
		public int PeriodNear
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PeriodMiddle", Description="Period Middle", Order=3, GroupName=GPS_CUSTOM_PARAMS)]
		public int PeriodMiddle
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PeriodFar", Description="Period Far", Order=4, GroupName=GPS_CUSTOM_PARAMS)]
		public int PeriodFar
		{ get; set; }

		[Range(0, 100), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SPKLineThresholdLow", Order=5, GroupName = GPS_CUSTOM_PARAMS)]
		public double SPKLineThresholdLow
		{ get; set; }

		[Range(0, 100), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SPKLineThresholdMid", Order=6, GroupName = GPS_CUSTOM_PARAMS)]
		public double SPKLineThresholdMid
		{ get; set; }
		
		[Range(0, 100), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SPKLineThresholdHigh", Order=7, GroupName = GPS_CUSTOM_PARAMS)]
		public double SPKLineThresholdHigh
		{ get; set; }

		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TradeCostRate", Order=8, GroupName = GPS_CUSTOM_PARAMS)]
		public double TradeCostRate
		{ get; set; }
		
		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "DaysToHoldPos", Order=9, GroupName = GPS_CUSTOM_PARAMS)]
		public int DaysToHoldPos
		{ get; set; }

		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SlippageRate", Order=10, GroupName = GPS_CUSTOM_PARAMS)]
		public double SlippageRate
		{ get; set; }
		
		[NinjaScriptProperty]
		//[Range(1, double.MaxValue)]
		[Display(Name="MktPosition1", Description="Long or short for the leg1 entry", Order=11, GroupName=GPS_CUSTOM_PARAMS)]
		public MarketPosition MktPosition1
		{ 	get; set;
		}
		
		[NinjaScriptProperty]
		//[Range(1, double.MaxValue)]
		[Display(Name="MktPosition2", Description="Long or short for the leg2 entry", Order=12, GroupName=GPS_CUSTOM_PARAMS)]
		public MarketPosition MktPosition2
		{ 	get; set;
		}
		
		[NinjaScriptProperty]
		[Range(0.001, double.MaxValue)]
		[Display(Name="CapRatio1", Description="CapRatio of first leg", Order=13, GroupName=GPS_CUSTOM_PARAMS)]
		public double CapRatio1
		{ 	get{
				return capRatio1;
			}
			set{
				capRatio1 = value;
			}
		}
		
		[NinjaScriptProperty]
		[Range(0.001, double.MaxValue)]
		[Display(Name="CapRatio2", Description="CapRatio of 2nd leg", Order=14, GroupName=GPS_CUSTOM_PARAMS)]
		public double CapRatio2
		{ 	get{
				return capRatio2;
			}
			set{
				capRatio2 = value;
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

		#endregion
		
		#region Pre Defined parameters
		private double capRatio1 = 1;//1.25;
		private double capRatio2 = 1;
		private string secondSymbol = "CWENA";
		private int chartMinutes = 0;
		#endregion
	}
}
