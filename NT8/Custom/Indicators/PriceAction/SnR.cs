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
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.PriceActions
{
	public class SnR
	{
		/// <summary>
		/// SnR in a time period:
		/// Support;
		/// Resistance;
		///
		/// Two types: barSnR, indicatorSnR
		/// </summary>
		private double hi, low;		
	}
	
	public class SupportResistanceBar
	{
		public void NewSupportResistanceBar(int barNo, SupportResistanceType snrType, PriceSubtype snrPriceType) {
			BarNo = barNo;
			SnRType = snrType;
			SnRPriceType = snrPriceType;
		}
		
		[Range(0, int.MaxValue), NinjaScriptProperty]
        [Browsable(false)]
		[XmlIgnore()]
		public int BarNo
		{ get; set; }		

		[NinjaScriptProperty]
        [Browsable(false)]
		[XmlIgnore()]
		public SupportResistanceType SnRType
		{ get; set; }
		
		[NinjaScriptProperty]
        [Browsable(false)]
		[XmlIgnore()]
		public PriceSubtype SnRPriceType
		{ get; set; }
	}

	public class SupportResistanceLine
	{
		public void NewSupportResistanceLine(int barNoStart, int barNoEnd, SupportResistanceType snrType, double snrPrice) {
			BarNoStart = barNoStart;
			BarNoEnd = barNoEnd;
			SnRType = snrType;
			SnRPrice = snrPrice;
		}
		
		[Range(0, int.MaxValue), NinjaScriptProperty]
        [Browsable(false)]
		[XmlIgnore()]
		public int BarNoStart
		{ get; set; }
		
		[Range(0, int.MaxValue), NinjaScriptProperty]
        [Browsable(false)]
		[XmlIgnore()]
		public int BarNoEnd
		{ get; set; }
		
		[NinjaScriptProperty]
        [Browsable(false)]
		[XmlIgnore()]
		public SupportResistanceType SnRType
		{ get; set; }
		
		[Range(0, double.MaxValue), NinjaScriptProperty]
        [Browsable(false)]
		[XmlIgnore()]
		public double SnRPrice
		{ get; set; }
	}
	
	/// <summary>
	/// Define the range with the support and resistance
	/// SnR could be two bars, or two lines, which form a range
	/// </summary>
	/// <typeparam name="T">SupportResistanceBar, SupportResistanceLine or double</typeparam>
	public class SupportResistanceRange<T> {
		[NinjaScriptProperty]
        [Browsable(false)]
		[XmlIgnore()]
		public T Support
		{ get; set; }

		[NinjaScriptProperty]
        [Browsable(false)]
		[XmlIgnore()]
		public T Resistance
		{ get; set; }
	}	
}




