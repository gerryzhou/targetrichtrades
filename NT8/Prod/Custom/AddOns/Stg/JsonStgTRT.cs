#region Using declarations
#endregion

//This namespace holds Add ons in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
    public class JsonStgTRT
    {
		public string Symbol{get;set;}
		public string ChartType{get;set;}
		public string Version{get;set;}
		public string SessionId{get;set;}
		public string Date{get;set;}
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
		
		public int EnTicOffset{get;set;}
		public int ExTrailTics{get;set;}
		public int StoplossTics{get;set;}
		
		public double S1{get;set;}
		public double R1{get;set;}
		public double S2{get;set;}
		public double R2{get;set;}
		public double S3{get;set;}
		public double R3{get;set;}
		public double S4{get;set;}
		public double R4{get;set;}
		public double S5{get;set;}
		public double R5{get;set;}
		
		public double T1{get;set;}
		public double T2{get;set;}

    }
}
