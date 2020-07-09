using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ImasArchiveApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var datacontext = new MainWindowModel();
            var window = new MainWindow()
            {
                DataContext = datacontext
            };
            FileModelFactory.parent = datacontext;
            window.Show();
        }
    }
}
