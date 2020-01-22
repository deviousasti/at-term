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

        public TermViewModel ViewModel => this.DataContext as TermViewModel;

        private void OnSubmit(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                    ViewModel.OnContinuation();
                else
                    ViewModel.OnSubmit();
            }

            if (e.Key == Key.Tab)
            {
                ViewModel.AutoCompleteText();
                CommandInput.SetCaretPosition(int.MaxValue);
                e.Handled = true;
            }

            if (e.Key == Key.Up)
            {
                ViewModel.CycleHistory();
                CommandInput.SetCaretPosition(int.MaxValue);
            }

            if (e.Key == Key.Escape)
            {
                ViewModel.ClearText();
            }

        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ViewModel.OnExit();
        }

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
    }
}
