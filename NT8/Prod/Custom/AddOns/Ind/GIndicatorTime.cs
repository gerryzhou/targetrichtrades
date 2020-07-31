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
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.Gui.Tools;
//using NinjaTrader.NinjaScript.Universal;
#endregion

//This namespace holds Add ons in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class GIndicatorBase : Indicator
	{
		#region Time Functions
		
		public string GetTimeDate(String str_timedate, int time_date) {
			char[] delimiterChars = {' '};
			string[] str_arr = str_timedate.Split(delimiterChars);
			return str_arr[time_date];
		}
		
		public string GetTimeDate(DateTime dt, int time_date) {
			string str_dt = dt.ToString("MMddyyyy HH:mm:ss");
			char[] delimiterChars = {' '};
			string[] str_arr = str_dt.Split(delimiterChars);
			return str_arr[time_date];
		}
		
		public string GetCurTime() {
			return DateTime.Now.ToString("HH:mm:ss");
		}
		
		public double GetTimeDiff(DateTime dt_st, DateTime dt_en) {
			double diff = -1;
			//if(diff < 0 ) return 100; 
			try {
			
			if((int)dt_st.DayOfWeek==(int)dt_en.DayOfWeek) { //Same day
				if(CompareTimeWithSessionBreak(dt_st, SessionBreak.AfternoonClose)*CompareTimeWithSessionBreak(dt_en, SessionBreak.AfternoonClose) > 0) {
					diff = dt_en.Subtract(dt_st).TotalMinutes;
				}
				else if(CompareTimeWithSessionBreak(dt_st, SessionBreak.AfternoonClose) < 0 && CompareTimeWithSessionBreak(dt_en, SessionBreak.AfternoonClose) > 0) {
					diff = GetTimeDiffSession(dt_st, SessionBreak.AfternoonClose) + GetTimeDiffSession(dt_en, SessionBreak.EveningOpen);
				}
			}
			else if((dt_st.DayOfWeek==DayOfWeek.Friday && dt_en.DayOfWeek==DayOfWeek.Sunday) || ((int)dt_st.DayOfWeek>(int)dt_en.DayOfWeek) || ((int)dt_st.DayOfWeek<(int)dt_en.DayOfWeek-1)) { // Fiday - Sunday or Cross to next Week or have day off trade
				diff = GetTimeDiffSession(dt_st, SessionBreak.AfternoonClose) + GetTimeDiffSession(dt_en, SessionBreak.EveningOpen);
			}
			else if((int)dt_st.DayOfWeek==(int)dt_en.DayOfWeek-1) { //Same day or next day
				if(CompareTimeWithSessionBreak(dt_st, SessionBreak.AfternoonClose) < 0) {
					diff = GetTimeDiffSession(dt_st, SessionBreak.AfternoonClose) + GetTimeDiffSession(dt_en, SessionBreak.NextDay);
				}
				else if(CompareTimeWithSessionBreak(dt_st, SessionBreak.AfternoonClose) > 0) { // dt_st passed evening open, no need to adjust
					diff = diff = dt_en.Subtract(dt_st).TotalMinutes;
				}
			}
			else {
				diff = dt_en.Subtract(dt_st).TotalMinutes;
			}
			} catch(Exception ex) {
				Print("GetTimeDiff ex:" + dt_st.ToString() + "--" + dt_en.ToString() + "--" + ex.Message);
				diff = 100;
			}
			return Math.Round(diff, 2);
		}

		public double GetMinutesDiff(DateTime dt_st, DateTime dt_en) {
			double diff = -1;
			TimeSpan ts = dt_en.Subtract(dt_st);
			diff = ts.TotalMinutes;
			return Math.Round(diff, 2);
		}
		
		public int CompareTimeWithSessionBreak(DateTime dt_st, SessionBreak sb) {
			DateTime dt = DateTime.Now;
			try {
			switch(sb) {
				case SessionBreak.AfternoonClose:
					dt = GetNewDateTime(dt_st.Year,dt_st.Month,dt_st.Day,16,0,0);
					break;
				case SessionBreak.EveningOpen:
					if(dt_st.Hour < 16)
						dt = GetNewDateTime(dt_st.Year,dt_st.Month,dt_st.Day-1,17,0,0);
					//if(dt_st.Hour >= 17)
					else dt = GetNewDateTime(dt_st.Year,dt_st.Month,dt_st.Day,16,0,0);
					break;
				default:
					dt = GetNewDateTime(dt_st.Year,dt_st.Month,dt_st.Day,16,0,0);
					break;
			}
			} catch(Exception ex) {
				Print("CompareTimeWithSessionBreak ex:" + dt_st.ToString() + "--" + sb.ToString() + "--" + ex.Message);
			}
			return dt_st.CompareTo(dt);
		}

		public double GetTimeDiffSession(DateTime dt_st, SessionBreak sb) {			
			DateTime dt_session = DateTime.Now;
			TimeSpan ts = dt_session.Subtract(dt_st);
			double diff = 100;
			try{
			switch(sb) {
				case SessionBreak.AfternoonClose:
					dt_session = GetNewDateTime(dt_st.Year, dt_st.Month, dt_st.Day, 16, 0, 0);
					ts = dt_session.Subtract(dt_st);
					break;
				case SessionBreak.EveningOpen:
					if(dt_st.Hour < 16)
						dt_session = GetNewDateTime(dt_st.Year,dt_st.Month,dt_st.Day-1,17,0,0);
					else 
						dt_session = GetNewDateTime(dt_st.Year, dt_st.Month, dt_st.Day, 17, 0, 0);
					ts = dt_st.Subtract(dt_session);
					break;
				case SessionBreak.NextDay:
					dt_session = GetNewDateTime(dt_st.Year, dt_st.Month, dt_st.Day-1, 17, 0, 0);
					ts = dt_st.Subtract(dt_session);
					break;
				default:
					dt_session = GetNewDateTime(dt_st.Year, dt_st.Month, dt_st.Day-1, 17, 0, 0);
					ts = dt_st.Subtract(dt_session);
					break;
			}
			diff = ts.TotalMinutes;
			} catch(Exception ex) {
				Print("GetTimeDiffSession ex:" + dt_st.ToString() + "--" + sb.ToString() + "--" + ex.Message);
			}
			
			return Math.Round(diff, 2);
		}
		
		public DateTime GetNewDateTime(int year, int month, int day, int hr, int min, int sec) {
			//DateTime(dt_st.Year,dt_st.Month,dt_st.Day,16,0,0);
			if(day == 0) {
				if(month == 1) {
					year = year-1;
					month = 12;
					day = 31;
				}
				else if (month == 3) {
					month = 2;
					day = 28;
				}
				else {
					month--;
					if(month == 1 || month == 3 || month == 5 || month == 7 || month == 8 || month == 10)
						day = 31;
					else day = 30;
				}
			}
			return new DateTime(year, month, day, hr, min, sec);
		}
		
		public string Get24HDateTime(DateTime dt) {
			return dt.ToString("MM/dd/yyyy HH:mm:ss");
		}

		/// <summary>
		/// if (ToDay(Time[0]) > 20140915)
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public int GetDateByDateTime(DateTime dt) {
			return ToDay(dt);
//			int date = dt.Year*10000 + dt.Month*100 + dt.Day;
//			return date;
		}

		public string GetDateStrByDateTime(DateTime dt) {
			return dt.ToString("yyyyMMdd");
		}
		
		/// <summary>
		/// if (ToTime(Time[0]) >= 74500 && ToTime(Time[0]) <= 134500)
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public int GetTimeByDateTime(DateTime dt) {
			return ToTime(dt);
		}
		
		//time=10000*H + 100*M + S
		public int GetTimeByHMS(int hour, int min, int second) {
			return ToTime(hour, min, second);
		}
		
		//time=10000*H + 100*M + S
		public int GetTimeByHM(int hour, int min, bool withSecond) {
			if(withSecond)
				return GetTimeByHMS(hour, min, 0);
			else
				return 100*hour + min;
		}
				
		public bool IsTimeInSpan(DateTime dt, int start, int end) {
			int t = 100*dt.Hour + dt.Minute;
			if(start <= t && t <= end) return true;
			else return false;
		}

		public int GetTimeDiffByHM(int hour, int min, DateTime dt) {
			int t = GetTimeByHM(hour, min, true);
			int t0 = GetTimeByHM(dt.Hour, dt.Minute, true);
			Print("[hour,min]=[" + hour + "," + min + "], [dt.Hour,dt.Minute]=[" + dt.Hour + "," + dt.Minute + "]," + dt.TimeOfDay);
			return t-t0;
		}
		
		public int GetTimeDiffByHM(int start_hour, int start_min, int end_hour, int end_min) {
			int t = GetTimeByHM(end_hour, end_min, true);
			int t0 = GetTimeByHM(start_hour, start_min, true);
			Print("[start_hour,start_min]=[" + start_hour + "," + start_min + "], [end_hour,end_min]=[" + end_hour + "," + end_min + "]");
			return t-t0;
		}
		
		public int GetBarNoByDateTime(DateTime dt) {
			return Bars.GetBar(dt);
		}

		/// <summary>
		/// Check if now is the time allowed to put trade
		/// </summary>
		/// <param name="time_start">start time</param>
		/// <param name="time_end">end time</param>
		/// <param name="session_start">the overnight session start time: 170000 for ES</param>
		/// <returns></returns>
//		public bool IsTradingTime(int time_start, int time_end, int session_start) {
//			int time_now = ToTime(Time[0]);
//			bool isTime= false;
//			if(time_start >= session_start) {
//				if(time_now >= time_start || time_now <= time_end)
//					isTime = true;
//			}
//			else if (time_now >= time_start && time_now <= time_end) {
//				isTime = true;
//			}
//			return isTime;
//		}
		
		/// <summary>
		/// Check if now is the first bar pass the startTime
		/// </summary>
		/// <param name="time_start">the overnight session start time: 170000 for ES</param>
		/// <returns></returns>
		public bool IsStartTimeBar(int time_start, int time_now, int time_lastbar) {
			Print(string.Format("{0}: time_now={1}, time_lastbar={2}, time_start={3}",
				CurrentBar, time_now, time_lastbar, time_start));
			bool isTime= false;
			if( time_lastbar < time_start && time_now >= time_start) {
					isTime = true;
			}
			return isTime;
		}
		
		/// <summary>
		/// Check if now is cutoff time
		/// </summary>		/// 
		/// <returns></returns>
		public bool IsCutoffTime(int bip, int time_h, int time_m) {
			bool isTime= false;
			int cutoff = GetTimeByHM(time_h, time_m, false);
			int t0 = GetTimeByHM(Times[bip][0].Hour, Times[bip][0].Minute, false);
			int t1 = GetTimeByHM(Times[bip][1].Hour, Times[bip][1].Minute, false);
			if(t0 >= cutoff && t1 < cutoff) {
				isTime = true;
				Print(string.Format("{0}: Cutoff time={1}",	CurrentBar, t0));
			}
			return isTime;
		}
				
		#endregion
	
		#region Properties
		[Description("Hour of opening start")]
 		[Range(0, 23), NinjaScriptProperty]		
		[Display(Name="OpenStartH", Order=ODI_OpenStartH, GroupName=GPI_TIMING)]
		public int TM_OpenStartH
		{ get; set; }

		[Description("Minute of opening start")]
		[Range(0, 59), NinjaScriptProperty]
		[Display(Name="OpenStartM", Order=ODI_OpenStartM, GroupName=GPI_TIMING)]
		public int TM_OpenStartM
		{ get; set; }

		[Description("Hour of opening end")]
 		[Range(0, 23), NinjaScriptProperty]		
		[Display(Name="OpenEndH", Order=ODI_OpenEndH, GroupName=GPI_TIMING)]
		public int TM_OpenEndH
		{ get; set; }

		[Description("Minute of opening end")]
		[Range(0, 59), NinjaScriptProperty]
		[Display(Name="OpenEndM", Order=ODI_OpenEndM, GroupName=GPI_TIMING)]
		public int TM_OpenEndM
		{ get; set; }
		
		[Description("Hour of closing")]
 		[Range(0, 23), NinjaScriptProperty]		
		[Display(Name="ClosingH", Order=ODI_ClosingH, GroupName=GPI_TIMING)]
		public int TM_ClosingH
		{ get; set; }

		[Description("Minute of closing")]
		[Range(0, 59), NinjaScriptProperty]
		[Display(Name="ClosingM", Order=ODI_ClosingM, GroupName=GPI_TIMING)]
		public int TM_ClosingM
		{ get; set; }
			
		#endregion
		
		#region Variables for Properties	
		private int tm_OpenStartH = 8; //Default setting for open Start hour
		private int tm_OpenStartM = 30; //Default setting for open Start minute		
        
		private int tm_OpenEndH = 10; //Default setting for open End hour
		private int tm_OpenEndM = 30; //Default setting for open End minute
		
		private int tm_ClosingH = 11; //Default setting for closing hour
		private int tm_ClosingM = 15; //Default setting for closing minute
		#endregion
	}
}