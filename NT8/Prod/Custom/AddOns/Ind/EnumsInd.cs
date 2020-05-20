#region Using declarations
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
    //Enum conversion:
    //Enum e = Question.Role;
    //int i = Convert.ToInt32(e);
    //YourEnum foo = (YourEnum)yourInt;
    public enum SessionBreak { AfternoonClose, EveningOpen, MorningOpen, NextDay }

    public enum TrendDirection { Up = 1, Down = -1, Flat = 0, UnKnown = 9 }
    public enum BreakoutDirection { Up = 1, Down = -1, UnKnown = 0 }
    public enum ReversalDirection { Up = 1, Down = -1, UnKnown = 0 }
	public enum SupportResistanceType { Support = 1, Resistance = -1, Unknown = 0 }
	public enum LineCrossType { Above, Below, Both } //over/under or above/below
	public enum DivergenceType { Divergent=-1, Convergent=1, UnKnown=0 }
	public enum SignalBarByType { ByTime=1, ByVolume=2, ByPrice=3, ByBarNo=4 } // Identify singal bar by time/vol/price/barNo

	public enum SignalType { Direction, Volatility, SnR,
					SimplePriceAction, CombinedPriceAction, Unknown }
	
	public enum SignalActionType { TrendUp, TrendDn, BreakoutUp, BreakoutDn, PullbackUp, PullbackDn, ReversalUp, ReversalDn,
					CrossOver, CrossUnder, Divergence, Convergence, InflectionUp, InflectionDn,
					Flat, Unknown }
	
	//The size of High-Low of the bar
	public enum BarRangeType { Doji, Small, Large, Huge }
	//The size of Close-Open of the bar
	public enum BarBodyRangeType { Doji, Small, Large, Huge }

    public enum PriceActionType { UpTight=1, UpWide=2, DnTight=4, DnWide=8, RngTight=16, RngWide=32, UnKnown=64 }
	public enum PriceSubtype { Close, Open, High, Low, Median, Typical }
	
	public enum ChannelType { UpTight=1, UpWide=2, DnTight=4, DnWide=8, RngTight=16, RngWide=32, UnKnown=64 }
	
	public enum MarketCycleType { //1-5 wave or ABC wave
		W1Early, W1Middle, W1End,
		W2Early, W2Middle, W2End,
		W3Early, W3Middle, W3End, 
		W4Early, W4Middle, W4End,
		W5Early, W5Middle, W5End,
		WAEarly, WAMiddle, WAEnd,
		WBEarly, WBMiddle, WBEnd,
		WCEarly, WCMiddle, WCEnd }
	
	public enum MovingAvgType { SMA, EMA, TMA, KAMA }
	
	public enum FibRatioType { R1, R2, R3, R4, R5, E1, E2, E3, E4, E5, E6 }
	
	public enum GannRatioType { R1, R2, R3, E1, E2, E3, E4 }
}












































