#region Using declarations
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using NinjaTrader.NinjaScript.Indicators.PriceActions;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.ZTraderInd
{
    /// <summary>
    /// This object carry the signal that can trigger a trade (entry or exit)
    /// It includes:
    /// * BarNo, SigName, SigType
    /// * SignalAction: breakout, reversal, pullback, crossOver/Under, inflection, etc.
    /// * Direction
    /// * Volatility
    /// * SnR
    /// </summary>
    public class IndicatorSignal
	{
		
		#region Protperies
		/// <summary>
		/// The barNo the signal refer to
		/// </summary>
		[Range(0, int.MaxValue)]
		[Browsable(false), XmlIgnore]
		public int BarNo
		{
			get; set;
		}

		/// <summary>
		/// The name of the signal
		/// </summary>
		[Browsable(false), XmlIgnore]
		public string SignalName
		{
			get; set;
		}

		/// <summary>
		/// The type of the signal
		/// </summary>		
		[Browsable(false), XmlIgnore]
		public SignalType IndicatorSignalType
		{
			get; set;
		}		
		
		[Browsable(false), XmlIgnore]
		//[DefaultValueAttribute(TrendDirection.UnKnown)]
		public Direction TrendDir {
			get; set;
		}

		[Browsable(false), XmlIgnore]
		[DefaultValueAttribute(BreakoutDirection.UnKnown)]
		public BreakoutDirection BreakoutDir {
			get; set;
		}
		
		[Browsable(false), XmlIgnore]
		//[DefaultValueAttribute(Reversal.UnKnown)]
		public Reversal ReversalDir {
			get; set;
		}
		
		[Browsable(false), XmlIgnore]
		public SupportResistanceRange<SupportResistanceBar> SnR {
			get; set;
		}
		
		[Browsable(false), XmlIgnore]
		public SignalAction SignalAction {
			get; set;
		}
		
		#endregion
	}
}






