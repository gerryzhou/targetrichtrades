#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Indicators.PriceActions;
using NinjaTrader.NinjaScript.Strategies.ZTraderStg;
using NinjaTrader.NinjaScript.AddOns;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class Trigger {
		private Strategy instStrategy = null;
		public Trigger(Strategy inst_strategy) {
			this.instStrategy = inst_strategy;
		}
	}
	
	public partial class GStrategyBase : Strategy
	{
		#region Trigger Trade Functions
		
		public virtual void InitTrigger() {}
			
		/// <summary>
		/// Check: 1) if it's time to liquidate
		/// 2) if it's time to put entry order
		/// 3) if there is signal fired for exit
		/// 4) if there is signal fired for entry
		/// </summary>
		public virtual void CheckTrigger() {
			if(IsLiquidateTime(TG_TradeLiqH, TG_TradeLiqM)) {
				CloseAllPositions();
			}
			else {
				//tradeSignal = GetTradeSignal();
				if (GetMarketPosition() != MarketPosition.Flat) { //There are positions
					CheckExitTrade();
				}
				else { // no positions
					CheckNewEntryTrade();
				}
			}
		}		
		
		/// <summary>
		/// Check if now is the time allowed to put trade
		/// </summary>
		/// <param name="time_start">start time</param>
		/// <param name="time_end">end time</param>
		/// <param name="session_start">the overnight session start time: 170000 for ES</param>
		/// <returns></returns>
		public bool IsTradingTime(int session_start) {
			//Bars.Session.GetNextBeginEnd(DateTime time, out DateTime sessionBegin, out DateTime sessionEnd)
			int time_start = indicatorProxy.GetTimeByHM(TG_TradeStartH, TG_TradeStartM);
			int time_end = indicatorProxy.GetTimeByHM(TG_TradeEndH, TG_TradeEndM);
			int time_now = ToTime(Time[0]);
			bool isTime= false;
			if(time_start >= session_start) {
				if(time_now >= time_start || time_now <= time_end)
					isTime = true;
			}
			else if (time_now >= time_start && time_now <= time_end) {
				isTime = true;
			}
			return isTime;
		}
		
		/// <summary>
		/// Check if now is the time to liquidate
		/// </summary>
		/// <param name="timeH">time hour</param>
		/// <param name="timeM">time min</param>
		/// <returns></returns>
		public bool IsLiquidateTime(int timeH, int timeM) {
			int time_now = indicatorProxy.GetTimeByHM(Time[0].Hour, Time[0].Minute);
			int time_lastBar = indicatorProxy.GetTimeByHM(Time[1].Hour, Time[1].Minute);
			int time_liq = indicatorProxy.GetTimeByHM(timeH, timeM);
			bool isTime= false;
			
			if(time_now == time_liq || (time_liq > time_lastBar && time_liq <= time_now)) {
				isTime = true;
			}
			return isTime;
		}
		
		protected virtual void SetTradeContext(PriceAction pa) {
			switch(pa.paType) {
				case PriceActionType.UpTight: //
					TM_TradingStyle = TradingStyle.TrendFollowing;
					TM_TradingDirection = TradingDirection.Up;
					break;
				case PriceActionType.UpWide: //wide up channel
					TM_TradingStyle = TradingStyle.CounterTrend;
					TM_TradingDirection = TradingDirection.Up;
					break;
				case PriceActionType.DnTight: //
					TM_TradingStyle = TradingStyle.TrendFollowing;
					TM_TradingDirection = TradingDirection.Down;
					break;
				case PriceActionType.DnWide: //wide dn channel
					TM_TradingStyle = TradingStyle.CounterTrend;
					TM_TradingDirection = TradingDirection.Down;
					break;
				case PriceActionType.RngTight: //
					TM_TradingStyle = TradingStyle.Ranging;//-1;
					TM_TradingDirection = TradingDirection.Both;
					break;
				case PriceActionType.RngWide: //
					TM_TradingStyle = TradingStyle.CounterTrend;
					TM_TradingDirection = TradingDirection.Both;
					break;
				default:
					TM_TradingStyle = TradingStyle.TrendFollowing;
					TM_TradingDirection = TradingDirection.Both;
					break;
			}
		}

		protected double GetTimeSinceEntry() {
			int bse = BarsSinceEntryExecution();
			double timeSinceEn = -1;
			if(bse > 0) {
				timeSinceEn = indicatorProxy.GetMinutesDiff(Time[0], Time[bse]);
			}
			return timeSinceEn;
		}
		
		/// <summary>
		/// Timestamp: 20190503172538957
		/// </summary>
		/// <returns></returns>
		public string GetCurTimestampStr() {
			return DateTime.Now.ToString("yyyyMMddHHmmssfff");
		}
		
		/// <summary>
		/// Timestamp for bars ago: 20190503172538957
		/// </summary>
		/// <returns></returns>
		public string GetBarTimestampStr(int barsAgo) {
			return Time[barsAgo].ToString("yyyyMMddHHmmssfff") 
				+ GZUtils.GetRandomNumber(1000, 9999).ToString();
		}
		
		#endregion Trigger Functions
		
		#region Pattern Functions
		
		protected virtual bool PatternMatched()
		{
			//Print("CurrentBar, barsMaxLastCross, barsAgoMaxPbSAREn,=" + CurrentBar + "," + barsAgoMaxPbSAREn + "," + barsSinceLastCross);
//			if (giParabSAR.IsSpvAllowed4PAT(curBarPriceAction.paType) && barsSinceLastCross < barsAgoMaxPbSAREn) 
//				return true;
//			else return false;
			return false;
			//barsAgoMaxPbSAREn Bars Since PbSAR reversal. Enter the amount of the bars ago maximum for PbSAR entry allowed
		}
		
		/// <summary>
		/// Check the first reversal bar for the pullback under current ZigZag gap
		/// </summary>
		/// <param name="cur_gap">current ZigZag gap</param>
		/// <param name="tick_size">tick size of the symbol</param>
		/// <param name="n_bars">bar count with the pullback prior to the last reversal bar</param>
		/// <returns>is TBR or not</returns>
		public bool IsTwoBarReversal(double cur_gap, double tick_size, int n_bars) {
			bool isTBR= false;
			
			if(n_bars < 0) return isTBR;
			
			if(cur_gap > 0)
			{
				//Check if the last n_bars are pullback bars (bear bar)
				for(int i=1; i<=n_bars; i++)
				{
					if(Close[i] > Open[i])
						return isTBR;
				}
				if(Close[0]-Open[0] > tick_size && Open[1]-Close[1] >= tick_size) {
					isTBR= true;
				}
			}
			else if(cur_gap < 0)
			{
				//Check if the last n_bars are pullback bars (bull bar)
				for(int i=1; i<=n_bars; i++)
				{
					if(Close[i] < Open[i])
						return isTBR;
				}
				if(Open[0] - Close[0] > tick_size && Close[1] - Open[1] >= tick_size) {
					isTBR= true;
				}
			}
			return isTBR;
		}
		
		/// <summary>
		/// Get the Two Bar Reversal count for the past barsBack
		/// </summary>
		/// <param name="cur_gap">current ZigZag gap</param>
		/// <param name="tick_size">tick size of the symbol</param>
		/// <param name="barsBack">bar count to look back</param>
		/// <returns>pairs count for TBR during the barsBack</returns>
		public int GetTBRPairsCount(double cur_gap, double tick_size, int barsBack) {
			int tbr_count = 0;
			for(int i=0; i<barsBack; i++) {
				if(cur_gap > 0 && Close[i]-Open[i] > tick_size && Open[i+1]-Close[i+1] >= tick_size) {
					tbr_count++;
				}
				if(cur_gap < 0 && Open[i] - Close[i] > tick_size && Close[i+1] - Open[i+1] >= tick_size) {
					tbr_count++;
				}
			}
			return tbr_count;
		}
		
		#endregion Pattern Functions
		
        #region Trigger Properties
		
        [Description("Min swing size for entry")]
 		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EnSwingMinPnts", GroupName = GPS_TRIGGER, Order = ODG_EnSwingMinPnts)]
        public double TG_EnSwingMinPnts
        {
            get { return tg_EnSwingMinPnts; }
            set { tg_EnSwingMinPnts = Math.Max(1, value); }
        }

        [Description("Max swing size for entry")]
 		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EnSwingMaxPnts", GroupName = GPS_TRIGGER, Order = ODG_EnSwingMaxPnts)]
        public double TG_EnSwingMaxPnts
        {
            get { return tg_EnSwingMaxPnts; }
            set { tg_EnSwingMaxPnts = Math.Max(4, value); }
        }

		[Description("Min pullback size for entry")]
 		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EnPullbackMinPnts", GroupName = GPS_TRIGGER, Order = ODG_EnPullbackMinPnts)]
        public double TG_EnPullbackMinPnts
        {
            get { return tg_EnPullbackMinPnts; }
            set { tg_EnPullbackMinPnts = Math.Max(1, value); }
        }

        [Description("Max pullback size for entry")]
 		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EnPullbackMaxPnts", GroupName = GPS_TRIGGER, Order = ODG_EnPullbackMaxPnts)]
        public double TG_EnPullbackMaxPnts
        {
            get { return tg_EnPullbackMaxPnts; }
            set { tg_EnPullbackMaxPnts = Math.Max(2, value); }
        }

        [Description("Trade start hour")]
 		[Range(0, 23), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TimeStartH", GroupName = GPS_TRIGGER, Order = ODG_TimeStartH)]
        public int TG_TradeStartH
        {
            get { return tg_TradeStartH; }
            set { tg_TradeStartH = Math.Max(0, value); }
        }
		
        [Description("Trade start minute")]
 		[Range(0, 59), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TimeStartM", GroupName = GPS_TRIGGER, Order = ODG_TimeStartM)]
        public int TG_TradeStartM
        {
            get { return tg_TradeStartM; }
            set { tg_TradeStartM = Math.Max(0, value); }
        }
		
        [Description("Trade end hour")]
 		[Range(0, 23), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TimeEndH", GroupName = GPS_TRIGGER, Order = ODG_TimeEndH)]
        public int TG_TradeEndH
        {
            get { return tg_TradeEndH; }
            set { tg_TradeEndH = Math.Max(0, value); }
        }

        [Description("Trade end minute")]
 		[Range(0, 59), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TimeEndM", GroupName = GPS_TRIGGER, Order = ODG_TimeEndM)]
        public int TG_TradeEndM
        {
            get { return tg_TradeEndM; }
            set { tg_TradeEndM = Math.Max(0, value); }
        }
		
        [Description("Liquidate hour")]
 		[Range(0, 23), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TimeLiqH", GroupName = GPS_TRIGGER, Order = ODG_TimeLiqH)]
        public int TG_TradeLiqH
        {
            get { return tg_TradeLiqH; }
            set { tg_TradeLiqH = Math.Max(0, value); }
        }

        [Description("Liquidate minute")]
 		[Range(0, 59), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TimeLiqM", GroupName = GPS_TRIGGER, Order = ODG_TimeLiqM)]
        public int TG_TradeLiqM
        {
            get { return tg_TradeLiqM; }
            set { tg_TradeLiqM = Math.Max(0, value); }
        }

        [Description("Open start hour")]
 		[Range(0, 23), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "OpenStartH", GroupName = GPS_TRIGGER, Order = ODG_OpenStartH)]
        public int TG_OpenStartH
        {
            get { return tg_OpenStartH; }
            set { tg_OpenStartH = Math.Max(0, value); }
        }

        [Description("Open start minute")]
 		[Range(0, 59), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "OpenStartM", GroupName = GPS_TRIGGER, Order = ODG_OpenStartM)]
        public int TG_OpenStartM
        {
            get { return tg_OpenStartM; }
            set { tg_OpenStartM = Math.Max(0, value); }
        }
		
        [Description("Open end hour")]
 		[Range(0, 23), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "OpenEndH", GroupName = GPS_TRIGGER, Order = ODG_OpenEndH)]
        public int TG_OpenEndH
        {
            get { return tg_OpenEndH; }
            set { tg_OpenEndH = Math.Max(0, value); }
        }

        [Description("Open end minute")]
 		[Range(0, 59), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "OpenEndM", GroupName = GPS_TRIGGER, Order = ODG_OpenEndM)]
        public int TG_OpenEndM
        {
            get { return tg_OpenEndM; }
            set { tg_OpenEndM = Math.Max(0, value); }
        }		
        #endregion
		
		#region Variables for Properties

		//time=H*10000+M*100+S, S is skipped here;
        private int tg_TradeStartH = 17; //10100 Default setting for trade Start hour
		private int tg_TradeStartM = 1; //10100 Default setting for trade Start minute
		//private int timeStart = -1; //10100 Default setting for timeStart
        private int tg_TradeEndH = 15; // Default setting for trade End hour
		private int tg_TradeEndM = 59; // Default setting for trade End minute
		//private int timeEnd = -1; // Default setting for timeEnd
		private int tg_TradeLiqH = 15; //Time H to liquidate
		private int tg_TradeLiqM = 8; //Time M to liquidate
		
        private int tg_OpenStartH = 8; //Default setting for open Start hour
		private int tg_OpenStartM = 30; //Default setting for open Start minute		
        
		private int tg_OpenEndH = 10; //Default setting for open End hour
		private int tg_OpenEndM = 30; //Default setting for open End minute
		
        private double tg_EnSwingMinPnts = 10; //10 Default setting for EnSwingMinPnts
        private double tg_EnSwingMaxPnts = 35; //16 Default setting for EnSwingMaxPnts
		private double tg_EnPullbackMinPnts = 1; //6 Default setting for EnPullbackMinPnts
        private double tg_EnPullbackMaxPnts = 8; //10 Default setting for EnPullbackMaxPnts
		
		#endregion
	}
}
