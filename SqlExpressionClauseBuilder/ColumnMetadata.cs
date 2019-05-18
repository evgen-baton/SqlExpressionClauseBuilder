using System;

namespace SqlExpressionClauseBuilder
{
    public class ColumnMetadata
    {
        public Type Type { get; set; }
        public string Name { get; set; }

        public ColumnMetadata(Type type, string name)
        {
            this.Type = type;
            this.Name = name;
        }
    }
}
