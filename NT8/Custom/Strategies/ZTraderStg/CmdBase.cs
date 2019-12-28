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
	public class CmdBase
	{		
		#region Methods
		public CmdBase() {
			ParamMap = new Dictionary<string, string>();
		}
		#endregion
		
		#region Protperies
		/// <summary>
		/// The type of the command
		/// </summary>
		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		[DefaultValueAttribute(TradeActionType.Bracket)]
		public CommandType CmdType
		{
			get; set;
		}
		
		/// <summary>
		/// The parameter map
		/// </summary>
		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		public Dictionary<string,string> ParamMap
		{
			get; set;
		}
		
		[NinjaScriptProperty, XmlIgnore, Browsable(false)]
		[DefaultValueAttribute(null)]
		public GStrategyBase InstStrategy {
			get; set;
		}
		
		#endregion
	}
}
