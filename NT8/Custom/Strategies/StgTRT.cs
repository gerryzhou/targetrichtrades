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
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Strategies.ZTraderStg;
using NinjaTrader.NinjaScript.AddOns;

#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class StgTRT : GSZTraderBase //Strategy
	{
		private GISMI giSMI;
		private GIAwesomeOscillator awOscillator;
		private GISnR giSnR;
		
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				Print("StgTRT set defaults called....");
				Description									= @"Traget Rich Trade.";
				Name										= "StgTRT";
				Calculate									= Calculate.OnBarClose;
				//EntriesPerDirection							= 1;
				//EntryHandling								= EntryHandling.AllEntries;
				//IsExitOnSessionCloseStrategy				= true;
				//ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				//MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				//OrderFillResolution							= OrderFillResolution.Standard;
				//Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				//TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				//RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				//StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				//BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				//IsInstantiatedOnEachOptimizationIteration	= true;
			}
			else if (State == State.DataLoaded)
			{
				//indicatorProxy = new GIndicatorBase();
				//indicatorProxy = GIndicatorProxy(1);
				giSMI = GISMI(3, 5, 5, 8);
				awOscillator = GIAwesomeOscillator(5, 34, 5, MovingAvgType.SMA);
				giSnR = GISnR(false, true, false);
				
				//smaSlow = SMA(Slow);

				//giSMI.Plots[0].Brush = Brushes.Blue;
				
				//smaFast.Plots[0].Brush = Brushes.Goldenrod;
				//smaSlow.Plots[0].Brush = Brushes.SeaGreen;

				AddChartIndicator(giSMI);
				AddChartIndicator(awOscillator);
				AddChartIndicator(giSnR);
				AddChartIndicator(indicatorProxy);
				//AddChartIndicator(smaFast);
				//AddChartIndicator(smaSlow);
			}			
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Day, 1);
				//IncludeCommission = true;
				tradeObj = new TradeObj(this);
				//tradeObj.barsSincePTSL = TM_BarsSincePTSL;
			}
		}

		protected override void OnBarUpdate()
		{
			int prtLevel = 0;
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			indicatorSignal = GetIndicatorSignal();
			//if(BarsInProgress !=0) return;
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			base.OnBarUpdate();
			//Print(CurrentBar.ToString() + " -- StgTRT - Add your custom strategy logic here.");
			//Print("OnBarUpdate - 2");
			//indicatorProxy.Update(); //base has this call;

			//SetStopLoss(CalculationMode.Price, tradeObj.stopLossPrice);
			//CheckExitTrade();
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			//CheckEntryTrade();
			//PutTrade();
		}
		
		public override Direction GetDirection(GIndicatorBase ind){
			//Print(CurrentBar.ToString() + " -- StgTRT override GetDirection: " + ind.GetType().Name + " called...");
			//ind.Update();
			return ind.GetDirection();
//			Direction dir_gismi = giSMI.GetDirection();
//			Direction dir_awo = awOscillator.GetDirection();
//			return null;
		}
		
		public override IndicatorSignal GetIndicatorSignal() {
			giSMI.Update();
			awOscillator.Update();
			giSnR.Update();
			IndicatorSignal indSignal = new IndicatorSignal(); //= new IndicatorSignal();
			Print("GetIndicatorSignal - 1");
			int infl_dn = giSMI.InflectionRecorder.GetLastIndex(CurrentBar, LookbackBarType.Down);//GetLastInflection(giSMI.GetInflection(), CurrentBar, TrendDirection.Down, BarIndexType.BarsAgo);
			int infl_up = giSMI.InflectionRecorder.GetLastIndex(CurrentBar, LookbackBarType.Up);//GetLastInflection(giSMI.GetInflection(), CurrentBar, TrendDirection.Down, BarIndexType.BarsAgo);
			int infl = Math.Max(infl_dn, infl_up);
			//Print(CurrentBar + ": GetIndicatorSignal giSMI.InflectionRecorder.GetLastIndex=" + infl);
			Print("GetIndicatorSignal - 2");
			if(infl > 0 && CurrentBar-infl < CP_EnBarsBeforeInflection) {				
				try{
					//isolate the last inflection
					//int inft = giSMI.GetLastInflection(giSMI.GetInflection(), CurrentBar, TrendDirection.Down);				
					//lookback to the crossover and if that candle is bearish we isolate the open as resistance;
					// if that candlestick is bullish we isolate the close as resistance
					//giSMI.LastCrossover = giSMI.GetLastCrossover(giSMI.GetCrossover(), CurrentBar-inft, CrossoverType.Both);
					indSignal.SnR.Resistance = giSMI.GetSupportResistance(infl, SupportResistanceType.Resistance);
					
				} catch(Exception ex) {
					Print("GetIndicatorSignal ex=" + ex.Message);
				}
				//SMI below the moving average or not
				Direction dir_gismi = GetDirection(giSMI);
				
				//awesome oscillator below zero or not
				Direction dir_awo = GetDirection(awOscillator);
				
				Direction dir = new Direction();
				
				if(dir_gismi.TrendDir == TrendDirection.Down && dir_awo.TrendDir == TrendDirection.Down)
					dir.TrendDir = TrendDirection.Down;

				if(dir_gismi.TrendDir == TrendDirection.Up && dir_awo.TrendDir == TrendDirection.Up)
					dir.TrendDir = TrendDirection.Down;
				indSignal.TrendDir = dir;
			}
			
			return indSignal;
		}
		
		public override TradeObj CheckNewEntryTrade() {
//			if(NewOrderAllowed()) {
			int prtLevel = 0;
			indicatorProxy.TraceMessage(this.Name, prtLevel);//|| Position.Quantity == 0 giSMI.IsNewInflection(TrendDirection.Down) && 
			Print("indicatorSignal null=" + (indicatorSignal==null));
			tradeObj.InitNewEntryTrade();
			if(indicatorSignal != null) {
				if(indicatorSignal.TrendDir.TrendDir == TrendDirection.Down)
				//&& giSMI.GetResistance(indicatorSignal.SnR.Resistance) > High[0]) {
				{
					indicatorProxy.TraceMessage(this.Name, prtLevel);
					tradeObj.tradeDirection = TradingDirection.Down;
				}
				else if(indicatorSignal.TrendDir.TrendDir == TrendDirection.Up)
				//&& giSMI.GetResistance(indicatorSignal.SnR.Resistance) > High[0]) {
				{
					indicatorProxy.TraceMessage(this.Name, prtLevel);
					tradeObj.tradeDirection = TradingDirection.Up;
				}
				
				tradeObj.tradeStyle = TradingStyle.TrendFollowing;
				
			} else {
				tradeObj.SetTradeType(TradeType.NoTrade);
			}
			
			if(indicatorSignal != null && indicatorSignal.TrendDir.TrendDir == TrendDirection.Down) {
				//Print(CurrentBar + ": GetResistance=" + indicatorProxy.GetResistance(indicatorSignal.SnR) + ", SnR.BarNo=" + indicatorSignal.SnR.BarNo + ", SnRPriceType=" + indicatorSignal.SnR.SnRPriceType);
			}
			return tradeObj;
		}
		
		public override void PutTrade() {
			if(tradeObj.GetTradeType() == TradeType.Entry) {
				indicatorProxy.PrintLog(true, !BackTest, "PutTrade tradeObj.stopLossAmt=" + tradeObj.stopLossAmt + "," + MM_StopLossAmt);
				if(tradeObj.tradeDirection == TradingDirection.Down) {
					//tradeObj.BracketOrder.EntryOrder = 
					//EnterShort(OrderSignalName.EntryShort.ToString());
					//EnterShortLimit(Close[0], OrderSignalName.EntryShort.ToString());
					//tradeObj.entrySignalName = GetNewEnOrderSignalName(OrderSignalName.EntryShort.ToString());
					indicatorProxy.PrintLog(true, !BackTest, "PutTrade Down OrderSignalName=" + tradeObj.entrySignalName);
					tradeObj.enLimitPrice = Close[0];					
					NewShortLimitOrderUM(OrderSignalName.EntryShortLmt.ToString());
				}
				else if(tradeObj.tradeDirection == TradingDirection.Up) {
					indicatorProxy.PrintLog(true, !BackTest, "PutTrade Up OrderSignalName=" + tradeObj.entrySignalName);
					EnterLong(OrderSignalName.EntryLongMkt.ToString());
					//EnterLongLimit(Low[0]-5, OrderSignalName.EntryLong.ToString());
				}				
			}
		}
		
        #region Custom Properties
		
        [Description("Bars count before inflection for entry")]
 		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EnBarsBeforeInflection", GroupName = "CustomParams", Order = 0)]
        public int CP_EnBarsBeforeInflection
        {
            get { return cp_EnBarsBeforeInflection; }
            set { cp_EnBarsBeforeInflection = Math.Max(1, value); }
        }
		
		private int cp_EnBarsBeforeInflection = 2;
		
		#endregion
	}
}
