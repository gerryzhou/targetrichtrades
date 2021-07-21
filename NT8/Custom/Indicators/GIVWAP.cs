#region Using declarations
using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Data;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
    public class GIVWAP : GIndicatorBase
	{
		double	iCumVolume			= 0;
		double	iCumTypicalVolume	= 0;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"Volume Weighted Average Price";
				Name								= "GIVWAP";
				Calculate							= Calculate.OnBarClose;
				IsOverlay							= true;
				DisplayInDataBox					= true;
				DrawOnPricePanel					= true;
				DrawHorizontalGridLines				= true;
				DrawVerticalGridLines				= true;
				PaintPriceMarkers					= true;
				ScaleJustification					= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive			= true;
				AddPlot(Brushes.Black, "PlotVWAP");
			}
		}

		protected override void OnBarUpdate()
		{
			if (Bars.IsFirstBarOfSession)
			{
				iCumVolume = VOL()[0];
				iCumTypicalVolume = VOL()[0] * ((High[0] + Low[0] + Close[0]) / 3);
			}
			else
			{
				iCumVolume = iCumVolume + VOL()[0];
				iCumTypicalVolume = iCumTypicalVolume + (VOL()[0] * ((High[0] + Low[0] + Close[0]) / 3));
			}

			PlotVWAP[0] = (iCumTypicalVolume / iCumVolume);
			CheckVwapBreakoutDayHLEvent();
		}
		
		public double GetVwapOpenDOffset(SupportResistanceType srt, double price) {
			double offset = 0;
			double openD = CurrentDayOHL().CurrentOpen[0];
			switch(srt) {
				case SupportResistanceType.Support:
					offset = price - Math.Min(PlotVWAP[1], openD);
					break;
				case SupportResistanceType.Resistance:
					offset = Math.Max(PlotVWAP[1], openD) - price;
					break;
			}
			
			return offset;
		}

		public void CheckVwapBreakoutDayHLEvent() {
			IndicatorSignal isig = new IndicatorSignal();
			double openD = CurrentDayOHL().CurrentOpen[0];
			//if(CurrentBar < 300)
				Print(String.Format("{0}:PlotVWAP={1},OpenD={2}",
				CurrentBar, PlotVWAP[0], openD));
			if(PlotVWAP[0] < openD) {
				isig.BreakoutDir = BreakoutDirection.Down;
				isig.SignalName = SignalName_BreakoutOpenD;
			} else if(PlotVWAP[0] > openD) {
				isig.BreakoutDir = BreakoutDirection.Up;
				isig.SignalName = SignalName_BreakoutOpenD;
			} else
				return;
			
			isig.BarNo = CurrentBar;
			isig.IndicatorSignalType = SignalType.SimplePriceAction;
			IndicatorEventArgs ievt = new IndicatorEventArgs(this.GetType().Name, " CheckVwapBreakoutDayHLEvent: ");
			ievt.IndSignal = isig;
			//FireEvent(ievt);
			OnRaiseIndicatorEvent(ievt);
		}
		#region Properties

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PlotVWAP
		{
			get { return Values[0]; }
		}
		#endregion
		
		#region Pre-defined signal name
		[Browsable(false), XmlIgnore]
		public string SignalName_BreakoutOpenD
		{
			get { return "BreakoutOpenD";}
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIVWAP[] cacheGIVWAP;
		public GIVWAP GIVWAP()
		{
			return GIVWAP(Input);
		}

		public GIVWAP GIVWAP(ISeries<double> input)
		{
			if (cacheGIVWAP != null)
				for (int idx = 0; idx < cacheGIVWAP.Length; idx++)
					if (cacheGIVWAP[idx] != null &&  cacheGIVWAP[idx].EqualsInput(input))
						return cacheGIVWAP[idx];
			return CacheIndicator<GIVWAP>(new GIVWAP(), input, ref cacheGIVWAP);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIVWAP GIVWAP()
		{
			return indicator.GIVWAP(Input);
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
		public Indicators.GIVWAP GIVWAP()
		{
			return indicator.GIVWAP(Input);
		}

		public Indicators.GIVWAP GIVWAP(ISeries<double> input )
		{
			return indicator.GIVWAP(input);
		}
	}
}

#endregion
