using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjaTrader.NinjaScript.Indicators.ZTraderInd
{
    /// <summary>
    /// This file holds all user defined indicator classes.
    /// </summary>

    public class ZigZagSwing
    {
        public int Bar_Start;
        public int Bar_End;
        public double Size;
        public double TwoBar_Ratio;
        public ZigZagSwing(int bar_start, int bar_end, double size, double twobar_ratio)
        {
            this.Bar_Start = bar_start;
            this.Bar_End = bar_end;
            this.Size = size;
            this.TwoBar_Ratio = twobar_ratio;
        }
    }
}



