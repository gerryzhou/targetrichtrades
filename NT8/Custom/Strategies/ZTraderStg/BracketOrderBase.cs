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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies.ZTraderStg
{
	public class BracketOrderBase : Order
	{
		private BracketOrderSubType bracketOrderSubType = BracketOrderSubType.UnKnown;
		private Order entryOrder;
		private OCOBase ocoOrder = new OCOBase();
		public OrderType EnOrderType = OrderType.Limit;
		public CalculationMode EnCalculationMode = CalculationMode.Price;
		
		public bool enTrailing = true; //use trailing entry: counter pullback bars or simple enOffsetPnts
		public bool ptTrailing = true; //use trailing profit target every bar
		public bool slTrailing = true; //use trailing stop loss every bar
		
		public double trailingPTTic = 36; //400, tick amount of trailing target
		public double trailingSLTic = 16; // 200, tick amount of trailing stop loss
		
		public double enPrice = 0; //The price of entry order
		public double enOffsetPnts = 1.25;//Price offset for entry
		
		#region Properites
		[Browsable(false)]
		[XmlIgnore()]
		public Order EntryOrder
		{
			get { return entryOrder; }
			set { entryOrder= value; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public OCOBase OCOOrder
		{
			get { return ocoOrder;	}
			set { ocoOrder= value; }
		}
		#endregion
	}
}
