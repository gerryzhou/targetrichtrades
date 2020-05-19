#region Using declarations
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.PriceActions
{
    public class MarketContext
	{
//		private int time;
//		private PriceAction priceAction;
		
		public MarketContext(int t, PriceAction pa) {
			Time = t;
			Price_Ation = pa;
		}
		
		#region Properties
		public int Time {
			get;set;
		}
		
		public PriceAction Price_Ation {
			get;set;
		}
		#endregion
		
	/*
		#region SpvPR Vars
		/// <summary>
		/// Loaded from supervised file;
		/// Key1=Date; Key2=Time;
		/// </summary>		
		protected Dictionary<string,Dictionary<int,PriceAction>> Dict_SpvPR = null;
		
		/// <summary>
		/// Bitwise op to tell which Price Action allowed to be the supervised entry approach
		/// 0111 1111: [0 UnKnown RngWide RngTight DnWide DnTight UpWide UpTight]
		/// UnKnown:spvPRBits&0100 000(64)
		/// RngWide:spvPRBits&0010 0000(32), RngTight:spvPRBits&0001 0000(16)
		/// DnWide:spvPRBits&0000 1000(8), DnTight:spvPRBits&0000 0100(4)
		/// UpWide:spvPRBits&0000 0010(2), UpTight:spvPRBits&0000 0001(1)
		/// </summary>

		protected int SpvPRBits = 0;
		
		#endregion
	*/		
	}

//	public class CTXDaily {
//		public string Date{get;set;}
//		public List<MarketContext> TimeCtx{get;set;}
//	}
//	public class CTXWeekly {
//		public string Date{get;set;}
//		public List<MarketContext> TimeCtx{get;set;}
//	}
	
	public class MktContext{
		public DateCtx[] MktCtxDaily{get;set;}
	}
	
	public class DateCtx {
		public string Date{get;set;}
		public TimeCtx[] TimeCtxs{get;set;}
	}
		
	public class TimeCtx {
		public string Time{get;set;}
		public string ChannelType{get;set;}
		public double Support{get;set;}
		public double Resistance{get;set;}
		public int MinUp{get;set;}
		public int MaxUp{get;set;}
		public int MinDn{get;set;}
		public int MaxDn{get;set;}
	}
}































