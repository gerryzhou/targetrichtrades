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
		
		/// <summary>
		/// The OCO id for bracket or OCO trade
		/// </summary>		
		[Browsable(false), XmlIgnore]
		[DefaultValueAttribute(null)]
		public string OcoID
		{
			get; set;
		}
		
		[Browsable(false), XmlIgnore]
		[DefaultValueAttribute(null)]
		public TradeSignal EntrySignal {
			get; set;
		}
		
		[Browsable(false), XmlIgnore]
		[DefaultValueAttribute(null)]
		public TradeSignal StopLossSignal {
			get; set;
		}
		
		[Browsable(false), XmlIgnore]
		[DefaultValueAttribute(null)]
		public TradeSignal ProfitTargetSignal {
			get; set;
		}
				
		#endregion
	}
}





























