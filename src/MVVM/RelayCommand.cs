using System.Windows.Input;

namespace System.Windows
{
    public class RelayCommand : ICommand
    {
        protected Func<object, bool> CanExecuteFunc { get; }

        protected Action<object> ExecuteFunc { get; }

        private static bool AlwaysEnabled(object _) => true;

        public RelayCommand(Action<object> executeFunc) : this(executeFunc, AlwaysEnabled)
        {
        }

        public RelayCommand(Action<object> executeFunc, Func<object, bool> canExecuteFunc)
        {
            CanExecuteFunc = canExecuteFunc;
            ExecuteFunc = executeFunc;
        }

        public bool CanExecute(object parameter) => CanExecuteFunc(parameter);

        public void Execute(object parameter) => ExecuteFunc(parameter);

        public event EventHandler CanExecuteChanged;
        protected virtual void OnCanExecuteChanged(object sender, EventArgs e)
        {
            CanExecuteChanged?.Invoke(sender, e);
        }

        public void CanExecuteChange()
        {
            OnCanExecuteChanged(this, EventArgs.Empty);
        }
    }
}