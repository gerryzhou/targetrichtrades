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
	public class TrailingSLOrderBase : Order
	{
		private Order entryOrder;
		public OrderType TLSLOrderType = OrderType.Market;
		public CalculationMode TLSLCalculationMode = CalculationMode.Price;

		public double maxProfitAmt = 0; //The max profit for current position
		
		public double highestPrice = 0; //The highest price of long position
		public double lowestPrice = 0; //The lowest price of short position
		
		#region Properites
		[Browsable(false)]
		[XmlIgnore()]
		public Order EntryOrder
		{
			get { return entryOrder; }
			set { entryOrder = value; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public double HighestPrice
		{
			get { return highestPrice; }
			set { highestPrice = value; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public double LowestPrice
		{
			get { return lowestPrice; }
			set { lowestPrice = value; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public double MaxProfitAmt
		{
			get { return maxProfitAmt; }
			set { maxProfitAmt = value; }
		}		
		#endregion
	}
}
