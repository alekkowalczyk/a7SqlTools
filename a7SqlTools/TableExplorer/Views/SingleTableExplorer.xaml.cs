using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace a7SqlTools.TableExplorer.Views
{
    /// <summary>
    /// Interaction logic for DBExplorer.xaml
    /// </summary>
    public partial class SingleTableExplorer : UserControl
    {
        public a7SingleTableExplorer ViewModel
        {
            get
            {
                if (this.DataContext is a7SingleTableExplorer)
                    return this.DataContext as a7SingleTableExplorer;
                else
                    return null;
            }
        }

        public SingleTableExplorer()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
            this.filterEditorButton.UpdateFilterFunction += data =>
            {
                ViewModel.AdvFilter = data;
                ViewModel.Refresh(true);
            };
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("a7SqlTools.Resources.avalonEditSql.xshd"))
            {
                using (var reader = new System.Xml.XmlTextReader(stream))
                {
                    this.sqlEditor.SyntaxHighlighting =
                        ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader,
                        ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                }
            }
        }

        private void bFilter_Click(object sender, RoutedEventArgs e) => 
            ViewModel?.Refresh();

        private void bCommitChanges_Click(object sender, RoutedEventArgs e) =>
            ViewModel.CommitChanges();

        private void BEditSql_OnClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.IsSqlEditMode = true;
            this.sqlEditor.Focus();
        }

        private void BFormatSql_OnClick(object sender, RoutedEventArgs e) =>
            this.ViewModel.FormatSql();
    }
}
