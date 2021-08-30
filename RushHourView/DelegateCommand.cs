using System;
using System.Windows.Input;

namespace RushHourView
{
    public class DelegateCommand : ICommand
    {
        private Action<object> _methodToExecute;
        private Func<bool> _canExecuteEvaluator;

        public DelegateCommand(Action methodToExecute, Func<bool> canExecuteEvaluator = null)
            : this(x => methodToExecute(), canExecuteEvaluator)
        {
        }

        public DelegateCommand(Action<object> methodToExecute, Func<bool> canExecuteEvaluator = null)
        {
            if (methodToExecute == null)
            {
                throw new ArgumentNullException("methodToExecute cannot be null");
            }
            _methodToExecute = methodToExecute;
            _canExecuteEvaluator = canExecuteEvaluator;
        }

        #region ICommand Members

        public event EventHandler CanExecuteChanged; 
        //{
        //    add { CommandManager.RequerySuggested += value; }
        //    remove { CommandManager.RequerySuggested -= value; }
        //}

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

        #endregion

        // THIS WAS TAKEN FROM A "DelegateCommand" IMPLEMENTATION. IT WAS NOT APART OF THIS ORIGINAL RelayCommand.
        // THIS ONLY COMPILES IF THE CanExecuteChanged ADD/REMOVE ARE COMMENTED OUT.
        // THIS IS BECAUSE "DelegateCommand" REQUIRES CanExecuteChanged TO BE RAISED MANUALLY, AND RELAY COMMAND
        // RELIES ON CommandManager.RequerySuggested TP RAISE CanExecuteChanged AUTOMATICALLY WHEN FOCUES IS LOST
        // OR BUTTONS ARE CLICKED.
        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
}
