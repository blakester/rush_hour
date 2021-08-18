using System;
using System.Windows.Input;

namespace RushHourView
{
    public class RelayCommand : ICommand
    {
        private Action<object> _methodToExecute;
        private Func<bool> _canExecuteEvaluator;
        
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        //public RelayCommand(Action methodToExecute, Func<bool> canExecuteEvaluator)
        //{
        //    this.methodToExecute = methodToExecute;
        //    this.canExecuteEvaluator = canExecuteEvaluator;
        //}

        public RelayCommand(Action<object> methodToExecute, Func<bool> canExecuteEvaluator)
        {
            _methodToExecute = methodToExecute;
            _canExecuteEvaluator = canExecuteEvaluator;
        }

        public RelayCommand(Action<object> methodToExecute)
            : this(methodToExecute, null)
        {
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecuteEvaluator == null)
            {
                return true;
            }
            return _canExecuteEvaluator.Invoke();            
        }

        public void Execute(object parameter)
        {
            _methodToExecute(parameter);
        }
    }
}
