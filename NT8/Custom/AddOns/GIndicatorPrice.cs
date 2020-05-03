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
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Indicators.PriceActions;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Class to hold volume
	/// 
	/// </summary>
	public partial class GIndicatorBase : Indicator
	{
		/// <summary>
		/// Hold the indicator volume from the underlining indicator
		/// 
		/// </summary>

		
		#region Methods		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="barsAgo"></param>
		/// <param name="includeCurBar"></param>
		/// <returns></returns>
		public double GetHighestPrice(int barsAgo, bool includeCurBar) {			
			double hiPrc = includeCurBar? High[0] : High[1];
			if(barsAgo > 0) {
//				for(int i=0; i<barsAgo; i++) {
//					Print(String.Format("{0}:GetHighestPrice={1}", CurrentBar, High[i]));
//				}
				hiPrc = Math.Max(hiPrc, High[HighestBar(High, barsAgo)]);
			}
			PrintLog(true, false, 
				CurrentBar + ":hiPrc=" + hiPrc 
				+ ";barsAgo=" + barsAgo);
			return hiPrc;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="barsAgo">LowestBar(Low, Bars.BarsSinceNewTradingDay);</param>
		/// <param name="includeCurBar"></param>
		/// <returns></returns>
		public double GetLowestPrice(int barsAgo, bool includeCurBar) {
			double loPrc = includeCurBar? Low[0] : Low[1];
			if(barsAgo > 0) {
				loPrc = Math.Min(loPrc, Low[LowestBar(Low, barsAgo)]);
			}
			PrintLog(true, false, 
				CurrentBar + ":loPrc=" + loPrc
				+ ";barsAgo=" + barsAgo);
			return loPrc;
		}
		
		public double GetTypicalPrice(int barsAgo) {
			MasterInstrument maIns = Bars.Instrument.MasterInstrument;			
			return maIns.RoundToTickSize(Typical[barsAgo]);
		}
		
		public double GetTickValue() {
			MasterInstrument maIns = Bars.Instrument.MasterInstrument;
			//Print("TickSize, name, pointvalue=" + maIns.TickSize + "," + maIns.Name + "," + maIns.PointValue);
			return maIns.TickSize*maIns.PointValue;
		}
		
		public double GetPriceByCurrency(double amt) {
			return amt/Bars.Instrument.MasterInstrument.PointValue;
		}
		
		public double GetPriceByTicks(int tics) {
			return TickSize*tics;
		}
		
		public double MovePriceByTicks(double prc, int tics) {
			return prc + GetPriceByTicks(tics);
		}

		public double MovePriceByCurrency(double prc, double amt) {
			return prc + GetPriceByCurrency(amt);			
		}
		
		public int GetTicksByCurrency(double amt) {
			int tic = -1;
			if(amt > 0)
				tic = (int)(amt/GetTickValue());
			return tic;
		}
		
		public double GetCurrencyByTicks(int tics) {
			double amt = -1;
			if(tics > 0)
				amt = tics*GetTickValue();
			return amt;
		}
		
		#endregion
	}
}
