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
    /// MACross not good to use: two or more big bars already appeared at cross;
	/// Too lagging: good for a trend indicator but not for breakout;
    /// </summary>
    public class GIMACross : GIndicatorBase
	{
		private GIEMA fastEMA;
		private GIEMA slowEMA;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "MA cross indicator";
				Name						= "GIMACross";
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				PeriodFast					= 21;
				PeriodSlow					= 55;

				AddPlot(Brushes.Cyan, "MACross Fast");
				AddPlot(Brushes.Gold, "MACross Slow");
			}
			else if (State == State.Configure)
			{
				fastEMA = GIEMA(PeriodFast, 0);
				slowEMA = GIEMA(PeriodSlow, 0);
				BarsSinceLastCross = new Series<int>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar > PeriodSlow) {
				FastEMA[0] = fastEMA[0];
				SlowEMA[0] = slowEMA[0];
				CheckMACrossEvent();
			}
		}
		
		public SignalActionType GetMACross() {
			SignalActionType isCross = SignalActionType.Unknown;
			if (CrossAbove(fastEMA, slowEMA, 1))
				isCross = SignalActionType.CrossAbove;
			else if (CrossBelow(fastEMA, slowEMA, 1))
				isCross = SignalActionType.CrossBelow;
			return isCross;
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

		public void CheckMACrossEvent() {
			IndicatorSignal isig = new IndicatorSignal();
			SignalActionType isCross = GetMACross();
			if(isCross != SignalActionType.Unknown) {
				Print(String.Format("{0}:isCross={1}, BarsSinceLastCross={2}",
				CurrentBar, isCross, BarsSinceLastCross[1]));
				BarsSinceLastCross[0] = 0;
			} else
				BarsSinceLastCross[0] = BarsSinceLastCross[1] + 1;
			return;
			
			if(isCross == SignalActionType.CrossAbove) {
				isig.BreakoutDir = BreakoutDirection.Up;
				isig.SignalName = SignalName_CrossAbove;				
			} else if(isCross == SignalActionType.CrossBelow) {
				isig.BreakoutDir = BreakoutDirection.Down;
				isig.SignalName = SignalName_CrossBelow;
			} else
				return;
			
			isig.BarNo = CurrentBar;
			isig.IndicatorSignalType = SignalType.SimplePriceAction;
			IndicatorEventArgs ievt = new IndicatorEventArgs(this.GetType().Name, " CheckMACrossEvent: ");
			ievt.IndSignal = isig;
			//FireEvent(ievt);
			OnRaiseIndicatorEvent(ievt);
		}
		
		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PeriodSlow", GroupName = "NinjaScriptParameters", Order = 0)]
		public int PeriodSlow
		{ get; set; }
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PeriodFast", GroupName = "NinjaScriptParameters", Order = 1)]
		public int PeriodFast
		{ get; set; }

		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "OffsetTicks", GroupName = "NinjaScriptParameters", Order = 2)]
		public int OffsetTicks
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> FastEMA
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SlowEMA
		{
			get { return Values[1]; }
		}
		
		//Bar count since last cross
		[Browsable(false)]
		[XmlIgnore()]
		public Series<int> BarsSinceLastCross
		{ get; set; }
		#endregion
		
		#region Pre-defined signal name
		/*
		[Browsable(false), XmlIgnore]
		public string SignalName_BreakoutEmaDownTics
		{
			get { return "BreakoutEmaDownTics";}
		}

		[Browsable(false), XmlIgnore]
		public string SignalName_BreakoutEmaUpTics
		{
			get { return "BreakoutEmaUpTics";}
		} */
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIMACross[] cacheGIMACross;
		public GIMACross GIMACross(int periodSlow, int periodFast, int offsetTicks)
		{
			return GIMACross(Input, periodSlow, periodFast, offsetTicks);
		}

		public GIMACross GIMACross(ISeries<double> input, int periodSlow, int periodFast, int offsetTicks)
		{
			if (cacheGIMACross != null)
				for (int idx = 0; idx < cacheGIMACross.Length; idx++)
					if (cacheGIMACross[idx] != null && cacheGIMACross[idx].PeriodSlow == periodSlow && cacheGIMACross[idx].PeriodFast == periodFast && cacheGIMACross[idx].OffsetTicks == offsetTicks && cacheGIMACross[idx].EqualsInput(input))
						return cacheGIMACross[idx];
			return CacheIndicator<GIMACross>(new GIMACross(){ PeriodSlow = periodSlow, PeriodFast = periodFast, OffsetTicks = offsetTicks }, input, ref cacheGIMACross);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIMACross GIMACross(int periodSlow, int periodFast, int offsetTicks)
		{
			return indicator.GIMACross(Input, periodSlow, periodFast, offsetTicks);
		}

		public Indicators.GIMACross GIMACross(ISeries<double> input , int periodSlow, int periodFast, int offsetTicks)
		{
			return indicator.GIMACross(input, periodSlow, periodFast, offsetTicks);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIMACross GIMACross(int periodSlow, int periodFast, int offsetTicks)
		{
			return indicator.GIMACross(Input, periodSlow, periodFast, offsetTicks);
		}

		public Indicators.GIMACross GIMACross(ISeries<double> input , int periodSlow, int periodFast, int offsetTicks)
		{
			return indicator.GIMACross(input, periodSlow, periodFast, offsetTicks);
		}
	}
}

#endregion
