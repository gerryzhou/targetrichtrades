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
	[Gui.CategoryOrder(GPI_CUSTOM_PARAMS, 1)] //"CustomParams"
<<<<<<< HEAD
//	[Gui.CategoryOrder(GPI_NJSCRIPT_PARAMS, 6)] //"NinjaScriptParameters"	
=======
	
>>>>>>> 7b0fc315bf601b4b69a10d4879094eef2ea42503
	[Gui.CategoryOrder(GPI_GINDICATOR, 2)] // "GIndicator", Indicator general
	[Gui.CategoryOrder(GPI_TIMING, 3)] // "Timming"
	[Gui.CategoryOrder(GPI_MA, 4)] // "MovingAverage"
	[Gui.CategoryOrder(GPI_OUTPUT, 5)] // "Output"
<<<<<<< HEAD
=======
	[Gui.CategoryOrder(GPI_NJSCRIPT_PARAMS, 6)] //"NinjaScriptParameters"
>>>>>>> 7b0fc315bf601b4b69a10d4879094eef2ea42503
	
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
<<<<<<< HEAD
		#endregion


		#region NinjaScriptParameters
//		public const string GPI_NJSCRIPT_PARAMS = "NinjaScriptParameters";
=======
>>>>>>> 7b0fc315bf601b4b69a10d4879094eef2ea42503
		#endregion
		
		#region GIndicator
		public const string GPI_GINDICATOR = "GIndicator";
		public const int OD_SpvPRBits = 1;
		#endregion
		
		#region Timming
		public const string GPI_TIMING = "Timming";
		public const int ODI_OpenStartH = 1;
		public const int ODI_OpenStartM = 2;
		public const int ODI_OpenEndH = 3;
		public const int ODI_OpenEndM = 4;
		#endregion
		
		#region MovingAverage
		public const string GPI_MA = "MovingAverage";
		#endregion
		
		#region Output
		public const string GPI_OUTPUT = "Output";
<<<<<<< HEAD
=======
		#endregion

		#region NinjaScriptParameters
		public const string GPI_NJSCRIPT_PARAMS = "NinjaScriptParameters";
>>>>>>> 7b0fc315bf601b4b69a10d4879094eef2ea42503
		#endregion
		
		private const int IndParamOrder = 0;
	}
}