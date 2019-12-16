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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;

using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Indicators.PriceActions;
using NinjaTrader.NinjaScript.Strategies.ZTraderStg;
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
		private List<Indicator> listIndicator = new List<Indicator>();
			
		//protected GIndicatorProxy indicatorProxy;		
        // User defined variables (add any user defined variables below)
        //private int startH = 9; // Default setting for StartH
       // private int startM = 5; // Default setting for StartM
        //private int endH = 11; // Default setting for EndH
       // private int endM = 5; // Default setting for EndM
		//private string accName = ""; //account name from strategy, extracting simply string for print/log;

		#endregion

		#region Methods
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
		
//		public string GetAccName() {
//			return accName;
//		}		
		public void AddIndicator(Indicator i) {
			this.listIndicator.Add(i);
		}
		#endregion
		
        #region Properties
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
//		[XmlIgnore]
//		[Display(Name="CustomColor1", Description="Color-1", Order=1, GroupName="Parameters")]
//		public Brush CustomColor1
//		{ get; set; }

//		[Browsable(false)]
//		public string CustomColor1Serializable
//		{
//			get { return Serialize.BrushToString(CustomColor1); }
//			set { CustomColor1 = Serialize.StringToBrush(value); }
//		}			

//		[NinjaScriptProperty]
//		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
//		[Display(Name="CustomTime1", Description="CustomTime-1", Order=4, GroupName="Parameters")]
//		public DateTime CustomTime1
//		{ get; set; }		
        #endregion
	}
}
