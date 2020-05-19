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
	public class GIMktCtx : GIndicatorBase
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Indicator for supervised Market Context;";
				Name										= "GIMktCtx";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= false;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= false;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{
			}
			else if (State == State.Historical)
			{
				if (Calculate == Calculate.OnPriceChange)
				{
					Log(string.Format(Custom.Resource.NinjaScriptOnPriceChangeError, Name), LogLevel.Error);
				}
			}
		}

		protected override void OnBarUpdate()
		{
		}

		#region Properties
		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "CtxFilePath", GroupName = GPI_CUSTOM_PARAMS, Order = OD_CtxFilePath)]
		public int CtxFilePath
		{ 
			get; set;
		}
		
		private const int OD_CtxFilePath = 1;
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIMktCtx[] cacheGIMktCtx;
		public GIMktCtx GIMktCtx(int ctxFilePath)
		{
			return GIMktCtx(Input, ctxFilePath);
		}

		public GIMktCtx GIMktCtx(ISeries<double> input, int ctxFilePath)
		{
			if (cacheGIMktCtx != null)
				for (int idx = 0; idx < cacheGIMktCtx.Length; idx++)
					if (cacheGIMktCtx[idx] != null && cacheGIMktCtx[idx].CtxFilePath == ctxFilePath && cacheGIMktCtx[idx].EqualsInput(input))
						return cacheGIMktCtx[idx];
			return CacheIndicator<GIMktCtx>(new GIMktCtx(){ CtxFilePath = ctxFilePath }, input, ref cacheGIMktCtx);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIMktCtx GIMktCtx(int ctxFilePath)
		{
			return indicator.GIMktCtx(Input, ctxFilePath);
		}

		public Indicators.GIMktCtx GIMktCtx(ISeries<double> input , int ctxFilePath)
		{
			return indicator.GIMktCtx(input, ctxFilePath);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIMktCtx GIMktCtx(int ctxFilePath)
		{
			return indicator.GIMktCtx(Input, ctxFilePath);
		}

		public Indicators.GIMktCtx GIMktCtx(ISeries<double> input , int ctxFilePath)
		{
			return indicator.GIMktCtx(input, ctxFilePath);
		}
	}
}

#endregion
