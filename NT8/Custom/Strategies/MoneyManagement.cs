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
using NinjaTrader.NinjaScript.Indicators.ZTraderInd;
using NinjaTrader.NinjaScript.Strategies.ZTraderStg;
using NinjaTrader.NinjaScript.DrawingTools;

#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class GStrategyBase : Strategy
	{		
		#region Money Mgmt Functions
		
		public double CalProfitTargetAmt(double price, double profitFactor) {
			IndicatorProxy.PrintLog(true, true, 
				CurrentBar + ":CalProfitTargetAmt;IsLiveTrading=" + IsLiveTrading() +
				";=GetMarketPosition()" + GetMarketPosition().ToString() +
				";CurrentTrade.SLCalculationMode=" + CurrentTrade.SLCalculationMode.ToString() +
				";avgPrice=" + price +
				";CurrentTrade.TradeAction.StopLossPrice=" + CurrentTrade.TradeAction.StopLossPrice +
				";CurrentTrade.TradeAction.ProfitTargetPrice=" + CurrentTrade.TradeAction.ProfitTargetPrice
				);			
			switch(CurrentTrade.SLCalculationMode) {
				case CalculationMode.Currency:
					if(profitFactor == 0)
						CurrentTrade.profitTargetAmt = MM_ProfitTargetAmt;
					else
						CurrentTrade.profitTargetAmt = 
							profitFactor*CurrentTrade.stopLossAmt;
					break;
				case CalculationMode.Price:
					if(profitFactor == 0)
						CurrentTrade.profitTargetAmt = MM_ProfitTargetAmt;			
					else
						CurrentTrade.profitTargetAmt = 
							profitFactor*Math.Abs(price-CurrentTrade.TradeAction.StopLossPrice)*Instrument.MasterInstrument.PointValue;
					break;
			}
			IndicatorProxy.PrintLog(true, true, 
				CurrentBar + ":CalProfitTargetAmt;IsLiveTrading=" + IsLiveTrading() +
				";=GetMarketPosition()" + GetMarketPosition().ToString() +
				";CurrentTrade.PTCalculationMode=" + CurrentTrade.PTCalculationMode.ToString() +
				";avgPrice=" + price +
				";CurrentTrade.TradeAction.StopLossPrice=" + CurrentTrade.TradeAction.StopLossPrice +
				";CurrentTrade.TradeAction.ProfitTargetPrice=" + CurrentTrade.TradeAction.ProfitTargetPrice
				);	
			return 0;
		}

		/// <summary>
		/// Calculation of price for unmanaged OCO exit order
		/// </summary>
		/// <param name="avgPrice">Avg entry price</param>
		/// <param name="profitFactor">PT/SL>0</param>
		public void CalExitOcoPrice(double avgPrice, double profitFactor) {
			int prtLevel = 0;
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
			IndicatorProxy.PrintLog(true, true, 
				CurrentBar + ":CalExitOcoPrice;IsLiveTrading=" + IsLiveTrading() +
				";=GetMarketPosition()" + GetMarketPosition().ToString() +
				";CurrentTrade.SLCalculationMode=" + CurrentTrade.SLCalculationMode.ToString() +
				";avgPrice=" + avgPrice +
				";CurrentTrade.TradeAction.StopLossPrice=" + CurrentTrade.TradeAction.StopLossPrice +
				";CurrentTrade.TradeAction.ProfitTargetPrice=" + CurrentTrade.TradeAction.ProfitTargetPrice
				);
			switch(CurrentTrade.SLCalculationMode) {
				case CalculationMode.Currency:
					if(profitFactor > 0)
						CurrentTrade.profitTargetAmt = 
							profitFactor*CurrentTrade.stopLossAmt;
					if(GetMarketPosition() == MarketPosition.Long) {
						CurrentTrade.TradeAction.StopLossPrice = avgPrice - GetPriceByCurrency(CurrentTrade.stopLossAmt);
						CurrentTrade.TradeAction.ProfitTargetPrice = avgPrice + GetPriceByCurrency(CurrentTrade.profitTargetAmt);
					}
					else if(GetMarketPosition() == MarketPosition.Short) {
						CurrentTrade.TradeAction.StopLossPrice = avgPrice + GetPriceByCurrency(CurrentTrade.stopLossAmt);
						CurrentTrade.TradeAction.ProfitTargetPrice = avgPrice - GetPriceByCurrency(CurrentTrade.profitTargetAmt);
					}
					break;
				case CalculationMode.Ticks:
					if(profitFactor > 0)
						CurrentTrade.profitTargetTic = 
							(int)(profitFactor*CurrentTrade.stopLossTic);
					if(GetMarketPosition() == MarketPosition.Long) {
						CurrentTrade.TradeAction.StopLossPrice = avgPrice - GetPriceByTicks(CurrentTrade.stopLossTic);
						CurrentTrade.TradeAction.ProfitTargetPrice = avgPrice + GetPriceByTicks(CurrentTrade.profitTargetTic);
					}
					else if(GetMarketPosition() == MarketPosition.Short) {
						CurrentTrade.TradeAction.StopLossPrice = avgPrice + GetPriceByTicks(CurrentTrade.stopLossTic);
						CurrentTrade.TradeAction.ProfitTargetPrice = avgPrice - GetPriceByTicks(CurrentTrade.profitTargetTic);
					}					
					break;
				case CalculationMode.Price:
					if(profitFactor > 0)
						CurrentTrade.profitTargetAmt = 
							profitFactor*Math.Abs(avgPrice-CurrentTrade.TradeAction.StopLossPrice)*Instrument.MasterInstrument.PointValue;
					break;
			}
			IndicatorProxy.Log2Disk = true;
			IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				CurrentBar + ":CalExitOcoPrice" +
				";=GetMarketPosition()" + GetMarketPosition().ToString() +
				";CurrentTrade.SLCalculationMode=" + CurrentTrade.SLCalculationMode.ToString() +
				";avgPrice=" + avgPrice +
				";CurrentTrade.TradeAction.StopLossPrice=" + CurrentTrade.TradeAction.StopLossPrice +
				";CurrentTrade.TradeAction.ProfitTargetPrice=" + CurrentTrade.TradeAction.ProfitTargetPrice
				);
		}

		/// <summary>
		/// Calculation of price for unmanaged trailing stoploss order
		/// </summary>
		/// <param name="avgPrice">Avg entry price</param>
		public virtual void CalTLSLPrice(double avgPrice, int pl_tics) {
			int prtLevel = 0;
			IndicatorProxy.TraceMessage(this.Name, prtLevel);

//			if(pl_tics >= (CurrentTrade.profitLockMaxTic+2*CurrentTrade.profitTgtIncTic))
//				overAmt = pl_tics - CurrentTrade.profitLockMaxTic;
			
			switch(CurrentTrade.TLSLCalculationMode) {
				case CalculationMode.Currency:

					break;
				case CalculationMode.Ticks:
					
					break;
				case CalculationMode.Percent:
					throw new Exception("CalculationMode.Percent for trailing stoploss not supported yet!");
					break;
			}
			IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				CurrentBar + ":CalTLSLPrice-Pre"
			+ ";TLSLCalculationMode=" + CurrentTrade.TLSLCalculationMode
			+ ";trailingSLPrice=" + CurrentTrade.TradeAction.StopLossPrice
			+ ";trailingPTTic=" + CurrentTrade.TradeAction.TrailingProfitTargetTics
			+ ";trailingSLTic=" + CurrentTrade.trailingSLTic
			+ ";avgPrice=" + avgPrice);
			
			if(CurrentTrade.TradeAction.TrailingProfitTargetTics < CurrentTrade.profitLockMaxTic)
				CurrentTrade.TradeAction.TrailingProfitTargetTics = Math.Max(CurrentTrade.profitLockMaxTic, pl_tics-CurrentTrade.trailingSLTic);
			else
				CurrentTrade.TradeAction.TrailingProfitTargetTics = Math.Max(CurrentTrade.TradeAction.TrailingProfitTargetTics, pl_tics-CurrentTrade.trailingSLTic);
			
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
			if(GetMarketPosition() == MarketPosition.Long) {
				CurrentTrade.TradeAction.StopLossPrice = MovePriceByTicks(avgPrice, CurrentTrade.TradeAction.TrailingProfitTargetTics);
			}
			else if(GetMarketPosition() == MarketPosition.Short) {
				CurrentTrade.TradeAction.StopLossPrice = MovePriceByTicks(avgPrice, -CurrentTrade.TradeAction.TrailingProfitTargetTics);
			}
			
			IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":CalTLSLPrice"
			+ ";TLSLCalculationMode=" + CurrentTrade.TLSLCalculationMode
			+ ";trailingSLPrice=" + CurrentTrade.TradeAction.StopLossPrice
			+ ";trailingPTTic=" + CurrentTrade.TradeAction.TrailingProfitTargetTics
			+ ";avgPrice=" + avgPrice);
		}
		
		/// <summary>
		/// For position exit OCO order adjustment
		/// breakEvenAmt<MinLockPT<ProfitTargetAmt<MaxLockPT
		/// breakEvenAmt<MinLockPT+2*SLIncTic<ProfitTargetAmt+2*PTIncTic<MaxLockPT
		/// ** Check trailing stop loss if PnL>MaxLockPT
		///   * Cancel OCO order
		///   * Create new TLSL order, trailing price and validate it
		/// 
		/// ** Check Lock MinStoploss: MinLock<PnL<MaxLock
		///   * Keep moving target away;
		///   * Lock stop loss to MinLockPT
		///   
		/// ** Check BreakEven:
		///   * If PnL>breakEvenAmt, set SL to breakeven;
		///   * Keep target no change;
		///
		/// </summary>
		/// <returns></returns>
		public bool ChangeSLPT()
		{
			int prtLevel = 0;
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
			double pl = CheckUnrealizedPnL();
			int pl_tics = CheckUnrealizedPnLTicks();
			double avgPrc = GetAvgPrice();
			GetExitOrderType(pl, pl_tics);
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
			
			switch(CurrentTrade.BracketOrder.ExitOrderType) {
				case EntryExitOrderType.TrailingStopLoss: // start trailing SL
					IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":isOverMaxLockPT, pl_tics=" + pl_tics);
					IndicatorProxy.TraceMessage(this.Name, prtLevel);
					CalTLSLPrice(avgPrc, pl_tics);
					SetTrailingStopLossOrder(CurrentTrade.TradeAction.EntrySignal.SignalName.ToString());
					break;
				case EntryExitOrderType.LockMinProfit: // move PT, lock SL at MinPT
					IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":isOverMinLockPT, pl_tics=" + pl_tics);
					IndicatorProxy.TraceMessage(this.Name, prtLevel);
					LockMinProfitTarget(avgPrc);
					break;
				case EntryExitOrderType.BreakEven: // PT no change, BE SL
					IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":isOverBreakeven, pl_tics=" + pl_tics);
					IndicatorProxy.TraceMessage(this.Name, prtLevel);
					SetBreakEvenOrder(avgPrc);
					break;
				case EntryExitOrderType.SimpleOCO: // set simple PT, SL
					IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":isBelowBreakeven, pl_tics=" + pl_tics);
					IndicatorProxy.TraceMessage(this.Name, prtLevel);
					SetSimpleExitOCO(CurrentTrade.TradeAction.EntrySignal.SignalName);
					break;
			}
			
			return false;
		}
		
		/// <summary>
		/// Trail PT after PnL>MinLockPT
		/// Using tics to record the current trailing PT;
		/// Using price to setup PT order;
		/// </summary>
		public void MoveProfitTarget(double avgPrc) {
			//if(!CurrentTrade.ptTrailing) return;
			if(GetCurrencyByTicks(CurrentTrade.TradeAction.TrailingProfitTargetTics) <= CurrentTrade.profitTargetAmt) { //first time move the profit target
				CurrentTrade.TradeAction.TrailingProfitTargetTics = GetTicksByCurrency(CurrentTrade.profitTargetAmt) + CurrentTrade.profitTgtIncTic;
			}
			else {
				double pl_tics = CheckUnrealizedPnLTicks();
				if(pl_tics >= (CurrentTrade.TradeAction.TrailingProfitTargetTics - 2*CurrentTrade.profitTgtIncTic)) {
					CurrentTrade.TradeAction.TrailingProfitTargetTics = CurrentTrade.TradeAction.TrailingProfitTargetTics + CurrentTrade.profitTgtIncTic;
					//CurrentTrade.TradeAction.ProfitTargetPrice = MovePriceByTicks(avgPrc, CurrentTrade.profitTgtIncTic);
				}
			}
			
			double newPrc;
			if(GetMarketPosition() == MarketPosition.Long) {
				newPrc = MovePriceByTicks(avgPrc, CurrentTrade.TradeAction.TrailingProfitTargetTics);
				CurrentTrade.TradeAction.ProfitTargetPrice = Math.Max(newPrc, CurrentTrade.TradeAction.ProfitTargetPrice);
			}
			else if(GetMarketPosition() == MarketPosition.Short) {
				newPrc = MovePriceByTicks(avgPrc, -CurrentTrade.TradeAction.TrailingProfitTargetTics);
				CurrentTrade.TradeAction.ProfitTargetPrice = Math.Min(newPrc, CurrentTrade.TradeAction.ProfitTargetPrice);
			}
			
			SetProfitTargetOrder(CurrentTrade.TradeAction.EntrySignal.SignalName);
			//LockStopLossAtMinPT();
		}
		
		/// <summary>
		/// Move target away
		/// Move stop loss to LockMinProfit
		/// </summary>
		public void LockMinProfitTarget(double avgPrc){
			MoveProfitTarget(avgPrc);
			LockStopLossAtMinPT(avgPrc);
		}
		
		/// <summary>
		/// Lock stop loss at LockMinProfit
		/// </summary>
		public void LockStopLossAtMinPT(double avgPrc){
			double newPrc;
			if(GetMarketPosition() == MarketPosition.Long) {
				newPrc = MovePriceByTicks(avgPrc, CurrentTrade.profitLockMinTic);
				CurrentTrade.TradeAction.StopLossPrice = Math.Max(newPrc, CurrentTrade.TradeAction.StopLossPrice);
			} 
			else if(GetMarketPosition() == MarketPosition.Short) {
				newPrc = MovePriceByTicks(avgPrc, -CurrentTrade.profitLockMinTic);				
				CurrentTrade.TradeAction.StopLossPrice = Math.Min(newPrc, CurrentTrade.TradeAction.StopLossPrice);
			}
			CurrentTrade.SLCalculationMode = CalculationMode.Price;
			SetStopLossOrder(CurrentTrade.TradeAction.EntrySignal.SignalName);
		}
		
		#endregion
		
		#region Check PnL functions
		//Instrument.MasterInstrument.RoundToTickSize(double price)
		
		/// <summary>
		/// Get the difference of PnL over the breakEven threshold
		/// return -1 if PnL is below the breakEven threshold
		/// </summary>
		/// <returns></returns>
		public double isOverBreakeven(double pl) {
			double overAmt = -1;			
			if(pl >= CurrentTrade.breakEvenAmt)
				overAmt = pl - CurrentTrade.breakEvenAmt;
			return overAmt;
		}
		
		/// <summary>
		/// Get the difference of PnL over the MinLockProfitTarget threshold
		/// return -1 if PnL is below the MinLockProfitTarget threshold
		/// </summary>
		/// <returns></returns>		
		public int isOverMinLockPT(int pl_tics) {
			int overAmt = -1;			
			if(pl_tics >= (CurrentTrade.profitLockMinTic+2*CurrentTrade.stopLossIncTic))
			//&& (pl >= (CurrentTrade.profitTargetAmt+2*GetCurrencyByTicks(CurrentTrade.profitTgtIncTic))))
				overAmt = pl_tics - CurrentTrade.profitLockMinTic;
			return overAmt;			
		}

		/// <summary>
		/// Get the difference of PnL over the MinLockProfitTarget threshold
		/// return -1 if PnL is below the MinLockProfitTarget threshold
		/// </summary>
		/// <returns></returns>		
		public int isOverMaxLockPT(int pl_tics) {
			int overAmt = -1;
			if(pl_tics >= (CurrentTrade.profitLockMaxTic+2*CurrentTrade.profitTgtIncTic))
				overAmt = pl_tics - CurrentTrade.profitLockMaxTic;
			return overAmt;
		}

		/// <summary>
		/// Get exit order type
		/// </summary>
		public virtual void GetExitOrderType(double pl, int pl_tics) {
			switch(CurrentTrade.BracketOrder.ExitOrderType) {
				case EntryExitOrderType.LockMinProfit: // move PT, lock SL at MinPT
					if(isOverMaxLockPT(pl_tics) > 0)
						CurrentTrade.InitNewTLSL();
						//CurrentTrade.BracketOrder.ExitOrderType = EntryExitOrderType.TrailingStopLoss;			
					break;
				case EntryExitOrderType.BreakEven: // PT no change, BE SL
					if(isOverMaxLockPT(pl_tics) > 0)
						CurrentTrade.BracketOrder.ExitOrderType = EntryExitOrderType.TrailingStopLoss;
					else if (isOverMinLockPT(pl_tics) > 0)
						CurrentTrade.BracketOrder.ExitOrderType = EntryExitOrderType.LockMinProfit;
					break;
				case EntryExitOrderType.SimpleOCO: // set simple PT, SL
					if(isOverMaxLockPT(pl_tics) > 0)
						CurrentTrade.BracketOrder.ExitOrderType = EntryExitOrderType.TrailingStopLoss;
					else if (isOverMinLockPT(pl_tics) > 0)
						CurrentTrade.BracketOrder.ExitOrderType = EntryExitOrderType.LockMinProfit;
					else if (isOverBreakeven(pl) > 0)
						CurrentTrade.BracketOrder.ExitOrderType = EntryExitOrderType.BreakEven;
					break;
			}
		}
		
		public double CheckUnrealizedPnL() {
			if(IsLiveTrading())
				return PositionAccount.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0]);
			else 
				return Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0]);
		}
		
		public int CheckUnrealizedPnLTicks() {
			return GetTicksByCurrency(CheckUnrealizedPnL());
		}		
		
		public double CheckAccPnL() {
			double pnl = 0;//GetAccountValue(AccountItem.RealizedProfitLoss);
			//Print(CurrentBar + "-" + AccName + ": GetAccountValue(AccountItem.RealizedProfitLoss)= " + pnl + " -- " + Time[0].ToString());
			return pnl;
		}
		
		public double CheckAccCumProfit() {
			///Performance.RealtimeTrades.TradesPerformance.Currency.CumProfit;
			//Print(CurrentBar + "-" + AccName + ": Cum runtime PnL= " + plrt);
			if(IsLiveTrading()) {
				return SystemPerformance.RealTimeTrades.TradesPerformance.Currency.CumProfit;
			}
			else {
				return SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit;
			}
		}
		
		public double CheckPerformance()
		{
			double pl = SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit;//Performance.AllTrades.TradesPerformance.Currency.CumProfit;
			double plrt = SystemPerformance.RealTimeTrades.TradesPerformance.Currency.CumProfit;//Performance.RealtimeTrades.TradesPerformance.Currency.CumProfit;
			IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				CurrentBar + "-" + AccName + ": Cum all PnL= " + pl + ", Cum runtime PnL= " + plrt);
			return plrt;
		}
		
		public double CheckPnlByDay(int year, int month, int day) {
			double pnl = 0;
			TradeCollection tc = null;
			DateTime dayKey = new DateTime(year, month, day);//(Time[0].Year,Time[0].Month,Time[0].Day);
			SetPrintOut(1);
			IndicatorProxy.TraceMessage(CurrentBar + "-CheckPnlByDay AllTrades(dayKey, ByDay.Count)=" + dayKey
			+ "," + SystemPerformance.AllTrades.ByDay.Count
			+ "RealTimeTrades ByDay.Count=" + SystemPerformance.RealTimeTrades.ByDay.Count
			+ "::" + this.Name, PrintOut);
			
			if(IsLiveTrading()) {
				if(SystemPerformance.RealTimeTrades.ByDay.Keys.Contains(dayKey))
					tc = (TradeCollection)SystemPerformance.RealTimeTrades.ByDay[dayKey];
			} else {
				if(SystemPerformance.AllTrades.ByDay.Keys.Contains(dayKey))
					tc = (TradeCollection)SystemPerformance.AllTrades.ByDay[dayKey];//Performance.AllTrades.ByDay[dayKey];
			}
			
			if(tc != null) {
				pnl = tc.TradesPerformance.Currency.CumProfit;
				IndicatorProxy.TraceMessage(CurrentBar + "-CheckPnlByDay: Count, IsLiveTrading, pnl="
				+ tc.Count + "," + IsLiveTrading().ToString() + "," + pnl
				+ "::" + this.Name, PrintOut);				
			}
			return pnl;
		}
		
		#endregion
		
		#region Price functions
		/// <summary>
		/// Check if the SL/PT prices are valid
		/// </summary>
		/// <returns></returns>
		public bool isOcoPriceValid() {
			bool isValid = false;
			if(CurrentTrade.TradeAction.StopLossPrice > 0 && CurrentTrade.TradeAction.ProfitTargetPrice > 0) {
				if(GetMarketPosition() == MarketPosition.Long) {
					isValid = (CurrentTrade.TradeAction.ProfitTargetPrice > CurrentTrade.TradeAction.StopLossPrice);
				}
				else if(GetMarketPosition() == MarketPosition.Short) {
					isValid = (CurrentTrade.TradeAction.ProfitTargetPrice < CurrentTrade.TradeAction.StopLossPrice);
				}
			}
			return isValid;
		}

		/// <summary>
		/// Check if the trailing stoploss price is valid
		/// </summary>
		/// <returns></returns>
		public bool isTLSLPriceValid() {
			bool isValid = false;
			if(CurrentTrade.TradeAction.StopLossPrice > 0) {
				if(GetMarketPosition() == MarketPosition.Long) {
					isValid = (CurrentTrade.TradeAction.StopLossPrice < Close[0]);
				}
				else if(GetMarketPosition() == MarketPosition.Short) {
					isValid = (CurrentTrade.TradeAction.StopLossPrice > Close[0]);
				}
			}
			return isValid;
		}
		
		public double GetPriceByCurrency(double amt) {
			return amt/Bars.Instrument.MasterInstrument.PointValue;
		}
		
		public double GetPriceByTicks(int tics) {
			return TickSize*tics;
		}
		
		public double MovePriceByTicks(double prc, int tics) {
			return prc + GetPriceByTicks(tics);
		}

		public double MovePriceByCurrency(double prc, double amt) {
			return prc + GetPriceByCurrency(amt);			
		}
		
		public int GetTicksByCurrency(double amt) {
			int tic = -1;
			if(amt > 0)
				tic = (int)(amt/GetTickValue());
			return tic;
		}
		
		public double GetCurrencyByTicks(int tics) {
			double amt = -1;
			if(tics > 0)
				amt = tics*GetTickValue();
			return amt;
		}
		
		public double GetHighestPrice(int barsAgo) {
			double hiPrc = High[1];
			if(barsAgo > 0) {
				hiPrc = High[HighestBar(High, barsAgo)];
			}
			IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				CurrentBar + ":hiPrc=" + hiPrc 
				+ ";barsAgo=" + barsAgo);
			return hiPrc;
		}

		public double GetLowestPrice(int barsAgo) {
			double loPrc = Low[1];
			if(barsAgo > 0) {
				loPrc = Low[LowestBar(Low, barsAgo)];
			}
			IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				CurrentBar + ":loPrc=" + loPrc
				+ ";barsAgo=" + barsAgo);
			return loPrc;
		}
		
		public double GetTypicalPrice(int barsAgo) {
			MasterInstrument maIns = Bars.Instrument.MasterInstrument;			
			return maIns.RoundToTickSize(Typical[barsAgo]);
		}
		
		#endregion

		#region Depricated methods
		/// <summary>
		/// Trailing max and min profits then converted to trailing stop after over the max
		/// </summary>
		public void Dep_ChangeStopLoss() {
			if(!CurrentTrade.slTrailing) return;
			else {
				double pl = CheckUnrealizedPnL();
				double slPrc = Position.AveragePrice;
				if(CurrentTrade.trailingSLTic > CurrentTrade.profitLockMaxTic && pl >= GetTickValue()*(CurrentTrade.trailingSLTic + 2*CurrentTrade.profitTgtIncTic)) {
					CurrentTrade.trailingSLTic = CurrentTrade.trailingSLTic + CurrentTrade.profitTgtIncTic;
					if(Position.MarketPosition == MarketPosition.Long)
						slPrc = Position.AveragePrice+TickSize*CurrentTrade.trailingSLTic;
					if(Position.MarketPosition == MarketPosition.Short)
						slPrc = Position.AveragePrice-TickSize*CurrentTrade.trailingSLTic;
					Print(AccName + "- update SL over Max: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= (" + CurrentTrade.slTrailing + "," + CurrentTrade.trailingSLTic + "," + slPrc + ")");						
				}
				if(CurrentTrade.trailingSLTic > CurrentTrade.profitLockMaxTic && pl >= GetTickValue()*(CurrentTrade.trailingSLTic + 2*CurrentTrade.profitTgtIncTic)) {
					CurrentTrade.trailingSLTic = CurrentTrade.trailingSLTic + CurrentTrade.profitTgtIncTic;
					if(CurrentTrade.BracketOrder.OCOOrder.StopLossOrder != null)
						CancelOrder(CurrentTrade.BracketOrder.OCOOrder.StopLossOrder);
					if(CurrentTrade.BracketOrder.OCOOrder.ProfitTargetOrder != null)
						CancelOrder(CurrentTrade.BracketOrder.OCOOrder.ProfitTargetOrder);
					SetTrailStop(CalculationMode.Currency, CurrentTrade.trailingSLAmt);
					IndicatorProxy.PrintLog(true, IsLiveTrading(),
						AccName + "- SetTrailStop over SL Max: PnL=" + pl +
						"(slTrailing, trailingSLTic, slPrc)= (" + CurrentTrade.slTrailing + "," + CurrentTrade.trailingSLTic + "," + slPrc + ")");						
				}
				else if(pl >= GetTickValue()*(CurrentTrade.profitLockMaxTic + 2*CurrentTrade.profitTgtIncTic)) { // lock max profits
					CurrentTrade.trailingSLTic = CurrentTrade.trailingSLTic + CurrentTrade.profitTgtIncTic;
					if(Position.MarketPosition == MarketPosition.Long)
						slPrc = CurrentTrade.trailingSLTic > CurrentTrade.profitLockMaxTic ? Position.AveragePrice+TickSize*CurrentTrade.trailingSLTic : Position.AveragePrice+TickSize*CurrentTrade.profitLockMaxTic;
					if(Position.MarketPosition == MarketPosition.Short)
						slPrc = CurrentTrade.trailingSLTic > CurrentTrade.profitLockMaxTic ? Position.AveragePrice-TickSize*CurrentTrade.trailingSLTic :  Position.AveragePrice-TickSize*CurrentTrade.profitLockMaxTic;
					IndicatorProxy.PrintLog(true, IsLiveTrading(),
						AccName + "- update SL Max: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= ("
						+ CurrentTrade.slTrailing + "," + CurrentTrade.trailingSLTic + "," + slPrc + ")");
					//SetStopLoss(CalculationMode.Price, slPrc);
				}
				else if(pl >= GetTickValue()*(CurrentTrade.profitLockMinTic + 2*CurrentTrade.profitTgtIncTic)) { //lock min profits
					CurrentTrade.trailingSLTic = CurrentTrade.trailingSLTic + CurrentTrade.profitTgtIncTic;
					if(Position.MarketPosition == MarketPosition.Long)
						slPrc = Position.AveragePrice+TickSize*CurrentTrade.profitLockMinTic;
					if(Position.MarketPosition == MarketPosition.Short)
						slPrc = Position.AveragePrice-TickSize*CurrentTrade.profitLockMinTic;
					IndicatorProxy.PrintLog(true, IsLiveTrading(), 
						AccName + "- update SL Min: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= ("
						+ CurrentTrade.slTrailing + "," + CurrentTrade.trailingSLTic + "," + slPrc + ")");
					//SetStopLoss(CalculationMode.Price, slPrc);
				}
			}
		}
		
		/// <summary>
		/// For position exit OCO order adjustment
		/// breakEvenAmt<MinLock+2*SLIncTic<ProfitTargetAmt+2*PTIncTic<MaxLock
		/// ** Check trailing stop loss if PnL>MaxLock
		/// ** Check trailing profit target if ProfitTargetAmt<PnL<MaxLock
		///   * Keep moving target away;
		///   * Keep stop loss at MinLock no change;
		/// ** Check stop loss:
		///   * Check if MinLock<PnL, lock stop loss to MinLock
		///   * Keep target no change;??-->Keep moving target away;
		/// ** Check BreakEven:
		///   * If PnL>breakEvenAmt, set SL to breakeven;
		///   * Keep target no change;
		///
		/// </summary>
		/// <returns></returns>
		public bool Dep_ChangeSLPT()
		{
//			int bse = BarsSinceEntry();
//			double timeSinceEn = -1;
//			if(bse > 0) {
//				timeSinceEn = indicatorProxy.GetMinutesDiff(Time[0], Time[bse]);
//			}
			int prtLevel = 0;
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
			double pl = CheckUnrealizedPnL();
			double pl_tics = CheckUnrealizedPnLTicks();
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
			if(pl_tics >= (CurrentTrade.profitLockMaxTic+2*CurrentTrade.profitTgtIncTic)) {
				IndicatorProxy.TraceMessage(this.Name, prtLevel);
				//SetTrailingStopLossOrder(CurrentTrade.TradeAction.EntrySignal.SignalName.ToString());
			} // start trailing SL
			else if (pl >= (CurrentTrade.profitTargetAmt+2*GetCurrencyByTicks(CurrentTrade.profitTgtIncTic))) {
				IndicatorProxy.TraceMessage(this.Name, prtLevel);
				//MoveProfitTarget();
			} // move PT, lock SL
			else if (pl_tics >= (CurrentTrade.profitLockMinTic+2*CurrentTrade.stopLossIncTic)) {
				IndicatorProxy.TraceMessage(this.Name, prtLevel);
				//LockMinProfitTarget();
			} // PT no change, lock SL
			else if (pl >= CurrentTrade.breakEvenAmt) {
				IndicatorProxy.TraceMessage(this.Name, prtLevel);
				SetBreakEvenOrder(GetAvgPrice());
			} // PT no change, BE SL
			else {
				IndicatorProxy.TraceMessage(this.Name, prtLevel);
				SetSimpleExitOCO(CurrentTrade.TradeAction.EntrySignal.SignalName.ToString());
			} // set simple PT, SL
			
//			if(Position.Quantity == 0)
//				indicatorProxy.PrintLog(true, !backTest, 
//					AccName + "- ChangeSLPT=0: (PnL, posAvgPrc, MM_BreakEvenAmt)=(" + pl + "," + Position.AveragePrice + "," + CurrentTrade.breakEvenAmt + ")");
//			else
//				indicatorProxy.PrintLog(true, !backTest, 
//					AccName + "- ChangeSLPT<>0: (PnL, posAvgPrc, MM_BreakEvenAmt)=(" + pl + "," + Position.AveragePrice + "," + CurrentTrade.breakEvenAmt + ")");
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
			// If not flat print out unrealized PnL
    		if (Position.MarketPosition != MarketPosition.Flat) 
			{
         		//giParabSAR.PrintLog(true, !backTest, log_file, AccName + "- Open PnL: " + pl);
				//int nChkPnL = (int)(timeSinceEn/minutesChkPnL);
				double curPTTics = -1;
				double slPrc = CurrentTrade.BracketOrder.OCOOrder.StopLossOrder == null ?
					Position.AveragePrice : CurrentTrade.BracketOrder.OCOOrder.StopLossOrder.StopPrice;
				
				//MoveProfitTarget();

				IndicatorProxy.PrintLog(true, IsLiveTrading(),
					AccName + "- SL Breakeven: (PnL, posAvgPrc, MM_BreakEvenAmt)=(" + pl + "," + Position.AveragePrice + "," + CurrentTrade.breakEvenAmt + ")");
				//ChangeBreakEven();
				//ChangeStopLoss();
				
				if(CurrentTrade.BracketOrder.OCOOrder.StopLossOrder == null ||
					(Position.MarketPosition == MarketPosition.Long && slPrc > CurrentTrade.BracketOrder.OCOOrder.StopLossOrder.StopPrice) ||
					(Position.MarketPosition == MarketPosition.Short && slPrc < CurrentTrade.BracketOrder.OCOOrder.StopLossOrder.StopPrice))
				{
					//SetStopLoss(CalculationMode.Price, slPrc);
				}
			} else {
				//InitTradeMgmt();
			}

			return false;
		}		
		#endregion
		
		#region MM Properties
        [Description("Money amount of profit target")]
 		[Range(0, double.MaxValue), NinjaScriptProperty, XmlIgnore]		
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitTgtAmt", GroupName = GPS_MONEY_MGMT, Order = ODG_ProfitTgtAmt)]	
        public double MM_ProfitTargetAmt
        {
            get{return mm_ProfitTargetAmt;}
            set{mm_ProfitTargetAmt = Math.Max(0, value);}
        }

        [Description("Ticks amount for profit target")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitTgtTic", GroupName = GPS_MONEY_MGMT, Order = ODG_ProfitTgtTic)]	
        public int MM_ProfitTgtTic
        {
            get{return mm_ProfitTgtTic;}
            set{mm_ProfitTgtTic = Math.Max(0, value);}
        }
		
        [Description("Money amount for profit target increasement")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitTgtIncTic", GroupName = GPS_MONEY_MGMT, Order = ODG_ProfitTgtIncTic)]	
        public int MM_ProfitTgtIncTic
        {
            get{return mm_ProfitTgtIncTic;}
            set{mm_ProfitTgtIncTic = Math.Max(0, value);}
        }
		
        [Description("Tick amount for min profit locking")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitLockMinTic", GroupName = GPS_MONEY_MGMT, Order = ODG_ProfitLockMinTic)]	
        public int MM_ProfitLockMinTic
        {
            get{return mm_ProfitLockMinTic;}
            set{mm_ProfitLockMinTic = Math.Max(0, value);}
        }

		[Description("Tick amount for max profit locking")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitLockMaxTic", GroupName = GPS_MONEY_MGMT, Order = ODG_ProfitLockMaxTic)]	
        public int MM_ProfitLockMaxTic
        {
            get{return mm_ProfitLockMaxTic;}
            set{mm_ProfitLockMaxTic = Math.Max(0, value);}
        }
		
        [Description("Money amount of stop loss")]
 		[Range(0, double.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "StopLossAmt", GroupName = GPS_MONEY_MGMT, Order = ODG_StopLossAmt)]	
        public double MM_StopLossAmt
        {
            get{return mm_StopLossAmt;}
            set{mm_StopLossAmt = Math.Max(0, value);}
        }
		
		[Description("Ticks amount for stop loss")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "StopLossTic", GroupName = GPS_MONEY_MGMT, Order = ODG_StopLossTic)]	
        public int MM_StopLossTic
        {
            get{return mm_StopLossTic;}
            set{mm_StopLossTic = Math.Max(0, value);}
        }
		
		[Description("Money amount for stop loss increasement")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "StopLossIncTic", GroupName = GPS_MONEY_MGMT, Order = ODG_StopLossIncTic)]	
        public int MM_StopLossIncTic
        {
            get{return mm_StopLossIncTic;}
            set{mm_StopLossIncTic = Math.Max(0, value);}
        }

        [Description("Break Even amount")]
 		[Range(0, double.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BreakEvenAmt", GroupName = GPS_MONEY_MGMT, Order = ODG_BreakEvenAmt)]	
        public double MM_BreakEvenAmt
        {
            get{return mm_BreakEvenAmt;}
            set{mm_BreakEvenAmt = Math.Max(0, value);}
        }
		
        [Description("Money amount of trailing stop loss")]
 		[Range(0, double.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TrailingStopLossAmt", GroupName = GPS_MONEY_MGMT, Order = ODG_TrailingStopLossAmt)]	
        public double MM_TrailingStopLossAmt
        {
            get{return mm_TrailingStopLossAmt;}
            set{mm_TrailingStopLossAmt = Math.Max(0, value);}
        }
        
		[Description("Ticks amount of trailing stop loss")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TrailingStopLossTic", GroupName = GPS_MONEY_MGMT, Order = ODG_TrailingStopLossTic)]	
        public int MM_TrailingStopLossTic
        {
            get{return mm_TrailingStopLossTic;}
            set{mm_TrailingStopLossTic = Math.Max(0, value);}
		}

        [Description("Percent amount of trailing stop loss")]
 		[Range(0, double.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TrailingStopLossPercent", GroupName = GPS_MONEY_MGMT, Order = ODG_TrailingStopLossPercent)]	
        public double MM_TrailingStopLossPercent
        {
            get{return mm_TrailingStopLossPercent;}
            set{mm_TrailingStopLossPercent = Math.Max(0, value);}
        }
		
		[Description("Use trailing stop loss every bar")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SLTrailing", GroupName = GPS_MONEY_MGMT, Order = ODG_SLTrailing)]	
        public bool MM_SLTrailing
        {
            get{return mm_SLTrailing;}
            set{mm_SLTrailing = value;}
        }
		
		[Description("Use trailing profit target every bar")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PTTrailing", GroupName = GPS_MONEY_MGMT, Order = ODG_PTTrailing)]	
        public bool MM_PTTrailing
        {
            get{return mm_PTTrailing;}
            set{mm_PTTrailing = value;}
        }

		[Description("Calculation mode for profit target")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PTCalculationMode", GroupName = GPS_MONEY_MGMT, Order = ODG_PTCalculationMode)]	
        public CalculationMode MM_PTCalculationMode
        {
            get{return mm_PTCalculationMode;}
            set{mm_PTCalculationMode = value;}
        }

		[Description("Calculation mode for stop loss")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SLCalculationMode", GroupName = GPS_MONEY_MGMT, Order = ODG_SLCalculationMode)]	
        public CalculationMode MM_SLCalculationMode
        {
            get{return mm_SLCalculationMode;}
            set{mm_SLCalculationMode = value;}
        }

		[Description("Calculation mode for break even")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BECalculationMode", GroupName = GPS_MONEY_MGMT, Order = ODG_BECalculationMode)]	
        public CalculationMode MM_BECalculationMode
        {
            get{return mm_BECalculationMode;}
            set{mm_BECalculationMode = value;}
        }
		
		[Description("Calculation mode for trailing stop loss")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TLSLCalculationMode", GroupName = GPS_MONEY_MGMT, Order = ODG_TLSLCalculationMode)]	
        public CalculationMode MM_TLSLCalculationMode
        {
            get{return mm_TLSLCalculationMode;}
            set{mm_TLSLCalculationMode = value;}
        }		
		
		[Description("Daily Loss Limit amount")]
 		[Range(double.MinValue, double.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "DailyLossLmt", GroupName = GPS_MONEY_MGMT, Order = ODG_DailyLossLmt)]	
        public double MM_DailyLossLmt
        {
            get{return mm_DailyLossLmt;}
            set{mm_DailyLossLmt = Math.Min(-100, value);}
        }

		[Description("Profit Factor")]
 		[Range(0, double.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitFactor", GroupName = GPS_MONEY_MGMT, Order = ODG_ProfitFactor)]	
        public double MM_ProfitFactor
        {
            get{return mm_ProfitFactor;}
            set{mm_ProfitFactor = Math.Max(0, value);}
        }		
		#endregion
		
		#region Variables for Properties		
		
		private double mm_ProfitTargetAmt = 500;
		private int mm_ProfitTgtTic = 32;
		private int mm_ProfitTgtIncTic = 8;
		private int mm_ProfitLockMinTic = 16;
		private int mm_ProfitLockMaxTic = 40;
		
		private double mm_StopLossAmt = 300;
		private int mm_StopLossTic = 16;
		private int mm_StopLossIncTic = 8;
		
		private double mm_BreakEvenAmt = 150;
		
		private double mm_TrailingStopLossAmt = 200;
		private int mm_TrailingStopLossTic = 16;
		private double mm_TrailingStopLossPercent = 1;

		//private bool mm_EnTrailing = true;
		private bool mm_PTTrailing = false;
		private bool mm_SLTrailing = false;
		
		private CalculationMode mm_PTCalculationMode = CalculationMode.Currency;
		private CalculationMode mm_SLCalculationMode = CalculationMode.Currency;
		private CalculationMode mm_BECalculationMode = CalculationMode.Currency;
		private CalculationMode mm_TLSLCalculationMode = CalculationMode.Ticks;
		
		private double mm_DailyLossLmt = -200;
		private double mm_ProfitFactor = 2;
		
		#endregion
	}
}
