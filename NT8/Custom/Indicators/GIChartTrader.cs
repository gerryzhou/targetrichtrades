// Coded by Chelsea Bell. chelsea.bell@ninjatrader.com
#region Using declarations
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
	public class GIChartTrader : GIndicatorBase
	{
		private RowDefinition			addedRow1, addedRow2;
		private Gui.Chart.ChartTab						chartTab;
		private Gui.Chart.Chart							chartWindow;
		private Grid					chartTraderGrid, chartTraderButtonsGrid, lowerButtonsGrid, upperButtonsGrid;
		//private Button[]				buttonsArray;
		private Button					btnBarsBackLeft, btnBarsBackRight, btnBuyStop, btnSellStop, btnClose;
		private TextBox					tbBarsBack, tbStopBuyPrice, tbStopSellPrice;
		private CheckBox				cbAutoExit;
		private bool					panelActive;
		private TabItem					tabItem;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"Customized Chart Trader";
				Name						= "GIChartTrader";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= true;
				DisplayInDataBox			= false;
				PaintPriceMarkers			= false;
			}
			else if (State == State.Historical)
			{
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync(() =>
					{
						CreateWPFControls();
					});
				}
			}
			else if (State == State.Terminated)
			{
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync(() =>
					{
						DisposeWPFControls();
					});
				}
			}
		}
		
		protected override void OnBarUpdate() { }

		#region Buttons functions
		
		protected void BarsBackLeftClick(object sender, RoutedEventArgs e)
		{
			this.Update();
			Print(string.Format("{0}: BarsBackLeftClick {1}, {2}, {3}", CurrentBar, this.GetType().Name, Time[0], Low[0]));
			
			//Draw.TextFixed(this, "infobox", "Button 1 Clicked", TextPosition.BottomLeft, Brushes.Green, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			// refresh the chart so that the text box will appear on the next render pass even if there is no incoming data
			ForceRefresh();
			IndicatorEventArgs ievt = new IndicatorEventArgs(this.GetType().Name, string.Format(" [{0}] {1}", Times[BarsInProgress][0], CurrentBar));
			ievt.IndSignal = new IndicatorSignal();
			ievt.IndSignal.BarNo = CurrentBar;
			ievt.IndSignal.SignalName = this.GetType().Name;
			ievt.IndSignal.IndicatorSignalType = SignalType.ChartDraw;
			ievt.IndSignal.SignalAction = new SignalAction();
			ievt.IndSignal.SignalAction.SignalActionType = SignalActionType.BarToLeft;
				//FireEvent(ievt);
			OnRaiseIndicatorEvent(ievt);
		}

		protected void BarsBackRightClick(object sender, RoutedEventArgs e)
		{
			this.Update();
			Print(string.Format("{0}: BarsBackRightClick {1}, {2}, {3}", CurrentBar, this.GetType().Name, Time[0], Low[0]));
			
			//Draw.TextFixed(this, "infobox", "Button 2 Clicked", TextPosition.BottomLeft, Brushes.Green, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			// refresh the chart so that the text box will appear on the next render pass even if there is no incoming data
			ForceRefresh();
			IndicatorEventArgs ievt = new IndicatorEventArgs(this.GetType().Name, string.Format(" [{0}] {1}", Times[BarsInProgress][0], CurrentBar));
			ievt.IndSignal = new IndicatorSignal();
			ievt.IndSignal.BarNo = CurrentBar;
			ievt.IndSignal.SignalName = this.GetType().Name;
			ievt.IndSignal.IndicatorSignalType = SignalType.ChartDraw;
			ievt.IndSignal.SignalAction = new SignalAction();
			ievt.IndSignal.SignalAction.SignalActionType = SignalActionType.BarToRight;
				//FireEvent(ievt);
			OnRaiseIndicatorEvent(ievt);
		}

		protected void Button3Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "Button 3 Clicked", TextPosition.BottomLeft, Brushes.DarkOrange, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			ForceRefresh();
		}

		protected void Button4Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "Button 4 Clicked", TextPosition.BottomLeft, Brushes.CadetBlue, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			ForceRefresh();
		}
		
		protected Button CreateButton(string name) {
			Style basicButtonStyle	= Application.Current.FindResource("BasicEntryButton") as Style;
			Button btn	= new Button()
			{
				Content			= name, //string.Format("MyButton{0}", i + 1),
				Height			= 30,
				Margin			= new Thickness(0,0,0,0),
				Padding			= new Thickness(0,0,0,0),
				Style			= basicButtonStyle,
				Background		= Brushes.Gray,
				BorderBrush		= Brushes.DimGray
			};
			return btn;
		}
		
		protected void CreateButtons() {
			btnBarsBackLeft = CreateButton("<<");
			btnBarsBackRight = CreateButton(">>");
			btnBuyStop = CreateButton("BuyStop");
			btnSellStop = CreateButton("SellStop");
			btnClose = CreateButton("Close");
			if (btnBarsBackLeft != null)
				btnBarsBackLeft.Click += BarsBackLeftClick;
			if (btnBarsBackRight != null)
				btnBarsBackRight.Click += BarsBackRightClick;
			if (btnBuyStop != null)
				btnBuyStop.Click += Button3Click;
			if (btnSellStop != null)
				btnSellStop.Click += Button4Click;
		}
		
		#endregion
		
		public void SetStopPrice(string val) {
			if(tbBarsBack == null ) return;
			else tbBarsBack.Text = val;				
		}

		protected void CreateWPFControls()
		{
			chartWindow = Window.GetWindow(ChartControl.Parent) as Gui.Chart.Chart;

			// if not added to a chart, do nothing
			if (chartWindow == null)
				return;

			// this is the entire chart trader area grid
			chartTraderGrid			= (chartWindow.FindFirst("ChartWindowChartTraderControl") as Gui.Chart.ChartTrader).Content as Grid;

			// this grid contains the existing chart trader buttons
			chartTraderButtonsGrid	= chartTraderGrid.Children[0] as Grid;

			// this grid is a grid i'm adding to a new row (at the bottom) in the grid that contains bid and ask prices and order controls (chartTraderButtonsGrid)
			upperButtonsGrid = new Grid();
			Grid.SetColumnSpan(upperButtonsGrid, 3);

			upperButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition());
			upperButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength((double)Application.Current.FindResource("MarginBase")) }); // separator column
			upperButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition());

			// this grid is to organize stuff below
			lowerButtonsGrid = new Grid();
			Grid.SetColumnSpan(lowerButtonsGrid, 4);

			lowerButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition());
			lowerButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength((double)Application.Current.FindResource("MarginBase")) });
			lowerButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition());

			// these rows will be added later, but we can create them now so they only get created once
			addedRow1	= new RowDefinition() { Height = new GridLength(31) };
			addedRow2	= new RowDefinition() { Height = new GridLength(40) };

			tbBarsBack = new TextBox();
			tbBarsBack.Text = "bars back=1";
			
			// this style (provided by NinjaTrader_MichaelM) gives the correct default minwidth (and colors) to make buttons appear like chart trader buttons
			Style basicButtonStyle	= Application.Current.FindResource("BasicEntryButton") as Style;
			CreateButtons();
			// all of the buttons are basically the same so to save lines of code I decided to use a loop over an array
//			buttonsArray = new Button[4];

//			for (int i = 0; i < 4; ++i)
//			{
//				buttonsArray[i]	= new Button()
//				{
//					Content			= string.Format("MyButton{0}", i + 1),
//					Height			= 30,
//					Margin			= new Thickness(0,0,0,0),
//					Padding			= new Thickness(0,0,0,0),
//					Style			= basicButtonStyle
//				};

//				// change colors of the buttons if you'd like. i'm going to change the first and fourth.
//				if (i % 3 != 0)
//				{
//					buttonsArray[i].Background	= Brushes.Gray;
//					buttonsArray[i].BorderBrush	= Brushes.DimGray;
//				}
//			}

//			buttonsArray[0].Click += BarsBackLeftClick;
//			buttonsArray[1].Click += Button2Click;
//			buttonsArray[2].Click += Button3Click;
//			buttonsArray[3].Click += Button4Click;
//			tbBarsBack.KeyDown -= null;
//			tbBarsBack.KeyUp -= null;

//			Grid.SetColumn(buttonsArray[1], 2);
//			// add button3 to the lower grid
//			Grid.SetColumn(buttonsArray[2], 0);
//			// add button4 to the lower grid
//			Grid.SetColumn(buttonsArray[3], 2);
//			for (int i = 0; i <= 1; ++i) //for (int i = 0; i < 2; ++i)
//				upperButtonsGrid.Children.Add(buttonsArray[i]);
			Grid.SetColumn(btnBarsBackLeft, 0);
			Grid.SetColumn(btnBarsBackRight, 2);
			Grid.SetColumn(btnBuyStop, 0);
			Grid.SetColumn(btnSellStop, 2);
			
			upperButtonsGrid.Children.Add(btnBarsBackLeft);
			upperButtonsGrid.Children.Add(btnBarsBackRight);
			
			//	upperButtonsGrid.Children.Add(tbBarsBack);
//			for (int i = 2; i < 4; ++i)
//				lowerButtonsGrid.Children.Add(buttonsArray[i]);
			lowerButtonsGrid.Children.Add(btnBuyStop);
			lowerButtonsGrid.Children.Add(btnSellStop);

			if (TabSelected())
				InsertWPFControls();

			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
		}

		public void DisposeWPFControls()
		{
			if (chartWindow != null)
				chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;

//			if (buttonsArray[0] != null)
//				buttonsArray[0].Click -= BarsBackLeftClick;
//			if (buttonsArray[0] != null)
//				buttonsArray[1].Click -= Button2Click;
//			if (buttonsArray[0] != null)
//				buttonsArray[2].Click -= Button3Click;
//			if (buttonsArray[0] != null)
//				buttonsArray[3].Click -= Button4Click;

			if (btnBarsBackLeft != null)
				btnBarsBackLeft.Click -= BarsBackLeftClick;
			if (btnBarsBackRight != null)
				btnBarsBackRight.Click -= BarsBackRightClick;
			if (btnBuyStop != null)
				btnBuyStop.Click -= Button3Click;
			if (btnSellStop != null)
				btnSellStop.Click -= Button4Click;
			RemoveWPFControls();
		}
		
		public void InsertWPFControls()
		{
			if (panelActive)
				return;

			// add a new row (addedRow1) for upperButtonsGrid to the existing buttons grid
			chartTraderButtonsGrid.RowDefinitions.Add(addedRow1);
			// set our upper grid to that new panel
			Grid.SetRow(upperButtonsGrid, (chartTraderButtonsGrid.RowDefinitions.Count - 1));
			// and add it to the buttons grid
			chartTraderButtonsGrid.Children.Add(upperButtonsGrid);
			
			// add a new row (addedRow2) for our lowerButtonsGrid below the ask and bid prices and pnl display			
			chartTraderGrid.RowDefinitions.Add(addedRow2);
			Grid.SetRow(lowerButtonsGrid, (chartTraderGrid.RowDefinitions.Count - 1));
			chartTraderGrid.Children.Add(lowerButtonsGrid);

			panelActive = true;
		}

		protected void RemoveWPFControls()
		{
			if (!panelActive)
				return;

			if (chartTraderButtonsGrid != null || upperButtonsGrid != null)
			{
				chartTraderButtonsGrid.Children.Remove(upperButtonsGrid);
				chartTraderButtonsGrid.RowDefinitions.Remove(addedRow1);
			}
			
			if (chartTraderButtonsGrid != null || lowerButtonsGrid != null)
			{
				chartTraderGrid.Children.Remove(lowerButtonsGrid);
				chartTraderGrid.RowDefinitions.Remove(addedRow2);
			}

			panelActive = false;
		}

		private bool TabSelected()
		{
			bool tabSelected = false;

			// loop through each tab and see if the tab this indicator is added to is the selected item
			foreach (TabItem tab in chartWindow.MainTabControl.Items)
				if ((tab.Content as Gui.Chart.ChartTab).ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem)
					tabSelected = true;

			return tabSelected;
		}

		private void TabChangedHandler(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0)
				return;

			tabItem = e.AddedItems[0] as TabItem;
			if (tabItem == null)
				return;

			chartTab = tabItem.Content as Gui.Chart.ChartTab;
			if (chartTab == null)
				return;

			if (TabSelected())
				InsertWPFControls();
			else
				RemoveWPFControls();
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GIChartTrader[] cacheGIChartTrader;
		public GIChartTrader GIChartTrader()
		{
			return GIChartTrader(Input);
		}

		public GIChartTrader GIChartTrader(ISeries<double> input)
		{
			if (cacheGIChartTrader != null)
				for (int idx = 0; idx < cacheGIChartTrader.Length; idx++)
					if (cacheGIChartTrader[idx] != null &&  cacheGIChartTrader[idx].EqualsInput(input))
						return cacheGIChartTrader[idx];
			return CacheIndicator<GIChartTrader>(new GIChartTrader(), input, ref cacheGIChartTrader);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GIChartTrader GIChartTrader()
		{
			return indicator.GIChartTrader(Input);
		}

		public Indicators.GIChartTrader GIChartTrader(ISeries<double> input )
		{
			return indicator.GIChartTrader(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GIChartTrader GIChartTrader()
		{
			return indicator.GIChartTrader(Input);
		}

		public Indicators.GIChartTrader GIChartTrader(ISeries<double> input )
		{
			return indicator.GIChartTrader(input);
		}
	}
}

#endregion
