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
	/// This object carry the actions that a trade defines:
	/// entry, stop loss, profit target
	/// it is stored by bar and trigger the strategy to put orders when conditions met
	/// </summary>
	public class TradeAction
	{		
		#region Methods
		public TradeAction() {
			this.Executed = false;
		}
		#endregion
		
		#region Protperies
		/// <summary>
		/// The barNo the trade action refers to
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
		public string ActionName
		{
			get; set;
		}

		/// <summary>
		/// The type of the signal
		/// </summary>
		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		[DefaultValueAttribute(TradeActionType.Bracket)]
		public TradeActionType TradeActionType
		{
			get; set;
		}
				
		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		[DefaultValueAttribute(null)]
		public TradeSignal EntrySignal {
			get; set;
		}
		
		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		[DefaultValueAttribute(null)]
		public TradeSignal StopLossSignal {
			get; set;
		}
		
		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		[DefaultValueAttribute(null)]
		public TradeSignal ProfitTargetSignal {
			get; set;
		}
		
		/// <summary>
		/// Entry price
		/// </summary>
		public double EntryPrice {
			get{
				if(EntrySignal == null) return -1;
				else if(OrderType.StopMarket.Equals(EntrySignal.Order_Type))
					return EntrySignal.StopPrice;
				else return EntrySignal.LimitPrice;
			}
			set {
				if(OrderType.StopMarket.Equals(EntrySignal.Order_Type))
					EntrySignal.StopPrice = value;
				else EntrySignal.LimitPrice = value;
			}
		}
		
		/// <summary>
		/// StopLoss price, stop price only
		/// </summary>
		public double StopLossPrice {
			get{ return StopLossSignal==null? -1 : StopLossSignal.StopPrice; }
			set { StopLossSignal.StopPrice = value;	}
		}
		
		/// <summary>
		/// ProfitTarget price, limit price only
		/// </summary>
		public double ProfitTargetPrice {
			get{ return ProfitTargetSignal==null? -1 : ProfitTargetSignal.LimitPrice; }
			set { ProfitTargetSignal.LimitPrice = value; }
		}
		
		/// <summary>
		/// Trailing ProfitTarget tics
		/// </summary>
		public int TrailingProfitTargetTics {
			get{ return ProfitTargetSignal.PriceOffset; }
			set { ProfitTargetSignal.PriceOffset = value; }
		}
		
		/// <summary>
		/// The action has been taken or not
		/// </summary>
		public bool Executed { get; set; }
		
		#endregion
	}
}
