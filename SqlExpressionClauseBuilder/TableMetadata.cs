using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlExpressionClauseBuilder
{
    public class TableMetadata
    {
        public Type Type { get; set; }
        public string Name { get; set; }

        public List<ColumnMetadata> Columns { get; set; }

        public TableMetadata(Type tableType, string tableName)
        {
            this.Type = tableType;
            this.Name = tableName;

            this.Columns = new List<ColumnMetadata>();

            var typeProperties = tableType.GetProperties();

            foreach (var property in typeProperties)
            {
                var columnMetadata = new ColumnMetadata(property.PropertyType, property.Name);
                this.Columns.Add(columnMetadata);
            }
        }

        public string GetFullColumnName(string columnName)
        {
            this.ValidateColumnExists(columnName);

            return $"{this.Name}.{columnName}";
        }

        private void ValidateColumnExists(string columnName)
        {
            var columnExists = this.Columns.Any(c => c.Name == columnName);

            if(columnExists == false)
            {
                throw new InvalidOperationException("No column found for specified column name");
            }
        }
    }
}
