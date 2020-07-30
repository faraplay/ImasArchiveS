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
            Dialogs dialogs = new Dialogs();
            var datacontext = new MainWindowModel(dialogs);
            var window = new MainWindow()
            {
                DataContext = datacontext
            };
            FileModelFactory.report = datacontext;
            FileModelFactory.getFileName = dialogs;

            if (e.Args.Length == 1)
            {
                datacontext.Open(e.Args[0]);
            }
            window.Show();
        }
    }
}
