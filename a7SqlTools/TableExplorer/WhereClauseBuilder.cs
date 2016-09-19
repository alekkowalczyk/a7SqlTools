using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using a7SqlTools.TableExplorer.Enums;
using a7SqlTools.TableExplorer.Filter;
using a7SqlTools.Utils;

namespace a7SqlTools.TableExplorer
{
    public class WhereClauseBuilder
    {
        private readonly FilterExpressionData _filter;
        private readonly a7SingleTableExplorer _table;
        private readonly StringBuilder _whereStringBuilder;
        public Dictionary<string, object> Parameters { get; private set; }
        public string WhereClause { get; private set; }

        /// <summary>
        /// if collName is empty string no collName will be added to field names on where clause, and no relFilters will be replaced, simplified logic.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="table"></param>
        /// <param name="collName"></param>
        public WhereClauseBuilder(FilterExpressionData filter, a7SingleTableExplorer table, string collName = "")
        {
            _filter = filter;
            _table = table;
            _whereStringBuilder = new StringBuilder();
            Parameters = new Dictionary<string, object>();
            filterExprAppendWhereAndParams(_filter, "", collName);
            WhereClause = _whereStringBuilder.ToString();
            if (WhereClause.IsNotEmpty())
                WhereClause = $" WHERE {WhereClause}";
        }

        private bool filterExprAppendWhereAndParams(FilterExpressionData filter, string preffixToAppend, string collName)
        {
            if (filter != null && filter.HasActiveFilter)
            {
                if (filter.Negate)
                    preffixToAppend += " NOT ";
                var fa = filter as FltAtomExprData;//atomic expr
                if (fa != null)
                {
                    string val = fa.Value;
                    appendFilterAtom2WhereSb(fa, preffixToAppend, collName);
                    return true;
                }
                var fg = filter as FltGroupExprData; //group expr
                if (fg != null)
                {
                    if (fg.FilterExpressions.Count == 1)
                        filterExprAppendWhereAndParams(fg.FilterExpressions[0], preffixToAppend, collName);
                    else
                        appendFilterGroup2WhereSb(fg, preffixToAppend, collName);
                    return true;
                }
                var fsg = filter as FltFlatGroupExprData;//simple group expr
                if (fsg != null)
                {
                    appendFilterSimpleGroup2WhereSb(fsg, preffixToAppend, collName);
                    return true;
                }
            }
            return true;
        }


        private void appendFilterSimpleGroup2WhereSb(FltFlatGroupExprData filterGroup, string preffixToAppend, string collName)
        {
            _whereStringBuilder.Append(preffixToAppend);
            if (filterGroup.FieldFilters.Count(kv => kv.Value.HasActiveFilter) > 1)
                _whereStringBuilder.Append(" (");
            if (filterGroup.FieldFilters.Count == 1)
                filterExprAppendWhereAndParams(filterGroup.FieldFilters.Values.First<FltAtomExprData>(), "", collName);
            else
            {
                string sJoinOperator = " AND ";
                if (filterGroup.AndOr == eAndOrJoin.Or)
                    sJoinOperator = " OR ";
                bool isFirstActive = true;
                foreach (var flt in filterGroup.FieldFilters.Values)
                {
                    if (isFirstActive && flt.HasActiveFilter)
                    {
                        if (filterExprAppendWhereAndParams(flt, "", collName))
                            isFirstActive = false;
                    }
                    else
                        filterExprAppendWhereAndParams(flt, sJoinOperator, collName);
                }
            }
            if (filterGroup.FieldFilters.Count(kv => kv.Value.HasActiveFilter) > 1)
                _whereStringBuilder.Append(")");
        }

        private void appendFilterGroup2WhereSb(FltGroupExprData filterGroup, string preffixToAppend, string collName)
        {
            _whereStringBuilder.Append(preffixToAppend);
            if (filterGroup.FilterExpressions.Count(f => f.HasActiveFilter) > 1)
                _whereStringBuilder.Append("(");
            if (filterGroup.FilterExpressions.Count == 1)
                filterExprAppendWhereAndParams(filterGroup.FilterExpressions[0], preffixToAppend, collName);
            else
            {
                string sJoinOperator = " AND ";
                if (filterGroup.AndOr == eAndOrJoin.Or)
                    sJoinOperator = " OR ";
                bool isFirstActive = true;
                for (int i = 0; i < filterGroup.FilterExpressions.Count; i++)
                {
                    var flt = filterGroup.FilterExpressions[i];
                    if (flt == null)
                        continue;
                    if (isFirstActive && flt.HasActiveFilter)
                    {
                        if (filterExprAppendWhereAndParams(flt, "", collName))
                            isFirstActive = false;
                    }
                    else
                        filterExprAppendWhereAndParams(flt, sJoinOperator, collName);
                }
            }
            if (filterGroup.FilterExpressions.Count(f => f.HasActiveFilter) > 1)
                _whereStringBuilder.Append(")");
        }

        private void appendFilterAtom2WhereSb(FltAtomExprData filterAtom, string preffixToAppend, string collName)
        {
            if (filterAtom == null)
                return;
            var fltValue = filterAtom.Value;
            var isCustomValue = false;

            var pField = filterAtom.Field;// StringUtils.FirstCharacterToLower(filterAtom.Field);

            if (filterAtom.Operator == FilterFieldOperator.Between)
            {
                var filterBigger = filterAtom.Clone() as FltAtomExprData;
                filterBigger.Operator = FilterFieldOperator.GreaterEqualThan;
                filterBigger.Value = filterBigger.Value.BeforeString(";");
                var filterSmaller = filterAtom.Clone() as FltAtomExprData;
                filterSmaller.Operator = FilterFieldOperator.LessEqualThan;
                filterSmaller.Value = filterSmaller.Value.AfterString(";");
                var group = Filter.Filter.And(filterBigger, filterSmaller);
                filterExprAppendWhereAndParams(group, preffixToAppend, collName);
                return;
            }

            if (filterAtom.Operator == FilterFieldOperator.In && (filterAtom.Values == null || filterAtom.Values.Count == 0) && filterAtom.Value.IsEmpty())
            {
                filterAtom = Filter.Filter.AlwaysFalse();
            }
            if (filterAtom.Field == "1")
            {
                _whereStringBuilder.Append(preffixToAppend);
                _whereStringBuilder.Append(" (1=" + filterAtom.Value + ")");
                return;
            }

            if (pField == null)
            {
                filterAtom.Field = "";
                filterAtom.Operator = FilterFieldOperator.IsNull;
            }
            _whereStringBuilder.Append(preffixToAppend);
            _whereStringBuilder.Append("(");



            var fld = pField;

            if (filterAtom.Operator != FilterFieldOperator.Contains &&
                filterAtom.Operator != FilterFieldOperator.StartsWith &&
                filterAtom.Operator != FilterFieldOperator.EndsWith)
            {
                if (collName.IsNotEmpty())
                {
                    _whereStringBuilder.Append(collName);
                    _whereStringBuilder.Append(".");
                }
                _whereStringBuilder.Append(fld);
            }
            else
            {
                fld = $"{collName}.{fld}";
            }

            _whereStringBuilder.Append(" ");
            switch (filterAtom.Operator)
            {
                case FilterFieldOperator.Equal:
                    _whereStringBuilder.Append("="); break;
                case FilterFieldOperator.GreaterThan:
                    _whereStringBuilder.Append(">"); break;
                case FilterFieldOperator.GreaterEqualThan:
                    _whereStringBuilder.Append(">="); break;
                case FilterFieldOperator.In:
                    _whereStringBuilder.Append("IN"); break;
                case FilterFieldOperator.LessThan:
                    _whereStringBuilder.Append("<"); break;
                case FilterFieldOperator.LessEqualThan:
                    _whereStringBuilder.Append("<="); break;
                case FilterFieldOperator.Like:
                case FilterFieldOperator.Contains:
                case FilterFieldOperator.StartsWith:
                case FilterFieldOperator.EndsWith:
                    _whereStringBuilder.Append("LIKE"); break;
                case FilterFieldOperator.NotEqual:
                    _whereStringBuilder.Append("<>"); break;
                case FilterFieldOperator.IsNull:
                    _whereStringBuilder.Append("IS NULL"); break;
                case FilterFieldOperator.IsNotNull:
                    _whereStringBuilder.Append("IS NOT NULL"); break;
            }
            _whereStringBuilder.Append(" ");
            if (filterAtom.Operator == FilterFieldOperator.In ||
                filterAtom.Operator == FilterFieldOperator.Like ||
                filterAtom.Operator == FilterFieldOperator.Contains ||
                filterAtom.Operator == FilterFieldOperator.StartsWith ||
                filterAtom.Operator == FilterFieldOperator.EndsWith)
            {
                _whereStringBuilder.Append("(");
            }

            if (filterAtom.Operator == FilterFieldOperator.In)
            {
                var valuesList = new List<string>();
                valuesList = filterAtom.Values ?? filterAtom.Value.SplitCharList(';');

                for (var i = 0; i < valuesList.Count; i++)
                {
                    if (i > 0)
                        _whereStringBuilder.Append(",");
                    _whereStringBuilder.Append("@");
                    _whereStringBuilder.Append(FieldName2ParamName(pField + i, filterAtom));
                    Parameters.Add(FieldName2ParamName(pField + i, filterAtom), valuesList[i]);
                }
            }
            else if (filterAtom.Operator != FilterFieldOperator.IsNull && filterAtom.Operator != FilterFieldOperator.IsNotNull)
            {
                if (!isCustomValue)
                {
                    _whereStringBuilder.Append("@");
                    _whereStringBuilder.Append(FieldName2ParamName(pField, filterAtom));

                    object value = fltValue;

                    var column = _table.AvailableColumns.FirstOrDefault(c => c.Name == fld);
                    var fieldType = column?.Type ?? PropertyType.String;

                    //if date type field, and less than operator, and minutes and hours is 00:00, add one day, so that the day which less than should be, would be included
                    if (fieldType == PropertyType.DateTime)
                    {
                        DateTime tmpDt = DateTime.MinValue;
                        bool parsed = false;

                        if (DateTime.TryParse(fltValue, CultureInfo.CurrentCulture, DateTimeStyles.None, out tmpDt))
                        {
                            parsed = true;
                        }
                        else if (DateTime.TryParse(fltValue, out tmpDt))
                        {
                            parsed = true;
                        }
                        else
                        {
                            string[] formats = {"M/d/yyyy h:mm:ss tt", "M/d/yyyy h:mm tt",
                                                   "MM/dd/yyyy hh:mm:ss", "M/d/yyyy h:mm:ss",
                                                   "M/d/yyyy hh:mm tt", "M/d/yyyy hh tt",
                                                   "M/d/yyyy h:mm", "M/d/yyyy h:mm",
                                                   "MM/dd/yyyy hh:mm", "M/dd/yyyy hh:mm", "dd.MM.yyyy"};
                            if (DateTime.TryParseExact(fltValue, formats,
                              new CultureInfo("en-US"),
                              DateTimeStyles.None,
                              out tmpDt))
                            {
                                parsed = true;
                            }
                        }

                        if (parsed)
                        {
                            if (filterAtom.Operator == FilterFieldOperator.LessEqualThan)
                            {
                                if (tmpDt.Hour == 0 && tmpDt.Minute == 0)
                                {
                                    tmpDt = tmpDt.AddDays(1);
                                }
                            }

                            value = tmpDt.ToString("s");
                        }

                    }

                    if (filterAtom.Operator == FilterFieldOperator.Contains)
                        value = $"%{value}%";
                    else if (filterAtom.Operator == FilterFieldOperator.StartsWith)
                        value = $"{value}%";
                    else if (filterAtom.Operator == FilterFieldOperator.EndsWith)
                        value = $"%{value}";

                    Parameters[FieldName2ParamName(pField, filterAtom)] = value;
                }
                else
                {
                    fltValue = fltValue.Replace("'", "''");
                    _whereStringBuilder.Append(fltValue);
                }
            }

            if (filterAtom.Operator == FilterFieldOperator.In ||
                filterAtom.Operator == FilterFieldOperator.Like ||
                filterAtom.Operator == FilterFieldOperator.Contains ||
                filterAtom.Operator == FilterFieldOperator.StartsWith ||
                filterAtom.Operator == FilterFieldOperator.EndsWith)
            {
                _whereStringBuilder.Append(")");
            }
            _whereStringBuilder.Append(")");
        }

        private string FieldName2ParamName(string fieldName, FltAtomExprData atomic)
        {
            return fieldName.Replace(".", "") + atomic.GetHashCode().ToString();
        }
    }
}
