#region Using declarations
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.NinjaScript.AddOns.PriceActions;

#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
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
		[Browsable(false), XmlIgnore]
		public SignalActionType SignalActionType
		{
			get; set;
		}

		//		[Browsable(false), XmlIgnore]
//		public SupportResistanceRange<SupportResistanceBar> SnR {
//			get; set;
//		}
		
		[Browsable(false), XmlIgnore]
		public SupportResistanceRange<double> SnR {
			get; set;
		}

		[Browsable(false), XmlIgnore]
		public List<PairSpread<int>> PairSpds {
			get; set;
		}
		#endregion
	}
}

