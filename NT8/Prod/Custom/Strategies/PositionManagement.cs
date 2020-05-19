#region Using declarations
using System;
using NinjaTrader.Cbi;

using NinjaTrader.Data;
using NinjaTrader.NinjaScript.Strategies.ZTraderStg;
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
		
		#region Event Handlers
		
		protected override void OnPositionUpdate(Cbi.Position position, double averagePrice, 
			int quantity, Cbi.MarketPosition marketPosition)
		{
			IndicatorProxy.Log2Disk = true;
			int bsx = BarsSinceExitExecution(0, "", 0);
			int bse = BarsSinceEntryExecution(0, "", 0);
			
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
			IndicatorProxy.PrintLog(true, IsLiveTrading(),			
			String.Format("{0}: OnPositionUpdate, CurrentTrade updated -- CurrentTrade.PosAvgPrice: {1}, CurrentTrade.PosQuantit={2}, CurrentTrade.MktPosition={3}, PnL={4}",
					CurrentBar, CurrentTrade.PosAvgPrice, CurrentTrade.PosQuantity, CurrentTrade.MktPosition, CurrentTrade.PosUnrealizedPnL));
		}
		
		#endregion
	}
}
