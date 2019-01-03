//
// Copyright (C) 2018, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
#endregion

//This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// </summary>
	public class AwesomeOscillator : Indicator
	{
        private int fastPeriod 			= 5;
        private int slowPeriod 			= 34;
		private int smooth		 		= 5;
		private int barWidth			= 5;
		private int lineWidth			= 2;
		private double oscillatorValue 	= 0;
		private bool isRising			= false;
		private SolidColorBrush upColor 		= Brushes.LimeGreen;
		private SolidColorBrush downColor 		= Brushes.Red;
		private SolidColorBrush signalColor		= Brushes.LightSteelBlue;
		private bool showLines			= false;
		private SMA fastSMA;
		private SMA slowSMA;
		private EMA fastEMA;
		private EMA slowEMA;
		private TMA fastTMA;
		private TMA slowTMA;
		private MovingAvgType movAvgType = MovingAvgType.SMA;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Print("AwesomeOscillator set defaults called....");
				//AddPlot(Brushes.DodgerBlue,		NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameUltimateOscillator);
				AddPlot(new Stroke(Brushes.Gray,2), PlotStyle.Bar, "OscillatorLine");
				AddPlot(new Stroke(SignalColor,2), PlotStyle.Bar, "SignalLine");
				AddPlot(new Stroke(Brushes.Gray,6), PlotStyle.Bar, "Awesome Oscillator");
				AddLine(Brushes.DarkGray, 0, "Zero line");
				//AddLine(Brushes.DarkGray,	70,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorOverbought);
				Calculate = Calculate.OnBarClose;
			}
			else if (State == State.Configure)
			{
	            //CalculateOnBarClose	= false;
	            //Overlay				= false;
				//PlotsConfigurable	= false;
			}
			else if (State == State.DataLoaded)
			{
//			Plots[1].Pen.Color = signalColor;
//			Plots[0].Pen.Width = lineWidth;
//			Plots[1].Pen.Width = lineWidth;
//			Plots[2].Pen.Width = barWidth;
				switch(movAvgType) {
					case MovingAvgType.SMA:
						fastSMA = SMA(Median, fastPeriod);
						slowSMA = SMA(Median, slowPeriod);
						break;
					case MovingAvgType.EMA:
						fastEMA = EMA(Median, fastPeriod);
						slowEMA = EMA(Median, slowPeriod);
						break;
					case MovingAvgType.TMA:
						fastTMA = TMA(Median, fastPeriod);
						slowTMA = TMA(Median, slowPeriod);
						break;
				}
			}
		}

		protected override void OnBarUpdate()
		{
			Print(CurrentBar.ToString() + " -- AwesomeOscillator OnBarUpdate called");
	        if (CurrentBar < 1)
			{
				OscillatorLine[0] = 0;
				SignalLine[0] = 0;
				Oscillator[0] = 0;	
				return;
			}
			
			oscillatorValue = GetOscillatorValue();//fastSMA[0]-slowSMA[0];
			if (ShowLines)
			{
				OscillatorLine[0] = oscillatorValue;
				SignalLine[0] = GetSignalLine();//SMA(OscillatorLine,Smooth)[0];
			}
			else
			{
				OscillatorLine.Reset();
				SignalLine.Reset();
			}
			Oscillator[0] = oscillatorValue;
			Brush draw_color = new SolidColorBrush(Color.FromRgb(0, 0, 0));//Color.Black;
			if(isRising && !IsRising(Oscillator)) {
				Draw.Text(this, CurrentBar.ToString(), "O", 1, High[1]+10, draw_color);
			} 
			if(!isRising && IsRising(Oscillator)) {
				Draw.Text(this, CurrentBar.ToString(), "B", 1, Low[1]-10, draw_color);
				//Draw.Text(this, CurrentBar.ToString(), Time[0]+"\r\nB", 1, Low[0]-10, draw_color);
			}
			
			isRising = IsRising(Oscillator);				
			if (isRising)
			{
				PlotBrushes[0][0] = UpColor;
				PlotBrushes[2][0] = UpColor;
				//Draw.Text(this, tag+barNo.ToString(), GetTimeDate(Time[bars_ago], 1)+"\r\n#"+barNo+"\r\nZ:"+zzGap, bars_ago, y, draw_color)
				
//				PlotColors[0][0] = UpColor;
//				PlotColors[2][0] = UpColor;
			}
			else
			{
				PlotBrushes[0][0] = DownColor;
				PlotBrushes[2][0] = DownColor;
			}
		}
		
		private double GetOscillatorValue() {
			double val = 0;
			switch(movAvgType) {
				case MovingAvgType.SMA:
					val = fastSMA[0]-slowSMA[0];
					break;
				case MovingAvgType.EMA:
					val = fastEMA[0]-slowEMA[0];
					break;
				case MovingAvgType.TMA:
					val = fastTMA[0]-slowTMA[0];
					break;
			}			
			return val;
		}
		
		private double GetSignalLine() {
			double val = 0;
			switch(movAvgType) {
				case MovingAvgType.SMA:
					val = SMA(OscillatorLine,Smooth)[0];
					break;
				case MovingAvgType.EMA:
					val = EMA(OscillatorLine,Smooth)[0];
					break;
				case MovingAvgType.TMA:
					val = TMA(OscillatorLine,Smooth)[0];
					break;
			}
			return val;
		}

        #region Properties
       
		/// <summary>
		/// </summary>
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> OscillatorLine
		{
			get { return Values[0]; }
		}		/// <summary>
		
		/// <summary>
		/// </summary>
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SignalLine
		{
			get { return Values[1]; }
		}
		
		/// <summary>
		/// </summary>
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Oscillator
		{
			get { return Values[2]; }
		}
		
		/// <summary>
		/// </summary>
		//[Description("Show Oscillator and Signalline")]
		//[Gui.Design.DisplayName("Display Lines")]
		//[Category("Parameters")]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ShowLines", Description = "Set true to display on chart the lines", GroupName = "NinjaScriptGeneral", Order = 5)]
		public bool ShowLines
		{
			get { return showLines; }
			set { showLines = value; }
		}

		/// <summary>
		/// </summary>
		//[Description("Period for fast EMA")]
		//[Category("Parameters")]
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "FastPeriod", GroupName = "AOParameters", Order = 2)]		
		public int FastPeriod
		{
			//get;set;
			get { return fastPeriod; }
			set { fastPeriod = Math.Max(1, value); }
		}

		/// <summary>
		/// </summary>
		//[Description("Period for slow EMA")]
		//[Category("Parameters")]
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SlowPeriod", GroupName = "AOParameters", Order = 3)]
		public int SlowPeriod
		{
			get { return slowPeriod; }
			set { slowPeriod = Math.Max(1, value); }
		}

		/// <summary>
		/// </summary>
//		[Description("Period for Smoothing of Signal Line")]
//		[Category("Parameters")]
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Smooth", GroupName = "AOParameters", Order = 4)]		
		public int Smooth
		{
			get { return smooth; }
			set { smooth = Math.Max(1, value); }
		}

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MovingAvgType", GroupName = "AOParameters", Order = 1)]		
		public MovingAvgType MovingAverageType
		{
			get { return movAvgType; }
			set { movAvgType = value; }
		}
		
		/// <summary>
		/// </summary>
		[Description("Width of bars")]
		//[Gui.Design.DisplayName("Bar Width")]
		[Category("Plots")]
		public int BarWidth
		{
			get { return barWidth; }
			set { barWidth = Math.Max(1, value); }
		}

		/// <summary>
		/// </summary>
		[Description("Width of plotted lines")]
		//[Gui.Design.DisplayName("Line Width")]
		[Category("Plots")]
		public int LineWidth
		{
			get { return lineWidth; }
			set { lineWidth = Math.Max(1, value); }
		}

		/// <summary>
		/// </summary>
        [XmlIgnore()]		
		[Description("Select Color")]
		[Category("Plots")]
		//[Gui.Design.DisplayName("Oscillator Rising")]
		public SolidColorBrush UpColor
		{
			get { return upColor; }
			set { upColor = value; }
		}
		
		// Serialize Color object
//		[Browsable(false)]
//		public string UpColorSerialize
//		{
//			get { return NinjaTrader.Gui.Design.SerializableColor.ToString(upColor); }
//			set { upColor = NinjaTrader.Gui.Design.SerializableColor.FromString(value); }
//		}

		/// <summary>
		/// </summary>
        [XmlIgnore()]		
		[Description("Select Color")]
		[Category("Plots")]
		//[Gui.Design.DisplayName("Oscillator Falling")]
		public SolidColorBrush DownColor
		{
			get { return downColor; }
			set { downColor = value; }
		}
		
		// Serialize Color object
//		[Browsable(false)]
//		public string DownColorSerialize
//		{
//			get { return NinjaTrader.Gui.Design.SerializableColor.ToString(downColor); }
//			set { downColor = NinjaTrader.Gui.Design.SerializableColor.FromString(value); }
//		}

		/// <summary>
		/// </summary>
		[XmlIgnore()]		
		[Description("Select Color")]
		[Category("Plots")]
		//[Gui.Design.DisplayName("Signalline")]
		public SolidColorBrush SignalColor
		{
			get { return signalColor; }
			set { signalColor = value; }
		}
		
		// Serialize Color object
//		[Browsable(false)]
//		public string SignalColorSerialize
//		{
//			get { return NinjaTrader.Gui.Serialize.BrushToString(new SolidColorBrush(signalColor));}//.Design.SerializableColor.ToString(signalColor); }
//			set { signalColor = NinjaTrader.Gui.Serialize.StringToBrush(value);}//.Design.SerializableColor.FromString(value); }
//		}		
		#endregion
		
//		#region Properties
//		[Range(1, int.MaxValue), NinjaScriptProperty]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast", GroupName = "NinjaScriptParameters", Order = 0)]
//		public int Fast
//		{ get; set; }

//		[Range(1, int.MaxValue), NinjaScriptProperty]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Intermediate", GroupName = "NinjaScriptParameters", Order = 1)]
//		public int Intermediate
//		{ get; set; }

//		[Range(1, int.MaxValue), NinjaScriptProperty]
//		[Display(ResourceType = typeof(Custom.Resource), Name = "Slow", GroupName = "NinjaScriptParameters", Order = 2)]
//		public int Slow
//		{ get; set; }
//		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AwesomeOscillator[] cacheAwesomeOscillator;
		public AwesomeOscillator AwesomeOscillator(int fastPeriod, int slowPeriod, int smooth, MovingAvgType movingAverageType)
		{
			return AwesomeOscillator(Input, fastPeriod, slowPeriod, smooth, movingAverageType);
		}

		public AwesomeOscillator AwesomeOscillator(ISeries<double> input, int fastPeriod, int slowPeriod, int smooth, MovingAvgType movingAverageType)
		{
			if (cacheAwesomeOscillator != null)
				for (int idx = 0; idx < cacheAwesomeOscillator.Length; idx++)
					if (cacheAwesomeOscillator[idx] != null && cacheAwesomeOscillator[idx].FastPeriod == fastPeriod && cacheAwesomeOscillator[idx].SlowPeriod == slowPeriod && cacheAwesomeOscillator[idx].Smooth == smooth && cacheAwesomeOscillator[idx].MovingAverageType == movingAverageType && cacheAwesomeOscillator[idx].EqualsInput(input))
						return cacheAwesomeOscillator[idx];
			return CacheIndicator<AwesomeOscillator>(new AwesomeOscillator(){ FastPeriod = fastPeriod, SlowPeriod = slowPeriod, Smooth = smooth, MovingAverageType = movingAverageType }, input, ref cacheAwesomeOscillator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AwesomeOscillator AwesomeOscillator(int fastPeriod, int slowPeriod, int smooth, MovingAvgType movingAverageType)
		{
			return indicator.AwesomeOscillator(Input, fastPeriod, slowPeriod, smooth, movingAverageType);
		}

		public Indicators.AwesomeOscillator AwesomeOscillator(ISeries<double> input , int fastPeriod, int slowPeriod, int smooth, MovingAvgType movingAverageType)
		{
			return indicator.AwesomeOscillator(input, fastPeriod, slowPeriod, smooth, movingAverageType);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AwesomeOscillator AwesomeOscillator(int fastPeriod, int slowPeriod, int smooth, MovingAvgType movingAverageType)
		{
			return indicator.AwesomeOscillator(Input, fastPeriod, slowPeriod, smooth, movingAverageType);
		}

		public Indicators.AwesomeOscillator AwesomeOscillator(ISeries<double> input , int fastPeriod, int slowPeriod, int smooth, MovingAvgType movingAverageType)
		{
			return indicator.AwesomeOscillator(input, fastPeriod, slowPeriod, smooth, movingAverageType);
		}
	}
}

#endregion
