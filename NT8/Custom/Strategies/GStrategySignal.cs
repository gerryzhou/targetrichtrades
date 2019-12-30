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
		/// Hold the command/rule/performacne signals from the underlining strategy
		/// The key=BarNo that holds the signal set
		/// value=the set of signals
		/// </summary>
		private SortedDictionary<int, List<TradeSignal>> commandSignals = 
			new SortedDictionary<int, List<TradeSignal>>();

		/// <summary>
		/// Hold the event signals from the underlining strategy
		/// The key=BarNo that holds the signal set
		/// value=the set of signals
		/// </summary>
		private SortedDictionary<int, List<TradeSignal>> eventSignals = 
			new SortedDictionary<int, List<TradeSignal>>();

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
				case TradeSignalType.Liquidate: 
					return CheckExitSignals();
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
		
		public virtual TradeSignal SetEntrySignal() {return null;}
		
		public virtual TradeSignal SetStopLossSignal() {return null;}
		
		public virtual TradeSignal SetProfitTargetSignal() {return null;}
		
		/// <summary>
		/// Check trade signals from indicator signals
		/// </summary>
		/// <returns></returns>
		public virtual bool CheckTradeSignals() {
			if(HasPosition() != 0 && CheckIndicatorSignals(TradeSignalType.Liquidate)) {
				return SetExitTradeAction();
			}
			else if(NewOrderAllowed() && CheckIndicatorSignals(TradeSignalType.Entry))
			{
				IndicatorProxy.TraceMessage(this.Name, PrintOut);
				return SetNewEntryTradeAction();
			}

			return false;
		}
		
		public virtual double GetEntryPrice(SupportResistanceType srt) {
			return 0;
		}
		
		public virtual double GetStopLossPrice(SupportResistanceType srt) {
			return 0;
		}
		
		public virtual double GetProfitTargetPrice(SupportResistanceType srt) {
			return 0;
		}
		
		#endregion

		#region Command/Event signals function
		public void AddTradeSignals(int barNo, SortedDictionary<int, List<TradeSignal>> listSignals, List<TradeSignal> signals) {
			listSignals.Add(barNo, signals);
		}
		
		/// <summary>
		/// Add the signal to the list of the bar with barNo
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signal"></param>
		public void AddTradeSignal(int barNo, SortedDictionary<int, List<TradeSignal>> listSignals, TradeSignal signal) {
			List<TradeSignal> list_signal;
			if(!listSignals.TryGetValue(barNo, out list_signal)) {
				list_signal = new List<TradeSignal>();
			}
			list_signal.Add(signal);
			listSignals[barNo] = list_signal;
		}
		
		public void AddTradeSignal(int barNo, SortedDictionary<int, List<TradeSignal>> listSignals, TradeSignalType tdSigType,
			TradeSignalSource tdSigSrc, OrderAction ordAct, OrderType ordType) {
			TradeSignal isig = new TradeSignal();
			isig.BarNo = barNo;
			isig.SignalType = tdSigType;
			isig.SignalSource = tdSigSrc;
			isig.Action = ordAct;
			isig.Order_Type = ordType;
			AddTradeSignal(barNo, listSignals, isig);
		}

		public void RemoveTradeSignal(int barNo, SortedDictionary<int, List<TradeSignal>> listSignals, string signame) { //SignalActionType saType, SupportResistanceRange<double> snr
			List<TradeSignal> list_signal;
			if(listSignals.TryGetValue(barNo, out list_signal)) {
				foreach(TradeSignal s in list_signal) {
					if(signame.Equals(s.SignalName)) {
						Print(CurrentBar + ": Removed--" + s.BarNo + "," + s.SignalName);
						list_signal.Remove(s);
						break;
					}
				}
			}		

			listSignals[barNo] = list_signal;
		}
			
		public int GetLastTradeSignalBarNo(int barNo, SortedDictionary<int, List<TradeSignal>> listSignals) {
			int k = -1;
			foreach(int kk in listSignals.Keys.Reverse()) {
				if(kk < barNo) {
					k = kk;
					break;
				}
			}
			return k;
		}
		
		/// <summary>
		/// Get the signal list for the bar 
		/// </summary>
		/// <param name="barNo"></param>
		/// <returns></returns>
		public List<TradeSignal> GetTradeSignals(int barNo, SortedDictionary<int, List<TradeSignal>> listSignals) {
			List<TradeSignal> list_signal;
			if(!listSignals.TryGetValue(barNo, out list_signal))
				return null;
			else
				return list_signal;
		}

		/// <summary>
		/// Get the last signal list before the barNo
		/// </summary>
		/// <param name="barNo"></param>
		/// <returns></returns>
		public List<TradeSignal> GetLastTradeSignals(int barNo, SortedDictionary<int, List<TradeSignal>> listSignals) {
			return GetTradeSignals(GetLastTradeSignalBarNo(barNo, listSignals), listSignals);
		}
		
		/// <summary>
		/// Get the signal from bar with barNo and the signal_name
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signal_name"></param>
		/// <returns></returns>
		public TradeSignal GetTradeSignalByName(int barNo, SortedDictionary<int, List<TradeSignal>> listSignals, string signal_name) {
			List<TradeSignal> list_signal = GetTradeSignals(barNo, listSignals);
			if(list_signal != null) {
				foreach(TradeSignal sig in list_signal) {
					if(signal_name.Equals(sig.SignalName))
						return sig;
				}
			}
			
			return null;			
		}

		/// <summary>
		/// Get the last signal before barNo by signal_name
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signal_name"></param>
		/// <returns></returns>
		public TradeSignal GetLastTradeSignalByName(int barNo, SortedDictionary<int, List<TradeSignal>> listSignals, string signal_name) {
			int k = barNo;
			foreach(int kk in listSignals.Keys.Reverse()) {
				if(kk < k) {
					TradeSignal sig = GetTradeSignalByName(k, listSignals, signal_name);
					if(sig != null) return sig;
					k = kk;
				}
			}
			return null;			
		}
		
		/// <summary>
		/// Get the signal list for the bar by signal type
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signal_type"></param>
		/// <returns></returns>
		public List<TradeSignal> GetTradeSignalByType(int barNo, SortedDictionary<int, List<TradeSignal>> listSignals, TradeSignalType signal_type) {
			List<TradeSignal> list_signal = GetTradeSignals(barNo, listSignals);
			if(list_signal != null) {				
				List<TradeSignal> list_sigByType = new List<TradeSignal>();
				foreach(TradeSignal sig in list_signal) {
					if(signal_type == sig.SignalType)
						list_sigByType.Add(sig);
				}
				if(list_sigByType.Count > 0)
					return list_sigByType;
			}
			
			return null;
		}		

		/// <summary>
		/// Get last signal list before barNo by signal type
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signal_type"></param>
		/// <returns></returns>
		public List<TradeSignal> GetLastTradeSignalByType(int barNo, SortedDictionary<int, List<TradeSignal>> listSignals, TradeSignalType signal_type) {
			int k = barNo;
			foreach(int kk in listSignals.Keys.Reverse()) {
				if(kk < k) {
					List<TradeSignal> sigs = GetTradeSignalByType(k, listSignals, signal_type);
					if(sigs != null) return sigs;
					k = kk;
				}
			}
			return null;		
		}
		
		/// <summary>
		/// Get the signal list for the bar by signal source
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signal_type"></param>
		/// <returns></returns>
		public List<TradeSignal> GetTradeSignalBySource(int barNo, SortedDictionary<int, List<TradeSignal>> listSignals, TradeSignalSource signal_src) {
			List<TradeSignal> list_signal = GetTradeSignals(barNo, listSignals);
			if(list_signal != null) {
				List<TradeSignal> list_sigByType = new List<TradeSignal>();
				foreach(TradeSignal sig in list_signal) {
					if(signal_src == sig.SignalSource)
						list_sigByType.Add(sig);
				}
				if(list_sigByType.Count > 0)
					return list_sigByType;
			}
			
			return null;
		}		

		/// <summary>
		/// Get last signal list before barNo by signal source
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signal_type"></param>
		/// <returns></returns>
		public List<TradeSignal> GetLastTradeSignalBySource(int barNo, SortedDictionary<int, List<TradeSignal>> listSignals, TradeSignalSource signal_src) {
			int k = barNo;
			foreach(int kk in listSignals.Keys.Reverse()) {
				if(kk < k) {
					List<TradeSignal> sigs = GetTradeSignalBySource(k, listSignals, signal_src);
					if(sigs != null) return sigs;
					k = kk;
				}
			}
			return null;		
		}
		
		/// <summary>
		/// Get the signal from bar with barNo and the OrderActionType
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signalActionType"></param>
		/// <returns></returns>
		public TradeSignal GetTradeSignalByActionType(int barNo, SortedDictionary<int, List<TradeSignal>> listSignals, OrderAction ord_actiontype) {
			List<TradeSignal> list_signal = GetTradeSignals(barNo, listSignals);
			if(list_signal != null) {
				foreach(TradeSignal sig in list_signal) {
					if(sig.Action != null && 
						ord_actiontype.Equals(sig.Action))
						return sig;
				}
			}
			
			return null;			
		}

		/// <summary>
		/// Get the last signal before barNo by OrderActionType
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signalActionType"></param>
		/// <returns></returns>
		public TradeSignal GetLastTradeSignalByActionType(int barNo, SortedDictionary<int, List<TradeSignal>> listSignals, OrderAction ord_actiontype) {
			int k = barNo;
			foreach(int kk in listSignals.Keys.Reverse()) {				
				if(kk < k) {
					Print(CurrentBar + ": kk,k=" + kk + "," + k);
					TradeSignal sig = GetTradeSignalByActionType(k, listSignals, ord_actiontype);
					if(sig != null) return sig;
					k = kk;
				}
			}
			return null;			
		}
		
		/// <summary>
		/// Get the signal from bar with barNo and the OrderType
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signalActionType"></param>
		/// <returns></returns>
		public TradeSignal GetTradeSignalByOrderType(int barNo, SortedDictionary<int, List<TradeSignal>> listSignals, OrderType ord_type) {
			List<TradeSignal> list_signal = GetTradeSignals(barNo, listSignals);
			if(list_signal != null) {
				foreach(TradeSignal sig in list_signal) {
					if(sig.Order_Type != null && 
						ord_type.Equals(sig.Order_Type))
						return sig;
				}
			}
			
			return null;			
		}

		/// <summary>
		/// Get the last signal before barNo by OrderType
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signalActionType"></param>
		/// <returns></returns>
		public TradeSignal GetLastTradeSignalByOrderType(int barNo, SortedDictionary<int, List<TradeSignal>> listSignals, OrderType ord_type) {
			int k = barNo;
			foreach(int kk in listSignals.Keys.Reverse()) {				
				if(kk < k) {
					Print(CurrentBar + ": kk,k=" + kk + "," + k);
					TradeSignal sig = GetTradeSignalByOrderType(k, listSignals, ord_type);
					if(sig != null) return sig;
					k = kk;
				}
			}
			return null;			
		}
		
		public void PrintTradeSignal(SortedDictionary<int, List<TradeSignal>> listSignals) {
			foreach(KeyValuePair<int, List<TradeSignal>> sig in listSignals) {
				List<TradeSignal> list_signal = sig.Value;
				int key = sig.Key;
				if(list_signal != null) {
					foreach(TradeSignal s in list_signal) {
						Print(key + ":" + s.BarNo + "," + s.SignalName);
					}
				}
			}
		}
		#endregion
        
		#region Properties

        #endregion		
	}
}
