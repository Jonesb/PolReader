using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace PolReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(exceptionHandler);

        }
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Application Exception:" + e.Exception.ToString());
        }

        static void exceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            MessageBox.Show("Application Exception:" + args.ExceptionObject.ToString());
        }


    }
}
