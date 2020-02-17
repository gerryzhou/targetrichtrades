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
			List<string> names = new List<string>(){"CmdPathRoot","CmdFileName"};
			Dictionary<string,object> dic =	GUtils.GetConfigItems(GUtils.MainConfigFile, names);
			object dir = null, name = null;
			dic.TryGetValue("CmdPathRoot", out dir);
			dic.TryGetValue("CmdFileName", out name);
			string path = dir.ToString() + name.ToString();
			Print("GetCmdFilePath=" + path);
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
