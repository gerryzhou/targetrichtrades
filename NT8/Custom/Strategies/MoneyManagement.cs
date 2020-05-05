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
		
		public virtual bool IsProfitFactorValid(double risk, double reward, double pfMin, double pfMax) {
			bool is_valid = false;
			if(risk > 0 && reward > 0 && pfMin <= reward/risk && pfMax >= reward/risk)
				is_valid = true;
			return is_valid;
		}
		
		public double CalProfitTargetAmt(double price, double profitFactorMin, double profitFactorMax) {
			IndicatorProxy.PrintLog(true, true, 
				CurrentBar + ":CalProfitTargetAmt;IsLiveTrading=" + IsLiveTrading() +
				";=GetMarketPosition()" + GetMarketPosition().ToString() +
				";MM_SLCalculationMode=" + MM_SLCalculationMode.ToString() +
				";avgPrice=" + price +
				";CurrentTrade.TradeAction.StopLossPrice=" + CurrentTrade.TradeAction.StopLossPrice +
				";CurrentTrade.TradeAction.ProfitTargetPrice=" + CurrentTrade.TradeAction.ProfitTargetPrice
				);			
			switch(MM_SLCalculationMode) {
				case CalculationMode.Currency:
					if(profitFactorMax > 0)
						MM_ProfitTargetAmt = profitFactorMax*MM_StopLossAmt;
					break;
//				case CalculationMode.Price:
//					if(profitFactor == 0)
//						MM_ProfitTargetAmt = MM_ProfitTargetAmt;			
//					else
//						MM_ProfitTargetAmt = 
//							profitFactor*Math.Abs(price-CurrentTrade.TradeAction.StopLossPrice)*Instrument.MasterInstrument.PointValue;
//					break;
			}
			IndicatorProxy.PrintLog(true, true, 
				CurrentBar + ":CalProfitTargetAmt;IsLiveTrading=" + IsLiveTrading() +
				";=GetMarketPosition()" + GetMarketPosition().ToString() +
				";CurrentTrade.PTCalculationMode=" + MM_PTCalculationMode.ToString() +
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
		public void CalExitOcoPrice(double avgPrice, double profitFactorMin, double profitFactorMax) {
			int prtLevel = 0;
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
			IndicatorProxy.PrintLog(true, true, 
				CurrentBar + ":CalExitOcoPrice;IsLiveTrading=" + IsLiveTrading() +
				";=GetMarketPosition()" + GetMarketPosition().ToString() +
				";MM_SLCalculationMode=" + MM_SLCalculationMode.ToString() +
				";avgPrice=" + avgPrice +
				";CurrentTrade.TradeAction.StopLossPrice=" + CurrentTrade.TradeAction.StopLossPrice +
				";CurrentTrade.TradeAction.ProfitTargetPrice=" + CurrentTrade.TradeAction.ProfitTargetPrice
				);
			switch(MM_SLCalculationMode) {
				case CalculationMode.Currency:
					if(profitFactorMax > 0)
						MM_ProfitTargetAmt = profitFactorMax*MM_StopLossAmt;
					if(GetMarketPosition() == MarketPosition.Long) {
						CurrentTrade.TradeAction.StopLossPrice = avgPrice - IndicatorProxy.GetPriceByCurrency(MM_StopLossAmt);
						CurrentTrade.TradeAction.ProfitTargetPrice = avgPrice + IndicatorProxy.GetPriceByCurrency(MM_ProfitTargetAmt);
					}
					else if(GetMarketPosition() == MarketPosition.Short) {
						CurrentTrade.TradeAction.StopLossPrice = avgPrice + IndicatorProxy.GetPriceByCurrency(MM_StopLossAmt);
						CurrentTrade.TradeAction.ProfitTargetPrice = avgPrice - IndicatorProxy.GetPriceByCurrency(MM_ProfitTargetAmt);
					}
					break;
				case CalculationMode.Ticks:
					if(profitFactorMax > 0)
						MM_ProfitTgtTic = (int)(profitFactorMax*MM_StopLossTic);
					if(GetMarketPosition() == MarketPosition.Long) {
						CurrentTrade.TradeAction.StopLossPrice = avgPrice - IndicatorProxy.GetPriceByTicks(MM_StopLossTic);
						CurrentTrade.TradeAction.ProfitTargetPrice = avgPrice + IndicatorProxy.GetPriceByTicks(MM_ProfitTgtTic);
					}
					else if(GetMarketPosition() == MarketPosition.Short) {
						CurrentTrade.TradeAction.StopLossPrice = avgPrice + IndicatorProxy.GetPriceByTicks(MM_StopLossTic);
						CurrentTrade.TradeAction.ProfitTargetPrice = avgPrice - IndicatorProxy.GetPriceByTicks(MM_ProfitTgtTic);
					}					
					break;
//				case CalculationMode.Price:
//					if(profitFactor > 0)
//						MM_ProfitTargetAmt = 
//							profitFactor*Math.Abs(avgPrice-CurrentTrade.TradeAction.StopLossPrice)*Instrument.MasterInstrument.PointValue;
//					break;
			}
			IndicatorProxy.Log2Disk = true;
			IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				CurrentBar + ":CalExitOcoPrice" +
				";=GetMarketPosition()" + GetMarketPosition().ToString() +
				";MM_SLCalculationMode=" + MM_SLCalculationMode.ToString() +
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

//			if(pl_tics >= (MM_ProfitLockMaxTic+2*MM_ProfitTgtIncTic))
//				overAmt = pl_tics - MM_ProfitLockMaxTic;
			
			switch(MM_TLSLCalculationMode) {
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
			+ ";TLSLCalculationMode=" + MM_TLSLCalculationMode
			+ ";trailingSLPrice=" + CurrentTrade.TradeAction.StopLossPrice
			+ ";trailingPTTic=" + CurrentTrade.TradeAction.TrailingProfitTargetTics
			+ ";trailingSLTic=" + MM_TrailingStopLossTic
			+ ";avgPrice=" + avgPrice);
			
			if(CurrentTrade.TradeAction.TrailingProfitTargetTics < MM_ProfitLockMaxTic)
				CurrentTrade.TradeAction.TrailingProfitTargetTics = Math.Max(MM_ProfitLockMaxTic, pl_tics-MM_TrailingStopLossTic);
			else
				CurrentTrade.TradeAction.TrailingProfitTargetTics = Math.Max(CurrentTrade.TradeAction.TrailingProfitTargetTics, pl_tics-MM_TrailingStopLossTic);
			
			IndicatorProxy.TraceMessage(this.Name, prtLevel);
			if(GetMarketPosition() == MarketPosition.Long) {
				CurrentTrade.TradeAction.StopLossPrice = IndicatorProxy.MovePriceByTicks(avgPrice, CurrentTrade.TradeAction.TrailingProfitTargetTics);
			}
			else if(GetMarketPosition() == MarketPosition.Short) {
				CurrentTrade.TradeAction.StopLossPrice = IndicatorProxy.MovePriceByTicks(avgPrice, -CurrentTrade.TradeAction.TrailingProfitTargetTics);
			}
			
			IndicatorProxy.PrintLog(true, IsLiveTrading(), CurrentBar + ":CalTLSLPrice"
			+ ";TLSLCalculationMode=" + MM_TLSLCalculationMode
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
					SetSimpleExitOCO();
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
			if(IndicatorProxy.GetCurrencyByTicks(CurrentTrade.TradeAction.TrailingProfitTargetTics) <= MM_ProfitTargetAmt) { //first time move the profit target
				CurrentTrade.TradeAction.TrailingProfitTargetTics = IndicatorProxy.GetTicksByCurrency(MM_ProfitTargetAmt) + MM_ProfitTgtIncTic;
			}
			else {
				double pl_tics = CheckUnrealizedPnLTicks();
				if(pl_tics >= (CurrentTrade.TradeAction.TrailingProfitTargetTics - 2*MM_ProfitTgtIncTic)) {
					CurrentTrade.TradeAction.TrailingProfitTargetTics = CurrentTrade.TradeAction.TrailingProfitTargetTics + MM_ProfitTgtIncTic;
					//CurrentTrade.TradeAction.ProfitTargetPrice = MovePriceByTicks(avgPrc, MM_ProfitTgtIncTic);
				}
			}
			
			double newPrc;
			if(GetMarketPosition() == MarketPosition.Long) {
				newPrc = IndicatorProxy.MovePriceByTicks(avgPrc, CurrentTrade.TradeAction.TrailingProfitTargetTics);
				CurrentTrade.TradeAction.ProfitTargetPrice = Math.Max(newPrc, CurrentTrade.TradeAction.ProfitTargetPrice);
			}
			else if(GetMarketPosition() == MarketPosition.Short) {
				newPrc = IndicatorProxy.MovePriceByTicks(avgPrc, -CurrentTrade.TradeAction.TrailingProfitTargetTics);
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
				newPrc = IndicatorProxy.MovePriceByTicks(avgPrc, MM_ProfitLockMinTic);
				CurrentTrade.TradeAction.StopLossPrice = Math.Max(newPrc, CurrentTrade.TradeAction.StopLossPrice);
			} 
			else if(GetMarketPosition() == MarketPosition.Short) {
				newPrc = IndicatorProxy.MovePriceByTicks(avgPrc, -MM_ProfitLockMinTic);				
				CurrentTrade.TradeAction.StopLossPrice = Math.Min(newPrc, CurrentTrade.TradeAction.StopLossPrice);
			}
			MM_SLCalculationMode = CalculationMode.Price;
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
			if(pl >= MM_BreakEvenAmt)
				overAmt = pl - MM_BreakEvenAmt;
			return overAmt;
		}
		
		/// <summary>
		/// Get the difference of PnL over the MinLockProfitTarget threshold
		/// return -1 if PnL is below the MinLockProfitTarget threshold
		/// </summary>
		/// <returns></returns>		
		public int isOverMinLockPT(int pl_tics) {
			int overAmt = -1;			
			if(pl_tics >= (MM_ProfitLockMinTic+2*MM_StopLossIncTic))
			//&& (pl >= (MM_ProfitTargetAmt+2*GetCurrencyByTicks(MM_ProfitTgtIncTic))))
				overAmt = pl_tics - MM_ProfitLockMinTic;
			return overAmt;			
		}

		/// <summary>
		/// Get the difference of PnL over the MinLockProfitTarget threshold
		/// return -1 if PnL is below the MinLockProfitTarget threshold
		/// </summary>
		/// <returns></returns>		
		public int isOverMaxLockPT(int pl_tics) {
			int overAmt = -1;
			if(pl_tics >= (MM_ProfitLockMaxTic+2*MM_ProfitTgtIncTic))
				overAmt = pl_tics - MM_ProfitLockMaxTic;
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
			return IndicatorProxy.GetTicksByCurrency(CheckUnrealizedPnL());
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
			if (IndicatorProxy.IsLastBarOnChart() > 0 && SystemPerformance.AllTrades.Count > 0)
			{
			    foreach (Trade myTrade in SystemPerformance.AllTrades)
			    {
			    	if (myTrade.Entry.MarketPosition == MarketPosition.Long)
			        	IndicatorProxy.PrintLog(true, IsLiveTrading(), 
						String.Format("#{0}, ProfitCurrency={1}", myTrade.TradeNumber, myTrade.ProfitCurrency));
			    }
				IndicatorProxy.PrintLog(true, IsLiveTrading(), 
					String.Format("There are {0} trades, NetProfit={1}",
					SystemPerformance.AllTrades.Count, SystemPerformance.AllTrades.TradesPerformance.NetProfit));
			}
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
			TradeAction ta = CurrentTrade.TradeAction;
			if(ta.StopLossPrice != null && ta.StopLossPrice > 0 &&
				ta.ProfitTargetPrice != null && ta.ProfitTargetPrice > 0) {
				if(GetMarketPosition() == MarketPosition.Long) {
					isValid = (ta.ProfitTargetPrice > ta.StopLossPrice);
				}
				else if(GetMarketPosition() == MarketPosition.Short) {
					isValid = (ta.ProfitTargetPrice < ta.StopLossPrice);
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

		public virtual double GetValidSLPTPrice(double enPrc, List<double> prices, PriceGap pg, SupportResistanceType srt) {
			//double avgPrc = GetAvgPrice();
			double prcGap = pg==PriceGap.Tighter? double.MaxValue : 0;
			double prc = 0;

			switch(srt) {
				case SupportResistanceType.Resistance: //PriceGap.Tighter:
					foreach(double p in prices) {
						if(p > enPrc) {
							double pGap = p - enPrc;
							if(pg == PriceGap.Tighter) {
								prcGap = Math.Min(prcGap, pGap);
							}
							else if (pg == PriceGap.Wider) {
								prcGap = Math.Max(prcGap, pGap);
							}
						}
					}
					if(prcGap > 0 && prcGap < double.MaxValue)
						prc = enPrc + prcGap;
					break;
				case SupportResistanceType.Support: //PriceGap.Wider:
					foreach(double p in prices) {
						if(p < enPrc) {
							double pGap = enPrc - p;
							if(pg == PriceGap.Tighter) {
								prcGap = Math.Min(prcGap, pGap);
							}
							else if (pg == PriceGap.Wider) {
								prcGap = Math.Max(prcGap, pGap);
							}
						}
					}
					if(prcGap > 0 && prcGap < double.MaxValue)
						prc = enPrc - prcGap;
					break;
			}				
			
			return prc;
		}
		
		public virtual double GetEntryPrice(SupportResistanceType srt) {
			return 0;
		}
		
		public virtual double GetStopLossPrice(SupportResistanceType srt, double price) {
			return 0;
		}
		
		public virtual double GetStopLossOffset(SupportResistanceType srt, double price) {
			return 0;
		}

		public virtual double GetProfitTargetPrice(SupportResistanceType srt, double price) {
			return 0;
		}
		
		public virtual double GetProfitTargetOffset(SupportResistanceType srt, double price) {
			return 0;
		}
		
		public virtual double GetValidStopLossPrice(List<double> prices, PriceGap pg) {
			double prc = 0;
			double curPrc = Close[0]; //GetAvgPrice();
			switch(GetMarketPosition()) {
				case MarketPosition.Long:
					prc = GetValidSLPTPrice(curPrc, prices, pg, SupportResistanceType.Support);
					break;
				case MarketPosition.Short:
					prc = GetValidSLPTPrice(curPrc, prices, pg, SupportResistanceType.Resistance);
					break;
			}
			
			return prc;
		}
		
		public virtual double GetValidStopLossPrice(double enPrc) {
			double prc = 0, slOffset = 0;
			if(enPrc <= 0)
				enPrc = Close[0];
			
			switch(MM_SLCalculationMode) {
				case CalculationMode.Currency:
					slOffset = IndicatorProxy.GetPriceByCurrency(MM_StopLossAmt);
					break;
			}

			switch(GetMarketPosition()) {
				case MarketPosition.Long:
					prc = enPrc - slOffset;
					break;
				case MarketPosition.Short:
					prc = enPrc + slOffset;
					break;
			}
			
			return prc;
		}
		
		public virtual double GetProfitTargetPrice(SupportResistanceType srt) {
			return 0;
		}
		
		public virtual double GetValidProfitTargetPrice(List<double> prices, PriceGap pg) {
			double prc = 0;
			double avgPrc = GetAvgPrice();
			switch(GetMarketPosition()) {
				case MarketPosition.Long:
					prc = GetValidSLPTPrice(avgPrc, prices, pg, SupportResistanceType.Resistance);
					break;
				case MarketPosition.Short:
					prc = GetValidSLPTPrice(avgPrc, prices, pg, SupportResistanceType.Support);
					break;
			}
			
			return 0;
		}
		
		public virtual double GetValidProfitTargetPrice(double enPrc) {
			double prc = 0, ptOffset = 0;
			if(enPrc <= 0)
				enPrc = Close[0];
			
			switch(MM_PTCalculationMode) {
				case CalculationMode.Currency:
					ptOffset = IndicatorProxy.GetPriceByCurrency(MM_ProfitTargetAmt);
					break;
			}

			switch(GetMarketPosition()) {
				case MarketPosition.Long:
					prc = enPrc + ptOffset;
					break;
				case MarketPosition.Short:
					prc = enPrc - ptOffset;
					break;
			}
			
			return prc;
		}
		
		/// <summary>
		/// Get the default OCO exit order prices if no valid prieces were assigned
		/// by Indicator signals or command, perform/rule, etc.
		/// </summary>
		/// <param name="mp"></param>
		/// <param name="enPrc"></param>
		/// <param name="ta"></param>
		/// <returns></returns>
		public virtual TradeAction GetOcoPrice(double enPrc, TradeAction ta) {
			double slOffset, ptOffset;
			if(enPrc <= 0)
				enPrc = Close[0];
			MarketPosition mp = GetMarketPosition();
			
			switch(MM_SLCalculationMode) {
				case CalculationMode.Currency:
					slOffset = IndicatorProxy.GetPriceByCurrency(MM_StopLossAmt);
					break;
			}
			switch(MM_PTCalculationMode) {
				case CalculationMode.Currency:
					ptOffset = IndicatorProxy.GetPriceByCurrency(MM_ProfitTargetAmt);
					break;
			}
			
			if(mp == MarketPosition.Long) {
				ta.StopLossPrice = enPrc - IndicatorProxy.GetPriceByCurrency(MM_StopLossAmt);
				ta.ProfitTargetPrice = enPrc + IndicatorProxy.GetPriceByCurrency(MM_ProfitTargetAmt);
			} else if(mp == MarketPosition.Short) {
				ta.StopLossPrice = enPrc + IndicatorProxy.GetPriceByCurrency(MM_StopLossAmt);
				ta.ProfitTargetPrice = enPrc - IndicatorProxy.GetPriceByCurrency(MM_ProfitTargetAmt);
			}
			
			return ta;
		}
		#endregion

		#region Depricated methods
		/// <summary>
		/// Trailing max and min profits then converted to trailing stop after over the max
		/// </summary>
		public void Dep_ChangeStopLoss() {
			if(!MM_SLTrailing) return;
			else {
				double pl = CheckUnrealizedPnL();
				double slPrc = Position.AveragePrice;
				if(MM_TrailingStopLossTic > MM_ProfitLockMaxTic && pl >= IndicatorProxy.GetTickValue()*(MM_TrailingStopLossTic + 2*MM_ProfitTgtIncTic)) {
					MM_TrailingStopLossTic = MM_TrailingStopLossTic + MM_ProfitTgtIncTic;
					if(Position.MarketPosition == MarketPosition.Long)
						slPrc = Position.AveragePrice+TickSize*MM_TrailingStopLossTic;
					if(Position.MarketPosition == MarketPosition.Short)
						slPrc = Position.AveragePrice-TickSize*MM_TrailingStopLossTic;
					Print(AccName + "- update SL over Max: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= (" + MM_SLTrailing + "," + MM_TrailingStopLossTic + "," + slPrc + ")");						
				}
				if(MM_TrailingStopLossTic > MM_ProfitLockMaxTic && pl >= IndicatorProxy.GetTickValue()*(MM_TrailingStopLossTic + 2*MM_ProfitTgtIncTic)) {
					MM_TrailingStopLossTic = MM_TrailingStopLossTic + MM_ProfitTgtIncTic;
					if(CurrentTrade.BracketOrder.OCOOrder.StopLossOrder != null)
						CancelOrder(CurrentTrade.BracketOrder.OCOOrder.StopLossOrder);
					if(CurrentTrade.BracketOrder.OCOOrder.ProfitTargetOrder != null)
						CancelOrder(CurrentTrade.BracketOrder.OCOOrder.ProfitTargetOrder);
					SetTrailStop(CalculationMode.Currency, MM_TrailingStopLossAmt);
					IndicatorProxy.PrintLog(true, IsLiveTrading(),
						AccName + "- SetTrailStop over SL Max: PnL=" + pl +
						"(slTrailing, trailingSLTic, slPrc)= (" + MM_SLTrailing + "," + MM_TrailingStopLossTic + "," + slPrc + ")");						
				}
				else if(pl >= IndicatorProxy.GetTickValue()*(MM_ProfitLockMaxTic + 2*MM_ProfitTgtIncTic)) { // lock max profits
					MM_TrailingStopLossTic = MM_TrailingStopLossTic + MM_ProfitTgtIncTic;
					if(Position.MarketPosition == MarketPosition.Long)
						slPrc = MM_TrailingStopLossTic > MM_ProfitLockMaxTic ? Position.AveragePrice+TickSize*MM_TrailingStopLossTic : Position.AveragePrice+TickSize*MM_ProfitLockMaxTic;
					if(Position.MarketPosition == MarketPosition.Short)
						slPrc = MM_TrailingStopLossTic > MM_ProfitLockMaxTic ? Position.AveragePrice-TickSize*MM_TrailingStopLossTic :  Position.AveragePrice-TickSize*MM_ProfitLockMaxTic;
					IndicatorProxy.PrintLog(true, IsLiveTrading(),
						AccName + "- update SL Max: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= ("
						+ MM_SLTrailing + "," + MM_TrailingStopLossTic + "," + slPrc + ")");
					//SetStopLoss(CalculationMode.Price, slPrc);
				}
				else if(pl >= IndicatorProxy.GetTickValue()*(MM_ProfitLockMinTic + 2*MM_ProfitTgtIncTic)) { //lock min profits
					MM_TrailingStopLossTic = MM_TrailingStopLossTic + MM_ProfitTgtIncTic;
					if(Position.MarketPosition == MarketPosition.Long)
						slPrc = Position.AveragePrice+TickSize*MM_ProfitLockMinTic;
					if(Position.MarketPosition == MarketPosition.Short)
						slPrc = Position.AveragePrice-TickSize*MM_ProfitLockMinTic;
					IndicatorProxy.PrintLog(true, IsLiveTrading(), 
						AccName + "- update SL Min: PnL=" + pl + "(slTrailing, trailingSLTic, slPrc)= ("
						+ MM_SLTrailing + "," + MM_TrailingStopLossTic + "," + slPrc + ")");
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
			if(pl_tics >= (MM_ProfitLockMaxTic+2*MM_ProfitTgtIncTic)) {
				IndicatorProxy.TraceMessage(this.Name, prtLevel);
				//SetTrailingStopLossOrder(CurrentTrade.TradeAction.EntrySignal.SignalName.ToString());
			} // start trailing SL
			else if (pl >= (MM_ProfitTargetAmt+2*IndicatorProxy.GetCurrencyByTicks(MM_ProfitTgtIncTic))) {
				IndicatorProxy.TraceMessage(this.Name, prtLevel);
				//MoveProfitTarget();
			} // move PT, lock SL
			else if (pl_tics >= (MM_ProfitLockMinTic+2*MM_StopLossIncTic)) {
				IndicatorProxy.TraceMessage(this.Name, prtLevel);
				//LockMinProfitTarget();
			} // PT no change, lock SL
			else if (pl >= MM_BreakEvenAmt) {
				IndicatorProxy.TraceMessage(this.Name, prtLevel);
				SetBreakEvenOrder(GetAvgPrice());
			} // PT no change, BE SL
			else {
				IndicatorProxy.TraceMessage(this.Name, prtLevel);
				SetSimpleExitOCO();
			} // set simple PT, SL
			
//			if(Position.Quantity == 0)
//				indicatorProxy.PrintLog(true, !backTest, 
//					AccName + "- ChangeSLPT=0: (PnL, posAvgPrc, MM_BreakEvenAmt)=(" + pl + "," + Position.AveragePrice + "," + MM_BreakEvenAmt + ")");
//			else
//				indicatorProxy.PrintLog(true, !backTest, 
//					AccName + "- ChangeSLPT<>0: (PnL, posAvgPrc, MM_BreakEvenAmt)=(" + pl + "," + Position.AveragePrice + "," + MM_BreakEvenAmt + ")");
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
					AccName + "- SL Breakeven: (PnL, posAvgPrc, MM_BreakEvenAmt)=(" + pl + "," + Position.AveragePrice + "," + MM_BreakEvenAmt + ")");
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

		[Description("Profit Factor Min")]
 		[Range(0, double.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitFactorMin", GroupName = GPS_MONEY_MGMT, Order = ODG_ProfitFactorMin)]	
        public double MM_ProfitFactorMin
        {
            get{return mm_ProfitFactorMin;}
            set{mm_ProfitFactorMin = Math.Max(0, value);}
        }
		
		[Description("Profit Factor Max")]
 		[Range(0, double.MaxValue), NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ProfitFactorMax", GroupName = GPS_MONEY_MGMT, Order = ODG_ProfitFactorMax)]	
        public double MM_ProfitFactorMax
        {
            get{return mm_ProfitFactorMax;}
            set{mm_ProfitFactorMax = Math.Max(0, value);}
        }
		
		[Description("Stop Loss Price Gap Preference")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SLPriceGapPref", GroupName = GPS_MONEY_MGMT, Order = ODG_SLPriceGapPref)]	
        public PriceGap MM_SLPriceGapPref
        {
            get{return mm_SLPriceGapPref;}
            set{mm_SLPriceGapPref = value;}
        }

		[Description("Profit Target Price Gap Preference")]
 		[NinjaScriptProperty, XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PTPriceGapPref", GroupName = GPS_MONEY_MGMT, Order = ODG_PTPriceGapPref)]	
        public PriceGap MM_PTPriceGapPref
        {
            get{return mm_PTPriceGapPref;}
            set{mm_PTPriceGapPref = value;}
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
		private double mm_ProfitFactorMin = 0.1;
		private double mm_ProfitFactorMax = 2.5;
		
		private PriceGap mm_SLPriceGapPref = PriceGap.Tighter;
		
		private PriceGap mm_PTPriceGapPref = PriceGap.Wider;		
		
		#endregion
	}
}
