using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlExpressionClauseBuilder
{
    public class Tables
    {
        public List<TableMetadata> TablesMetadata { get; }

        public Tables()
        {
            this.TablesMetadata = new List<TableMetadata>();
        }

        public void Add(TableMetadata tableMetadata)
        {
            this.TablesMetadata.Add(tableMetadata);
        }


        public TableMetadata TryGetExistingMetadata<TDto>()
        {
            var type = typeof(TDto);

            return this.TryGetExistingMetadata(type);
        }

        public TableMetadata TryGetExistingMetadata(Type type)
        {
            var metadata = this.TablesMetadata.SingleOrDefault(tm => tm.Type == type);

            return metadata;
        }

        public TableMetadata GetExistingMetadata<TDto>()
        {
            var type = typeof(TDto);

            return this.GetExistingMetadata(type);
        }

        public TableMetadata GetExistingMetadata(Type type)
        {
            var metadata = this.TablesMetadata.SingleOrDefault(tm => tm.Type == type);

            if (metadata is null)
            {
                throw new InvalidOperationException("No TableMetadata found for specified type");
            }

            return metadata;
        }
    }
}
