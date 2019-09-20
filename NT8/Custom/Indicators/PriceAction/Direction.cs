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
	public class Direction
	{
		private TrendDirection trendDir = TrendDirection.UnKnown;
		
		#region Properties		
		/// <summary>
		/// </summary>
		[Browsable(false)]
		[XmlIgnore()]
		[DefaultValueAttribute(TrendDirection.UnKnown)]
		public TrendDirection TrendDir
		{
			get { return trendDir; }
			set { trendDir = value; }
		}		
		#endregion
	}
}


