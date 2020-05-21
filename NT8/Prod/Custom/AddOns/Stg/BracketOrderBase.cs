#region Using declarations
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
    public class BracketOrderBase : Order
	{
		private BracketOrderSubType bracketOrderSubType = BracketOrderSubType.UnKnown;
		
		private Order entryOrder;
		public OrderType EnOrderType = OrderType.Limit;
		public CalculationMode EnCalculationMode = CalculationMode.Price;
		
		private EntryExitOrderType exitOrderType = EntryExitOrderType.SimpleOCO;
		private OCOBase ocoOrder = new OCOBase();
		
		private TrailingSLOrderBase trailingSLOrder = new TrailingSLOrderBase();
		
		public bool enTrailing = true; //use trailing entry: counter pullback bars or simple enOffsetPnts
		public bool ptTrailing = true; //use trailing profit target every bar
		public bool slTrailing = true; //use trailing stop loss every bar
		
		public double trailingPTTic = 36; //400, tick amount of trailing target
		public double trailingSLTic = 16; // 200, tick amount of trailing stop loss
		
		public double enPrice = 0; //The price of entry order
		public double enOffsetPnts = 1.25;//Price offset for entry
		
		#region Properites
		[Browsable(false), XmlIgnore]
		public Order EntryOrder
		{
			get { return entryOrder; }
			set { entryOrder = value; }
		}
		
		[Browsable(false), XmlIgnore]
		public OCOBase OCOOrder
		{
			get { return ocoOrder;	}
			set { ocoOrder = value; }
		}

		[Browsable(false), XmlIgnore]
		public TrailingSLOrderBase TrailingSLOrder
		{
			get { return trailingSLOrder; }
			set { trailingSLOrder = value; }
		}
		
		[Browsable(false), XmlIgnore]
		public EntryExitOrderType ExitOrderType
		{
			get { return exitOrderType;	}
			set { exitOrderType = value; }
		}
		#endregion
	}
}
