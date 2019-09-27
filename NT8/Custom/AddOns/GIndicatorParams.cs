#region Using declarations
using System;
using System.IO;
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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Reflection;
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
using NinjaTrader.NinjaScript.AddOns;
#endregion

//[assembly: log4net.Config.XmlConfigurator(ConfigFile = "C:\\www\\log\\log4net.config", Watch = true)]

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Display "CP" first, the entry defined in individual indicators
	/// </summary>
	[Gui.CategoryOrder("CustomParams", 1)]
	
	[Gui.CategoryOrder("GIndicator", 2)] // Indicator general
	[Gui.CategoryOrder("Timming", 3)] //
	[Gui.CategoryOrder("MA", 4)] // "Moving Average"
	[Gui.CategoryOrder("OSI", 5)] // ""
	
	/// <summary>
	/// The class to manage parameters for indicator
	/// Including grouping, ordering, and naming;
	/// </summary>
	public partial class GIndicatorBase : Indicator
	{	
		
		#region CustomParams
		/// <summary>
		/// The group for individual indicators parameters
		/// The order for each entry is defined in the class of individual indicator 
		/// </summary>
		public const string GP_CUSTOM_PARAMS = "CustomParams";
		#endregion
		
		#region GIndicator
		public const string GP_GINDICATOR = "GIndicator";
		public const int OD_SpvPRBits = 1;
		#endregion
		
		#region Timming
		public const string GP_TIMING = "Timming";
		public const int OD_OpenStartH = 1;
		public const int OD_OpenStartM = 2;
		public const int OD_OpenEndH = 3;
		public const int OD_OpenEndM = 4;
		#endregion
		
		#region MovingAverage
		public const string GP_MA = "MovingAverage";
		#endregion
		
		#region Output
		public const string GP_OUTPUT = "Output";
		#endregion
		
		private const int IndParamOrder = 0;
	}
}
		