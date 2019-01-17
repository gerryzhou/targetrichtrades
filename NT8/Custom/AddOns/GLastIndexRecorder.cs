#region Using declarations
using System;
using System.Collections;
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
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.Indicators;
#endregion

//This namespace holds Add ons in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.AddOns
{
	public class GLastIndexRecord
	{
		private int BarNo;
		private LookbackBarType LBBarType;
		
		public GLastIndexRecord(int bar_no, LookbackBarType lbBarType){
			BarNumber = bar_no;
			BarType = lbBarType;
		}
		
		#region Properties
		
		[Range(0, int.MaxValue), NinjaScriptProperty]
        [Browsable(false)]
		[XmlIgnore()]		
		public int BarNumber
		{ get; set; }
		
		[Range(0, int.MaxValue), NinjaScriptProperty]
        [Browsable(false)]
		[XmlIgnore()]	
		public LookbackBarType BarType
		{ get; set; }
		
		#endregion
	}
	
	public class GLastIndexRecorder{
		private Indicator ind;
		private List<GLastIndexRecord> lastIndexRecords = new List<GLastIndexRecord>();
		
		public GLastIndexRecorder(Indicator indicator) {
			this.ind = indicator;
		}
		
		public GLastIndexRecord GetLastIndexRecord() {
			if(lastIndexRecords.Count > 0) {
				return lastIndexRecords[lastIndexRecords.Count-1];
			} else
				return null;
		}
		
		public void AddLastIndexRecord(GLastIndexRecord r) {
			lastIndexRecords.Add(r);
		}
		
		public void PrintRecords() {
			foreach(GLastIndexRecord item in lastIndexRecords) {
				ind.Print("GLastIndexRecord:" + item.BarNumber + "," + item.BarType);
			}
		}
	}
}
