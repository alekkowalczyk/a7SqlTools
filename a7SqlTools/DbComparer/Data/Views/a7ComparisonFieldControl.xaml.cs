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
                    columnB.Width = new GridLength(0.0);
                }
                else
                { 
                    tbB.Text = field.ValueB?.ToString();
                    if (field.RowAExists && field.RowBExists)
                    {
                        tbB.Background = new SolidColorBrush(Colors.Yellow);
                        tbA.Background = new SolidColorBrush(Colors.Yellow);
                        this.Background = new SolidColorBrush(Colors.LightYellow);
                    }
                    if (!field.RowAExists)
                    {
                        //tbA.Background = new SolidColorBrush(Colors.Lime);
                        tbA.Visibility = Visibility.Collapsed;
                        columnA.Width = new GridLength(0.0);
                    }
                    if (!field.RowBExists)
                    {
                        //tbB.Background = new SolidColorBrush(Colors.Lime);
                        tbB.Visibility = Visibility.Collapsed;
                        columnB.Width = new GridLength(0.0);
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
