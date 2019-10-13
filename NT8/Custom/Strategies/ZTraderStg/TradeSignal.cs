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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Indicators.PriceActions;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies.ZTraderStg
{
	/// <summary>
	/// This object carry the signal that can trigger a trade (file an order)
	/// It provides the essentials for SubmitOrderUnmanaged
	/// (OrderAction orderAction, OrderType orderType, 
	/// int quantity, double limitPrice, double stopPrice)
	/// </summary>
	public class TradeSignal
	{		
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
		/// The name of the signal
		/// </summary>
 		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		public string SignalName
		{
			get; set;
		}

		/// <summary>
		/// The type of the signal
		/// </summary>		
 		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		[DefaultValueAttribute(TradeSignalType.Entry)]
		public TradeSignalType TradeSignalType
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
        public OrderType OrderType
        {
            get; set;
        }
		
		[Description("Quantity for orders")]
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





























