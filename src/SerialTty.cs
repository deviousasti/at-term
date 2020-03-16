using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
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

        private bool dtrEnabled;
        public bool DtrEnabled
        {
            get => dtrEnabled;
            set
            {
                dtrEnabled = value;
                try
                {
                    Port.DtrEnable = value;
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                }

            }
        }

        private bool _rtsEnabled;
        public bool RtsEnabled
        {
            get => _rtsEnabled;
            set
            {
                _rtsEnabled = value;
                try
                {
                    Port.RtsEnable = value;
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                }

            }
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
                var instance = new SerialPort(PortName, PortBaud) { DtrEnable = DtrEnabled, RtsEnable = RtsEnabled };
                instance.Open();
                Port = instance;
                Port.PinChanged += OnPinChanged;
                Port.DataReceived += OnDataReceived;
                Port.ErrorReceived += OnErrorReceived;
                Connected?.Invoke($"Connected to {PortName}");
                OnPropertyChanged(nameof(IsConnected));
                IsStarted = true;
            }
            catch (Exception ex)
            {
                Disconnected?.Invoke($"Could not connect to {PortName}: {ex.Message}");
            }

            return IsConnected;
        }

        private void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            
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
                OnPropertyChanged(nameof(IsConnected));
                OnPropertyChanged(nameof(PortNameSuggestions));
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }

            return false;

        }


        protected void OnPinChanged(object sender, SerialPinChangedEventArgs e)
        {

        }

        protected void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var builder = new StringBuilder();
                while (Port.BytesToRead > 0)
                {
                    builder.Append(Port.ReadExisting());
                    Thread.Yield();
                }
                
                Received?.Invoke(builder.ToString());
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
            set
            {
                if (!value)
                    Disconnect();
                else
                    Connect();
            }
        }


        public void Send(string text)
        {
            try
            {                
                if (!IsConnected && IsStarted)
                    Disconnect();

               Task.Factory.StartNew(() => Port.Write(text));
            }
            catch (Exception ex)
            {
                Disconnected?.Invoke("Failed: " + ex.Message);
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

        public override string ToString()
        {
            return $"{PortName}";
        }

        public void SendBinary(byte[] bytes)
        {
            Port.Write(bytes, 0, bytes.Length);
        }
    }
}
