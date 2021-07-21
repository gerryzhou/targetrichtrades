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
	public class DailyMaxMin
	{
		public DailyMaxMin() {
			DailyMax = double.MinValue;
			DailyMin = double.MaxValue;
		}
		
		[Browsable(false), XmlIgnore]
		public double DailyMax {
			get; set;
		}

		[Browsable(false), XmlIgnore]
		public double DailyMin {
			get; set;
		}

		[Browsable(false), XmlIgnore]
		public DateTime DailyMaxTime {
			get; set;
		}

		[Browsable(false), XmlIgnore]
		public DateTime DailyMinTime {
			get; set;
		}
	}
}
