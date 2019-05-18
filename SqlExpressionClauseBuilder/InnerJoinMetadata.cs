using System.Linq.Expressions;

namespace SqlExpressionClauseBuilder
{
    public class InnerJoinMetadata
    {
        public TableMetadata InnerTable { get; }
        public TableMetadata OuterTable { get; }

        public Expression InnerKeySelector { get; }
        public Expression OuterKeySelector { get; }

        public string InnerTableName { get; set; }
        public string OuterTableName { get; set; }

        public string InnerColumnName { get; set; }
        public string OuterColumnName { get; set; }

        public InnerJoinMetadata(string innerTableName, string outerTableName, string innerColumnName, string outerColumnName)
        {
            this.InnerTableName = innerTableName;
            this.OuterTableName = outerTableName;

            this.InnerColumnName = innerColumnName;
            this.OuterColumnName = outerColumnName;
        }

        public InnerJoinMetadata(TableMetadata innerTable, TableMetadata outerTable, Expression innerKeySelector, Expression outerKeySelector)
        {
            this.InnerTable = innerTable;
            this.OuterTable = outerTable;
            this.InnerKeySelector = innerKeySelector;
            this.OuterKeySelector = outerKeySelector;
        }
    }
}
