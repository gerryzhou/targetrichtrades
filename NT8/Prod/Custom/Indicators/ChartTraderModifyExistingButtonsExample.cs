// Coded by Chelsea Bell. chelsea.bell@ninjatrader.com
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class ChartTraderModifyExistingButtonsExample : Indicator
	{
		private System.Windows.Controls.Button		buyMarketButton;
		private NinjaTrader.Gui.Chart.ChartTab		chartTab;
		private System.Windows.Controls.Grid		chartTraderGrid;
		private NinjaTrader.Gui.Chart.Chart			chartWindow;
		private Brush								originalButtonColor;
		private bool								panelActive;
		private System.Windows.Controls.Button		sellMarketButton;
		private System.Windows.Controls.TabItem		tabItem;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"Enter the description for your new custom Indicator here.";
				Name						= "ChartTraderModifyExistingButtonsExample";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= true;
				DisplayInDataBox			= true;
				DrawOnPricePanel			= true;
				DrawHorizontalGridLines		= true;
				DrawVerticalGridLines		= true;
				PaintPriceMarkers			= true;
				ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive	= true;
			}
			else if (State == State.Historical)
			{
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync((Action)(() =>
					{
						CreateWPFControls();
					}));
				}
			}
			else if (State == State.Terminated)
			{
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync((Action)(() =>
					{
						if (chartWindow != null)
							chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;

						HideWPFControls();
					}));
				}
			}
		}
		protected void BuyMarketButton_Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "Buy Market Button Clicked", TextPosition.BottomLeft, Brushes.Green, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, ChartControl.Properties.ChartBackground, 100);
			// only invalidate the chart so that the text box will appear even if there is no incoming data
			ChartControl.InvalidateVisual();
		}

		protected void CreateWPFControls()
		{
			chartWindow				= System.Windows.Window.GetWindow(ChartControl.Parent) as NinjaTrader.Gui.Chart.Chart;
			chartTraderGrid			= (Window.GetWindow(ChartControl.Parent).FindFirst("ChartWindowChartTraderControl") as ChartTrader).Content as System.Windows.Controls.Grid;
			buyMarketButton			= chartTraderGrid.FindFirst("ChartTraderControlQuickBuyMarketButton") as System.Windows.Controls.Button;
			sellMarketButton		= chartTraderGrid.FindFirst("ChartTraderControlQuickSellMarketButton") as System.Windows.Controls.Button;

			originalButtonColor		= buyMarketButton.Background;

			if (TabSelected())
				ShowWPFControls();

			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
		}

		protected override void OnBarUpdate() { }

		protected void SellMarketButton_Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "Sell Market Button Clicked", TextPosition.BottomLeft, Brushes.Red, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, ChartControl.Properties.ChartBackground, 100);
			// only invalidate the chart so that the text box will appear even if there is no incoming data
			ChartControl.InvalidateVisual();
		}

		protected void ShowWPFControls()
		{
			if (panelActive)
				return;
			
			// when the tab is selected or the indicator is added change the button colors and add an additional event handler
			if (buyMarketButton != null)
			{
				buyMarketButton.Click		+= BuyMarketButton_Click;
				buyMarketButton.Background	= Brushes.Green;
			}

			if (sellMarketButton != null)
			{
				sellMarketButton.Click		+= SellMarketButton_Click;
				sellMarketButton.Background	= Brushes.Red;
			}

			panelActive = true;
		}

		protected void HideWPFControls()
		{
			if (!panelActive)
				return;

			// when the tab is selected or the indicator is removed, reset the colors and remove the added click handler
			if (buyMarketButton != null)
			{
				buyMarketButton.Click		-= BuyMarketButton_Click;
				buyMarketButton.Background	= originalButtonColor;
			}

			if (sellMarketButton != null)
			{
				sellMarketButton.Click		-= SellMarketButton_Click;
				sellMarketButton.Background	= originalButtonColor;
			}

			panelActive = false;
		}

		private bool TabSelected()
		{
			bool tabSelected = false;

			// loop through each tab and see if the tab this indicator is added to is the selected item
			foreach (System.Windows.Controls.TabItem tab in chartWindow.MainTabControl.Items)
				if ((tab.Content as ChartTab).ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem)
					tabSelected = true;

			return tabSelected;
		}

		private void TabChangedHandler(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0)
				return;

			tabItem = e.AddedItems[0] as System.Windows.Controls.TabItem;
			if (tabItem == null)
				return;

			chartTab = tabItem.Content as NinjaTrader.Gui.Chart.ChartTab;
			if (chartTab == null)
				return;

			if (TabSelected())
				ShowWPFControls();
			else
				HideWPFControls();
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ChartTraderModifyExistingButtonsExample[] cacheChartTraderModifyExistingButtonsExample;
		public ChartTraderModifyExistingButtonsExample ChartTraderModifyExistingButtonsExample()
		{
			return ChartTraderModifyExistingButtonsExample(Input);
		}

		public ChartTraderModifyExistingButtonsExample ChartTraderModifyExistingButtonsExample(ISeries<double> input)
		{
			if (cacheChartTraderModifyExistingButtonsExample != null)
				for (int idx = 0; idx < cacheChartTraderModifyExistingButtonsExample.Length; idx++)
					if (cacheChartTraderModifyExistingButtonsExample[idx] != null &&  cacheChartTraderModifyExistingButtonsExample[idx].EqualsInput(input))
						return cacheChartTraderModifyExistingButtonsExample[idx];
			return CacheIndicator<ChartTraderModifyExistingButtonsExample>(new ChartTraderModifyExistingButtonsExample(), input, ref cacheChartTraderModifyExistingButtonsExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ChartTraderModifyExistingButtonsExample ChartTraderModifyExistingButtonsExample()
		{
			return indicator.ChartTraderModifyExistingButtonsExample(Input);
		}

		public Indicators.ChartTraderModifyExistingButtonsExample ChartTraderModifyExistingButtonsExample(ISeries<double> input )
		{
			return indicator.ChartTraderModifyExistingButtonsExample(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ChartTraderModifyExistingButtonsExample ChartTraderModifyExistingButtonsExample()
		{
			return indicator.ChartTraderModifyExistingButtonsExample(Input);
		}

		public Indicators.ChartTraderModifyExistingButtonsExample ChartTraderModifyExistingButtonsExample(ISeries<double> input )
		{
			return indicator.ChartTraderModifyExistingButtonsExample(input);
		}
	}
}

#endregion
