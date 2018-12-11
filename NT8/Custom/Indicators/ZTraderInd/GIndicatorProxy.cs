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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.ZTraderInd
{
	public class GIndicatorProxy : GIndicatorBase
	{
		private Series<double> CustomDataSeries1;

		public SupportResistance GetSupport(){return null;}
		public SupportResistance GetResistance(){return null;}

		#region Variables
        // User defined variables (add any user defined variables below)
            private int startH = 9; // Default setting for StartH
            private int startM = 5; // Default setting for StartM
            private int endH = 11; // Default setting for EndH
            private int endM = 5; // Default setting for EndM
			private string accName = ""; //account name from strategy, extracting simply string for print/log;
			
		#endregion

		private List<Indicator> listIndicator = new List<Indicator>();

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        protected void Initialize()
        {
            //Add(new Plot(Color.FromKnownColor(KnownColor.Black), PlotStyle.Block, "StartHM"));
            //Add(new Plot(Color.FromKnownColor(KnownColor.DarkBlue), PlotStyle.Block, "EndHM"));
            //Overlay				= true;
			accName = "";//GetTsTAccName(Strategy.Account.Name);
			IndicatorSignal indicatorSignal = new IndicatorSignal();
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected void OnBarUpdate()
        {
            // Use this method for calculating your indicator values. Assign a value to each
            // plot below by replacing 'Close[0]' with your own formula.
//			if(CurrentBar >= BarsRequired) {
//				if(GetTimeDiffByHM(StartH, StartM, Time[1]) > 0 && GetTimeDiffByHM(StartH, StartM, Time[0]) <= 0)
//            		StartHM.Set(High[0]+0.25);
//				if(GetTimeDiffByHM(EndH, EndM, Time[1]) > 0 && GetTimeDiffByHM(EndH, EndM, Time[0]) <= 0)
//            		EndHM.Set(Low[0]-0.25);
//			}
        }
		
		public IndicatorSignal CheckIndicatorSignal() {
			return null;
		}
		
        #region Properties
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> StartHM
        {
            get { return Values[0]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> EndHM
        {
            get { return Values[1]; }
        }

        [Description("Hour of start trading")]
        //[GridCategory("Parameters")]
        public int StartH
        {
            get { return startH; }
            set { startH = Math.Max(0, value); }
        }

        [Description("Min of start trading")]
        //[GridCategory("Parameters")]
        public int StartM
        {
            get { return startM; }
            set { startM = Math.Max(0, value); }
        }

        [Description("Hour of end trading")]
        //[GridCategory("Parameters")]
        public int EndH
        {
            get { return endH; }
            set { endH = Math.Max(0, value); }
        }

        [Description("Min of end trading")]
        //[GridCategory("Parameters")]
        public int EndM
        {
            get { return endM; }
            set { endM = Math.Max(0, value); }
        }
        #endregion
		
		#region Methods
		public string GetAccName() {
			return accName;
		}
		
		public void AddIndicator(Indicator i) {
			this.listIndicator.Add(i);
		}

		#endregion
		
		#region Properties
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="CustomColor1", Description="Color-1", Order=1, GroupName="Parameters")]
		public Brush CustomColor1
		{ get; set; }

		[Browsable(false)]
		public string CustomColor1Serializable
		{
			get { return Serialize.BrushToString(CustomColor1); }
			set { CustomColor1 = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="CustomPrc1", Description="CustomPrc-1", Order=2, GroupName="Parameters")]
		public double CustomPrc1
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="CustomStr1", Description="CustomStr-1", Order=3, GroupName="Parameters")]
		public string CustomStr1
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="CustomTime1", Description="CustomTime-1", Order=4, GroupName="Parameters")]
		public DateTime CustomTime1
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ZTraderInd.GIndicatorProxy[] cacheGIndicatorProxy;
		public ZTraderInd.GIndicatorProxy GIndicatorProxy(Brush customColor1, double customPrc1, string customStr1, DateTime customTime1)
		{
			return GIndicatorProxy(Input, customColor1, customPrc1, customStr1, customTime1);
		}

		public ZTraderInd.GIndicatorProxy GIndicatorProxy(ISeries<double> input, Brush customColor1, double customPrc1, string customStr1, DateTime customTime1)
		{
			if (cacheGIndicatorProxy != null)
				for (int idx = 0; idx < cacheGIndicatorProxy.Length; idx++)
					if (cacheGIndicatorProxy[idx] != null && cacheGIndicatorProxy[idx].CustomColor1 == customColor1 && cacheGIndicatorProxy[idx].CustomPrc1 == customPrc1 && cacheGIndicatorProxy[idx].CustomStr1 == customStr1 && cacheGIndicatorProxy[idx].CustomTime1 == customTime1 && cacheGIndicatorProxy[idx].EqualsInput(input))
						return cacheGIndicatorProxy[idx];
			return CacheIndicator<ZTraderInd.GIndicatorProxy>(new ZTraderInd.GIndicatorProxy(){ CustomColor1 = customColor1, CustomPrc1 = customPrc1, CustomStr1 = customStr1, CustomTime1 = customTime1 }, input, ref cacheGIndicatorProxy);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ZTraderInd.GIndicatorProxy GIndicatorProxy(Brush customColor1, double customPrc1, string customStr1, DateTime customTime1)
		{
			return indicator.GIndicatorProxy(Input, customColor1, customPrc1, customStr1, customTime1);
		}

		public Indicators.ZTraderInd.GIndicatorProxy GIndicatorProxy(ISeries<double> input , Brush customColor1, double customPrc1, string customStr1, DateTime customTime1)
		{
			return indicator.GIndicatorProxy(input, customColor1, customPrc1, customStr1, customTime1);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ZTraderInd.GIndicatorProxy GIndicatorProxy(Brush customColor1, double customPrc1, string customStr1, DateTime customTime1)
		{
			return indicator.GIndicatorProxy(Input, customColor1, customPrc1, customStr1, customTime1);
		}

		public Indicators.ZTraderInd.GIndicatorProxy GIndicatorProxy(ISeries<double> input , Brush customColor1, double customPrc1, string customStr1, DateTime customTime1)
		{
			return indicator.GIndicatorProxy(input, customColor1, customPrc1, customStr1, customTime1);
		}
	}
}

#endregion
