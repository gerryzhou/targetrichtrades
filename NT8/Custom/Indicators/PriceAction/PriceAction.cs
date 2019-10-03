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
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.PriceActions
{
    /// <summary>
    /// This file holds main price action classes.
    /// Price action generally refers to the up and down movement of a security's price when it is plotted over time.
	/// PriceAction includes SimplePriceMove and CombinedPriceMoves;
    /// enum PriceActionType { UpTight, UpWide, DnTight, DnWide, RngTight, RngWide, UnKnown };
    ///
    /// PriceAtion and volatility measurement
    /// min and max ticks of up/down expected
    /// shrinking, expanding, or paralleling motion;
    /// </summary>
    public class PriceAction
    {
        public PriceActionType paType;
        public int minUpTicks;
        public int maxUpTicks;
        public int minDownTicks;
        public int maxDownTicks;
		
        public PriceAction(PriceActionType pat, int min_UpTicks, int max_UpTicks, int min_DnTicks, int max_DnTicks)
        {
            this.paType = pat;
            this.minUpTicks = min_UpTicks;
            this.maxUpTicks = max_UpTicks;
            this.minDownTicks = min_DnTicks;
            this.maxDownTicks = max_DnTicks;
        }
    }
}
	
	







