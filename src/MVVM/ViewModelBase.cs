using AtTerm;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Windows
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            RaisePropertyChanged(propertyName);
        }

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static bool InDesignMode => !(Application.Current is App);
        
        protected void DispatcherInvoke(Action action)
        {
            App.Current.Dispatcher.Invoke(action);
        }


    }
}