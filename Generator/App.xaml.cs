using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Generator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Show the window on start up
        /// </summary>
        /// <param name="e">Start up agruments</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ViewModel model = new Stats();

            Window window = new MainWindow();

            window.DataContext = model;

            window.Show();
        }
        
    }
}
