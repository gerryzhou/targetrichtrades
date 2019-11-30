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
//		private TradeSignal entrySignal = null;
//		private TradeSignal stopLossSignal = null;
//		private TradeSignal profitTargetSignal = null;
		
//		private Direction trendDir = new Direction();//TrendDirection.UnKnown; //1=up, -1=down, 0=flat/unknown
//		private Breakout breakoutDir = Breakout.UnKnown; //1=bk up, -1=bk down, 0=no bk/unknown
//		private Reversal reversalDir = Reversal.UnKnown; //1=rev up, -1=rev down, 0=no rev/unknown
//		private SupportResistanceRange<SupportResistanceBar> sptRst;
		
		#region Methods
		
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
				if(OrderType.StopMarket.Equals(EntrySignal.OrderType))
					return EntrySignal.StopPrice;
				else return EntrySignal.LimitPrice;
			}
			set {
				if(OrderType.StopMarket.Equals(EntrySignal.OrderType))
					EntrySignal.StopPrice = value;
				else EntrySignal.LimitPrice = value;
			}
		}
		
		/// <summary>
		/// StopLoss price, stop price only
		/// </summary>
		public double StopLossPrice {
			get{ return StopLossSignal.StopPrice; }
			set { StopLossSignal.StopPrice = value;	}
		}
		
		/// <summary>
		/// ProfitTarget price, limit price only
		/// </summary>
		public double ProfitTargetPrice {
			get{ return ProfitTargetSignal.LimitPrice; }
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
