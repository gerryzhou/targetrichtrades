#region Using declarations
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Indicators.PriceActions;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
    /// <summary>
    /// This file defined the interfaces talking with indicators;
    /// This interface needs to work with multiple indicators in the strategy
    /// to generate the signal or get the value of the functions 
    /// </summary>
    public partial class GStrategyBase : Strategy
	{		
		#region Variables

		#endregion

		#region Methods
		public virtual void GetMarketContext() {
			MarketCTX = ReadCmdParaObj<MktContext>();
			//ReadCtxParaObj();
		}
		/// <summary>
		/// Volatility measurement is for target, stop loss, etc.
		/// </summary>
		/// <returns></returns>
		public virtual Volatility GetVolatility() {return null;}
		
		/// <summary>
		/// MarketCycle is for 
		/// </summary>
		/// <returns></returns>
		public virtual MarketCycle GetMarketCycle() {return null;}
		
		/// <summary>
		/// Direction is to tell up/down, buy or sell;
		/// </summary>
		/// <returns></returns>
		public virtual Direction GetDirection(GIndicatorBase indicator) {return null;}
		
		/// <summary>
		/// Support and resistance is to define entry/exit level, target and stop loss
		/// </summary>
		/// <returns></returns>
		public virtual SupportResistanceBar GetSupport(){return null;}
		public virtual SupportResistanceBar GetResistance(){return null;}
		public virtual SupportResistanceBar GetSptRest(int barNo) {return null;}
		
		/// <summary>
		/// Check if divergence occurs in this indicator;
		/// </summary>
		/// <returns></returns>
		public virtual DivergenceType CheckDivergence(GIndicatorBase indicator) {
			return DivergenceType.UnKnown;
		}
		
		public virtual double GetMomentum() {
			return 0;
		}

		//public virtual GetIndicatorSignals(){}
		
		public virtual void SetVolatility(){}
		
		public virtual void SetDirection(){}
		
		public virtual void SetMarketCycle(){}
		
		public virtual void SetSnP(){}
		
		public virtual void SetInflection(){}
		
		public virtual void SetDivergence(){}
		
		public virtual void SetMomentum(){}
		
		/// <summary>
		/// Detect if the market condition has changed or not since last signal
		/// </summary>
		/// <returns></returns>
		public virtual bool HasMarketContextChanged() {return false;}

		#endregion
		
        #region Properties
		[Browsable(false), XmlIgnore()]
		public MktContext MarketCTX {get;set;}
				
        [Browsable(false), XmlIgnore()]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        public Series<double> StartHM
        {
            get;set;// { return Values[0]; }
        }

        [Browsable(false), XmlIgnore()]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		public Series<double> EndHM
        {
            get;set;// { return Values[1]; }
        }
		
        [Browsable(false), XmlIgnore()]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
		public GIndicatorProxy IndicatorProxy
        {
            get;set;
        }

//		[NinjaScriptProperty]
//		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
//		[Display(Name="CustomTime1", Description="CustomTime-1", Order=4, GroupName="Parameters")]
//		public DateTime CustomTime1
//		{ get; set; }
        #endregion
	}
}
