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

	/// AlgoMode: 0=liquidate; 
	/// 1=trading; 
	/// 2=semi-algo(manual entry, algo exit);
	/// -1=ExitOnly(no entry/exit, cancel entry orders and keep the exit order as it is if there has position);
	/// -2=Stop trading(no entry/exit, liquidate positions and cancel all entry/exit orders);
	/// -3=Cancel Orders(no entry/exit, cancel all orders)
	public enum AlgoModeType {Trading=1, Liquidate=0, SemiAlgo=2, ExitOnly=-1, StopTrading=-2, CancelOrders=-3};
	//public enum SessionBreak {AfternoonClose, EveningOpen, MorningOpen, NextDay};
	public enum TradingDirection {Up=1, Down=-1, Both=0};
	
	public enum TradingStyle {TrendFollowing=1, CounterTrend=-1, Ranging=0};
	
	public enum TradeType {Entry=1, Exit=-1, Liquidate=-2, Reverse=2, NoTrade=0};
	
	public enum OrderSignalName {EntryLongLmt, EntryShortLmt, EntryLongMkt, EntryShortMkt, EntryLongStopMkt, EntryShortStopMkt, 
		ExitLong, ExitShort, Profittarget, Stoploss, Trailstop, UnKnown};
	
	public enum BracketOrderSubType {Entry, ProfitTarget, StoppLoss, UnKnown};
	
	public enum MoneyMgmtPolicy {SimpleSLPT, TrailingSLPT, PositionScaleInOut};
}
