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
	/// <summary>
	/// The class holds current trade, including positions, orders, tradeAction etc.
	/// All the trading parameters from strategy are held in the CurrentTrade;
	/// All the parameters from the signal are held in TradeAction 
	/// CurrentTrade->TradeAction->TradeSignal;
	/// 
	/// </summary>
	public class CurrentTradeBase {
		
		public CurrentTradeBase(GStrategyBase instStg) {
			InstStrategy = instStg;
			InitNewTrade();
		}

		#region Order Objects
		
		public string ocoID = String.Empty;
		private BracketOrderBase bracketOrder = new BracketOrderBase();
		private TrailingSLOrderBase trailingSLOrder = new TrailingSLOrderBase();
		public EntryExitOrderType exitOrderType = EntryExitOrderType.SimpleOCO;
		
		#endregion
		
		#region Trade Mgmt variables
		
		public TradingDirection tradeDirection = TradingDirection.Both;
		public TradingStyle tradeStyle = TradingStyle.TrendFollowing;
		
		public int enCounterPBBars = 1;//Bar count of pullback for breakout entry setup
		public bool enTrailing = true;//Trailing the entry price
		public double enOffsetPnts = 1;//The offset for price of entry limite order

		//Set at runtime
//		public double enStopPrice = 0;//The stop market price for entry order
//		public double enLimitPrice = 0;//The limit price for entry order
		
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
		public int quantity = 1; //Quantity of the total contracts allowed to trade for the strategy

//		public double profitTargetPrice = 0;//Runtime var. For trailing PT using the price to set OCO order
//		public double stopLossPrice = 0;//Runtime var; Using price to set OCO order, since Amt could be negative		
//		public double trailingSLPrice = 0;//Runtime var. Price for trailing stoploss price
//		public int trailingPTTic = 16;//Runtime var. Ticks for trailing ProfitTarget, using ticks to record current PT	
		
		#endregion
		
		#region Trade Methods
		
		private string GetNewTradeID() {
			return "TD-" + InstStrategy.GetBarTimestampStr(0);
		}
		
		private void InitNewTrade() {
			TradeID = GetNewTradeID();
			//TM variables
			tradeDirection = InstStrategy.TM_TradingDirection;
			tradeStyle = InstStrategy.TM_TradingStyle;
			
			enCounterPBBars = InstStrategy.TM_EnCounterPBBars;
			enTrailing = InstStrategy.TM_EnTrailing;
			enOffsetPnts = InstStrategy.TM_EnOffsetPnts;
			
			minutesChkEnOrder = InstStrategy.TM_MinutesChkEnOrder;
			minutesChkPnL = InstStrategy.TM_MinutesChkPnL;
			
			barsHoldEnOrd = InstStrategy.TM_BarsHoldEnOrd;
	        barsSincePTSL = InstStrategy.TM_BarsSincePTSL;
			barsToCheckPnL = InstStrategy.TM_BarsToCheckPnL;
			
			//MM variables
			profitTargetAmt = InstStrategy.MM_ProfitTargetAmt;
			profitTargetTic = InstStrategy.MM_ProfitTgtTic;
			profitTgtIncTic = InstStrategy.MM_ProfitTgtIncTic;
			profitLockMinTic = InstStrategy.MM_ProfitLockMinTic;
			profitLockMaxTic = InstStrategy.MM_ProfitLockMaxTic;
			
	        stopLossAmt = InstStrategy.MM_StopLossAmt;
			stopLossTic = InstStrategy.MM_StopLossTic;
			stopLossIncTic = InstStrategy.MM_StopLossIncTic;

			breakEvenAmt = InstStrategy.MM_BreakEvenAmt;

			trailingSLAmt = InstStrategy.MM_TrailingStopLossAmt;			
			trailingSLTic = InstStrategy.MM_TrailingStopLossTic;
			traillingSLPercent = InstStrategy.MM_TrailingStopLossPercent;
			
//			trailingPTTic = profitLockMinTic;

			slTrailing = InstStrategy.MM_SLTrailing;
			ptTrailing = InstStrategy.MM_PTTrailing;
							
			PTCalculationMode = InstStrategy.MM_PTCalculationMode;
			SLCalculationMode = InstStrategy.MM_SLCalculationMode;
			BECalculationMode = InstStrategy.MM_BECalculationMode;
			TLSLCalculationMode = InstStrategy.MM_TLSLCalculationMode;
			
			profitFactor = InstStrategy.MM_ProfitFactor;
			dailyLossLmt = InstStrategy.MM_DailyLossLmt;
			
			quantity = InstStrategy.DefaultQuantity;
		}
		
		public void InitNewEntryTrade() {
			//InitParams();
			//CurrentTradeType = TradeType.Entry;
			exitOrderType = EntryExitOrderType.SimpleOCO;
		}

		public void InitNewTLSL() {
			//InitParams();
			//CurrentTradeType = TradeType.Exit;
			exitOrderType = EntryExitOrderType.TrailingStopLoss;
			this.TradeAction.TrailingProfitTargetTics = 0;
			switch(TLSLCalculationMode) {
				case CalculationMode.Currency:
					this.trailingSLTic = InstStrategy.GetTicksByCurrency(this.trailingSLAmt);
					break;
				
				case CalculationMode.Percent:
					break;
			}
		}
		
		#endregion
		
		#region Event Handler Methods
		public void OnCurPositionUpdate(Cbi.Position position, double averagePrice, 
			int quantity, Cbi.MarketPosition marketPosition)
		{
			PosAvgPrice = averagePrice;
			PosQuantity = quantity;
			MktPosition = marketPosition;
			PosUnrealizedPnL = position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, InstStrategy.Close[0]);
			InstStrategy.Print(InstStrategy.CurrentBar + ": OnCurPositionUpdate--");
			try{
			InstStrategy.Print(String.Format("{0}, AvgPrc: {1}, Quant={2}, MktPos={3}, PnL={4}",
					InstStrategy.CurrentBar, PosAvgPrice, PosQuantity, MktPosition, PosUnrealizedPnL));
			//Position pos = position.MemberwiseClone();
			if (MktPosition == MarketPosition.Flat)
			{
				//TradeAction.TrailingProfitTargetTics = InstStrategy.GetTicksByCurrency(TradeAction.profitTargetAmt);
				//trailingSLTic = GetTicksByCurrency(CurrentTrade.stopLossAmt);
			}
			else
			{
				InstStrategy.CalProfitTargetAmt(PosAvgPrice, InstStrategy.MM_ProfitFactor);
				InstStrategy.CalExitOcoPrice(PosAvgPrice, InstStrategy.MM_ProfitFactor);
				InstStrategy.SetSimpleExitOCO(TradeAction.EntrySignal.SignalName);
//				SetBracketOrder.OCOOrder.ProfitTargetOrder(OrderSignalName.EntryShort.ToString());
//				SetBracketOrder.OCOOrder.StopLossOrder(OrderSignalName.EntryShort.ToString());
			}
			}catch(Exception ex) {
				InstStrategy.Print("Exception=" + ex.StackTrace);
			}
		}
		#endregion
		
		#region Properties
		
		/// <summary>
		/// Removed since the type is defined at TradeAction;
		/// TradeType is not clear concept, a trade includs entry, exit, etc.
		/// The tradeAction will define the type of action
		/// </summary>
//		[Browsable(false), XmlIgnore]
//		public TradeType CurrentTradeType {
//			get{ return tradeType;}
//			set { tradeType = value;}
//		}
		/// <summary>
		/// The unique ID for each trade
		/// TD-yyyyMMddHHmmssfff
		/// The ocoId could use the TradeID
		/// The timestamp could be removed from:
		/// Entry, StopLoss, PT, and TLSL Signal Names
		/// </summary>
		[Description("The ID for current trade")]
		[Browsable(false), XmlIgnore]
		public string TradeID
		{
			get; set;
		}
		
		[Description("Trade action for current trade")]
		[Browsable(false), XmlIgnore]
		public TradeAction TradeAction
		{
			get; set;
		}
		
		[Browsable(false), XmlIgnore]
		public BracketOrderBase BracketOrder
		{
			get { return bracketOrder; }
			set { bracketOrder = value; }
		}

		[Browsable(false), XmlIgnore]
		public TrailingSLOrderBase TrailingSLOrder
		{
			get { return trailingSLOrder; }
			set { trailingSLOrder = value; }
		}
		
		[DefaultValueAttribute(MarketPosition.Flat)]
		[Browsable(false), XmlIgnore]
		public MarketPosition MktPosition
		{
			get; set;
		}
		
		[DefaultValueAttribute(0)]
		[Browsable(false), XmlIgnore]
		public int PosQuantity
		{
			get; set;
		}
		
		[DefaultValueAttribute(0)]
		[Browsable(false), XmlIgnore]
		public double PosAvgPrice
		{
			get; set;
		}
		
		[DefaultValueAttribute(0)]
		[Browsable(false), XmlIgnore]
		public double PosUnrealizedPnL
		{
			get; set;
		}		

		[Display(Name="InstStg", Description="Strategy instance for Current Trade")]
		[Browsable(false), XmlIgnore]
		public GStrategyBase InstStrategy
		{ 
			get;set;
		}
		
		//private GStrategyBase InstStrategy = null;
		
		#endregion
	}
}
