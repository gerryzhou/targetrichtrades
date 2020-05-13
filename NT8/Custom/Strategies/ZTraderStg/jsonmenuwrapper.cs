#region Using declarations
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
