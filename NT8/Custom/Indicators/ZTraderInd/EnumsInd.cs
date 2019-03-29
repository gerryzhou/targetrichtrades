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
	public enum DivergenceType {Divergent=-1, Convergent=1, UnKnown=0};

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
	
	public enum GannRatioType { R1, R2, R3, E1, E2, E3, E4};
}




