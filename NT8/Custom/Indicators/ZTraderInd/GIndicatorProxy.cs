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
using System.Reflection;
using System.IO;
using System.Web.Script.Serialization;

using log4net;
using log4net.Config;

using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators.ZTraderPattern;
using NinjaTrader.NinjaScript.Indicators.PriceActions;
using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.NinjaScript.AddOns;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.ZTraderInd
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
				CustInput					= 1;
				AddPlot(Brushes.Orange, "CustPlot");
			}
			else if (State == State.Configure)
			{
				GetConfigItem("", "");
				string config_file = GZLogger.GetConfigFilePath(GetConfigFileDir());
				Print(this.Name + ":GetConfigFilePath=" + config_file);
				XmlConfigurator.Configure(new FileInfo(@config_file));////"C:\\www\\log\\log4net.config"));
				//GZLogger.ConfigureFileAppender( "C:\\www\\log\\log_test.txt" );
			}
			else if (State == State.DataLoaded)
			{				
				CustData = new Series<double>(this);				
				SetLogFile(GetFileNameByDateTime(DateTime.Now, @"C:\www\log\", GSZTrader.AccName, GetSymbol(), "log"));
				BackTest = GSZTrader.BackTest;
				dailyPattern = LoadSpvPRList(SpvDailyPatternES.spvPRDayES);
			}
		}

		protected override void OnConnectionStatusUpdate(ConnectionStatusEventArgs connectionStatusUpdate)
		{
			
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			PrintTo = PrintTo.OutputTab2;
			if(IsLastBarOnChart() > 0)
				PrintLog(true, false, "dailyPattern=" + DailyPattern.Count);
		}
		
		public string GetConfigFileDir() {
			string ud_dir = NinjaTrader.Core.Globals.UserDataDir
				+ "bin" + Path.DirectorySeparatorChar
				+ "Custom" + Path.DirectorySeparatorChar;
			Print(this.Name + ":NinjaTrader.Core.Globals.UserDataDir=" + NinjaTrader.Core.Globals.UserDataDir);
			string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			Print(this.Name + ":Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)=" + currentDirectory);
			string appPath = System.AppDomain.CurrentDomain.BaseDirectory;
			Print(this.Name + ":System.AppDomain.CurrentDomain.BaseDirectory=" + appPath);
			string entryPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			Print(this.Name + ":System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)=" + entryPath);
			string curPath = System.Environment.CurrentDirectory;
			Print(this.Name + ":System.Environment.CurrentDirectory=" + curPath);
			return ud_dir; //Directory.GetParent(currentDirectory).FullName;
		}
		
		public string GetConfigItem(string config_file, string item_name) {
			string json_path = GetConfigFileDir() + "ztrader.json";
			string json = System.IO.File.ReadAllText(json_path);
            //DataContractJsonSerializer ser = new DataContractJsonSerializer();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> dic = serializer.Deserialize<Dictionary<string, object>>(json);
            //var jsonObject = JsonValue.Parse(json);
			foreach(KeyValuePair<string, object> ele1 in dic) 
          { 
              Print(string.Format("JSON={0} and {1}", ele1.Key, ele1.Value));
          }
			
            string item = null;
			return item;
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="GSZTrader", Description="GSZTrader instance for indicator proxy")]
		public GStrategyBase GSZTrader
		{ get; set; }
		
		//[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		//[Display(Name="CustInput", Description="CustInput for indicator proxy", Order=0, GroupName="Parameters")]
		[Browsable(false)]
		[XmlIgnore]
		public int CustInput
		{ get; set; }

		//[NinjaScriptProperty]
		//[Range(1, int.MaxValue)]
		//[Display(Name="AccName", Description="Account Name for strategy", Order=1, GroupName="Parameters")]
//		[Browsable(false)]
//		[XmlIgnore]
//		public string AccName
//		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public Dictionary<string, List<MarketContext>> DailyPattern
		{
			get { return dailyPattern; }
			set { dailyPattern = value;}
		}
		
		[Browsable(false)]
		[XmlIgnore]
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
		private ZTraderInd.GIndicatorProxy[] cacheGIndicatorProxy;
		public ZTraderInd.GIndicatorProxy GIndicatorProxy(GStrategyBase gSZTrader)
		{
			return GIndicatorProxy(Input, gSZTrader);
		}

		public ZTraderInd.GIndicatorProxy GIndicatorProxy(ISeries<double> input, GStrategyBase gSZTrader)
		{
			if (cacheGIndicatorProxy != null)
				for (int idx = 0; idx < cacheGIndicatorProxy.Length; idx++)
					if (cacheGIndicatorProxy[idx] != null && cacheGIndicatorProxy[idx].GSZTrader == gSZTrader && cacheGIndicatorProxy[idx].EqualsInput(input))
						return cacheGIndicatorProxy[idx];
			return CacheIndicator<ZTraderInd.GIndicatorProxy>(new ZTraderInd.GIndicatorProxy(){ GSZTrader = gSZTrader }, input, ref cacheGIndicatorProxy);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ZTraderInd.GIndicatorProxy GIndicatorProxy(GStrategyBase gSZTrader)
		{
			return indicator.GIndicatorProxy(Input, gSZTrader);
		}

		public Indicators.ZTraderInd.GIndicatorProxy GIndicatorProxy(ISeries<double> input , GStrategyBase gSZTrader)
		{
			return indicator.GIndicatorProxy(input, gSZTrader);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ZTraderInd.GIndicatorProxy GIndicatorProxy(GStrategyBase gSZTrader)
		{
			return indicator.GIndicatorProxy(Input, gSZTrader);
		}

		public Indicators.ZTraderInd.GIndicatorProxy GIndicatorProxy(ISeries<double> input , GStrategyBase gSZTrader)
		{
			return indicator.GIndicatorProxy(input, gSZTrader);
		}
	}
}

#endregion
