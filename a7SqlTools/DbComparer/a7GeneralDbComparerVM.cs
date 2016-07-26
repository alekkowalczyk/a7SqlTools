using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using a7SqlTools.Connection;
using a7SqlTools.Utils;

namespace a7SqlTools.DbComparer
{
    public class a7GeneralDbComparerVM : ViewModelBase
    {
        private bool _isInitialized;
        public bool IsInitialized
        {
            get { return _isInitialized; }
            set
            {
                _isInitialized = value;
                OnPropertyChanged();
            }
        }

        public ICommand CompareAgainCommand => new a7LambdaCommand(o =>
        {

        });

        public a7GeneralDbComparerVM(string dbNameA, string dbNameB, a7DbCompareType type, ConnectionViewModel parentVm)
        {
            
        }
    }
}
