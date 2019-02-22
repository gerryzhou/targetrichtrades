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
namespace NinjaTrader.NinjaScript.Indicators
{
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
	/// <typeparam name="T">SupportResistanceBar or SupportResistanceLine</typeparam>
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
	
	public partial class GIndicatorBase : Indicator
	{
		public int Count;
//		private List<KeyValuePair<int, double>> sptRstBars;
//		private double sptRstValue;		
		
//		private SupportResistanceType sptRestType;
		
		private List<SupportResistanceBar> SnRBars;
		
		private List<SupportResistanceLine> SnRLines;
		
		private List<SupportResistanceRange<SupportResistanceLine>> SnRRanges;
		
		public void AddSupport(int barNo, double price) {
			SupportResistanceBar snrBar = new SupportResistanceBar();
		}
		
		public void AddResistance(int barNo, double price) {
		}
		
		public SupportResistanceBar GetSupport() {
			KeyValuePair<int, double> kv;
			return null;
		}
		
		public SupportResistanceBar GetResistance() {
			return null;
		}
		
		public double GetSptRstValue() {
			return 0;//sptRstValue;
		}
		
		public void SetSptRstValue(double val) {
			//sptRstValue = val;
		}
		
		public double GetSupport(SupportResistanceBar snrBar) {
			double prc = 0;
			if(snrBar.SnRType == SupportResistanceType.Support) {
				prc = GetPriceByType(snrBar.BarNo, snrBar.SnRPriceType);
			}
			return prc;
		}
		
		public double GetResistance(SupportResistanceBar snrBar) {
			double prc = 0;
			Print(CurrentBar + ":GetResistance=" + snrBar.BarNo + "," + snrBar.SnRPriceType + "," + snrBar.SnRType);
			if(snrBar.SnRType == SupportResistanceType.Resistance) {
				prc = GetPriceByType(snrBar.BarNo, snrBar.SnRPriceType);
			}
			return prc;
		}
		
		public SupportResistanceRange<SupportResistanceLine> GetSnRRangeByTime(int startH, int startM, int endH, int endM) {
			return null;
		}
		
		public SupportResistanceRange<SupportResistanceLine> NewSupportResistanceRange(SupportResistanceLine rstLine, SupportResistanceLine sptLine) {
			SupportResistanceRange<SupportResistanceLine> snrRange = new SupportResistanceRange<SupportResistanceLine>();
			snrRange.Resistance = rstLine;
			snrRange.Support = sptLine;
			return snrRange;
		}
		
		#region Properties
//		public SupportResistanceType GetSupportResistanceType() {
//			return null;//sptRestType;
//		}
//		public SupportResistanceType SetSupportResistanceType(SupportResistanceType value) {
//			return null;//sptRestType = value;
//		}		
		#endregion

	}
}




































