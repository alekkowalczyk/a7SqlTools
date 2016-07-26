using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7SqlTools.DbComparer.Struct
{
    public class a7DbTableUtils
    {
        public static void CopyTable(Table sourcetable, string destTableName, Database destDb)
        {
            var schema = sourcetable.Schema;
            var copiedtable = new Table(destDb, destTableName, schema);
            var server = sourcetable.Parent.Parent;

            createColumns(sourcetable, copiedtable);

            copiedtable.AnsiNullsStatus = sourcetable.AnsiNullsStatus;
            copiedtable.QuotedIdentifierStatus = sourcetable.QuotedIdentifierStatus;
            copiedtable.TextFileGroup = sourcetable.TextFileGroup;
            copiedtable.FileGroup = sourcetable.FileGroup;
            copiedtable.Create();

            foreach (var tr in sourcetable.Triggers)
            {
                var trigger = tr as Trigger;
                var newTr = new Trigger(copiedtable, trigger.Name);
                newTr.AnsiNullsStatus = trigger.AnsiNullsStatus;
                newTr.AssemblyName = trigger.AssemblyName;
                newTr.ClassName = trigger.ClassName;
                //newTr.Delete = trigger.Delete;
                //newTr.DeleteOrder = trigger.DeleteOrder;
                newTr.ExecutionContext = trigger.ExecutionContext;
                newTr.ExecutionContextPrincipal = trigger.ExecutionContextPrincipal;
                newTr.ImplementationType = trigger.ImplementationType;
                //newTr.Insert = trigger.Insert;
                //newTr.InsertOrder = trigger.InsertOrder;
                //newTr.InsteadOf = trigger.InsteadOf;
                newTr.IsEnabled = trigger.IsEnabled;
                //newTr.IsEncrypted = trigger.IsEncrypted;
                newTr.MethodName = trigger.MethodName;
                newTr.Name = trigger.Name;
               // newTr.NotForReplication = trigger.NotForReplication;
                newTr.QuotedIdentifierStatus = trigger.QuotedIdentifierStatus;
                newTr.TextBody = trigger.TextBody;
                newTr.TextHeader = trigger.TextHeader;
                newTr.TextMode = trigger.TextMode;
                //newTr.Update = trigger.Update;
                //newTr.UpdateOrder = trigger.UpdateOrder;
                newTr.UserData = trigger.UserData;
                newTr.Create();
            }
            createChecks(sourcetable, copiedtable);
            createForeignKeys(sourcetable, copiedtable);
            createIndexes(sourcetable, copiedtable);
        }


        public static void CopyColumn(Column source, Table sourceTable, Table copiedtable, Server server, int? colPos = null)
        {
            var column = new Column(copiedtable, source.Name, source.DataType);
            column.Collation = source.Collation;
            column.Nullable = source.Nullable;
            column.Computed = source.Computed;
            column.ComputedText = source.ComputedText;
            column.Default = source.Default;
            if (source.DefaultConstraint != null)
            {
                var tabname = copiedtable.Name;
                var constrname = source.DefaultConstraint.Name;
                column.AddDefaultConstraint(tabname + "_" + constrname);
                column.DefaultConstraint.Text = source.DefaultConstraint.Text;
            }

            column.IsPersisted = source.IsPersisted;
            column.DefaultSchema = source.DefaultSchema;
            column.RowGuidCol = source.RowGuidCol;

            if (server.VersionMajor >= 10)
            {
                column.IsFileStream = source.IsFileStream;
                column.IsSparse = source.IsSparse;
                column.IsColumnSet = source.IsColumnSet;
            }
            if (colPos != null)
            {
                var columnNameBeforeCopiedTable = "";
                for (var i = 0; i < sourceTable.Columns.Count; i++)
                {
                    var sc = sourceTable.Columns[i];
                    if (sc == source)
                    {
                        if (i > 0)
                        {
                            for (var j = i - 1; j >= 0; i--)
                            {
                                var tmpColumnName = sourceTable.Columns[j].Name;
                                var fromDest = copiedtable.Columns[tmpColumnName];
                                if (fromDest != null)
                                {
                                    columnNameBeforeCopiedTable = fromDest.Name;
                                    break;
                                }
                            }
                            if(columnNameBeforeCopiedTable=="")
                                colPos = null;
                            break;
                        }
                        else
                            colPos = 0;
                    }
                }
                if (columnNameBeforeCopiedTable == "")
                    copiedtable.Columns.Add(column, colPos.Value);
                else
                {
                    copiedtable.Columns.Add(column, columnNameBeforeCopiedTable);
                }
            }
            else
            {
                copiedtable.Columns.Add(column);
            }
        }

        private static void createColumns(Table sourcetable, Table copiedtable)
        {
            var server = sourcetable.Parent.Parent;

            foreach (Column source in sourcetable.Columns)
            {
                CopyColumn(source, sourcetable, copiedtable, server);
            }
        }


        private static void createChecks(Table sourcetable, Table copiedtable)
        {
            foreach (Check chkConstr in sourcetable.Checks)
            {
                var name = copiedtable.Name + "_" + chkConstr.Name;
                var check = new Check(copiedtable, name);
                check.IsChecked = chkConstr.IsChecked;
                check.IsEnabled = chkConstr.IsEnabled;
                check.Text = chkConstr.Text;
                check.Create();
            }
        }

        private static void createForeignKeys(Table sourcetable, Table copiedtable)
        {
            foreach (ForeignKey sourcefk in sourcetable.ForeignKeys)
            {
                var name = copiedtable.Name + "_" + sourcefk.Name;
                var foreignkey = new ForeignKey(copiedtable, name);
                foreignkey.DeleteAction = sourcefk.DeleteAction;
                foreignkey.IsChecked = sourcefk.IsChecked;
                foreignkey.IsEnabled = sourcefk.IsEnabled;
                foreignkey.ReferencedTable = sourcefk.ReferencedTable;
                foreignkey.ReferencedTableSchema = sourcefk.ReferencedTableSchema;
                foreignkey.UpdateAction = sourcefk.UpdateAction;

                foreach (ForeignKeyColumn scol in sourcefk.Columns)
                {
                    var refcol = scol.ReferencedColumn;
                    var column =
                     new ForeignKeyColumn(foreignkey, scol.Name, refcol);
                    foreignkey.Columns.Add(column);
                }

                foreignkey.Create();
            }
        }

        private static void createIndexes(Table sourcetable, Table copiedtable)
        {
            foreach (Index srcind in sourcetable.Indexes)
            {
                if (!srcind.IsDisabled && (srcind.IsClustered ||
                    (!srcind.IsClustered && !srcind.IsXmlIndex)))
                {
                    var name = copiedtable.Name + "_" + srcind.Name;
                    var index = new Index(copiedtable, name);

                    index.IndexKeyType = srcind.IndexKeyType;
                    index.IsClustered = srcind.IsClustered;
                    index.IsUnique = srcind.IsUnique;
                    index.CompactLargeObjects = srcind.CompactLargeObjects;
                    index.IgnoreDuplicateKeys = srcind.IgnoreDuplicateKeys;
                    index.IsFullTextKey = srcind.IsFullTextKey;
                    index.PadIndex = srcind.PadIndex;
                    index.FileGroup = srcind.FileGroup;

                    foreach (IndexedColumn srccol in srcind.IndexedColumns)
                    {
                        var column =
                         new IndexedColumn(index, srccol.Name, srccol.Descending);
                        column.IsIncluded = srccol.IsIncluded;
                        index.IndexedColumns.Add(column);
                    }

                    index.Create();
                }
            }
        }
    }
}
