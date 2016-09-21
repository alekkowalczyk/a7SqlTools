using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using a7SqlTools.Utils;

namespace a7SqlTools.DbComparer.Struct
{
    public class a7DbTableFieldDifferences : ViewModelBase
    {
        public string Header { get; private set; }
        public string TableName { get; private set; }
        public string DbAName => Comparer.DbAName;
        public string DbBName => Comparer.DbBName;
        public a7DbStructureComparer Comparer { get; private set; }
        public bool FieldsOnlyInAExist => TableFieldsOnlyInA.Count > 0;
        public bool FieldsOnlyInBExist => TableFieldsOnlyInB.Count > 0;
        public bool FieldsExistenceInSync => TableFieldsOnlyInA.Count == 0 && TableFieldsOnlyInB.Count == 0;
        public bool FieldsInSync => TableFieldsDifferentType.Count == 0 && FieldsExistenceInSync;
        public bool FieldTypeDifferencesExist => TableFieldsDifferentType.Count > 0;
        public ObservableCollection<a7DbTableFieldCopyTo> TableFieldsOnlyInA { get; private set; }
        public ObservableCollection<a7DbTableFieldCopyTo> TableFieldsOnlyInB { get; private set; }
        public ObservableCollection<a7DbTableFieldDifferent> TableFieldsDifferentType { get; private set; }

        public a7DbTableFieldDifferences(string tableNameA, a7DbStructureComparer comparer)
        {
            TableFieldsOnlyInA = new ObservableCollection<a7DbTableFieldCopyTo>();
            TableFieldsOnlyInA.CollectionChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(FieldsOnlyInAExist));
                OnPropertyChanged(nameof(FieldsExistenceInSync));
                OnPropertyChanged(nameof(FieldsInSync));
            };
            TableFieldsOnlyInB = new ObservableCollection<a7DbTableFieldCopyTo>();
            TableFieldsOnlyInB.CollectionChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(FieldsOnlyInBExist));
                OnPropertyChanged(nameof(FieldsExistenceInSync));
                OnPropertyChanged(nameof(FieldsInSync));
            };
            TableFieldsDifferentType = new ObservableCollection<a7DbTableFieldDifferent>();
            TableFieldsDifferentType.CollectionChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(FieldTypeDifferencesExist));
                OnPropertyChanged(nameof(FieldsInSync));
            };
            Header = string.Format("Table '{0}':", tableNameA);
            TableName = tableNameA;
            Comparer = comparer;
        }
    }
}
