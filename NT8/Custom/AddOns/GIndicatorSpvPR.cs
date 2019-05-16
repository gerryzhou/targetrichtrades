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
using System.IO;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Indicators.ZTraderPattern;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
    /// <summary>
    /// This file holds supervised pattern recognition class.
    /// </summary>

	/// <summary>
	/// Supervised Pattern Recognization
	/// 20100610;8301045:UpTight#10-16-3-5;11011245:RngWide#4-6;13011459:DnWide;
	/// </summary>
	public class SpvPR {
		public int Date; //key as the dictionary for daily PriceAction
		//Market condition for TimeRange;
		//HHMMHHMM=TimeStart*10000+TimeEnd;
		public Dictionary<int,PriceAction> Mkt_Ctx; 
		
		public SpvPR (int date, Dictionary<int,PriceAction> mktCtx) {
			this.Date = date;
			this.Mkt_Ctx = mktCtx;
		}
	}
	
	public partial class GIndicatorBase : Indicator
	{
		#region SpvPR Vars
		/// <summary>
		/// Loaded from supervised file;
		/// Key1=Date; Key2=Time;
		/// </summary>		
		protected Dictionary<string,Dictionary<int,PriceAction>> Dict_SpvPR = null;
		private List<SpvPR> List_SpvPR = null; 
		/// <summary>
		/// Bitwise op to tell which Price Action allowed to be the supervised entry approach
		/// 0111 1111: [0 UnKnown RngWide RngTight DnWide DnTight UpWide UpTight]
		/// UnKnown:spvPRBits&0100 000(64)
		/// RngWide:spvPRBits&0010 0000(32), RngTight:spvPRBits&0001 0000(16)
		/// DnWide:spvPRBits&0000 1000(8), DnTight:spvPRBits&0000 0100(4)
		/// UpWide:spvPRBits&0000 0010(2), UpTight:spvPRBits&0000 0001(1)
		/// </summary>

		private int spvPRBits = 0;
		
		#endregion
		
		#region Supervised pattern recognition
		
		public FileInfo[] GetSpvFile(string srcDir, string symbol) {
			//Print("GetSupervisedFile src: " + srcDir);
		    DirectoryInfo DirInfo = new DirectoryInfo(srcDir);

//            var filesInOrder = from f in DirInfo.EnumerateFiles()
//                               orderbydescending f.CreationTime
//                               select f;
			
//			var filesInOrder = DirInfo.GetFiles("*.*",SearchOption.AllDirectories).OrderBy(f => f.LastWriteTime)
//								.ToList();
			//DirectoryInfo dir = new DirectoryInfo (folderpath);

			FileInfo[] filesInOrder = DirInfo.GetFiles().OrderByDescending(p => p.LastWriteTime).ToArray();
			
            foreach (FileInfo item in filesInOrder)
            {
                //Print("cmdFile=" + item.FullName);
            }
			
			return filesInOrder;
		}
		
		/// <summary>
		/// "20170607,8401151#DnWide#10-16-3-5,11521459#UpTight#10-16-3-5",
		/// </summary>
		/// <param name="line"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		protected Dictionary<int,PriceAction> ReadSpvPRLine(string line, out string key) {
			int prt_lev = 1;
			string[] line_pa = line.Split(',');
			Print("line_pa=" + line_pa[0] + "," + line_pa[1]);
			Dictionary<int,PriceAction> mkt_ctxs = new Dictionary<int,PriceAction>();
			for(int i=1; i<line_pa.Length; i++) {
				Print("line_pa[" + i + "]=" + line_pa[i]);
				int t, minUp, maxUp, minDn, maxDn;
				TraceMessage(this.Name, prt_lev);
				string[] mkt_ctx = line_pa[i].Split('#');
				TraceMessage(this.Name, prt_lev);
				int.TryParse(mkt_ctx[0], out t);//parse the time of the PA;
				TraceMessage(this.Name, prt_lev);
				//string[] pa = mkt_ctx[1].Split('#');
				PriceActionType pat = (PriceActionType)Enum.Parse(typeof(PriceActionType), mkt_ctx[1]);//parse the PA type;
				TraceMessage(this.Name, prt_lev);
				string[] v = mkt_ctx[2].Split('-');
				TraceMessage(this.Name, prt_lev);
				int.TryParse(v[0], out minUp);
				int.TryParse(v[1], out maxUp);
				int.TryParse(v[2], out minDn);
				int.TryParse(v[3], out maxDn);					
				TraceMessage(this.Name, prt_lev);
				mkt_ctxs.Add(t, new PriceAction(pat, minUp, maxUp, minDn, maxDn));
			}
//				if(mkt_ctxs.Count > 0) {
//					Dict_SpvPR.Add(line_pa[0], mkt_ctxs);
//				}
			key = line_pa[0];
			return mkt_ctxs;
		}
		
		/// <summary>
		/// 20170522;9501459:UpTight#10-16-3-5
		/// </summary>
		/// <param name="srcDir"></param>
		/// <param name="symbol"></param>
		/// <returns></returns>
		public Dictionary<string,Dictionary<int,PriceAction>> ReadSpvFile(string srcDir, string symbol) {
			//Dictionary<string,Dictionary<int,PriceActionType>> 
			Dict_SpvPR = new Dictionary<string,Dictionary<int,PriceAction>>();
			string src = srcDir + symbol + ".txt";
			//Print("ReadSpvPRFile src: " + src);
//			if (!src.Exists)
//			{
//				return paraMap;
//			}
	
			int counter = 0;  
			string line;

			// Read the file and display it line by line.  
			System.IO.StreamReader file =   
				new System.IO.StreamReader(src);//@"c:\test.txt");
			while((line = file.ReadLine()) != null)  
			{
				if(line.StartsWith("//")) continue; //comments line, skip it;
				string key;
				Dictionary<int,PriceAction> mkt_ctxs = ReadSpvPRLine(line.Substring(1, line.Length-3), out key);//remove leading " and ending ",
				//Print(line);
				if(mkt_ctxs.Count > 0) {
					Dict_SpvPR.Add(key, mkt_ctxs);
				}
				counter++;
			}

			file.Close();

//			foreach(var pair in Dict_SpvPR) {
				//Print("mktCtx: key,val=" + pair.Key + "," + pair.Value + "," + pair.ToString());
//				Dictionary<int,PriceAction> mkcnd = (Dictionary<int,PriceAction>)pair.Value;
//				foreach(var cnd in mkcnd) {
//					Print("time,cnd=" + cnd.Key + "," + cnd.Value);
//				}
//			}
			return Dict_SpvPR;
		}
		
		/// <summary>
		/// 20170522;9501459:UpTight#10-16-3-5
		/// Load daily pattern line to the dictionary
		/// </summary>
		/// <returns></returns>
		public List<SpvPR> LoadSpvPRList(List<string> dailyPattern) {
			//Dictionary<string,Dictionary<int,PriceActionType>> 
			Dict_SpvPR = new Dictionary<string,Dictionary<int,PriceAction>>();			
			List_SpvPR = new List<SpvPR>();
			
			int counter = 0;  
			//string line;
			foreach(string dayPR in dailyPattern) { //SpvDailyPattern.spvPRDay
				Print("dayPR=" + dayPR);
				string key;
				int key_int;
				Dictionary<int,PriceAction> mkt_ctxs = ReadSpvPRLine(dayPR, out key);
				int.TryParse(key, out key_int);
				if(mkt_ctxs.Count > 0) {					
					List_SpvPR.Add(new SpvPR(key_int, mkt_ctxs));
				}
				counter++;
			}
			//Print("ReadSpvPRList:" + counter);
			return List_SpvPR;
		}

		/// <summary>
		/// 20170522;9501459:UpTight#10-16-3-5
		/// </summary>
		/// <param name="srcDir"></param>
		/// <param name="symbol"></param>
		/// <returns></returns>
		public Dictionary<string,Dictionary<int,PriceAction>> ReadSpvPRList() {
			//Dictionary<string,Dictionary<int,PriceActionType>> 
			Dict_SpvPR = new Dictionary<string,Dictionary<int,PriceAction>>();			
			
			int counter = 0;  
			//string line;
			foreach(string dayPR in SpvDailyPattern.spvPRDay) {
				//Print(dayPR);
				//ReadSpvPRLine(dayPR);
				counter++;
			}
			//Print("ReadSpvPRList:" + counter);
			return Dict_SpvPR;
		}

		public PriceAction GetPriceAction(DateTime dt) {
			
			PriceAction pa = new PriceAction(PriceActionType.UnKnown, -1, -1, -1, -1);
			
			int key_date = GetDateByDateTime(dt);
			int t = dt.Hour*100 + dt.Minute;
			
			Dictionary<int,PriceAction> mkt_ctxs = null;
			if(Dict_SpvPR != null)
				Dict_SpvPR.TryGetValue(key_date.ToString(), out mkt_ctxs);
			//Print("key_year, time, Dict_SpvPR, mkt_ctxs=" + key_year.ToString() + "," + t.ToString() + "," + Dict_SpvPR + "," + mkt_ctxs);
			if(mkt_ctxs != null) {
				foreach(var mkt_ctx in mkt_ctxs) {
					//Print("time,mkt_ctx=" + mkt_ctx.Key + "," + mkt_ctx.Value);
					int start = mkt_ctx.Key/10000;
					int end = mkt_ctx.Key % 10000;
					
					if(t >= start && t <= end) {
						pa = mkt_ctx.Value;
						break;
					}
				}
			}
			return pa;
		}
		
		/// <summary>
		/// Check if the price action type allowed for supervised PR 
		/// </summary>
		/// <returns></returns>
		public bool IsSpvAllowed4PAT(PriceActionType pat) {
			int i;
			switch(pat) {
				case PriceActionType.UpTight: //
					i = (1 & SpvPRBits);
					//Print("IsSpvAllowed4PAT:" + pat.ToString() + ", (1 & SpvPRBits)=" + i);
					return (1 & SpvPRBits) > 0;
				case PriceActionType.UpWide: //wide up channel
					i = (2 & SpvPRBits);
					//Print("IsSpvAllowed4PAT:" + pat.ToString() + ", (2 & SpvPRBits)=" + i);
					return (2 & SpvPRBits) > 0;
				case PriceActionType.DnTight: //
					i = (4 & SpvPRBits);
					//Print("IsSpvAllowed4PAT:" + pat.ToString() + ", (4 & SpvPRBits)=" + i);
					return (4 & SpvPRBits) > 0;
				case PriceActionType.DnWide: //wide dn channel
					i = (8 & SpvPRBits);
					//Print("IsSpvAllowed4PAT:" + pat.ToString() + ", (8 & SpvPRBits)=" + i);
					return (8 & SpvPRBits) > 0;
				case PriceActionType.RngTight: //
					i = (16 & SpvPRBits);
					//Print("IsSpvAllowed4PAT:" + pat.ToString() + ", (16 & SpvPRBits)=" + i);
					return (16 & SpvPRBits) > 0;
				case PriceActionType.RngWide: //
					i = (32 & SpvPRBits);
					//Print("IsSpvAllowed4PAT:" + pat.ToString() + ", (32 & SpvPRBits)=" + i);
					return (32 & SpvPRBits) > 0;
				case PriceActionType.UnKnown: //
					i = (64 & SpvPRBits);
					//Print("IsSpvAllowed4PAT:" + pat.ToString() + ", (64 & SpvPRBits)=" + i);
					return (64 & SpvPRBits) > 0;					
				default:
					return false;
			}			
		}		
		#endregion
		
		#region Properties
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="SpvPRBits", Description="Supervised PR Bits", Order=1, GroupName="Parameters")]
        public int SpvPRBits
        {
            get { return spvPRBits; }
            set { spvPRBits = Math.Max(0, value); }
        }
		#endregion
    }
}
