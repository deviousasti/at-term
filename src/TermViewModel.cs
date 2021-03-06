﻿using AtTerm.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace AtTerm
{
    public class Files
    {
        public const string History = "at-term-history.txt";
        public const string Commands = "at-term-commands.txt";
        public const string Favourites = "at-term-favourites.txt";
    }

    public abstract class TextEvent
    {
        public abstract string Type { get; }

        public string Text { get; set; } = String.Empty;

        public static DateTime Start { get; set; }

        public DateTime Timestamp { get; set; }

        public string RelativeTimestamp => (Timestamp - Start).ToString(@"mm\:ss");

        public string FullTimestamp => Timestamp.ToString(@"dd/MM/yy HH:mm\:ss.ff");

        public override string ToString()
        {
            return Text;
        }
    }

    public class SendEvent : TextEvent
    {
        public override string Type => "Sent";
        public bool Raw { get; set; }
    }

    public class ReceiveEvent : TextEvent
    {
        public override string Type => "Received";
    }

    public class ConnectionEvent : TextEvent
    {
        public override string Type => "Connected";
    }

    public class DisconnectionEvent : TextEvent
    {
        public override string Type => "Disconnected";
    }

    public partial class TermViewModel : ViewModelBase
    {
        #region Properties

        public AtCommand[] AllCommands { get; set; }

        public AtTerm.SerialTty TTy { get; set; }

        private string _commandText;
        public string CommandText
        {
            get => _commandText;
            set
            {
                _commandText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasCommand));
            }
        }

        public void SendLast()
        {
            var lastSent = History.FirstOrDefault();
            CommandText = lastSent;
            Send();
        }

        public string QualifiedCommmandText =>
                CurrentMode == ">" ?
                CommandText
                :
                AtCommand.Qualify(CommandText);


        private AtCommand _selectedCommand;

        public AtCommand SelectedCommand
        {
            get => _selectedCommand;
            set { _selectedCommand = value; OnPropertyChanged(); }
        }

        public void ClearText()
        {
            CommandText = String.Empty;
        }

        private IEnumerable<AtCommand> availableCommands;
        public IEnumerable<AtCommand> AvailableCommands
        {
            get => availableCommands;
            set { availableCommands = value; OnPropertyChanged(); }
        }

        private string _currentMode = "AT+";
        public string CurrentMode
        {
            get => _currentMode;
            set { _currentMode = value; OnPropertyChanged(); }
        }

        public RelayCommand SwitchModeCommand { get; }

        public bool HasCommand => !String.IsNullOrWhiteSpace(CommandText);

        public ObservableCollection<TextEvent> Log { get; set; } =
            new ObservableCollection<TextEvent>();

        private TextEvent _selectedLog;

        public TextEvent SelectedLog
        {
            get => _selectedLog;
            set
            {
                _selectedLog = value;
                OnPropertyChanged();
                if (value is SendEvent sent)
                {
                    CommandText = AtCommand.Unqualify(sent.Text);
                }
            }
        }

        public ICollection<TextEvent> SelectedLogItems { get; set; }
            = new List<TextEvent>();

        public RelayCommand ClearCommand { get; }

        public RelayCommand LogCommand { get; }

        private bool _isLogging;
        public bool IsLogging
        {
            get => _isLogging;
            set { _isLogging = value; OnPropertyChanged(); }
        }

        public string LogFileName { get; set; }

        public string Title => $"AT Term - {TTy}";

        #endregion

        public TermViewModel(ITTy tty)
        {
            TTy = tty as SerialTty;
            tty.Received += text => Write(new ReceiveEvent { Text = text });
            tty.Connected += text => Write(new ConnectionEvent { Text = text });
            tty.Disconnected += text => Write(new DisconnectionEvent { Text = text });
            tty.PropertyChanged += (s, e) => OnPropertyChanged(nameof(Title));

            AddFavouritesCommand = new RelayCommand(_ => AddToFavourites(QualifiedCommmandText));

            SendCommand = new RelayCommand(_ => Send());

            ClearCommand = new RelayCommand(_ => Log.Clear());

            LogCommand = new RelayCommand(_ =>
                LogFileName = $"{DateTime.Now:dd-MM-yyyy--HH-mm-ss}.tsv"                
            );

            SwitchModeCommand = new RelayCommand(mode => CurrentMode = mode as string, _ => true);

            ResetHistoryCycle();

            if (InDesignMode)
            {
                Log = new ObservableCollection<TextEvent>
                {
                    new SendEvent { Text = "AT+CFUN=1,1\nCFUN\nCFUN" },
                    new ReceiveEvent { Text = "OK"},
                    new ReceiveEvent { Text = "+CFUN: 1 "},
                    new SendEvent { Text = "AT+CFUN=1,1" },

                };
            }
        }

        public void OnExit()
        {
            if (!InDesignMode)
            {
                Save();
            }
        }


        #region Autocomplete

        public void AutoCompleteText()
        {
            if (SelectedCommand != null)
                return;

            var match = AllCommands.FirstOrDefault(c => c.Command.IndexOf(CommandText, StringComparison.InvariantCultureIgnoreCase) >= 0);
            if (match != null)
            {
                CommandText = match.Command;
            }
        }

        #endregion

        #region History

        public List<string> History { get; set; } =
            new List<string>();

        private int HistoryIndex { get; set; }

        public void CycleHistory(bool direction)
        {
            if (History.Count > 0)
            {
                HistoryIndex = HistoryIndex + (direction ? 1 : -1);
                var max = History.Count - 1;
                HistoryIndex = HistoryIndex < 0 ? max :
                               HistoryIndex > max ? 0 :
                               HistoryIndex;
                CommandText = History[Math.Abs(HistoryIndex)];
            }
        }

        public void SendFile(string filename)
        {
            try
            {
                var contents = File.ReadAllText(filename);
                Send(contents);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
        }

        public void ResetHistoryCycle() => HistoryIndex = -1;

        public void AddToHistory(string commandText)
        {
            if (String.IsNullOrWhiteSpace(commandText))
                return;

            History.Remove(commandText);
            History.Insert(0, commandText);
            HistoryIndex = History.Count - 1;
        }

        #endregion

        #region Favourites

        public ObservableCollection<string> Favourites { get; set; }
            = new ObservableCollection<string>();

        public RelayCommand AddFavouritesCommand { get; }

        public void AddToFavourites(string text)
        {
            Favourites.Remove(text);
            Favourites.Add(text);
            OnPropertyChanged(nameof(NoFavourites));
        }


        public bool NoFavourites => Favourites.Count == 0;

        public void RemoveFavourite(string str)
        {
            if (str != null)
            {
                Favourites.Remove(str);
                OnPropertyChanged(nameof(NoFavourites));
            }
        }
        #endregion

        #region Send

        public RelayCommand SendCommand { get; }

        public void OnSubmit()
        {
            Send();
            CommandText = String.Empty;
        }

        public void OnContinuation()
        {
            var last = Log.OfType<SendEvent>().LastOrDefault();
            if (!(last?.Raw).GetValueOrDefault(false))
                last = new SendEvent { Raw = true };

            var command = HasCommand ? CommandText : CRLF;
            last.Text += command;

            Log.Remove(last);
            Log.Add(last);

            TTy.Send(command);
            CommandText = String.Empty;
        }

        public void Send()
        {
            AddToHistory(CommandText);
            Send(QualifiedCommmandText);
        }

        public const string CRLF = "\r\n";
        

        public void Send(string command, string display = default)
        {
            if (!TTy.IsConnected)
                return;

            try
            {
                if (command.StartsWith(AtCommand.Base64Prefix))
                {
                    var bytes = Convert.FromBase64String(command.Substring(AtCommand.Base64Prefix.Length));
                    TTy.SendBinary(bytes);
                    Write(new SendEvent { Text = $"<binary data: {bytes.Length} bytes>" });
                }
                else
                {
                    Write(new SendEvent { Text = display ?? command });
                    TTy.Send(command.EndsWith(CRLF) ? command : command + CRLF);
                }
            }
            catch (Exception ex)
            {
                Write(new DisconnectionEvent { Text = ex.Message });
            }
     
        }

        #endregion

        #region Load / Save

        public void LoadFromFiles()
        {
            WriteFromResourceIfNotExist(Files.Commands, "Commands");

            AllCommands =
                File.ReadAllLines(Files.Commands)
                    .Select(AtCommand.Parse)
                    .Where(c => c.IsValid)
                    .ToArray();

            AvailableCommands = AllCommands;

            if (File.Exists(Files.History))
            {
                History = File.ReadAllLines(Files.History).Where(AtCommand.IsValidCommand).ToList();
            }


            if (File.Exists(Files.Favourites))
            {
                Favourites = new ObservableCollection<string>(File.ReadAllLines(Files.Favourites).Where(AtCommand.IsValidCommand));
            }
        }

        void WriteFromResourceIfNotExist(string file, string resource)
        {
            if (!File.Exists(file))
            {
                File.WriteAllText(file, Resources.ResourceManager.GetString(resource));
            }
        }

        public void Save()
        {
            try
            {
                File.WriteAllLines(Files.History, this.History);
                File.WriteAllLines(Files.Favourites, this.Favourites);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region Log 

        public void Write(TextEvent evt)
        {
            if (evt is ConnectionEvent || TextEvent.Start == DateTime.MinValue)
                TextEvent.Start = DateTime.Now;

            if (evt.Timestamp == default)
                evt.Timestamp = DateTime.Now;

            DispatcherInvoke(() =>
            {
                if (Log.Count > 100000)
                {
                    Log.RemoveAt(0);
                }

                Log.Add(evt);
            });

            if (IsLogging && !String.IsNullOrEmpty(LogFileName))
            {
                var lines = evt.Text.Split('\n').Select(s => $"{evt.Timestamp}\t{evt.Type}\t{s.Trim()}");
                File.AppendAllLines(LogFileName, lines);
            }
        }



        #endregion

        #region Start/Stop

        public void Start()
        {
            TTy.Connect();
        } 

        #endregion


    }
}
