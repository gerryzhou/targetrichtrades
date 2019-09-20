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
		private List<Indicator> listIndicator = new List<Indicator>();
		//private IndicatorSignal indSignal;
		
		/// <summary>
		/// Hold the indicator signal from the underlining indicator
		/// The key=BarNo that holds the signal set
		/// value=the set of signals
		/// </summary>
		private SortedDictionary<int, List<IndicatorSignal>> indicatorSignals = 
			new SortedDictionary<int, List<IndicatorSignal>>();
		
		#region Methods
		
		public Volatility GetVolatility() {return null;}
		
		public MarketCycle GetMarketCycle() {return null;}
		
		public virtual Direction GetDirection() {return null;}
		
		//public virtual SupportResistance GetSupport(){return null;}
		//public virtual SupportResistance GetResistance(){return null;}
		public virtual SupportResistanceBar GetSupportResistance(int barNo, SupportResistanceType srType){return null;}
		public virtual SupportResistanceRange<SupportResistanceBar> GetSupportResistance(int barNo){return null;}

		public virtual DivergenceType CheckDivergence() {return DivergenceType.UnKnown;}
		
		public IndicatorSignal CheckIndicatorSignal() {
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
		
		public void AddIndicatorSignal(int barNo, string signame, SignalActionType saType, SupportResistanceRange<double> snr) {
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
		/// Get the signal from bar with barNo and the signal_name
		/// </summary>
		/// <param name="barNo"></param>
		/// <param name="signal_name"></param>
		/// <returns></returns>
		public IndicatorSignal GetIndicatorSignalByName(int barNo, string signal_name) {
			
			if(this.indicatorSignals.ContainsKey(barNo)) {
				List<IndicatorSignal> list_signal = this.indicatorSignals[barNo];
				foreach(IndicatorSignal sig in list_signal) {
					if(signal_name.Equals(sig.SignalName))
						return sig;
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
			
			if(this.indicatorSignals.ContainsKey(barNo)) {
				List<IndicatorSignal> list_signal = this.indicatorSignals[barNo];
				List<IndicatorSignal> list_sigByType = new List<IndicatorSignal>();
				foreach(IndicatorSignal sig in list_signal) {
					if(signal_type == sig.IndicatorSignalType)
						list_sigByType.Add(sig);
				}
				return list_sigByType;
			}
			
			return null;			
		}		
		
		public void AddIndicator(Indicator i) {
			this.listIndicator.Add(i);
		}

		#endregion				
	}
}
