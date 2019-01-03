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
using NinjaTrader.NinjaScript.Universal;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.AddOns.ZTraderAO;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
    /// <summary>
    /// This file holds all user defined indicator methods.
    /// </summary>
    public partial class GIndicatorBase : Indicator
    {
		//protected ZTTimeFunc ztTimeFunc = new ZTTimeFunc();
				
		#region Time Functions
		
//		public string GetTimeDate(String str_timedate, int time_date) {
//			char[] delimiterChars = { ' '};
//			string[] str_arr = str_timedate.Split(delimiterChars);
//			return str_arr[time_date];
//		}
		
//		public string GetTimeDate(DateTime dt, int time_date) {
//			string str_dt = dt.ToString("MMddyyyy HH:mm:ss");
//			char[] delimiterChars = {' '};
//			string[] str_arr = str_dt.Split(delimiterChars);
//			return str_arr[time_date];
//		}
		
//		public double GetTimeDiff(DateTime dt_st, DateTime dt_en) {
//			double diff = -1;
//			//if(diff < 0 ) return 100; 
//			try {
			
//			if((int)dt_st.DayOfWeek==(int)dt_en.DayOfWeek) { //Same day
//				if(CompareTimeWithSessionBreak(dt_st, SessionBreak.AfternoonClose)*CompareTimeWithSessionBreak(dt_en, SessionBreak.AfternoonClose) > 0) {
//					diff = dt_en.Subtract(dt_st).TotalMinutes;
//				}
//				else if(CompareTimeWithSessionBreak(dt_st, SessionBreak.AfternoonClose) < 0 && CompareTimeWithSessionBreak(dt_en, SessionBreak.AfternoonClose) > 0) {
//					diff = GetTimeDiffSession(dt_st, SessionBreak.AfternoonClose) + GetTimeDiffSession(dt_en, SessionBreak.EveningOpen);
//				}
//			}
//			else if((dt_st.DayOfWeek==DayOfWeek.Friday && dt_en.DayOfWeek==DayOfWeek.Sunday) || ((int)dt_st.DayOfWeek>(int)dt_en.DayOfWeek) || ((int)dt_st.DayOfWeek<(int)dt_en.DayOfWeek-1)) { // Fiday - Sunday or Cross to next Week or have day off trade
//				diff = GetTimeDiffSession(dt_st, SessionBreak.AfternoonClose) + GetTimeDiffSession(dt_en, SessionBreak.EveningOpen);
//			}
//			else if((int)dt_st.DayOfWeek==(int)dt_en.DayOfWeek-1) { //Same day or next day
//				if(CompareTimeWithSessionBreak(dt_st, SessionBreak.AfternoonClose) < 0) {
//					diff = GetTimeDiffSession(dt_st, SessionBreak.AfternoonClose) + GetTimeDiffSession(dt_en, SessionBreak.NextDay);
//				}
//				else if(CompareTimeWithSessionBreak(dt_st, SessionBreak.AfternoonClose) > 0) { // dt_st passed evening open, no need to adjust
//					diff = diff = dt_en.Subtract(dt_st).TotalMinutes;
//				}
//			}
//			else {
//				diff = dt_en.Subtract(dt_st).TotalMinutes;
//			}
//			} catch(Exception ex) {
//				Print("GetTimeDiff ex:" + dt_st.ToString() + "--" + dt_en.ToString() + "--" + ex.Message);
//				diff = 100;
//			}
//			return Math.Round(diff, 2);
//		}

//		public double GetMinutesDiff(DateTime dt_st, DateTime dt_en) {
//			double diff = -1;
//			TimeSpan ts = dt_en.Subtract(dt_st);
//			diff = ts.TotalMinutes;
//			return Math.Round(diff, 2);
//		}
		
//		public int CompareTimeWithSessionBreak(DateTime dt_st, SessionBreak sb) {
//			DateTime dt = DateTime.Now;
//			try {
//			switch(sb) {
//				case SessionBreak.AfternoonClose:
//					dt = GetNewDateTime(dt_st.Year,dt_st.Month,dt_st.Day,16,0,0);
//					break;
//				case SessionBreak.EveningOpen:
//					if(dt_st.Hour < 16)
//						dt = GetNewDateTime(dt_st.Year,dt_st.Month,dt_st.Day-1,17,0,0);
//					//if(dt_st.Hour >= 17)
//					else dt = GetNewDateTime(dt_st.Year,dt_st.Month,dt_st.Day,16,0,0);
//					break;
//				default:
//					dt = GetNewDateTime(dt_st.Year,dt_st.Month,dt_st.Day,16,0,0);
//					break;
//			}
//			} catch(Exception ex) {
//				Print("CompareTimeWithSessionBreak ex:" + dt_st.ToString() + "--" + sb.ToString() + "--" + ex.Message);
//			}
//			return dt_st.CompareTo(dt);
//		}

//		public double GetTimeDiffSession(DateTime dt_st, SessionBreak sb) {			
//			DateTime dt_session = DateTime.Now;
//			TimeSpan ts = dt_session.Subtract(dt_st);
//			double diff = 100;
//			try{
//			switch(sb) {
//				case SessionBreak.AfternoonClose:
//					dt_session = GetNewDateTime(dt_st.Year, dt_st.Month, dt_st.Day, 16, 0, 0);
//					ts = dt_session.Subtract(dt_st);
//					break;
//				case SessionBreak.EveningOpen:
//					if(dt_st.Hour < 16)
//						dt_session = GetNewDateTime(dt_st.Year,dt_st.Month,dt_st.Day-1,17,0,0);
//					else 
//						dt_session = GetNewDateTime(dt_st.Year, dt_st.Month, dt_st.Day, 17, 0, 0);
//					ts = dt_st.Subtract(dt_session);
//					break;
//				case SessionBreak.NextDay:
//					dt_session = GetNewDateTime(dt_st.Year, dt_st.Month, dt_st.Day-1, 17, 0, 0);
//					ts = dt_st.Subtract(dt_session);
//					break;
//				default:
//					dt_session = GetNewDateTime(dt_st.Year, dt_st.Month, dt_st.Day-1, 17, 0, 0);
//					ts = dt_st.Subtract(dt_session);
//					break;
//			}
//			diff = ts.TotalMinutes;
//			} catch(Exception ex) {
//				Print("GetTimeDiffSession ex:" + dt_st.ToString() + "--" + sb.ToString() + "--" + ex.Message);
//			}
			
//			return Math.Round(diff, 2);
//		}
		
//		public DateTime GetNewDateTime(int year, int month, int day, int hr, int min, int sec) {
//			//DateTime(dt_st.Year,dt_st.Month,dt_st.Day,16,0,0);
//			if(day == 0) {
//				if(month == 1) {
//					year = year-1;
//					month = 12;
//					day = 31;
//				}
//				else if (month == 3) {
//					month = 2;
//					day = 28;
//				}
//				else {
//					month--;
//					if(month == 1 || month == 3 || month == 5 || month == 7 || month == 8 || month == 10)
//						day = 31;
//					else day = 30;
//				}
//			}
//			return new DateTime(year, month, day, hr, min, sec);
//		}
		
//		public string Get24HDateTime(DateTime dt) {
//			return dt.ToString("MM/dd/yyyy HH:mm:ss");
//		}

//		public int GetDateByDateTime(DateTime dt) {
//			int date = dt.Year*10000 + dt.Month*100 + dt.Day;
//			return date;
//		}
		
//		//time=10000*H + 100*M + S
//		public int GetTimeByHM(int hour, int min) {
//			return 10000*hour + 100*min;
//		}
				
//		public bool IsTimeInSpan(DateTime dt, int start, int end) {
//			int t = 100*dt.Hour + dt.Minute;
//			if(start <= t && t <= end) return true;
//			else return false;
//		}

//		public int GetTimeDiffByHM(int hour, int min, DateTime dt) {
//			int t = GetTimeByHM(hour, min);
//			int t0 = GetTimeByHM(dt.Hour, dt.Minute);
//			Print("[hour,min]=[" + hour + "," + min + "], [dt.Hour,dt.Minute]=[" + dt.Hour + "," + dt.Minute + "]," + dt.TimeOfDay);
//			return t-t0;
//		}
		
//		public int GetTimeDiffByHM(int start_hour, int start_min, int end_hour, int end_min) {
//			int t = GetTimeByHM(end_hour, end_min);
//			int t0 = GetTimeByHM(start_hour, start_min);
//			Print("[start_hour,start_min]=[" + start_hour + "," + start_min + "], [end_hour,end_min]=[" + end_hour + "," + end_min + "]");
//			return t-t0;
//		}
		
		#endregion

		#region Properties
		
//		[Description("If it runs for backtesting")]
//        [GridCategory("Parameters")]
//		[Gui.Design.DisplayNameAttribute("Back Testing")]
//	[NinjaScriptProperty]
//	[XmlIgnore]
//	[Display(Name="BackTest", Description="Back Testing", Order=1, GroupName="Parameters")]		
//    public bool BackTest
//    {
//        get { return backTest; }
//        set { backTest = value; }
//    }
		#endregion
		
    }
}


