using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using a7SqlTools.TableExplorer.Enums;
using a7SqlTools.Utils;

namespace a7SqlTools.Controls
{
    class a7FilterDatePicker : ComboBox, INotifyPropertyChanged
    {
        public static DependencyProperty FilterTypeProperty = 
            DependencyProperty.Register("FilterType", typeof(FilterFieldOperator), typeof(a7FilterDatePicker)
            , new PropertyMetadata(FilterFieldOperator.Equal, new PropertyChangedCallback(changed)) );
        public FilterFieldOperator FilterType
        {
            get { return (FilterFieldOperator)GetValue(FilterTypeProperty); }
            set { SetValue(FilterTypeProperty, value); }
        }

        public static DependencyProperty AvailableFilterTypesProperty =
            DependencyProperty.Register("AvailableFilterTypes", typeof(List<FilterFieldOperator>), typeof(a7FilterDatePicker));
        public List<FilterFieldOperator> AvailableFilterTypes
        {
            get { return (List<FilterFieldOperator>)GetValue(AvailableFilterTypesProperty); }
            set { SetValue(AvailableFilterTypesProperty, value); }
        }

        public static readonly DependencyProperty HasTimeProperty =
            DependencyProperty.Register("HasTime", typeof (bool), typeof (a7FilterDatePicker), new PropertyMetadata(true));

        public bool HasTime
        {
            get { return (bool) GetValue(HasTimeProperty); }
            set { SetValue(HasTimeProperty, value); }
        }


        public bool TwoDatesSelectable
        {
            get { return (bool)GetValue(TwoDatesSelectableProperty); }
            set { SetValue(TwoDatesSelectableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TwoDatesSelectable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TwoDatesSelectableProperty =
            DependencyProperty.Register("TwoDatesSelectable", typeof(bool), typeof(a7FilterDatePicker), new PropertyMetadata(false));


        public a7FilterDatePicker() : base()
        {
            this.Template = ResourcesManager.Instance.GetControlTemplate("a7FilterDatePickerTemplate"); 
            FilterType = FilterFieldOperator.Contains;
            this.IsEditable = true;
            AvailableFilterTypes = new List<FilterFieldOperator>()
            {
                FilterFieldOperator.Between,
                FilterFieldOperator.GreaterThan,
                FilterFieldOperator.LessThan,
                FilterFieldOperator.Equal
            };
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            e.Handled = true;
            //base.OnSelectionChanged(e);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var dp = GetTemplateChild("innerDatePicker") as a7DateTimePicker;
            if (dp != null)
            {
                dp.SelectedDateChanged += dp_SelectedDateChanged;
            }
        }

        void dp_SelectedDateChanged(object sender, EventArgs e)
        {
            if (FilterDateChanged != null)
                FilterDateChanged(this, new EventArgs());
        }


        public event EventHandler FilterTypeChanged;
        public event EventHandler FilterDateChanged;

        static void changed(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            a7FilterDatePicker cb = o as a7FilterDatePicker;
            cb.IsDropDownOpen = false;
            if (cb.FilterType == FilterFieldOperator.Between)
                cb.TwoDatesSelectable = true;
            else
            {
                cb.TwoDatesSelectable = false;
            }
            cb.FilterTypeChanged?.Invoke(cb, new EventArgs());
        }


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
