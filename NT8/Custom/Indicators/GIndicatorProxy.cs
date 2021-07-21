#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using System.IO;
using log4net.Config;

using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Addons.Patterns;
using NinjaTrader.NinjaScript.AddOns.PriceActions;
using NinjaTrader.NinjaScript.AddOns.MarketCtx;
using NinjaTrader.NinjaScript.AddOns.MarketCtx;
using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.NinjaScript.AddOns;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
    public class GIndicatorProxy : GIndicatorBase
	{		
		private Series<double> CustData;
		//private List<SpvPR> dailyPattern;
		private Dictionary<string, List<MarketContext>> dailyPattern;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Common indicator to transfer indicator info to strategy;";
				Name										= "GIndicatorProxy";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				MaximumBarsLookBack							= MaximumBarsLookBack.Infinite;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				CustInput									= 1;
				//AddPlot(Brushes.Orange, "CustPlot");
			}
			else if (State == State.Configure)
			{
				/*if(!GSZTrader.IsInStrategyAnalyzer) {
					string cmdPathRoot = GConfig.GetConfigItem(GConfig.MainConfigFile, "CmdPathRoot");
					string log_config_file = GConfig.GetLogConfigFilePath();
					Print(String.Format("{0}:GLogger.GetLogConfigFilePath={1}, CmdPathRoot={2}",
					this.Name, log_config_file, cmdPathRoot));
					XmlConfigurator.Configure(new FileInfo(@log_config_file));////"C:\\www\\log\\log4net.config"));
					//GZLogger.ConfigureFileAppender( "C:\\www\\log\\log_test.txt" );
					GLogger.Initialize(GConfig.GetLogDir());
				}*/
			}
			else if (State == State.DataLoaded)
			{				
//				CustData = new Series<double>(this);				
				//SetLogFile(GetFileNameByDateTime(DateTime.Now, @"C:\www\log\", GSZTrader.AccName, GetSymbol(), "log"));
				//BackTest = GSZTrader.BackTest;
//				dailyPattern = LoadSpvPRList(SpvDailyPatternES.spvPRDayES);
			}
		}

		protected override void OnConnectionStatusUpdate(ConnectionStatusEventArgs connectionStatusUpdate)
		{
			
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			PrintTo = PrintTo.OutputTab2;
//			if(IsLastBarOnChart() > 0)
//				PrintLog(true, false, "dailyPattern=" + DailyPattern.Count);
		}
			
		#region Properties
		[NinjaScriptProperty]
		[Display(Name="GSZTrader", Description="GSZTrader instance for indicator proxy")]
		public GStrategyBase GSZTrader
		{ get; set; }
		
		//[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		//[Display(Name="CustInput", Description="CustInput for indicator proxy", Order=0, GroupName="Parameters")]
		[Browsable(false), XmlIgnore]
		public int CustInput
		{ get; set; }
		
		[Browsable(false), XmlIgnore]
		public Dictionary<string, List<MarketContext>> DailyPattern
		{
			get { return dailyPattern; }
			set { dailyPattern = value;}
		}
		
		[Browsable(false), XmlIgnore]
		public Series<double> CustPlot
		{
			get { return Values[0]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIndicatorProxy[] cacheGIndicatorProxy;
		public GIndicatorProxy GIndicatorProxy(GStrategyBase gSZTrader)
		{
			return GIndicatorProxy(Input, gSZTrader);
		}

		public GIndicatorProxy GIndicatorProxy(ISeries<double> input, GStrategyBase gSZTrader)
		{
			if (cacheGIndicatorProxy != null)
				for (int idx = 0; idx < cacheGIndicatorProxy.Length; idx++)
					if (cacheGIndicatorProxy[idx] != null && cacheGIndicatorProxy[idx].GSZTrader == gSZTrader && cacheGIndicatorProxy[idx].EqualsInput(input))
						return cacheGIndicatorProxy[idx];
			return CacheIndicator<GIndicatorProxy>(new GIndicatorProxy(){ GSZTrader = gSZTrader }, input, ref cacheGIndicatorProxy);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIndicatorProxy GIndicatorProxy(GStrategyBase gSZTrader)
		{
			return indicator.GIndicatorProxy(Input, gSZTrader);
		}

		public Indicators.GIndicatorProxy GIndicatorProxy(ISeries<double> input , GStrategyBase gSZTrader)
		{
			return indicator.GIndicatorProxy(input, gSZTrader);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIndicatorProxy GIndicatorProxy(GStrategyBase gSZTrader)
		{
			return indicator.GIndicatorProxy(Input, gSZTrader);
		}

		public Indicators.GIndicatorProxy GIndicatorProxy(ISeries<double> input , GStrategyBase gSZTrader)
		{
			return indicator.GIndicatorProxy(input, gSZTrader);
		}
	}
}

#endregion
