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

        #region Properties

        #endregion		
	}
}
