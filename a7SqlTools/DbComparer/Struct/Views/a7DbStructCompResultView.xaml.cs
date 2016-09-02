using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using a7SqlTools.Annotations;

namespace a7SqlTools.DbComparer.Struct.Views
{
    /// <summary>
    /// Interaction logic for a7DbStructCompResultWnd.xaml
    /// </summary>
    public partial class a7DbStructCompResultView : UserControl, INotifyPropertyChanged
    {
        public a7DbStructureComparer Comparer { get; private set; }

        public a7DbStructCompResultView()
        {
            //Comparer = comparer;
            //this.DataContext = comparer;
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == DataContextProperty && DataContext is a7DbStructureComparer)
            {
                Comparer = (a7DbStructureComparer) DataContext;
                OnPropertyChanged(nameof(Comparer));
            }
        }
    }
}
