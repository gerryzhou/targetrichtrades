#region Using declarations
using System;
using System.IO;
using System.Reflection;
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
#endregion

//This namespace holds Add ons in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.AddOns
{
	public class GUtils
	{
		//Function to get random number
		private static readonly Random getrandom = new Random();

		public static int GetRandomNumber(int min, int max)
		{
		    lock(getrandom) // synchronize
		    {
		        return getrandom.Next(min, max);
		    }
		}
		
		public static string GetConfigFileDir() {
			string ud_dir = NinjaTrader.Core.Globals.UserDataDir
				+ "bin" + Path.DirectorySeparatorChar
				+ "Custom" + Path.DirectorySeparatorChar;
			//Print(this.Name + ":NinjaTrader.Core.Globals.UserDataDir=" + NinjaTrader.Core.Globals.UserDataDir);
			string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			//Print(this.Name + ":Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)=" + currentDirectory);
			string appPath = System.AppDomain.CurrentDomain.BaseDirectory;
			//Print(this.Name + ":System.AppDomain.CurrentDomain.BaseDirectory=" + appPath);
			string entryPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			//Print(this.Name + ":System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)=" + entryPath);
			string curPath = System.Environment.CurrentDirectory;
			//Print(this.Name + ":System.Environment.CurrentDirectory=" + curPath);
			return ud_dir; //Directory.GetParent(currentDirectory).FullName;
		}
		
		public static string GetConfigItem(string config_file, string item_name) {
			string json_path = GetConfigFileDir() + config_file;
			string json = System.IO.File.ReadAllText(json_path);
            //DataContractJsonSerializer ser = new DataContractJsonSerializer();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> dic = serializer.Deserialize<Dictionary<string, object>>(json);
            //var jsonObject = JsonValue.Parse(json);
            string item = null;
			foreach(KeyValuePair<string, object> ele1 in dic)
			{ 
				//Print(string.Format("JSON={0} and {1}", ele1.Key, ele1.Value));
				if(item_name != null && item_name.Equals(ele1.Key))
					item = ele1.Value.ToString();
			}

			return item;
		}
		
		/// <summary>
		/// Get the config items by name from the config file
		/// </summary>
		/// <param name="config_file"></param>
		/// <param name="item_names"></param>
		/// <returns></returns>
		public static Dictionary<string, object> GetConfigItems(string config_file, List<string> item_names) {
			string json_path = GetConfigFileDir() + config_file;
			string json = System.IO.File.ReadAllText(json_path);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> dic = serializer.Deserialize<Dictionary<string, object>>(json);
//            string item = null;
//			foreach(KeyValuePair<string, object> ele in dic)
//			{ 
//				if(item_name != null && item_name.Equals(ele.Key))
//					item = ele.Value.ToString();
//			}
			
			foreach(string k in item_names) {
				if(!dic.Keys.Contains(k))
					dic.Remove(k);
			}				

			return dic;
		}
		
		/// <summary>
		/// Load the config/cmd file into a dictionary
		/// </summary>
		/// <param name="config_file"></param>
		/// <returns></returns>
		public static Dictionary<string, object> LoadJsonDictionary(string json_path) {
			//string json_path = GetConfigFileDir() + config_file;
			string json = System.IO.File.ReadAllText(json_path);
            //DataContractJsonSerializer ser = new DataContractJsonSerializer();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> dic = serializer.Deserialize<Dictionary<string, object>>(json);
			return dic;
		}
		
		#region Properties
		[Browsable(false), XmlIgnore]
		public static string MainConfigFile
		{
			get { return "ztrader.json"; }
		}
		#endregion
	}
}
