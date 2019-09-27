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
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Indicators.PriceActions;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Class to hold volume
	/// 
	/// </summary>
	public partial class GIndicatorBase : Indicator
	{
		/// <summary>
		/// Hold the indicator volume from the underlining indicator
		/// 
		/// </summary>

		
		#region Methods
		
		public virtual long CheckVolume(int barIndex) {
			long volumeValue = Bars.GetVolume(barIndex);
    		Print("Bar #" + barIndex + " volume value is " + volumeValue);
			return volumeValue;
		}
		
		public virtual double GetVolWPR(int period) {
			double r = -1;
			double max = MAX(Volume, period)[0];
			r = 100*Volume[0]/max;
			return r;
		}
		
		#endregion
	}
}
