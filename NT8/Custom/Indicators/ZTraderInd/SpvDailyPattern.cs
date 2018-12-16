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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.ZTraderInd
{
	public class SpvDailyPattern
	{
	
		#region spvPRDay		
		public static List<string> spvPRDay = new List<string>(){
			"20160714;7501059:RngWide#6-10-6-10",
			"20160715;7501059:RngWide#6-10-6-10",
			"20160718;7501059:RngWide#6-10-6-10",
			"20160719;7501059:RngWide#6-10-6-10",
			"20160720;7501059:RngWide#6-10-6-10",
			"20160721;7501059:RngWide#6-10-6-10",
			"20160722;7501059:RngWide#6-10-6-10",
			"20160725;7501059:RngWide#6-10-6-10",
			"20160726;7501059:RngWide#6-10-6-10",
			"20160727;7501059:RngWide#6-10-6-10",
			"20160728;7501059:RngWide#6-10-6-10",
			"20160729;7501059:RngWide#6-10-6-10",
			"20160731;7501059:RngWide#6-10-6-10",
			"20160801;7501059:RngWide#6-10-6-10",
			"20160802;7501059:RngWide#6-10-6-10",
			"20160803;7501059:RngWide#6-10-6-10",
			"20160804;7501059:RngWide#6-10-6-10",
			"20160807;7501059:RngWide#6-10-6-10",
			"20160808;7501059:RngWide#6-10-6-10",
			"20160809;7501059:RngWide#6-10-6-10",
			"20160810;7501059:RngWide#6-10-6-10",
			"20160811;7501059:RngWide#6-10-6-10",
			"20160814;7501059:RngWide#6-10-6-10",
			"20160815;7501059:RngWide#6-10-6-10",
			"20160816;7501059:RngWide#6-10-6-10",
			"20160817;7501059:RngWide#6-10-6-10",
			"20160818;7501059:RngWide#6-10-6-10",
			"20160821;7501059:RngWide#6-10-6-10",
			"20160822;7501059:RngWide#6-10-6-10",
			"20160823;7501059:RngWide#6-10-6-10",
			"20160824;7501059:RngWide#6-10-6-10",
			"20160825;7501059:RngWide#6-10-6-10",
			"20160828;7501059:RngWide#6-10-6-10",
			"20160829;7501059:RngWide#6-10-6-10",
			"20160830;7501059:RngWide#6-10-6-10",
			"20160831;7501059:RngWide#6-10-6-10",
			"20160901;7501059:RngWide#6-10-6-10",
			"20160904;7501059:RngWide#6-10-6-10",
			"20160905;7501059:RngWide#6-10-6-10",
			"20160906;7501059:RngWide#6-10-6-10",
			"20160907;7501059:RngWide#6-10-6-10",
			"20160908;7501059:RngWide#6-10-6-10",
			"20160909;7501059:RngWide#6-10-6-10",
			"20170522;9501459:UpTight#10-16-3-5",
			"20170523;8501059:UpTight#10-16-3-5",
			"20170524;13051459:UpTight#10-16-3-5",
			"20170525;8401459:UpTight#10-16-3-5",
			"20170526;8401459:RngTight#10-16-3-5",
			"20170529;8401459:RngTight#10-16-3-5",
			"20170530;8401459:RngTight#10-16-3-5",
			"20170531;8200910:DnTight#10-16-3-5;13101459:UpTight#10-16-3-5",
			"20170601;8401459:UpTight#10-16-3-5",
			"20170602;8591459:UpTight#10-16-3-5",
			"20170605;8401459:RngTight#10-16-3-5",
			"20170606;8401401:UpWide#10-16-3-5",
			"20170607;8401151:DnWide#10-16-3-5;11521459:UpTight#10-16-3-5",
			"20170608;8401123:UpTight#10-16-3-5;11301459:DnTight#10-16-3-5",
			"20170609;8040940:UpTight#10-16-3-5;9411459:DnTight#10-16-3-5",
			"20170612;8401205:RngWide#10-16-3-5;13011459:RngTight#10-16-3-5",
			"20170613;1000700:RngTight#10-16-3-5;8301000:DnTight#10-16-3-5;10001459:UpTight#10-16-3-5",
			"20170614;7301105:DnTight#10-16-3-5;11011305:RngTight#10-16-3-5;13061415:DnTight#10-16-3-5;14161459:UpTight#10-16-3-5",
			"20170615;1300800:DnTight#10-16-3-5;8300930:RngWide#10-16-3-5;9301459:UpTight#10-16-3-5",
			"20170616;6301000:DnTight#10-16-3-5;10051459:UpTight#10-16-3-5",
			//April 2018
			"20180402;7501059:UpWide#6-22-6-22",
			"20180403;7501059:UpWide#6-22-6-22",
			"20180404;7501059:UpWide#6-22-6-22",
			"20180405;7501059:UpWide#6-22-6-22",
			"20180406;7501059:UpWide#6-22-6-22",
			"20180409;7501059:UpWide#6-22-6-22",
			"20180410;7501059:UpWide#6-22-6-22",
			"20180411;7501059:UpWide#6-22-6-22",
			"20180412;7501059:UpWide#6-22-6-22",
			"20180413;7501059:UpWide#6-22-6-22",
			"20180416;7501059:UpWide#6-22-6-22",
			"20180417;7501059:UpWide#6-22-6-22",
			"20180418;7501059:UpWide#6-22-6-22",
			"20180419;7501059:DnWide#6-22-6-22",
			"20180420;7501059:DnWide#6-22-6-22",
			"20180423;7501059:RngWide#6-22-6-22",
			"20180424;7501059:DnWide#6-22-6-22",
			"20180425;7501059:RngWide#6-22-6-22",
			"20180426;7501059:UpWide#6-22-6-22",
			"20180427;7501059:UpWide#6-22-6-22",
			"20180430;7501059:RngWide#6-22-6-22",
			//May 2018
			"20180501;7501059:RngWide#6-22-6-22",
			"20180502;7501059:RngWide#6-22-6-22",
			"20180503;7501059:RngWide#6-22-6-22",
			"20180504;7501059:UpWide#6-22-6-22",
			"20180507;7501059:UpWide#6-22-6-22",
			"20180508;7501059:UpWide#6-22-6-22",
			"20180509;7501059:UpWide#6-22-6-22",
			"20180510;7501059:UpWide#6-22-6-22",
			"20180511;7501059:UpWide#6-22-6-22",
			"20180514;7501059:UpWide#6-22-6-22",
			"20180515;7501059:UpWide#6-22-6-22",
			"20180516;7501059:UpWide#6-22-6-22",
			"20180517;7501059:UpWide#6-22-6-22",
			"20180518;7501059:UpWide#6-22-6-22",
			"20180521;7501059:DnWide#6-22-6-22",
			"20180522;7501059:DnWide#6-22-6-22",
			"20180523;7501059:RngWide#6-22-6-22",
			"20180524;7501059:DnWide#6-22-6-22",
			"20180525;7501059:RngWide#6-22-6-22",
			"20180528;7501059:UnKnown#6-22-6-22",
			"20180529;7501059:UpWide#6-22-6-22",
			"20180530;7501059:UpWide#6-22-6-22",
			"20180531;7501059:UpWide#6-22-6-22",
		};
		#endregion spvPRDay		
	}
}
