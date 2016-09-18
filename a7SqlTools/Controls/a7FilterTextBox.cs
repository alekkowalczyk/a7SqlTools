using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using a7SqlTools.TableExplorer.Enums;
using a7SqlTools.Utils;
using ColumnDefinition = a7SqlTools.TableExplorer.ColumnDefinition;

namespace a7SqlTools.Controls
{
    class a7FilterTextBox : ComboBox, INotifyPropertyChanged
    {
        public static DependencyProperty FilterTypeProperty = 
            DependencyProperty.Register("FilterType", typeof(FilterFieldOperator), typeof(a7FilterTextBox)
            , new PropertyMetadata(FilterFieldOperator.Contains, new PropertyChangedCallback(changed)) );
        public FilterFieldOperator FilterType
        {
            get { return (FilterFieldOperator)GetValue(FilterTypeProperty); }
            set { SetValue(FilterTypeProperty, value); }
        }

        public static DependencyProperty AvailableFilterTypesProperty =
            DependencyProperty.Register("AvailableFilterTypes", typeof(List<FilterFieldOperator>), typeof(a7FilterTextBox));
        public List<FilterFieldOperator> AvailableFilterTypes
        {
            get { return (List<FilterFieldOperator>)GetValue(AvailableFilterTypesProperty); }
            set { SetValue(AvailableFilterTypesProperty, value); }
        }

        //public static DependencyProperty FilterTextProperty =
        //        DependencyProperty.Register("FilterText", typeof(string), typeof(a7FilterTextBox));
        //public string FilterText
        //{
        //    get { return (string)GetValue(FilterTextProperty); }
        //    set { SetValue(FilterTextProperty, value); }
        //}


        public a7FilterTextBox(ColumnDefinition fld) : base()
        {
            this.Template = ResourcesManager.Instance.GetControlTemplate("a7FilterTextBoxTemplate"); 
            
            this.IsEditable = true;
            if (fld == null || fld.Type == PropertyType.String)
            {
                FilterType = FilterFieldOperator.Contains;
                AvailableFilterTypes = new List<FilterFieldOperator>()
                        {
                            FilterFieldOperator.Contains,
                            FilterFieldOperator.StartsWith,
                            FilterFieldOperator.EndsWith,
                            FilterFieldOperator.Equal
                        };
            }
            else
            {
                FilterType = FilterFieldOperator.Equal;
                AvailableFilterTypes = new List<FilterFieldOperator>()
                        {
                            FilterFieldOperator.Equal,
                            FilterFieldOperator.GreaterThan,
                            FilterFieldOperator.LessThan
                        };
            }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            e.Handled = true;
            //base.OnSelectionChanged(e);
        }

        public event EventHandler FilterTypeChanged;
        public static readonly RoutedEvent FilterTypeChangedRoutedEvent = EventManager.RegisterRoutedEvent(
   "FilterTypeChangedRouted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(a7FilterTextBox));

        // Provide CLR accessors for the event 
        public event RoutedEventHandler FilterTypeChangedRouted
        {
            add { AddHandler(FilterTypeChangedRoutedEvent, value); }
            remove { RemoveHandler(FilterTypeChangedRoutedEvent, value); }
        }

        static void changed(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            a7FilterTextBox cb = o as a7FilterTextBox;
            cb.IsDropDownOpen = false;
            if (cb.FilterTypeChanged != null)
                cb.FilterTypeChanged(cb, new EventArgs());
            cb.RaiseEvent(new RoutedEventArgs(FilterTypeChangedRoutedEvent));
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
