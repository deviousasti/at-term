using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AtTerm
{
    public class SerialTty : ViewModelBase, ITTy
    {

        public SerialPort Port { get; protected set; }

        public event Action<string> Received;

        public event Action<string> Connected;

        public event Action<string> Disconnected;

        public string[] PortNameSuggestions => SerialPort.GetPortNames();
        
        private string _portName = "COM1";
        public string PortName 
        { 
            get => _portName;
            set
            {
                _portName = value;
                OnPropertyChanged();
            }
        }

        private int _portBaud = 115200;
        public int PortBaud
        {
            get => _portBaud;
            set
            {
                _portBaud = value;
                OnPropertyChanged();
            }
        }

        private string _portSettings = "8N1";
        public string PortSettings
        {
            get => _portSettings;
            set
            {
                _portSettings = value;
                OnPropertyChanged();
            }
        }

        public string PortBaudAsString
        {
            get => PortBaud.ToString();
            set => int.TryParse(value, out _portBaud);
        }

        public bool IsStarted { get; set; }

        public string[] PortBaudSuggestions => 
            "1200,1800,2400,3600,4800,9600,14400,19200,28800,33600,38400,56000,57600,76800,115200,128000,153600,230400,460800,921600,1000000,1500000,2000000".Split(',');

        public bool Connect()
        {
            if (IsConnected)
                return true;

            try
            {
                var instance = new SerialPort(PortName, PortBaud);
                instance.Open();
                Port = instance;
                Port.DataReceived += OnDataReceived;
                Connected?.Invoke($"Connected to {PortName}");
                IsStarted = true;
            }
            catch (Exception ex)
            {
                Disconnected?.Invoke($"Could not connect to {PortName}: {ex.Message}");
            }            

            return IsConnected;
        }

        protected void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Received?.Invoke(Port.ReadExisting()?.Trim());
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
            
        }

        public bool IsConnected
        {
            get
            {
                try
                {
                    return Port != null && Port.IsOpen;
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                }
                return false;
            }
        }


        public bool Disconnect()
        {
            try
            {
                Port?.Close();
                Port?.Dispose();
                Port = null;

                Disconnected?.Invoke($"Disconnected from {PortName}");
                IsStarted = false;
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }

            return false;
            
        }

        public void Send(string text)
        {
            try
            {
                Port?.Write(text);
            }
            catch (Exception)
            {

                throw;
            }
        }

        protected override void RaisePropertyChanged(string propertyName)
        {
            base.RaisePropertyChanged(propertyName);
            if (!IsStarted)
                return;
            Disconnect();
            Connect();
        }
    }
}
