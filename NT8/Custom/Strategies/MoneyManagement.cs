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
	public partial class GSZTraderBase : Strategy
	{		
		#region Money Mgmt Functions
		
		protected double CalProfitTargetAmt(double price, double profitFactor) {
			switch(tradeObj.SLCalculationMode) {
				case CalculationMode.Currency:
					if(profitFactor == 0)
						tradeObj.profitTargetAmt = MM_ProfitTargetAmt;
					else
						tradeObj.profitTargetAmt = 
							profitFactor*tradeObj.stopLossAmt;
					break;
				case CalculationMode.Price:
					if(profitFactor == 0)
						tradeObj.profitTargetAmt = MM_ProfitTargetAmt;			
					else
						tradeObj.profitTargetAmt = 
							profitFactor*Math.Abs(price-tradeObj.stopLossPrice)*Instrument.MasterInstrument.PointValue;
					break;
			}			
			return 0;
		}

		/// <summary>
		/// Calculation of price for unmanaged OCO exit order
		/// </summary>
		/// <param name="avgPrice">Avg entry price</param>
		/// <param name="profitFactor">PT/SL>0</param>
		protected void CalExitOcoPrice(double avgPrice, double profitFactor) {
			int prtLevel = 0;
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			switch(tradeObj.SLCalculationMode) {
				case CalculationMode.Currency:
					if(profitFactor > 0)
						tradeObj.profitTargetAmt = 
							profitFactor*tradeObj.stopLossAmt;
					if(Position.MarketPosition == MarketPosition.Long) {
						tradeObj.stopLossPrice = avgPrice - GetPriceByCurrency(tradeObj.stopLossAmt);
						tradeObj.profitTargetPrice = avgPrice + GetPriceByCurrency(tradeObj.profitTargetAmt);
					}
					else if(Position.MarketPosition == MarketPosition.Short) {
						tradeObj.stopLossPrice = avgPrice + GetPriceByCurrency(tradeObj.stopLossAmt);
						tradeObj.profitTargetPrice = avgPrice - GetPriceByCurrency(tradeObj.profitTargetAmt);
					}
					break;
				case CalculationMode.Ticks:
					if(profitFactor > 0)
						tradeObj.profitTargetTic = 
							(int)(profitFactor*tradeObj.stopLossTic);
					if(Position.MarketPosition == MarketPosition.Long) {
						tradeObj.stopLossPrice = avgPrice - GetPriceByTicks(tradeObj.stopLossTic);
						tradeObj.profitTargetPrice = avgPrice + GetPriceByTicks(tradeObj.profitTargetTic);
					}
					else if(Position.MarketPosition == MarketPosition.Short) {
						tradeObj.stopLossPrice = avgPrice + GetPriceByTicks(tradeObj.stopLossTic);
						tradeObj.profitTargetPrice = avgPrice - GetPriceByTicks(tradeObj.profitTargetTic);
					}					
					break;
				case CalculationMode.Price:
					if(profitFactor > 0)
						tradeObj.profitTargetAmt = 
							profitFactor*Math.Abs(avgPrice-tradeObj.stopLossPrice)*Instrument.MasterInstrument.PointValue;
					break;
			}
			Print(CurrentBar + ":CalExitOcoPrice-tradeObj.SLCalculationMode" + tradeObj.SLCalculationMode + ","
			+ "-stopLossPrice=" + tradeObj.stopLossPrice + ","
			+ "-profitTargetPrice=" + tradeObj.profitTargetPrice);
		}

		/// <summary>
		/// Calculation of price for unmanaged trailing stoploss order
		/// </summary>
		/// <param name="avgPrice">Avg entry price</param>
		public virtual void CalTLSLPrice(double avgPrice, int pl_tics) {
			int prtLevel = 0;
			indicatorProxy.TraceMessage(this.Name, prtLevel);

//			if(pl_tics >= (tradeObj.profitLockMaxTic+2*tradeObj.profitTgtIncTic))
//				overAmt = pl_tics - tradeObj.profitLockMaxTic;
			
			switch(tradeObj.TLSLCalculationMode) {
				case CalculationMode.Currency:

					break;
				case CalculationMode.Ticks:
					
					break;
				case CalculationMode.Percent:
					throw new Exception("CalculationMode.Percent for trailing stoploss not supported yet!");
					break;
			}
			Print(CurrentBar + ":CalTLSLPrice-Pre"
			+ ";TLSLCalculationMode=" + tradeObj.TLSLCalculationMode
			+ ";trailingSLPrice=" + tradeObj.trailingSLPrice
			+ ";trailingPTTic=" + tradeObj.trailingPTTic
			+ ";trailingSLTic=" + tradeObj.trailingSLTic
			+ ";avgPrice=" + avgPrice);
			
			if(tradeObj.trailingPTTic < tradeObj.profitLockMaxTic)
				tradeObj.trailingPTTic = Math.Max(tradeObj.profitLockMaxTic, pl_tics-tradeObj.trailingSLTic);
			else
				tradeObj.trailingPTTic = Math.Max(tradeObj.trailingPTTic, pl_tics-tradeObj.trailingSLTic);
			
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			if(Position.MarketPosition == MarketPosition.Long) {
				tradeObj.trailingSLPrice = MovePriceByTicks(avgPrice, tradeObj.trailingPTTic);
			}
			else if(Position.MarketPosition == MarketPosition.Short) {
				tradeObj.trailingSLPrice = MovePriceByTicks(avgPrice, -tradeObj.trailingPTTic);
			}
			
			Print(CurrentBar + ":CalTLSLPrice"
			+ ";TLSLCalculationMode=" + tradeObj.TLSLCalculationMode
			+ ";trailingSLPrice=" + tradeObj.trailingSLPrice
			+ ";trailingPTTic=" + tradeObj.trailingPTTic
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
		protected bool ChangeSLPT()
		{
			int prtLevel = 0;
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			double pl = CheckUnrealizedPnL();
			int pl_tics = CheckUnrealizedPnLTicks();
			double avgPrc = GetAvgPrice();
			GetExitOrderType(pl, pl_tics);
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			
			switch(tradeObj.exitOrderType) {
				case ExitOrderType.TrailingStopLoss: // start trailing SL
					Print(CurrentBar + ":isOverMaxLockPT, pl_tics=" + pl_tics);
					indicatorProxy.TraceMessage(this.Name, prtLevel);
					CalTLSLPrice(avgPrc, pl_tics);
					SetTrailingStopLossOrder(tradeObj.entrySignalName.ToString());
					break;
				case ExitOrderType.LockMinProfit: // move PT, lock SL at MinPT
					Print(CurrentBar + ":isOverMinLockPT, pl_tics=" + pl_tics);
					indicatorProxy.TraceMessage(this.Name, prtLevel);
					LockMinProfitTarget(avgPrc);
					break;
				case ExitOrderType.BreakEven: // PT no change, BE SL
					Print(CurrentBar + ":isOverBreakeven, pl_tics=" + pl_tics);
					indicatorProxy.TraceMessage(this.Name, prtLevel);
					SetBreakEvenOrder(avgPrc);
					break;
				case ExitOrderType.SimpleOCO: // set simple PT, SL
					Print(CurrentBar + ":isBelowBreakeven, pl_tics=" + pl_tics);
					indicatorProxy.TraceMessage(this.Name, prtLevel);
					SetSimpleExitOCO(tradeObj.entrySignalName.ToString());
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
			//if(!tradeObj.ptTrailing) return;
			if(GetCurrencyByTicks(tradeObj.trailingPTTic) <= tradeObj.profitTargetAmt) { //first time move the profit target
				tradeObj.trailingPTTic = GetTicksByCurrency(tradeObj.profitTargetAmt) + tradeObj.profitTgtIncTic;
			}
			else {
				double pl_tics = CheckUnrealizedPnLTicks();
				if(pl_tics >= (tradeObj.trailingPTTic - 2*tradeObj.profitTgtIncTic)) {
					tradeObj.trailingPTTic = tradeObj.trailingPTTic + tradeObj.profitTgtIncTic;
					//tradeObj.profitTargetPrice = MovePriceByTicks(avgPrc, tradeObj.profitTgtIncTic);
				}
			}
			
			double newPrc;
			if(Position.MarketPosition == MarketPosition.Long) {
				newPrc = MovePriceByTicks(avgPrc, tradeObj.trailingPTTic);
				tradeObj.profitTargetPrice = Math.Max(newPrc, tradeObj.profitTargetPrice);
			}
			else if(Position.MarketPosition == MarketPosition.Short) {
				newPrc = MovePriceByTicks(avgPrc, -tradeObj.trailingPTTic);
				tradeObj.profitTargetPrice = Math.Min(newPrc, tradeObj.profitTargetPrice);
			}
			
			SetProfitTargetOrder(tradeObj.entrySignalName.ToString());
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
			if(Position.MarketPosition == MarketPosition.Long) {
				newPrc = MovePriceByTicks(avgPrc, tradeObj.profitLockMinTic);
				tradeObj.stopLossPrice = Math.Max(newPrc, tradeObj.stopLossPrice);
			} 
			else if(Position.MarketPosition == MarketPosition.Short) {
				newPrc = MovePriceByTicks(avgPrc, -tradeObj.profitLockMinTic);				
				tradeObj.stopLossPrice = Math.Min(newPrc, tradeObj.stopLossPrice);
			}
			tradeObj.SLCalculationMode = CalculationMode.Price;
			SetStopLossOrder(tradeObj.entrySignalName.ToString());
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
			if(pl >= tradeObj.breakEvenAmt)
				overAmt = pl - tradeObj.breakEvenAmt;
			return overAmt;
		}
		
		/// <summary>
		/// Get the difference of PnL over the MinLockProfitTarget threshold
		/// return -1 if PnL is below the MinLockProfitTarget threshold
		/// </summary>
		/// <returns></returns>		
		public int isOverMinLockPT(int pl_tics) {
			int overAmt = -1;			
			if(pl_tics >= (tradeObj.profitLockMinTic+2*tradeObj.stopLossIncTic))
			//&& (pl >= (tradeObj.profitTargetAmt+2*GetCurrencyByTicks(tradeObj.profitTgtIncTic))))
				overAmt = pl_tics - tradeObj.profitLockMinTic;
			return overAmt;			
		}

		/// <summary>
		/// Get the difference of PnL over the MinLockProfitTarget threshold
		/// return -1 if PnL is below the MinLockProfitTarget threshold
		/// </summary>
		/// <returns></returns>		
		public int isOverMaxLockPT(int pl_tics) {
			int overAmt = -1;
			if(pl_tics >= (tradeObj.profitLockMaxTic+2*tradeObj.profitTgtIncTic))
				overAmt = pl_tics - tradeObj.profitLockMaxTic;
			return overAmt;
		}

		/// <summary>
		/// Get exit order type
		/// </summary>
		public virtual void GetExitOrderType(double pl, int pl_tics) {
			switch(tradeObj.exitOrderType) {
				case ExitOrderType.LockMinProfit: // move PT, lock SL at MinPT
					if(isOverMaxLockPT(pl_tics) > 0)
						tradeObj.InitNewTLSL();
						//tradeObj.exitOrderType = ExitOrderType.TrailingStopLoss;			
					break;
				case ExitOrderType.BreakEven: // PT no change, BE SL
					if(isOverMaxLockPT(pl_tics) > 0)
						tradeObj.exitOrderType = ExitOrderType.TrailingStopLoss;
					else if (isOverMinLockPT(pl_tics) > 0)
						tradeObj.exitOrderType = ExitOrderType.LockMinProfit;
					break;
				case ExitOrderType.SimpleOCO: // set simple PT, SL
					if(isOverMaxLockPT(pl_tics) > 0)
						tradeObj.exitOrderType = ExitOrderType.TrailingStopLoss;
					else if (isOverMinLockPT(pl_tics) > 0)
						tradeObj.exitOrderType = ExitOrderType.LockMinProfit;
					else if (isOverBreakeven(pl) > 0)
						tradeObj.exitOrderType = ExitOrderType.BreakEven;
					break;
			}
		}
		
		/// <summary>
		/// Check if the SL/PT prices are valid
		/// </summary>
		/// <returns></returns>
		public bool isOcoPriceValid() {
			bool isValid = false;
			if(tradeObj.stopLossPrice > 0 && tradeObj.profitTargetPrice > 0) {
				if(Position.MarketPosition == MarketPosition.Long) {
					isValid = (tradeObj.profitTargetPrice > tradeObj.stopLossPrice);
				}
				else if(Position.MarketPosition == MarketPosition.Short) {
					isValid = (tradeObj.profitTargetPrice < tradeObj.stopLossPrice);
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
			if(tradeObj.trailingSLPrice > 0) {
				if(Position.MarketPosition == MarketPosition.Long) {
					isValid = (tradeObj.trailingSLPrice < Close[0]);
				}
				else if(Position.MarketPosition == MarketPosition.Short) {
					isValid = (tradeObj.trailingSLPrice > Close[0]);
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
		
		public double CheckUnrealizedPnL() {
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
			indicatorProxy.PrintLog(true, !backTest, CurrentBar + "-" + AccName + ": Cum all PnL= " + pl + ", Cum runtime PnL= " + plrt);
			return plrt;
		}
		
		public double CheckPnlByDay(int year, int month, int day) {
			double pnl = 0;
			TradeCollection tc = null;
			DateTime dayKey = new DateTime(year, month, day);//(Time[0].Year,Time[0].Month,Time[0].Day);
			Print(CurrentBar + "-CheckPnlByDay AllTrades(dayKey, ByDay.Count)=" + dayKey + "," + SystemPerformance.AllTrades.ByDay.Count
			+ "RealTimeTrades ByDay.Count=" + SystemPerformance.RealTimeTrades.ByDay.Count);
			if(IsLiveTrading()) {
				if(SystemPerformance.RealTimeTrades.ByDay.Keys.Contains(dayKey))
					tc = (TradeCollection)SystemPerformance.RealTimeTrades.ByDay[dayKey];
			} else {
				if(SystemPerformance.AllTrades.ByDay.Keys.Contains(dayKey))
					tc = (TradeCollection)SystemPerformance.AllTrades.ByDay[dayKey];//Performance.AllTrades.ByDay[dayKey];
			}
			
			if(tc != null) {
				pnl = tc.TradesPerformance.Currency.CumProfit;
				Print(CurrentBar + "-CheckPnlByDay: Count, IsLiveTrading, pnl=" + tc.Count + "," + IsLiveTrading().ToString() + "," + pnl);
			}
			return pnl;
		}
		
		#endregion
		
		#region Event Handlers
		
		protected override void OnPositionUpdate(Cbi.Position position, double averagePrice, 
			int quantity, Cbi.MarketPosition marketPosition)
		{			
			int bsx = BarsSinceExitExecution(0, "", 0);
			int bse = BarsSinceEntryExecution(0, "", 0);
			
			Print(CurrentBar + ":OnPositionUpdate- quantity, marketPosition, BarsSinceExit, BarsSinceEntry=" 
			+ quantity + "," + marketPosition + ","
			+ bsx + "," + bse);
			//Print(position.ToString() + "--MarketPosition=" + position.MarketPosition);
			if (position.MarketPosition == MarketPosition.Flat)
			{
				tradeObj.trailingPTTic = GetTicksByCurrency(tradeObj.profitTargetAmt);
				tradeObj.trailingSLTic = GetTicksByCurrency(tradeObj.stopLossAmt);
			} else {
//				SetBracketOrder.OCOOrder.ProfitTargetOrder(OrderSignalName.EntryShort.ToString());
//				SetBracketOrder.OCOOrder.StopLossOrder(OrderSignalName.EntryShort.ToString());
			}
		}
		
		#endregion

		#region Dep methods
		/// <summary>
		/// Trailing max and min profits then converted to trailing stop after over the max
		/// </summary>
		public void Dep_ChangeStopLoss() {
			if(!tradeObj.slTrailing) return;
			else {
				double pl = CheckUnrealizedPnL();
				double slPrc = Position.AveragePrice;
				if(tradeObj.trailingSLTic > tradeObj.profitLockMaxTic && pl >= GetTickValue()*(tradeObj.trailingSLTic + 2*tradeObj.profitTgtIncTic)) {
					tradeObj.trailingSLTic = tradeObj.trailingSLTic + tradeObj.profitTgtIncTic;
					if(Position.MarketPosition == MarketPosition.Long)
						slPrc = Position.AveragePrice+TickSize*tradeObj.trailingSLTic;
					if(Position.MarketPosition == MarketPosition.Short)
						slPrc = Position.AveragePrice-TickSize*tradeObj.trailingSLTic;
					Print(AccName + "- update SL over Max: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= (" + tradeObj.slTrailing + "," + tradeObj.trailingSLTic + "," + slPrc + ")");						
				}
				if(tradeObj.trailingSLTic > tradeObj.profitLockMaxTic && pl >= GetTickValue()*(tradeObj.trailingSLTic + 2*tradeObj.profitTgtIncTic)) {
					tradeObj.trailingSLTic = tradeObj.trailingSLTic + tradeObj.profitTgtIncTic;
					if(tradeObj.BracketOrder.OCOOrder.StopLossOrder != null)
						CancelOrder(tradeObj.BracketOrder.OCOOrder.StopLossOrder);
					if(tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder != null)
						CancelOrder(tradeObj.BracketOrder.OCOOrder.ProfitTargetOrder);
					SetTrailStop(CalculationMode.Currency, tradeObj.trailingSLAmt);
					indicatorProxy.PrintLog(true, !backTest,
						AccName + "- SetTrailStop over SL Max: PnL=" + pl +
						"(slTrailing, trailingSLTic, slPrc)= (" + tradeObj.slTrailing + "," + tradeObj.trailingSLTic + "," + slPrc + ")");						
				}
				else if(pl >= GetTickValue()*(tradeObj.profitLockMaxTic + 2*tradeObj.profitTgtIncTic)) { // lock max profits
					tradeObj.trailingSLTic = tradeObj.trailingSLTic + tradeObj.profitTgtIncTic;
					if(Position.MarketPosition == MarketPosition.Long)
						slPrc = tradeObj.trailingSLTic > tradeObj.profitLockMaxTic ? Position.AveragePrice+TickSize*tradeObj.trailingSLTic : Position.AveragePrice+TickSize*tradeObj.profitLockMaxTic;
					if(Position.MarketPosition == MarketPosition.Short)
						slPrc = tradeObj.trailingSLTic > tradeObj.profitLockMaxTic ? Position.AveragePrice-TickSize*tradeObj.trailingSLTic :  Position.AveragePrice-TickSize*tradeObj.profitLockMaxTic;
					indicatorProxy.PrintLog(true, !backTest,
						AccName + "- update SL Max: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= ("
						+ tradeObj.slTrailing + "," + tradeObj.trailingSLTic + "," + slPrc + ")");
					//SetStopLoss(CalculationMode.Price, slPrc);
				}
				else if(pl >= GetTickValue()*(tradeObj.profitLockMinTic + 2*tradeObj.profitTgtIncTic)) { //lock min profits
					tradeObj.trailingSLTic = tradeObj.trailingSLTic + tradeObj.profitTgtIncTic;
					if(Position.MarketPosition == MarketPosition.Long)
						slPrc = Position.AveragePrice+TickSize*tradeObj.profitLockMinTic;
					if(Position.MarketPosition == MarketPosition.Short)
						slPrc = Position.AveragePrice-TickSize*tradeObj.profitLockMinTic;
					indicatorProxy.PrintLog(true, !backTest, 
						AccName + "- update SL Min: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= ("
						+ tradeObj.slTrailing + "," + tradeObj.trailingSLTic + "," + slPrc + ")");
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
		protected bool Dep_ChangeSLPT()
		{
//			int bse = BarsSinceEntry();
//			double timeSinceEn = -1;
//			if(bse > 0) {
//				timeSinceEn = indicatorProxy.GetMinutesDiff(Time[0], Time[bse]);
//			}
			int prtLevel = 0;
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			double pl = CheckUnrealizedPnL();
			double pl_tics = CheckUnrealizedPnLTicks();
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			if(pl_tics >= (tradeObj.profitLockMaxTic+2*tradeObj.profitTgtIncTic)) {
				indicatorProxy.TraceMessage(this.Name, prtLevel);
				//SetTrailingStopLossOrder(tradeObj.entrySignalName.ToString());
			} // start trailing SL
			else if (pl >= (tradeObj.profitTargetAmt+2*GetCurrencyByTicks(tradeObj.profitTgtIncTic))) {
				indicatorProxy.TraceMessage(this.Name, prtLevel);
				//MoveProfitTarget();
			} // move PT, lock SL
			else if (pl_tics >= (tradeObj.profitLockMinTic+2*tradeObj.stopLossIncTic)) {
				indicatorProxy.TraceMessage(this.Name, prtLevel);
				//LockMinProfitTarget();
			} // PT no change, lock SL
			else if (pl >= tradeObj.breakEvenAmt) {
				indicatorProxy.TraceMessage(this.Name, prtLevel);
				SetBreakEvenOrder(GetAvgPrice());
			} // PT no change, BE SL
			else {
				indicatorProxy.TraceMessage(this.Name, prtLevel);
				SetSimpleExitOCO(tradeObj.entrySignalName.ToString());
			} // set simple PT, SL
			
//			if(Position.Quantity == 0)
//				indicatorProxy.PrintLog(true, !backTest, 
//					AccName + "- ChangeSLPT=0: (PnL, posAvgPrc, MM_BreakEvenAmt)=(" + pl + "," + Position.AveragePrice + "," + tradeObj.breakEvenAmt + ")");
//			else
//				indicatorProxy.PrintLog(true, !backTest, 
//					AccName + "- ChangeSLPT<>0: (PnL, posAvgPrc, MM_BreakEvenAmt)=(" + pl + "," + Position.AveragePrice + "," + tradeObj.breakEvenAmt + ")");
			indicatorProxy.TraceMessage(this.Name, prtLevel);
			// If not flat print out unrealized PnL
    		if (Position.MarketPosition != MarketPosition.Flat) 
			{
         		//giParabSAR.PrintLog(true, !backTest, log_file, AccName + "- Open PnL: " + pl);
				//int nChkPnL = (int)(timeSinceEn/minutesChkPnL);
				double curPTTics = -1;
				double slPrc = tradeObj.BracketOrder.OCOOrder.StopLossOrder == null ?
					Position.AveragePrice : tradeObj.BracketOrder.OCOOrder.StopLossOrder.StopPrice;
				
				//MoveProfitTarget();

				indicatorProxy.PrintLog(true, !backTest,
					AccName + "- SL Breakeven: (PnL, posAvgPrc, MM_BreakEvenAmt)=(" + pl + "," + Position.AveragePrice + "," + tradeObj.breakEvenAmt + ")");
				//ChangeBreakEven();
				//ChangeStopLoss();
				
				if(tradeObj.BracketOrder.OCOOrder.StopLossOrder == null ||
					(Position.MarketPosition == MarketPosition.Long && slPrc > tradeObj.BracketOrder.OCOOrder.StopLossOrder.StopPrice) ||
					(Position.MarketPosition == MarketPosition.Short && slPrc < tradeObj.BracketOrder.OCOOrder.StopLossOrder.StopPrice))
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
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitTgtAmt", GroupName = "MoneyMgmt", Order = 0)]	
        public double MM_ProfitTargetAmt
        {
            get{return mm_ProfitTargetAmt;}
            set{mm_ProfitTargetAmt = Math.Max(0, value);}
        }

        [Description("Ticks amount for profit target")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitTgtTic", GroupName = "MoneyMgmt", Order = 1)]	
        public int MM_ProfitTgtTic
        {
            get{return mm_ProfitTgtTic;}
            set{mm_ProfitTgtTic = Math.Max(0, value);}
        }
		
        [Description("Money amount for profit target increasement")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitTgtIncTic", GroupName = "MoneyMgmt", Order = 2)]	
        public int MM_ProfitTgtIncTic
        {
            get{return mm_ProfitTgtIncTic;}
            set{mm_ProfitTgtIncTic = Math.Max(0, value);}
        }
		
        [Description("Tick amount for min profit locking")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitLockMinTic", GroupName = "MoneyMgmt", Order = 3)]	
        public int MM_ProfitLockMinTic
        {
            get{return mm_ProfitLockMinTic;}
            set{mm_ProfitLockMinTic = Math.Max(0, value);}
        }

		[Description("Tick amount for max profit locking")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitLockMaxTic", GroupName = "MoneyMgmt", Order = 4)]	
        public int MM_ProfitLockMaxTic
        {
            get{return mm_ProfitLockMaxTic;}
            set{mm_ProfitLockMaxTic = Math.Max(0, value);}
        }
		
        [Description("Money amount of stop loss")]
 		[Range(0, double.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "StopLossAmt", GroupName = "MoneyMgmt", Order = 5)]	
        public double MM_StopLossAmt
        {
            get{return mm_StopLossAmt;}
            set{mm_StopLossAmt = Math.Max(0, value);}
        }
		
		[Description("Ticks amount for stop loss")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "StopLossTic", GroupName = "MoneyMgmt", Order = 6)]	
        public int MM_StopLossTic
        {
            get{return mm_StopLossTic;}
            set{mm_StopLossTic = Math.Max(0, value);}
        }
		
		[Description("Money amount for stop loss increasement")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "StopLossIncTic", GroupName = "MoneyMgmt", Order = 7)]	
        public int MM_StopLossIncTic
        {
            get{return mm_StopLossIncTic;}
            set{mm_StopLossIncTic = Math.Max(0, value);}
        }

        [Description("Break Even amount")]
 		[Range(0, double.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BreakEvenAmt", GroupName = "MoneyMgmt", Order = 8)]	
        public double MM_BreakEvenAmt
        {
            get{return mm_BreakEvenAmt;}
            set{mm_BreakEvenAmt = Math.Max(0, value);}
        }
		
        [Description("Money amount of trailing stop loss")]
 		[Range(0, double.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TrailingStopLossAmt", GroupName = "MoneyMgmt", Order = 9)]	
        public double MM_TrailingStopLossAmt
        {
            get{return mm_TrailingStopLossAmt;}
            set{mm_TrailingStopLossAmt = Math.Max(0, value);}
        }
        
		[Description("Ticks amount of trailing stop loss")]
 		[Range(0, int.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TrailingStopLossTic", GroupName = "MoneyMgmt", Order = 10)]	
        public int MM_TrailingStopLossTic
        {
            get{return mm_TrailingStopLossTic;}
            set{mm_TrailingStopLossTic = Math.Max(0, value);}
		}

        [Description("Percent amount of trailing stop loss")]
 		[Range(0, double.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TrailingStopLossPercent", GroupName = "MoneyMgmt", Order = 11)]	
        public double MM_TrailingStopLossPercent
        {
            get{return mm_TrailingStopLossPercent;}
            set{mm_TrailingStopLossPercent = Math.Max(0, value);}
        }
		
		[Description("Use trailing stop loss every bar")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SLTrailing", GroupName = "MoneyMgmt", Order = 12)]	
        public bool MM_SLTrailing
        {
            get{return mm_SLTrailing;}
            set{mm_SLTrailing = value;}
        }
		
		[Description("Use trailing profit target every bar")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PTTrailing", GroupName = "MoneyMgmt", Order = 13)]	
        public bool MM_PTTrailing
        {
            get{return mm_PTTrailing;}
            set{mm_PTTrailing = value;}
        }

		[Description("Calculation mode for profit target")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PTCalculationMode", GroupName = "MoneyMgmt", Order = 14)]	
        public CalculationMode MM_PTCalculationMode
        {
            get{return mm_PTCalculationMode;}
            set{mm_PTCalculationMode = value;}
        }

		[Description("Calculation mode for stop loss")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SLCalculationMode", GroupName = "MoneyMgmt", Order = 15)]	
        public CalculationMode MM_SLCalculationMode
        {
            get{return mm_SLCalculationMode;}
            set{mm_SLCalculationMode = value;}
        }

		[Description("Calculation mode for break even")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BECalculationMode", GroupName = "MoneyMgmt", Order = 16)]	
        public CalculationMode MM_BECalculationMode
        {
            get{return mm_BECalculationMode;}
            set{mm_BECalculationMode = value;}
        }
		
		[Description("Calculation mode for trailing stop loss")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TLSLCalculationMode", GroupName = "MoneyMgmt", Order = 17)]	
        public CalculationMode MM_TLSLCalculationMode
        {
            get{return mm_TLSLCalculationMode;}
            set{mm_TLSLCalculationMode = value;}
        }		
		
		[Description("Daily Loss Limit amount")]
 		[Range(double.MinValue, double.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "DailyLossLmt", GroupName = "MoneyMgmt", Order = 18)]	
        public double MM_DailyLossLmt
        {
            get{return mm_DailyLossLmt;}
            set{mm_DailyLossLmt = Math.Min(-100, value);}
        }

		[Description("Profit Factor")]
 		[Range(0, double.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitFactor", GroupName = "MoneyMgmt", Order = 19)]	
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
		private double mm_ProfitFactor = 1.5;
		
		#endregion
	}
}
