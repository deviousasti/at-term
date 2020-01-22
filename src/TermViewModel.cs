using AtTerm.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

    public class SendEvent : TextEvent
    {

    }

    public class ReceiveEvent : TextEvent
    {

    }

    public class ConnectionEvent : TextEvent
    {

    }

    public class DisconnectionEvent : TextEvent
    {

    }

    public partial class TermViewModel : ViewModelBase
    {
        #region Properties

        public AtCommand[] AllCommands { get; set; }
        
        public AtTerm.ITTy TTy { get; set; }

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

        public string QualifiedCommmandText =>
            String.IsNullOrWhiteSpace(CommandText) ? "AT" :
                CommandText.StartsWith(">") ? CommandText.Substring(1) :
                    $"AT+{CommandText}";


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

        #endregion

        public TermViewModel(ITTy tty)
        {
            this.TTy = tty;
            tty.Received += text => Write(new ReceiveEvent { Text = text });
            tty.Connected += text => Write(new ConnectionEvent { Text = text });
            tty.Disconnected += text => Write(new DisconnectionEvent { Text = text });

            AddFavouritesCommand =
                new RelayCommand(() => AddToFavourites(QualifiedCommmandText), () => true);

            SendCommand =
                new RelayCommand(() => Send(), () => true);

            //if (InDesignMode)
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

        public void CycleHistory()
        {
            if (SelectedCommand == null && History.Count > 0)
            {
                HistoryIndex = (HistoryIndex + 1) % History.Count;
                CommandText = History[HistoryIndex];
            }
        }

        public void AddToHistory(string commandText)
        {
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
            TTy?.Send(CommandText);
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
            Write(new SendEvent { Text = command });
            TTy?.Send(command + CRLF);
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
        
        public void Write(TextEvent sendEvent)
        {
            DispatcherInvoke(() => Log.Add(sendEvent));
        }

        

        #endregion

        public void Start()
        {
            TTy.Connect();
        }


    }
}
