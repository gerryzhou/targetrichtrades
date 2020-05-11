#region Using declarations
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using System.Web.Script.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Indicators.PriceActions;
using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.NinjaScript.Strategies.ZTraderStg;
#endregion

//This namespace holds Add ons in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.AddOns
{
	public class GAlert
	{
		protected static volatile bool PlayAlert = true;

		public static void PlaySoundLoop(SoundPlay sp) {//A Task return type will eventually yield a void
			while(PlayAlert && sp.SoundLoop > 0 && !String.IsNullOrEmpty(sp.SoundLoopFile)) {
				sp.SoundLoop--;
				NinjaTrader.Core.Globals.PlaySound(sp.SoundLoopFile.ToString());
				Thread.Sleep(2000);
			}
        }

		public static void ShowMessage(AlertMessage msg) {
			if (MessageBox.Show(msg.MessageTxt, msg.CaptionTxt) == MessageBoxResult.OK)
				PlayAlert = false;
		}
	
		public static void PlaySoundFile(AlertMessage msg, GIndicatorBase indProxy) {
			List<string> names = new List<string>(){"SoundFileName", "SoundPlayLoop"};
			Dictionary<string,object> dic =	GConfig.GetConfigItems(GConfig.MainConfigFile, names);
			object name = null, loop = null;
			if(dic.TryGetValue("SoundFileName", out name) &&
				dic.TryGetValue("SoundPlayLoop", out loop)) {
				string path = GConfig.GetSoundFileDir() + name.ToString();
				indProxy.Print("GetSoundFilePath,SoundPlayLoop=" + path + ", " + loop);

				SoundPlay soundplay = new SoundPlay(path, loop.ToString());				
				
				PlayAlert = true;

				Thread thdSoundPlay = new Thread(() => PlaySoundLoop(soundplay));
				Thread thdMsgShow = new Thread(() => ShowMessage(msg));

				thdSoundPlay.Start();
				thdMsgShow.Start();
			}
		}
	}
	
	public class SoundPlay {
		public string SoundLoopFile = "";
		public short SoundLoop = 12;
		public SoundPlay(string path, string loop) {
			if(Int16.TryParse(loop.ToString(), out SoundLoop)) {
				SoundLoopFile = path;
			}
		}
	}
	
	public class AlertMessage {
		public string MessageTxt = "";
		public string CaptionTxt = "";
		public Window OwnerWin = null;
		public AlertMessage(Window winOwner, string msgTxt, string capTxt) {
			this.OwnerWin = winOwner;
			this.MessageTxt = msgTxt;
			this.CaptionTxt = capTxt;
		}
	}
}
