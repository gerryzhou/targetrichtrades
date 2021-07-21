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
using log4net.Repository;
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
	public class GLogger
	{
		public static void Initialize(string logDirectory)
		{
		    //get the current logging repository for this application
		    ILoggerRepository repository = LogManager.GetRepository();
		    //get all of the appenders for the repository
		    IAppender[] appenders = repository.GetAppenders();
		    //only change the file path on the 'FileAppenders'
		    foreach (IAppender appender in (from iAppender in appenders
		                                    where iAppender is FileAppender
		                                    select iAppender))
		    {
		        FileAppender fileAppender = appender as FileAppender;
		        //set the path to your logDirectory using the original file name defined
		        //in configuration
				//Print(String.Format("Path.GetFileName(fileAppender.File)=", Path.GetFileName(fileAppender.File)));
		        fileAppender.File = Path.Combine(logDirectory, Path.GetFileName(fileAppender.File));
		        //make sure to call fileAppender.ActivateOptions() to notify the logging
		        //sub system that the configuration for this appender has changed.
		        fileAppender.ActivateOptions();
		    }
		}
		
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
	}
}
