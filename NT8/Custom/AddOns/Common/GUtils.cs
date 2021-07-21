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

		/// <summary>
		/// Update the properties for obj with the values in the dictionary
		/// </summary>
		/// <param name="obj">Target object</param>
		/// <param name="cmd_dict">dictionary holds the new values</param>
		/// <param name="indProxy">caller indicator/strategy</param>
		public static void UpdateProperties(object stg, Dictionary<string, object> cmd_dict, GIndicatorBase indProxy) {
			Dictionary<string, ArrayList> dict_new = GConfig.ParseCmdJson(cmd_dict, indProxy);
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
		
		public static void DisplayProperties<T>(T obj, GIndicatorBase indProxy) {
			indProxy.Print(string.Format("DisplayProperties GetType.Name = {0}", obj.GetType().Name));
			foreach(PropertyInfo info in obj.GetType().GetProperties()) {
				indProxy.Print(string.Format("Name={0}, Val={1}", info.Name, info.GetValue(obj)));
			}			
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

		/// <summary>
		/// Adds indentation and line breaks to output of JavaScriptSerializer
		/// </summary>
		public static string FormatJson(string jsonString)
		{
		    var stringBuilder = new StringBuilder();

		    bool escaping = false;
		    bool inQuotes = false;
		    int indentation = 0;

		    foreach (char character in jsonString)
		    {
		        if (escaping)
		        {
		            escaping = false;
		            stringBuilder.Append(character);
		        }
		        else
		        {
		            if (character == '\\')
		            {
		                escaping = true;
		                stringBuilder.Append(character);
		            }
		            else if (character == '\"')
		            {
		                inQuotes = !inQuotes;
		                stringBuilder.Append(character);
		            }
		            else if (!inQuotes)
		            {
		                if (character == ',')
		                {
		                    stringBuilder.Append(character);
		                    stringBuilder.Append("\r\n");
		                    stringBuilder.Append('\t', indentation);
		                }
		                else if (character == '[' || character == '{')
		                {
		                    stringBuilder.Append(character);
		                    stringBuilder.Append("\r\n");
		                    stringBuilder.Append('\t', ++indentation);
		                }
		                else if (character == ']' || character == '}')
		                {
		                    stringBuilder.Append("\r\n");
		                    stringBuilder.Append('\t', --indentation);
		                    stringBuilder.Append(character);
		                }
		                else if (character == ':')
		                {
		                    stringBuilder.Append(character);
		                    stringBuilder.Append('\t');
		                }
		                else if (!Char.IsWhiteSpace(character))
		                {
		                    stringBuilder.Append(character);
		                }
		            }
		            else
		            {
		                stringBuilder.Append(character);
		            }
		        }
		    }

		    return stringBuilder.ToString();
		}
		
		public static string JsonFormat(string inputText)
	    {
	        bool escaped = false;
	        bool inquotes = false;
	        int column = 0;
	        int indentation = 0;
	        Stack<int> indentations = new Stack<int>();
	        int TABBING = 8;
	        StringBuilder sb = new StringBuilder();
	        foreach (char x in inputText)
	        {
	            sb.Append(x);
	            column++;
	            if (escaped)
	            {
	                escaped = false;
	            }
	            else
	            {
	                if (x == '\\')
	                {
	                    escaped = true;
	                }
	                else if (x == '\"')
	                {
	                    inquotes = !inquotes;
	                }
	                else if ( !inquotes)
	                {
	                    if (x == ',')
	                    {
	                        // if we see a comma, go to next line, and indent to the same depth
	                        sb.Append("\r\n");
	                        column = 0;
	                        for (int i = 0; i < indentation; i++)
	                        {
	                            sb.Append(" ");
	                            column++;
	                        }
	                    } else if (x == '[' || x== '{') {
	                        // if we open a bracket or brace, indent further (push on stack)
	                        indentations.Push(indentation);
	                        indentation = column;
	                    }
	                    else if (x == ']' || x == '}')
	                    {
	                        // if we close a bracket or brace, undo one level of indent (pop)
	                        indentation = indentations.Pop();
	                    }
	                    else if (x == ':')
	                    {
	                        // if we see a colon, add spaces until we get to the next
	                        // tab stop, but without using tab characters!
	                        while ((column % TABBING) != 0)
	                        {
	                            sb.Append(' ');
	                            column++;
	                        }
	                    }
	                }
	            }
	        }
	        return sb.ToString();
	    }
		#region Properties

		#endregion
	}
}
