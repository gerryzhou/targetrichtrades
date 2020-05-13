#region Using declarations
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies.ZTraderStg
{
    public class TrailingSLOrderBase : Order
	{
		public OrderType TLSLOrderType = OrderType.Market;
		public CalculationMode TLSLCalculationMode = CalculationMode.Price;

		public double maxProfitAmt = 0; //The max profit for current position
		
		public double highestPrice = 0; //The highest price of long position
		public double lowestPrice = 0; //The lowest price of short position
		
		private Order entryOrder = null;		
		private Order tlslOrder = null;
		
		#region Properites
		[Browsable(false), XmlIgnore]
		public double HighestPrice
		{
			get { return highestPrice; }
			set { highestPrice = value; }
		}
		
		[Browsable(false), XmlIgnore]
		public double LowestPrice
		{
			get { return lowestPrice; }
			set { lowestPrice = value; }
		}
		
		[Browsable(false), XmlIgnore]
		public double MaxProfitAmt
		{
			get { return maxProfitAmt; }
			set { maxProfitAmt = value; }
		}		
		
		[Browsable(false), XmlIgnore]
		public Order EntryOrder
		{
			get { return entryOrder; }
			set { entryOrder = value; }
		}
		
		[Browsable(false), XmlIgnore]
		public Order TLSLOrder
		{
			get { return tlslOrder; }
			set { tlslOrder = value; }
		}		
		#endregion
	}
}
