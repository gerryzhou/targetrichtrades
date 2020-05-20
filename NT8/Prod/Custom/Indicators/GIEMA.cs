//
// Copyright (C) 2019, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.NinjaScript.Indicators;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    /// <summary>
    /// Exponential Moving Average. The Exponential Moving Average is an indicator that
    /// shows the average value of a security's price over a period of time. When calculating
    /// a moving average. The EMA applies more weight to recent prices than the SMA.
    /// </summary>
    public class GIEMA : GIndicatorBase
	{
		private double constant1;
		private double constant2;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionEMA;
				Name						= NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameEMA;
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				Period						= 14;

				AddPlot(Brushes.Goldenrod, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameEMA);
			}
			else if (State == State.Configure)
			{
				constant1 = 2.0 / (1 + Period);
				constant2 = 1 - (2.0 / (1 + Period));
			}
		}

		protected override void OnBarUpdate()
		{
			Value[0] = (CurrentBar == 0 ? Input[0] : Input[0] * constant1 + constant2 * Value[1]);
			if(CurrentBar > Period)
				CheckBreakoutEmaTicsEvent();
		}
		
		public double GetEmaOffset(SupportResistanceType srt, double price) {
			double offset = 0;
			switch(srt) {
				case SupportResistanceType.Support:
					offset = price - Value[1] - GetPriceByTicks(OffsetTicks);
					break;
				case SupportResistanceType.Resistance:
					offset = Value[1] + GetPriceByTicks(OffsetTicks) - price;
					break;
			}
			
			return offset;
		}

		public void CheckBreakoutEmaTicsEvent() {
			IndicatorSignal isig = new IndicatorSignal();
			//if(CurrentBar < 300)
				Print(String.Format("{0}:Close={1},EMA={2},OffsetTicks={3}",
				CurrentBar, Close[0], Value[0], OffsetTicks));
			if(Close[0] < Value[1] - GetPriceByTicks(OffsetTicks)) {
				isig.BreakoutDir = BreakoutDirection.Down;
				isig.SignalName = SignalName_BreakoutEmaDownTics;
			} else if(Close[0] > Value[1] + GetPriceByTicks(OffsetTicks)) {
				isig.BreakoutDir = BreakoutDirection.Up;
				isig.SignalName = SignalName_BreakoutEmaUpTics;
			} else
				return;
			
			isig.BarNo = CurrentBar;
			isig.IndicatorSignalType = SignalType.SimplePriceAction;
			IndicatorEventArgs ievt = new IndicatorEventArgs(this.GetType().Name, " CheckBreakoutEmaTicsEvent: ");
			ievt.IndSignal = isig;
			//FireEvent(ievt);
			OnRaiseIndicatorEvent(ievt);
		}
		
		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }
		
		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "OffsetTicks", GroupName = "NinjaScriptParameters", Order = 1)]
		public int OffsetTicks
		{ get; set; }
		#endregion

		#region Pre-defined signal name
		[Browsable(false), XmlIgnore]
		public string SignalName_BreakoutEmaDownTics
		{
			get { return "BreakoutEmaDownTics";}
		}

		[Browsable(false), XmlIgnore]
		public string SignalName_BreakoutEmaUpTics
		{
			get { return "BreakoutEmaUpTics";}
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIEMA[] cacheGIEMA;
		public GIEMA GIEMA(int period, int offsetTicks)
		{
			return GIEMA(Input, period, offsetTicks);
		}

		public GIEMA GIEMA(ISeries<double> input, int period, int offsetTicks)
		{
			if (cacheGIEMA != null)
				for (int idx = 0; idx < cacheGIEMA.Length; idx++)
					if (cacheGIEMA[idx] != null && cacheGIEMA[idx].Period == period && cacheGIEMA[idx].OffsetTicks == offsetTicks && cacheGIEMA[idx].EqualsInput(input))
						return cacheGIEMA[idx];
			return CacheIndicator<GIEMA>(new GIEMA(){ Period = period, OffsetTicks = offsetTicks }, input, ref cacheGIEMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIEMA GIEMA(int period, int offsetTicks)
		{
			return indicator.GIEMA(Input, period, offsetTicks);
		}

		public Indicators.GIEMA GIEMA(ISeries<double> input , int period, int offsetTicks)
		{
			return indicator.GIEMA(input, period, offsetTicks);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIEMA GIEMA(int period, int offsetTicks)
		{
			return indicator.GIEMA(Input, period, offsetTicks);
		}

		public Indicators.GIEMA GIEMA(ISeries<double> input , int period, int offsetTicks)
		{
			return indicator.GIEMA(input, period, offsetTicks);
		}
	}
}

#endregion
