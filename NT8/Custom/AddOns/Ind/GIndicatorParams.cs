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
	[Gui.CategoryOrder(GPI_CUSTOM_PARAMS, 1)] //"CustomParams"
//	[Gui.CategoryOrder(GPI_NJSCRIPT_PARAMS, 1)] //"NinjaScriptParameters"
	[Gui.CategoryOrder(GPI_GINDICATOR, 2)] // "GIndicator", Indicator general
	[Gui.CategoryOrder(GPI_TIMING, 3)] // "Timming"
	[Gui.CategoryOrder(GPI_MA, 4)] // "MovingAverage"
	[Gui.CategoryOrder(GPI_OUTPUT, 5)] // "Output"
	
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
		public const string GPI_CUSTOM_PARAMS = "CustomParams";
		#endregion

		#region NinjaScriptParameters
//		public const string GPI_CUSTOM_PARAMS = "Parameters";//"NinjaScriptParameters";
		#endregion
		
		#region GIndicator
		public const string GPI_GINDICATOR = "GIndicator";
		public const int ODI_SpvPRBits = 1;
		public const int ODI_PrintOut = 2;
		#endregion
		
		#region Timming
		public const string GPI_TIMING = "Timming";
		public const int ODI_OpenStartH = 1;
		public const int ODI_OpenStartM = 2;
		public const int ODI_OpenEndH = 3;
		public const int ODI_OpenEndM = 4;
		public const int ODI_ClosingH = 5;
		public const int ODI_ClosingM = 6;
		#endregion
		
		#region MovingAverage
		public const string GPI_MA = "MovingAverage";
		#endregion
		
		#region Output
		public const string GPI_OUTPUT = "Output";
		#endregion
		
		private const int IndParamOrder = 0;
	}
}