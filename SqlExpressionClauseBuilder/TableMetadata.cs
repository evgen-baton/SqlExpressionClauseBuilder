using System;

namespace SqlExpressionClauseBuilder
{
    public class TableMetadata
    {
        public Type TableType { get; set; }
        public string TableName { get; set; }

        public TableMetadata(Type tableType, string tableName)
        {
            this.TableType = tableType;
            this.TableName = tableName;
        }
    }
}
