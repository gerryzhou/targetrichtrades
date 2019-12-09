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
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Indicators.ZTraderPattern;
using NinjaTrader.NinjaScript.Indicators.PriceActions;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.NinjaScript.Strategies.ZTraderStg;
using NinjaTrader.NinjaScript.AddOns;

#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class StgSnR : GStrategyBase
	{
		private GISMI giSMI;
		private GISnR giSnR;
		
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				Print(this.Name + " set defaults called....");
				Description									= @"Strategy based on support and resistance, combined with fibonacci ratios.";
				Name										= "StgSnR";
				Calculate									= Calculate.OnBarClose;
//				EntriesPerDirection							= 1;
//				EntryHandling								= EntryHandling.AllEntries;
//				IsExitOnSessionCloseStrategy				= true;
//				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
//				MaximumBarsLookBack							= MaximumBarsLookBack.Infinite;
//				OrderFillResolution							= OrderFillResolution.Standard;
//				Slippage									= 0;
//				StartBehavior								= StartBehavior.WaitUntilFlat;
//				TimeInForce									= TimeInForce.Day;
				TraceOrders									= false;
//				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
//				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
//				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
//				IsInstantiatedOnEachOptimizationIteration	= true;
				ShowLastDayHL								= false;
				ShowOpenHL									= true;
				ShowOvernightHL								= false;
				//AddPlot(new Stroke(Brushes.LimeGreen, 2), PlotStyle.Hash, "Spt");
				//AddPlot(new Stroke(Brushes.Tomato, 2), PlotStyle.Hash, "Rst");
			}
			else if (State == State.DataLoaded)
			{
				Print(this.Name + " set DataLoaded called....");
				//indicatorProxy = new GIndicatorBase();
				//indicatorProxy = GIndicatorProxy(1);
				giSMI = GISMI(3, 5, 5, 8, 50);
				giSnR = GISnR(ShowOvernightHL, ShowOpenHL, ShowLastDayHL);
				//awOscillator = GIAwesomeOscillator(5, 34, 5, MovingAvgType.SMA);

				AddChartIndicator(giSMI);
				AddChartIndicator(giSnR);
				AddChartIndicator(indicatorProxy);
				indicatorProxy.LoadSpvPRList(SpvDailyPatternES.spvPRDayES);
			}			
			else if (State == State.Configure)
			{
				Print(this.Name + " set Configure called....");
				AddDataSeries(Data.BarsPeriodType.Day, 1);
				//IncludeCommission = true;
				CurrentTrade = new CurrentTrade(this);
				CurrentTrade.barsSincePTSL = TM_BarsSincePTSL;
			}			
		}

		protected override void OnBarUpdate()
		{	
			if (BarsInProgress != 0) return;
			
			//Print(CurrentBar.ToString() + " -- StgTRT - Add your custom strategy logic here.");
			indicatorProxy.Update();
			PriceAction pa = indicatorProxy.GetPriceAction(Time[0]);
			indicatorProxy.PrintLog(true, false, CurrentBar + ":Time=" + Time[0].ToShortDateString() + "-" + Time[0].ToShortTimeString() + ", PriceAction=" + pa.paType.ToString());
			//tradeSignal = GetTradeSignal();

			base.OnBarUpdate();
			//SetStopLoss(CalculationMode.Price, CurrentTrade.stopLossPrice);
			//CheckExitTrade();
			//CheckEntryTrade();
			//PutTrade();
		}

		public override Direction GetDirection(GIndicatorBase ind){
			return ind.GetDirection();
		}
		
		public override TradeSignal GetTradeSignal() {
			giSMI.Update();
			giSnR.Update();
			TradeSignal trdSignal = null; //= new IndicatorSignal();
			
			if(CurrentBar <= BarsRequiredToTrade) return trdSignal;
			
			int infl = giSMI.GetInflection(giSMI.SMITMA);
			
			if(infl != 0) {
				//Check divergence for CurrentBar with the last infl bar;
				DivergenceType dvType = CheckDivergence(giSMI);
				if(infl < 0 && dvType == DivergenceType.Divergent) {
					trdSignal = new TradeSignal();
					trdSignal.BarNo = CurrentBar;
//					trdSignal.ReversalDir = Reversal.Down;
//					trdSignal.SnR = giSMI.GetSupportResistance(CurrentBar-1);
				}//entry short by reversal
				else if(infl > 0 && dvType == DivergenceType.Convergent) {
					trdSignal = new TradeSignal();
					trdSignal.BarNo = CurrentBar;
					Direction dir = new Direction();
//					dir.TrendDir = TrendDirection.Down;
//					trdSignal.TrendDir = dir;
//					trdSignal.SnR = giSMI.GetSupportResistance(CurrentBar-1);
				}//entry short following down trend
				else if(infl < 0 && dvType == DivergenceType.Convergent) {
					trdSignal = new TradeSignal();
					trdSignal.BarNo = CurrentBar;
					Direction dir = new Direction();
//					dir.TrendDir = TrendDirection.Up;
//					trdSignal.TrendDir = dir;
//					trdSignal.SnR = giSMI.GetSupportResistance(CurrentBar-1);
				}//entry long following up trend
				else if(infl > 0 && dvType == DivergenceType.Divergent) {
					trdSignal = new TradeSignal();
					trdSignal.BarNo = CurrentBar;
//					trdSignal.ReversalDir = Reversal.Up;
//					trdSignal.SnR = giSMI.GetSupportResistance(CurrentBar-1);
				}//entry long by reversal
			}
			return trdSignal;
		}
		
		public override bool CheckNewEntryTrade() {
			int prtLevel = 1;
			indicatorProxy.TraceMessage(this.Name, prtLevel);
//			if(GetTradeSignal(CurrentBar) != null) {
//				CurrentTrade.InitNewEntryTrade();
//				if(GetTradeSignal(CurrentBar).TrendDir != null 
//					&& GetTradeSignal(CurrentBar).TrendDir.TrendDir == TrendDirection.Down
//					&& indicatorProxy.GetResistance(GetTradeSignal(CurrentBar).SnR.Resistance) > High[0]) {
//					indicatorProxy.TraceMessage(this.Name, prtLevel);
//					CurrentTrade.tradeDirection = TradingDirection.Down;
//					CurrentTrade.tradeStyle = TradingStyle.TrendFollowing;
//					CurrentTrade.stopLossPrice = indicatorProxy.GetResistance(GetTradeSignal(CurrentBar).SnR.Resistance);
//				}
//				else if(GetTradeSignal(CurrentBar).TrendDir != null 
//					&& GetTradeSignal(CurrentBar).TrendDir.TrendDir == TrendDirection.Up
//					&& indicatorProxy.GetSupport(GetTradeSignal(CurrentBar).SnR.Support) < Low[0]) {
//					indicatorProxy.TraceMessage(this.Name, prtLevel);
//					CurrentTrade.tradeDirection = TradingDirection.Up;
//					CurrentTrade.tradeStyle = TradingStyle.TrendFollowing;
//					CurrentTrade.stopLossPrice = indicatorProxy.GetSupport(GetTradeSignal(CurrentBar).SnR.Support);
					//Print(CurrentBar + ": GetResistance=" + indicatorProxy.GetResistance(indicatorSignal.SnR) + ", SnR.BarNo=" + indicatorSignal.SnR.BarNo + ", SnRPriceType=" + indicatorSignal.SnR.SnRPriceType);
//				}
//			} else {
//				CurrentTrade.CurrentTradeType = TradeType.NoTrade;
//			}
			return false;
		}

		public override void PutTrade() {
			if(CurrentTrade.TradeAction.TradeActionType == TradeActionType.EntrySimple) {
				indicatorProxy.PrintLog(true, IsLiveTrading(), "PutTrade CurrentTrade.stopLossAmt=" + CurrentTrade.stopLossAmt + "," + MM_StopLossAmt);
				if(CurrentTrade.tradeDirection == TradingDirection.Down) {
					//CurrentTrade.TradeAction.EntrySignal.SignalName = GetNewEnOrderSignalName(OrderSignalName.EntryShort.ToString());
					indicatorProxy.PrintLog(true, IsLiveTrading(), "PutTrade Down OrderSignalName=" + CurrentTrade.TradeAction.EntrySignal.SignalName);
					CurrentTrade.TradeAction.EntryPrice = Close[0];
					NewShortLimitOrderUM(OrderSignalName.EntryShortLmt.ToString());
				}
				else if(CurrentTrade.tradeDirection == TradingDirection.Up) {
					indicatorProxy.PrintLog(true, IsLiveTrading(), "PutTrade Up OrderSignalName=" + CurrentTrade.TradeAction.EntrySignal.SignalName);
					CurrentTrade.TradeAction.EntryPrice = Close[0];
					NewLongLimitOrderUM(OrderSignalName.EntryLongLmt.ToString());
				}				
			}
		}
		
		public override DivergenceType CheckDivergence(GIndicatorBase indicator) {
			
			return indicator.CheckDivergence();
		}
		

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="ShowOvernightHL", Description="Show the overnight High/Low", Order=1, GroupName="CustomParams")]
		public bool ShowOvernightHL
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowOpenHL", Description="Show Opening High/Low of today", Order=2, GroupName="CustomParams")]
		public bool ShowOpenHL
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="ShowLastDayHL", Description="Show High/Low of last day", Order=3, GroupName="CustomParams")]
		public bool ShowLastDayHL
		{ get; set; }
		
        [Description("Bars count before inflection for entry")]
 		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EnBarsBeforeInflection", GroupName = "CustomParams", Order = 4)]
        public int CP_EnBarsBeforeInflection
        {
            get { return cp_EnBarsBeforeInflection; }
            set { cp_EnBarsBeforeInflection = Math.Max(1, value); }
        }
		
		private int cp_EnBarsBeforeInflection = 2;		

//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> Spt
//		{
//			get { return Values[0]; }
//		}

//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> Rst
//		{
//			get { return Values[1]; }
//		}
		#endregion

	}
}
