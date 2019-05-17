namespace SqlExpressionClauseBuilder
{
    public class InnerJoinMetadata
    {
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
    }
}
