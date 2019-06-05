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
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.AddOns;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class GIKeyReversal : GIndicatorBase
	{
		private MAX maxHi;
		private MIN minLo;
		private MAX maxLo;
		private MIN minHi;				
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Key reversal up and down;";
				Name										= "GIKeyReversal";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				PeriodLeft						= 15;
				PeriodRight						= 5;
				AddPlot(new Stroke(Brushes.DarkGreen, 2), PlotStyle.Dot, "KeyUp");
				AddPlot(new Stroke(Brushes.Crimson, 2), PlotStyle.Dot, "KeyDn");
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded) {
				maxHi = MAX(High, PeriodLeft);
				minLo = MIN(Low, PeriodLeft);
				maxLo = MAX(Low, PeriodRight);
				minHi = MIN(High, PeriodRight);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < PeriodLeft + 1)
				return;
//			if(Low[0] < min[1] && Close[0] > Close[1])
//				KeyUp[0] =  Low[0] - 4*GetTick4Symbol();
//			if(High[0] > max[1] && Close[0] < Close[1])
//				KeyDn[0] = High[0] + 4*GetTick4Symbol();
			if(Low[PeriodRight] == minLo[PeriodRight] && Low[0] == maxLo[0]){
				if(KeyUp[PeriodRight+1] > 0) {
					Print("KeyUp[PeriodRight+1]=" + KeyUp[PeriodRight+1]);
					KeyUp[PeriodRight+1] = 0;
				}
				KeyUp[PeriodRight] = Low[PeriodRight] - 4*TickSize;
				Print("KeyUp[PeriodRight]=" + KeyUp[PeriodRight]);
			}
			if(High[PeriodRight] == maxHi[PeriodRight] && High[0] == minHi[0]) {
				if(KeyDn[PeriodRight+1] > 0) {
					Print("KeyDn[PeriodRight+1]=" + KeyDn[PeriodRight+1]);
					KeyDn[PeriodRight+1] = 0;
				}
				KeyDn[PeriodRight] = High[PeriodRight] + 4*TickSize;
			}
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PeriodLeft", GroupName = "NinjaScriptParameters", Order = 0)]
		public int PeriodLeft
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PeriodRight", GroupName = "NinjaScriptParameters", Order = 1)]
		
		public int PeriodRight
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> KeyUp
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> KeyDn
		{
			get { return Values[1]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIKeyReversal[] cacheGIKeyReversal;
		public GIKeyReversal GIKeyReversal(int periodLeft, int periodRight)
		{
			return GIKeyReversal(Input, periodLeft, periodRight);
		}

		public GIKeyReversal GIKeyReversal(ISeries<double> input, int periodLeft, int periodRight)
		{
			if (cacheGIKeyReversal != null)
				for (int idx = 0; idx < cacheGIKeyReversal.Length; idx++)
					if (cacheGIKeyReversal[idx] != null && cacheGIKeyReversal[idx].PeriodLeft == periodLeft && cacheGIKeyReversal[idx].PeriodRight == periodRight && cacheGIKeyReversal[idx].EqualsInput(input))
						return cacheGIKeyReversal[idx];
			return CacheIndicator<GIKeyReversal>(new GIKeyReversal(){ PeriodLeft = periodLeft, PeriodRight = periodRight }, input, ref cacheGIKeyReversal);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIKeyReversal GIKeyReversal(int periodLeft, int periodRight)
		{
			return indicator.GIKeyReversal(Input, periodLeft, periodRight);
		}

		public Indicators.GIKeyReversal GIKeyReversal(ISeries<double> input , int periodLeft, int periodRight)
		{
			return indicator.GIKeyReversal(input, periodLeft, periodRight);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIKeyReversal GIKeyReversal(int periodLeft, int periodRight)
		{
			return indicator.GIKeyReversal(Input, periodLeft, periodRight);
		}

		public Indicators.GIKeyReversal GIKeyReversal(ISeries<double> input , int periodLeft, int periodRight)
		{
			return indicator.GIKeyReversal(input, periodLeft, periodRight);
		}
	}
}

#endregion
