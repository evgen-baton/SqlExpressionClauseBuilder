﻿using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SqlExpressionClauseBuilder.Extensions
{
    public static class ExpressionExtensions
    {
        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(this Expression<Func<TSource, TProperty>> propertyLambda)
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

        public static PropertyInfo GetPropertyInfo(this Expression expression)
        {
            var unaryExpression = expression as UnaryExpression;
            if (!(unaryExpression is null))
            {
                return GetPropertyInfo(unaryExpression.Operand as MemberExpression);
            }

            var memberExpression = expression as MemberExpression;
            if (!(memberExpression is null))
            {
                return GetPropertyInfo(memberExpression);
            }

            throw new InvalidOperationException("Could not get PropertyInfo from specified expression");
        }

        private static PropertyInfo GetPropertyInfo(this MemberExpression memberExpression)
        {
            PropertyInfo propInfo = memberExpression.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException("Expression is not PropertyInfo");
            }

            return propInfo;
        }

        public static FieldInfo GetFieldInfo(this Expression expression)
        {
            var unaryExpression = expression as UnaryExpression;
            if (!(unaryExpression is null))
            {
                return GetFieldInfo(unaryExpression.Operand as MemberExpression);
            }

            var memberExpression = expression as MemberExpression;
            if (!(memberExpression is null))
            {
                return GetFieldInfo(memberExpression);
            }

            throw new InvalidOperationException("Could not get FieldInfo from specified expression");
        }

        private static FieldInfo GetFieldInfo(this MemberExpression memberExpression)
        {
            var fieldInfo = memberExpression.Member as FieldInfo;
            if (fieldInfo == null)
            {
                throw new ArgumentException("Expression is not FieldInfo");
            }

            return fieldInfo;
        }

        public static MemberInfo GetMemberInfo(this MemberExpression memberExpression)
        {
            var memberInfo = memberExpression.Member as MemberInfo;
            if (memberInfo is null)
                throw new ArgumentException("Expression is not MemberInfo");

            return memberInfo;
        }

    }
}
