//
// Copyright (C) 2019, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.AddOns;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Exponential Moving Average. The Exponential Moving Average is an indicator that
	/// shows the average value of a security's price over a period of time. When calculating
	/// a moving average. The EMA applies more weight to recent prices than the SMA.
	/// </summary>
	public class GIHLnBars : GIndicatorBase
	{
//		private double constant1;
//		private double constant2;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "The High/Low of last N bars";
				Name						= "GIHLnBars";
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				Period						= 5;
				Calculate					= Calculate.OnBarClose;
				AddPlot(Brushes.Goldenrod, "LowestN");
				AddPlot(Brushes.Goldenrod, "HighestN");
			}
			else if (State == State.Configure)
			{
//				constant1 = 2.0 / (1 + Period);
//				constant2 = 1 - (2.0 / (1 + Period));
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar > Period) {
				LowestN[0] = CurrentBar < Period ? Low[0] : GetLowestPrice(Period, false);
//				LowestN[0] = (CurrentBar < Period ? Low[0] : GetLowestPrice(Period, false));
				HighestN[0] = CurrentBar < Period ? High[0] : GetHighestPrice(Period, false);
				CheckBreakoutNBarsHLEvent();
				}

		}

		public double GetNBarsHLOffset(SupportResistanceType srt, double price) {
			double offset = 0;
			switch(srt) {
				case SupportResistanceType.Support:
					offset = price - LowestN[1];
					break;
				case SupportResistanceType.Resistance:
					offset = HighestN[1] - price;
					break;
			}
			
			return offset;
		}

		public void CheckBreakoutNBarsHLEvent() {
			if(CurrentBar-RefBarLowestN < 1 || CurrentBar-RefBarHighestN < 1)
				return;
			IndicatorSignal isig = new IndicatorSignal();
			//if(CurrentBar < 300)
				Print(String.Format("{0}:Close={1},RefBarLowestN={2},RefBarLowestN={3},LowestN={4},HighestN={5}",
				CurrentBar, Close[0], RefBarLowestN, RefBarHighestN,
				LowestN[CurrentBar-RefBarLowestN], HighestN[CurrentBar-RefBarHighestN]));
			if(Close[0] < LowestN[CurrentBar-RefBarLowestN]) {
				isig.BreakoutDir = BreakoutDirection.Down;
				isig.SignalName = SignalName_BreakoutNBarsLow;
			} else if(Close[0] > HighestN[CurrentBar-RefBarHighestN]) {
				isig.BreakoutDir = BreakoutDirection.Up;
				isig.SignalName = SignalName_BreakoutNBarsHigh;
			} else
				return;
			
			isig.BarNo = CurrentBar;
			isig.IndicatorSignalType = SignalType.SimplePriceAction;
			IndicatorEventArgs ievt = new IndicatorEventArgs(this.GetType().Name, " CheckBreakoutNBarsHLEvent: ");
			ievt.IndSignal = isig;
			//FireEvent(ievt);
			OnRaiseIndicatorEvent(ievt);
		}
		
		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LowestN
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HighestN
		{
			get { return Values[1]; }
		}
		
		/// <summary>
		/// The bar NO for the prior LowestN as reference
		/// Used to locate LowestN for entry bar 
		/// </summary>
		[Browsable(false)]
		[XmlIgnore]
		public int RefBarLowestN
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public int RefBarHighestN
		{ get; set; }
		#endregion

		#region Pre-defined signal name
		[Browsable(false), XmlIgnore]
		public string SignalName_BreakoutNBarsLow
		{
			get { return "BreakoutNBarsLow";}
		}

		[Browsable(false), XmlIgnore]
		public string SignalName_BreakoutNBarsHigh
		{
			get { return "BreakoutNBarsHigh";}
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIHLnBars[] cacheGIHLnBars;
		public GIHLnBars GIHLnBars(int period)
		{
			return GIHLnBars(Input, period);
		}

		public GIHLnBars GIHLnBars(ISeries<double> input, int period)
		{
			if (cacheGIHLnBars != null)
				for (int idx = 0; idx < cacheGIHLnBars.Length; idx++)
					if (cacheGIHLnBars[idx] != null && cacheGIHLnBars[idx].Period == period && cacheGIHLnBars[idx].EqualsInput(input))
						return cacheGIHLnBars[idx];
			return CacheIndicator<GIHLnBars>(new GIHLnBars(){ Period = period }, input, ref cacheGIHLnBars);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIHLnBars GIHLnBars(int period)
		{
			return indicator.GIHLnBars(Input, period);
		}

		public Indicators.GIHLnBars GIHLnBars(ISeries<double> input , int period)
		{
			return indicator.GIHLnBars(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIHLnBars GIHLnBars(int period)
		{
			return indicator.GIHLnBars(Input, period);
		}

		public Indicators.GIHLnBars GIHLnBars(ISeries<double> input , int period)
		{
			return indicator.GIHLnBars(input, period);
		}
	}
}

#endregion
