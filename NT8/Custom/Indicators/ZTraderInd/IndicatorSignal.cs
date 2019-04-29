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
	/// <summary>
	/// This object carry the signal that can trigger a trade (entry or exit)
	/// </summary>
	public class IndicatorSignal
	{	
		private Direction trendDir = new Direction();//TrendDirection.UnKnown; //1=up, -1=down, 0=flat/unknown
		private Breakout breakoutDir = Breakout.UnKnown; //1=bk up, -1=bk down, 0=no bk/unknown
		private Reversal reversalDir = Reversal.UnKnown; //1=rev up, -1=rev down, 0=no rev/unknown
		private SupportResistanceRange<SupportResistanceBar> sptRst;
		
		#region Protperies
		[Range(0, int.MaxValue)]
		[Browsable(false)]
		[XmlIgnore]
		public int BarNo
		{
			get; set;
		}
		
		[Browsable(false)]
		[XmlIgnore]
		//[DefaultValueAttribute(TrendDirection.UnKnown)]
		public Direction TrendDir {
			get { return trendDir;}
			set { trendDir = value;}
		}

		[Browsable(false)]
		[XmlIgnore]
		[DefaultValueAttribute(Breakout.UnKnown)]
		public Breakout BreakoutDir {
			get { return breakoutDir;}
			set { breakoutDir = value;}
		}
		
		[Browsable(false)]
		[XmlIgnore]
		[DefaultValueAttribute(Reversal.UnKnown)]
		public Reversal ReversalDir {
			get { return reversalDir;}
			set { reversalDir = value;}
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public SupportResistanceRange<SupportResistanceBar> SnR {
			get { return sptRst;}
			set { sptRst = value;}
		}		
		#endregion
	}
}

























