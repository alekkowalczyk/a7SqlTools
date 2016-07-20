using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace a7SqlTools.Utils
{
    public class a7LambdaCommand : ICommand
    {
        Action<object> Execute;
        Func<object, bool> CanExecute;

        public a7LambdaCommand(Action<object> Execute, Func<object, bool> CanExecute = null)
        {
            this.Execute = Execute;
            this.CanExecute = CanExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        #region ICommand Members

        bool ICommand.CanExecute(object parameter)
        {
            if (CanExecute != null)
                return CanExecute(parameter);
            else
                return true;
        }

        void ICommand.Execute(object parameter)
        {
            Execute(parameter);
        }

        #endregion
    }
}
