#region copyright
// Copyright 2017 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Linq.Expressions;
using System.Linq;
using System;
using System.Reflection;
using System.Collections.Generic;
using SimpleIdentityServer.EventStore.EF.Extensions;

namespace SimpleIdentityServer.EventStore.EF.Parsers
{
    public class BuildLambdaExpressionResult
    {
        public BuildLambdaExpressionResult(LambdaExpression lambda, ParameterExpression parameter, Type type)
        {
            Lambda = lambda;
            Parameter = parameter;
            TargetType = type;
        }

        public LambdaExpression Lambda;
        public ParameterExpression Parameter;
        public Type TargetType;
    }

    public static class ExpressionBuilder
    {
        public static BuildLambdaExpressionResult BuildLambdaExpression(string selector, Type type)
        {
            return BuildLambdaExpression(selector, type);
        }

        public static BuildLambdaExpressionResult BuildLambdaExpression(string selector, Type type, string parameterName)
        {
            var fieldNames = selector.Split('|');
            var propertyArg = Expression.Parameter(type, parameterName);
            if (fieldNames.Count() == 1)
            {
                
                var propertySelector = Expression.Property(propertyArg, selector);
                return new BuildLambdaExpressionResult(Expression.Lambda(propertySelector, new ParameterExpression[] { propertyArg }), propertyArg, type.GetProperty(selector).PropertyType);
            }

            var expressions = fieldNames.Select(s => Expression.Property(propertyArg, s));
            var sourceProperties = fieldNames.ToDictionary(name => name, name => type.GetProperty(name));
            var anonType = ReflectionHelper.CreateNewAnonymousType(type, fieldNames);
            var newExpr = Expression.New(anonType.DeclaredConstructors.First(), expressions);
            return new BuildLambdaExpressionResult(Expression.Lambda(newExpr, new ParameterExpression[] { propertyArg }), propertyArg, anonType.AsType());
        }
        
        public static NewExpression BuildNew(string selector, Type type, string parameterName)
        {
            var fieldNames = selector.GetParameters();
            var anonType = ReflectionHelper.CreateNewAnonymousType(type, fieldNames);
            return BuildAnonymous(fieldNames, type, anonType, Expression.Parameter(type, parameterName));
        }

        public static NewExpression BuildNew(string selector, Type type, TypeInfo anonType, string parameterName)
        {
            var fieldNames = selector.GetParameters();
            return BuildAnonymous(fieldNames, type, anonType, Expression.Parameter(type, parameterName));
        }

        public static NewExpression BuildNew(string selector, Type type, TypeInfo anonType, ParameterExpression arg)
        {
            var fieldNames = selector.GetParameters();
            return BuildAnonymous(fieldNames, type, anonType, arg);
        }

        private static NewExpression BuildAnonymous(IEnumerable<string> fieldNames, Type type, TypeInfo anonType, ParameterExpression arg)
        {
            var expressions = fieldNames.Select(s => Expression.Property(arg, s));
            var sourceProperties = fieldNames.ToDictionary(name => name, name => type.GetProperty(name));
            return Expression.New(anonType.DeclaredConstructors.First(), expressions);
        }
    }
}
