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

#endregion

//This namespace holds Share adapters in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.ShareServices
{
	public class GZTraderShareService : ShareService
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Share Service here.";
				Name										= "GZTraderShareService";
				GA_StartTime						= DateTime.Parse("20:21", System.Globalization.CultureInfo.InvariantCulture);
				GA_EndTime						= DateTime.Parse("20:22", System.Globalization.CultureInfo.InvariantCulture);
				GA_ErrorMsg					= @"Error message";
			}
			else if (State == State.Configure)
			{
			}
		}

		public override async Task OnShare(string text, string imgFilePath)
		{
			// place your share service logic here
		}

		public override async Task OnAuthorizeAccount()
		{
			// place any authorization logic needed here
		}

		#region Properties
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="GA_StartTime", Description="StartTime of the trade", Order=1, GroupName="Parameters")]
		public DateTime GA_StartTime
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="GA_EndTime", Description="End time of the trade", Order=2, GroupName="Parameters")]
		public DateTime GA_EndTime
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="GA_ErrorMsg", Description="Show up error message", Order=3, GroupName="Parameters")]
		public string GA_ErrorMsg
		{ get; set; }
		#endregion

	}
}
