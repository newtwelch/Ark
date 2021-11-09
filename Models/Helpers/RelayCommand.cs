using System;
using System.Windows.Input;

namespace Ark.Models.Helpers
{
    class RelayCommands : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommands(Action<object> execute, Predicate<object> canExecute = null)
        {
            if (execute is null) throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute is null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter ?? "<N/A>");

    }
}
