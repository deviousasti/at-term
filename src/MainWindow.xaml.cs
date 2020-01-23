using System;
using System.Collections.Generic;
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
            this.DataContext = viewModel;
        }


        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ViewModel.OnExit();
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
                    if (listView.Items.Count > 0)
                        listView.ScrollIntoView(listView.Items.GetItemAt(listView.Items.Count - 1));
                }
            }
        }

        private void OnListViewDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewModel.Send();
        }

        private void OnSubmit(object sender, KeyEventArgs e)
        {
            var hasCtrl = e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control);

            if (e.Key == Key.Enter)
            {
                if (hasCtrl)
                    ViewModel.OnContinuation();
                else
                    ViewModel.OnSubmit();
            }

            if (hasCtrl && e.Key == Key.R)
            {
                ViewModel.SendLast();
            }

            if (e.Key == Key.Tab && !hasCtrl)
            {
                ViewModel.AutoCompleteText();
                CommandInput.SetCaretPosition(int.MaxValue);
                e.Handled = true;
            }

            if (e.Key == Key.Up && !CommandInput.IsDropDownOpen && !hasCtrl)
            {
                ViewModel.CycleHistory();                
                CommandInput.SetCaretPosition(int.MaxValue);
            }

            if (e.Key == Key.Escape)
            {
                ViewModel.ClearText();
            }
        }

        private void OnListViewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.Send();
            }

            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (e.Key == Key.C)
                {
                    var listview = e.Source as ListView;
                    var text = String.Join("\r\n", listview.SelectedItems.Cast<Object>());
                    Clipboard.SetText(text);
                }

                if (e.Key == Key.V)
                {
                    ViewModel.Send(Clipboard.GetText());
                }

                //if (e.Key == Key.V)
                //{
                //    Clipboard.GetText().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                //    ViewModel.Send();
                //}
            }

        }
    }
}
