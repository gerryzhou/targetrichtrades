#region Using declarations
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.NinjaScript.Indicators.PriceActions;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies.ZTraderStg
{
    /// <summary>
    /// This object carry the parameters of the command:
    /// it can directly loaded from JSON string;
    /// </summary>
    public class CmdParam
	{		
		#region Methods
		#endregion
		
		#region Protperies
		/// <summary>
		/// The type of the command
		/// </summary>
		[XmlIgnore, Browsable(false)]
		public StrategyCmd cmdStrategy
		{
			get; set;
		}
		
		/// <summary>
		/// The parameter map
		/// </summary>
		[XmlIgnore, Browsable(false)]
		public IndicatorCmd cmdIndicator
		{
			get; set;
		}
		
		[XmlIgnore, Browsable(false)]
		public TriggerCmd cmdTrigger {
			get; set;
		}
		
		[XmlIgnore, Browsable(false)]
		public MoneyManagementCmd cmdMoneyManagement {
			get; set;
		}
		
		[XmlIgnore, Browsable(false)]
		public TradeManagementCmd cmdTradeManagement {
			get; set;
		}
		
		[XmlIgnore, Browsable(false)]
		public MarketContextCmd cmdMarketContext {
			get; set;
		}
		
		#endregion
	}
	
	public class StrategyCmd {
		ArrayList cmd;
	}
	public class IndicatorCmd {
		ArrayList cmd;
	}
	public class TriggerCmd {
		ArrayList cmd;
	}
	public class MoneyManagementCmd {
		ArrayList cmd;
	}
	public class TradeManagementCmd {
		ArrayList cmd;
	}
	
	public class MarketContextCmd {	
		public Dictionary<string, List<MarketContext>> CTX_Daily{get;set;}
		public Dictionary<string, List<MarketContext>> CTX_Weekly{get;set;}
	}
}
