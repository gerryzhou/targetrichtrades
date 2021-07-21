#region Using declarations
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
    public class OCOBase : Order
	{
		private Order stopLossOrder;
		private Order profitTargetOrder;
		
		#region Properites
		/// <summary>
		/// The OCO id for bracket or OCO trade
		/// Setup this field when SL and PT order
		/// are submitted, linked and working properly;
		/// otherwise, it means this order set 
		/// is not verified yet;
		/// </summary>		
		[Browsable(false), XmlIgnore]
		[DefaultValueAttribute(null)]
		public string OcoID
		{
			get; set;
		}
		
		[Browsable(false), XmlIgnore]
		public Order StopLossOrder
		{
			get { return stopLossOrder;	}
			set { stopLossOrder= value; }
		}
		
		[Browsable(false), XmlIgnore]
		public Order ProfitTargetOrder
		{
			get { return profitTargetOrder;	}
			set { profitTargetOrder= value; }
		}		
		#endregion
	}
}
