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
namespace NinjaTrader.NinjaScript.Indicators.ZTraderInd
{
	public class SupportResistance
	{
		public int Count;
		private List<KeyValuePair<int, double>> sptRstBars;
		private double sptRstValue;
		
		
		private SupportResistanceType sptRestType;
		
		public SupportResistance() {
		}
		
		public void AddSupport(int barNo, double price) {
		}
		
		public void AddResistance(int barNo, double price) {
		}
		
		public SupportResistance GetSupport() {
			KeyValuePair<int, double> kv;
			return null;
		}
		
		public SupportResistance GetResistance() {
			return null;
		}
		
		public double GetSptRstValue() {
			return sptRstValue;
		}
		
		public void SetSptRstValue(double val) {
			sptRstValue = val;
		}
		
		#region Properties
		public SupportResistanceType GetSupportResistanceType() {
			return sptRestType;
		}
		public SupportResistanceType SetSupportResistanceType(SupportResistanceType value) {
			return sptRestType = value;
		}		
		#endregion

	}
}









