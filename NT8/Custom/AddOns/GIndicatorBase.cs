#region Using declarations
using System;
using System.IO;
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
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.AddOns;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	[Gui.CategoryOrder("CustomParams", 1)] // display "CP" first
	[Gui.CategoryOrder("GIndicator", 2)] // then "GStrategy"
	[Gui.CategoryOrder("OSI", 3)] // then "MM"
	[Gui.CategoryOrder("MA", 4)] // and then "TM"
	[Gui.CategoryOrder("Timming", 5)] // and finally "TG"
	
	/// <summary>
	/// It defined a set of interfaces to talk with strategy;
	/// The derived indicators should override the methods to be able be combined with other indicators; 
	/// </summary>
	public partial class GIndicatorBase : Indicator
	{
		private Series<double> CustomDataSeries1;
				
		#region Variables
        // User defined variables (add any user defined variables below)
		private string accName = ""; //account name from strategy, extracting simply string for print/log;
		protected string symbol = "";
		protected int printOut = 1;
		protected string logFile = ""; //Log file full path		
		private bool drawTxt = false; // Draw the text on chart
		protected Text it_gap = null; // The Text object drawn for bar
		
		protected bool backTest = true; //if it runs for backtesting;private bool backTest = true; //if it runs for backtesting;
		protected double Day_Count = 0;
		protected double Week_Count = 0;
		
		#endregion
		
		#region ZZ Vars
		/// <summary>
		/// Two Bar ZZ Swing ratio = curSize/prevSize
		/// </summary>
		protected List<double> ZZ_Ratio_0_6 = null;
		protected List<double> ZZ_Ratio_6_10 = null;
		protected List<double> ZZ_Ratio_10_16 = null;
		protected List<double> ZZ_Ratio_16_22 = null;
		protected List<double> ZZ_Ratio_22_30 = null;
		protected List<double> ZZ_Ratio_30_ = null;
		protected List<double> ZZ_Ratio = null;

		/// <summary>
		/// ZZ Swing sum for each day
		/// </summary>
		protected Dictionary<string,double> ZZ_Count_0_6 = null;
		protected Dictionary<string,double> ZZ_Count_6_10 = null;
		protected Dictionary<string,double> ZZ_Count_10_16 = null;
		protected Dictionary<string,double> ZZ_Count_16_22 = null;
		protected Dictionary<string,double> ZZ_Count_22_30 = null;
		protected Dictionary<string,double> ZZ_Count_30_ = null;
		protected Dictionary<string,double> ZZ_Count = null;
		
		/// <summary>
		/// ZZ Swing sum for each day
		/// </summary>
		protected Dictionary<string,double> ZZ_Sum_0_6 = null;
		protected Dictionary<string,double> ZZ_Sum_6_10 = null;
		protected Dictionary<string,double> ZZ_Sum_10_16 = null;
		protected Dictionary<string,double> ZZ_Sum_16_22 = null;
		protected Dictionary<string,double> ZZ_Sum_22_30 = null;
		protected Dictionary<string,double> ZZ_Sum_30_ = null;
		protected Dictionary<string,double> ZZ_Sum = null;
		
//		protected double ZZ_Avg_Daily_Count;
//		protected double ZZ_Avg_Daily_Sum;
//		protected double ZZ_Avg_Weekly_Count;
//		protected double ZZ_Avg_Weekly_Sum;
		#endregion
		
        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
//        protected void Initialize()
//        {
            //Add(new Plot(Color.FromKnownColor(KnownColor.Black), PlotStyle.Block, "StartHM"));
            //Add(new Plot(Color.FromKnownColor(KnownColor.DarkBlue), PlotStyle.Block, "EndHM"));
            //Overlay				= true;
//			accName = "";//GetTsTAccName(Strategy.Account.Name);
//			IndicatorSignal indicatorSignal = new IndicatorSignal();
//        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
//        protected void OnBarUpdate()
//        {
            // Use this method for calculating your indicator values. Assign a value to each
            // plot below by replacing 'Close[0]' with your own formula.
//			if(CurrentBar >= BarsRequired) {
//				if(GetTimeDiffByHM(StartH, StartM, Time[1]) > 0 && GetTimeDiffByHM(StartH, StartM, Time[0]) <= 0)
//            		StartHM.Set(High[0]+0.25);
//				if(GetTimeDiffByHM(EndH, EndM, Time[1]) > 0 && GetTimeDiffByHM(EndH, EndM, Time[0]) <= 0)
//            		EndHM.Set(Low[0]-0.25);
//			}
//        }
		
		public int IsLastBarOnChart() {
			try{				
//				if(CurrentBar < Count - 1)
//					return -1;
				if(CurrentBar < Count - 2)
				//if(Inputs[0].Count - Bars.CurrentBar <= 2) 
				{
					return -1;					
				} else {
					Print("IsLastBarOnChart called:(CurBar,Count)=" + CurrentBar + "," + Count);
					return Count;//Inputs[0].Count;
				}
		
			} catch(Exception ex){
				//Print("IsLastBarOnChart:" + ex.Message);
				return -1;
			}
		}
		
		public void DayWeekMonthCount() {
			if(Bars.CurrentBar < BarsRequiredToPlot) return;
			if(Time[0].Day != Time[1].Day) {
				Day_Count ++;
			}
			if(Time[0].DayOfWeek == DayOfWeek.Sunday &&  Time[1].DayOfWeek != DayOfWeek.Sunday) {
				Week_Count ++;
			}
			
		}
		
		#region Search Functions
		
		public int GetInflection(ISeries<double> d){
			int inft = 0;//inflection[0] = 0;

			if(d[1].ApproxCompare(d[0]) > 0 && d[1].ApproxCompare(d[2]) > 0) 
				inft = -1;//inflection[1] = 1;
			else if(d[1].ApproxCompare(d[0]) < 0 && d[1].ApproxCompare(d[2]) < 0)
				inft = 1;//inflection[1] = -1;
			//Print("inft=" + (CurrentBar-1).ToString() + "," + inft);
			return inft;//inflection[1];
		}
		
		public int GetLastBar4Inflection(Series<int> inflection, int curBar) {
			int n = 0;
			for(int i=1; i>curBar-BarsRequiredToPlot; i--) {
				if (inflection[i] != 0)
					n = curBar - i;
			}
			return n;
		}
		
		/// <summary>
		/// Get the last inflection for the given barNo
		/// </summary>
		/// <param name="barNo"></param>
		/// <returns>the number of bars ago/barNo for last inflection</returns>
		public int GetLastInflection(Series<int> inflection, int barNo, TrendDirection dir, BarIndexType barIdxType) {
			int inft = -1;
			for(int i = 1; i<barNo-BarsRequiredToPlot-1; i++) {				
				if((inflection[i]<0 && dir == TrendDirection.Down) ||
					(inflection[i]>0 && dir == TrendDirection.Up))
					inft = i;
				if(barNo >= Input.Count)
					Print("inflection[" + i + "]=" + inflection[i] + ", inft=" + inft);
			}
			if(inft > 0 && barIdxType == BarIndexType.BarNO)
				inft = CurrentBar - inft;
			//Print("GetLastInflection barNo, currentBar, inft=" + barNo + "," + CurrentBar + "," + inft);
			return inft;
		}

		/// <summary>
		/// Get the last crossover for the given barNo
		/// </summary>
		/// <param name="barNo"></param>
		/// <returns>the number of bars ago for last crossover</returns>
		public int GetLastCrossover(Series<int> crossover, int barNo, CrossoverType crsType, BarIndexType barIdxType) {
			int crsov = -1;
			for(int i = 1; i<barNo-BarsRequiredToPlot-1; i++) {
				if((crossover[i]>0 && crsType == CrossoverType.Above) ||
					(crossover[i]<0 && crsType == CrossoverType.Below) ||
					(crossover[i] != 0 && crsType == CrossoverType.Both))
					crsov = i;
			}
			if(barIdxType == BarIndexType.BarNO)
				crsov = CurrentBar - crsov;
			return crsov;
		}
		
		public int GetLastIndexRecord(GLastIndexRecorder<double> rec, int barNo, BarIndexType barIdxType, LookbackBarType lbBarType) {
			int idx = -1;
			
			return idx;
		}
		
		#endregion
		
		#region Pattern Functions
		/// <summary>
		/// Check the first reversal bar for the pullback under current ZigZag gap
		/// </summary>
		/// <param name="cur_gap">current ZigZag gap</param>
		/// <param name="tick_size">tick size of the symbol</param>
		/// <param name="n_bars">bar count with the pullback prior to the last reversal bar</param>
		/// <returns>is TBR or not</returns>
		public bool IsTwoBarReversal(double cur_gap, double tick_size, int n_bars) {
			bool isTBR= false;
			
			if(n_bars < 0) return isTBR;
			
			if(cur_gap > 0)
			{
				//Check if the last n_bars are pullback bars (bear bar)
				for(int i=1; i<=n_bars; i++)
				{
					if(Close[i] > Open[i])
						return isTBR;
				}
				if(Close[0]-Open[0] > tick_size && Open[1]-Close[1] >= tick_size) {
					isTBR= true;
				}
			}
			else if(cur_gap < 0)
			{
				//Check if the last n_bars are pullback bars (bull bar)
				for(int i=1; i<=n_bars; i++)
				{
					if(Close[i] < Open[i])
						return isTBR;
				}
				if(Open[0] - Close[0] > tick_size && Close[1] - Open[1] >= tick_size) {
					isTBR= true;
				}
			}
			return isTBR;
		}
		
		/// <summary>
		/// Get the Two Bar Reversal count for the past barsBack
		/// </summary>
		/// <param name="cur_gap">current ZigZag gap</param>
		/// <param name="tick_size">tick size of the symbol</param>
		/// <param name="barsBack">bar count to look back</param>
		/// <returns>pairs count for TBR during the barsBack</returns>
		public int GetTBRPairsCount(double cur_gap, double tick_size, int barsBack) {
			int tbr_count = 0;
			for(int i=0; i<barsBack; i++) {
				if(cur_gap > 0 && Close[i]-Open[i] > tick_size && Open[i+1]-Close[i+1] >= tick_size) {
					tbr_count++;
				}
				if(cur_gap < 0 && Open[i] - Close[i] > tick_size && Close[i+1] - Open[i+1] >= tick_size) {
					tbr_count++;
				}
			}
			return tbr_count;
		}

		protected void SaveTwoBarRatio(ZigZagSwing zzS){
			if( ZZ_Ratio_0_6 == null) 
				ZZ_Ratio_0_6 = new List<double>();
			if( ZZ_Ratio_6_10 == null) 
				ZZ_Ratio_6_10 = new List<double>();
			if( ZZ_Ratio_10_16 == null) 
				ZZ_Ratio_10_16 = new List<double>();
			if( ZZ_Ratio_16_22 == null) 
				ZZ_Ratio_16_22 = new List<double>();
			if( ZZ_Ratio_22_30 == null) 
				ZZ_Ratio_22_30 = new List<double>();
			if( ZZ_Ratio_30_ == null) 
				ZZ_Ratio_30_ = new List<double>();
			if( ZZ_Ratio == null) 
				ZZ_Ratio = new List<double>();

			double zzSizeAbs = Math.Abs(zzS.Size);
			if(zzSizeAbs > 0 && zzSizeAbs <6){
				ZZ_Ratio_0_6.Add(zzS.TwoBar_Ratio);
			}
			else if(zzSizeAbs >= 6 && zzSizeAbs <10){
				ZZ_Ratio_6_10.Add(zzS.TwoBar_Ratio);
			}
			else if(zzSizeAbs >= 10 && zzSizeAbs <16){
				ZZ_Ratio_10_16.Add(zzS.TwoBar_Ratio);
			}
			else if(zzSizeAbs >= 16 && zzSizeAbs <22){
				ZZ_Ratio_16_22.Add(zzS.TwoBar_Ratio);
			}
			else if(zzSizeAbs >= 22 && zzSizeAbs <30){
				ZZ_Ratio_22_30.Add(zzS.TwoBar_Ratio);
			}
			else if(zzSizeAbs >= 30){
				ZZ_Ratio_30_.Add(zzS.TwoBar_Ratio);
			}
			if(zzS.Size != 0) {
				ZZ_Ratio.Add(zzS.TwoBar_Ratio);
			}
		}
		
		protected void PrintTwoBarRatio(){
			if( ZZ_Ratio_0_6 != null) {
				Print("========ZZ_Ratio_0_6 count=" + ZZ_Ratio_0_6.Count + "=========");
				foreach(double val in ZZ_Ratio_0_6) {
					Print(val + "\r");
				}
			}
			if( ZZ_Ratio_6_10 != null) {
				Print("========ZZ_Ratio_6_10 count=" + ZZ_Ratio_6_10.Count + "=========");
				foreach(double val in ZZ_Ratio_6_10) {
					Print(val + "\r");
				}
			}
			if( ZZ_Ratio_10_16 != null) {
				Print("========ZZ_Ratio_10_16 count=" + ZZ_Ratio_10_16.Count + "=========");
				foreach(double val in ZZ_Ratio_10_16) {
					Print(val + "\r");
				}
			}
			if( ZZ_Ratio_16_22 != null) {
				Print("========ZZ_Ratio_16_22 count=" + ZZ_Ratio_16_22.Count + "=========");
				foreach(double val in ZZ_Ratio_16_22) {
					Print(val + "\r");
				}
			}
			if( ZZ_Ratio_22_30 != null) {
				Print("========ZZ_Ratio_22_30 count=" + ZZ_Ratio_22_30.Count + "=========");
				foreach(double val in ZZ_Ratio_22_30) {
					Print(val + "\r");
				}
			}
			if( ZZ_Ratio_30_ != null) {
				Print("========ZZ_Ratio_30_ count=" + ZZ_Ratio_30_.Count + "=========");
				foreach(double val in ZZ_Ratio_30_) {
					Print(val + "\r");
				}
			}
			if( ZZ_Ratio != null) {
				Print("========ZZ_Ratio count=" + ZZ_Ratio.Count + "=========");
				foreach(double val in ZZ_Ratio) {
					Print(val + "\r");
				}
			}
		}
		
		#endregion
		
		#region File and Dict functions
		
		public string GetFileNameByDateTime(DateTime dt, string path, string accName, string symbol, string ext) {
			Print("GetFileNameByDateTime: " + dt.ToString());
			//path = "C:\\inetpub\\wwwroot\\nt_files\\log\\";
			//ext = "log";
			long flong = DateTime.Now.Minute + 100*DateTime.Now.Hour+ 10000*DateTime.Now.Day + 1000000*DateTime.Now.Month + (long)100000000*DateTime.Now.Year;
			string fname = path + accName + Path.DirectorySeparatorChar + accName + "_" + symbol + "_" + flong.ToString() + "." + ext;
			//Print(", FileName=" + fname);
			//FileTest(DateTime.Now.Minute + 100*DateTime.Now.Hour+ 10000*DateTime.Now.Day+ 1000000*DateTime.Now.Month + (long)100000000*DateTime.Now.Year);

		 	//if(barNo > 0) return;
//			FileStream F = new FileStream(fname, FileMode.OpenOrCreate, FileAccess.ReadWrite);
			
//			using (System.IO.StreamWriter file = 
//				new System.IO.StreamWriter(@fname, true))
//			{
//				for (int i = 0; i <= 3; i++) {
//					file.WriteLine("Line " + i + ":" + i);
//				}
//			}
			return fname;
		}

		public string GetDictKeyByDateTime(DateTime dt, string prefix, string sufix) {
			string kname = prefix + "_" + dt.Year + "-" + dt.Month + "-" + dt.Day + "_" + sufix;
			//Print("GetDictKeyByDateTime: " + dt.ToString() + ", DictKey=" + kname);
			return kname;
		}
		
		public bool AddDictVal(Dictionary<string,double> dict, string key, double val) {
			double dict_val;
			if(dict.TryGetValue(key,out dict_val)) {
				dict[key] = dict_val + val;
			} else {
				dict.Add(key, val);
			}
			return true;
		}
		
		public double SumDictVal(Dictionary<string,double> dict) {
			double sum=0;
			foreach(var item in dict){
				//Print("SumDictVal:" + item.Key);
				sum = sum + item.Value;
			}

			return sum;
		}
		
		#endregion

		public String GetTsTAccName(String tst_acc) {
			char[] delimiterChars = {'!'};
			string[] words = tst_acc.Split(delimiterChars);
			return words[0];
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

		/// <summary>
		/// Draw Gap from last ZZ to current bar
		/// </summary>
		/// <param name="y_base">the base for y axis</param>
		/// <param name="y_offset">the offset for y axis</param>
		/// <param name="zzGap">the gap size to draw text</param>
		/// <returns></returns>
		public Text DrawGapText(double zzGap, string tag, int bars_ago, double y_base, double y_offset)
		{
			Text gapText = null;
			double y = 0;
			int barNo = CurrentBar-bars_ago;
			Brush up_color = new SolidColorBrush(Color.FromRgb(0, 255, 0));//Color.Green;
			Brush dn_color = new SolidColorBrush(Color.FromRgb(255, 0, 0));//Color.Red;
			Brush sm_color = new SolidColorBrush(Color.FromRgb(0, 0, 0));//Color.Black;
			Brush draw_color = sm_color;
			if(zzGap > 0) {
				draw_color = up_color;
//				y = double.Parse(Low[0].ToString())-1 ;
				y = y_base-y_offset ;
			}
			else if (zzGap < 0) {
				draw_color = dn_color;
//				y = double.Parse(High[0].ToString())+1 ;
				y = y_base+y_offset ;
			}
			Print(barNo + "-" + Time[bars_ago] + ": y=" + y + ", zzGap=" + zzGap);
			gapText = Draw.Text(this, tag+barNo.ToString(), GetTimeDate(Time[bars_ago], 1)+"\r\n#"+barNo+"\r\nZ:"+zzGap, bars_ago, y, draw_color);
			//DrawText(tag+barNo.ToString(), GetTimeDate(Time[bars_ago], 1)+"\r\n#"+barNo+"\r\nZ:"+zzGap, bars_ago, y, draw_color);
//			}
			if(gapText != null) gapText.IsLocked = false;
			//if(printOut > 0)
				//PrintLog(true, log_file, CurrentBar + "::" + this.ToString() + " GaP= " + gap + " - " + Time[0].ToShortTimeString());
			return gapText; 
		}
		
		public void PrintLog(bool prt_con, bool prt_file, string text) {
			string fpath = GetLogFile();
			//Print("PrintLog: " + fpath);
			if(prt_con) Print(text); // return;
			if(prt_file) {
				using (System.IO.StreamWriter file = 
					new System.IO.StreamWriter(@fpath, true))
				{
					file.WriteLine(text);
				}
			}
		}				
		
        #region Properties
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> StartHM
        {
            get { return Values[0]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> EndHM
        {
            get { return Values[1]; }
        }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="CustomColor1", Description="Color-1", Order=0, GroupName="GIndicator")]
		public Brush CustomColor1
		{ get; set; }

		[Browsable(false)]
		public string CustomColor1Serializable
		{
			get { return Serialize.BrushToString(CustomColor1); }
			set { CustomColor1 = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="CustomPrc1", Description="CustomPrc-1", Order=1, GroupName="GIndicator")]
		public double CustomPrc1
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="CustomStr1", Description="CustomStr-1", Order=2, GroupName="GIndicator")]
		public string CustomStr1
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="CustomTime1", Description="CustomTime-1", Order=3, GroupName="GIndicator")]
		public DateTime CustomTime1
		{ get; set; }
		#endregion
		
		#region Other Properties
		/// <summary>
		/// The symbol of the instument
		/// </summary>
		/// <returns></returns>
        public string GetSymbol()
        {
            return Instrument.FullName;
        }
		
		/// <summary>
		/// If it runs for backtesting
		/// </summary>
		/// <returns></returns>
        public bool IsBackTest()
        {
            return backTest;
        }
		
		public void SetBackTest(bool back_test)
        {
           backTest = back_test;
        }
		
		/// <summary>
		/// The log file name/path
		/// </summary>
		/// <returns></returns>
        public string GetLogFile()
        {
            return logFile;
        }
		
		public void SetLogFile(string log_file)
        {
           logFile = log_file;
        }
		
		/// <summary>
		/// The print out level
		/// </summary>
		/// <returns></returns>
        public int GetPrintOut()
        {
            return printOut;
        }
		public void SetPrintOut(int print_out)
        {
           printOut = print_out;
        }
		
		/// <summary>
		/// If it draws text on the chart
		/// </summary>
		/// <returns></returns>
        public bool IsDrawTxt()
        {
            return drawTxt;
        }
		public void SetDrawTxt(bool draw_txt)
        {
           drawTxt = draw_txt;
        }
		#endregion		
	}	
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIndicatorBase[] cacheGIndicatorBase;
		public GIndicatorBase GIndicatorBase()
		{
			return GIndicatorBase(Input);
		}

		public GIndicatorBase GIndicatorBase(ISeries<double> input)
		{
			if (cacheGIndicatorBase != null)
				for (int idx = 0; idx < cacheGIndicatorBase.Length; idx++)
					if (cacheGIndicatorBase[idx] != null &&  cacheGIndicatorBase[idx].EqualsInput(input))
						return cacheGIndicatorBase[idx];
			return CacheIndicator<GIndicatorBase>(new GIndicatorBase(), input, ref cacheGIndicatorBase);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIndicatorBase GIndicatorBase()
		{
			return indicator.GIndicatorBase(Input);
		}

		public Indicators.GIndicatorBase GIndicatorBase(ISeries<double> input )
		{
			return indicator.GIndicatorBase(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIndicatorBase GIndicatorBase()
		{
			return indicator.GIndicatorBase(Input);
		}

		public Indicators.GIndicatorBase GIndicatorBase(ISeries<double> input )
		{
			return indicator.GIndicatorBase(input);
		}
	}
}

#endregion



