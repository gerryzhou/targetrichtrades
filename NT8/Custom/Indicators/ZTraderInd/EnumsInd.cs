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
	//Enum conversion:
    //Enum e = Question.Role;
    //int i = Convert.ToInt32(e);
    //YourEnum foo = (YourEnum)yourInt;
    public enum SessionBreak { AfternoonClose, EveningOpen, MorningOpen, NextDay };

    public enum TrendDirection { Up = 1, Down = -1, Flat = 0, UnKnown = 9 };
    public enum Breakout { Up = 1, Down = -1, UnKnown = 0 };
    public enum Reversal { Up = 1, Down = -1, UnKnown = 0 };
	public enum SupportResistanceType {Support = 1, Resistance = -1, Unknown = 0};
	public enum CrossoverType {Above, Below, Both};

    public enum PriceActionType { UpTight, UpWide, DnTight, DnWide, RngTight, RngWide, UnKnown };
	public enum PriceSubtype {Close, Open, High, Low, Median};
	
	public enum MarketCycleType {
		W1Early, W1Middle, W1End,
		W2Early, W2Middle, W2End,
		W3Early, W3Middle, W3End, 
		W4Early, W4Middle, W4End,
		W5Early, W5Middle, W5End,
		WAEarly, WAMiddle, WAEnd,
		WBEarly, WBMiddle, WBEnd,
		WCEarly, WCMiddle, WCEnd };
	
	public enum MovingAvgType {SMA, EMA, TMA};
	
	public enum FibRatioType { R1, R2, R3, R4, R5, E1, E2, E3, E4, E5, E6};
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
}


























