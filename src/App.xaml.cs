using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AtTerm
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public MainWindow UI { get; set; }

        public readonly string[] DefaultArguments = { "COM1", "115200", "8N1" };

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var args =
                e.Args
                .Concat(DefaultArguments.Select(_ => String.Empty))
                .Take(3)
                .Zip(DefaultArguments, (a, b) => String.IsNullOrEmpty(a) ? b : a)
                .ToArray();

            var ViewModel = new AtTerm.TermViewModel(new AtTerm.SerialTty { PortName = args[0], PortBaudAsString = args[1], PortSettings = args[2] });
            ViewModel.LoadFromFiles();
            ViewModel.Start();

            (UI = new MainWindow(ViewModel)).Show();
        }
    }
}
