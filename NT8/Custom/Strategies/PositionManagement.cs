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
			indicatorProxy.TraceMessage(this.Name, 0);
			int pos = 0;
			if(IsLiveTrading()) {
				//if(PositionAccount != null)
				pos = PositionAccount.Quantity;
			}
			else //if(Position != null)
				pos = Position.Quantity;
			return pos;
		}
		
		public double GetTickValue() {
			MasterInstrument maIns = Bars.Instrument.MasterInstrument;
			//Print("TickSize, name, pointvalue=" + maIns.TickSize + "," + maIns.Name + "," + maIns.PointValue);
			return maIns.TickSize*maIns.PointValue;
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
		#endregion
		
		#region Event Handlers
		
		protected override void OnPositionUpdate(Cbi.Position position, double averagePrice, 
			int quantity, Cbi.MarketPosition marketPosition)
		{
			indicatorProxy.Log2Disk = true;
			int bsx = BarsSinceExitExecution(0, "", 0);
			int bse = BarsSinceEntryExecution(0, "", 0);
			
			indicatorProxy.PrintLog(true, IsLiveTrading(), 
				CurrentBar + ":OnPositionUpdate"
				+ ";BarsSinceExit, BarsSinceEntry="
				+ bsx + "," + bse
				+ ";IsUnmanaged=" + IsUnmanaged
				+ ";IsLiveTrading=" + IsLiveTrading()
				+ ";GetMarketPosition=" + GetMarketPosition()
				+ ";marketPosition=" + marketPosition
				+ ";HasPosition=" + HasPosition()
				+ ";quantity=" + quantity
				+ ";GetAvgPrice=" + GetAvgPrice()
				+ ";averagePrice=" + averagePrice);
			//Print(position.ToString() + "--MarketPosition=" + position.MarketPosition);
			CurrentTrade.OnCurPositionUpdate(position, averagePrice, quantity, marketPosition);
			if(CurrentTrade.MktPosition != null && CurrentTrade.PosAvgPrice != null
				&& CurrentTrade.PosQuantity != null && CurrentTrade.PosUnrealizedPnL != null)
			indicatorProxy.PrintLog(true, IsLiveTrading(),			
			String.Format("{0}, AvgPrc: {1}, Quant={2}, MktPos={3}, PnL={4}",
					CurrentBar, CurrentTrade.PosAvgPrice, CurrentTrade.PosQuantity, CurrentTrade.MktPosition, CurrentTrade.PosUnrealizedPnL));
		}
		
		#endregion
	}
}
