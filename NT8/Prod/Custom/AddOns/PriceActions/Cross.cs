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
using NinjaTrader.Gui.Tools;
#endregion

//This namespace holds Add ons in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.AddOns.PriceActions
{
	public class Cross<T>
	{
		public Cross(T sym1, T sym2) {
			Sym1 = sym1;
			Sym2 = sym2;
			CrossType = 0;
		}
		
		[Browsable(false), XmlIgnore]
		public DateTime CrossTime {
			get; set;
		}
		
		[Browsable(false), XmlIgnore]
		public T Sym1 {
			get; set;
		}

		[Browsable(false), XmlIgnore]
		public T Sym2 {
			get; set;
		}
		
		/// <summary>
		/// CrossType: 1=cross over mid, 2=cross over high;
		/// -1=corss below mid, -2=cross below low, 0=UnKnown
		/// </summary>
		[Browsable(false), XmlIgnore]
		public int CrossType {
			get; set;
		}
	}
	
	public class DailyCross<T> : NinjaTrader.NinjaScript.AddOnBase {
		
		Dictionary<string, List<Cross<T>>> DailyCrosses;
		
		public DailyCross() {
			DailyCrosses = new Dictionary<string, List<Cross<T>>>();
		}
		
		public void AddCross(Cross<T> cross) {
			string date = string.Format("{0:yyyyMMdd}", cross.CrossTime);
			List<Cross<T>> crosslist = GetCrossesByDate(date);
			if(crosslist == null) {
				crosslist = new List<Cross<T>>();
				crosslist.Add(cross);
				DailyCrosses.Add(date, crosslist);
			}
			else
				DailyCrosses[date] = crosslist;
		}
		
		public List<Cross<T>> GetCrossesByDate(string date) {
			List<Cross<T>> crosslist;
			DailyCrosses.TryGetValue(date, out crosslist);
			return crosslist;
		}
		
		public List<Cross<T>> GetCrossesByDate(DateTime date) {
			string str_date = string.Format("{0:yyyyMMdd}", date);
			return GetCrossesByDate(str_date);
		}
		
		public void PrintCrosses() {
			Print("========PrintCrosses========");
			foreach(KeyValuePair<string, List<Cross<T>>> item in DailyCrosses) {
				string key = item.Key;
				List<Cross<T>> lst = item.Value;
				foreach(Cross<T> crs in lst) {
					Print(string.Format("{0}\t{1}\t{2}\t{3}\t{4:yyyyMMdd}\t{4:HHmm}", 
						key, crs.Sym1, crs.Sym2, crs.CrossType, crs.CrossTime));
				}
			}
		}
	}
}
