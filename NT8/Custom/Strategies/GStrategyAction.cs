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

using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Indicators.PriceActions;
using NinjaTrader.NinjaScript.Strategies.ZTraderStg;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	/// <summary>
	/// The trade action is to trigger entry/exit, 
	/// or modify existing orders for extry/exit;
	/// </summary>
	public partial class GStrategyBase : Strategy
	{
		#region Variables
		/// <summary>
		/// Hold the trade action from the underlining strategy
		/// The key=BarNo that holds the action
		/// value=the action
		/// </summary>
		private SortedDictionary<int, TradeAction> tradeActions = 
			new SortedDictionary<int, TradeAction>();

		#endregion
		
		#region Set/Check TradeActions
		/// <summary>
		/// Take the triggers from command, perform/rule, and indicator signals
		/// to generate tradeAction for current Bar or next Bar(most likely current Bar);
		/// This function could be overridden by custom strategy to define
		/// different logics/priorities to handle the triggers;
		/// CurrentTrade.TradeAction stores the TradeAction that has taken or will be taken;
		/// GetTradeAction(barNo) provides the TradeAction that is generated from the triggers;
		/// </summary>
		public virtual void SetTradeAction() {
			//Read signals from command(cur bar), 
			//perform/rule(next bar, because the trigger could fire before/after SetTradeAction was called),
			//indicators(cur bar)
			if(HasPosition() == 0) {
				SetNewEntryTradeAction();
			} else {
				SetExitTradeAction();
			}
		}
		
		/// <summary>
		/// Set new Entry TradeSignal
		/// and SL/PT TradeSignals
		/// </summary>
		/// <returns></returns>
		public virtual bool SetNewEntryTradeAction() {
			return false;
		}
		
		/// <summary>
		/// Set SL/PT TradedSignals;
		/// Get cmd and event trigger signals;
		/// Indicator signals will depend;
		/// </summary>
		/// <returns></returns>
		public virtual bool SetExitTradeAction() {
			return false;
		}
		
		public virtual TradeAction CheckTradeAction() {
			return null;
		}
		
		/// <summary>
		/// 
		/// </summary>
		public virtual void TakeTradeAction() {
			//CurrentTrade.TradeAction = GetTradeAction(CurrentBar);//??
			try {
				TradeAction ta = CurrentTrade.TradeAction;
				if(ta == null || ta.ActionStatus == TradeActionStatus.Executed) {
					IndicatorProxy.PrintLog(true, IsLiveTrading(),
						String.Format("{0}:TakeTradeAction called, CurrentTrade.TradeAction=null or Executed=={1}",
						CurrentBar, ta.ActionStatus.ToString()));
					return;
				}
				String sigName = "UnKnown";
				if(ta.EntrySignal != null)
					sigName = ta.EntrySignal.SignalName;
				if(ta.StopLossSignal != null)
					sigName = ta.StopLossSignal.SignalName;
				if(ta.ProfitTargetSignal != null)
					sigName = ta.ProfitTargetSignal.SignalName;
				if(ta.ScaleOutSignal != null)
					sigName = ta.ScaleOutSignal.SignalName;
								
				IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":TakeTradeAction"
				+ ";SignalName=" + sigName
				+ ";ActionName=" + ta.ActionName
				+ ";ActionType=" + ta.ActionType.ToString()
				+ ";CurrentTrade.TDID=" + CurrentTrade.TradeID
				+ ";OcoID=" + CurrentTrade.OcoID
				+ ";HasPosition=" + HasPosition());
				
				ta.ActionStatus = TradeActionStatus.Executed;
				switch(ta.ActionType) {
					case TradeActionType.EntrySimple:
					case TradeActionType.Bracket:
						PutEntryTrade();
						break;
					case TradeActionType.ExitOCO:
						PutExitTrade();
						break;
					case TradeActionType.ExitSimple:
						PutLiquidateTrade();
						break;
				}
			} catch(Exception ex) {
				IndicatorProxy.PrintLog(true, IsLiveTrading(), 
					CurrentBar + ":Exception TakeTradeAction--" + ex.StackTrace);
			}
		}		
		#endregion
		
		#region Manage TradeActions
		
		public void AddTradeAction(int barNo, TradeAction action) {
			this.tradeActions.Add(barNo, action);
		}
		
		/// <summary>
		/// Add the action to the list of the bar with barNo
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="action"></param>
		public void UpdateTradeAction(int barNo, TradeAction action) {
			TradeAction old_action;
			if(!this.tradeActions.TryGetValue(barNo, out old_action)) {
				AddTradeAction(barNo, action);
			} else
				this.tradeActions[barNo] = action;
		}
		
		public void AddTradeAction(int barNo, string actname,
			TradeActionType saType, SupportResistanceRange<double> snr) {
			
			TradeAction sa = new TradeAction();
			sa.ActionType = saType;
			//sa.SnR = snr;
			TradeAction tact = new TradeAction();
			tact.BarNo = barNo;
//			tact.ActionName = actname;
//			tact.TradeActionType = TradeActionType.BracketSignal;
			//tact.Signal_Action = sa;
			AddTradeAction(barNo, tact);
		}

		public int GetLastTradeActionBarNo(int barNo) {
			int k = -1;
			foreach(int kk in this.tradeActions.Keys.Reverse()) {
				if(kk < barNo) {
					k = kk;
					break;
				}
			}
			return k;
		}
		
		/// <summary>
		/// Get the trade action for the bar 
		/// </summary>
		/// <param name="barNo"></param>
		/// <returns></returns>
		public TradeAction GetTradeAction(int barNo) {
			TradeAction action;
			if(!this.tradeActions.TryGetValue(barNo, out action))
				return null;
			else
				return action;
		}

		/// <summary>
		/// Get the last trade action before the barNo
		/// </summary>
		/// <param name="barNo"></param>
		/// <returns></returns>
		public TradeAction GetLastTradeAction(int barNo) {
			return GetTradeAction(GetLastTradeActionBarNo(barNo));
		}
		
		/// <summary>
		/// Get the action from bar with barNo and the action_name
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="action_name"></param>
		/// <returns></returns>
		public TradeAction GetTradeActionByName(int barNo, string action_name) {
			//if(list_action != null) {
				foreach(TradeAction sig in this.tradeActions.Values) {
					if(action_name.Equals(sig.ActionName))
						return sig;
				}
			//}
			
			return null;			
		}

		/// <summary>
		/// Get the last action before barNo by action_name
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="action_name"></param>
		/// <returns></returns>
		public TradeAction GetLastTradeActionByName(int barNo, string action_name) {
			int k = barNo;
			foreach(int kk in this.tradeActions.Keys.Reverse()) {
				if(kk < k) {
					TradeAction sig = GetTradeActionByName(k, action_name);
					if(sig != null) return sig;
					k = kk;
				}
			}
			return null;			
		}
		
		/// <summary>
		/// Get the action list for the bar by action type ???
		///  Get the trade action from bar with barNo and the actionActionType ???
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="action_type"></param>
		/// <returns></returns>
		public TradeAction GetTradeActionByType(int barNo, TradeActionType action_type) {
			TradeAction list_action = GetTradeAction(barNo);
			if(list_action != null) {				
				TradeAction list_sigByType = new TradeAction();
//				foreach(TradeAction sig in list_action) {
//					if(action_type == sig.TradeActionType)
//						list_sigByType.Add(sig);
//				}
//				if(list_sigByType.Count > 0)
//					return list_sigByType;
			}
			
			return null;
		}		

		/// <summary>
		/// Get last action list before barNo by action type ???
		/// Get the last action before barNo by actionActionType ???
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="action_type"></param>
		/// <returns></returns>
		public TradeAction GetLastTradeActionByType(int barNo, TradeActionType action_type) {
			int k = barNo;
			foreach(int kk in this.tradeActions.Keys.Reverse()) {				
				if(kk < k) {
					TradeAction sigs = GetTradeActionByType(k, action_type);
					if(sigs != null) return sigs;
					k = kk;
				}
			}
			return null;		
		}
		#endregion

        #region Properties

        #endregion		
	}
}
