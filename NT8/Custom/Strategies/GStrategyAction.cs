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
		/// <summary>
		/// Hold the trade action from the underlining strategy
		/// The key=BarNo that holds the action
		/// value=the action
		/// </summary>
		private SortedDictionary<int, TradeAction> tradeActions = 
			new SortedDictionary<int, TradeAction>();
		
		#region Variables

		#endregion
		
		#region Methods
		public virtual void SetTradeAction() {}
		
		public virtual TradeAction CheckTradeAction() {
			return null;
		}
		
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
			sa.TradeActionType = saType;
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
		/// Get the trade action from bar with barNo and the actionActionType ???
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="actionActionType"></param>
		/// <returns></returns>
		public TradeAction GetTradeActionByActionType(int barNo, TradeActionType action_actiontype) {
			TradeAction list_action = GetTradeAction(barNo);
			if(list_action != null) {
//				foreach(TradeAction sig in list_action) {
//					if(sig.Action_Action != null && 
//						action_actiontype.Equals(sig.Action_Action.TradeActionType))
//						return sig;
//				}
			}
			
			return null;			
		}

		/// <summary>
		/// Get the last action before barNo by actionActionType ???
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="actionActionType"></param>
		/// <returns></returns>
		public TradeAction GetLastTradeActionByActionType(int barNo, TradeActionType action_actiontype) {
			int k = barNo;
			foreach(int kk in this.tradeActions.Keys.Reverse()) {				
				if(kk < k) {
					TradeAction sig = GetTradeActionByActionType(k, action_actiontype);
					if(sig != null) return sig;
					k = kk;
				}
			}
			return null;			
		}
		
		/// <summary>
		/// Get the action list for the bar by action type ???
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
