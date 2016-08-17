using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7SqlTools.Utils
{
    public class BusyViewModel : IDisposable
    {
        private ViewModelBase _vm;
        private bool _isRunning;

        public BusyViewModel(ViewModelBase vm, string busyMessage = "")
        {
            _isRunning = true;
            _vm = vm;
            _vm.IsBusy = true;
            _vm.BusyMessage = busyMessage;
            _vm.BusyProgress = 0;
            progressUp();
        }

        public void UpdateMessage(string str) => _vm.BusyMessage = str;

        private async Task progressUp()
        {
            while (_isRunning)
            {
                await Task.Delay(200);
                if (_vm.BusyProgress < 100)
                    _vm.BusyProgress += 5;
                else
                    _vm.BusyProgress = 0;
            }
        }

        public void Dispose()
        {
            _vm.IsBusy = false;
            _vm.BusyProgress = 0;
            _vm.BusyMessage = "";
            _isRunning = false;
        }
    }
}
