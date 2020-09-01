#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Indicators;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
    /// <summary>
    /// This object carry the signal that can trigger a trade (file an order)
    /// It provides the essentials for SubmitOrderUnmanaged
    /// (OrderAction orderAction, OrderType orderType, 
    /// int quantity, double limitPrice, double stopPrice)
    /// </summary>
    public class TradeSignal
	{
		public virtual string SignalToStr() {
			string str = this.BarNo + ":" + this.SignalName + Environment.NewLine
				+ "Lmt:" + this.LimitPrice + ", Stp:" + this.StopPrice
				+ ", PrcOffset:" + this.PriceOffset;
			return str;
		}
		
		#region Protperies
		/// <summary>
		/// The barNo the signal refer to
		/// </summary>
		[Range(0, int.MaxValue)]
 		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		public int BarNo
		{
			get; set;
		}
		
		/// <summary>
		/// The bar in progress the signal/trade refer to
		/// </summary>
		[Range(0, int.MaxValue)]
 		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		public int BarInProgress
		{
			get; set;
		}
		
		/// <summary>
		/// The name of the signal
		/// Assign the Ordername with this name
		/// </summary>
 		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		public string SignalName
		{
			get{
				return SignalType.ToString() + "-"
					+ SignalSource.ToString() + "-"
					+ Action.ToString() + "-"
					+ Order_Type.ToString();
			}
			//set;
		}

		/// <summary>
		/// The type of the signal
		/// </summary>		
 		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		[DefaultValueAttribute(TradeSignalType.Entry)]
		public TradeSignalType SignalType
		{
			get; set;
		}
		
		/// <summary>
		/// The source of the signal: indicator, command, event
		/// </summary>		
 		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		[DefaultValueAttribute(TradeSignalSource.Indicator)]
		public TradeSignalSource SignalSource
		{
			get; set;
		}
		
		[Description("Calculation mode for orders")]
 		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		[DefaultValueAttribute(CalculationMode.Price)]
        public CalculationMode OrderCalculationMode
        {
            get; set;
        }
				
		[Description("Action for orders:OrderAction.Buy,BuyToCover,Sell,SellShort")]
 		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		[DefaultValueAttribute(null)]
        public OrderAction Action
        {
            get; set;
        }
		
		[Description("Type for orders:Limit,Market,MIT,StopMarket,StopLimit")]
 		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		[DefaultValueAttribute(null)]
        public OrderType Order_Type
        {
            get; set;
        }
		
		[Description("Quantity for each order from the signal")]
 		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		[DefaultValueAttribute(1)]
        public int Quantity
        {
            get; set;
        }
		
		[Description("Limit Price for orders")]
 		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		[DefaultValueAttribute(0)]
        public double LimitPrice
        {
            get; set;
        }
		
		[Description("Stop Price for orders")]
 		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		[DefaultValueAttribute(0)]
        public double StopPrice
        {
            get; set;
        }
		
		[Description("Price offset by currency/ticks/pips/percent")]
 		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		[DefaultValueAttribute(0)]
        public int PriceOffset
        {
            get; set;
        }
		
		#endregion
	}
}
