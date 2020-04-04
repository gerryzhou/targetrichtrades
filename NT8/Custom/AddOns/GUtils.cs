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
		
		public static string GetUserDir() {
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
		
		public static string GetConfigFileDir() {
			string ud_dir = GetUserDir() + "Config" + Path.DirectorySeparatorChar;
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
		/// Get the config items by name from the config dictionary
		/// </summary>
		/// <param name="config_dict"></param>
		/// <param name="item_names"></param>
		/// <returns></returns>
		public static Dictionary<string, object> GetConfigItems(Dictionary<string, object> config_dict, List<string> item_names) {		
			foreach(string k in item_names) {
				if(!config_dict.Keys.Contains(k))
					config_dict.Remove(k);
			}				

			return config_dict;
		}
		
		/// <summary>
		/// Load the config/cmd file into a dictionary
		/// </summary>
		/// <param name="config_file"></param>
		/// <returns></returns>
		public static Dictionary<string, object> LoadJson2Dictionary(string json_path) {
			//string json_path = GetConfigFileDir() + config_file;
			string json = System.IO.File.ReadAllText(json_path);
            //DataContractJsonSerializer ser = new DataContractJsonSerializer();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> dict = serializer.Deserialize<Dictionary<string, object>>(json);
			return dict;
		}

		/// <summary>
		/// Load the config/cmd file into the object
		/// </summary>
		/// <param name="config_file"></param>
		/// <returns></returns>
		public static MktContext LoadJson2Obj(string json_path) {
			//string json_path = GetConfigFileDir() + config_file;
			string json = System.IO.File.ReadAllText(json_path);
            //DataContractJsonSerializer ser = new DataContractJsonSerializer();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            MktContext dict = serializer.Deserialize<MktContext>(json);
			return dict;
		}

		/// <summary>
		/// Load the config/cmd string into the object
		/// </summary>
		/// <param name="json_str"></param>
		/// <returns></returns>
		public static List<GitHubRelease> LoadJsonStr2Obj(string json_str) {
			//string json_path = GetConfigFileDir() + config_file;
			//string json = System.IO.File.ReadAllText(json_path);
            //DataContractJsonSerializer ser = new DataContractJsonSerializer();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            //MktContext dict = serializer.Deserialize<MktContext>(json_str);
			List<GitHubRelease> dict = serializer.Deserialize<List<GitHubRelease>>(json_str);
			return dict;
		}
		
		/// <summary>
		/// Transfer the key/value from the original CMD JSON into a new dictionary
		/// "Indicator":[
		/// {"EnBarsBeforeInflection":[ "2", "Int32", "Bars count before inflection for entry"]},
		/// {"BarsLookback":[ "2", "Int32", "Bars count before inflection for entry"]}
		/// ],
		/// </summary>
		/// <param name="dict_ori"></param>
		/// <returns></returns>
		public static Dictionary<string, ArrayList> ParseCmdJson(Dictionary<string, object> dict_ori, GIndicatorBase indProxy) {
			Dictionary<string, ArrayList> dict_new = new Dictionary<string, ArrayList>();
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			indProxy.Print("==ParseCmdJson== " + dict_ori.Count);
			//Get the array for parameter row at CMD JSON file
			try{
			foreach(ArrayList val in dict_ori.Values) {
				indProxy.Print(string.Format("dict_ori.val: {0}, {1}", val.ToString(), val.Count));
				//List<object> list = serializer.Deserialize<List<object>>(val.ToString());
				foreach(Dictionary<string, object> obj in val) {
					indProxy.Print(string.Format("dict_ori.val.dict: {0}, {1}", obj.GetType(), obj.Count));
					foreach (KeyValuePair<string, object> pa in obj) {
						dict_new.Add(pa.Key, pa.Value as ArrayList);
					}
					//Dictionary<string, object> dict = serializer.Deserialize<Dictionary<string, object>>(obj.ToString());
				}
			}
			} catch(Exception ex) {
				indProxy.Print("Exception=" + ex.StackTrace);
			}
			
			indProxy.Print("==dict_new== " + dict_new.Count);
			foreach (KeyValuePair<string, ArrayList> pa in dict_new) {
				indProxy.Print(string.Format("{0},{1}",	pa.Key, pa.Value[0]));
			}
			
			return dict_new;
		}
		
		/// <summary>
		//"MktCtxDaily":[
		//      {
		//         "Date":"20170607",
		//         "TimeCtxs":[
		//            {
		//               "Time":8401151,
		//               "ChannelType":"UpTight",
		//			   "Support":8600.75, "Resistance":9600.75,
		//               "MinUp":12, "MaxUp":16, "MinDn":3, "MaxDn":5
		//            },
		//            {
		//               "Time":11521459,
		//               "ChannelType":"RngTight",
		//			   "Support":8978.25, "Resistance":9270.75,
		//               "MinUp":10, "MaxUp":16, "MinDn":4, "MaxDn":6
		//            }
		//         ]
		//      },
		//	......
		// ]
		/// </summary>
		/// <param name="dict_ori"></param>
		/// <returns></returns>
		public static Dictionary<string, List<MarketContext>> ParseCTXJson(Dictionary<string, object> dict_ori, GIndicatorBase indProxy) {
			Dictionary<string, List<MarketContext>> dict_new = null; // = new Dictionary<string, List<MarketContext>>();
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			indProxy.Print("==ParseCTXJson== " + dict_ori.Count);
			//Get the array for parameter row at CMD JSON file
			try{
				foreach(ArrayList val in dict_ori.Values) {
					indProxy.Print(string.Format("{0}, {1}", val.ToString(), val.Count));
					
					//List<MarketContext> list = serializer.Deserialize<List<MarketContext>>(val.ToString());
	//				foreach(Dictionary<string, object> obj in val) {
	//					indProxy.Print(string.Format("{0},{1}",	obj.GetType(), obj.Count));
	//					foreach (KeyValuePair<string, object> pa in obj) {
	//						dict_new.Add(pa.Key, pa.Value as List<MarketContext>);
	//					}
	//					//Dictionary<string, object> dict = serializer.Deserialize<Dictionary<string, object>>(obj.ToString());
	//				}
					LoadCtxDaily(val, out dict_new, indProxy);
					if(dict_new != null) {
						indProxy.Print("==dict_new== " + dict_new.Count);
						foreach (KeyValuePair<string, List<MarketContext>> pa in dict_new) {
							indProxy.Print(string.Format("{0},{1}",	pa.Key, pa.Value[0]));
						}
					}
				}
			} catch(Exception ex) {
				indProxy.Print("Exception=" + ex.StackTrace);
			}
			
			return dict_new;
		}
		
		public static bool LoadCtxDaily(ArrayList al_ori, out Dictionary<string, List<MarketContext>> dict_out, GIndicatorBase indProxy) {
			dict_out = new Dictionary<string, List<MarketContext>>();
			indProxy.Print("==LoadCtxDaily== " + al_ori.Count);
			foreach(Dictionary<string, object> ctx_daily in al_ori) { //CTX_Daily index=0
				indProxy.Print(String.Format("ctx_daily in al_ori: {0}, {1}", ctx_daily.GetType().ToString(), ctx_daily.Count));
				foreach(KeyValuePair<string, object> date_al in ctx_daily) { //Date_CTX arraylist (Key=Date, Value=ArrayList_TimeCtx)
					indProxy.Print(String.Format("date_al.key={0}, date_al.val={1}", date_al.Key, date_al.Value.ToString()));
					foreach(Dictionary<string, object> dict_time_ctx in date_al.Value as ArrayList) { //Time_CTX dict (Key=Time, Value=CTX)
						indProxy.Print(String.Format("dict_time_ctx.type={0}, dict_time_ctx.count={1}", dict_time_ctx.GetType().ToString(), dict_time_ctx.Count));
						foreach(KeyValuePair<string, object> time_ctx in dict_time_ctx) { //Time_CTX item (Key=Time,CTX, Value=int,string)
							indProxy.Print(String.Format("time_ctx.Value.type={0}, time_ctx.Key={1}, time_ctx.Value={2}", time_ctx.Value.GetType().ToString(), time_ctx.Key, time_ctx.Value));
						}
					}
				}
			}
			
			return false;
		}		
		/// <summary>
		/// Update the properties for obj with the values in the dictionary
		/// </summary>
		/// <param name="obj">Target object</param>
		/// <param name="cmd_dict">dictionary holds the new values</param>
		/// <param name="indProxy">caller indicator/strategy</param>
		public static void UpdateProperties(object stg, Dictionary<string, object> cmd_dict, GIndicatorBase indProxy) {
			Dictionary<string, ArrayList> dict_new = ParseCmdJson(cmd_dict, indProxy);
			try{
				indProxy.Print("==UpdateProperties== " + stg.GetType().FullName);
				foreach (PropertyInfo p in stg.GetType().GetProperties())
				{
					if(dict_new.Keys.Contains(p.Name)) {
					    indProxy.Print(string.Format("SetValue=={0},{1}",
							p.Name, p.PropertyType)); //p.ReflectedType, p.DeclaringType, p.ToString(), GetValue(this, null)
						p.SetValue(stg, ParseProperties(dict_new, p.Name, p.PropertyType, indProxy));
					}
				}
			} catch(Exception ex) {
				indProxy.Print("Exception=" + ex.StackTrace);
			}			
		}
		
		public static object ParseProperties(Dictionary<string, ArrayList> dict, string name, Type t, GIndicatorBase indProxy) {
			ArrayList list;
			dict.TryGetValue(name, out list);
			object val = null;// = list[0];
			switch(t.Name) {
				case "Double":
					val = Double.Parse(list[0].ToString());
					break;
				case "Int32":
					val = Int32.Parse(list[0].ToString());
					break;
				case "String":
					//val = String.Parse(list[0].ToString());
					break;
				case "Boolean":
					val = Boolean.Parse(list[0].ToString());
					break;
				default:
					val = Enum.Parse(t, list[0].ToString()); 
					break;
			}
			indProxy.Print(string.Format("ParseProperties== t.Name={0}, Val={1}", t.Name, val));
//			foreach (KeyValuePair<string, ArrayList> pa in dict) {
//				indProxy.Print(string.Format("{0},{1},{2},{3},{4}",
//				pa.Key, pa.Value[0], pa.Value[1], pa.Value[2], val));
//			}
			return val;
		}
		
		public static bool DisplayDictionary(Dictionary<string, object> dict, int level, StringBuilder tbOutput)
        {
            bool bSuccess = false;
            int indentLevel = level++;
			if(tbOutput == null)
				tbOutput = new StringBuilder();

            foreach (string strKey in dict.Keys)
            {
                string strOutput = "".PadLeft(indentLevel * 8 ) + strKey + ":";
                tbOutput.Append("\r\n" + strOutput);

                object o = dict[strKey];
                if (o is Dictionary<string, object>)
                {
                    DisplayDictionary((Dictionary<string, object>)o, indentLevel, tbOutput);
                }
                else if (o is ArrayList)
                {
                    foreach (object oChild in ((ArrayList)o))
                    {
                        if (oChild is string)
                        {
                            strOutput = ((string) oChild);
                            tbOutput.Append(strOutput + ",");
                        }
                        else if (oChild is Dictionary<string, object>)
                        {
                            DisplayDictionary((Dictionary<string, object>)oChild, indentLevel, tbOutput);
                            tbOutput.Append("\r\n");
                        }
                    }
                }
                else
                {
                    strOutput = o.ToString();
                    tbOutput.Append(strOutput);
                }
            }

            indentLevel--;

            return bSuccess;
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
