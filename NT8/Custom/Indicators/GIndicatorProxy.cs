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
namespace NinjaTrader.NinjaScript.Indicators
{
	public class GIndicatorProxy : Indicator
	{
		private Series<double> CustomDataSeries1;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Combine more indicators, provides interface for strategy to call the methods and obtain the signals for trade decision;";
				Name										= "IndicatorProxy";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				CustomColor1					= Brushes.Orange;
				CustomPrc1					= 100;
				CustomStr1					= @"str-1";
				CustomTime1						= DateTime.Parse("08:25", System.Globalization.CultureInfo.InvariantCulture);
			}
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Minute, 1);
			}
			else if (State == State.DataLoaded)
			{				
				CustomDataSeries1 = new Series<double>(this);
			}
		}

		protected override void OnConnectionStatusUpdate(ConnectionStatusEventArgs connectionStatusUpdate)
		{
			
		}

		protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
		{
			
		}

		protected override void OnMarketDepth(MarketDepthEventArgs marketDepthUpdate)
		{
			
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
		}

		#region Properties
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="CustomColor1", Description="Color-1", Order=1, GroupName="Parameters")]
		public Brush CustomColor1
		{ get; set; }

		[Browsable(false)]
		public string CustomColor1Serializable
		{
			get { return Serialize.BrushToString(CustomColor1); }
			set { CustomColor1 = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="CustomPrc1", Description="CustomPrc-1", Order=2, GroupName="Parameters")]
		public double CustomPrc1
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="CustomStr1", Description="CustomStr-1", Order=3, GroupName="Parameters")]
		public string CustomStr1
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="CustomTime1", Description="CustomTime-1", Order=4, GroupName="Parameters")]
		public DateTime CustomTime1
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIndicatorProxy[] cacheGIndicatorProxy;
		public GIndicatorProxy GIndicatorProxy(Brush customColor1, double customPrc1, string customStr1, DateTime customTime1)
		{
			return GIndicatorProxy(Input, customColor1, customPrc1, customStr1, customTime1);
		}

		public GIndicatorProxy GIndicatorProxy(ISeries<double> input, Brush customColor1, double customPrc1, string customStr1, DateTime customTime1)
		{
			if (cacheGIndicatorProxy != null)
				for (int idx = 0; idx < cacheGIndicatorProxy.Length; idx++)
					if (cacheGIndicatorProxy[idx] != null && cacheGIndicatorProxy[idx].CustomColor1 == customColor1 && cacheGIndicatorProxy[idx].CustomPrc1 == customPrc1 && cacheGIndicatorProxy[idx].CustomStr1 == customStr1 && cacheGIndicatorProxy[idx].CustomTime1 == customTime1 && cacheGIndicatorProxy[idx].EqualsInput(input))
						return cacheGIndicatorProxy[idx];
			return CacheIndicator<GIndicatorProxy>(new GIndicatorProxy(){ CustomColor1 = customColor1, CustomPrc1 = customPrc1, CustomStr1 = customStr1, CustomTime1 = customTime1 }, input, ref cacheGIndicatorProxy);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIndicatorProxy GIndicatorProxy(Brush customColor1, double customPrc1, string customStr1, DateTime customTime1)
		{
			return indicator.GIndicatorProxy(Input, customColor1, customPrc1, customStr1, customTime1);
		}

		public Indicators.GIndicatorProxy GIndicatorProxy(ISeries<double> input , Brush customColor1, double customPrc1, string customStr1, DateTime customTime1)
		{
			return indicator.GIndicatorProxy(input, customColor1, customPrc1, customStr1, customTime1);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIndicatorProxy GIndicatorProxy(Brush customColor1, double customPrc1, string customStr1, DateTime customTime1)
		{
			return indicator.GIndicatorProxy(Input, customColor1, customPrc1, customStr1, customTime1);
		}

		public Indicators.GIndicatorProxy GIndicatorProxy(ISeries<double> input , Brush customColor1, double customPrc1, string customStr1, DateTime customTime1)
		{
			return indicator.GIndicatorProxy(input, customColor1, customPrc1, customStr1, customTime1);
		}
	}
}

#endregion
