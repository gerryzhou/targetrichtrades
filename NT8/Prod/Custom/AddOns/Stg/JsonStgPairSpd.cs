#region Using declarations
using System.Collections.Generic;
#endregion

//This namespace holds Add ons in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
    public class JsonStgPairSpd
    {
		public string Date{get;set;}
		public IList<CtxPairSpd> CtxDaily{get;set;}
    }
	
	public class CtxPairSpdDaily {
		public List<CtxPairSpd> CtxDaily{get;set;}
	}
	
	public class CtxPairSpd {
		public string Symbol{get;set;}
		public string ChartType{get;set;}
		public int TimeOpen{get;set;}
		public int TimeClose{get;set;}
		public int TimeStart{get;set;}
		public int TimeEnd{get;set;}
		
		public string ChannelType{get;set;}
		public string TrendDirection{get;set;}
		public string TradingStyle{get;set;}
		public string TradingDirection{get;set;}
		
		public int BarsLookback{get;set;}
		public int DaysLookback{get;set;}
		public int MALength{get;set;}
	}
}
