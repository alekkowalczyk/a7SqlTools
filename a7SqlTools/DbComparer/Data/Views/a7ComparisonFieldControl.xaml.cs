using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using a7SqlTools.Utils;

namespace a7SqlTools.DbComparer.Data.Views
{
    /// <summary>
    /// Interaction logic for a7ComparisonFieldControl.xaml
    /// </summary>
    public partial class a7ComparisonFieldControl : UserControl
    {
        public a7ComparisonFieldControl(a7ComparisonField field)
        {
            InitializeComponent();
            if (field != null)
            {
                tbA.Text = field.ValueA?.ToString();
                if (field.AisB)
                {
                    tbB.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    tbB.Text = field.ValueB?.ToString();
                    tbB.Background = new SolidColorBrush(Colors.Yellow);
                    tbA.Background = new SolidColorBrush(Colors.Yellow);
                    this.Background = new SolidColorBrush(Colors.LightYellow);

                    if (field.ValueA.IsEmpty())
                    {
                        tbA.Background = new SolidColorBrush(Colors.Lime);
                    }
                    if (field.ValueB.IsEmpty())
                    {
                        tbB.Background = new SolidColorBrush(Colors.Lime);
                    }
                }
                tbA.MouseUp += (s, e) =>
                    {
                        tbA.SelectAll();
                    };
                tbA.MouseDoubleClick += (s, e) =>
                    {
                        var wnd = new a7DetailViewWindow(tbA.Text);
                        wnd.Show();
                        wnd.Activate();
                        wnd.Topmost = true;
                        wnd.Focus();
                    };

                tbB.MouseUp += (s, e) =>
                {
                    tbB.SelectAll();
                };
                tbB.MouseDoubleClick += (s, e) =>
                    {
                        var wnd = new a7DetailViewWindow(tbB.Text);
                        wnd.Show();
                        wnd.Activate();
                        wnd.Topmost = true;
                        wnd.Focus();
                    };
            }
            else
                this.Background = new SolidColorBrush(Colors.Black);
        }
    }
}
