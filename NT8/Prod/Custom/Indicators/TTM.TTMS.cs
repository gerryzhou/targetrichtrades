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
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.TTM
{
	public class TTMS : Indicator
	{
		private bool alertArmed = false; // sound alert control flag for COBC=false
		
		private Bollinger 		BB;
		private KeltnerChannel	KC;
		private DonchianChannel DC;
		private LinReg			LR;
		private EMA				ema_;
		
		private Series<double> lrm_;
		private Series<double> medianPriceOsc_;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"TTMS momentum histogram with squeeze dots.";
				Name						= "TTMS";
				
				Calculate					= Calculate.OnPriceChange;
				IsOverlay					= false;
				DisplayInDataBox			= true;
				DrawOnPricePanel			= false;
				DrawHorizontalGridLines		= false;
				DrawVerticalGridLines		= false;
				PaintPriceMarkers			= true;
				ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive	= true;
				
				MomentumLength				= 20;
				SqueezeLength				= 20;
				
				SqueezeDotBrush				= Brushes.Red;
				NormalDotBrush				= Brushes.Blue;
				HistAboveZeroRising			= Brushes.Lime;
				HistAboveZeroFalling		= Brushes.DarkSlateGray;
				HistBelowZeroFalling		= Brushes.Red;
				HistBelowZeroRising			= Brushes.DarkSlateGray;
				
				SoundAlertsOn				= false;
				BuySoundAlert				= "Alert3.wav";
				SellSoundAlert				= "Alert4.wav";
				
				AddPlot(new Stroke(Brushes.DarkSlateGray, 5), PlotStyle.Bar, "MomentumHistogram");
				AddPlot(new Stroke(NormalDotBrush, 2), PlotStyle.Dot, "SqueezeDots");
			}
			else if (State == State.Configure)
			{
				lrm_ = new Series<double>(this);
				medianPriceOsc_ = new Series<double>(this);

				BB = Bollinger(2.0, SqueezeLength);
				KC = KeltnerChannel(1.5, SqueezeLength);
				DC = DonchianChannel(MomentumLength);
				LR = LinReg(medianPriceOsc_, MomentumLength);
				ema_ = EMA(MomentumLength);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < MomentumLength) return;
			
			// Squeeze Dots
			SqueezeDots[0] = 0;
			if ((BB.Upper[0] <= KC.Upper[0]) || (BB.Lower[0] >= KC.Lower[0]))
				PlotBrushes[1][0] = SqueezeDotBrush;
			else
				PlotBrushes[1][0] = NormalDotBrush;
			
			// Momentum Histogram
			medianPriceOsc_[0] = Close[0]-((DC.Mean[0] + ema_[0]) / 2.0);
			lrm_[0] = LR[0];
			MomentumHistogram[0] = lrm_[0];
			
			// Histogram Colors
			if (lrm_[0] > 0)
			{
				if (lrm_[0] > lrm_[1])
					 PlotBrushes[0][0] = HistAboveZeroRising;
				else
					PlotBrushes[0][0] = HistAboveZeroFalling;
			}
			else
			{
				if (lrm_[0] < lrm_[1])
					PlotBrushes[0][0] = HistBelowZeroFalling;
				else
					PlotBrushes[0][0] = HistBelowZeroRising;
			}
			
			// Sound Alert
			if (SoundAlertsOn && (State != State.Historical))
			{
				if (IsFirstTickOfBar) alertArmed = true;
					
				if (alertArmed)
				{
					try
					{
						if (((PlotBrushes[1][0] == NormalDotBrush)&&(PlotBrushes[0][0] == HistAboveZeroRising)&&(PlotBrushes[1][1] == SqueezeDotBrush))
						||  ((PlotBrushes[1][0] == NormalDotBrush)&&(PlotBrushes[0][0] == HistAboveZeroRising)&&(PlotBrushes[0][1] != HistAboveZeroRising)))
						{
							PlaySound(string.Format( @"{0}sounds\{1}", NinjaTrader.Core.Globals.InstallDir, BuySoundAlert));
							alertArmed=false; 
						}
						else if (((PlotBrushes[1][0] == NormalDotBrush)&&(PlotBrushes[0][0] == HistBelowZeroFalling)&&(PlotBrushes[1][1] == SqueezeDotBrush))
							 ||  ((PlotBrushes[1][0] == NormalDotBrush)&&(PlotBrushes[0][0] == HistBelowZeroFalling)&&(PlotBrushes[0][1] != HistBelowZeroFalling)))
						{
							PlaySound(string.Format( @"{0}sounds\{1}", NinjaTrader.Core.Globals.InstallDir, SellSoundAlert));
							alertArmed=false; 
						}
					}
					catch(Exception sae){Print("TTMS:OnBarUpdate() Sound Alert Exception Thrown = " + sae.ToString() + "Bar Number = " + CurrentBar); return;}
				}
			}
		}

		#region Properties
		
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="MomentumLength", Description="Momentum Histogram Length", Order=1, GroupName="Parameters")]
		public int MomentumLength
		{ get; set; }

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="SqueezeLength", Description="Volatility Bands Length", Order=2, GroupName="Parameters")]
		public int SqueezeLength
		{ get; set; }

		[XmlIgnore]
		[Display(Name="SqueezeDotColor", Description="Dot Color to indicate a Volatilty Squeeze", Order=1, GroupName="Colors")]
		public Brush SqueezeDotBrush
		{ get; set; }

		[Browsable(false)]
		public string SqueezeDotBrushSerializable
		{
			get { return Serialize.BrushToString(SqueezeDotBrush); }
			set { SqueezeDotBrush = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="NormalDotColor", Description="Normal Volatility Dot Color", Order=2, GroupName="Colors")]
		public Brush NormalDotBrush
		{ get; set; }

		[Browsable(false)]
		public string NormalDotBrushSerializable
		{
			get { return Serialize.BrushToString(NormalDotBrush); }
			set { NormalDotBrush = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="HistAboveZeroRising", Description="Momentum Histogram Above Zero and Rising", Order=3, GroupName="Colors")]
		public Brush HistAboveZeroRising
		{ get; set; }

		[Browsable(false)]
		public string HistAboveZeroRisingSerializable
		{
			get { return Serialize.BrushToString(HistAboveZeroRising); }
			set { HistAboveZeroRising = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="HistAboveZeroFalling", Description="Momentum Histogram Above Zero and Falling", Order=4, GroupName="Colors")]
		public Brush HistAboveZeroFalling
		{ get; set; }

		[Browsable(false)]
		public string HistAboveZeroFallingSerializable
		{
			get { return Serialize.BrushToString(HistAboveZeroFalling); }
			set { HistAboveZeroFalling = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="HistBelowZeroFalling", Description="Momentum Histogram Below Zero and Falling", Order=5, GroupName="Colors")]
		public Brush HistBelowZeroFalling
		{ get; set; }

		[Browsable(false)]
		public string HistBelowZeroFallingSerializable
		{
			get { return Serialize.BrushToString(HistBelowZeroFalling); }
			set { HistBelowZeroFalling = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="HistBelowZeroRising", Description="Momentum Histogram Below Zero and Rising", Order=6, GroupName="Colors")]
		public Brush HistBelowZeroRising
		{ get; set; }

		[Browsable(false)]
		public string HistBelowZeroRisingSerializable
		{
			get { return Serialize.BrushToString(HistBelowZeroRising); }
			set { HistBelowZeroRising = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="SoundAlertsOn", Description="Enables Sound Alerts", Order=1, GroupName="Sound")]
		public bool SoundAlertsOn
		{ get; set; }

		[XmlIgnore]
		[Display(Name="BuySoundAlert", Description="Buy Sound Alert .wav filename", Order=2, GroupName="Sound")]
		public string BuySoundAlert
		{ get; set; }

		[XmlIgnore]
		[Display(Name="SellSoundAlert", Description="Sell Sound Alert .wav filename", Order=3, GroupName="Sound")]
		public string SellSoundAlert
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MomentumHistogram
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SqueezeDots
		{
			get { return Values[1]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TTM.TTMS[] cacheTTMS;
		public TTM.TTMS TTMS(int momentumLength, int squeezeLength)
		{
			return TTMS(Input, momentumLength, squeezeLength);
		}

		public TTM.TTMS TTMS(ISeries<double> input, int momentumLength, int squeezeLength)
		{
			if (cacheTTMS != null)
				for (int idx = 0; idx < cacheTTMS.Length; idx++)
					if (cacheTTMS[idx] != null && cacheTTMS[idx].MomentumLength == momentumLength && cacheTTMS[idx].SqueezeLength == squeezeLength && cacheTTMS[idx].EqualsInput(input))
						return cacheTTMS[idx];
			return CacheIndicator<TTM.TTMS>(new TTM.TTMS(){ MomentumLength = momentumLength, SqueezeLength = squeezeLength }, input, ref cacheTTMS);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TTM.TTMS TTMS(int momentumLength, int squeezeLength)
		{
			return indicator.TTMS(Input, momentumLength, squeezeLength);
		}

		public Indicators.TTM.TTMS TTMS(ISeries<double> input , int momentumLength, int squeezeLength)
		{
			return indicator.TTMS(input, momentumLength, squeezeLength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TTM.TTMS TTMS(int momentumLength, int squeezeLength)
		{
			return indicator.TTMS(Input, momentumLength, squeezeLength);
		}

		public Indicators.TTM.TTMS TTMS(ISeries<double> input , int momentumLength, int squeezeLength)
		{
			return indicator.TTMS(input, momentumLength, squeezeLength);
		}
	}
}

#endregion
