﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AngleSharp.Common;
using Microsoft.FSharp.Collections;
using Slp.Evi.Common;
using Slp.Evi.Database;
using Slp.Evi.Relational.Algebra;
using Slp.Evi.Storage.Common.FSharpExtensions;
using Slp.Evi.Storage.MsSql.Database;

namespace Slp.Evi.Storage.MsSql.QueryWriter
{
    class MsSqlQueryWriter
        : ISqlDatabaseWriter<MsSqlQuery>
    {
        /// <inheritdoc />
        public MsSqlQuery WriteQuery(SqlQuery query)
        {
            StringBuilder sb = new StringBuilder();
            WriteQuery(sb, query);
            return new MsSqlQuery(sb.ToString());
        }

        private static void WriteQuery(StringBuilder sb, SqlQuery sqlQuery)
        {
            // The ORDER BY and DISTINCT may cause issue if it is in the same query level
            if (sqlQuery.IsDistinct && !sqlQuery.Ordering.IsEmpty)
            {
                sb.Append("SELECT * FROM (");
            }

            var variablesMappings = new Dictionary<string, List<Variable>>();
            foreach (var variable in sqlQuery.Variables)
            {
                if (sqlQuery.NamingProvider.TryGetVariableName(variable, out var varName))
                {
                    if (!variablesMappings.TryGetValue(varName, out var varList))
                    {
                        varList = new List<Variable>();
                        variablesMappings.Add(varName, varList);
                    }

                    varList.Add(variable);
                }
                else
                {
                    throw new InvalidOperationException("Ended up with a variable without name");
                }
            }

            if (variablesMappings.Count == 0)
            {
                variablesMappings.Add("c", new List<Variable>());
            }

            var variables = variablesMappings.Keys.OrderBy(x => x).ToList();

            var innerLimit = sqlQuery.InnerQueries.Length == 1 && sqlQuery.Ordering.IsEmpty && sqlQuery.Offset.IsNone()
                ? sqlQuery.Limit.ToNullable()
                : null;

            var firstInnerQuery = true;
            foreach (var innerQuery in sqlQuery.InnerQueries)
            {
                if (firstInnerQuery)
                {
                    firstInnerQuery = false;
                }
                else
                {
                    sb.Append(sqlQuery.IsDistinct ? " UNION " : " UNION ALL ");
                }

                WriteInnerQuery(sb, innerQuery, variables, variablesMappings, sqlQuery.IsDistinct, innerLimit);
            }

            if (sqlQuery.IsDistinct && !sqlQuery.Ordering.IsEmpty)
            {
                sb.Append(")");
            }

            // TODO: Add ordering and offset and limit
            throw new NotImplementedException();
        }

        private static void WriteInnerQuery(StringBuilder sb, QueryContent query, List<string> variables, Dictionary<string, List<Variable>> variablesMappings, bool isDistinct, int? innerLimit)
        {
            sb.Append("SELECT");

            if (isDistinct)
            {
                sb.Append(" DISTINCT");
            }

            if (innerLimit.HasValue)
            {
                sb.Append(" TOP");
                sb.Append(innerLimit.Value);
            }

            if (query.IsSelectQuery)
            {
                WriteInnerSelectQueryContent(sb, ((QueryContent.SelectQuery) query).Item, variables, variablesMappings);
            }
            else if (query.IsNoResultQuery)
            {
                WriteInnerNoResultQueryContent(sb, variables, variablesMappings);
            }
            else if (query.IsSingleEmptyResultQuery)
            {
                WriteInnerSingleEmptyResultQueryContent(sb, variables, variablesMappings);
            }
            else
            {
                throw new ArgumentException("Produced query does not have supported type", nameof(query));
            }
        }

        private static void WriteInnerSingleEmptyResultQueryContent(StringBuilder sb, List<string> variables, Dictionary<string, List<Variable>> variablesMappings)
        {
            throw new NotImplementedException();
        }

        private static void WriteInnerNoResultQueryContent(StringBuilder sb, List<string> variables, Dictionary<string, List<Variable>> variablesMappings)
        {
            throw new NotImplementedException();
        }

        private static void WriteInnerSelectQueryContent(StringBuilder sb, InnerQuery query, List<string> variables, Dictionary<string, List<Variable>> variablesMappings)
        {
            var isFirstVariable = true;
            foreach (var variableName in variables)
            {
                if (isFirstVariable)
                {
                    sb.Append(" ");
                    isFirstVariable = false;
                }
                else
                {
                    sb.Append(", ");
                }

                var providedVariables = variablesMappings.GetOrDefault(variableName, new List<Variable>())
                    .Where(var => query.ProvidedVariables.Contains(var))
                    .ToList();

                if (providedVariables.Count > 1)
                {
                    throw new InvalidOperationException($"There are more provided variables for name {variableName}");
                }
                else if (providedVariables.Count == 1)
                {
                    var variable = providedVariables[0];
                    WriteVariable(sb, query, variable);
                }
                else
                {
                    WriteExpression(sb, query, Expression.Null);
                }

                sb.Append(" AS ");
                sb.Append(variableName);
            }

            sb.Append(" FROM");

            var isFirstSource = true;
            foreach (var innerSource in query.Sources)
            {
                sb.Append(!isFirstSource ? " INNER JOIN " : " ");

                WriteInnerSource(sb, innerSource);

                sb.Append(" AS ");

                if (query.NamingProvider.TryGetSourceName(innerSource, out var sourceName))
                {
                    sb.Append(sourceName);
                }
                else
                {
                    throw new InvalidOperationException($"Name for source has not been found. Source: {innerSource}");
                }

                if (!isFirstSource)
                {
                    sb.Append(" ON 1=1");
                }
                else
                {
                    isFirstSource = false;
                }
            }

            foreach (var leftJoined in query.LeftJoinedSources)
            {
                sb.Append(" LEFT JOIN ");
                WriteInnerSource(sb, leftJoined.Item1);
                sb.Append(" ON ");
                WriteCondition(sb, query, leftJoined.Item2);
            }

            var isFirstCondition = true;
            foreach (var condition in query.Filters)
            {
                if (!isFirstCondition)
                {
                    sb.Append(" AND ");
                }
                else
                {
                    sb.Append(" WHERE ");
                    isFirstCondition = false;
                }

                WriteCondition(sb, query, condition);
            }
        }

        private static void WriteInnerSource(StringBuilder sb, InnerSource innerSource)
        {
            if (innerSource.IsInnerTable)
            {
                var table = (InnerSource.InnerTable) innerSource;
                sb.Append(table.Item1.Schema.Name);
            }
            else if (innerSource.IsInnerSource)
            {
                sb.Append("(");
                var source = (InnerSource.InnerSource) innerSource;
                WriteQuery(sb, source.Item);
                sb.Append(")");
            }
            else
            {
                throw new InvalidOperationException("Source has to be either a table or inner source");
            }
        }

        private static void WriteVariable(StringBuilder sb, InnerQuery query, Variable variable)
        {
            if (query.NamingProvider.TryGetSource(variable, out var innerSource))
            {
                WriteSourcedVariable(sb, query, innerSource, variable);
            }
            else
            {
                if (variable.IsAssigned)
                {
                    var assignedVariable = ((Variable.Assigned) variable).Item;
                    var assignment = query.Assignments.Single(x => x.Variable == assignedVariable);
                    WriteExpression(sb, query, assignment.Expression);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Variable {variable} has not been found in any source and still, it is not assigned.");
                }
            }
        }

        private static void WriteSourcedVariable(StringBuilder sb, InnerQuery query, InnerSource innerSource, Variable variable)
        {
            if (query.NamingProvider.TryGetSourceName(innerSource, out var innerSourceName))
            {
                sb.Append(innerSourceName);
                sb.Append(".");

                if (innerSource.NamingProvider.TryGetVariableName(variable, out var variableName))
                {
                    sb.Append(variableName);
                }
                else 
                {
                    throw new InvalidOperationException("Cannot find name for a variable from source");
                }
            }
            else
            {
                throw new InvalidOperationException("Cannot find name for an existing variable source");
            }
        }

        private static void WriteExpression(StringBuilder sb, InnerQuery query, Expression expression)
        {
            var writer = new MsSqlExpressionWriter(sb, query);
            SqlDatabaseWriterHelper.ProcessExpression(writer, expression);
        }

        private static void WriteCondition(StringBuilder sb, InnerQuery query, Condition condition)
        {
            var writer = new MsSqlExpressionWriter(sb, query);
            SqlDatabaseWriterHelper.ProcessCondition(writer, condition);
        }

        private class MsSqlExpressionWriter: SqlDatabaseWriterHelper.ISqlExpressionWriter
        {
            private readonly StringBuilder _sb;
            private readonly InnerQuery _query;

            public MsSqlExpressionWriter(StringBuilder sb, InnerQuery query)
            {
                _sb = sb;
                _query = query;
            }

            private void ProcessCondition(Condition condition)
            {
                SqlDatabaseWriterHelper.ProcessCondition(this, condition);
            }

            private void ProcessExpression(Expression expression)
            {
                SqlDatabaseWriterHelper.ProcessExpression(this, expression);
            }

            /// <inheritdoc />
            public void WriteNull()
            {
                _sb.Append("NULL");
            }

            /// <inheritdoc />
            public void WriteBinaryNumericOperation(Algebra.ArithmeticOperator @operator, Expression leftOperand, Expression rightOperand)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public void WriteSwitch(FSharpList<CaseStatement> caseStatements)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public void WriteCoalesce(FSharpList<Expression> expressions)
            {
                _sb.Append("COALESCE(");

                var isFirstExpression = true;
                foreach (var expression in expressions)
                {
                    if (isFirstExpression)
                    {
                        isFirstExpression = false;
                    }
                    else
                    {
                        _sb.Append(", ");
                    }

                    ProcessExpression(expression);
                }

                _sb.Append(")");
            }

            /// <inheritdoc />
            public void WriteVariable(Variable variable)
            {
                MsSqlQueryWriter.WriteVariable(_sb, _query, variable);
            }

            /// <inheritdoc />
            public void WriteIriSafeVariable(Variable variable)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public void WriteConstant(Literal literal)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public void WriteConcatenation(FSharpList<Expression> expressions)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public void WriteBooleanExpression(Condition condition)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public void WriteTrue()
            {
                _sb.Append("1=1");
            }

            /// <inheritdoc />
            public void WriteFalse()
            {
                _sb.Append("1=0");
            }

            /// <inheritdoc />
            public void WriteComparison(Algebra.Comparisons comparison, Expression leftOperand, Expression rightOperand)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public void WriteConjunction(FSharpList<Condition> conditions)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public void WriteDisjunction(FSharpList<Condition> conditions)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public void WriteEqualVariableTo(Variable variable, Literal literal)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public void WriteEqualVariables(Variable leftVariable, Variable rightVariable)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public void WriteIsNull(Variable variable)
            {
                WriteVariable(variable);
                _sb.Append(" IS NULL");
            }

            /// <inheritdoc />
            public void WriteLanguageMatch(Expression langExpression, Expression langRangeExpression)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public void WriteLikeMatch(Expression expression, string pattern)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public void WriteNot(Condition condition)
            {
                _sb.Append("NOT ");
                ProcessCondition(condition);
            }
        }
    }
}
