#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The proxy indicator that carry general indicator methods to strategy;
	/// All the methods those only need to impact the ZTrader framework will be implemented here.
	/// </summary>
	public partial class GIndicatorBase : Indicator
	{
		private List<Indicator> listIndicator = new List<Indicator>();
		private IndicatorSignal indSignal;
	
		#region Methods
		public string GetAccName() {
			return accName;
		}
		
		public Volatility GetVolatility() {return null;}
		
		public MarketCycle GetMarketCycle() {return null;}
		
		public virtual Direction GetDirection() {return null;}
		
		//public virtual SupportResistance GetSupport(){return null;}
		//public virtual SupportResistance GetResistance(){return null;}
		public virtual SupportResistanceBar GetSupportResistance(int barNo, SupportResistanceType srType){return null;}

		public IndicatorSignal CheckIndicatorSignal() {
			return null;
		}
		
		public void AddIndicator(Indicator i) {
			this.listIndicator.Add(i);
		}

		#endregion				
	}
}
