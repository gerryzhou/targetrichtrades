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
	public class IndicatorSignal
	{
		public TrendDirection TrendDir = TrendDirection.UnKnown; //1=up, -1=down, 0=flat/unknown
		public Breakout BreakoutDir = Breakout.UnKnown; //1=bk up, -1=bk down, 0=no bk/unknown
		public Reversal ReversalDir = Reversal.UnKnown; //1=rev up, -1=rev down, 0=no rev/unknown			
	}
}




