using System.ComponentModel;
using System.Runtime.CompilerServices;
using App = at_term.App;
namespace System.Windows
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static bool InDesignMode => !(Application.Current is App);

    }
}