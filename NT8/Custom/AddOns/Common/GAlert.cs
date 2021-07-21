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
using NinjaTrader.NinjaScript.AddOns.PriceActions;
using NinjaTrader.NinjaScript.Strategies;
#endregion

//This namespace holds Add ons in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.AddOns
{
	public class GAlert
	{
		protected static volatile bool PlayAlert = true;
		public static int AlertBarsBack = -1;
		public static string SoundLoopFilePath = String.Empty;
		public static int SoundLoopCount = 40;

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
//			List<string> names = new List<string>(){"SoundFileName", "SoundPlayLoop"};
//			Dictionary<string,object> dic =	GConfig.GetConfigItems(GConfig.MainConfigFile, names);
			if(String.IsNullOrEmpty(SoundLoopFilePath)) {
				LoadAlerConfig(indProxy);
			}
			SoundPlay soundplay = new SoundPlay(SoundLoopFilePath, SoundLoopCount);				
			
			PlayAlert = true;
			indProxy.Print(String.Format("PlaySoundFile AlertBarsBackStr={0}, GetSoundFilePath={1}, SoundPlayLoopStr={2}",
			 		AlertBarsBack, SoundLoopFilePath, SoundLoopCount));
			
			Thread thdSoundPlay = new Thread(() => PlaySoundLoop(soundplay));
			Thread thdMsgShow = new Thread(() => ShowMessage(msg));

			thdSoundPlay.Start();
			thdMsgShow.Start();
//			object name = null, loop = null;
//			if(dic.TryGetValue("SoundFileName", out name) &&
//				dic.TryGetValue("SoundPlayLoop", out loop)) {
//				string path = GConfig.GetSoundFileDir() + name.ToString();
//				indProxy.Print("GetSoundFilePath,SoundPlayLoop=" + path + ", " + loop);

//				SoundPlay soundplay = new SoundPlay(path, loop.ToString());				
				
//				PlayAlert = true;

//				Thread thdSoundPlay = new Thread(() => PlaySoundLoop(soundplay));
//				Thread thdMsgShow = new Thread(() => ShowMessage(msg));

//				thdSoundPlay.Start();
//				thdMsgShow.Start();
//			}
		}
		
		public static void LoadAlerConfig(GIndicatorBase indProxy) {
			List<string> names = new List<string>(){"AlertBarsBack", "SoundFileName", "SoundPlayLoop"};
			Dictionary<string,object> dic =	GConfig.GetConfigItems(GConfig.MainConfigFile, names);
			object altBarsBack, name = null, loop = null;
			if(dic.TryGetValue("AlertBarsBack", out altBarsBack) &&
				dic.TryGetValue("SoundFileName", out name) &&
				dic.TryGetValue("SoundPlayLoop", out loop)) {
				SoundLoopFilePath = GConfig.GetSoundFileDir() + name.ToString();
				int.TryParse(altBarsBack.ToString(), out AlertBarsBack);
				int.TryParse(loop.ToString(), out SoundLoopCount);
				indProxy.Print(String.Format("LoadAlerConfig AlertBarsBackStr={0}, GetSoundFilePath={1}, SoundPlayLoopStr={2}",
					AlertBarsBack, SoundLoopFilePath, SoundLoopCount));
			}
		}
	}
	
	public class SoundPlay {
		public string SoundLoopFile = "";
		public int SoundLoop = 12;
		public SoundPlay(string path, int loop) {
			SoundLoopFile = path;
			SoundLoop = loop;
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
