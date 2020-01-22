using AtTerm.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace AtTerm
{

    public partial class TermViewModel : ViewModelBase
    {
        private string _commandText;
        public string CommandText
        {
            get => _commandText;
            set { _commandText = value; OnPropertyChanged(); }
        }

        private AtCommand _selectedCommand;

        public AtCommand SelectedCommand
        {
            get => _selectedCommand;
            set { _selectedCommand = value; OnPropertyChanged(); }
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



        public void AutoComplete()
        {
            if (SelectedCommand != null)
                return;

            var match = AllCommands.FirstOrDefault(c => c.Command.IndexOf(CommandText, StringComparison.InvariantCultureIgnoreCase) >= 0);
            if (match != null)
            {
                CommandText = match.Command;
            }
        }

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

        public void OnSubmit()
        {
            Send(String.IsNullOrWhiteSpace(CommandText) ? "AT" : CommandText);
            CommandText = String.Empty;
        }

        public void Send(string command)
        {
            History.Remove(command);
            History.Add(command);
        }

        public AtCommand[] AllCommands { get; set; }

        public TermViewModel()
        {
            if (!InDesignMode)
            {
                LoadCommands();
            }
        }

        public void LoadCommands()
        {

            var command_file = $"{nameof(Resources.Commands)}.txt";
            WriteFromResourceIfNotExist(command_file);

            AllCommands =
                File.ReadAllLines(command_file)
                    .Select(AtCommand.Parse)
                    .Where(c => c.IsValid)
                    .ToArray();

            AvailableCommands = AllCommands;

            const string faves_file = "Favourites.txt";
        }

        void WriteFromResourceIfNotExist(string file)
        {
            if (!File.Exists(file))
            {
                File.WriteAllText(file, Resources.ResourceManager.GetString(Path.GetFileNameWithoutExtension(file)));
            }
        }
    }
}
