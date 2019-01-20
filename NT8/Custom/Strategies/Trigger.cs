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
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Strategies.ZTraderStg;
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
	
	public partial class GSZTraderBase : Strategy
	{	
		public virtual void InitTrigger() {
			
		}
		
		/// <summary>
		/// Check: 1) if it's time to liquidate
		/// 2) if it's time to put entry order
		/// 3) if there is signal fired for exit
		/// 4) if there is signal fired for entry
		/// </summary>
		public virtual void CheckTrigger() {
			if(IsLiquidateTime(TG_TimeLiqH, TG_TimeLiqM)) {
				CloseAllPositions();
			}
			else {
				IndicatorSignal indSignal = indicatorProxy.CheckIndicatorSignal();
				if (Position.MarketPosition != MarketPosition.Flat) { //There are positions
					CheckExitTrade(indSignal);
				}
				else { // no positions
					CheckEntryTrade();
				}
			}
		}
		
		//public virtual IndicatorSignal GetSignal() {return null;}
		
		/// <summary>
		/// Check if now is the time allowed to put trade
		/// </summary>
		/// <param name="time_start">start time</param>
		/// <param name="time_end">end time</param>
		/// <param name="session_start">the overnight session start time: 170000 for ES</param>
		/// <returns></returns>
		public bool IsTradingTime(int session_start) {
			//Bars.Session.GetNextBeginEnd(DateTime time, out DateTime sessionBegin, out DateTime sessionEnd)
			int time_start = indicatorProxy.GetTimeByHM(TG_TimeStartH, TG_TimeStartM);
			int time_end = indicatorProxy.GetTimeByHM(TG_TimeEndH, TG_TimeEndM);
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
					TM_TradeStyle = TradingStyle.TrendFollowing;
					TM_TradeDirection = 1;
					break;
				case PriceActionType.UpWide: //wide up channel
					TM_TradeStyle = TradingStyle.CounterTrend;
					TM_TradeDirection = 1;
					break;
				case PriceActionType.DnTight: //
					TM_TradeStyle = TradingStyle.TrendFollowing;
					TM_TradeDirection = -1;
					break;
				case PriceActionType.DnWide: //wide dn channel
					TM_TradeStyle = TradingStyle.CounterTrend;
					TM_TradeDirection = -1;
					break;
				case PriceActionType.RngTight: //
					TM_TradeStyle = TradingStyle.Ranging;//-1;
					TM_TradeDirection = 0;
					break;
				case PriceActionType.RngWide: //
					TM_TradeStyle = TradingStyle.CounterTrend;
					TM_TradeDirection = 1;
					break;
				default:
					TM_TradeStyle = TradingStyle.TrendFollowing;
					TM_TradeDirection = 0;
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
		
        #region Trigger Properties
		
        [Description("Min swing size for entry")]
 		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TG_EnSwingMinPnts", GroupName = "Trigger", Order = 2)]
        public double TG_EnSwingMinPnts
        {
            get { return tg_EnSwingMinPnts; }
            set { tg_EnSwingMinPnts = Math.Max(1, value); }
        }

        [Description("Max swing size for entry")]
 		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TG_EnSwingMaxPnts", GroupName = "Trigger", Order = 3)]
        public double TG_EnSwingMaxPnts
        {
            get { return tg_EnSwingMaxPnts; }
            set { tg_EnSwingMaxPnts = Math.Max(4, value); }
        }

		[Description("Min pullback size for entry")]
 		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TG_EnPullbackMinPnts", GroupName = "Trigger", Order = 4)]
        public double TG_EnPullbackMinPnts
        {
            get { return tg_EnPullbackMinPnts; }
            set { tg_EnPullbackMinPnts = Math.Max(1, value); }
        }

        [Description("Max pullback size for entry")]
 		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TG_EnPullbackMaxPnts", GroupName = "Trigger", Order = 5)]
        public double TG_EnPullbackMaxPnts
        {
            get { return tg_EnPullbackMaxPnts; }
            set { tg_EnPullbackMaxPnts = Math.Max(2, value); }
        }

        [Description("Time start hour")]
 		[Range(0, 23), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TG_TimeStartH", GroupName = "Trigger", Order = 6)]
        public int TG_TimeStartH
        {
            get { return tg_TimeStartH; }
            set { tg_TimeStartH = Math.Max(0, value); }
        }
		
        [Description("Time start minute")]
 		[Range(0, 59), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TG_TimeStartM", GroupName = "Trigger", Order = 7)]
        public int TG_TimeStartM
        {
            get { return tg_TimeStartM; }
            set { tg_TimeStartM = Math.Max(0, value); }
        }
		
        [Description("Time end hour")]
 		[Range(0, 23), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TG_TimeEndH", GroupName = "Trigger", Order = 8)]
        public int TG_TimeEndH
        {
            get { return tg_TimeEndH; }
            set { tg_TimeEndH = Math.Max(0, value); }
        }

        [Description("Time end minute")]
 		[Range(0, 59), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TG_TimeEndM", GroupName = "Trigger", Order = 9)]
        public int TG_TimeEndM
        {
            get { return tg_TimeEndM; }
            set { tg_TimeEndM = Math.Max(0, value); }
        }
		
        [Description("Liquidate hour")]
 		[Range(0, 23), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TG_TimeLiqH", GroupName = "Trigger", Order = 10)]
        public int TG_TimeLiqH
        {
            get { return tg_TimeLiqH; }
            set { tg_TimeLiqH = Math.Max(0, value); }
        }

        [Description("Liquidate minute")]
 		[Range(0, 59), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TG_TimeLiqM", GroupName = "Trigger", Order = 11)]
        public int TG_TimeLiqM
        {
            get { return tg_TimeLiqM; }
            set { tg_TimeLiqM = Math.Max(0, value); }
        }
		
        #endregion
		
		#region Variables for Properties

		//time=H*10000+M*100+S, S is skipped here;
        private int tg_TimeStartH = 1; //10100 Default setting for timeStart hour
		private int tg_TimeStartM = 1; //10100 Default setting for timeStart minute
		//private int timeStart = -1; //10100 Default setting for timeStart
        private int tg_TimeEndH = 14; // Default setting for timeEnd hour
		private int tg_TimeEndM = 59; // Default setting for timeEnd minute
		//private int timeEnd = -1; // Default setting for timeEnd
		private int tg_TimeLiqH = 15; //Time H to liquidate
		private int tg_TimeLiqM = 8; //Time M to liquidate
		
        private double tg_EnSwingMinPnts = 10; //10 Default setting for EnSwingMinPnts
        private double tg_EnSwingMaxPnts = 35; //16 Default setting for EnSwingMaxPnts
		private double tg_EnPullbackMinPnts = 1; //6 Default setting for EnPullbackMinPnts
        private double tg_EnPullbackMaxPnts = 8; //10 Default setting for EnPullbackMaxPnts
		
		#endregion
	}
}
