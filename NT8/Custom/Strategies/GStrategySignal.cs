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
	/// This file defined the interfaces talking with indicators;
	/// This interface needs to work with multiple indicators in the strategy
	/// to generate the signal or get the value of the functions;
	/// *** It may not need to save all the trade signals in the dictionary, 
	/// since the tradeAction saved for the bar that has the trade action and signal;
	/// </summary>
	public partial class GStrategyBase : Strategy
	{		
		
		#region Variables

		/// <summary>
		/// Hold the trade signal from the underlining strategy
		/// The key=BarNo that holds the signal set
		/// value=the set of signals
		/// </summary>
//		private SortedDictionary<int, TradeSignal> tradeSignals = 
//			new SortedDictionary<int, TradeSignal>();
		#endregion
		
		#region Methods
		/// <summary>
		/// The indicator signal is to trigger trade actions
		/// Set the indicator signals for each bar/indicator
		/// </summary>
		public virtual bool CheckIndicatorSignals(TradeSignalType tdSigType){
			switch(tdSigType) {
				case TradeSignalType.Entry:
					return CheckNewEntrySignals();
				case TradeSignalType.Exit: 
					return CheckExitSignals();
				case TradeSignalType.ExitOCO:
					return CheckExitOCOSignals();
				case TradeSignalType.ProfitTarget:
					return CheckProfitTargetSignals();
				case TradeSignalType.StopLoss:
					return CheckStopLossSignals();
				case TradeSignalType.TrailingStopLoss:
					return CheckTrailingStopLossSignals();
				case TradeSignalType.ScaleIn:
					return CheckScaleInSignals();
				case TradeSignalType.ScaleOut:
					return CheckScaleOutSignals();
				default:
					return false;
			}
		}

		public virtual bool CheckNewEntrySignals(){return false;}
		public virtual bool CheckExitSignals(){return false;}
		public virtual bool CheckExitOCOSignals(){return false;}
		public virtual bool CheckStopLossSignals(){return false;}
		public virtual bool CheckProfitTargetSignals(){return false;}
		public virtual bool CheckTrailingStopLossSignals(){return false;}
		public virtual bool CheckScaleInSignals(){return false;}
		public virtual bool CheckScaleOutSignals(){return false;}
		
		/// <summary>
		/// The trade signal is to trigger entry/exit, 
		/// or modify existing orders for extry/exit;
		/// The trade signals are generated from indicator signals
		/// and stored in TradeActions so no need to save it again bar by bar;
		/// </summary>
		/// <returns></returns>
		public virtual TradeSignal GetTradeSignal() {return null;}
		
		public virtual void SetTradeSignal() {}
		
		public virtual void SetEntrySignal() {}
		
		public virtual void SetStopLossSignal() {}
		
		public virtual void SetProfitTargetSignal() {}
		
		/// <summary>
		/// Check trade signals from indicator signals
		/// </summary>
		/// <returns></returns>
		public virtual bool CheckTradeSignals() {
			if(HasPosition() != 0 && CheckIndicatorSignals(TradeSignalType.Exit)) {
				return SetExitTradeAction();
			}
			else if(NewOrderAllowed() && CheckIndicatorSignals(TradeSignalType.Entry))
			{
				indicatorProxy.TraceMessage(this.Name, PrintOut);
				return SetNewEntryTradeAction();
			}

			return false;
		}
		
//		public void AddTradeSignal(int barNo, TradeSignal signal) {
//			this.tradeSignals.Add(barNo, signal);
//		}
		
		/// <summary>
		/// Add the signal to the list of the bar with barNo
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signal"></param>
//		public void UpdateTradeSignal(int barNo, TradeSignal signal) {
//			TradeSignal old_signal;
//			if(!this.tradeSignals.TryGetValue(barNo, out old_signal)) {
//				AddTradeSignal(barNo, signal);
//			} else
//				this.tradeSignals[barNo] = signal;
//		}
		
//		public void AddTradeSignal(int barNo, string signame,
//			SignalActionType saType, SupportResistanceRange<double> snr) {
			
//			SignalAction sa = new SignalAction();
//			sa.SignalActionType = saType;
//			sa.SnR = snr;
//			TradeSignal tsig = new TradeSignal();
//			tsig.BarNo = barNo;
//			tsig.SignalName = signame;
//			tsig.TradeSignalType = TradeSignalType.Entry;
//			//tsig.Signal_Action = sa;
//			AddTradeSignal(barNo, tsig);
//		}

//		public int GetLastTradeSignalBarNo(int barNo) {
//			int k = -1;
//			foreach(int kk in this.tradeSignals.Keys.Reverse()) {
//				if(kk < barNo) {
//					k = kk;
//					break;
//				}
//			}
//			return k;
//		}
		
		/// <summary>
		/// Get the trade signal for the bar 
		/// </summary>
		/// <param name="barNo"></param>
		/// <returns></returns>
//		public TradeSignal GetTradeSignal(int barNo) {
//			TradeSignal signal;
//			if(!this.tradeSignals.TryGetValue(barNo, out signal))
//				return null;
//			else
//				return signal;
//		}

		/// <summary>
		/// Get the last trade signal before the barNo
		/// </summary>
		/// <param name="barNo"></param>
		/// <returns></returns>
//		public TradeSignal GetLastTradeSignal(int barNo) {
//			return GetTradeSignal(GetLastTradeSignalBarNo(barNo));
//		}
		
		/// <summary>
		/// Get the signal from bar with barNo and the signal_name
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signal_name"></param>
		/// <returns></returns>
//		public TradeSignal GetTradeSignalByName(int barNo, string signal_name) {
//			//if(list_signal != null) {
//				foreach(TradeSignal sig in this.tradeSignals.Values) {
//					if(signal_name.Equals(sig.SignalName))
//						return sig;
//				}
//			//}
			
//			return null;			
//		}

		/// <summary>
		/// Get the last signal before barNo by signal_name
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signal_name"></param>
		/// <returns></returns>
//		public TradeSignal GetLastTradeSignalByName(int barNo, string signal_name) {
//			int k = barNo;
//			foreach(int kk in this.tradeSignals.Keys.Reverse()) {
//				if(kk < k) {
//					TradeSignal sig = GetTradeSignalByName(k, signal_name);
//					if(sig != null) return sig;
//					k = kk;
//				}
//			}
//			return null;			
//		}

		/// <summary>
		/// Get the trade signal from bar with barNo and the signalActionType ???
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signalActionType"></param>
		/// <returns></returns>
//		public TradeSignal GetTradeSignalByActionType(int barNo, SignalActionType signal_actiontype) {
//			TradeSignal list_signal = GetTradeSignal(barNo);
//			if(list_signal != null) {
////				foreach(TradeSignal sig in list_signal) {
////					if(sig.Signal_Action != null && 
////						signal_actiontype.Equals(sig.Signal_Action.SignalActionType))
////						return sig;
////				}
//			}
			
//			return null;			
//		}

		/// <summary>
		/// Get the last signal before barNo by signalActionType ???
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signalActionType"></param>
		/// <returns></returns>
//		public TradeSignal GetLastTradeSignalByActionType(int barNo, SignalActionType signal_actiontype) {
//			int k = barNo;
//			foreach(int kk in this.tradeSignals.Keys.Reverse()) {				
//				if(kk < k) {
//					TradeSignal sig = GetTradeSignalByActionType(k, signal_actiontype);
//					if(sig != null) return sig;
//					k = kk;
//				}
//			}
//			return null;			
//		}
		
		/// <summary>
		/// Get the signal list for the bar by signal type ???
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signal_type"></param>
		/// <returns></returns>
//		public TradeSignal GetTradeSignalByType(int barNo, TradeSignalType signal_type) {
//			TradeSignal list_signal = GetTradeSignal(barNo);
//			if(list_signal != null) {				
//				TradeSignal list_sigByType = new TradeSignal();
////				foreach(TradeSignal sig in list_signal) {
////					if(signal_type == sig.TradeSignalType)
////						list_sigByType.Add(sig);
////				}
////				if(list_sigByType.Count > 0)
////					return list_sigByType;
//			}
			
//			return null;
//		}		

		/// <summary>
		/// Get last signal list before barNo by signal type ???
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signal_type"></param>
		/// <returns></returns>
//		public TradeSignal GetLastTradeSignalByType(int barNo, TradeSignalType signal_type) {
//			int k = barNo;
//			foreach(int kk in this.tradeSignals.Keys.Reverse()) {				
//				if(kk < k) {
//					TradeSignal sigs = GetTradeSignalByType(k, signal_type);
//					if(sigs != null) return sigs;
//					k = kk;
//				}
//			}
//			return null;		
//		}
		#endregion

        #region Properties

        #endregion		
	}
}
