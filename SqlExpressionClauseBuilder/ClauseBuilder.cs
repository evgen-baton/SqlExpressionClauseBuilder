using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SqlExpressionClauseBuilder
{
    public class ClauseBuilder
    {
        public TableMetadata BaseTable { get; set; }
        public List<TableMetadata> Tables { get; set; }
        public List<InnerJoinMetadata> InnerJoinItems { get; set; }

        public bool SelectAllColumns { get; set; }
        public List<string> SelectColumNames { get; set; }


        private ClauseBuilder(TableMetadata tableMetadata)
        {
            this.Tables = new List<TableMetadata>();
            this.InnerJoinItems = new List<InnerJoinMetadata>();
            this.SelectColumNames = new List<string>();

            this.BaseTable = tableMetadata;
            this.Tables.Add(tableMetadata);
        }

        public ClauseBuilder InnerJoin<TInnerDto, TOuterDto, TProperty>(Expression<Func<TInnerDto, TProperty>> innerKeySelector,
            Expression<Func<TOuterDto, TProperty>> outerKeySelector)
        {
            var outerTableType = typeof(TOuterDto);
            var outerTableName = outerTableType.Name;

            return this.InnerJoin<TInnerDto, TOuterDto, TProperty>(innerKeySelector, outerKeySelector, outerTableName);
        }

        public ClauseBuilder InnerJoin<TInnerDto, TOuterDto, TProperty>(Expression<Func<TInnerDto, TProperty>> innerKeySelector,
            Expression<Func<TOuterDto, TProperty>> outerKeySelector, string outerTableNameOverride)
        {
            var innerTableType = typeof(TInnerDto);
            var innerTableName = innerTableType.Name;

            var outerTableType = typeof(TOuterDto);
            var outerTableName = outerTableNameOverride;

            if (!this.Tables.Select(tm => tm.TableType).Contains(innerTableType) &&
                !this.Tables.Select(tm => tm.TableType).Contains(outerTableType))
            {
                throw new InvalidOperationException($"No tables found for both {innerTableType.Name} and {outerTableType.Name}");
            }

            if (!this.Tables.Select(tm => tm.TableType).Contains(innerTableType))
            {
                var tableMetadata = new TableMetadata(innerTableType, innerTableName);
                this.Tables.Add(tableMetadata);
            }

            if (!this.Tables.Select(tm => tm.TableType).Contains(outerTableType))
            {
                var tableMetadata = new TableMetadata(outerTableType, outerTableName);
                this.Tables.Add(tableMetadata);
            }

            var innerTableMetadata = this.Tables.Single(tm => tm.TableType == innerTableType);
            var outerTableMetadata = this.Tables.Single(tm => tm.TableType == outerTableType);

            var innerColumnName = GetPropertyInfo(innerKeySelector).Name;
            var outerColumnName = GetPropertyInfo(outerKeySelector).Name;

            var innerJoinMetadata = new InnerJoinMetadata(innerTableMetadata.TableName, outerTableMetadata.TableName, innerColumnName, outerColumnName);
            this.InnerJoinItems.Add(innerJoinMetadata);

            return this;
        }

        public ClauseBuilder SelectAll()
        {
            this.SelectAllColumns = true;

            return this;
        }

        public ClauseBuilder Select<TDto>(Expression<Func<TDto, object[]>> columnNamesSelector)
        {
            var tableType = typeof(TDto);
            var tableMetadata = this.Tables.Single(tm => tm.TableType == tableType);

            var columnNames = new List<string>();

            var columnExpressions = (columnNamesSelector.Body as NewArrayExpression).Expressions;

            foreach (var expression in columnExpressions)
            {
                var unaryExpression = expression as UnaryExpression;
                if (!(unaryExpression is null))
                {
                    var propertyInfo = GetPropertyInfo(unaryExpression.Operand as MemberExpression);
                    var columnName = propertyInfo.Name;

                    columnNames.Add($"{tableMetadata.TableName}.{columnName}");

                    continue;
                }

                var memberExpression = expression as MemberExpression;
                if (!(memberExpression is null))
                {
                    var propertyInfo = GetPropertyInfo(memberExpression);
                    var columnName = propertyInfo.Name;

                    columnNames.Add($"{tableMetadata.TableName}.{columnName}");

                    continue;
                }
            }

            this.SelectColumNames.AddRange(columnNames);

            return this;
        }

        public string Compile()
        {
            var stringBuilder = new StringBuilder();

            if(this.SelectAllColumns == true && this.SelectColumNames.Any())
            {
                throw new InvalidOperationException("Select All columns property specified as true but specific columns also appeared not empty");
            }

            if(this.SelectAllColumns == false && !this.SelectColumNames.Any())
            {
                throw new InvalidOperationException("No columns specified to select from query");
            }

            if (this.SelectAllColumns == true)
            {
                stringBuilder.AppendLine("SELECT *");
            }
            else
            {
                var selectColumnsString = string.Join(", ", this.SelectColumNames);
                stringBuilder.AppendLine($"SELECT {selectColumnsString}");
            }

            stringBuilder.AppendLine($"FROM {this.BaseTable.TableName}");

            if (this.InnerJoinItems.Any())
            {
                foreach (var metadata in this.InnerJoinItems)
                {
                    stringBuilder.AppendLine($"INNER JOIN {metadata.OuterTableName} ON {metadata.InnerTableName}.{metadata.InnerColumnName} = {metadata.OuterTableName}.{metadata.OuterColumnName}");
                }
            }

            return stringBuilder.ToString();
        }

        public static ClauseBuilder From<TDto>()
        {
            var type = typeof(TDto);
            var typeName = type.Name;

            return From<TDto>(typeName);
        }

        public static ClauseBuilder From<TDto>(string tableNameOverride)
        {
            var type = typeof(TDto);
            var typeName = tableNameOverride;

            var tableMetadata = new TableMetadata(type, typeName);

            return new ClauseBuilder(tableMetadata);
        }

        public PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type type = typeof(TSource);

            System.Linq.Expressions.MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a property that is not from type {1}.",
                    propertyLambda.ToString(),
                    type));

            return propInfo;
        }

        public PropertyInfo GetPropertyInfo(MemberExpression memberExpression)
        {
            PropertyInfo propInfo = memberExpression.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression refers to a field, not a property."));

            return propInfo;
        }
    }
}
