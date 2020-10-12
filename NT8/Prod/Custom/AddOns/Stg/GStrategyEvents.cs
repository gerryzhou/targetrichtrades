#region Using declarations
using System;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class GStrategyBase : Strategy
	{
		#region OnBarUpdate Function
		/// <summary>
		/// The first event handler for each bar;
		/// Other handlers like OnOrderUpdate, OnExecutionUpdate, OnPositionUpdate,
		/// are used to setup status for CurrentTrade, the TradeAction will be taken
		/// on the next bar(CurrentBar+1) at OnBarUpdate;
		/// The command and performance triggerred TradeAction can be taken at the 
		/// same bar at PutTrade();
		/// </summary>
		protected override void OnBarUpdate()
		{
			if(PrintOut > 1 && !IsInStrategyAnalyzer) {
				IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				String.Format("{0}:===========OnBarUpdate======HasPosition={1}, IsLiveTrading={2}",
				CurrentBar, HasPosition(), IsLiveTrading()));
				IndicatorProxy.TraceMessage(this.Name, PrintOut);
			}
			//Print(CurrentBar.ToString() + " -- GSZTraderBase - Add your custom strategy logic here.");
			if(CurrentBar <= BarsRequiredToTrade)
				return;
			int bsx = BarsSinceExitExecution(0, "", 0);
			int bse = BarsSinceEntryExecution(0, "", 0);
			SetPrintOut(-1);
			//Print(CurrentBar + ":" + this.Name + " OnBarUpdate, BarsSinceExit, BarsSinceEntry=" + bsx + "," + bse);
			if(PrintOut > 1 && !IsInStrategyAnalyzer)
				IndicatorProxy.TraceMessage(this.Name, PrintOut);
			IndicatorProxy.Update();
			if(PrintOut > 1 && !IsInStrategyAnalyzer)
				IndicatorProxy.TraceMessage(this.Name, PrintOut);
			CheckCmd(); //Command trigger
			
			switch(AlgoMode) {
				case AlgoModeType.Liquidate: //liquidate
					if(PrintOut > 1 && !IsInStrategyAnalyzer)
						IndicatorProxy.TraceMessage(this.Name, PrintOut);
					CloseAllPositions();
					break;
				case AlgoModeType.CancelOrders: //cancel order
					if(PrintOut > 1 && !IsInStrategyAnalyzer)
						IndicatorProxy.TraceMessage(this.Name, PrintOut);
					CancelAllOrders();
					break;
				case AlgoModeType.StopTrading: // -2=stop trading(no entry/exit, liquidate positions and cancel all entry/exit orders);
					CancelAllOrders();
					CloseAllPositions();
					if(PrintOut > 1 && !IsInStrategyAnalyzer) {
						IndicatorProxy.TraceMessage(this.Name, PrintOut);
						IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + "- Stop trading cmd:" + IndicatorProxy.Get24HDateTime(Time[0]));
					}
					break;
				case AlgoModeType.ExitOnly: // -1=stop trading(no entry/exit, cancel entry orders and keep the exit order as it is if there has position);
					CancelEntryOrders();
					break;
				case AlgoModeType.Trading: //trading
					//SetTradeAction(); called from CheckExitTrade() or CheckNewEntryTrade();
					//CheckIndicatorSignals(); called from SetTradeAction(); save trade signals into the trade action;
					//PutTrade(); first GetTradeAction() and then put exit or entry trade;
					if(PrintOut > 1 && !IsInStrategyAnalyzer)
						IndicatorProxy.TraceMessage(this.Name, PrintOut);
					//CheckPerformance(); //Performance/Rule trigger					
					//SetTradeAction();
					if(HasPosition() != 0)
						CheckPerformance();
					//Produce trade signals from indicator indicator signals
					CheckIndicatorSignals();
					//Set trade action replaced by the event hanlder from indicator signals
					SetTradeAction();
					//PutTrade();
					TakeTradeAction();
					break;
				case AlgoModeType.SemiAlgo:	// 2=semi-algo(manual entry, algo exit);
					if(HasPosition() != 0) {
						CheckPerformance(); //Performance/Rule trigger
						//ChangeSLPT(); //re-implement to fit TradeAction process
						//Produce trade signals from indicator indicator signals
						CheckIndicatorSignals();
						SetTradeAction();
						//PutTrade();
						TakeTradeAction();
					}
					break;
			}

			if(PrintOut > 1 && !IsInStrategyAnalyzer)
				IndicatorProxy.TraceMessage(this.Name, PrintOut);
		}		
		#endregion
		
		#region OnOrderUpdate Function
		/// <summary>
		/// OnOrderUpdate->OnExecutionUpdate->OnPositionUpdate
		/// OnPositionUpdate: check new trade, change position for curTrade, no order update;
		/// OnExecutionUpdate: check filled order, update the new entry order and liquidate order; 
		/// OnOrderUpdate: deal with working order, rejected/cancelled orders;
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
		protected override void OnOrderUpdate(Cbi.Order order, double limitPrice, double stopPrice, 
			int quantity, int filled, double averageFillPrice, 
			Cbi.OrderState orderState, DateTime time, Cbi.ErrorCode error, string comment)
		{
			//if(BarsInProgress !=0) return;
			IndicatorProxy.Log2Disk = true;

			if(PrintOut > 1 && !IsInStrategyAnalyzer)
				IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				CurrentBars[BarsInProgress] + "[" + BarsInProgress + "]:OnOrderUpdate IsUnmanaged=" + IsUnmanaged);
			
			//The logic is implemented in the method below
			CurrentTrade.OnCurOrderUpdate(order, limitPrice, stopPrice, quantity, filled, averageFillPrice, 
				orderState, time, error, comment);
			return;
			
			//The order execution is implemented in the method below
			if(IsUnmanaged)
				OnOrderUpdateUM(order, limitPrice, stopPrice, quantity, filled, 
				averageFillPrice, orderState, time, error, comment);
			else
				OnOrderUpdateMG(order, limitPrice, stopPrice, quantity, filled, 
				averageFillPrice, orderState, time, error, comment);
		}
		#endregion
		
		#region OnExecutionUpdate Function
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition,
			string orderId, DateTime time)
		{
			if(BarsInProgress !=0) return;
			IndicatorProxy.Log2Disk = true;
			if(PrintOut > 1 && !IsInStrategyAnalyzer)
				IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				CurrentBar + ":OnExecutionUpdate"
				+ ";IsUnmanaged=" + IsUnmanaged
				+ ";IsLiveTrading=" + IsLiveTrading()
				+ ";IsInitialEntry=" + execution.IsInitialEntry
				+ ";IsEntry=" + execution.IsEntry
				+ ";IsExit=" + execution.IsExit
				+ ";IsLastExit=" + execution.IsLastExit
				+ ";GetMarketPosition=" + GetMarketPosition()
				+ ";marketPosition=" + marketPosition
				+ ";quantity=" + quantity
				+ ";HasPosition=" + HasPosition()
				+ ";GetAvgPrice=" + GetAvgPrice()
				+ ";price=" + price);
			
			CurrentTrade.OnCurExecutionUpdate(execution, executionId, price, quantity, marketPosition, orderId, time);
			return;
			if(IsUnmanaged)
				OnExecutionUpdateUM(execution, executionId, price, quantity, marketPosition, orderId, time);
			else
				OnExecutionUpdateMG(execution, executionId, price, quantity, marketPosition, orderId, time);
		}
		#endregion
		
		#region OnData/AccountUpdate Functions
		/// <summary>
		/// Only updated on live/sim trading, not triggered at back-testing;
		/// The evernt posted tick by tick at sim/living trading with poistion hold;
		/// </summary>
		/// <param name="account"></param>
		/// <param name="accountItem"></param>
		/// <param name="value"></param>
		protected override void OnAccountItemUpdate(Cbi.Account account, Cbi.AccountItem accountItem, double value)
		{
			if (account == null || accountItem == null || IndicatorProxy == null) 
				return;
			
			if(accountItem == AccountItem.UnrealizedProfitLoss && PrintOut > 1 && !IsInStrategyAnalyzer)
				IndicatorProxy.PrintLog(true, IsLiveTrading(), //":OnAccountItemUpdate"
					CurrentBar + ":OnAccountItemUpdate"
					+ ";Name=" + account.DisplayName
					+ ";Item=" + accountItem.ToString()
					+ ";value=" + value
					+ ";DailyLossLmt=" + account.DailyLossLimit
					+ ";Status=" + account.AccountStatus.ToString()
					);
		}

		protected override void OnConnectionStatusUpdate(ConnectionStatusEventArgs connectionStatusUpdate)
		{
			
		}

		protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
		{
			
		}

		protected override void OnMarketDepth(MarketDepthEventArgs marketDepthUpdate)
		{
			
		}
		#endregion

		#region Custom Event Handler
        // Declare the event using EventHandler<T>
        public event EventHandler<StrategyEventArgs> RaiseStrategyEvent;

        public virtual void FireEvent(StrategyEventArgs e)
        {
            // Write some code that does something useful here
            // then raise the event. You can also raise an event
            // before you execute a block of code.
            //OnRaiseCustomEvent(new IndicatorEventArgs(this.GetType().Name, " did something: "));
			OnRaiseStrategyEvent(e);
        }

        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void OnRaiseStrategyEvent(StrategyEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<StrategyEventArgs> handler = RaiseStrategyEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Format the string to send inside the CustomEventArgs parameter
                e.Message += String.Format("Hello, at {0:HH:mm} now.", DateTime.Now); //$" at {DateTime.Now}"; available at C# 6

                // Use the () operator to raise the event.
                handler(this, e);
            }
        }
		#endregion
	}
	
    // Define a class to hold strategy event info
    public class StrategyEventArgs : EventArgs
    {
        public StrategyEventArgs(string name, string msg)
        {
			EventName = name;
            Message = msg;
        }

        public string Message
        {
            get; set;
        }
		
        public string EventName
        {
            get; set;
        }
		
		public TradeSignal TdSignal
        {
            get; set;
        }
    }
}
