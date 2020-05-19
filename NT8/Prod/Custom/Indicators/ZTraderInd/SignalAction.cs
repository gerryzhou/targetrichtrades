#region Using declarations
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.NinjaScript.Indicators.PriceActions;

#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.ZTraderInd
{
    /// <summary>
    /// This object carry the signal action that represents simple price action includes:
    /// breakout, pullback, reversal, crossOver/Under, inflection, etc. 
    /// * SnR
    /// </summary>
    public class SignalAction
	{	
		#region Protperies
		/// <summary>
		/// The type of the signal
		/// </summary>		
		[Browsable(false)]
		[XmlIgnore]
		public SignalActionType SignalActionType
		{
			get; set;
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public SupportResistanceRange<double> SnR {
			get; set;
		}
		#endregion
	}
}

