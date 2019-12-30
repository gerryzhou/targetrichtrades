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
using System.IO;

using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Repository.Hierarchy;
using log4net.Core;
using log4net.Layout;

using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.Gui.Tools;
#endregion

//This namespace holds Add ons in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.AddOns
{	
	public class GZLogger
	{
	    public static void ConfigureFileAppender( string logFile )
	    {
	        var fileAppender = GetFileAppender( logFile );
	        BasicConfigurator.Configure( fileAppender );
	        ( ( Hierarchy ) LogManager.GetRepository() ).Root.Level = Level.Debug;
	    }

	    private static IAppender GetFileAppender( string logFile )
	    {
	        var layout = new PatternLayout( "%-5level[%date] %logger - %message%newline" );
	        layout.ActivateOptions(); // According to the docs this must be called as soon as any properties have been changed.

	        var appender = new FileAppender
	            {
	                File = logFile,
	                Encoding = Encoding.UTF8,
	                Threshold = Level.Debug,
	                Layout = layout
	            };

	        appender.ActivateOptions();

	        return appender;
	    }
		
		public static string GetConfigFilePath(string config_dir) {
			return config_dir
				+ "log4net.config";
		}
	}
}
