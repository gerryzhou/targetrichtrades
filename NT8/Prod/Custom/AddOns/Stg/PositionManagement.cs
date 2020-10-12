#region Using declarations
using System;
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
			if(!IsInStrategyAnalyzer)
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
			if(!IsInStrategyAnalyzer)
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
		
		public bool HasPairPosition() {
			if(!IsInStrategyAnalyzer)
				IndicatorProxy.TraceMessage(this.Name, 0);
			bool pos = false;
			if(GetMarketPosition(0) != MarketPosition.Flat 
				&& GetMarketPosition(1) != MarketPosition.Flat) {
				pos = true;
			}
			return pos;
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
		
		#endregion
		#region Event Handlers
		
		protected override void OnPositionUpdate(Cbi.Position position, double averagePrice, 
			int quantity, Cbi.MarketPosition marketPosition)
		{
			IndicatorProxy.Log2Disk = true;
			int bsx = BarsSinceExitExecution(0, "", 0);
			int bse = BarsSinceEntryExecution(0, "", 0);
			
			if(!IsInStrategyAnalyzer)
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
			if(!IsInStrategyAnalyzer)
				IndicatorProxy.PrintLog(true, IsLiveTrading(),			
				String.Format("{0}: OnPositionUpdate, CurrentTrade updated -- CurrentTrade.PosAvgPrice: {1}, CurrentTrade.PosQuantit={2}, CurrentTrade.MktPosition={3}, PnL={4}",
					CurrentBar, CurrentTrade.PosAvgPrice, CurrentTrade.PosQuantity, CurrentTrade.MktPosition, CurrentTrade.PosUnrealizedPnL));
		}
		
		#endregion
	}
}
