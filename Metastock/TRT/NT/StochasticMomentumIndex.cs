#region Using declarations
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    /// <summary>
    /// Enter the description of your new custom indicator here
    /// </summary>
    [Description("Enter the description of your new custom indicator here")]
    public class StochasticMomentumIndex : Indicator
    {
        #region Variables
        // Wizard generated variables
            private int percentD = 3; // Default setting for PercentD
            private int percentK = 5; // Default setting for PercentK
        // User defined variables (add any user defined variables below)
		    private double minLow, maxHigh;
		    private DataSeries relDiff, diff, avgRel;
		    private DataSeries avgDiff;
		    
        #endregion

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        protected override void Initialize()
        {
            Add(new Plot(Color.DarkGreen, PlotStyle.Line, "SMI"));
            Add(new Plot(Color.DarkMagenta, PlotStyle.Line, "SMIAvg"));
            Add(new Line(Color.DarkMagenta, 40, "OverBought"));
            Add(new Line(Color.DarkMagenta, -40, "OverSold"));
            CalculateOnBarClose	= false;
            Overlay				= false;
            PriceTypeSupported	= true;
			
			relDiff = new DataSeries(this);
			diff = new DataSeries(this);
			avgRel = new DataSeries(this);
			avgDiff = new DataSeries(this);		
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
            // Use this method for calculating your indicator values. Assign a value to each
            // plot below by replacing 'Close[0]' with your own formula.
			minLow = MIN (Low, percentK)[0];
			maxHigh = MAX (High, percentK)[0];
			relDiff.Set(Close[0] - (maxHigh + minLow)/2);
			diff.Set(maxHigh - minLow);
			avgRel.Set(EMA(EMA(relDiff, percentD), percentD)[0]);
			avgDiff.Set(EMA(EMA(diff, percentD),percentD)[0]);
			
			if (CurrentBar == 0) {
				SMI.Set(0);
			} else {
				SMI.Set(avgRel[0]/(avgDiff[0]/2)*100);
			}
			SMIAvg.Set(EMA(SMI, percentD)[0]);
		}
	
		


        #region Properties
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public DataSeries SMI
        {
            get { return Values[0]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public DataSeries SMIAvg
        {
            get { return Values[1]; }
        }

        [Description("")]
        [Category("Parameters")]
        public int PercentD
        {
            get { return percentD; }
            set { percentD = Math.Max(1, value); }
        }

        [Description("")]
        [Category("Parameters")]
        public int PercentK
        {
            get { return percentK; }
            set { percentK = Math.Max(1, value); }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    public partial class Indicator : IndicatorBase
    {
        private StochasticMomentumIndex[] cacheStochasticMomentumIndex = null;

        private static StochasticMomentumIndex checkStochasticMomentumIndex = new StochasticMomentumIndex();

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public StochasticMomentumIndex StochasticMomentumIndex(int percentD, int percentK)
        {
            return StochasticMomentumIndex(Input, percentD, percentK);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public StochasticMomentumIndex StochasticMomentumIndex(Data.IDataSeries input, int percentD, int percentK)
        {
            checkStochasticMomentumIndex.PercentD = percentD;
            percentD = checkStochasticMomentumIndex.PercentD;
            checkStochasticMomentumIndex.PercentK = percentK;
            percentK = checkStochasticMomentumIndex.PercentK;

            if (cacheStochasticMomentumIndex != null)
                for (int idx = 0; idx < cacheStochasticMomentumIndex.Length; idx++)
                    if (cacheStochasticMomentumIndex[idx].PercentD == percentD && cacheStochasticMomentumIndex[idx].PercentK == percentK && cacheStochasticMomentumIndex[idx].EqualsInput(input))
                        return cacheStochasticMomentumIndex[idx];

            StochasticMomentumIndex indicator = new StochasticMomentumIndex();
            indicator.BarsRequired = BarsRequired;
            indicator.CalculateOnBarClose = CalculateOnBarClose;
            indicator.Input = input;
            indicator.PercentD = percentD;
            indicator.PercentK = percentK;
            indicator.SetUp();

            StochasticMomentumIndex[] tmp = new StochasticMomentumIndex[cacheStochasticMomentumIndex == null ? 1 : cacheStochasticMomentumIndex.Length + 1];
            if (cacheStochasticMomentumIndex != null)
                cacheStochasticMomentumIndex.CopyTo(tmp, 0);
            tmp[tmp.Length - 1] = indicator;
            cacheStochasticMomentumIndex = tmp;
            Indicators.Add(indicator);

            return indicator;
        }

    }
}

// This namespace holds all market analyzer column definitions and is required. Do not change it.
namespace NinjaTrader.MarketAnalyzer
{
    public partial class Column : ColumnBase
    {
        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.StochasticMomentumIndex StochasticMomentumIndex(int percentD, int percentK)
        {
            return _indicator.StochasticMomentumIndex(Input, percentD, percentK);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public Indicator.StochasticMomentumIndex StochasticMomentumIndex(Data.IDataSeries input, int percentD, int percentK)
        {
            return _indicator.StochasticMomentumIndex(input, percentD, percentK);
        }

    }
}

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    public partial class Strategy : StrategyBase
    {
        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.StochasticMomentumIndex StochasticMomentumIndex(int percentD, int percentK)
        {
            return _indicator.StochasticMomentumIndex(Input, percentD, percentK);
        }

        /// <summary>
        /// Enter the description of your new custom indicator here
        /// </summary>
        /// <returns></returns>
        public Indicator.StochasticMomentumIndex StochasticMomentumIndex(Data.IDataSeries input, int percentD, int percentK)
        {
            if (InInitialize && input == null)
                throw new ArgumentException("You only can access an indicator with the default input/bar series from within the 'Initialize()' method");

            return _indicator.StochasticMomentumIndex(input, percentD, percentK);
        }

    }
}
#endregion
