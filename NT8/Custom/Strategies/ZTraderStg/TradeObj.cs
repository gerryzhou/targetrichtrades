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
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies.ZTraderStg
{
	public class TradeObj {
		private GSZTraderBase instStrategy = null;
		private TradeType tradeType = TradeType.NoTrade;

		#region Order Objects
		public string entrySignalName = String.Empty;
		public string stopLossSignalName = String.Empty;
		public string profitTargetSignalName = String.Empty;
		public string ocoID = String.Empty;
		public string trailingSLSignalName = String.Empty;
		
		private BracketOrderBase bracketOrder = new BracketOrderBase();
		private TrailingSLOrderBase trailingSLOrder = new TrailingSLOrderBase();
		public ExitOrderType exitOrderType = ExitOrderType.SimpleOCO;

		#endregion
		
		#region Trade Mgmt variables
		
		public TradingDirection tradeDirection = TradingDirection.Both;
		public TradingStyle tradeStyle = TradingStyle.TrendFollowing;
		
		public int enCounterPBBars = 1;//Bar count of pullback for breakout entry setup
		public bool enTrailing = true;//Trailing the entry price
		public double enOffsetPnts = 1;//The offset for price of entry limite order

		//Set at runtime
		public double enStopPrice = 0;//The stop market price for entry order
		public double enLimitPrice = 0;//The limit price for entry order
		
		public int minutesChkEnOrder = 20;//How long before checking an entry order filled or not
		public int minutesChkPnL = 30;//How long before checking P&L
		
		public int barsHoldEnOrd = 10; //Bars count since en order was issued
        public int barsSincePTSL = 1;//Bar count since last P&L was filled
		public int barsToCheckPnL = 2;//Bar count to check P&L since the entry

		//Runtime var
		public int barsSinceEnOrd = 0;//Bar count since the en order issued
		
		#endregion
		
		#region Money Mgmt variables
		
		public double profitTargetAmt = 350;//36 Default(450-650 USD) setting for MM_ProfitTargetAmt
		public int profitTargetTic = 36;//Ticks of profit target
		public int profitTgtIncTic = 6;//8 Default tick Amt for ProfitTarget increase Amt
		public int profitLockMinTic = 16;//24 Default ticks Amt for Min Profit locking
		public int profitLockMaxTic = 30;//80 Default ticks Amt for Max Profit locking
		     
		public double stopLossAmt = 200;//16 ticks Default setting for stopLossAmt
		public int stopLossTic = 16;//16 Default setting for stopLossTic
		public int stopLossIncTic = 4;//4 Default tick Amt for StopLoss increase Amt

		public double breakEvenAmt = 150;//150 the profits amount to trigger setting breakeven order

		public double trailingSLAmt = 100;//300 Default setting for trailing Stop Loss Amt
		public int trailingSLTic = 4;//Ticks for trailing stop loss order
		public double traillingSLPercent = 1;//Percent for trailing stop loss order

		public bool slTrailing = false;//Trailing stop loss
		public bool ptTrailing = false;//Trailing profit target
	
		/// <summary>
		/// Use price for the internal unified CalculationMode
		/// since the stop loss over breakeven will be negative,
		/// and it is easy to find conflict with SL/PT/TLSL price in OCO order;
		/// SL, PT Amt and Tic are non-changable after tradeObj is init for new trade,
		/// the price is changable to ajust during the exit OCO lifecycle;
		/// </summary>
		public CalculationMode PTCalculationMode = CalculationMode.Currency;//Profit target CalMode
		public CalculationMode SLCalculationMode = CalculationMode.Currency;//Stoploss CalMode
		public CalculationMode BECalculationMode = CalculationMode.Currency;//Breakeven CalMode
		public CalculationMode TLSLCalculationMode = CalculationMode.Ticks;//Trailing SL CalMode
		
		public double dailyLossLmt = -200;//-300 the daily loss limit amount
		public double profitFactor = 0.5;//PT/SL ratio
		public int quantity = 1; //Quantity of the contracts traded

		public double profitTargetPrice = 0;//Runtime var. For trailing PT using the price to set OCO order
		public double stopLossPrice = 0;//Runtime var; Using price to set OCO order, since Amt could be negative		
		public double trailingSLPrice = 0;//Runtime var. Price for trailing stoploss price
		public int trailingPTTic = 16;//Runtime var. Ticks for trailing ProfitTarget, using ticks to record current PT	
		
		#endregion
		
		public TradeObj(GSZTraderBase inst_strategy) {
			this.instStrategy = inst_strategy;
			InitParams();
		}
		
		private void InitParams() {
			//TM variables
			tradeDirection = instStrategy.TM_TradingDirection;
			tradeStyle = instStrategy.TM_TradingStyle;
			
			enCounterPBBars = instStrategy.TM_EnCounterPBBars;
			enTrailing = instStrategy.TM_EnTrailing;
			enOffsetPnts = instStrategy.TM_EnOffsetPnts;
			
			minutesChkEnOrder = instStrategy.TM_MinutesChkEnOrder;
			minutesChkPnL = instStrategy.TM_MinutesChkPnL;
			
			barsHoldEnOrd = instStrategy.TM_BarsHoldEnOrd;
	        barsSincePTSL = instStrategy.TM_BarsSincePTSL;
			barsToCheckPnL = instStrategy.TM_BarsToCheckPnL;
			
			//MM variables
			profitTargetAmt = instStrategy.MM_ProfitTargetAmt;
			profitTargetTic = instStrategy.MM_ProfitTgtTic;
			profitTgtIncTic = instStrategy.MM_ProfitTgtIncTic;
			profitLockMinTic = instStrategy.MM_ProfitLockMinTic;
			profitLockMaxTic = instStrategy.MM_ProfitLockMaxTic;
			
	        stopLossAmt = instStrategy.MM_StopLossAmt;
			stopLossTic = instStrategy.MM_StopLossTic;
			stopLossIncTic = instStrategy.MM_StopLossIncTic;

			breakEvenAmt = instStrategy.MM_BreakEvenAmt;

			trailingSLAmt = instStrategy.MM_TrailingStopLossAmt;			
			trailingSLTic = instStrategy.MM_TrailingStopLossTic;
			traillingSLPercent = instStrategy.MM_TrailingStopLossPercent;
			
			trailingPTTic = profitLockMinTic;						

			slTrailing = instStrategy.MM_SLTrailing;
			ptTrailing = instStrategy.MM_PTTrailing;
							
			PTCalculationMode = instStrategy.MM_PTCalculationMode;
			SLCalculationMode = instStrategy.MM_SLCalculationMode;
			BECalculationMode = instStrategy.MM_BECalculationMode;
			TLSLCalculationMode = instStrategy.MM_TLSLCalculationMode;
			
			profitFactor = instStrategy.MM_ProfitFactor;
			dailyLossLmt = instStrategy.MM_DailyLossLmt;
			
			quantity = instStrategy.DefaultQuantity;
		}
		
		public void InitNewEntryTrade() {
			InitParams();
			SetTradeType(TradeType.Entry);
			exitOrderType = ExitOrderType.SimpleOCO;
		}

		public void InitNewTLSL() {
			//InitParams();
			SetTradeType(TradeType.Exit);
			exitOrderType = ExitOrderType.TrailingStopLoss;
			this.trailingPTTic = 0;
			switch(TLSLCalculationMode) {
				case CalculationMode.Currency:
					this.trailingSLTic = instStrategy.GetTicksByCurrency(this.trailingSLAmt);
					break;
				
				case CalculationMode.Percent:
					break;				
			}
		}
		
		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public BracketOrderBase BracketOrder
		{
			get { return bracketOrder; }
			set { bracketOrder = value; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public TrailingSLOrderBase TrailingSLOrder
		{
			get { return trailingSLOrder; }
			set { trailingSLOrder = value; }
		}
		#endregion
		
		#region Other Properties
		public TradeType GetTradeType() {
			return tradeType;
		}
		public void SetTradeType(TradeType t) {
			tradeType = t;
		}
		#endregion
	}
}
