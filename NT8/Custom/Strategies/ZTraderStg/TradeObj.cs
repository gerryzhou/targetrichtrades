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
		public TradingDirection tradeDirection = TradingDirection.Both;
		public TradingStyle tradeStyle = TradingStyle.TrendFollowing;
		public string entrySignalName = "";
		
		#region Money Mgmt variables
		
		public double profitTargetAmt = 350;//36 Default(450-650 USD) setting for MM_ProfitTargetAmt
		public int profitTargetTic = 36;//Ticks of profit target
		public int profitTgtIncTic = 6;//8 Default tick Amt for ProfitTarget increase Amt
		public int profitLockMinTic = 16;//24 Default ticks Amt for Min Profit locking
		public int profitLockMaxTic = 30;//80 Default ticks Amt for Max Profit locking
		public int trailingPTTic = 16;//Ticks for trailing ProfitTarget, using ticks to record current PT
		public double profitTargetPrice = 0;//For trailing PT using the price to set OCO order
		public double stopLossPrice = 0;//Using price to set OCO order, since Amt could be negative
        public double stopLossAmt = 200;//16 ticks Default setting for stopLossAmt
		public int stopLossTic = 16;//16 Default setting for stopLossTic
		public int stopLossIncTic = 4;//4 Default tick Amt for StopLoss increase Amt
		public double breakEvenAmt = 150;//150 the profits amount to trigger setting breakeven order
		public double trailingSLAmt = 100;//300 Default setting for trailing Stop Loss Amt
		public int trailingSLTic = 4;//Ticks for trailing stop loss order
		public double traillingSLPercent = 1;//Percent for trailing stop loss order
		public bool slTrailing = false;//Trailing stop loss
		public bool ptTrailing = false;//Trailing profit target
		
		public double dailyLossLmt = -200;//-300 the daily loss limit amount
		public int quantity = 1; //Quantity of the contracts traded
		public double profitFactor = 0.5;//PT/SL ratio
	
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

		#endregion
		
		#region Trade Mgmt variables
		
		public int enCounterPBBars = 1;//Bar count of pullback for breakout entry setup
		public bool enTrailing = true;//Trailing the entry price
		public double enOffsetPnts = 1;//The offset for price of entry limite order
		public double enStopPrice = 0;//The stop market price for entry order
		public double enLimitPrice = 0;//The limit price for entry order
		
		public int minutesChkEnOrder = 20;//How long before checking an entry order filled or not
		public int minutesChkPnL = 30;//How long before checking P&L
		
		public int barsHoldEnOrd = 10; //Bars count since en order was issued
        public int barsSincePTSL = 1;//Bar count since last P&L was filled
		public int barsToCheckPnL = 2;//Bar count to check P&L since the entry

		public int barsSinceEnOrd = 0;//Bar count since the en order issued
		
		#endregion		
		
		#region Order Objects
		private BracketOrderBase bracketOrder = new BracketOrderBase();
		
		#endregion
		
		public TradeObj(GSZTraderBase inst_strategy) {
			this.instStrategy = inst_strategy;
			InitParams();
		}
		
		private void InitParams() {
			enTrailing = instStrategy.TM_EnTrailing;
			enOffsetPnts = instStrategy.TM_EnOffsetPnts;
			enCounterPBBars = instStrategy.TM_EnCounterPBBars;
			
			minutesChkEnOrder = instStrategy.TM_MinutesChkEnOrder;
			minutesChkPnL = instStrategy.TM_MinutesChkPnL;
			
			barsHoldEnOrd = instStrategy.TM_BarsHoldEnOrd;
	        barsSincePTSL = instStrategy.TM_BarsSincePTSL;
			barsToCheckPnL = instStrategy.TM_BarsToCheckPnL;
			
			profitTargetAmt = instStrategy.MM_ProfitTargetAmt;
			profitTgtIncTic = instStrategy.MM_ProfitTgtIncTic;
			profitLockMinTic = instStrategy.MM_ProfitLockMinTic;
			profitLockMaxTic = instStrategy.MM_ProfitLockMaxTic;
			
	        stopLossAmt = instStrategy.MM_StopLossAmt;
			stopLossIncTic = instStrategy.MM_StopLossIncTic;
			
			trailingPTTic = profitLockMinTic;
			trailingSLTic = instStrategy.MM_TrailingStopLossTicks;
			trailingSLAmt = instStrategy.MM_TrailingStopLossAmt;
			traillingSLPercent = 1;
			
						
			breakEvenAmt = instStrategy.MM_BreakEvenAmt;
			slTrailing = instStrategy.MM_SLTrailing;
			ptTrailing = instStrategy.MM_PTTrailing;
			
			profitFactor = instStrategy.MM_ProfitFactor;
			dailyLossLmt = instStrategy.MM_DailyLossLmt;
			
			quantity = instStrategy.DefaultQuantity;
		}
		
		public void InitNewEntryTrade() {
			InitParams();
		}
		
		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public BracketOrderBase BracketOrder
		{
			get { return bracketOrder; }
			set { bracketOrder = value; }
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
