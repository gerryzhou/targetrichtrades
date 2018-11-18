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
using NinjaTrader.Gui.Tools;
#endregion

//This namespace holds Add ons in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.AddOns
{
	public class GZTraderAddOn : NinjaTrader.NinjaScript.AddOnBase
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Add on here.";
				Name										= "GZTraderAddOn";
				GA_ProfitTarget					= 1;
				GA_StopLoss					= 1;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnWindowCreated(Window window)
		{
			
		}

		protected override void OnWindowDestroyed(Window window)
		{
			
		}


		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="GA_ProfitTarget", Description="ProfitTarget", Order=1, GroupName="Parameters")]
		public int GA_ProfitTarget
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="GA_StopLoss", Description="Stop loss", Order=2, GroupName="Parameters")]
		public double GA_StopLoss
		{ get; set; }
		#endregion

	}
}
