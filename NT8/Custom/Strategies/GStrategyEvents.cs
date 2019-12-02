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
