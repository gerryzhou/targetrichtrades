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
			IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				String.Format("{0}:===========OnBarUpdate============", CurrentBar));
			IndicatorProxy.TraceMessage(this.Name, PrintOut);
			//Print(CurrentBar.ToString() + " -- GSZTraderBase - Add your custom strategy logic here.");
			if(CurrentBar <= BarsRequiredToTrade)
				return;
			int bsx = BarsSinceExitExecution(0, "", 0);
			int bse = BarsSinceEntryExecution(0, "", 0);
			SetPrintOut(-1);
			//Print(CurrentBar + ":" + this.Name + " OnBarUpdate, BarsSinceExit, BarsSinceEntry=" + bsx + "," + bse);
			IndicatorProxy.TraceMessage(this.Name, PrintOut);
			IndicatorProxy.Update();
			IndicatorProxy.TraceMessage(this.Name, PrintOut);
			CheckCmd(); //Command trigger
			
			//double gap = GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Cyan).GetCurZZGap();
			//bool isReversalBar = true;//CurrentBar>BarsRequired?false:GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Cyan).IsReversalBar();
			IndicatorProxy.TraceMessage(this.Name, PrintOut);			

			PutTrade();
		}		
		#endregion
		
		#region OnExecutionUpdate Function
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition,
			string orderId, DateTime time)
		{
			if(BarsInProgress !=0) return;
			IndicatorProxy.Log2Disk = true;
			Print(CurrentBar + ":OnExecutionUpdate"
			+ ";IsUnmanaged=" + IsUnmanaged
			+ ";IsLiveTrading=" + IsLiveTrading()
			+ ";GetMarketPosition=" + GetMarketPosition()
			+ ";marketPosition=" + marketPosition
			+ ";quantity=" + quantity
			+ ";HasPosition=" + HasPosition()
			+ ";GetAvgPrice=" + GetAvgPrice()
			+ ";price=" + price);
			CurrentTrade.OnCurExecutionUpdate(execution, executionId, price, quantity, marketPosition, orderId, time);
			
			if(IsUnmanaged)
				OnExecutionUpdateUM(execution, executionId, price, quantity, marketPosition, orderId, time);
			else
				OnExecutionUpdateMG(execution, executionId, price, quantity, marketPosition, orderId, time);
		}
		#endregion
		
		#region OnOrderUpdate Function
		protected override void OnOrderUpdate(Cbi.Order order, double limitPrice, double stopPrice, 
			int quantity, int filled, double averageFillPrice, 
			Cbi.OrderState orderState, DateTime time, Cbi.ErrorCode error, string comment)
		{
			if(BarsInProgress !=0) return;
			IndicatorProxy.Log2Disk = true;
			
			Print(CurrentBar + ":OnOrderUpdate IsUnmanaged=" + IsUnmanaged);
			CurrentTrade.OnCurOrderUpdate(order, limitPrice, stopPrice, quantity, filled, averageFillPrice, 
				orderState, time, error, comment);
			
			if(IsUnmanaged)
				OnOrderUpdateUM(order, limitPrice, stopPrice, quantity, filled, 
				averageFillPrice, orderState, time, error, comment);
			else
				OnOrderUpdateMG(order, limitPrice, stopPrice, quantity, filled, 
				averageFillPrice, orderState, time, error, comment);
		}
		#endregion
		
		#region OnData/AccountUpdate Functions
		/// <summary>
		/// Only updated on live/sim trading, not triggered at back-testing
		/// </summary>
		/// <param name="account"></param>
		/// <param name="accountItem"></param>
		/// <param name="value"></param>
		protected override void OnAccountItemUpdate(Cbi.Account account, Cbi.AccountItem accountItem, double value)
		{
			if(accountItem == AccountItem.UnrealizedProfitLoss)
				IndicatorProxy.PrintLog(true, IsLiveTrading(), 
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
	}
}
