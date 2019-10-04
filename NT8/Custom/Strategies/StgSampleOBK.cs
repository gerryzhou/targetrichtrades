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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.AddOns;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Indicators.PriceActions;
using NinjaTrader.NinjaScript.Indicators.ZTraderPattern;
using NinjaTrader.NinjaScript.Strategies.ZTraderStg;

#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	/// <summary>
	/// Sample strategy fro GStrategy:
	/// 1) OnStateChange();
	/// 2) OnBarUpdate();
	/// 3) GetIndicatorSignal();
	/// 4) GetTradeSignal();
	/// 5) CheckNewEntryTrade();
	/// 6) PutTrade();
	/// Indicator Combination:
	/// * SnR: daily high/low
	/// * Breakout: morning breakout of the SnR, big bar cross the SnR
	/// * Reversal Pivot: 9:00-11 AM morning session high/low
	/// * Pullback Pivot: left 20+, right 5+, i.e. (20+, 5+)
	/// * Trending pivot: breakout the pullback pivot, create a new (5+, 5+) pivot
	/// </summary>
	
	/// Open Breakout Trade
	/// OBK-1) Engulfing the largest bar after opening, which breakout the opening range and failed;
	/// OBK-2) Failed breakout: the day expected as range day, breakout beyond the opening range will pullback
	/// 						into the range, that may test the other side of the range;
	/// 						the extension to the breakout side may goes as far as 2x the range size;
	///
	public class StgSampleOBK : GStrategyBase
	{
		private GISMI giSMI;
		private GIAwesomeOscillator awOscillator;
		private GISnR giSnR;
		
		private double c0 = 0, hi3 = Double.MaxValue, lo3 = Double.MinValue;
		
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				Print(this.Name + " set defaults called....");
				Description									= @"The sample strategy for GSZTrader.";
				Name										= "StgSampleOBK";
				Calculate									= Calculate.OnBarClose;
				IsFillLimitOnTouch							= false;
				TraceOrders									= false;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
			}
			else if (State == State.DataLoaded)
			{
				Print(this.Name + " set DataLoaded called....");				
				AddChartIndicator(indicatorProxy);
				SetPrintOut(1);
				indicatorProxy.LoadSpvPRList(SpvDailyPatternES.spvPRDayES);
				indicatorProxy.AddPriceActionTypeAllowed(PriceActionType.DnWide);
				
				giSMI = GISMI(EMAPeriod1, EMAPeriod2, Range, SMITMAPeriod, SMICrossLevel);//(3, 5, 5, 8);
				awOscillator = GIAwesomeOscillator(FastPeriod, SlowPeriod, Smooth, MovingAvgType.SMA, false);//(5, 34, 5, MovingAvgType.SMA);
				
				AddChartIndicator(giSMI);
				AddChartIndicator(awOscillator);
				Print("GISMI called:" + "EMAPeriod1=" + EMAPeriod1 + "EMAPeriod2=" + EMAPeriod2 + "Range=" + Range + "SMITMAPeriod=" + SMITMAPeriod);
			}
			else if (State == State.Configure)
			{
				Print(this.Name + " set Configure called....");
				tradeObj = new TradeObj(this);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		protected override void OnBarUpdate()
		{
			try {
			base.OnBarUpdate();
			indicatorProxy.TraceMessage(this.Name, PrintOut);
			} catch (Exception ex) {
				indicatorProxy.Log2Disk = true;
				indicatorProxy.PrintLog(true, true, "Exception: " + ex.StackTrace);
			}
		}
		
		public override void SetIndicatorSignals(){
			giSMI.Update();
			Print(CurrentBar + ":SetIndicatorSignals called ---------------------");
			
			IndicatorSignal indSig = giSMI.GetLastIndicatorSignalByName(CurrentBar, giSMI.SignalName_Inflection);
			
			if(indSig != null && indSig.Signal_Action != null)
				Print(CurrentBar + ":stg-Last " + giSMI.SignalName_Inflection + "=" + indSig.BarNo + "," + indSig.Signal_Action.SignalActionType.ToString());

			IndicatorSignal indSigCrs = giSMI.GetLastIndicatorSignalByName(CurrentBar, giSMI.SignalName_LineCross);
			
			if(indSigCrs != null && indSigCrs.Signal_Action != null)
				Print(CurrentBar + ":stg-Last " + giSMI.SignalName_LineCross + "=" + indSigCrs.BarNo + "," + indSigCrs.Signal_Action.SignalActionType.ToString());
			
		}
		
		public override TradeSignal GetTradeSignal() {
			indicatorProxy.TraceMessage(this.Name, PrintOut);
			TradeSignal trdSignal = new TradeSignal();
			Direction dir = giSMI.GetDirection();// new Direction();
			PatternMatched();
			c0 = Close[0];
			
			Print(CurrentBar + ":"
			+ ";c0=" + c0
			+ ";hi3=" + hi3
			+ ";lo3=" + lo3
			+ ";BarsLookback=" + BarsLookback);
			
//			if(c0 > hi3)
//				dir.TrendDir = TrendDirection.Up;

//			if(c0 < lo3)
//				dir.TrendDir = TrendDirection.Down;
			trdSignal.TrendDir = dir;
			
			this.tradeSignal = trdSignal;
			hi3 = GetHighestPrice(BarsLookback);
			lo3 = GetLowestPrice(BarsLookback);
			
			return trdSignal;
		}
		
				
		protected override bool PatternMatched()
		{
			//Print("CurrentBar, barsMaxLastCross, barsAgoMaxPbSAREn,=" + CurrentBar + "," + barsAgoMaxPbSAREn + "," + barsSinceLastCross);
//			if (giParabSAR.IsSpvAllowed4PAT(curBarPriceAction.paType) && barsSinceLastCross < barsAgoMaxPbSAREn) 
//				return true;
//			else return false;
			PriceAction pa = indicatorProxy.GetPriceAction(Time[0]);
			indicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":"
				+ ";ToShortDateString=" + Time[0].ToString()
				+ ";paType=" + pa.paType.ToString()
				+ ";maxDownTicks=" + pa.maxDownTicks
				);
			return false;
			//barsAgoMaxPbSAREn Bars Since PbSAR reversal. Enter the amount of the bars ago maximum for PbSAR entry allowed
		}
		
		public override TradeObj CheckNewEntryTrade() {
			indicatorProxy.TraceMessage(this.Name, PrintOut);
			tradeObj.InitNewEntryTrade();
			if(tradeSignal != null) {
				if(tradeSignal.TrendDir.TrendDir == TrendDirection.Down)
				{
					indicatorProxy.TraceMessage(this.Name, PrintOut);
					tradeObj.tradeDirection = TradingDirection.Down;
				}
				else if(tradeSignal.TrendDir.TrendDir == TrendDirection.Up)
				{
					indicatorProxy.TraceMessage(this.Name, PrintOut);
					tradeObj.tradeDirection = TradingDirection.Up;
				}
				
				tradeObj.tradeStyle = TradingStyle.TrendFollowing;
				
			} else {
				tradeObj.SetTradeType(TradeType.NoTrade);
			}
			return tradeObj;
		}
		
		public override void PutTrade(){
			indicatorProxy.TraceMessage(this.Name, PrintOut);
			if(tradeObj.GetTradeType() == TradeType.Entry) {
				indicatorProxy.PrintLog(true, IsLiveTrading(), "PutTrade tradeObj.stopLossAmt=" + tradeObj.stopLossAmt + "," + MM_StopLossAmt);
				if(tradeObj.tradeDirection == TradingDirection.Down) {
					indicatorProxy.PrintLog(true, IsLiveTrading(), "PutTrade Down OrderSignalName=" + tradeObj.entrySignalName);
					tradeObj.enLimitPrice = GetTypicalPrice(0);
					NewShortLimitOrderUM(OrderSignalName.EntryShortLmt.ToString());
				}
				else if(tradeObj.tradeDirection == TradingDirection.Up) {
					indicatorProxy.PrintLog(true, IsLiveTrading(), "PutTrade Up OrderSignalName=" + tradeObj.entrySignalName);
					tradeObj.enLimitPrice = GetTypicalPrice(0);
					NewLongLimitOrderUM(OrderSignalName.EntryLongLmt.ToString());
				}
			}
		}

        #region Custom Properties
		
        [Description("Bars count before inflection for entry")]
 		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EnBarsBeforeInflection", GroupName = GPS_CUSTOM_PARAMS, Order = 0)]
        public int CP_EnBarsBeforeInflection
        {
            get { return cp_EnBarsBeforeInflection; }
            set { cp_EnBarsBeforeInflection = Math.Max(1, value); }
        }
		
		[Description("Bars lookback period")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarsLookback", GroupName = GPS_CUSTOM_PARAMS, Order = 1)]
        public int BarsLookback
        {
            get { return barsLookback; }
            set { barsLookback = Math.Max(1, value); }
        }

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="EMAPeriod1(SMI)", Description="1st ema smothing period. ( R )", Order=2, GroupName=GPS_CUSTOM_PARAMS)]
		public int EMAPeriod1
		{
			get { return emaperiod1;}
			set { emaperiod1 = Math.Max(1, value);}
		}

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="EMAPeriod2(SMI)", Description="2nd ema smoothing period. ( S )", Order=3, GroupName=GPS_CUSTOM_PARAMS)]
		public int EMAPeriod2
		{
			get { return emaperiod2;}
			set { emaperiod2 = Math.Max(1, value);}
		}
		
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Range(SMI)", Description="Range for momentum Calculation ( Q )", Order=4, GroupName=GPS_CUSTOM_PARAMS)]
		public int Range
		{
			get { return range;}
			set { range = Math.Max(1, value);}
		}		

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="SMITMAPeriod", Description="SMI TMA smoothing period", Order=5, GroupName=GPS_CUSTOM_PARAMS)]
		public int SMITMAPeriod
		{
			get { return smitmaperiod;}
			set { smitmaperiod = Math.Max(1, value);}
		}

		[Range(0, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="SMICrossLevel", Description="SMI&TMA Cross Level", Order=6, GroupName=GPS_CUSTOM_PARAMS)]
		public int SMICrossLevel
		{
			get { return smiCrossLevel;}
			set { smiCrossLevel = Math.Max(0, value);}
		}
		
		/// <summary>
		/// </summary>
		//[Description("Period for fast EMA")]
		//[Category("Parameters")]
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "FastPeriod(AO)", GroupName = GPS_CUSTOM_PARAMS, Order = 7)]		
		public int FastPeriod
		{
			//get;set;
			get { return fastPeriod; }
			set { fastPeriod = Math.Max(1, value); }
		}

		/// <summary>
		/// </summary>
		//[Description("Period for slow EMA")]
		//[Category("Parameters")]
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SlowPeriod(AO)", GroupName = GPS_CUSTOM_PARAMS, Order = 8)]
		public int SlowPeriod
		{
			get { return slowPeriod; }
			set { slowPeriod = Math.Max(1, value); }
		}

		/// <summary>
		/// </summary>
//		[Description("Period for Smoothing of Signal Line")]
//		[Category("Parameters")]
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Smooth(AO)", GroupName = GPS_CUSTOM_PARAMS, Order = 9)]		
		public int Smooth
		{
			get { return smooth; }
			set { smooth = Math.Max(1, value); }
		}

		
		private int cp_EnBarsBeforeInflection = 2;
				
		private int barsLookback = 1;// 15;
		
		//SMI parameters
		private int	range		= 5;
		private int	emaperiod1	= 3;
		private int	emaperiod2	= 5;
		private int smitmaperiod= 8;
		private int tmaperiod= 6;
		private int smiCrossLevel = 50;
		
		//AWO parameters
		private int fastPeriod 			= 5;
        private int slowPeriod 			= 34;
		private int smooth		 		= 5;
		
		#endregion
	}
}
