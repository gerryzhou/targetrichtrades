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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;

using NinjaTrader.NinjaScript.Indicators;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class StgChartTrader : GStrategyBase
	{
		public GIChartTrader giChartTrader;
		
		private const string tagHiLoPriceTextField = "tag-HiLoPriceTextField";
		private const string tagLoPriceArrow = "tag-LoPriceArrow";
		private const string tagHiPriceArrow = "tag-HiPriceArrow";
		private const string tagInfoTextField = "tag-InfoTextField";
		
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				Description									= @"Use Customized Chart Trader.";
				Name										= "StgChartTrader";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				IsUnmanaged									= false;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded) {
				giChartTrader = GIChartTrader(this.Input);				
				AddChartIndicator(giChartTrader);				
				giChartTrader.RaiseIndicatorEvent += OnTradeByChartTrader;
			}
		}

		protected override void OnBarUpdate()
		{
			UpdateStopEnBar();
			UpdateHiLoPrice();
		}

		protected void OnTradeByChartTrader(object sender, IndicatorEventArgs e) {
			this.Update();
			IndicatorSignal isig = e.IndSignal;
			Print(String.Format("{0}:OnTradeByChartTrader {1} Bip={2}, CurrentBar[0]={3}, DrawingTool.GetCurrentBar={4}, \r\n Bars.GetTime(CurrentBars[0])={5}, Time[BarsInProgress][0]={6}, Time[0][0]={7}",
			CurrentBars[BarsInProgress], isig.SignalName, BarsInProgress, CurrentBars[0], DrawingTool.GetCurrentBar(this), Bars.GetTime(CurrentBars[0]), Times[BarsInProgress][0], Times[0][0]));
			if(isig.SignalAction != null) {
				if(isig.SignalAction.SignalActionType == SignalActionType.BarToLeft)
					StopEnBarIndex = StopEnBarIndex < 1? CurrentBars[0] : StopEnBarIndex-1;
				else if(isig.SignalAction.SignalActionType == SignalActionType.BarToRight)
					StopEnBarIndex = StopEnBarIndex > CurrentBars[0]? 1 : StopEnBarIndex+1;
			}
			UpdateStopEnBar();
			UpdateHiLoPrice();
		}
		
		private void UpdateStopEnBar() {
			if(StopEnBarIndex < 0) return;
			//Draw.ArrowUp(this, "tag1", true, 0, Lows[0][0] - TickSize, Brushes.Red);
			RemoveDrawObject(tagLoPriceArrow);
			RemoveDrawObject(tagInfoTextField);
			Draw.Diamond(this, tagLoPriceArrow, true, Bars.GetTime(StopEnBarIndex), Bars.GetLow(StopEnBarIndex) - TickSize, Brushes.Yellow);
			//Draw.Diamond(this, "tag1", true, 0, Low[0] - TickSize, Brushes.Red);
			//giChartTrader.SetStopPrice(Bars.GetLow(StopEnBarIndex).ToString());
			Draw.TextFixed(this, tagInfoTextField, "Button ?\r\n Clicked", TextPosition.BottomLeft, Brushes.Green, 
				new Gui.Tools.SimpleFont("Arial", 12), Brushes.Transparent, Brushes.Transparent, 100);
		}
		
		private void UpdateHiLoPrice() {
			String strPrint = string.Format("StopEnBarIndex={0}", StopEnBarIndex);
			if(StopEnBarIndex >= 0) {
				int barsBack = CurrentBars[0] - StopEnBarIndex;
				strPrint = string.Format("Hi[{0}]={1} \r\nLo[{2}]={3}",
					barsBack, Bars.GetHigh(StopEnBarIndex), barsBack, Bars.GetLow(StopEnBarIndex));
			}
				
			//Draw.ArrowUp(this, "tag1", true, 0, Lows[0][0] - TickSize, Brushes.Red);
			//Draw.Diamond(this, "tag1", true, Bars.GetTime(StopEnBarIndex), Bars.GetLow(StopEnBarIndex) - TickSize, Brushes.Red);
			//Draw.Diamond(this, "tag1", true, 0, Low[0] - TickSize, Brushes.Red);
			//giChartTrader.SetStopPrice(Bars.GetLow(StopEnBarIndex).ToString());
			RemoveDrawObject(tagHiLoPriceTextField);
			Draw.TextFixed(this, tagHiLoPriceTextField, strPrint, TextPosition.BottomRight, Brushes.Blue, 
				new Gui.Tools.SimpleFont("Arial", 12), Brushes.Transparent, Brushes.Transparent, 100);
		}
		
		private void EntryStopBuy() {
			NewEntrySimpleOrderMG();
		}
		
		#region Properties
		private int StopEnBarIndex = -1;
		#endregion
	}
}
