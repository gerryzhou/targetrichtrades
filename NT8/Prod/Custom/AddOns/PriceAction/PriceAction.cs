#region Using declarations
using NinjaTrader.NinjaScript.Indicators;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.AddOns.PriceActions
{
    /// <summary>
    /// This file holds main price action classes.
    /// Price action generally refers to the up and down movement of a security's price when it is plotted over time.
	/// PriceAction includes SimplePriceMove and CombinedPriceMoves;
    /// enum PriceActionType { UpTight, UpWide, DnTight, DnWide, RngTight, RngWide, UnKnown };
    ///
    /// PriceAtion and volatility measurement
    /// min and max ticks of up/down expected
    /// shrinking, expanding, or paralleling motion;
    /// </summary>
    public class PriceAction
    {
        public PriceActionType paType;
		public ChannelType channelType;
		public Volatility voltality;
//        public int minUpTicks;
//        public int maxUpTicks;
//        public int minDownTicks;
//        public int maxDownTicks;
		
        public PriceAction(PriceActionType pat)
        {
            this.paType = pat;
//            this.minUpTicks = min_UpTicks;
//            this.maxUpTicks = max_UpTicks;
//            this.minDownTicks = min_DnTicks;
//            this.maxDownTicks = max_DnTicks;
        }
        public PriceAction(PriceActionType pat, Volatility volatility)
        {
            this.paType = pat;
			this.voltality = volatility;
        }
	}
}
	
	












