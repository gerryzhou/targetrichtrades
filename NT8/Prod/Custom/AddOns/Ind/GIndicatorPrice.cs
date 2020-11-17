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
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.AddOns.PriceActions;
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
		public double GetPriceByType(int barNo, PriceSubtype priceType) {
			Print(CurrentBar + ":GetPriceByType=" + barNo + "," + priceType);
			double prc = 0;
			switch(priceType) {
				case PriceSubtype.Low:
					prc = Bars.GetLow(barNo);
					break;
				case PriceSubtype.High:
					prc = Bars.GetHigh(barNo);
					break;
				case PriceSubtype.Open:
					prc = Bars.GetOpen(barNo);
					break;
				case PriceSubtype.Close:
					prc = Bars.GetClose(barNo);
					break;
			}
			return prc;
		}
		
		public double GetTick4Symbol() {
			return Instrument.MasterInstrument.TickSize;
		}
		
		public double GetOpenPrice(int time_open) {
			if(ToTime(Time[1]) < time_open && ToTime(Time[0]) >= time_open)
				return Open[0];
			else return -1;
		}
		
		public double GetClosePrice(int time_close) {
			if(ToTime(Time[1]) < time_close && ToTime(Time[0]) >= time_close)
				return Close[0];
			else return -1;
		}
		
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
			if(PrintOut > 1)
				PrintLog(true, false, 
				string.Format("{0}: hiPrc={1}, barsAgo={1}", CurrentBar, hiPrc, barsAgo));
			return hiPrc;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="barsAgo"></param>
		/// <param name="includeCurBar"></param>
		/// <returns></returns>
		public double GetHighestPrice(int barsAgo, ISeries<double> data, bool includeCurBar) {			
			double hiPrc = data[0];// includeCurBar? data[0] : data[1];
			if(barsAgo > 0) {
				hiPrc = Math.Max(hiPrc, data[HighestBar(data, barsAgo)]);
			}
			if(PrintOut > 1)
				PrintLog(true, false, 
				string.Format("{0}: hiPrc={1}, barsAgo={1}", CurrentBar, hiPrc, barsAgo));
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
			if(PrintOut > 1)
				PrintLog(true, false, 
				string.Format("{0}: loPrc={1}, barsAgo={1}", CurrentBar, loPrc, barsAgo));
			return loPrc;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="barsAgo">LowestBar(Low, Bars.BarsSinceNewTradingDay);</param>
		/// <param name="includeCurBar"></param>
		/// <returns></returns>
		public double GetLowestPrice(int barsAgo, ISeries<double> data, bool includeCurBar) {
			double loPrc = includeCurBar? data[0] : data[1];
			if(barsAgo > 0) {
				loPrc = Math.Min(loPrc, data[LowestBar(data, barsAgo)]);
			}
			if(PrintOut > 1)
				PrintLog(true, false, 
				string.Format("{0}: loPrc={1}, barsAgo={1}", CurrentBar, loPrc, barsAgo));
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
		
		/// <summary>
		/// Normalize the price to $100 base
		/// </summary>
		/// <param name="prc">the regular price</param>
		/// <param name="basePrc">the base price</param>
		/// <returns></returns>
		public double GetNormalizedPrice(double prc, double basePrc, int fraction) {
			if(basePrc > 0)
				return Math.Round(100*prc/basePrc, fraction);
			else return -1.1;
		}

		/// <summary>
		/// Normalize the ROC price to $100 base
		/// </summary>
		/// <param name="prc">The regular price</param>
		/// <param name="basePrc">The base price</param>
		/// <param name="rocScale">The folds to enlarge the small roc</param>
		/// <param name="fraction">The round digits</param>
		/// <returns>Invalid is -1.1</returns>
		public double GetNormalizedRocPrice(double prc, double basePrc, int rocScale, int fraction) {
			if(basePrc > 0) {
				double roc = rocScale*(prc - basePrc)/basePrc;
				return GetNormalizedPrice(roc, basePrc, fraction);
			}
			else return -1.1;
		}
		
		/// <summary>
		/// Get the covariance by Mean/StdDev
		/// </summary>
		/// <param name="prc">the regular price</param>
		/// <param name="basePrc">the base price</param>
		/// <returns></returns>
		public double GetCoVar(double mean, double stdDev, int scale, int fraction) {
			if(mean == 0)
				return double.MinValue;
			else if(scale <= 0) {
				return Math.Round(stdDev/mean, fraction);
			}
			else
				return Math.Round(scale*stdDev/mean, fraction);
		}
		#endregion
	}
}
