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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Strategies.ZTraderStg;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class CmdObject {
		private Strategy instStrategy = null;

		public Strategy GetStrategy() {
			return this.instStrategy;
		}
	}
	
	public partial class GSZTraderBase : Strategy
	{
		protected CmdObject cmdObj = null;

		public virtual void InitTradeCmd() {
			new CmdObject();
		}
		
		public virtual CmdObject CheckCmd() {
			return cmdObj;
		}
		
		public virtual void ExecuteCommand() {
			switch(AlgoMode) {
				case AlgoModeType.Liquidate: // 0=liquidate; 
					CloseAllPositions();
					break;
				case AlgoModeType.Trading:	// 1=trading; 
					//CheckPositions();
					CheckTrigger();
					ChangeSLPT();
					CheckEnOrder(-1);
					
					if(NewOrderAllowed() && PatternMatched())
					{
						//indicatorProxy.PrintLog(true, !backTest, "----------------PutTrade, isReversalBar=" + isReversalBar + ",giParabSAR.IsSpvAllowed4PAT(curBarPat)=" + giParabSAR.IsSpvAllowed4PAT(curBarPriceAction.paType));
						//PutTrade(zz_gap, cur_gap, isReversalBar);
					}
					break;
				case AlgoModeType.SemiAlgo:	// 2=semi-algo(manual entry, algo exit);
					ChangeSLPT();
					break;
				case AlgoModeType.ExitOnly: // -1=stop trading(no entry/exit, cancel entry orders and keep the exit order as it is if there has position);
					CancelEntryOrders();
					break;
				case AlgoModeType.StopTrading: // -2=stop trading(no entry/exit, liquidate positions and cancel all entry/exit orders);
					CancelAllOrders();
					break;
				default:
					break;
			}
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
		
        #region Properties
	
        #endregion		
	}	
}
