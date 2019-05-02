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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	/// <summary>
	/// @"Manage position size, scale in/out, etc.";
	/// </summary>
	public partial class GSZTraderBase : Strategy
	{
		public int hasPosition() {
			indicatorProxy.TraceMessage(this.Name);
			int pos = 0;
			if(Position != null)
				pos = Position.Quantity;
			return pos;
		}
		
		public double GetTickValue() {
			MasterInstrument maIns = Bars.Instrument.MasterInstrument;
			//Print("TickSize, name, pointvalue=" + maIns.TickSize + "," + maIns.Name + "," + maIns.PointValue);
			return maIns.TickSize*maIns.PointValue;
		}
	}
}
