#region Using declarations
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.PriceActions
{
    public class Reversal
	{
		private ReversalType reversalType = ReversalType.Percent;
		private ReversalDirection reversalDir = ReversalDirection.UnKnown; 
		
		#region Properties		
		/// <summary>
		/// </summary>
		[Browsable(false), XmlIgnore()]
		[DefaultValueAttribute(ReversalDirection.UnKnown)]
		public ReversalDirection ReversalDir
		{
			get { return reversalDir; }
			set { reversalDir = value; }
		}
		#endregion
	}
}






