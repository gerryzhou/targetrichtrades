#region Using declarations
using System;
using System.Collections;
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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Indicators.PriceActions;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies.ZTraderStg
{
	public class jsonmenuwrapper
    {
        public jsonmenu menu { get; set; }
    }
    public class jsonmenu
    {
        public string id {get;set;}
        public string value { get; set; }
        public jsonpopup popup { get; set; }
    }
    public class jsonpopup
    {
        public jsonmenuitem[] menuitem {get;set;}
    }
    public class jsonmenuitem
    {
        public string value { get; set;}
        public string onclick{get;set;}
    }
}
