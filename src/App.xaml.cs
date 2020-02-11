using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
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

        private Properties.Settings Settings { get; set; }
        private string[] DefaultArguments;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            LoadSettings();


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

        public void LoadSettings()
        {
            string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "at-term.exe.config");
            try
            {
                if (!File.Exists(file))
                {
                    var config = AtTerm.Properties.Resources.ResourceManager.GetString("App");
                    File.WriteAllText(file, config);
                    Settings = new AtTerm.Properties.Settings();
                    DefaultArguments = new[] { "COM1", "115200", "8N1" };
                }
                else
                {
                    Settings = AtTerm.Properties.Settings.Default;
                    Settings.LastPort.ToString();
                    DefaultArguments = new[] { Settings.LastPort, Settings.LastBaud, Settings.LastSetting };
                }
            }
            catch (Exception)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                    //do nothing
                }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var tty = UI.ViewModel.TTy;
            Settings.LastPort = tty.PortName;
            Settings.LastBaud = tty.PortBaudAsString;
            Settings.LastSetting = tty.PortSettings;
            Settings.LastWindowPosition = new Point(UI.Left, UI.Top);
            Settings.LastWindowSize = new Size(UI.Width, UI.Height);
            Settings.Save();

            base.OnExit(e);
        }
    }
}
