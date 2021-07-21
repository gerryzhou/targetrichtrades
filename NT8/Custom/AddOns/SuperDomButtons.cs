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
using NinjaTrader.Gui.Tools;
using System.Windows.Controls;
#endregion

//This namespace holds Add ons in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.AddOns
{
	
	public class SuperDomButtons : NinjaTrader.NinjaScript.AddOnBase
	{
/// <summary>
/// Written by Alan Palmer, Assisted by Jesse N.
/// </summary>

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description = "Addon Adds buttons to the bottom of a super dom";
				Name        = "SuperDomButtons";
			}
			
			else if (State == State.Terminated)
			{
							
			}
		}

		protected override void OnWindowDestroyed(Window w)
		{
//			if(w.IsEnabled) return;
			Cleanup(w);
		
		}

		protected override void OnWindowCreated(Window window)  ///
		{			

			if (window != null)
			{
				window.Dispatcher.InvokeAsync((Action)(() =>
				{
					AddButtonsMethod(window);
				}));
			
			}
		 }
		
		protected void AddButtonsMethod(Window window)
		{
			var be = window as  NinjaTrader.Gui.BasicEntry.BasicEntry;

			if (be == null)
			{
				return;
			}
			
			var	xBasicEntry = be.FindFirst("BasicEntryWindow") as NinjaTrader.Gui.BasicEntry.BasicEntry ;
			
			if(xBasicEntry==null) return;
		
			var xBEControl =xBasicEntry.FindFirst("BasicEntryControl") as NinjaTrader.Gui.BasicEntry.BasicEntryControl ;
		
			if(xBEControl==null) return;
			else Print(string.Format("AddButtonsMethod={0}", xBEControl.Name));
			
//			foreach( ) {
//			}
			
			var sd = window as  NinjaTrader.Gui.SuperDom.SuperDom;

			if (sd == null)	{	return;	}
			
			///So SuperDomWindow is the Parent and we are assiging it to a SuperDom Object.
			var	xSuperDom = sd.FindFirst("SuperDOMWindow") as NinjaTrader.Gui.SuperDom.SuperDom ;
			
			if(xSuperDom==null) return;
			///Creating SuperDomControl Object, which is the space inside the superdom. 
		
			//Assiging our SuperDomControl object the AutomationID inside our xSuperDom SuperDom object.  ///FindFirst finds the automation ID.
			var	xSDControl =xSuperDom.FindFirst("SuperDOMControl") as NinjaTrader.Gui.SuperDom.SuperDomControl ;
			
			if(xSDControl==null) return;
			
			//Creating a new button.		
			Button button1 = new System.Windows.Controls.Button {Content = "Button01"};
			Button button2 = new System.Windows.Controls.Button {Content = "Button02"};
//			button3 = new System.Windows.Controls.Button {Content = "Button3"}
		
			button1.Name= "button1Name";
			
			button2.Name= "button2Name";
			///Naming Buttons/RowDefinitions and Tagging (UI Compoenents allow you to set tag).
			///button1.Name ="Button1"
			////myGridChild.Name="MyRowDefinition1";
			/// myGridChild.Tag= "Store whatever you want, another way of naming/assigning informaiton to a UI Object"
			
			//Print Jesse's method, showing us the Visual Tree.
			//Print(GetVisualTreeInfo(xSDControl.Content as System.Windows.Media.Visual).ToString());
		
			//Creating a Grid object called myGrid.
			Grid myGrid = xSDControl.Content as Grid;  //getting child as a grid
			if(myGrid==null) return;
			
			//Looping through the Children of myGrid.  Because there is a grid then a grid within the grid.
			for(int i=0; i < myGrid.Children.Count; i++)
			 {
	
				if(myGrid.Children[i] is Grid)  //if the child object of myGrid, is a Grid (meaning a grid within the grid).
				{
					if(i ==1) //if its the 1st grid ie grid 0, then add these rows.
					{
						Grid myGridChild = myGrid.Children[i] as Grid;  //Setting grid 0, to a new grid object called myGridChild. 
					
						if(myGridChild==null) return;
			
						//Add new row 
						myGridChild.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
						
						///Naming the grid?
					//	myGridChild.Name="MyRowDefinition1";
						
						//Define rows height
						myGridChild.RowDefinitions[myGridChild.RowDefinitions.Count - 1].Height = myGridChild.RowDefinitions[myGridChild.RowDefinitions.Count - 2].Height;
						//Add button1
						myGridChild.Children.Add(button1);  //Adding a botton to myGridChild.
						//Set row for button to the last row, minus 1, so its the last.	
						Grid.SetRow(button1, myGridChild.RowDefinitions.Count-1);  //Setting the row of the botton we added to myGridChild.
						//Set the column to 1, which is under the price axis.
						Grid.SetColumn(button1,1);
						
						myGridChild.RowDefinitions[myGridChild.RowDefinitions.Count - 1].Name="MyRowDefinition1";
		
						//Add another row
						myGridChild.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
						//Deine hight of row to the same as the one prior to this.
						myGridChild.RowDefinitions[myGridChild.RowDefinitions.Count - 1].Height = myGridChild.RowDefinitions[myGridChild.RowDefinitions.Count - 2].Height;
						//Add button 2
						myGridChild.Children.Add(button2);  //Adding a botton to myGridChild.
						//Set the row of the button tot the last row.
						Grid.SetRow(button2, myGridChild.RowDefinitions.Count-1);  //Setting the row of the botton we added to myGridChild.
						//Set the column to 1, which is under the price axis.
						Grid.SetColumn(button2,1);
							
						myGridChild.RowDefinitions[myGridChild.RowDefinitions.Count - 1].Name="MyRowDefinition2";
						
//						//Add another row
//						myGridChild.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
//						//Deine hight of row to the same as the one prior to this.
//						myGridChild.RowDefinitions[myGridChild.RowDefinitions.Count - 1].Height = myGridChild.RowDefinitions[myGridChild.RowDefinitions.Count - 2].Height;
//						//Add button 3
//						myGridChild.Children.Add(button3);  //Adding a botton to myGridChild.
//						//Set the row of the button tot the last row.
//						Grid.SetRow(button3, myGridChild.RowDefinitions.Count-1);  //Setting the row of the botton we added to myGridChild.
//						//Set the column to 1, which is under the price axis.
//						Grid.SetColumn(button3,1);
				
					}}
					
				}
					 
		}
		
		protected void Cleanup(Window w)
		{
			var be = w as  NinjaTrader.Gui.BasicEntry.BasicEntry;

			if (be == null)
			{
				return;
			}
			
			var	xBasicEntry = be.FindFirst("BasicEntryWindow") as NinjaTrader.Gui.BasicEntry.BasicEntry ;
			
			if(xBasicEntry==null) return;
		
			var xBEControl =xBasicEntry.FindFirst("BasicEntryControl") as NinjaTrader.Gui.BasicEntry.BasicEntryControl ;
		
			if(xBEControl==null) return;
			
			var sd = w as  NinjaTrader.Gui.SuperDom.SuperDom;

			if (sd == null)
			{
				return;
			}
			
			var	xSuperDom = sd.FindFirst("SuperDOMWindow") as NinjaTrader.Gui.SuperDom.SuperDom ;
			
			if(xSuperDom==null) return;
		
			var xSDControl =xSuperDom.FindFirst("SuperDOMControl") as NinjaTrader.Gui.SuperDom.SuperDomControl ;
		
			if(xSDControl==null) return;
			
			Button button1 = new System.Windows.Controls.Button { Content = "Button1" };
			Button button2 = new System.Windows.Controls.Button { Content = "Button2" };
			
			
			
			
			Grid myGrid = xSDControl.Content as Grid;  //getting child as a grid
			
			if(myGrid==null) return;
			
			//Looping through the Children of myGrid.  Because there is a grid then a grid within the grid.
			for(int i=0; i < myGrid.Children.Count; i++)			
			 {
	
				if(myGrid.Children[i] is Grid)  //if the child object of myGrid, is a Grid (meaning a grid within the grid).
				{
					if(i ==1) //if its the 1st gride ie gride 0, then add these rows.
					{
						
						Grid myGridChild = myGrid.Children[i] as Grid;  //Setting grid 0, to a new grid object called myGridChild. 
							
						if(myGridChild==null) return;
						
						for (int j = 0; j < myGridChild.Children.Count; j++)
						{
				      		 var child = myGridChild.Children[j];			
						
//							if (child is Button) { myGridChild.Children.Remove(child as Button); }
						   
						
							 if (child is Button)
							 {
									 
								Button varbutton = child as Button;
			
								 
							   if(varbutton.Name== "button1Name")
								  myGridChild.Children.Remove(varbutton);
							   
						      if(varbutton.Name== "button2Name")
									myGridChild.Children.Remove(varbutton);

			
								if(myGridChild.RowDefinitions[myGridChild.RowDefinitions.Count - 1].Name =="MyRowDefinition1")
									myGridChild.RowDefinitions.RemoveAt(myGridChild.RowDefinitions.Count -1 );
								
								if(myGridChild.RowDefinitions[myGridChild.RowDefinitions.Count - 1].Name =="MyRowDefinition2")
									myGridChild.RowDefinitions.RemoveAt(myGridChild.RowDefinitions.Count -1 );
							
							 }	
						}
					}
				}
			 
			 }
		}
						
///Need to unsuscribe if you have handlers, for example,
	/// 	button1.Click -= Button1_Click;
		///	button2.Click -= Button2_Click;

		
//		protected void Button1_Click(object sender, RoutedEventArgs e)
//		{
//		}

//		protected void Button2_Click(object sender, RoutedEventArgs e)
//		{
//		}
//		protected void Button3_Click(object sender, RoutedEventArgs e)
//		{

//		}
			

	}


	


}
	
	
		