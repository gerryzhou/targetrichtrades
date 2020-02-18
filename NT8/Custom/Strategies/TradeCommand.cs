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
using System.Reflection;
using System.IO;
using System.Runtime.Serialization;

using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Indicators.PriceActions;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Strategies.ZTraderStg;
using NinjaTrader.NinjaScript.AddOns;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class GStrategyBase : Strategy
	{
		protected CmdBase cmdObj = new CmdBase();

		public virtual void InitTradeCmd() {
		}
		
		/// <summary>
		/// Command could be liquidate, stop trading, cancel orders, etc.
		/// Also it can be the change of parameters for current strategy;
		/// It can inject the market context into the strategy;
		/// It can trigger a tradeSignal which has highest priority;
		/// </summary>
		/// <returns></returns>
		public virtual CmdBase CheckCmd() {
			return cmdObj;
		}
		
		/// <summary>
		/// Replaced by PutTrade function;
		/// </summary>
		public virtual void ExecuteCommand() {
			switch(Command_Type) {
				case CommandType.ChangeAlgoType: //; 
					
					break;
				case CommandType.ChangeParams:	//; 
					
					break;
				case CommandType.InjectContext:	//;
					
					break;

				case CommandType.None: //;
					break;
			}
		}
		
		public string GetCmdFilePath() {
			List<string> names = new List<string>(){"CmdPathRoot","CmdFileName","CTXFileName"};
			Dictionary<string,object> dic =	GUtils.GetConfigItems(GUtils.MainConfigFile, names);
			object dir = null, name = null;
			dic.TryGetValue("CmdPathRoot", out dir);
			dic.TryGetValue("CmdFileName", out name);
			string path = dir.ToString() + name.ToString();
			Print("GetCmdFilePath=" + path);
			return path;
		}
		
		public string GetCTXFilePath() {
			List<string> names = new List<string>(){"CmdPathRoot","CmdFileName","CTXFileName","MenuFileName"};
			Dictionary<string,object> dic =	GUtils.GetConfigItems(GUtils.MainConfigFile, names);
			object dir = null, name = null;
			dic.TryGetValue("CmdPathRoot", out dir);
			dic.TryGetValue("CTXFileName", out name);
			string path = dir.ToString() + name.ToString();
			Print("GetCTXFilePath=" + path);
			return path;
		}
		
		public FileInfo[] GetCmdFile(string srcDir) {
			Print("GetCmdFile src: " + srcDir);
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
                Print("cmdFile=" + item.FullName);
            }
			
			return filesInOrder;
		}
		
		public void MoveCmdFiles(FileInfo[] src, string dest) {
			Print("MoveCmdFile src,dest: " + src.Length + "," + dest);
			foreach (FileInfo item in src)
            {
				string destFile = dest+item.Name;
				if (File.Exists(destFile))
				{
					File.Delete(destFile);
				}
				item.MoveTo(destFile);
				//File.Move(src, dest);
			}
		}
		
		public Dictionary<string,string> ReadParaFile(FileInfo src) {
			Dictionary<string,string> paraMap = new Dictionary<string,string>();
			
			Print("ReadParaFile src: " + src);
			if (!src.Exists)
			{
				return paraMap;
			}
	
			int counter = 0;  
			string line;

			// Read the file and display it line by line.  
			System.IO.StreamReader file =   
				new System.IO.StreamReader(src.FullName);//@"c:\test.txt");
			while((line = file.ReadLine()) != null)  
			{
				string[] pa = line.Split(':');
				paraMap.Add(pa[0], pa[1]);
				Print(line);  
				counter++;
			}

			file.Close();
			Print("There were {0} lines." + counter);
			// Suspend the screen.
			//System.Console.ReadLine();
			return paraMap;
		}
		
		/// <summary>
		/// Read Cmd parameters from json cmd file;
		/// Inidcator section set for Custom Strategy
		/// MM, TM, TG sections set for general Strategy
		/// Strategy embedded properties only set at State.SetDefaults from OnStateChange
		/// CurrentTrade only retrieved values from the custom/general Strategies;
		/// </summary>
		/// <returns></returns>
		public Dictionary<string,object> ReadCmdPara() {		
			Dictionary<string,object> paraDict = GUtils.LoadJson2Dictionary(GetCmdFilePath());
			foreach(KeyValuePair<string, object> ele in paraDict) {
				//paraMap.Add(ele.Key, ele.Value.ToString());
				Print(String.Format("ele.Key={0}, ele.Value.ToString()={1}", ele.Key, ele.Value.ToString()));
			}
			//List<Dictionary<string, object>> mkt_ctx = paraDict["MarketContextCmd"] as List<Dictionary<string, object>>;
			ArrayList mkt_ctx = paraDict["MarketContextCmd"] as ArrayList;
			Print(String.Format("mkt_ctx={0}, mkt_ctx.count={1}", mkt_ctx.GetType().ToString(), mkt_ctx.Count));
			Dictionary<string, object> ctx_dict = mkt_ctx[0] as Dictionary<string, object>;//["CTX_Daily"]
			foreach(KeyValuePair<string, object> ele2 in ctx_dict) {
				//paraMap.Add(ele.Key, ele.Value.ToString());
				Print(String.Format("ele2.Key={0}, ele2.Value.ToString()={1}", ele2.Key, ele2.Value.ToString()));
			}
			//Print(String.Format("ctx_dict={0}, ctx_dict.count={1}", ctx_dict.GetType().ToString(), ctx_dict.Count));
			GUtils.ParseCTXJson(ctx_dict, IndicatorProxy);
			return paraDict;
		}

		/// <summary>
		/// not used yet, 
		/// </summary>
		/// <returns></returns>
		public MktContext ReadCmdParaObj() {		
			MktContext paraDict = GUtils.LoadJson2Obj(GetCTXFilePath());
			Print(String.Format("ele.Key={0}, ele.Value.ToString()={1}", paraDict, paraDict.MktCtxDaily));
			foreach(DateCtx ele in paraDict.MktCtxDaily) {
				Print(String.Format("DateCtx.ele.Key={0}, ele.Value.ToString()={1}", ele.Date, ele.TimeCtxs));
				if(ele != null && ele.Date != null && ele.TimeCtxs != null) {
					Print(String.Format("DateCtx.ele.Key={0}, ele.Value.ToString()={1}", ele.Date, ele.TimeCtxs));
					foreach(TimeCtx tctx in ele.TimeCtxs) {
						Print(String.Format("ele.Date={0}, TimeCtx.tctx.Time={1}, tctx.ChannelType={2}, tctx.MinUp={3}, tctx.Support={4}",
						ele.Date, tctx.Time, tctx.ChannelType, tctx.MinUp, tctx.Support));
					}
				}
			}
//			foreach(KeyValuePair<string, List<TimeCTX>> ele in paraDict.cmdMarketContext.ctx_daily.ctx) {
//				//paraMap.Add(ele.Key, ele.Value.ToString());
//				Print(String.Format("ele.Key={0}, ele.Value.ToString()=", ele.Key));
//			}
			
			return paraDict;
		}
        #region Properties
		[Description("Command Type")]
 		[Browsable(false), XmlIgnore]
        public CommandType Command_Type
        {
            get; set;
        }	
        #endregion
	}	
}
