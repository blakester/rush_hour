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

        public RelayCommand(Action methodToExecute)
            : this(x => methodToExecute(), null)
        {
        }

        public RelayCommand(Action<object> methodToExecute)
            : this(methodToExecute, null)
        {
        }

        public RelayCommand(Action methodToExecute, Func<bool> canExecuteEvaluator) 
            : this(x => methodToExecute(), canExecuteEvaluator)
        {
        }

        public RelayCommand(Action<object> methodToExecute, Func<bool> canExecuteEvaluator)
        {
            _methodToExecute = methodToExecute;
            _canExecuteEvaluator = canExecuteEvaluator;
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

        // THIS WAS TAKEN FROM A "DelegateCommand" IMPLEMENTATION. IT WAS NOT APART OF THIS ORIGINAL RelayCommand.
        // THIS ONLY COMPILES IF THE CanExecuteChanged ADD/REMOVE ARE COMMENTED OUT.
        //public void RaiseCanExecuteChanged()
        //{
        //    if (CanExecuteChanged != null)
        //    {
        //        CanExecuteChanged(this, EventArgs.Empty);
        //    }
        //}
    }
}
