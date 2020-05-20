#region Using declarations
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.AddOns.PriceActions
{
    public class Volatility
	{
		/// <summary>
		/// Volume in a time period: 
		/// Volume burst at the open;
		/// </summary>
		private double hi;
		private double lo;
		private int minUp;
		private int maxUp;
		private int minDn;
		private int maxDn;
		
		public Volatility(int minUpTicks, int maxUpTicks, int minDnTicks, int maxDnTicks) {
			this.minUp = minUpTicks;
			this.maxUp = maxUpTicks;
			this.minDn = minDnTicks;
			this.maxDn = maxDnTicks;
		}
	}
}







