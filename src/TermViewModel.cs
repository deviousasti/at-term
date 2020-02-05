using AtTerm.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

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

        public string QualifiedCommmandText => AtCommand.Qualify(CommandText);


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


        public string[] AvailableModes { get; set; } =
        {
            "AT+",
            ">"
        };

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

        public RelayCommand ClearCommand { get; }

        public RelayCommand LogCommand { get; }

        private bool _isLogging;

        public bool IsLogging
        {
            get => _isLogging;
            set { _isLogging = value; OnPropertyChanged(); }
        }

        public string LogFileName { get; set; }

        #endregion

        public TermViewModel(ITTy tty)
        {
            this.TTy = tty as SerialTty;
            tty.Received += text => Write(new ReceiveEvent { Text = text });
            tty.Connected += text => Write(new ConnectionEvent { Text = text });
            tty.Disconnected += text => Write(new DisconnectionEvent { Text = text });

            AddFavouritesCommand = new RelayCommand(() => AddToFavourites(QualifiedCommmandText), () => true);

            SendCommand = new RelayCommand(() => Send(), () => true);

            ClearCommand = new RelayCommand(() => Log.Clear(), () => true);

            LogCommand = new RelayCommand(() =>
                LogFileName = $"{DateTime.Now:dd-MM-yyyy--HH-mm-ss}.tsv",
                () => true
            );

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
        public void Send(string command)
        {
            if (!TTy.IsConnected)
                return;
            Write(new SendEvent { Text = command });
            TTy.Send(command.EndsWith(CRLF) ? command : command + CRLF);
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
                var lines = evt.Text.Split('\n').Select(s => $"{DateTime.Now}\t{evt.Type}\t{s.Trim()}");
                File.AppendAllLines(LogFileName, lines);
            }
        }



        #endregion

        public void Start()
        {
            TTy.Connect();
        }


    }
}
