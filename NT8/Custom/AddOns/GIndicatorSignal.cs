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
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Indicators.PriceActions;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The proxy indicator that carry general indicator methods to strategy;
	/// All the methods those only need to impact the ZTrader framework will be implemented here.
	/// </summary>
	public partial class GIndicatorBase : Indicator
	{
		/// <summary>
		/// Hold the indicator signal from the underlining indicator
		/// The key=BarNo that holds the signal set
		/// value=the set of signals
		/// </summary>
		private SortedDictionary<int, List<IndicatorSignal>> indicatorSignals = 
			new SortedDictionary<int, List<IndicatorSignal>>();
		
		#region Methods
		
		public virtual IndicatorSignal CheckIndicatorSignal() {
			return null;
		}
		
		public void AddIndicatorSignals(int barNo, List<IndicatorSignal> signals) {
			this.indicatorSignals.Add(barNo, signals);
		}
		
		/// <summary>
		/// Add the signal to the list of the bar with barNo
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signal"></param>
		public void AddIndicatorSignal(int barNo, IndicatorSignal signal) {
			List<IndicatorSignal> list_signal;
			if(!this.indicatorSignals.TryGetValue(barNo, out list_signal)) {				
				list_signal = new List<IndicatorSignal>();
			}
			list_signal.Add(signal);
			this.indicatorSignals[barNo] = list_signal;
		}
		
		public void AddIndicatorSignal(int barNo, string signame,
			SignalActionType saType, SupportResistanceRange<double> snr) {
			
			SignalAction sa = new SignalAction();
			sa.SignalActionType = saType;
			sa.SnR = snr;
			IndicatorSignal isig = new IndicatorSignal();
			isig.BarNo = barNo;
			isig.SignalName = signame;
			isig.IndicatorSignalType = SignalType.SimplePriceAction;
			isig.Signal_Action = sa;
			AddIndicatorSignal(barNo, isig);
		}

		public int GetLastIndSignalBarNo(int barNo) {
			int k = -1;
			foreach(int kk in this.indicatorSignals.Keys.Reverse()) {
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
		public List<IndicatorSignal> GetIndicatorSignals(int barNo) {
			List<IndicatorSignal> list_signal;
			if(!this.indicatorSignals.TryGetValue(barNo, out list_signal))			
				return null;
			else
				return list_signal;
		}

		/// <summary>
		/// Get the last signal list before the barNo
		/// </summary>
		/// <param name="barNo"></param>
		/// <returns></returns>
		public List<IndicatorSignal> GetLastIndicatorSignals(int barNo) {
			return GetIndicatorSignals(GetLastIndSignalBarNo(barNo));
		}
		
		/// <summary>
		/// Get the signal from bar with barNo and the signal_name
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signal_name"></param>
		/// <returns></returns>
		public IndicatorSignal GetIndicatorSignalByName(int barNo, string signal_name) {
			List<IndicatorSignal> list_signal = GetIndicatorSignals(barNo);
			if(list_signal != null) {				
				foreach(IndicatorSignal sig in list_signal) {
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
		public IndicatorSignal GetLastIndicatorSignalByName(int barNo, string signal_name) {
			int k = barNo;
			foreach(int kk in this.indicatorSignals.Keys.Reverse()) {				
				if(kk < k) {
					IndicatorSignal sig = GetIndicatorSignalByName(k, signal_name);
					if(sig != null) return sig;
					k = kk;
				}
			}
			return null;			
		}

		/// <summary>
		/// Get the signal from bar with barNo and the signalActionType
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signalActionType"></param>
		/// <returns></returns>
		public IndicatorSignal GetIndicatorSignalByActionType(int barNo, SignalActionType signal_actiontype) {
			List<IndicatorSignal> list_signal = GetIndicatorSignals(barNo);
			if(list_signal != null) {				
				foreach(IndicatorSignal sig in list_signal) {
					if(sig.Signal_Action != null && 
						signal_actiontype.Equals(sig.Signal_Action.SignalActionType))
						return sig;
				}
			}
			
			return null;			
		}

		/// <summary>
		/// Get the last signal before barNo by signalActionType
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signalActionType"></param>
		/// <returns></returns>
		public IndicatorSignal GetLastIndicatorSignalByActionType(int barNo, SignalActionType signal_actiontype) {
			int k = barNo;
			foreach(int kk in this.indicatorSignals.Keys.Reverse()) {				
				if(kk < k) {
					IndicatorSignal sig = GetIndicatorSignalByActionType(k, signal_actiontype);
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
		public List<IndicatorSignal> GetIndicatorSignalByType(int barNo, SignalType signal_type) {
			List<IndicatorSignal> list_signal = GetIndicatorSignals(barNo);
			if(list_signal != null) {				
				List<IndicatorSignal> list_sigByType = new List<IndicatorSignal>();
				foreach(IndicatorSignal sig in list_signal) {
					if(signal_type == sig.IndicatorSignalType)
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
		public List<IndicatorSignal> GetLastIndicatorSignalByType(int barNo, SignalType signal_type) {
			int k = barNo;
			foreach(int kk in this.indicatorSignals.Keys.Reverse()) {				
				if(kk < k) {
					List<IndicatorSignal> sigs = GetIndicatorSignalByType(k, signal_type);
					if(sigs != null) return sigs;
					k = kk;
				}
			}
			return null;		
		}		
		#endregion
	}
}
