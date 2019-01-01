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
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// It defined a set of interfaces to talk with strategy;
	/// The derived indicators should override the methods to be able be combined with other indicators; 
	/// </summary>
	public partial class Indicator
	{
		private Series<double> CustomDataSeries1;
		
		private IndicatorSignal indSignal;
		
		public Volatility GetVolatility() {return null;}
		
		public MarketCycle GetMarketCycle() {return null;}
		
		public Direction GetDirection() {return null;}
		
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
//        protected void Initialize()
//        {
            //Add(new Plot(Color.FromKnownColor(KnownColor.Black), PlotStyle.Block, "StartHM"));
            //Add(new Plot(Color.FromKnownColor(KnownColor.DarkBlue), PlotStyle.Block, "EndHM"));
            //Overlay				= true;
//			accName = "";//GetTsTAccName(Strategy.Account.Name);
//			IndicatorSignal indicatorSignal = new IndicatorSignal();
//        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
//        protected void OnBarUpdate()
//        {
            // Use this method for calculating your indicator values. Assign a value to each
            // plot below by replacing 'Close[0]' with your own formula.
//			if(CurrentBar >= BarsRequired) {
//				if(GetTimeDiffByHM(StartH, StartM, Time[1]) > 0 && GetTimeDiffByHM(StartH, StartM, Time[0]) <= 0)
//            		StartHM.Set(High[0]+0.25);
//				if(GetTimeDiffByHM(EndH, EndM, Time[1]) > 0 && GetTimeDiffByHM(EndH, EndM, Time[0]) <= 0)
//            		EndHM.Set(Low[0]-0.25);
//			}
//        }
		
		public IndicatorSignal CheckIndicatorSignal() {
			return null;
		}
		
		#region Methods
		public string GetAccName() {
			return accName;
		}
		
		public void AddIndicator(Indicator i) {
			this.listIndicator.Add(i);
		}

		#endregion		
		
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
		[Range(0, double.MaxValue)]
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



