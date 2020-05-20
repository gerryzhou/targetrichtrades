#region Using declarations
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.NinjaScript.Indicators;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.AddOns.PriceActions
{
	/// <summary>
	/// The period of chart being investigated
	/// The start bar and end bar can be different "By-Type":
	/// ByTime, ByVolume, ByPrice, ByBarNo
	/// There is no overlap for periods
	/// Match the start bar first, then match 
	/// </summary>
	public class Period
	{
		private SignalBarByType startBarBy = SignalBarByType.ByTime;
		private SignalBarByType endBarBy = SignalBarByType.ByTime;
		
		private int startBarNo = -1;
		private int endBarNo = -1;
		
		#region Properties
		/// <summary>
		/// </summary>
		[Browsable(false)]
		[XmlIgnore()]
		[NinjaScriptProperty]
		[DefaultValueAttribute(SignalBarByType.ByTime)]
		public SignalBarByType StartBarBy
		{
			get { return startBarBy; }
			set { startBarBy = value; }
		}

		/// <summary>
		/// </summary>
		[Browsable(false)]
		[XmlIgnore()]
		[DefaultValueAttribute(-1)]
		public int StartBarNo
		{
			get { return startBarNo; }
			set { startBarNo = value; }
		}
		
		/// <summary>
		/// </summary>
		[Browsable(false)]
		[XmlIgnore()]
		[NinjaScriptProperty]
		[DefaultValueAttribute(SignalBarByType.ByTime)]
		public SignalBarByType EndBarBy
		{
			get { return endBarBy; }
			set { endBarBy = value; }
		}

		/// <summary>
		/// </summary>
		[Browsable(false)]
		[XmlIgnore()]
		[DefaultValueAttribute(-1)]
		public int EndBarNo
		{
			get { return endBarNo; }
			set { endBarNo = value; }
		}
		
		#endregion
	}
}




