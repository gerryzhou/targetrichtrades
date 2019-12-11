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
		protected override void OnBarUpdate()
		{
			indicatorProxy.TraceMessage(this.Name, PrintOut);
			//Print(CurrentBar.ToString() + " -- GSZTraderBase - Add your custom strategy logic here.");
			if(CurrentBar <= BarsRequiredToTrade)
				return;
			int bsx = BarsSinceExitExecution(0, "", 0);
			int bse = BarsSinceEntryExecution(0, "", 0);
			SetPrintOut(-1);
			//Print(CurrentBar + ":" + this.Name + " OnBarUpdate, BarsSinceExit, BarsSinceEntry=" + bsx + "," + bse);
			indicatorProxy.TraceMessage(this.Name, PrintOut);
			indicatorProxy.Update();
			indicatorProxy.TraceMessage(this.Name, PrintOut);
			CheckPerformance();
			//double gap = GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Cyan).GetCurZZGap();
			//bool isReversalBar = true;//CurrentBar>BarsRequired?false:GIParabolicSAR(0.002, 0.2, 0.002, AccName, Color.Cyan).IsReversalBar();
			indicatorProxy.TraceMessage(this.Name, PrintOut);			

			switch(AlgoMode) {
				case AlgoModeType.Liquidate: //liquidate
					indicatorProxy.TraceMessage(this.Name, PrintOut);
					CloseAllPositions();
					break;
				case AlgoModeType.Trading: //trading
					//SetTradeAction(); called from CheckExitTrade() or CheckNewEntryTrade();
					//CheckIndicatorSignals(); called from SetTradeAction(); save trade signals into the trade action;
					//PutTrade(); first GetTradeAction() and then put exit or entry trade;
					indicatorProxy.TraceMessage(this.Name, PrintOut);
					if(CheckTradeSignals()) {
						PutTrade();
					}
					break;
				case AlgoModeType.CancelOrders: //cancel order
					indicatorProxy.TraceMessage(this.Name, PrintOut);
					CancelAllOrders();
					break;
				case AlgoModeType.StopTrading: //stop trading
					indicatorProxy.TraceMessage(this.Name, PrintOut);
					indicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + "- Stop trading cmd:" + indicatorProxy.Get24HDateTime(Time[0]));
					break;
			}
		}		
		#endregion
		
		/// <summary>
		/// Only updated on live/sim trading, not triggered at back-testing
		/// </summary>
		/// <param name="account"></param>
		/// <param name="accountItem"></param>
		/// <param name="value"></param>
		protected override void OnAccountItemUpdate(Cbi.Account account, Cbi.AccountItem accountItem, double value)
		{
			if(accountItem == AccountItem.UnrealizedProfitLoss)
				indicatorProxy.PrintLog(true, IsLiveTrading(), 
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
	}
}
