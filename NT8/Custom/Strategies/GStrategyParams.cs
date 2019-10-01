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
	[Gui.CategoryOrder(GPS_CUSTOM_PARAMS, 1)] // "CustomParams", display "CP" first
<<<<<<< HEAD
	//[Gui.CategoryOrder(GPS_GSTRATEGY, 2)] // then "GStrategy"
	[Gui.CategoryOrder(GPS_NJSCRIPTST_PARAMS, 2)] //"NinjaScriptStrategyParameters"	
=======
	[Gui.CategoryOrder(GPS_GSTRATEGY, 2)] // then "GStrategy"
>>>>>>> 7b0fc315bf601b4b69a10d4879094eef2ea42503
	[Gui.CategoryOrder(GPS_MONEY_MGMT, 3)] // then "MoneyMgmt"
	[Gui.CategoryOrder(GPS_TRADE_MGMT, 4)] // and then "TradeMgmt"
	[Gui.CategoryOrder(GPS_TRIGGER, 5)] // and then "Trigger"
	[Gui.CategoryOrder(GPS_OUTPUT, 6)] // and finally "Output"
<<<<<<< HEAD
=======
	[Gui.CategoryOrder(GPS_NJSTSCRIPT_PARAMS, 7)] //"NinjaScriptStrategyParameters"
>>>>>>> 7b0fc315bf601b4b69a10d4879094eef2ea42503
	
	public partial class GStrategyBase : Strategy
	{
		#region CustomParams
		/// <summary>
		/// The group for individual indicators parameters
		/// The order for each entry is defined in the class of individual indicator 
		/// </summary>
		public const string GPS_CUSTOM_PARAMS = "CustomParams";
		#endregion
<<<<<<< HEAD

		
		#region NinjaScriptStrategyParameters
		public const string GPS_NJSCRIPTST_PARAMS = "NinjaScriptStrategyParameters";

=======
		
		#region GStrategy
		public const string GPS_GSTRATEGY = "GStrategy";
>>>>>>> 7b0fc315bf601b4b69a10d4879094eef2ea42503
		public const int ODG_AccName = 1;
		public const int ODG_AlgoMode = 2;
		public const int ODG_BackTest = 3;		
		public const int ODG_PrintOut = 4;		
		public const int ODG_StartH = 5;		
//		public const int ODG_ = ;		
		
		#endregion
		
		#region MoneyMgmt
		public const string GPS_MONEY_MGMT = "MoneyMgmt";
<<<<<<< HEAD
		
=======
>>>>>>> 7b0fc315bf601b4b69a10d4879094eef2ea42503
		public const int ODG_ProfitTgtAmt = 1;
		public const int ODG_ProfitTgtTic = 2;
		public const int ODG_ProfitTgtIncTic = 3;
		public const int ODG_ProfitLockMinTic = 4;
		public const int ODG_ProfitLockMaxTic = 5;
		public const int ODG_StopLossAmt = 6;
		public const int ODG_StopLossTic = 7;
		public const int ODG_StopLossIncTic = 8;
		public const int ODG_BreakEvenAmt = 9;
		public const int ODG_TrailingStopLossAmt = 10;
		public const int ODG_TrailingStopLossTic = 11;
		public const int ODG_TrailingStopLossPercent = 12;
		public const int ODG_SLTrailing = 13;
		public const int ODG_PTTrailing = 14;
		public const int ODG_PTCalculationMode = 15;
		public const int ODG_SLCalculationMode = 16;
		public const int ODG_BECalculationMode = 17;
		public const int ODG_TLSLCalculationMode = 18;
		public const int ODG_DailyLossLmt = 19;
		public const int ODG_ProfitFactor = 20;
//		public const int ODG_ = ;

		#endregion
		
		#region TradeMgmt
		public const string GPS_TRADE_MGMT = "TradeMgmt";
<<<<<<< HEAD
		
=======
>>>>>>> 7b0fc315bf601b4b69a10d4879094eef2ea42503
		public const int ODG_TradingDirection = 1;
		public const int ODG_TradingStyle = 2;
		public const int ODG_EnTrailing = 3;
		public const int ODG_EnOffsetPnts = 4;
		public const int ODG_MinutesChkEnOrder = 5;
		public const int ODG_MinutesChkPnL = 6;
		public const int ODG_BarsToCheckPL = 7;
		public const int ODG_BarsHoldEnOrd = 8;
		public const int ODG_EnCounterPBBars = 9;
		public const int ODG_BarsSincePTSL = 10;
//		public const int ODG_ = ;
		
		#endregion
		
		#region Trigger
		public const string GPS_TRIGGER = "Trigger";
<<<<<<< HEAD
		
=======
>>>>>>> 7b0fc315bf601b4b69a10d4879094eef2ea42503
		public const int ODG_EnSwingMinPnts = 1;
		public const int ODG_EnSwingMaxPnts = 2;
		public const int ODG_EnPullbackMinPnts = 3;
		public const int ODG_EnPullbackMaxPnts = 4;
		public const int ODG_TimeStartH = 5;
		public const int ODG_TimeStartM = 6;
		public const int ODG_TimeEndH = 7;
		public const int ODG_TimeEndM = 8;
		public const int ODG_TimeLiqH = 9;
		public const int ODG_TimeLiqM = 10;
		public const int ODG_OpenStartH = 11;
		public const int ODG_OpenStartM = 12;
		public const int ODG_OpenEndH = 13;
		public const int ODG_OpenEndM = 14;
//		public const int ODG_ = ;
		
		#endregion
		
		#region Output
		public const string GPS_OUTPUT = "Output";
//		public const int ODG_ = ;
		
<<<<<<< HEAD
=======
		#region NinjaScriptParameters
		public const string GPS_NJSTSCRIPT_PARAMS = "NinjaScriptStrategyParameters";
		#endregion
		
>>>>>>> 7b0fc315bf601b4b69a10d4879094eef2ea42503
		#endregion		
	}
}
