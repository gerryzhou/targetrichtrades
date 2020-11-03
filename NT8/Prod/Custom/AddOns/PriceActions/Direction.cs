#region Using declarations
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.NinjaScript.Indicators;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.AddOns.PriceActions
{
    public class Direction
	{
		private TrendDirection trendDir = TrendDirection.UnKnown;
		
		#region Properties		
		/// <summary>
		/// </summary>
		[Browsable(false), XmlIgnore()]
		public TrendDirection TrendDir
		{
			get { return trendDir; }
			set { trendDir = value; }
		}
		#endregion
	}
}
