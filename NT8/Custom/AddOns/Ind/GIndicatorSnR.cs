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
using NinjaTrader.NinjaScript.AddOns.PriceActions;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class GIndicatorBase : Indicator
	{
		public int Count;
//		private List<KeyValuePair<int, double>> sptRstBars;
//		private double sptRstValue;		
		
//		private SupportResistanceType sptRestType;
		
		private List<SupportResistanceBar> SnRBars;
		
		private List<SupportResistanceLine> SnRLines;
		
		private List<SupportResistanceRange<SupportResistanceLine>> SnRRanges;

		#region Methods
		
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

		#endregion
	}
}




































