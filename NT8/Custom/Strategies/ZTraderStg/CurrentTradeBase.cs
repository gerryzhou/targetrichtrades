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
	/// OrderMgmt: setup SL/PT, breakeven: when to setup? Triggerred by PerformRule;
	/// decide new trade, new pos: exe event or pos event?
	/// Signal triggerred order change? when to change the orders? OnBarUpdate
	/// Event triggerred order change? when to change the orders? OnBarUpdate: breakeven, trailing stop,
	/// Command triggerred order change? when to change the orders? OnBarUpdate
	/// PerformRule triggerred order change? when to change the orders? OnBarUpdate, the last trigger; 
	/// Priority of the three changes: command (change algo mode, change params, injectContext),
	/// signal/event(exclusive, event trigger new order allowed, init SL/PT; signal trigger entry/exit),
	/// performRule (trigger money mgmt rules, change SL/PT, break-even, trailing stop; trigger trailing entry, exit by price change)
	/// Command/rule/performance triggered change: change TradeSignal, fire the change the same bar; 
	/// treat command as if a signal(how to define the signal with regular signal, add to the list, type of command signal);
	/// Signal triggered change: signal changed, fire the change the same bar;
	/// Event triggered change: change the signal first, fire the change on the event handler;
	/// Sequence of the triggers: OnBarUpdate take the command, then the performace checking apply rule trigger changes,
	/// the signal trigger and event trigger could come mixed, it has no preset order.
	/// 
	/// </summary>
	public class CurrentTradeBase {
		
		public CurrentTradeBase(GStrategyBase instStg) {
			InstStrategy = instStg;
			InitNewTrade();
		}
	
		#region Trade Mgmt variables
		
//		public TradingDirection TradeDirection = TradingDirection.Both;
//		public TradingStyle TradeStyle = TradingStyle.TrendFollowing;
		
//		public int enCounterPBBars = 1;//Bar count of pullback for breakout entry setup
//		public bool enTrailing = true;//Trailing the entry price
//		public double enOffsetPnts = 1;//The offset for price of entry limite order

		//Set at runtime
//		public double enStopPrice = 0;//The stop market price for entry order
//		public double enLimitPrice = 0;//The limit price for entry order
		
//		public int minutesChkEnOrder = 20;//How long before checking an entry order filled or not
//		public int minutesChkPnL = 30;//How long before checking P&L
		
//		public int barsHoldEnOrd = 10; //Bars count since en order was issued
//        public int barsSincePTSL = 1;//Bar count since last P&L was filled
//		public int barsToCheckPnL = 2;//Bar count to check P&L since the entry

		//Runtime var
		public int barsSinceEnOrd = 0;//Bar count since the en order issued
		
		#endregion
		
		#region Money Mgmt variables
		
//		public double profitTargetAmt = 350;//36 Default(450-650 USD) setting for MM_ProfitTargetAmt
//		public int profitTargetTic = 36;//Ticks of profit target
//		public int profitTgtIncTic = 6;//8 Default tick Amt for ProfitTarget increase Amt
//		public int profitLockMinTic = 16;//24 Default ticks Amt for Min Profit locking
//		public int profitLockMaxTic = 30;//80 Default ticks Amt for Max Profit locking
		     
//		public double stopLossAmt = 200;//16 ticks Default setting for stopLossAmt
//		public int stopLossTic = 16;//16 Default setting for stopLossTic
//		public int stopLossIncTic = 4;//4 Default tick Amt for StopLoss increase Amt

//		public double breakEvenAmt = 150;//150 the profits amount to trigger setting breakeven order

//		public double trailingSLAmt = 100;//300 Default setting for trailing Stop Loss Amt
//		public int trailingSLTic = 4;//Ticks for trailing stop loss order
//		public double traillingSLPercent = 1;//Percent for trailing stop loss order

//		public bool slTrailing = false;//Trailing stop loss
//		public bool ptTrailing = false;//Trailing profit target
	
		/// <summary>
		/// Use price for the internal unified CalculationMode
		/// since the stop loss over breakeven will be negative,
		/// and it is easy to find conflict with SL/PT/TLSL price in OCO order;
		/// SL, PT Amt and Tic are non-changable after tradeObj is init for new trade,
		/// the price is changable to ajust during the exit OCO lifecycle;
		/// </summary>
//		public CalculationMode PTCalculationMode = CalculationMode.Currency;//Profit target CalMode
//		public CalculationMode SLCalculationMode = CalculationMode.Currency;//Stoploss CalMode
//		public CalculationMode BECalculationMode = CalculationMode.Currency;//Breakeven CalMode
//		public CalculationMode TLSLCalculationMode = CalculationMode.Ticks;//Trailing SL CalMode
		
//		public double DailyLossLmt = -200;//-300 the daily loss limit amount
//		public double ProfitFactor = 0.5;//PT/SL ratio
		public int MaxQuantity = 1; //Quantity of the total contracts allowed to trade for the strategy

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
			PosAvgPrice = 0;
			PosQuantity = 0;
			MktPosition = MarketPosition.Flat;
			PosUnrealizedPnL = 0;
			barsSinceEnOrd = 0;
			BracketOrder = new BracketOrderBase();
			TradeAction = null;
			
			//TM variables
//			TradeDirection = InstStrategy.TM_TradingDirection;
//			TradeStyle = InstStrategy.TM_TradingStyle;
			
//			enCounterPBBars = InstStrategy.TM_EnCounterPBBars;
//			enTrailing = InstStrategy.TM_EnTrailing;
//			enOffsetPnts = InstStrategy.TM_EnOffsetPnts;
			
//			minutesChkEnOrder = InstStrategy.TM_MinutesChkEnOrder;
//			minutesChkPnL = InstStrategy.TM_MinutesChkPnL;
			
//			barsHoldEnOrd = InstStrategy.TM_BarsHoldEnOrd;
//	        barsSincePTSL = InstStrategy.TM_BarsSincePTSL;
//			barsToCheckPnL = InstStrategy.TM_BarsToCheckPnL;
			
			//MM variables
//			profitTargetAmt = InstStrategy.MM_ProfitTargetAmt;
//			profitTargetTic = InstStrategy.MM_ProfitTgtTic;
//			profitTgtIncTic = InstStrategy.MM_ProfitTgtIncTic;
//			profitLockMinTic = InstStrategy.MM_ProfitLockMinTic;
//			profitLockMaxTic = InstStrategy.MM_ProfitLockMaxTic;
			
//	        stopLossAmt = InstStrategy.MM_StopLossAmt;
//			stopLossTic = InstStrategy.MM_StopLossTic;
//			stopLossIncTic = InstStrategy.MM_StopLossIncTic;

//			breakEvenAmt = InstStrategy.MM_BreakEvenAmt;

//			trailingSLAmt = InstStrategy.MM_TrailingStopLossAmt;			
//			trailingSLTic = InstStrategy.MM_TrailingStopLossTic;
//			traillingSLPercent = InstStrategy.MM_TrailingStopLossPercent;
			
//			trailingPTTic = profitLockMinTic;

//			slTrailing = InstStrategy.MM_SLTrailing;
//			ptTrailing = InstStrategy.MM_PTTrailing;
							
//			PTCalculationMode = InstStrategy.MM_PTCalculationMode;
//			SLCalculationMode = InstStrategy.MM_SLCalculationMode;
//			BECalculationMode = InstStrategy.MM_BECalculationMode;
//			TLSLCalculationMode = InstStrategy.MM_TLSLCalculationMode;
			
//			ProfitFactor = InstStrategy.MM_ProfitFactor;
//			DailyLossLmt = InstStrategy.MM_DailyLossLmt;
			
//			MaxQuantity = InstStrategy.DefaultQuantity;
		}
		
		public void InitParameters(Dictionary<string, object> dict) {
			
		}
		
		//unused, newEntryTrade=newTrade, replaced by InitNewTrade
		public void InitNewEntryTrade() {
			//InitParams();
			//CurrentTradeType = TradeType.Entry;
			BracketOrder.ExitOrderType = EntryExitOrderType.SimpleOCO;
		}

		public void InitNewTLSL() {
			//InitParams();
			//CurrentTradeType = TradeType.Exit;
			BracketOrder.ExitOrderType = EntryExitOrderType.TrailingStopLoss;
			this.TradeAction.TrailingProfitTargetTics = 0;
			switch(InstStrategy.MM_TLSLCalculationMode) {
				case CalculationMode.Currency:
					InstStrategy.MM_TrailingStopLossTic = InstStrategy.GetTicksByCurrency(InstStrategy.MM_TrailingStopLossAmt);
					break;
				
				case CalculationMode.Percent:
					break;
			}
		}
		
		public void UpdateCurPos(Cbi.Position position, double averagePrice, 
			int quantity, Cbi.MarketPosition marketPosition) {
			
				PosAvgPrice = averagePrice;
				PosQuantity = quantity;
				MktPosition = marketPosition;
				PosUnrealizedPnL = position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, InstStrategy.Close[0]);
		}
		
		#endregion
		
		#region Event Handler Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="order"></param>
		/// <param name="limitPrice"></param>
		/// <param name="stopPrice"></param>
		/// <param name="quantity"></param>
		/// <param name="filled"></param>
		/// <param name="averageFillPrice"></param>
		/// <param name="orderState"></param>
		/// <param name="time"></param>
		/// <param name="error"></param>
		/// <param name="comment"></param>
		public virtual void OnCurOrderUpdate(Cbi.Order order, double limitPrice, double stopPrice, 
			int quantity, int filled, double averageFillPrice, 
			Cbi.OrderState orderState, DateTime time, Cbi.ErrorCode error, string comment)
		{
			InstStrategy.IndicatorProxy.PrintLog(true, InstStrategy.IsLiveTrading(),
				String.Format("{0}:OnCurOrderUpdate, limitPrice={1}, stopPrice={2}, quantity={3},\t\n filled={4}, averageFillPrice={5}, orderState={6}",
				InstStrategy.CurrentBar, limitPrice, stopPrice, quantity, filled, averageFillPrice, orderState));
			try {
//			    if (order.Name == "myEntryOrder" && orderState != OrderState.Filled)
//			      entryOrder = order;

			 
//			    if (entryOrder != null && entryOrder == order)
//			    {
//			        Print(order.ToString());
//			        if (order.OrderState == OrderState.Filled)
//			              entryOrder = null;
//			    }
			} catch(Exception ex) {
				InstStrategy.Print("Exception=" + ex.StackTrace);
			}
		}
		
		/// <summary>
		/// NewTrade: Setup EnOrder
		/// EndTrade: Clean up En, SL, PT orders 
		/// </summary>
		/// <param name="execution"></param>
		/// <param name="executionId"></param>
		/// <param name="price"></param>
		/// <param name="quantity"></param>
		/// <param name="marketPosition"></param>
		/// <param name="orderId"></param>
		/// <param name="time"></param>
		public virtual void OnCurExecutionUpdate(Execution execution, string executionId,
			double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
			try {
				InstStrategy.IndicatorProxy.PrintLog(true, InstStrategy.IsLiveTrading(), 
				InstStrategy.CurrentBar + ":OnCurExecutionUpdate"
				+ ";executionId=" + executionId
				+ ";orderId=" + orderId
				+ ";execution.Order.Name=" + execution.Order.Name
				+ ";marketPosition=" + marketPosition
				+ ";quantity=" + quantity
				+ ";price=" + price
				);
				PositionStatus ps = InstStrategy.GetPositionStatus(PosQuantity);
				switch(ps) {
					case PositionStatus.NewEstablished://New position created, setup SL/PT
						InstStrategy.IndicatorProxy.PrintLog(true, InstStrategy.IsLiveTrading(),
						String.Format("{0}:OnCurExecutionUpdate PositionStatus.NewEstablished, MktPos={1}, PosQuantity={2}, marketPosition={3}, quantity={4}",
							InstStrategy.CurrentBar, MktPosition, PosQuantity, marketPosition, quantity));
						break;
					case PositionStatus.Liquidate://Positions were closed, trade is done, init a new trade;
						InstStrategy.IndicatorProxy.PrintLog(true, InstStrategy.IsLiveTrading(),
						String.Format("{0}:OnCurExecutionUpdate PositionStatus.Liquidate, MktPos={1}, PosQuantity={2}, marketPosition={3}, quantity={4}",
							InstStrategy.CurrentBar, MktPosition, PosQuantity, marketPosition, quantity));
						//InitNewTrade();
						break;
					case PositionStatus.Hold:
						break;
					case PositionStatus.Flat:
						break;
					case PositionStatus.ScaledIn:
						break;
					case PositionStatus.ScaledOut:
						break;
					case PositionStatus.UnKnown:
						break;						
				}
			} catch(Exception ex) {
				InstStrategy.Print("Exception=" + ex.StackTrace);
			}
		}
		
		/// <summary>
		/// New position created: update position, setup SL/PT orders(the same bar);
		/// Old position closed: Start a new trade;
		/// Position is being held: check performane to adjust SL/PT orders;
		/// All flat: wait for Command/TradeSignal, or detect new position created;
		/// 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="averagePrice"></param>
		/// <param name="quantity">always>0</param>
		/// <param name="marketPosition">flat/long/short</param>
		public virtual void OnCurPositionUpdate(Cbi.Position position, double averagePrice, 
			int quantity, Cbi.MarketPosition marketPosition)
		{
			try	{
				InstStrategy.IndicatorProxy.PrintLog(true, InstStrategy.IsLiveTrading(),
					String.Format("{0}:OnCurPositionUpdate - AvgPrc: {1}, Quant={2}, MktPos={3}, marketPosition={4}, PnL={5}",
						InstStrategy.CurrentBar, PosAvgPrice, PosQuantity, MktPosition, marketPosition, PosUnrealizedPnL));
				//Position pos = position.MemberwiseClone();
				PositionStatus ps = InstStrategy.GetPositionStatus(PosQuantity);
				switch(ps) {
					case PositionStatus.NewEstablished://New position created, setup SL/PT(the same bar)
						InstStrategy.IndicatorProxy.PrintLog(true, InstStrategy.IsLiveTrading(),
						String.Format("{0}:PositionStatus.NewEstablished, MktPos={1}, PosQuantity={2}, marketPosition={3}, quantity={4}",
							InstStrategy.CurrentBar, MktPosition, PosQuantity, marketPosition, quantity));
						UpdateCurPos(position, averagePrice, quantity, marketPosition);
						InstStrategy.CheckExitSignals();
						InstStrategy.SetExitTradeAction();
						InstStrategy.TakeTradeAction();
						break;
					case PositionStatus.Liquidate://Positions were closed, trade is done, init a new trade;
						InstStrategy.IndicatorProxy.PrintLog(true, InstStrategy.IsLiveTrading(),
						String.Format("{0}:PositionStatus.Liquidate, MktPos={1}, PosQuantity={2}, marketPosition={3}, quantity={4}",
							InstStrategy.CurrentBar, MktPosition, PosQuantity, marketPosition, quantity));
						InitNewTrade();
						break;
					case PositionStatus.Hold://Position is held, change SL/PT by rule/performance, scale in/out occured;
						//=> this case is replaced by CheckPerformance() from OnBarUpdate, it won't happen here, only scale in/out will happen
						InstStrategy.IndicatorProxy.PrintLog(true, InstStrategy.IsLiveTrading(),
							String.Format("{0}:Position is Held, MktPos={1}, marketPosition={2}, PosQuant={3}, Quant={4}", 
							InstStrategy.CurrentBar, MktPosition, marketPosition, PosQuantity, quantity));
//						InstStrategy.CalProfitTargetAmt(PosAvgPrice, InstStrategy.MM_ProfitFactor);
//						InstStrategy.CalExitOcoPrice(PosAvgPrice, InstStrategy.MM_ProfitFactor);
//						InstStrategy.SetSimpleExitOCO(TradeAction.EntrySignal.SignalName);
						break;
					case PositionStatus.Flat:
						break;
					case PositionStatus.ScaledIn:
						InstStrategy.IndicatorProxy.PrintLog(true, InstStrategy.IsLiveTrading(),
							String.Format("{0}:PositionStatus.ScaledIn, MktPos={1}, marketPosition={2}, PosQuant={3}, Quant={4}", 
							InstStrategy.CurrentBar, MktPosition, marketPosition, PosQuantity, quantity));
						break;
					case PositionStatus.ScaledOut:
						InstStrategy.IndicatorProxy.PrintLog(true, InstStrategy.IsLiveTrading(),
							String.Format("{0}:PositionStatus.ScaledOut, MktPos={1}, marketPosition={2}, PosQuant={3}, Quant={4}", 
							InstStrategy.CurrentBar, MktPosition, marketPosition, PosQuantity, quantity));
						break;
					case PositionStatus.UnKnown:
						break;						
				}
				/*
				if (MktPosition == MarketPosition.Flat)
				{
					if(marketPosition == MarketPosition.Flat) {
						InstStrategy.IndicatorProxy.PrintLog(true, InstStrategy.IsLiveTrading(),
						String.Format("{0}:NoChangeOnPosition, MktPos={1}, marketPosition={2}",
							InstStrategy.CurrentBar, MktPosition, marketPosition));
					} else { //New position created, setup SL/PT
						InstStrategy.IndicatorProxy.PrintLog(true, InstStrategy.IsLiveTrading(),
						String.Format("{0}:NewOnPosition, MktPos={1}, marketPosition={2}",
							InstStrategy.CurrentBar, MktPosition, marketPosition));
					}
					//TradeAction.TrailingProfitTargetTics = InstStrategy.GetTicksByCurrency(TradeAction.profitTargetAmt);
					//trailingSLTic = GetTicksByCurrency(CurrentTrade.stopLossAmt);
				}
				else if (marketPosition == MarketPosition.Flat) { //Positions were closed, trade is done, init a new trade;
					InstStrategy.IndicatorProxy.PrintLog(true, InstStrategy.IsLiveTrading(), "InitNewTrade called");
					InitNewTrade();
				}
				else //Position is held, change SL/PT by rule/performance, scale in/out occured;
				{
					InstStrategy.IndicatorProxy.PrintLog(true, InstStrategy.IsLiveTrading(),
						String.Format("{0}:Position is Held, MktPos={1}, marketPosition={2}, PosQuant={3}, Quant={4}", 
						InstStrategy.CurrentBar, MktPosition, marketPosition, PosQuantity, quantity));
					InstStrategy.CalProfitTargetAmt(PosAvgPrice, InstStrategy.MM_ProfitFactor);
					InstStrategy.CalExitOcoPrice(PosAvgPrice, InstStrategy.MM_ProfitFactor);
					InstStrategy.SetSimpleExitOCO(TradeAction.EntrySignal.SignalName);
	//				SetBracketOrder.OCOOrder.ProfitTargetOrder(OrderSignalName.EntryShort.ToString());
	//				SetBracketOrder.OCOOrder.StopLossOrder(OrderSignalName.EntryShort.ToString());
				}
				*/
			} catch(Exception ex) {
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

		[Description("The ocoID for BracketOrder exit leg")]
		[Browsable(false), XmlIgnore]
		public string OcoID
		{
			get {
				if(TradeID != null) {
					string[] str = TradeID.Split('-');
					return "OCO-" + str[1];
				} else
					return String.Empty;
			}
		}
		
		[Browsable(false), XmlIgnore]
		public BracketOrderBase BracketOrder
		{
			get { return bracketOrder; }
			set { bracketOrder = value; }
		}

//		[Browsable(false), XmlIgnore]
//		public TrailingSLOrderBase TrailingSLOrder
//		{
//			get { return trailingSLOrder; }
//			set { trailingSLOrder = value; }
//		}
		
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
			get; set;
		}
		
		//private GStrategyBase InstStrategy = null;		
		//Order Objects
		private BracketOrderBase bracketOrder = new BracketOrderBase();
		//private TrailingSLOrderBase trailingSLOrder = new TrailingSLOrderBase();
//		public EntryExitOrderType BracketOrder.ExitOrderType = EntryExitOrderType.SimpleOCO;		
		#endregion
	}
}
