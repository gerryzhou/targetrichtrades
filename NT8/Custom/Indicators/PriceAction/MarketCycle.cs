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
	/// <summary>
	/// Market condition 4 types:
	/// Up, Down, Neutral, and Reversal
	/// Neutral is the status hard to know up/down, it could resume the trend or reverse;
	/// Reversal is the status that is expected to change the trend soon, it's at the end of the trend life cycle;
	/// Up: tight or wide channel;
	/// Down: tight or wide channel;
	/// Neutral: tight or wide channel;
	/// Reversal: V or rounding;
	/// Three stages for each cycle: early, middle, and late
	/// </summary>
	
	public class MarketCycle
	{
	}
}






