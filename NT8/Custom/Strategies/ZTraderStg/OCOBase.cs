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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies.ZTraderStg
{
	public class OCOBase : Order
	{
		private Order stopLossOrder;
		private Order profitTargetOrder;
		
		#region Properites
		/// <summary>
		/// The OCO id for bracket or OCO trade
		/// </summary>		
		[Browsable(false), XmlIgnore]
		[DefaultValueAttribute(null)]
		public string OcoID
		{
			get; set;
		}
		
		[Browsable(false), XmlIgnore]
		public Order StopLossOrder
		{
			get { return stopLossOrder;	}
			set { stopLossOrder= value; }
		}
		
		[Browsable(false), XmlIgnore]
		public Order ProfitTargetOrder
		{
			get { return profitTargetOrder;	}
			set { profitTargetOrder= value; }
		}		
		#endregion
	}
}
