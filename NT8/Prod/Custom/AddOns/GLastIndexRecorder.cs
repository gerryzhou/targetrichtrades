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
	public class GLastIndexRecord<T>
	{
		private int BarNo;
		private int BarNo2;
		private LookbackBarType LBBarType;
		private T LB_Value; //Lookback value
		
		public GLastIndexRecord(int bar_no, LookbackBarType lbBarType){
			BarNumber = bar_no;
			BarType = lbBarType;
		}

		public GLastIndexRecord(int bar_no, int bar_no2, LookbackBarType lbBarType, T lbValue){
			BarNumber = bar_no;
			BarNumber2 = bar_no2;
			BarType = lbBarType;
			LBValue = lbValue;
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
		public int BarNumber2
		{ get; set; }
		
		[NinjaScriptProperty]
        [Browsable(false)]
		[XmlIgnore()]	
		public LookbackBarType BarType
		{ get; set; }
		
		[NinjaScriptProperty]
        [Browsable(false)]
		[XmlIgnore()]	
		public T LBValue
		{ get; set; }		
		#endregion
	}
	
	public class GLastIndexRecorder<T>{
		private Indicator ind;
		private List<GLastIndexRecord<T>> lastIndexRecords = new List<GLastIndexRecord<T>>();
		
		public GLastIndexRecorder(Indicator indicator) {
			this.ind = indicator;
		}
		
		public GLastIndexRecord<T> GetLastIndexRecord() {
			if(lastIndexRecords.Count > 0) {
				return lastIndexRecords[lastIndexRecords.Count-1];
			} else
				return null;
		}

		public GLastIndexRecord<T> GetLastIndexRecord(int barNo, LookbackBarType lbBarType) {		
			if(lastIndexRecords.Count > 0) {
				GLastIndexRecord<T> r = null;
				for(int i=lastIndexRecords.Count-1; i>=0; i--) {
					r = lastIndexRecords[i];
					if(r.BarNumber <= barNo && (lbBarType== LookbackBarType.Unknown || r.BarType == lbBarType))
						return r;
				}
			}
			return null;
		}
	
		public int GetLastIndex(int barNo, LookbackBarType lbBarType) {
			GLastIndexRecord<T> r = GetLastIndexRecord(barNo, lbBarType);
			if(r == null) return -1;
			else return r.BarNumber;
		}
		
		public void AddLastIndexRecord(GLastIndexRecord<T> r) {
			lastIndexRecords.Add(r);
		}
		
		public void PrintRecords() {
			foreach(GLastIndexRecord<T> item in lastIndexRecords) {
				ind.Print("GLastIndexRecord:" + item.BarNumber + "," + item.BarType);
			}
		}
	}
}
