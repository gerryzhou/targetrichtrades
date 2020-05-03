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
			this.ActionStatus = TradeActionStatus.New;
		}
		
		public virtual bool IsEntryAction() {
			if(this.ActionType != null &&
				(this.ActionType == TradeActionType.Bracket || this.ActionType == TradeActionType.EntrySimple ||
				this.ActionType == TradeActionType.EntryOCO || this.ActionType == TradeActionType.EntryTrailing))
				return true;
			else
				return false;
		}
		
		public virtual bool IsExitAction() {
			if(this.ActionType != null &&
				(this.ActionType == TradeActionType.ExitSimple || this.ActionType == TradeActionType.ExitOCO ||
				this.ActionType == TradeActionType.ExitTrailingSL || this.ActionType == TradeActionType.ExitTrailingPT))
				return true;
			else
				return false;
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
		public TradeActionType ActionType
		{
			get; set;
		}
		
		/// <summary>
		/// The action has been executed or not:
		/// New, Updated, Executed, or UnKnown
		/// </summary>
		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		public TradeActionStatus ActionStatus  { get; set; }
		
		/// <summary>
		/// Include regular entry and scale in entry
		/// </summary>
		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		[DefaultValueAttribute(null)]
		public TradeSignal EntrySignal {
			get; set;
		}
		
		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		[DefaultValueAttribute(null)]
		public TradeSignal ScaleOutSignal {
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
		/// Exit price, for scale out
		/// </summary>
		public double ExitPrice {
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
		#endregion
	}
}
