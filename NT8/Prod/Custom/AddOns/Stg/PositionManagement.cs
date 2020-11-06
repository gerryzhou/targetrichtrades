#region Using declarations
using System;
using System.Collections.Generic;
using NinjaTrader.Cbi;

using NinjaTrader.Data;

#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	/// <summary>
	/// @"Manage position size, scale in/out, etc.";
	/// </summary>
	public partial class GStrategyBase : Strategy
	{
		#region Utils Functions
		public int HasPosition() {
			if(PrintOut > 1 && !IsInStrategyAnalyzer)
				IndicatorProxy.TraceMessage(this.Name, 0);
			int pos = 0;
			if(IsLiveTrading()) {
				//if(PositionAccount != null)
				pos = PositionAccount.Quantity;
			}
			else //if(Position != null)
				pos = Position.Quantity;
			return pos;
		}
		
		public int HasPosition(int idx) {
			if(PrintOut > 1 && !IsInStrategyAnalyzer)
				IndicatorProxy.TraceMessage(this.Name, 0);
			if(idx < 0) return HasPosition();
			int pos = 0;
			if(IsLiveTrading()) {
				//if(PositionAccount != null)
				pos = PositionsAccount[idx].Quantity;
			}
			else //if(Position != null)
				pos = Positions[idx].Quantity;
			return pos;
		}
		
		public bool HasPairPosition(int startIndex) {
			if(PrintOut > 1 && !IsInStrategyAnalyzer)
				IndicatorProxy.TraceMessage(this.Name, 0);
			bool pos = false;
			if(GetMarketPosition(startIndex) != MarketPosition.Flat 
				&& GetMarketPosition(startIndex+1) != MarketPosition.Flat) {
				pos = true;
			}
			return pos;
		}
		
		public bool HasPairPosition() {
			return HasPairPosition(0);
		}
		
		public virtual bool HasPositions(int[] idxs, int posExp) {
			int cnt = 0;
			for(int i=0; i<idxs.Length; i++) {
				if(GetMarketPosition(idxs[i]) != MarketPosition.Flat)
					cnt ++;
			}
			if(cnt >= posExp)
				return true;
			else
				return false;
		}
		
		public double GetAvgPrice() {
			MasterInstrument maIns = Bars.Instrument.MasterInstrument;
			if(IsLiveTrading())
				return maIns.RoundToTickSize(PositionAccount.AveragePrice);
			else
				return maIns.RoundToTickSize(Position.AveragePrice);
		}
				
		public MarketPosition GetMarketPosition() {
			if(IsLiveTrading())
				return PositionAccount.MarketPosition;
			else
				return Position.MarketPosition;
		}
		
		public MarketPosition GetMarketPosition(int idx) {
			if(idx < 0) return GetMarketPosition();
			
			if(IsLiveTrading())
				return PositionsAccount[idx].MarketPosition;
			else
				return Positions[idx].MarketPosition;
		}
		
		public double GetUnrealizedPnL() {
			if(IsLiveTrading())
				return PositionAccount.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0]);
			else return
				Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0]);
		}
		
		public PositionStatus GetPositionStatus(int prevPos) {
			int curPos = HasPosition();
			if(curPos == 0) {
				if(prevPos != 0)
					return PositionStatus.Liquidate;
				else return PositionStatus.Flat;
			} else {
				if(prevPos == 0)
					return PositionStatus.NewEstablished;
				else if(prevPos == curPos)
					return PositionStatus.Hold;
				else if(Math.Abs(prevPos) < Math.Abs(curPos))
					return PositionStatus.ScaledIn;
				else if(Math.Abs(prevPos) > Math.Abs(curPos))
					return PositionStatus.ScaledOut;
			}
			
			return PositionStatus.UnKnown;
		}
		#endregion
		
		#region Sizing Functions
		
		/// <summary>
		/// CapRatio: ES:RTY=1.7:1, NQ:RTY=2.1:1, NQ:ES=1.25:1
		/// </summary>		
		public virtual int GetTradeQuantity(int idx, double ratio) {
			return (int)(ratio*DefaultQuantity);
		}
		#endregion
		
		#region Performance Functions
		public virtual double GetPairUnrealizedPnL(PerformanceUnit unit) {
			double pnl0, pnl1;
			if(IsLiveTrading()) {
				pnl0 = PositionsAccount[0].GetUnrealizedProfitLoss(unit, Closes[0][0]);
				pnl1 = PositionsAccount[1].GetUnrealizedProfitLoss(unit, Closes[1][0]);				
			}
			else {
				pnl0 = Positions[0].GetUnrealizedProfitLoss(unit, Closes[0][0]);
				pnl1 = Positions[1].GetUnrealizedProfitLoss(unit, Closes[1][0]);
			}
			return pnl0 + pnl1;
		}
		
		public virtual double GetAllPositionsUnrealizedPnL(PerformanceUnit unit) {
			double pnl=0;
			if(IsLiveTrading()) {
				for(int i=0; i<Positions.Length; i++) {
					if(PositionsAccount[i].MarketPosition != MarketPosition.Flat) {
						pnl = pnl + PositionsAccount[i].GetUnrealizedProfitLoss(unit, Closes[i][0]);
					}
				}
			}
			else {
				for(int i=0; i<Positions.Length; i++) {
					if(Positions[i].MarketPosition != MarketPosition.Flat) {
						pnl = pnl + Positions[i].GetUnrealizedProfitLoss(unit, Closes[i][0]);
					}
				}
			}
			return pnl;
		}
		
		public virtual double GetPairGrossPnL(PerformanceUnit unit) {
			double pnl0;
			if(IsLiveTrading()) {
				pnl0 = SystemPerformance.RealTimeTrades.TradesPerformance.GrossLoss
					+ SystemPerformance.RealTimeTrades.TradesPerformance.GrossProfit;
			}
			else {
				pnl0 = SystemPerformance.AllTrades.TradesPerformance.GrossLoss
					+ SystemPerformance.AllTrades.TradesPerformance.GrossProfit;
			}
			return pnl0;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="unit">one tick for stock is $0.01, 
		/// the total $$ amount will be 100*totalSlippage</param>
		/// <returns></returns>
		public virtual double GetPairTotalSlippage(PerformanceUnit unit) {
			double sp0, sp1;
			if(IsLiveTrading()) {
				sp0 = SystemPerformance.RealTimeTrades.TradesPerformance.TotalSlippage;
				//sp1 = PositionsAccount[1].GetUnrealizedProfitLoss(unit, Closes[1][0]);				
			}
			else {
				sp0 = SystemPerformance.AllTrades.TradesPerformance.TotalSlippage;
				//sp1 = SystemPerformance.AllTrades.ByDay
			}
			return sp0;
		}		

		public virtual double GetPairTotalCommission(PerformanceUnit unit) {
			double com0, com1;
			if(IsLiveTrading()) {
				com0 = SystemPerformance.RealTimeTrades.TradesPerformance.TotalCommission;
				//sp1 = PositionsAccount[1].GetUnrealizedProfitLoss(unit, Closes[1][0]);				
			}
			else {
				com0 = SystemPerformance.AllTrades.TradesPerformance.TotalCommission;
				//sp1 = SystemPerformance.AllTrades.ByDay
			}
			return com0;
		}
		
		public virtual double GetTotalCapitalTraded(PerformanceUnit unit) {
			double capTotal=0;
			if(IsLiveTrading()) {
				foreach(Trade td in SystemPerformance.RealTimeTrades) {
					capTotal = capTotal + td.Entry.Price * td.Entry.Quantity;
				}
			}
			else {
				foreach(Trade td in SystemPerformance.AllTrades) {
					capTotal = capTotal + td.Entry.Price * td.Entry.Quantity;
				}
				//sp1 = SystemPerformance.AllTrades.ByDay
			}
			return capTotal;
		}
		
		public virtual TradeCollection GetTradesToday(PerformanceUnit unit) {
			TradeCollection tdtoday = null;
			Dictionary<object, TradeCollection> dt;
			DateTime now = Time[0].Date;
			if(IsLiveTrading()) {
//				foreach(Trade td in SystemPerformance.RealTimeTrades) {
//					capTotal = capTotal + td.Entry.Price * td.Entry.Quantity;
//				}
				dt = SystemPerformance.RealTimeTrades.ByDay;
//				foreach(var item in dt){
//					Print(string.Format("ByDayRT: k={0}, v={1}", item.Key, item.Value));
//				}
			}
			else {
//				foreach(Trade td in SystemPerformance.AllTrades) {
//					capTotal = capTotal + td.Entry.Price * td.Entry.Quantity;
//				}
				dt = SystemPerformance.AllTrades.ByDay;
//				foreach(var item in dt){
//					Print(string.Format("ByDay: k={0}, ktype={1}, v={2}, tdtoday={3}",
//						item.Key, item.Key.GetType(), item.Value, tdtoday));
//				}
			}
			if(dt != null)
				dt.TryGetValue(now, out tdtoday);
			if(tdtoday != null)
				Print(string.Format("ByDay tdtoday: now={0}, tdtoday={1}, count={2}, TradesCount={3}",
				now, tdtoday, tdtoday.Count, tdtoday.TradesCount));
			return tdtoday;
		}
		#endregion
		
		#region Event Handlers
		
		protected override void OnPositionUpdate(Cbi.Position position, double averagePrice, 
			int quantity, Cbi.MarketPosition marketPosition)
		{
			IndicatorProxy.Log2Disk = true;
			int bsx = BarsSinceExitExecution(0, "", 0);
			int bse = BarsSinceEntryExecution(0, "", 0);
			
			if(PrintOut > 1 && !IsInStrategyAnalyzer)
				IndicatorProxy.PrintLog(true, IsLiveTrading(), 
				CurrentBar + ":OnPositionUpdate, CurrentTrade not updated -- "
				+ ";BarsSinceExit, BarsSinceEntry="
				+ bsx + "," + bse
				+ ";IsUnmanaged=" + IsUnmanaged
				+ ";IsLiveTrading=" + IsLiveTrading()
				+ ";GetMarketPosition=" + GetMarketPosition()
				+ ";marketPosition=" + marketPosition
				+ ";HasPosition=" + HasPosition()
				+ ";CurrentTrade.PosQuantity=" + CurrentTrade.PosQuantity
				+ ";CurrentTrade.MktPosition=" + CurrentTrade.MktPosition
				+ ";quantity=" + quantity
				+ ";GetAvgPrice=" + GetAvgPrice()
				+ ";averagePrice=" + averagePrice);
			//Print(position.ToString() + "--MarketPosition=" + position.MarketPosition);
			CurrentTrade.OnCurPositionUpdate(position, averagePrice, quantity, marketPosition);
			if(CurrentTrade.MktPosition != null && CurrentTrade.PosAvgPrice != null
				&& CurrentTrade.PosQuantity != null && CurrentTrade.PosUnrealizedPnL != null)
			if(PrintOut > 1 && !IsInStrategyAnalyzer)
				IndicatorProxy.PrintLog(true, IsLiveTrading(),			
				String.Format("{0}: OnPositionUpdate, CurrentTrade updated -- CurrentTrade.PosAvgPrice: {1}, CurrentTrade.PosQuantit={2}, CurrentTrade.MktPosition={3}, PnL={4}",
					CurrentBar, CurrentTrade.PosAvgPrice, CurrentTrade.PosQuantity, CurrentTrade.MktPosition, CurrentTrade.PosUnrealizedPnL));
		}
		
		#endregion
	}
}
