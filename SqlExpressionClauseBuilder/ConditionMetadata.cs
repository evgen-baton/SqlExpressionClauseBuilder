using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SqlExpressionClauseBuilder
{
    public class ConditionMetadata
    {
        public ConditionMetadata(Expression expression)
        {
            var binaryExpression = expression as BinaryExpression;

            if (binaryExpression is null)
                throw new ArgumentException("Specifed expression is not BinaryExpression", $"{nameof(expression)}");
        }
    }
}
