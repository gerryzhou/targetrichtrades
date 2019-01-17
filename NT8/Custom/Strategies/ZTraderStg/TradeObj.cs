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
		private Strategy instStrategy = null;
		private TradeType tradeType = TradeType.NoTrade;
		public TradingDirection tradeDirection = TradingDirection.Both;
		public TradingStyle tradeStyle = TradingStyle.TrendFollowing;
		
		#region Money Mgmt variables
		public double profitTargetAmt = 350; //36 Default(450-650 USD) setting for MM_ProfitTargetAmt
		public double profitTgtIncTic = 6; //8 Default tick Amt for ProfitTarget increase Amt
		public double profitLockMinTic = 16; //24 Default ticks Amt for Min Profit locking
		public double profitLockMaxTic = 30; //80 Default ticks Amt for Max Profit locking
        public double stopLossAmt = 200; //16 Default setting for stopLossAmt
		public double stopLossIncTic = 4; //4 Default tick Amt for StopLoss increase Amt
		public double breakEvenAmt = 150; //150 the profits amount to trigger setting breakeven order
		public double trailingSLAmt = 100; //300 Default setting for trailing Stop Loss Amt
		public double dailyLossLmt = -200; //-300 the daily loss limit amount

		public bool enTrailing = true; //use trailing entry: counter pullback bars or simple enOffsetPnts
		public bool ptTrailing = true; //use trailing profit target every bar
		public bool slTrailing = true; //use trailing stop loss every bar		
		#endregion
		
		#region Trade Mgmt variables
		public double enOffsetPnts = 1.25;//Price offset for entry
		public int enCounterPBBars = 1;//Bar count of pullback for breakout entry setup
		
		public int minutesChkEnOrder = 20; //how long before checking an entry order filled or not
		public int minutesChkPnL = 30; //how long before checking P&L
		
		public int barsHoldEnOrd = 10; // Bars count since en order was issued
        public int barsSincePtSl = 1; // Bar count since last P&L was filled
		public int barsToCheckPL = 2; // Bar count to check P&L since the entry		
		#endregion
		
		#region Order Objects
		public Order entryOrder = null;
		public Order profitTargetOrder = null;
		public Order stopLossOrder = null;
		public double trailingPTTic = 36; //400, tick amount of trailing target
		public double trailingSLTic = 16; // 200, tick amount of trailing stop loss
		public int barsSinceEnOrd = 0; // bar count since the en order issued
		
		#endregion
		
		public TradeObj(Strategy inst_strategy) {
			this.instStrategy = inst_strategy;
		}
		
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
