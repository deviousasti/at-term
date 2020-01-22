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
        public MainWindow()
        {
            InitializeComponent();
        }

        public TermViewModel ViewModel => this.DataContext as TermViewModel;

        private void OnSubmit(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.OnSubmit();
            }

            if (e.Key == Key.End)
            {
                ViewModel.AutoComplete();
                CommandInput.SetCaretPosition(int.MaxValue);
            }

            if (e.Key == Key.Up)
            {
                ViewModel.CycleHistory();
                CommandInput.SetCaretPosition(int.MaxValue);
            }

        }
        

    }
}
