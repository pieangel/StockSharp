using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SciTrader.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;
		private Action findSecurities;
		private Func<bool> canSubscribe;

		public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

		public RelayCommand(Action findSecurities)
		{
			this.findSecurities = findSecurities;
		}

		public RelayCommand(Action findSecurities, Func<bool> canSubscribe) : this(findSecurities)
		{
			this.canSubscribe = canSubscribe;
		}

		public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

		internal void RaiseCanExecuteChanged()
		{
			throw new NotImplementedException();
		}

		public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
