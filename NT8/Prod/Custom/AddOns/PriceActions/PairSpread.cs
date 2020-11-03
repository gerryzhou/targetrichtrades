#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
#endregion

//This namespace holds Add ons in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.AddOns.PriceActions
{
	public class PairSpread<T>
	{
		[Browsable(false), XmlIgnore]
		public SpreadType SpdType {
			get; set;
		}

		[Browsable(false), XmlIgnore]
		public double SpreadValue {
			get; set;
		}

		[Browsable(false), XmlIgnore]
		public T Symbol1 {
			get; set;
		}

		[Browsable(false), XmlIgnore]
		public T Symbol2 {
			get; set;
		}
	}
}
