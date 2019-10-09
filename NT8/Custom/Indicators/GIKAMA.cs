//
// Copyright (C) 2018, NinjaTrader LLC <www.ninjatrader.com>.
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
using NinjaTrader.NinjaScript.Indicators.PriceActions;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
#endregion

//This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Kaufman's Adaptive Moving Average. Developed by Perry Kaufman, this indicator is an
	/// EMA using an Efficiency Ratio to modify the smoothing constant, which ranges from
	/// a minimum of Fast Length to a maximum of Slow Length. Since this moving average is
	/// adaptive it tends to follow prices more closely than other MA's.
	/// </summary>
	public class GIKAMA : GIndicatorBase
	{
		private Series<double>	diffSeries;
		private double			fastCF;
		private double			slowCF;
		private SUM				sum;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionKAMA;
				Name						= NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameKAMA;
				Fast						= 2;
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				Period						= 10;
				Slow						= 30;

				AddPlot(Brushes.DodgerBlue, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameKAMA);
			}
			else if (State == State.Configure)
			{
				fastCF		= 2.0 / (Fast + 1);
				slowCF		= 2.0 / (Slow + 1);
			}
			else if (State == State.DataLoaded)
			{
				diffSeries = new Series<double>(this);
				sum = SUM(diffSeries, Period);
			}
		}

		protected override void OnBarUpdate()
		{
			double input0 = Input[0];
			diffSeries[0] = CurrentBar > 0 ? Math.Abs(input0 - Input[1]) : input0;

			if (CurrentBar < Period)
			{
				Value[0] = Input[0];
				return;
			}

			double signal = Math.Abs(input0 - Input[Period]);
			double noise  = sum[0];

			// Prevent div by zero
			if (noise == 0)
			{
				Value[0] = Value[1];
				return;
			}

			double value1   = Value[1];
			Value[0]		= value1 + Math.Pow((signal / noise) * (fastCF - slowCF) + slowCF, 2) * (input0 - value1);
		}
		

		/**
		the current price - KAMA, if the number is >= 0, we know the price is above KAMA,
		*/
		public override Direction GetDirection(){
			Direction tr = new Direction();
			double k = Math.Round(Value[0], 2);
			double dif = Close[0] - k;
			Print(CurrentBar + "-Kama dif=" + dif + ",kama=" + k + ",close=" + Close[0]);
			if (CurrentBar > Math.Max(Period, Slow)) {// BarsRequiredToPlot) {
				if(dif > 0)
					tr.TrendDir = TrendDirection.Up;
			else if(dif < 0)
					tr.TrendDir = TrendDirection.Down;
			}
			return tr;
		}		

		#region Properties
		[Range(1, 125), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Fast
		{ get; set; }

		[Range(5, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 1)]
		public int Period
		{ get; set; }

		[Range(1, 125), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Slow", GroupName = "NinjaScriptParameters", Order = 2)]
		public int Slow
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIKAMA[] cacheGIKAMA;
		public GIKAMA GIKAMA(int fast, int period, int slow)
		{
			return GIKAMA(Input, fast, period, slow);
		}

		public GIKAMA GIKAMA(ISeries<double> input, int fast, int period, int slow)
		{
			if (cacheGIKAMA != null)
				for (int idx = 0; idx < cacheGIKAMA.Length; idx++)
					if (cacheGIKAMA[idx] != null && cacheGIKAMA[idx].Fast == fast && cacheGIKAMA[idx].Period == period && cacheGIKAMA[idx].Slow == slow && cacheGIKAMA[idx].EqualsInput(input))
						return cacheGIKAMA[idx];
			return CacheIndicator<GIKAMA>(new GIKAMA(){ Fast = fast, Period = period, Slow = slow }, input, ref cacheGIKAMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIKAMA GIKAMA(int fast, int period, int slow)
		{
			return indicator.GIKAMA(Input, fast, period, slow);
		}

		public Indicators.GIKAMA GIKAMA(ISeries<double> input , int fast, int period, int slow)
		{
			return indicator.GIKAMA(input, fast, period, slow);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIKAMA GIKAMA(int fast, int period, int slow)
		{
			return indicator.GIKAMA(Input, fast, period, slow);
		}

		public Indicators.GIKAMA GIKAMA(ISeries<double> input , int fast, int period, int slow)
		{
			return indicator.GIKAMA(input, fast, period, slow);
		}
	}
}

#endregion
