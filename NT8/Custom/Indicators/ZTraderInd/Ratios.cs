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
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.ZTraderInd
{
	/// <summary>
	/// Fibonacci ratio: retracement and extensions
	///  Gartley Pattern: 0.618, 0.382, 0.786, 1.618
	/// </summary>
	/// 
	public static class FibRatio {
		public const double R1 = 0.236;
		public const double R2 = 0.382;
		public const double R3 = 0.5;
		public const double R4 = 0.618;
		public const double R5 = 0.764;
		public const double E1 = 1 + R1;
		public const double E2 = 1 + R2;
		public const double E3 = 1 + R3;
		public const double E4 = 1 + R4;
		public const double E5 = 2;
		public const double E6 = 2 + R4;
		
		public static double GetFibRetracePrice(double prc, FibRatioType fibRType) {
			switch(fibRType) {
				case FibRatioType.R1: prc = prc*R1;
					break;
				case FibRatioType.R2: prc = prc*R2;
					break;
				case FibRatioType.R3: prc = prc*R3;
					break;
				case FibRatioType.R4: prc = prc*R4;
					break;
				case FibRatioType.R5: prc = prc*R5;
					break;
			}
			return prc;
		}
		
		public static double GetFibExtensionPrice(double prc, FibRatioType fibRType) {
			switch(fibRType) {
				case FibRatioType.E1: prc = prc*E1;
					break;
				case FibRatioType.E2: prc = prc*E2;
					break;
				case FibRatioType.E3: prc = prc*E3;
					break;
				case FibRatioType.E4: prc = prc*E4;
					break;
				case FibRatioType.E5: prc = prc*E5;
					break;
				case FibRatioType.E6: prc = prc*E6;
					break;
			}			
			return prc;
		}		
	}
	
	/// <summary>
	/// Gann ratio: retracement and extensions
	/// 0.25, 0.5, 0.75
	/// </summary>
	/// 
	public static class GannRatio {
		public const double R1 = 0.25;
		public const double R2 = 0.5;
		public const double R3 = 0.75;
		public const double E1 = 1 + R1;
		public const double E2 = 1 + R2;
		public const double E3 = 1 + R3;
		public const double E4 = 2;
		
		public static double GetGannRetracePrice(double prc, GannRatioType GannRType) {
			switch(GannRType) {
				case GannRatioType.R1: prc = prc*R1;
					break;
				case GannRatioType.R2: prc = prc*R2;
					break;
				case GannRatioType.R3: prc = prc*R3;
					break;
			}
			return prc;
		}
		
		public static double GetGannExtensionPrice(double prc, GannRatioType GannRType) {
			switch(GannRType) {
				case GannRatioType.E1: prc = prc*E1;
					break;
				case GannRatioType.E2: prc = prc*E2;
					break;
				case GannRatioType.E3: prc = prc*E3;
					break;
				case GannRatioType.E4: prc = prc*E4;
					break;
			}			
			return prc;
		}
	}	
}



































