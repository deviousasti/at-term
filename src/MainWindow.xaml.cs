using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AtTerm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow(TermViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += (s, e) => FocusManager.SetFocusedElement(this, CommandInput);

        }


        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ViewModel.OnExit();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
        }

        public TermViewModel ViewModel => this.DataContext as TermViewModel;

        private void FavouriteItemClicked(object sender, RoutedEventArgs e)
        {
            if (e.Source is Button button)
                ViewModel.Send(button.DataContext as string);
        }

        private void FavouriteItemRemoveClicked(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Button button)
                ViewModel.RemoveFavourite(button.DataContext as string);
        }

        private void OnListViewScroll(object sender, ScrollChangedEventArgs e)
        {
            var delta = e.VerticalOffset + e.ViewportHeight - e.ExtentHeight;
            if (delta == -1)
            {
                if (e.Source is ListView listView)
                {
                    if (listView.Items.Count == 0)
                        return;

                    var lastItem = listView.Items.GetItemAt(listView.Items.Count - 1);
                    listView.ScrollIntoView(lastItem);
                }
            }
        }

        private void OnListViewDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewModel.Send();
        }

        private void OnInputKeyUp(object sender, KeyEventArgs e)
        {
            var hasCtrl = e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control);

            if (e.Key == Key.Enter)
            {
                if (hasCtrl)
                    ViewModel.OnContinuation();
                else
                    ViewModel.OnSubmit();

                ViewModel.ResetHistoryCycle();
            }

            if (hasCtrl && e.Key == Key.R)
            {
                ViewModel.SendLast();
            }

            if (hasCtrl && e.Key == Key.D)
            {
                ViewModel.AddFavouritesCommand.Execute(null);
            }

            if (hasCtrl && e.Key == Key.Z)
            {
                ViewModel.Send(Char.ToString((char)26), "Ctrl + Z");
                return;
            }

            if (hasCtrl && e.Key == Key.E)
            {
                ViewModel.Send(Char.ToString((char)27), "Ctrl + Esc");
                return;
            }

            if (e.Key == Key.Escape)
            {
                ViewModel.ClearText();
            }


            if (!CommandInput.IsDropDownOpen && !hasCtrl && (e.Key == Key.Up || e.Key == Key.Down))
            {
                ViewModel.CycleHistory(e.Key == Key.Up);
                CommandInput.CursorToEnd();
            }

            if (hasCtrl && e.Key == Key.Up)
            {
                var index = LogView.SelectedIndex == -1 ? LogView.Items.Count - 1 : LogView.SelectedIndex;
                LogView.SelectedItem = ViewModel.Log[index];
                var item = LogView.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
                item?.Focus();
            }

        }

        private void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            var hasCtrl = e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control);

            if (e.Key == Key.Tab)
            {
                ViewModel.AutoCompleteText();
                CommandInput.CursorToEnd();
                e.Handled = true;
            }
        }

        private void OnListViewKeyUp(object sender, KeyEventArgs e)
        {
            var listview = e.Source as ListView;

            if (e.Key == Key.Enter)
            {
                ViewModel.Send();
            }

            if (e.Key == Key.Escape)
            {
                CommandInput.Focus();
            }

            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (e.Key == Key.C)
                {
                    var text = String.Join("\r\n", listview.SelectedItems.Cast<Object>());
                    Clipboard.SetText(text);
                }

                if (e.Key == Key.V)
                {
                    ViewModel.Send(Clipboard.GetText());
                }


                if (e.Key == Key.M)
                {
                    var items = listview.SelectedItems.OfType<TextEvent>();
                    var avg =
                    items
                        .Zip(items.Skip(1), (e2, e1) => e2.Timestamp - e1.Timestamp)
                        .Select(ts => Math.Abs(ts.TotalMilliseconds))
                        .Average();

                    MessageBox.Show($"Average difference: {Math.Round(avg, 2)}ms", "Measure", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }


                //if (e.Key == Key.V)
                //{
                //    Clipboard.GetText().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                //    ViewModel.Send();
                //}
            }

        }

        private void OnWindowShortcut(object sender, KeyEventArgs e)
        {
            var hasCtrl = e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control);

            if (hasCtrl && e.Key == Key.S)
            {
                ViewModel.IsLogging = !ViewModel.IsLogging;
                ViewModel.LogCommand.Execute(null);
            }

            if (hasCtrl && e.Key == Key.L)
            {
                ViewModel.ClearCommand.Execute(null);
            }

            if (hasCtrl && (e.Key == Key.OemMinus || e.Key == Key.Subtract))
            {
                LogView.FontSize--;
            }

            if (hasCtrl && (e.Key == Key.OemPlus || e.Key == Key.Add))
            {
                LogView.FontSize++;
            }
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            var formats = e.Data.GetFormats();
            if (!formats.Contains("FileName"))
                return;

            var files = e.Data.GetData("FileName") as string[] ?? new string[] { };
            foreach (var file in files)
            {
                if (ViewModel.CommandText.IndexOf("$length", StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    ViewModel.CommandText = ViewModel.CommandText.Replace("$length", new FileInfo(file).Length.ToString());
                    ViewModel.Send();
                }
                ViewModel.SendFile(file);
            }
        }
    }
}
