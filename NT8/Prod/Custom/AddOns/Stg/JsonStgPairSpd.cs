#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
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
		public CtxPairSpdDaily() {
			DictCtxPairSpd = new Dictionary<string, List<CtxPairSpd>>();
		}
		
		public void AddDayCtx(string key, List<CtxPairSpd> list) {
			if(DictCtxPairSpd.ContainsKey(key))
				DictCtxPairSpd.Remove(key);
			DictCtxPairSpd.Add(key, list);
		}
		
		public CtxPairSpd GetDayCtx(string key) {
			List<CtxPairSpd> list;
			DictCtxPairSpd.TryGetValue(key, out list);
			if(list != null && list.Count > 0)
				return list[0];
			else return null;
		}
		
		//public List<CtxPairSpd> CtxDaily{get;set;}
		[Browsable(false), XmlIgnore()]
		public Dictionary<string, List<CtxPairSpd>> DictCtxPairSpd {
			get; set;
		}
		
		[Browsable(false), XmlIgnore()]
		public string KeyLastDay
		{ get; set; }
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
		public string PositionInBand{get;set;}
		public string TradingStyle{get;set;}
		public string TradingDirection{get;set;}
		
		public double PairATRRatio{get;set;}
		
		public int BarsLookback{get;set;}
		public int DaysLookback{get;set;}
		public int MALength{get;set;}
	}
}
