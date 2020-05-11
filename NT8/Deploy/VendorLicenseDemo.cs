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
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class LicensedIndicatorDemo : Indicator
	{
		// Place your vendor licensing in the default constructor. This guarantees, that your licensing
		// could not be overwritten later by fraudulent hackers who e.g. would derive from your class.
		// Note, that your code only ever should call 'VendorLicense' once.
		public LicensedIndicatorDemo()
		{
			//When no custom configuration is needed, the arguments below will suffice:
			//VendorLicense("NT", "Module", "www.your-url.com", "support@vendor.com",null);
			
			VendorLicense("YourVendorName", "YourProductName", "www.your-url.com", "yourAddress@your-url.com.com",
				// This optional callback is triggered right before the actual license verification and allows
				// you to delay the configuration.
				// It's defaulted to NULL if not provided. License verification then is triggered as configured above
				() =>
				{
					// The following demonstrates how to set up additional custom configuration for license verification.
					// For example, if you planned to offer the indicator for free for use with indexes only, you could skip
					// the verification process like below:
					if (Instrument.MasterInstrument.InstrumentType == InstrumentType.Index)
						return false;

                // For all other instruments the already configured license verification is triggered.
				return true;
				});
		}
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"This is a demo for how to properly apply the vendor licensing";
				Name						= "LicensedIndicatorDemo";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= false;
				DisplayInDataBox			= true;
				DrawOnPricePanel			= true;
				DrawHorizontalGridLines		= true;
				DrawVerticalGridLines		= true;
				PaintPriceMarkers			= true;
				ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive	= true;
				AddPlot(Brushes.Orange, "Plot1");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
            Values[0][0] = (High[0] - Low[0]) + High[0];
		}

		#region Properties

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Plot1
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
		private LicensedIndicatorDemo[] cacheLicensedIndicatorDemo;
		public LicensedIndicatorDemo LicensedIndicatorDemo()
		{
			return LicensedIndicatorDemo(Input);
		}

		public LicensedIndicatorDemo LicensedIndicatorDemo(ISeries<double> input)
		{
			if (cacheLicensedIndicatorDemo != null)
				for (int idx = 0; idx < cacheLicensedIndicatorDemo.Length; idx++)
					if (cacheLicensedIndicatorDemo[idx] != null &&  cacheLicensedIndicatorDemo[idx].EqualsInput(input))
						return cacheLicensedIndicatorDemo[idx];
			return CacheIndicator<LicensedIndicatorDemo>(new LicensedIndicatorDemo(), input, ref cacheLicensedIndicatorDemo);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LicensedIndicatorDemo LicensedIndicatorDemo()
		{
			return indicator.LicensedIndicatorDemo(Input);
		}

		public Indicators.LicensedIndicatorDemo LicensedIndicatorDemo(ISeries<double> input )
		{
			return indicator.LicensedIndicatorDemo(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LicensedIndicatorDemo LicensedIndicatorDemo()
		{
			return indicator.LicensedIndicatorDemo(Input);
		}

		public Indicators.LicensedIndicatorDemo LicensedIndicatorDemo(ISeries<double> input )
		{
			return indicator.LicensedIndicatorDemo(input);
		}
	}
}

#endregion
