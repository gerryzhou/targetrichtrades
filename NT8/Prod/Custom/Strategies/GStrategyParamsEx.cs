#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Xml.Serialization;

using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Strategies.ZTraderStg;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	[Gui.CategoryOrder(GPS_CUSTOM_PARAMS, 1)] // "CustomParams", display "CP" first
	[Gui.CategoryOrder(GPS_GSTRATEGY, 2)] // then "GStrategy"
//	[Gui.CategoryOrder(GPS_NJSCRIPTST_PARAMS, 2)] //"NinjaScriptStrategyParameters"	
	[Gui.CategoryOrder(GPS_MONEY_MGMT, 3)] // then "MoneyMgmt"
	[Gui.CategoryOrder(GPS_TRADE_MGMT,4)] // and then "TradeMgmt"
	[Gui.CategoryOrder(GPS_TRIGGER, 5)] // and then "Trigger"
	[Gui.CategoryOrder(GPS_OUTPUT, 6)] // and finally "Output"
	
	public partial class GStrategyBaseEx : Strategy
	{
		#region CustomParams
		/// <summary>
		/// The group for individual indicators parameters
		/// The order for each entry is defined in the class of individual indicator
		/// </summary>
		public const string GPS_CUSTOM_PARAMS = "CustomParams";
		#endregion
		
		#region NinjaScriptStrategyParameters
		public const string GPS_GSTRATEGY = "NinjaScriptStrategyParameters";

		public const int ODG_AccName = 1;
		public const int ODG_AlgoMode = 2;
		public const int ODG_BackTest = 3;
		public const int ODG_LiveModelUpdate = 4;
		public const int ODG_ModelUpdateURL = 5;
		public const int ODG_PrintOut = 6;
		public const int ODG_StartH = 7;
//		public const int ODG_ = ;
		
		#endregion
		
		#region MoneyMgmt
		public const string GPS_MONEY_MGMT = "MoneyMgmt";
		
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
		public const int ODG_ProfitFactorMin = 20;
		public const int ODG_ProfitFactorMax = 21;
		public const int ODG_SLPriceGapPref = 22;
		public const int ODG_PTPriceGapPref = 23;
//		public const int ODG_ = ;

		#endregion
		
		#region MM Properties
        [Description("Money amount of profit target")]
 		[Range(0, double.MaxValue), NinjaScriptProperty, XmlIgnore]		
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitTgtAmt", GroupName = GPS_MONEY_MGMT, Order = ODG_ProfitTgtAmt)]	
        public double MM_ProfitTargetAmt
        {
            get{return mm_ProfitTargetAmt;}
            set{mm_ProfitTargetAmt = Math.Max(0, value);}
        }

        [Description("Ticks amount for profit target")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitTgtTic", GroupName = GPS_MONEY_MGMT, Order = ODG_ProfitTgtTic)]	
        public int MM_ProfitTgtTic
        {
            get{return mm_ProfitTgtTic;}
            set{mm_ProfitTgtTic = Math.Max(0, value);}
        }
		
        [Description("Money amount for profit target increasement")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitTgtIncTic", GroupName = GPS_MONEY_MGMT, Order = ODG_ProfitTgtIncTic)]	
        public int MM_ProfitTgtIncTic
        {
            get{return mm_ProfitTgtIncTic;}
            set{mm_ProfitTgtIncTic = Math.Max(0, value);}
        }
		
        [Description("Tick amount for min profit locking")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitLockMinTic", GroupName = GPS_MONEY_MGMT, Order = ODG_ProfitLockMinTic)]	
        public int MM_ProfitLockMinTic
        {
            get{return mm_ProfitLockMinTic;}
            set{mm_ProfitLockMinTic = Math.Max(0, value);}
        }

		[Description("Tick amount for max profit locking")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitLockMaxTic", GroupName = GPS_MONEY_MGMT, Order = ODG_ProfitLockMaxTic)]	
        public int MM_ProfitLockMaxTic
        {
            get{return mm_ProfitLockMaxTic;}
            set{mm_ProfitLockMaxTic = Math.Max(0, value);}
        }
		
        [Description("Money amount of stop loss")]
 		[Range(0, double.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "StopLossAmt", GroupName = GPS_MONEY_MGMT, Order = ODG_StopLossAmt)]	
        public double MM_StopLossAmt
        {
            get{return mm_StopLossAmt;}
            set{mm_StopLossAmt = Math.Max(0, value);}
        }
		
		[Description("Ticks amount for stop loss")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "StopLossTic", GroupName = GPS_MONEY_MGMT, Order = ODG_StopLossTic)]	
        public int MM_StopLossTic
        {
            get{return mm_StopLossTic;}
            set{mm_StopLossTic = Math.Max(0, value);}
        }
		
		[Description("Money amount for stop loss increasement")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "StopLossIncTic", GroupName = GPS_MONEY_MGMT, Order = ODG_StopLossIncTic)]	
        public int MM_StopLossIncTic
        {
            get{return mm_StopLossIncTic;}
            set{mm_StopLossIncTic = Math.Max(0, value);}
        }

        [Description("Break Even amount")]
 		[Range(0, double.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BreakEvenAmt", GroupName = GPS_MONEY_MGMT, Order = ODG_BreakEvenAmt)]	
        public double MM_BreakEvenAmt
        {
            get{return mm_BreakEvenAmt;}
            set{mm_BreakEvenAmt = Math.Max(0, value);}
        }
		
        [Description("Money amount of trailing stop loss")]
 		[Range(0, double.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TrailingStopLossAmt", GroupName = GPS_MONEY_MGMT, Order = ODG_TrailingStopLossAmt)]	
        public double MM_TrailingStopLossAmt
        {
            get{return mm_TrailingStopLossAmt;}
            set{mm_TrailingStopLossAmt = Math.Max(0, value);}
        }
        
		[Description("Ticks amount of trailing stop loss")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TrailingStopLossTic", GroupName = GPS_MONEY_MGMT, Order = ODG_TrailingStopLossTic)]	
        public int MM_TrailingStopLossTic
        {
            get{return mm_TrailingStopLossTic;}
            set{mm_TrailingStopLossTic = Math.Max(0, value);}
		}

        [Description("Percent amount of trailing stop loss")]
 		[Range(0, double.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TrailingStopLossPercent", GroupName = GPS_MONEY_MGMT, Order = ODG_TrailingStopLossPercent)]	
        public double MM_TrailingStopLossPercent
        {
            get{return mm_TrailingStopLossPercent;}
            set{mm_TrailingStopLossPercent = Math.Max(0, value);}
        }
		
		[Description("Use trailing stop loss every bar")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SLTrailing", GroupName = GPS_MONEY_MGMT, Order = ODG_SLTrailing)]	
        public bool MM_SLTrailing
        {
            get{return mm_SLTrailing;}
            set{mm_SLTrailing = value;}
        }
		
		[Description("Use trailing profit target every bar")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PTTrailing", GroupName = GPS_MONEY_MGMT, Order = ODG_PTTrailing)]	
        public bool MM_PTTrailing
        {
            get{return mm_PTTrailing;}
            set{mm_PTTrailing = value;}
        }

		[Description("Calculation mode for profit target")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PTCalculationMode", GroupName = GPS_MONEY_MGMT, Order = ODG_PTCalculationMode)]	
        public CalculationMode MM_PTCalculationMode
        {
            get{return mm_PTCalculationMode;}
            set{mm_PTCalculationMode = value;}
        }

		[Description("Calculation mode for stop loss")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SLCalculationMode", GroupName = GPS_MONEY_MGMT, Order = ODG_SLCalculationMode)]	
        public CalculationMode MM_SLCalculationMode
        {
            get{return mm_SLCalculationMode;}
            set{mm_SLCalculationMode = value;}
        }

		[Description("Calculation mode for break even")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BECalculationMode", GroupName = GPS_MONEY_MGMT, Order = ODG_BECalculationMode)]	
        public CalculationMode MM_BECalculationMode
        {
            get{return mm_BECalculationMode;}
            set{mm_BECalculationMode = value;}
        }
		
		[Description("Calculation mode for trailing stop loss")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TLSLCalculationMode", GroupName = GPS_MONEY_MGMT, Order = ODG_TLSLCalculationMode)]	
        public CalculationMode MM_TLSLCalculationMode
        {
            get{return mm_TLSLCalculationMode;}
            set{mm_TLSLCalculationMode = value;}
        }		
		
		[Description("Daily Loss Limit amount")]
 		[Range(double.MinValue, double.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "DailyLossLmt", GroupName = GPS_MONEY_MGMT, Order = ODG_DailyLossLmt)]	
        public double MM_DailyLossLmt
        {
            get{return mm_DailyLossLmt;}
            set{mm_DailyLossLmt = Math.Min(-100, value);}
        }

		[Description("Profit Factor Min")]
 		[Range(0, double.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitFactorMin", GroupName = GPS_MONEY_MGMT, Order = ODG_ProfitFactorMin)]	
        public double MM_ProfitFactorMin
        {
            get{return mm_ProfitFactorMin;}
            set{mm_ProfitFactorMin = Math.Max(0, value);}
        }
		
		[Description("Profit Factor Max")]
 		[Range(0, double.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitFactorMax", GroupName = GPS_MONEY_MGMT, Order = ODG_ProfitFactorMax)]	
        public double MM_ProfitFactorMax
        {
            get{return mm_ProfitFactorMax;}
            set{mm_ProfitFactorMax = Math.Max(0, value);}
        }
		
		[Description("Stop Loss Price Gap Preference")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SLPriceGapPref", GroupName = GPS_MONEY_MGMT, Order = ODG_SLPriceGapPref)]	
        public PriceGap MM_SLPriceGapPref
        {
            get{return mm_SLPriceGapPref;}
            set{mm_SLPriceGapPref = value;}
        }

		[Description("Profit Target Price Gap Preference")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PTPriceGapPref", GroupName = GPS_MONEY_MGMT, Order = ODG_PTPriceGapPref)]	
        public PriceGap MM_PTPriceGapPref
        {
            get{return mm_PTPriceGapPref;}
            set{mm_PTPriceGapPref = value;}
        }
		#endregion
		
		#region Variables for MM Properties		
		
		private double mm_ProfitTargetAmt = 500;
		private int mm_ProfitTgtTic = 32;
		private int mm_ProfitTgtIncTic = 8;
		private int mm_ProfitLockMinTic = 16;
		private int mm_ProfitLockMaxTic = 40;
		
		private double mm_StopLossAmt = 300;
		private int mm_StopLossTic = 16;
		private int mm_StopLossIncTic = 8;
		
		private double mm_BreakEvenAmt = 150;
		
		private double mm_TrailingStopLossAmt = 200;
		private int mm_TrailingStopLossTic = 16;
		private double mm_TrailingStopLossPercent = 1;

		//private bool mm_EnTrailing = true;
		private bool mm_PTTrailing = false;
		private bool mm_SLTrailing = false;
		
		private CalculationMode mm_PTCalculationMode = CalculationMode.Currency;
		private CalculationMode mm_SLCalculationMode = CalculationMode.Currency;
		private CalculationMode mm_BECalculationMode = CalculationMode.Currency;
		private CalculationMode mm_TLSLCalculationMode = CalculationMode.Ticks;
		
		private double mm_DailyLossLmt = -200;
		private double mm_ProfitFactorMin = 0.1;
		private double mm_ProfitFactorMax = 2.5;
		
		private PriceGap mm_SLPriceGapPref = PriceGap.Tighter;
		
		private PriceGap mm_PTPriceGapPref = PriceGap.Wider;		
		
		#endregion
		
		#region TradeMgmt
		public const string GPS_TRADE_MGMT = "TradeMgmt";
		
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
		public const int ODG_MaxOpenPosition = 11;
		public const int ODG_ExitTradeBy = 12;
//		public const int ODG_ = ;
		
		#endregion
		
		#region TM Properties
		
//		[XmlIgnore, Browsable(false)]
//		public CurrentTradeBase CurrentTrade
//		{
//			get; set;
//		}
		
		//Only types which can be Xml Serialized should be marked as a NinjaScriptAttribute,
		//otherwise you may run into errors when persisting values in various scenarios 
		//(e.g., saving workspace, or running Strategy Optimizations). 
        [Description("Short, Long or both directions for entry")]
 		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TradingDirection", GroupName = GPS_TRADE_MGMT, Order = ODG_TradingDirection)]		
        public TradingDirection TM_TradingDirection
        {
            get { return tm_TradingDirection; }
            set { tm_TradingDirection = value; }
        }

        [Description("Trading style: trend following, counter trend, scalp")]
 		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TradingStyle", GroupName = GPS_TRADE_MGMT, Order = ODG_TradingStyle)]
        public TradingStyle TM_TradingStyle
        {
            get { return tm_TradingStyle; }
            set { tm_TradingStyle = value; }
        }
		
		[Description("Use trailing entry every bar")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EnTrailing", GroupName = GPS_TRADE_MGMT, Order = ODG_EnTrailing)]
        public bool TM_EnTrailing
        {
            get{return tm_EnTrailing;}
            set{tm_EnTrailing = value;}
        }
		
        [Description("Offeset points for limit price entry")]
		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EnOffsetPnts", GroupName = GPS_TRADE_MGMT, Order = ODG_EnOffsetPnts)]
        public double TM_EnOffsetPnts
        {
            get{return tm_EnOffsetPnts;}
            set{tm_EnOffsetPnts = Math.Max(0, value);}
        }
		
        [Description("How long to check entry order filled or not")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MinutesChkEnOrder", GroupName = GPS_TRADE_MGMT, Order = ODG_MinutesChkEnOrder)]
        public int TM_MinutesChkEnOrder
        {
            get{return tm_MinutesChkEnOrder;}
            set{tm_MinutesChkEnOrder = Math.Max(0, value);}
        }
		
        [Description("How long to check P&L")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MinutesChkPnL", GroupName = GPS_TRADE_MGMT, Order = ODG_MinutesChkPnL)]
        public int TM_MinutesChkPnL
        {
            get{return tm_MinutesChkPnL;}
            set{tm_MinutesChkPnL = Math.Max(0, value);}
        }
		
		[Description("Bar count before checking P&L")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarsToCheckPL", GroupName = GPS_TRADE_MGMT, Order = ODG_BarsToCheckPL)]
        public int TM_BarsToCheckPnL
        {
            get{return tm_BarsToCheckPnL;}
            set{tm_BarsToCheckPnL = Math.Max(1, value);}
        }
		
        [Description("How many bars to hold entry order before cancel it")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarsHoldEnOrd", GroupName = GPS_TRADE_MGMT, Order = ODG_BarsHoldEnOrd)]
        public int TM_BarsHoldEnOrd
        {
            get{return tm_BarsHoldEnOrd;}
            set{tm_BarsHoldEnOrd = Math.Max(1, value);}
        }
		
        [Description("Bar count for en order counter pullback")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EnCounterPBBars", GroupName = GPS_TRADE_MGMT, Order = ODG_EnCounterPBBars)]
        public int TM_EnCounterPBBars
        {
            get{return tm_EnCounterPBBars;}
            set{tm_EnCounterPBBars = Math.Max(1, value);}
        }
				
		[Description("Bar count since last filled PT or SL")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarsSincePTSL", GroupName = GPS_TRADE_MGMT, Order = ODG_BarsSincePTSL)]
        public int TM_BarsSincePTSL
        {
            get{return tm_BarsSincePtSl;}
            set{tm_BarsSincePtSl = Math.Max(1, value);}
        }
		
		[Description("Max open position")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MaxOpenPosition", GroupName = GPS_TRADE_MGMT, Order = ODG_MaxOpenPosition)]
        public int TM_MaxOpenPosition
        {
            get{return tm_MaxOpenPosition;}
            set{tm_MaxOpenPosition = Math.Max(1, value);}
        }
		
		[Description("Exit Trade By")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ExitTradeBy", GroupName = GPS_TRADE_MGMT, Order = ODG_ExitTradeBy)]
        public ExitBy TM_ExitTradeBy
		{
            get{return tm_ExitTradeBy;}
            set{tm_ExitTradeBy = value;}
        }
		#endregion

		#region Variables for TM Properties
		
		private TradingDirection tm_TradingDirection = TradingDirection.Both; // -1=short; 0-both; 1=long;
		private TradingStyle tm_TradingStyle = TradingStyle.TrendFollowing; // -1=counter trend; 1=trend following;				
		private bool tm_EnTrailing = false;
		private double tm_EnOffsetPnts = 1;
		private int tm_MinutesChkEnOrder = 10;
		private int tm_MinutesChkPnL = 30;
		private int tm_BarsToCheckPnL = 2;
		private int tm_BarsHoldEnOrd = 3;
		private int tm_EnCounterPBBars = 2;
		private int tm_BarsSincePtSl = 1;
		private int tm_MaxOpenPosition = 3;
		private ExitBy tm_ExitTradeBy = ExitBy.RRR;
		
		#endregion
		
		#region Trigger
		public const string GPS_TRIGGER = "Trigger";
		
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
		public const int ODG_CloseH = 15;
		public const int ODG_CloseM = 16;
		
//		public const int ODG_ = ;
		
		#endregion
		
		#region Output
		public const string GPS_OUTPUT = "Output";
//		public const int ODG_ = ;
		
		#endregion
	}
}
