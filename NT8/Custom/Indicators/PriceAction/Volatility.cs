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
namespace NinjaTrader.NinjaScript.Indicators.PriceActions
{
	public class Volatility
	{
		/// <summary>
		/// Volume in a time period: 
		/// Volume burst at the open;
		/// </summary>
		private double hi;
		private double lo;
		private int minUp;
		private int maxUp;
		private int minDn;
		private int maxDn;
		
		public Volatility(int minUpTicks, int maxUpTicks, int minDnTicks, int maxDnTicks) {
			this.minUp = minUpTicks;
			this.maxUp = maxUpTicks;
			this.minDn = minDnTicks;
			this.maxDn = maxDnTicks;
		}
	}
}







