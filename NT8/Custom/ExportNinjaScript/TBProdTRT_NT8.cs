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
using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.Core.FloatingPoint;

#endregion



#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		
		private GIEMA[] cacheGIEMA;
		private GIFibonacci[] cacheGIFibonacci;
		private GIGetHighLowByTimeRange[] cacheGIGetHighLowByTimeRange;
		private GIHLnBars[] cacheGIHLnBars;
		private GIndicatorProxy[] cacheGIndicatorProxy;
		private GISnR[] cacheGISnR;
		private GIVWAP[] cacheGIVWAP;

		
		public GIEMA GIEMA(int period, int offsetTicks)
		{
			return GIEMA(Input, period, offsetTicks);
		}

		public GIFibonacci GIFibonacci(int param1)
		{
			return GIFibonacci(Input, param1);
		}

		public GIGetHighLowByTimeRange GIGetHighLowByTimeRange(int startHour, int startMinute, int endHour, int endMinute)
		{
			return GIGetHighLowByTimeRange(Input, startHour, startMinute, endHour, endMinute);
		}

		public GIHLnBars GIHLnBars(int period)
		{
			return GIHLnBars(Input, period);
		}

		public GIndicatorProxy GIndicatorProxy(GStrategyBase gSZTrader)
		{
			return GIndicatorProxy(Input, gSZTrader);
		}

		public GISnR GISnR(bool showOvernightHL, bool showOpenHL, bool showLastdayHL, bool showLastdayClose, bool showTodayOpen, int timeOpen, int timeClose)
		{
			return GISnR(Input, showOvernightHL, showOpenHL, showLastdayHL, showLastdayClose, showTodayOpen, timeOpen, timeClose);
		}

		public GIVWAP GIVWAP()
		{
			return GIVWAP(Input);
		}


		
		public GIEMA GIEMA(ISeries<double> input, int period, int offsetTicks)
		{
			if (cacheGIEMA != null)
				for (int idx = 0; idx < cacheGIEMA.Length; idx++)
					if (cacheGIEMA[idx].Period == period && cacheGIEMA[idx].OffsetTicks == offsetTicks && cacheGIEMA[idx].EqualsInput(input))
						return cacheGIEMA[idx];
			return CacheIndicator<GIEMA>(new GIEMA(){ Period = period, OffsetTicks = offsetTicks }, input, ref cacheGIEMA);
		}

		public GIFibonacci GIFibonacci(ISeries<double> input, int param1)
		{
			if (cacheGIFibonacci != null)
				for (int idx = 0; idx < cacheGIFibonacci.Length; idx++)
					if (cacheGIFibonacci[idx].Param1 == param1 && cacheGIFibonacci[idx].EqualsInput(input))
						return cacheGIFibonacci[idx];
			return CacheIndicator<GIFibonacci>(new GIFibonacci(){ Param1 = param1 }, input, ref cacheGIFibonacci);
		}

		public GIGetHighLowByTimeRange GIGetHighLowByTimeRange(ISeries<double> input, int startHour, int startMinute, int endHour, int endMinute)
		{
			if (cacheGIGetHighLowByTimeRange != null)
				for (int idx = 0; idx < cacheGIGetHighLowByTimeRange.Length; idx++)
					if (cacheGIGetHighLowByTimeRange[idx].StartHour == startHour && cacheGIGetHighLowByTimeRange[idx].StartMinute == startMinute && cacheGIGetHighLowByTimeRange[idx].EndHour == endHour && cacheGIGetHighLowByTimeRange[idx].EndMinute == endMinute && cacheGIGetHighLowByTimeRange[idx].EqualsInput(input))
						return cacheGIGetHighLowByTimeRange[idx];
			return CacheIndicator<GIGetHighLowByTimeRange>(new GIGetHighLowByTimeRange(){ StartHour = startHour, StartMinute = startMinute, EndHour = endHour, EndMinute = endMinute }, input, ref cacheGIGetHighLowByTimeRange);
		}

		public GIHLnBars GIHLnBars(ISeries<double> input, int period)
		{
			if (cacheGIHLnBars != null)
				for (int idx = 0; idx < cacheGIHLnBars.Length; idx++)
					if (cacheGIHLnBars[idx].Period == period && cacheGIHLnBars[idx].EqualsInput(input))
						return cacheGIHLnBars[idx];
			return CacheIndicator<GIHLnBars>(new GIHLnBars(){ Period = period }, input, ref cacheGIHLnBars);
		}

		public GIndicatorProxy GIndicatorProxy(ISeries<double> input, GStrategyBase gSZTrader)
		{
			if (cacheGIndicatorProxy != null)
				for (int idx = 0; idx < cacheGIndicatorProxy.Length; idx++)
					if (cacheGIndicatorProxy[idx].GSZTrader == gSZTrader && cacheGIndicatorProxy[idx].EqualsInput(input))
						return cacheGIndicatorProxy[idx];
			return CacheIndicator<GIndicatorProxy>(new GIndicatorProxy(){ GSZTrader = gSZTrader }, input, ref cacheGIndicatorProxy);
		}

		public GISnR GISnR(ISeries<double> input, bool showOvernightHL, bool showOpenHL, bool showLastdayHL, bool showLastdayClose, bool showTodayOpen, int timeOpen, int timeClose)
		{
			if (cacheGISnR != null)
				for (int idx = 0; idx < cacheGISnR.Length; idx++)
					if (cacheGISnR[idx].ShowOvernightHL == showOvernightHL && cacheGISnR[idx].ShowOpenHL == showOpenHL && cacheGISnR[idx].ShowLastdayHL == showLastdayHL && cacheGISnR[idx].ShowLastdayClose == showLastdayClose && cacheGISnR[idx].ShowTodayOpen == showTodayOpen && cacheGISnR[idx].TimeOpen == timeOpen && cacheGISnR[idx].TimeClose == timeClose && cacheGISnR[idx].EqualsInput(input))
						return cacheGISnR[idx];
			return CacheIndicator<GISnR>(new GISnR(){ ShowOvernightHL = showOvernightHL, ShowOpenHL = showOpenHL, ShowLastdayHL = showLastdayHL, ShowLastdayClose = showLastdayClose, ShowTodayOpen = showTodayOpen, TimeOpen = timeOpen, TimeClose = timeClose }, input, ref cacheGISnR);
		}

		public GIVWAP GIVWAP(ISeries<double> input)
		{
			if (cacheGIVWAP != null)
				for (int idx = 0; idx < cacheGIVWAP.Length; idx++)
					if ( cacheGIVWAP[idx].EqualsInput(input))
						return cacheGIVWAP[idx];
			return CacheIndicator<GIVWAP>(new GIVWAP(), input, ref cacheGIVWAP);
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

		public Indicators.GIFibonacci GIFibonacci(int param1)
		{
			return indicator.GIFibonacci(Input, param1);
		}

		public Indicators.GIGetHighLowByTimeRange GIGetHighLowByTimeRange(int startHour, int startMinute, int endHour, int endMinute)
		{
			return indicator.GIGetHighLowByTimeRange(Input, startHour, startMinute, endHour, endMinute);
		}

		public Indicators.GIHLnBars GIHLnBars(int period)
		{
			return indicator.GIHLnBars(Input, period);
		}

		public Indicators.GIndicatorProxy GIndicatorProxy(GStrategyBase gSZTrader)
		{
			return indicator.GIndicatorProxy(Input, gSZTrader);
		}

		public Indicators.GISnR GISnR(bool showOvernightHL, bool showOpenHL, bool showLastdayHL, bool showLastdayClose, bool showTodayOpen, int timeOpen, int timeClose)
		{
			return indicator.GISnR(Input, showOvernightHL, showOpenHL, showLastdayHL, showLastdayClose, showTodayOpen, timeOpen, timeClose);
		}

		public Indicators.GIVWAP GIVWAP()
		{
			return indicator.GIVWAP(Input);
		}


		
		public Indicators.GIEMA GIEMA(ISeries<double> input , int period, int offsetTicks)
		{
			return indicator.GIEMA(input, period, offsetTicks);
		}

		public Indicators.GIFibonacci GIFibonacci(ISeries<double> input , int param1)
		{
			return indicator.GIFibonacci(input, param1);
		}

		public Indicators.GIGetHighLowByTimeRange GIGetHighLowByTimeRange(ISeries<double> input , int startHour, int startMinute, int endHour, int endMinute)
		{
			return indicator.GIGetHighLowByTimeRange(input, startHour, startMinute, endHour, endMinute);
		}

		public Indicators.GIHLnBars GIHLnBars(ISeries<double> input , int period)
		{
			return indicator.GIHLnBars(input, period);
		}

		public Indicators.GIndicatorProxy GIndicatorProxy(ISeries<double> input , GStrategyBase gSZTrader)
		{
			return indicator.GIndicatorProxy(input, gSZTrader);
		}

		public Indicators.GISnR GISnR(ISeries<double> input , bool showOvernightHL, bool showOpenHL, bool showLastdayHL, bool showLastdayClose, bool showTodayOpen, int timeOpen, int timeClose)
		{
			return indicator.GISnR(input, showOvernightHL, showOpenHL, showLastdayHL, showLastdayClose, showTodayOpen, timeOpen, timeClose);
		}

		public Indicators.GIVWAP GIVWAP(ISeries<double> input )
		{
			return indicator.GIVWAP(input);
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

		public Indicators.GIFibonacci GIFibonacci(int param1)
		{
			return indicator.GIFibonacci(Input, param1);
		}

		public Indicators.GIGetHighLowByTimeRange GIGetHighLowByTimeRange(int startHour, int startMinute, int endHour, int endMinute)
		{
			return indicator.GIGetHighLowByTimeRange(Input, startHour, startMinute, endHour, endMinute);
		}

		public Indicators.GIHLnBars GIHLnBars(int period)
		{
			return indicator.GIHLnBars(Input, period);
		}

		public Indicators.GIndicatorProxy GIndicatorProxy(GStrategyBase gSZTrader)
		{
			return indicator.GIndicatorProxy(Input, gSZTrader);
		}

		public Indicators.GISnR GISnR(bool showOvernightHL, bool showOpenHL, bool showLastdayHL, bool showLastdayClose, bool showTodayOpen, int timeOpen, int timeClose)
		{
			return indicator.GISnR(Input, showOvernightHL, showOpenHL, showLastdayHL, showLastdayClose, showTodayOpen, timeOpen, timeClose);
		}

		public Indicators.GIVWAP GIVWAP()
		{
			return indicator.GIVWAP(Input);
		}


		
		public Indicators.GIEMA GIEMA(ISeries<double> input , int period, int offsetTicks)
		{
			return indicator.GIEMA(input, period, offsetTicks);
		}

		public Indicators.GIFibonacci GIFibonacci(ISeries<double> input , int param1)
		{
			return indicator.GIFibonacci(input, param1);
		}

		public Indicators.GIGetHighLowByTimeRange GIGetHighLowByTimeRange(ISeries<double> input , int startHour, int startMinute, int endHour, int endMinute)
		{
			return indicator.GIGetHighLowByTimeRange(input, startHour, startMinute, endHour, endMinute);
		}

		public Indicators.GIHLnBars GIHLnBars(ISeries<double> input , int period)
		{
			return indicator.GIHLnBars(input, period);
		}

		public Indicators.GIndicatorProxy GIndicatorProxy(ISeries<double> input , GStrategyBase gSZTrader)
		{
			return indicator.GIndicatorProxy(input, gSZTrader);
		}

		public Indicators.GISnR GISnR(ISeries<double> input , bool showOvernightHL, bool showOpenHL, bool showLastdayHL, bool showLastdayClose, bool showTodayOpen, int timeOpen, int timeClose)
		{
			return indicator.GISnR(input, showOvernightHL, showOpenHL, showLastdayHL, showLastdayClose, showTodayOpen, timeOpen, timeClose);
		}

		public Indicators.GIVWAP GIVWAP(ISeries<double> input )
		{
			return indicator.GIVWAP(input);
		}

	}
}

#endregion
