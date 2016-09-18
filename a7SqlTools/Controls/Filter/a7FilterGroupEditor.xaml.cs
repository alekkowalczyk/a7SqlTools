using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using a7SqlTools.TableExplorer;
using a7SqlTools.TableExplorer.Filter;
using a7SqlTools.Utils;

namespace a7SqlTools.Controls.Filter
{
    /// <summary>
    /// Interaction logic for a7FilterGroupEditor.xaml
    /// </summary>
    public partial class a7FilterGroupEditor : UserControl, INotifyPropertyChanged
    {
        public event EventHandler AddedFirstElement;

        public static readonly DependencyProperty JoinTypeProperty =
            DependencyProperty.Register("JoinType", typeof(eAndOrJoin?), typeof(a7FilterGroupEditor), new PropertyMetadata(default(eAndOrJoin?)));
        public eAndOrJoin? JoinType
        {
            get { return (eAndOrJoin?)GetValue(JoinTypeProperty); }
            set { SetValue(JoinTypeProperty, value); }
        }

        public static readonly DependencyProperty FilterProperty =
    DependencyProperty.Register("Filter", typeof(FilterExpressionData), typeof(a7FilterGroupEditor), new PropertyMetadata(default(FltAtomExprData)));
        public FilterExpressionData Filter
        {
            get { return (FilterExpressionData)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }


        private Orientation _orientation;
        public Orientation Orientation { get { return _orientation; } set { _orientation = value; OnPropertyChanged("Orientation"); OnPropertyChanged("OrientationNegate"); } }
        public Orientation OrientationNegate { get { return (_orientation== Orientation.Vertical)?Orientation.Horizontal : Orientation.Vertical; } }

        private IEnumerable<a7FilterElementDefinition> _elements;
        public IEnumerable<a7FilterElementDefinition> Elements { get { return _elements; } set { _elements = value; OnPropertyChanged("Elements"); } }

        private a7FilterGroupEditor _parent;

        internal a7FilterGroupEditor Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                UnSetAsRoot();

            }
        }

        internal a7SqlTools.Controls.Filter.a7FilterEditor EditorContext { get; private set; }
        internal List<a7FilterGroupEditor> SubGroups { get; private set; }
        internal Label JoinLabelOnParent { get; set; }
        internal a7SqlTools.Controls.Filter.a7FilterElementEditor AtomFilter { get; private set; }

        private bool _isReadOnly;
        public bool IsReadOnly {
            get { return _isReadOnly; }
            set
            {
                _isReadOnly = value;
                if (AtomFilter != null)
                    AtomFilter.IsReadOnly = value;
                else
                {
                    foreach (var gr in this.SubGroups)
                    {
                        gr.IsReadOnly = value;
                    }
                };
                if (value)
                {
                    contextMenu.Visibility = Visibility.Collapsed;
                    grButtonsAndPopup.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    contextMenu.Visibility = Visibility.Visible;
                    grButtonsAndPopup.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        private a7SingleTableExplorer _entity;
        private bool _vertical;
        private Brush _currentBrush;
        private Brush _currentMouseOverBrush;

        public a7FilterGroupEditor(a7SingleTableExplorer entity, bool vertical, bool isReadOnly, a7SqlTools.Controls.Filter.a7FilterEditor editorContext, FilterExpressionData filter)
            : this(entity, vertical, isReadOnly, editorContext)
        {
            SetFilter(filter);
        }

        public a7FilterGroupEditor(a7SingleTableExplorer entity, bool vertical, bool isReadOnly, a7SqlTools.Controls.Filter.a7FilterEditor editorContext)
        {
            InitializeComponent();
            EditorContext = editorContext;
            _entity = entity;
            if (entity != null)
                Elements = a7FilterEditorUtils.GetFilterEditorElements(entity);
            else
                Elements = new List<a7FilterElementDefinition>();
            SubGroups = new List<a7FilterGroupEditor>();
            _vertical = vertical;
            this.VerticalAlignment = VerticalAlignment.Center;
            this.HorizontalAlignment = HorizontalAlignment.Center;
            if (!_vertical)
                Orientation = Orientation.Horizontal;
            else
                Orientation = Orientation.Vertical;
            IsReadOnly = false;
            popupFieldSelect.Opened += (sender, args) =>
            {
                EditorContext?.EntityFieldsPopups.Add(popupFieldSelect);
            };
            this.IsReadOnly = isReadOnly;
        }

        public void SetFilter(FilterExpressionData filter)
        {
            Reset();
            this.Filter = filter;
            if (filter != null)
            {
                if (filter is FltAtomExprData)
                {
                    if (filter.HasActiveFilter)
                    {
                        this.Filter = filter;
                        if(_entity!=null && _entity.AvailableColumns.Any(column => column.Name ==  (filter as FltAtomExprData)?.Field))
                            this.SetAtomFilter(new a7SqlTools.Controls.Filter.a7FilterElementEditor(_entity, filter as FltAtomExprData) { 
                                IsReadOnly = this.IsReadOnly ,
                                EditorContext = this.EditorContext
                            });
                    }
                }
                else if (filter is FltGroupExprData)
                {
                    var fge = filter as FltGroupExprData;
                    this.Filter = new FltGroupExprData(fge.AndOr);
                    this.JoinType = fge.AndOr;
                    foreach (var f in fge.FilterExpressions)
                    {
                        if (f.HasActiveFilter)
                        {
                            var subGroup = new a7FilterGroupEditor(_entity, !_vertical, this.IsReadOnly, EditorContext, f);
                            this.AddGroupSubFilter(subGroup);
                        }
                    }
                }
                else if (filter is FltFlatGroupExprData)
                {
                    var fge = filter as FltFlatGroupExprData;
                    this.Filter = new FltFlatGroupExprData(fge.AndOr);
                    this.JoinType = fge.AndOr;
                    foreach (var f in fge.FieldFilters.Values)
                    {
                        if (f.HasActiveFilter)
                        {
                            var subGroup = new a7FilterGroupEditor(_entity, !_vertical, this.IsReadOnly, EditorContext, f);
                            this.AddGroupSubFilter(subGroup);
                        }
                    }
                }
                this.Negate(filter.Negate);
            }
            if (this.AtomFilter == null && this.SubGroups.Count == 0)
                Reset();
        }

        public void Reset()
        {
            this.Filter = null;
            this.ccAtom.Content = null;
            this.AtomFilter = null;
            this.spSubGroups.Children.Clear();
            this.SubGroups.Clear();
        }

        public void RemoveSubGroup(a7FilterGroupEditor fe)
        {
            var ix = this.spSubGroups.Children.IndexOf(fe);
            if (ix == 0 && this.SubGroups.Count>1) //remove the join label from second element if first is removed
            {
                this.spSubGroups.Children.Remove(this.SubGroups[1].JoinLabelOnParent);
                this.SubGroups[1].JoinLabelOnParent = null;
            }
            this.spSubGroups.Children.Remove(fe);
            this.spSubGroups.Children.Remove(fe.JoinLabelOnParent);
            this.SubGroups.Remove(fe);
            var fgeExpr = this.Filter as FltGroupExprData;
            if (fgeExpr != null)
                fgeExpr.FilterExpressions.Remove(fe.Filter);
            if (this.SubGroups.Count == 1 )
            {
                if (Parent != null)
                {
                    var or = Parent.Orientation;
                    this.SubGroups[0].Orientation = (or == Orientation.Horizontal)
                                                        ? Orientation.Vertical
                                                        : Orientation.Horizontal;
                    this.SubGroups[0]._vertical = !Parent._vertical;
                    this.spSubGroups.Children.Remove(this.SubGroups[0]);
                    Parent.addGroupSubFilter(this.SubGroups[0], true);
                    Parent.RemoveSubGroup(this);
                }
                else
                {
                    if (this.SubGroups[0].SubGroups.Count < 2)
                    {
                        var or = this.SubGroups[0].Orientation;
                        this.SubGroups[0].Orientation = Orientation.Horizontal;
                        this.SubGroups[0]._vertical = false;
                        this.spSubGroups.Children.Remove(this.SubGroups[0]);
                        EditorContext.SetRootGroup(this.SubGroups[0]);
                    }
                    bAdd.Visibility = Visibility.Collapsed;
                    bAnd.Visibility = Visibility.Visible;
                    bOr.Visibility = Visibility.Visible;
                }
            }
            else if (this.SubGroups.Count == 0)
            {
                if (Parent != null)
                {
                    Parent.RemoveSubGroup(this);
                }
                else
                {
                    EditorContext.Reset();
                }
            }
        }

        public void SetAtomFilter(a7SqlTools.Controls.Filter.a7FilterElementEditor fa)
        {
            this.Background = Brushes.White;
            _currentBrush = Brushes.White;
            this.border.BorderBrush = ResourcesManager.Instance.GetBrush("ShadowBorderBrush");
            if (this.Parent != null)
                this.Parent.SetBackground(a7SqlTools.Controls.Filter.a7FilterEditor.BackgroundIndexStep);
            bAdd.Visibility = Visibility.Collapsed;
            bAnd.Visibility = Visibility.Visible;
            bOr.Visibility = Visibility.Visible;
            miChangeToAnd.Visibility = Visibility.Collapsed;
            miChangeToOr.Visibility = Visibility.Collapsed;
            this.Filter = fa.Filter;
            AtomFilter = fa;
            ccAtom.Content = fa;
            fa.FocusControl();
        }

        public void SetAsRoot()
        {
            this.Height = 80;
            this.spAndOr.VerticalAlignment = VerticalAlignment.Top;
        }

        public void UnSetAsRoot()
        {
            this.Height = Double.NaN;
            this.spAndOr.VerticalAlignment = VerticalAlignment.Center;
        }

        public void AddGroupSubFilter(a7FilterGroupEditor fge)
        {
            addGroupSubFilter(fge, false);
        }

        private void addGroupSubFilter(a7FilterGroupEditor fge, bool fromRemove)
        {
            fge.Parent = this;
            FltGroupExprData fgeExpr = null;
            if (this.Filter is FltGroupExprData)
                fgeExpr = this.Filter as FltGroupExprData;
            else
            {
                if (JoinType.HasValue)
                {
                    fgeExpr = new FltGroupExprData(JoinType.Value);
                }
                else
                {
                    fgeExpr = new FltGroupExprData();
                }
                if (Parent != null && Parent.Filter != null)
                {
                    var parentGroup = Parent.Filter as FltGroupExprData;
                    parentGroup.FilterExpressions.Remove(this.Filter);
                    parentGroup.FilterExpressions.Add(fgeExpr);
                }
                this.Filter = fgeExpr;
            }
            fgeExpr.FilterExpressions.Add(fge.Filter);              

            

            if (this.SubGroups.Count > 0 || this.AtomFilter != null )
            {
                bAdd.Visibility = Visibility.Visible;
                bAnd.Visibility = Visibility.Collapsed;
                bOr.Visibility = Visibility.Collapsed;
            }
            else 
            {
                SetBackground(fge.MyBackgroundIndex + a7SqlTools.Controls.Filter.a7FilterEditor.BackgroundIndexStep);
                bAdd.Visibility = Visibility.Collapsed;
                bAnd.Visibility = Visibility.Visible;
                bOr.Visibility = Visibility.Visible;
            }

            if (JoinType.HasValue)
            {
                if (JoinType.Value == eAndOrJoin.And)
                {
                    miChangeToOr.Visibility = System.Windows.Visibility.Visible;
                    miChangeToAnd.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    miChangeToOr.Visibility = System.Windows.Visibility.Collapsed;
                    miChangeToAnd.Visibility = System.Windows.Visibility.Visible;
                }
            }
            else
            {
                miChangeToOr.Visibility = System.Windows.Visibility.Collapsed;
                miChangeToAnd.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (!fromRemove && ( AddedFirstElement != null && this.Parent == null))
            {
                this.AddedFirstElement(this, null);
            }

            if (AtomFilter!=null) //replacing existing atomfilter with groupfilter containing the atomfilter
            {
                var newFge = new a7FilterGroupEditor(_entity, !_vertical, this.IsReadOnly, EditorContext, AtomFilter.Filter);
                fgeExpr.FilterExpressions.Add(AtomFilter.Filter);
                newFge.Parent = this;
                ccAtom.Content = null;
                newFge.SetAtomFilter(AtomFilter);
                AtomFilter = null;
                this.Negate(false);
                spSubGroups.Children.Add(newFge);
                SubGroups.Add(newFge);
            }

            if (SubGroups.Count>0 && JoinType.HasValue)
            {
                var andOrLabel = new Label()
                                     {
                                         Content = (JoinType.Value == eAndOrJoin.And) ? "And": "Or",
                                         VerticalAlignment = VerticalAlignment.Center,
                                         HorizontalAlignment = HorizontalAlignment.Center
                                     };
                spSubGroups.Children.Add(andOrLabel);
                fge.JoinLabelOnParent = andOrLabel;
                fgeExpr.AndOr = JoinType.Value;
            }
            SubGroups.Add(fge);
            spSubGroups.Children.Add(fge);
            fge.AtomFilter?.FocusControl();
        }

        public int MyBackgroundIndex = 0;
        public void SetBackground( int backgroundIndex)
        {
            if (backgroundIndex > MyBackgroundIndex)
            {
                if (backgroundIndex*2.5 > 255)
                    backgroundIndex = a7SqlTools.Controls.Filter.a7FilterEditor.BackgroundIndexStep;
                byte r = (byte) (255 - (backgroundIndex*2.5));
                byte g = (byte) (255 - (backgroundIndex));
                _currentBrush = new SolidColorBrush(new Color() {A = 255, R = r, G = g, B = 255});
                this.Background = _currentBrush;
                _currentMouseOverBrush = new SolidColorBrush(new Color() { A = 255, R = (byte)( r-5), G = (byte)(g-10), B = 250 });
                    this.border.BorderBrush = new SolidColorBrush(new Color() { A = 255, R = r, G = g, B = 240 });
                MyBackgroundIndex = backgroundIndex;
            }
            if (this.Parent != null)
                Parent.SetBackground(backgroundIndex + a7SqlTools.Controls.Filter.a7FilterEditor.BackgroundIndexStep);
        }

        private void bAdd_Click(object sender, RoutedEventArgs e)
        {
            popupFieldSelect.IsOpen = true;
        }

        private void bOr_Click(object sender, RoutedEventArgs e)
        {
            this.JoinType = eAndOrJoin.Or;
            popupFieldSelect.IsOpen = true;
        }

        private void bAnd_Click(object sender, RoutedEventArgs e)
        {
            this.JoinType = eAndOrJoin.And;
            popupFieldSelect.IsOpen = true;
        }

        private void lbFields_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var selectedField = lbFields.SelectedItem as a7FilterElementDefinition;
            if (selectedField != null)
            {
                var fae = new a7SqlTools.Controls.Filter.a7FilterElementEditor(selectedField) {Margin = new Thickness(0, 0, 0, 0), IsReadOnly = this.IsReadOnly};
                fae.EditorContext = this.EditorContext;
                var fge = new a7FilterGroupEditor(_entity, !_vertical, IsReadOnly, EditorContext)
                {Background = Brushes.White};
                fge.SetAtomFilter(fae);
                AddGroupSubFilter(fge);
                popupFieldSelect.IsOpen = false;
            }
        }

        private void miRemove_Click(object sender, RoutedEventArgs e)
        {
            if(Parent!=null)
                Parent.RemoveSubGroup(this);
            else
                EditorContext.Reset(true);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private void miNegate_Click(object sender, RoutedEventArgs e)
        {
            Negate(!this.Filter.Negate);
        }

        private void Negate(bool negate)
        {
            this.Filter.Negate = negate;
            if (this.Filter.Negate)
                this.borderNegate.Visibility = System.Windows.Visibility.Visible;
            else
                this.borderNegate.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void miChangeToOr_Click(object sender, RoutedEventArgs e)
        {
            this.JoinType = eAndOrJoin.Or;
            (this.Filter as FltGroupExprData).AndOr = eAndOrJoin.Or;
            this.miChangeToOr.Visibility = System.Windows.Visibility.Collapsed;
            this.miChangeToAnd.Visibility = System.Windows.Visibility.Visible;
            foreach (var subgr in this.SubGroups)
            {
                if (subgr.JoinLabelOnParent != null)
                    subgr.JoinLabelOnParent.Content = "Or";
            }
        }

        private void miChangeToAnd_Click(object sender, RoutedEventArgs e)
        {
            this.JoinType = eAndOrJoin.And;
            (this.Filter as FltGroupExprData).AndOr = eAndOrJoin.And;
            this.miChangeToOr.Visibility = System.Windows.Visibility.Visible;
            this.miChangeToAnd.Visibility = System.Windows.Visibility.Collapsed;
            foreach (var subgr in this.SubGroups)
            {
                if (subgr.JoinLabelOnParent!=null)
                    subgr.JoinLabelOnParent.Content = "And";
            }
        }
    }
}
