using SqlExpressionClauseBuilder.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SqlExpressionClauseBuilder
{
    public class ClauseBuilder
    {
        public TableMetadata BaseTable { get; set; }
        public Tables Tables { get; set; }
        public List<InnerJoinMetadata> InnerJoinItems { get; set; }

        public bool SelectAllColumns { get; set; }
        public List<string> SelectColumNames { get; set; }


        private ClauseBuilder(TableMetadata tableMetadata)
        {
            this.Tables = new Tables();
            this.InnerJoinItems = new List<InnerJoinMetadata>();
            this.SelectColumNames = new List<string>();

            this.BaseTable = tableMetadata;
            this.Tables.Add(tableMetadata);
        }

        public ClauseBuilder InnerJoin<TInnerDto, TOuterDto, TProperty>(
            Expression<Func<TInnerDto, TProperty>> innerKeySelector,
            Expression<Func<TOuterDto, TProperty>> outerKeySelector)
        {
            var outerTableType = typeof(TOuterDto);
            var outerTableName = outerTableType.Name;

            return this.InnerJoin<TInnerDto, TOuterDto, TProperty>(innerKeySelector, outerKeySelector, outerTableName);
        }

        public ClauseBuilder InnerJoin<TInnerDto, TOuterDto, TProperty>(
            Expression<Func<TInnerDto, TProperty>> innerKeySelector,
            Expression<Func<TOuterDto, TProperty>> outerKeySelector,
            string outerTableNameOverride)
        {
            var innerTableMetadata = this.Tables.TryGetExistingMetadata<TInnerDto>();
            if (innerTableMetadata is null)
            {
                throw new InvalidOperationException("No existing table for specified TDto");
            }

            var outerTableType = typeof(TOuterDto);
            var outerTableName = outerTableNameOverride;

            var outerTableMetadata = new TableMetadata(outerTableType, outerTableName);

            this.Tables.Add(outerTableMetadata);

            var innerJoinMetadata = new InnerJoinMetadata(innerTableMetadata, outerTableMetadata, innerKeySelector.Body, outerKeySelector.Body);
            this.InnerJoinItems.Add(innerJoinMetadata);

            return this;
        }

        public ClauseBuilder SelectAll()
        {
            this.SelectAllColumns = true;

            return this;
        }

        public ClauseBuilder Select<TDto>(params Expression<Func<TDto, object>>[] selectors)
        {
            var tableMetadata = this.Tables.GetExistingMetadata<TDto>();

            var columnNames = new List<string>();

            foreach (var selector in selectors)
            {
                var propertyInfo = selector.Body.GetPropertyInfo();
                var columnName = tableMetadata.GetFullColumnName(propertyInfo.Name);

                columnNames.Add(columnName);
            }

            this.SelectColumNames.AddRange(columnNames);

            return this;
        }

        public ClauseBuilder Select<TDto>(Expression<Func<TDto, object[]>> columnNamesSelector)
        {
            var tableMetadata = this.Tables.GetExistingMetadata<TDto>();

            var columnNames = new List<string>();

            var columnExpressions = (columnNamesSelector.Body as NewArrayExpression).Expressions;

            foreach (var expression in columnExpressions)
            {
                var propertyInfo = expression.GetPropertyInfo();
                var columnName = tableMetadata.GetFullColumnName(propertyInfo.Name);

                columnNames.Add(columnName);
            }

            this.SelectColumNames.AddRange(columnNames);

            return this;
        }

        public string Build()
        {
            var stringBuilder = new StringBuilder();

            if (this.SelectAllColumns == true && this.SelectColumNames.Any())
            {
                throw new InvalidOperationException("Select All columns property specified as true but specific columns also appeared not empty");
            }

            if (this.SelectAllColumns == false && !this.SelectColumNames.Any())
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

            stringBuilder.AppendLine($"FROM {this.BaseTable.Name}");

            if (this.InnerJoinItems.Any())
            {
                foreach (var metadata in this.InnerJoinItems)
                {
                    var outerTableName = metadata.OuterTable.Name;

                    var innerColumnPropertyName = metadata.InnerKeySelector.GetPropertyInfo().Name;
                    var innerColumnName = metadata.InnerTable.GetFullColumnName(innerColumnPropertyName);

                    var outerColumnPropertyName = metadata.OuterKeySelector.GetPropertyInfo().Name;
                    var outerColumnName = metadata.OuterTable.GetFullColumnName(outerColumnPropertyName);

                    stringBuilder.AppendLine($"INNER JOIN {outerTableName} ON {innerColumnName} = {outerColumnName}");
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

    }
}
