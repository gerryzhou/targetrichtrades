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
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.AddOns.PriceActions;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The proxy indicator that carry general indicator methods to strategy;
	/// All the methods those only need to impact the ZTrader framework will be implemented here.
	/// </summary>
	public partial class GIndicatorBase : Indicator
	{

		#region Event Handler
        // Declare the event using EventHandler<T>
        public event EventHandler<IndicatorEventArgs> RaiseIndicatorEvent;

        public virtual void FireEvent(IndicatorEventArgs e)
        {
            // Write some code that does something useful here
            // then raise the event. You can also raise an event
            // before you execute a block of code.
            //OnRaiseCustomEvent(new IndicatorEventArgs(this.GetType().Name, " did something: "));
			OnRaiseIndicatorEvent(e);
        }

        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void OnRaiseIndicatorEvent(IndicatorEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<IndicatorEventArgs> handler = RaiseIndicatorEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Format the string to send inside the CustomEventArgs parameter
                e.Message += String.Format(" {0} IndicatorEvent [{1:HH:mm}]", this.GetType().Name, Time[0]); //$" at {DateTime.Now}"; available at C# 6

                // Use the () operator to raise the event.
                handler(this, e);
            }
        }
		#endregion
		
		#region Predefined Indicator Event Name
		[Browsable(false), XmlIgnore]
		public string EventName_Inflection
		{
			get { return "Inflection";}
		}
		#endregion
	}

    // Define a class to hold indicator event info
    public class IndicatorEventArgs : EventArgs
    {
        public IndicatorEventArgs(string name, string msg)
        {
			event_name = name;
            message = msg;
        }
		
		private string event_name;
        private string message;

        public string Message
        {
            get { return message; }
            set { message = value; }
        }
		
        public string EventName
        {
            get { return event_name; }
            set { event_name = value; }
        }
		
		public IndicatorSignal IndSignal
        {
            get; set;
        }
    }
}
