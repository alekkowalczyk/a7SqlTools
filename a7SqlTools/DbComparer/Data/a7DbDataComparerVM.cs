using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using a7SqlTools.Config.Model;
using a7SqlTools.Connection;
using a7SqlTools.DbComparer.Struct;
using a7SqlTools.Utils;

namespace a7SqlTools.DbComparer.Data
{
    public class a7DbDataComparerVM : ViewModelBase
    {
        public string DbA { get; }
        public string DbB { get; }
        private readonly ConnectionData _connData;
        private readonly ConnectionViewModel _parentVm;
        public string Name => DbA + ":" + DbB;
        private string _log;

        public string Log
        {
            get { return _log; }
            set { _log = value; OnPropertyChanged(); }
        }

        private bool _pleaseClickCompare;

        public bool PleaseClickCompare
        {
            get { return _pleaseClickCompare; }
            set { _pleaseClickCompare = value; OnPropertyChanged(); }
        }

        private a7DbDataComparer _comparer;

        public a7DbDataComparer Comparer
        {
            get { return _comparer; }
            set { _comparer = value; OnPropertyChanged(); }
        }

        public ICommand Remove => new a7LambdaCommand(o =>
        {
            _parentVm.RemoveChild(this);
        });

        public ICommand CompareCommand => new a7LambdaCommand(async o => await Init());

        public a7DbDataComparerVM(string dbA, string dbB, ConnectionData connData, ConnectionViewModel parentVm, bool pleaseClickCompare = false)
        {
            DbA = dbA;
            DbB = dbB;
            _connData = connData;
            _parentVm = parentVm;
            _pleaseClickCompare = pleaseClickCompare;
        }

        public void AddLog(string text)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Log = text + Environment.NewLine + Log;
                //tbLog.AppendText(text + Environment.NewLine);
                //tbLog.Focus();
                //tbLog.CaretIndex = tbLog.Text.Length;
                //tbLog.ScrollToEnd();
            }
            ));

        }

        public async Task Init()
        {
            PleaseClickCompare = false;
            Comparer = null;
            a7DbDataComparer structComparer = null;
            using (var busyVm = new BusyViewModel(AppViewModel.Instance, "Comparing database structures..."))
            {
                await Task.Factory.StartNew(() =>
                {
                    structComparer = new a7DbDataComparer(_connData.Name, DbA, DbB, AddLog);
                });
            }
            if (structComparer != null)
            {
                Comparer = structComparer;
                PleaseClickCompare = false;
                //a7DbStructCompResultWnd wnd = new a7DbStructCompResultWnd(structComparer);
                //wnd.ShowDialog();
            }
            else
            {
                MessageBox.Show("Something is wrong...");
                PleaseClickCompare = true;
            }
        }
    }
}
