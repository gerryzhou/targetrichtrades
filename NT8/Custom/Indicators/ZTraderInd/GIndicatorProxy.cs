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
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.ZTraderInd
{
	public class GIndicatorProxy : GIndicatorBase
	{
		private Series<double> CustData;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Common indicator to transfer indicator info to strategy;";
				Name										= "GIndicatorProxy";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				MaximumBarsLookBack							= MaximumBarsLookBack.Infinite;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				CustInput					= 1;
				AddPlot(Brushes.Orange, "CustPlot");
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				CustData = new Series<double>(this);
			}
		}

		protected override void OnConnectionStatusUpdate(ConnectionStatusEventArgs connectionStatusUpdate)
		{
			
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="CustInput", Description="CustInput for indicator proxy", Order=1, GroupName="Parameters")]
		public int CustInput
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CustPlot
		{
			get { return Values[0]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ZTraderInd.GIndicatorProxy[] cacheGIndicatorProxy;
		public ZTraderInd.GIndicatorProxy GIndicatorProxy(int custInput)
		{
			return GIndicatorProxy(Input, custInput);
		}

		public ZTraderInd.GIndicatorProxy GIndicatorProxy(ISeries<double> input, int custInput)
		{
			if (cacheGIndicatorProxy != null)
				for (int idx = 0; idx < cacheGIndicatorProxy.Length; idx++)
					if (cacheGIndicatorProxy[idx] != null && cacheGIndicatorProxy[idx].CustInput == custInput && cacheGIndicatorProxy[idx].EqualsInput(input))
						return cacheGIndicatorProxy[idx];
			return CacheIndicator<ZTraderInd.GIndicatorProxy>(new ZTraderInd.GIndicatorProxy(){ CustInput = custInput }, input, ref cacheGIndicatorProxy);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ZTraderInd.GIndicatorProxy GIndicatorProxy(int custInput)
		{
			return indicator.GIndicatorProxy(Input, custInput);
		}

		public Indicators.ZTraderInd.GIndicatorProxy GIndicatorProxy(ISeries<double> input , int custInput)
		{
			return indicator.GIndicatorProxy(input, custInput);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ZTraderInd.GIndicatorProxy GIndicatorProxy(int custInput)
		{
			return indicator.GIndicatorProxy(Input, custInput);
		}

		public Indicators.ZTraderInd.GIndicatorProxy GIndicatorProxy(ISeries<double> input , int custInput)
		{
			return indicator.GIndicatorProxy(input, custInput);
		}
	}
}

#endregion
