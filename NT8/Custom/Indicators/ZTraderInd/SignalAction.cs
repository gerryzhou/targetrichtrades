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
using NinjaTrader.NinjaScript.Indicators.PriceActions;

#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.ZTraderInd
{
	/// <summary>
	/// This object carry the signal action that represents simple price action includes:
	/// breakout, pullback, reversal, crossOver/Under, inflection, etc. 
	/// * SnR
	/// </summary>
	public class SignalAction
	{	
		#region Protperies
		/// <summary>
		/// The type of the signal
		/// </summary>		
		[Browsable(false)]
		[XmlIgnore]
		public SignalActionType SignalActionType
		{
			get; set;
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public SupportResistanceRange<double> SnR {
			get; set;
		}
		#endregion
	}
}

