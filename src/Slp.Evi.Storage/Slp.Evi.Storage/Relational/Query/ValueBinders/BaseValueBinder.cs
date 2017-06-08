﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using Slp.Evi.Storage.Database;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Sparql.Types;
using Slp.Evi.Storage.Utils;
using TCode.r2rml4net;
using TCode.r2rml4net.Extensions;
using TCode.r2rml4net.Mapping;
using VDS.RDF;

namespace Slp.Evi.Storage.Relational.Query.ValueBinders
{
    /// <summary>
    /// Representation of the base value binder
    /// </summary>
    public class BaseValueBinder
        : IValueBinder
    {
        /// <summary>
        /// The needed variables
        /// </summary>
        private readonly Dictionary<string, ICalculusVariable> _variables;

        /// <summary>
        /// The load node function
        /// </summary>
        private Func<INodeFactory, IQueryResultRow, IQueryContext, INode> _loadNodeFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseValueBinder"/> class.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="termMap">The term map.</param>
        /// <param name="source">The source.</param>
        /// <param name="typeCache">The type cache.</param>
        public BaseValueBinder(string variableName, ITermMap termMap, ISqlCalculusSource source, ITypeCache typeCache)
        {
            VariableName = variableName;
            TermMap = termMap;
            _variables = new Dictionary<string, ICalculusVariable>();
            _loadNodeFunc = null;
            Type = typeCache.GetValueType(termMap);
            
            if (termMap.IsConstantValued)
            {
                // No columns needed
            }
            else if (termMap.IsColumnValued)
            {
                _variables.Add(termMap.ColumnName, source.GetVariable(termMap.ColumnName));
            }
            else if (termMap.IsTemplateValued)
            {
                var template = termMap.Template;

                var templateProcessor = new TemplateProcessor();
                TemplateParts = templateProcessor.ParseTemplate(template).ToArray();

                foreach (var part in TemplateParts.Where(x => x.IsColumn))
                {
                    _variables.Add(part.Column, source.GetVariable(part.Column));
                }
            }
            else
            {
                throw new Exception("Mapping can be only constant, column or template valued");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseValueBinder"/> class by copying another <see cref="BaseValueBinder"/>
        /// while replacing some of its columns
        /// </summary>
        /// <param name="baseValueBinder">The other base value binder.</param>
        /// <param name="calculusVariableSelection">The calculus variable selection function.</param>
        public BaseValueBinder(BaseValueBinder baseValueBinder, Func<ICalculusVariable, ICalculusVariable> calculusVariableSelection)
        {
            VariableName = baseValueBinder.VariableName;
            TermMap = baseValueBinder.TermMap;
            TemplateParts = baseValueBinder.TemplateParts;
            Type = baseValueBinder.Type;

            _variables = new Dictionary<string, ICalculusVariable>();
            _loadNodeFunc = null;

            foreach (var variableName in baseValueBinder._variables.Keys)
            {
                _variables.Add(variableName, calculusVariableSelection(baseValueBinder._variables[variableName]));
            }
        }

        /// <summary>
        /// Gets the term map.
        /// </summary>
        /// <value>The term map.</value>
        public ITermMap TermMap { get; }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>The name of the variable.</value>
        public string VariableName { get; }

        /// <summary>
        /// Gets the needed calculus variables to calculate the value.
        /// </summary>
        /// <value>The needed calculus variables.</value>
        public IEnumerable<ICalculusVariable> NeededCalculusVariables => _variables.Values.Distinct();

        /// <summary>
        /// Gets the template parts.
        /// </summary>
        /// <value>The template parts.</value>
        public IEnumerable<ITemplatePart> TemplateParts { get; }

        /// <summary>
        /// The type
        /// </summary>
        public IValueType Type { get; }

        /// <summary>
        /// Gets the calculus variable.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        public ICalculusVariable GetCalculusVariable(string columnName)
        {
            return _variables[columnName];
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IValueBinderVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets the calculus variable.
        /// </summary>
        /// <param name="columnName">The column name.</param>
        /// <exception cref="System.Exception">Cannot get variable that is not requested for evaluation</exception>
        public ICalculusVariable GetVariable(string columnName)
        {
            if (_variables.ContainsKey(columnName))
            {
                return _variables[columnName];
            }
            else
            {
                throw new Exception("Cannot get variable that is not requested for evaluation");
            }
        }

        /// <summary>
        /// Loads the node.
        /// </summary>
        /// <param name="nodeFactory">The node factory.</param>
        /// <param name="rowData">The row data.</param>
        /// <param name="context">The context.</param>
        public INode LoadNode(INodeFactory nodeFactory, IQueryResultRow rowData, IQueryContext context)
        {
            if (_loadNodeFunc == null)
                _loadNodeFunc = GenerateLoadNodeFunc(context);

            return _loadNodeFunc(nodeFactory, rowData, context);
        }

        #region GenerateLoadNodeFunc

        /// <summary>
        /// Generates the load node function.
        /// </summary>
        /// <param name="queryContext">The query context</param>
        /// <returns>Generated function.</returns>
        /// <exception cref="System.Exception">Term map must be either constant, column or template valued</exception>
        private Func<INodeFactory, IQueryResultRow, IQueryContext, INode> GenerateLoadNodeFunc(IQueryContext queryContext)
        {
            Expression<Func<INodeFactory, IQueryResultRow, IQueryContext, INode>> expr;

            if (TermMap.IsConstantValued)
            {
                expr = GenerateLoadNodeFuncFromConstant();
            }
            else if (TermMap.IsColumnValued)
            {
                expr = GenerateLoadNodeFuncFromColumn(queryContext);
            }
            else if (TermMap.IsTemplateValued)
            {
                expr = GenerateLoadNodeFuncFromTemplate(queryContext);
            }
            else
            {
                throw new Exception("Term map must be either constant, column or template valued");
            }

            return expr.Compile();
        }

        /// <summary>
        /// Generates the load node function from template.
        /// </summary>
        /// <param name="queryContext">The query context</param>
        private Expression<Func<INodeFactory, IQueryResultRow, IQueryContext, INode>> GenerateLoadNodeFuncFromTemplate(IQueryContext queryContext)
        {
            ParameterExpression nodeFactory = Expression.Parameter(typeof(INodeFactory), "nodeFactory");
            ParameterExpression row = Expression.Parameter(typeof(IQueryResultRow), "row");
            ParameterExpression context = Expression.Parameter(typeof(IQueryContext), "context");

            ParameterExpression valVar = Expression.Parameter(typeof(string), "val");

            List<Expression> expressions = new List<Expression>
            {
                Expression.Assign(valVar,
                    GenerateReplaceColumnReferencesFunc(row, TermMap.TermType.IsURI, queryContext)),
                Expression.Condition(Expression.Equal(valVar, Expression.Constant(null, typeof (string))),
                    Expression.Constant(null, typeof (INode)),
                    GenerateTermForValueFunc(nodeFactory, valVar, context))
            };
            // Change to generate term for value

            var block = Expression.Block(typeof(INode), new[] { valVar }, expressions);
            return Expression.Lambda<Func<INodeFactory, IQueryResultRow, IQueryContext, INode>>(block, nodeFactory, row, context);
        }

        /// <summary>
        /// Generates the replace column references function.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="escape">if set to <c>true</c> the value should be escaped.</param>
        /// <param name="queryContext">The query context</param>
        private Expression GenerateReplaceColumnReferencesFunc(ParameterExpression row, bool escape, IQueryContext queryContext)
        {
            List<Expression> expressions = new List<Expression>();
            ParameterExpression sbVar = Expression.Parameter(typeof(StringBuilder), "sb");
            ParameterExpression replacedVar = Expression.Parameter(typeof(string), "replaced");

            var endLabel = Expression.Label(typeof(string), "returnLabel");

            var appendMethod = typeof(StringBuilder).GetMethod("Append", new[] { typeof(string) });
            expressions.Add(Expression.Assign(sbVar, Expression.New(typeof(StringBuilder))));

            foreach (var part in TemplateParts)
            {
                if (part.IsText)
                {
                    expressions.Add(Expression.Call(sbVar, appendMethod, Expression.Constant(part.Text, typeof(string))));
                }
                else if (part.IsColumn)
                {
                    expressions.Add(Expression.Assign(replacedVar, GenerateReplaceColumnReferenceFunc(row, GetVariable(part.Column), escape, queryContext)));
                    expressions.Add(Expression.IfThen(Expression.Equal(replacedVar, Expression.Constant(null, typeof(string))), Expression.Return(endLabel, Expression.Constant(null, typeof(string)))));
                    expressions.Add(Expression.Call(sbVar, appendMethod, replacedVar));
                }
            }

            expressions.Add(Expression.Label(endLabel, Expression.Call(sbVar, "ToString", new Type[0])));

            return Expression.Block(typeof(string), new[] { sbVar, replacedVar }, expressions);
        }

        /// <summary>
        /// Generates the replace column reference function.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <param name="escape">if set to <c>true</c> the value should be escaped.</param>
        /// <param name="queryContext">The query context</param>
        private Expression GenerateReplaceColumnReferenceFunc(ParameterExpression row, ICalculusVariable column, bool escape, IQueryContext queryContext)
        {
            var dbColVar = Expression.Parameter(typeof(IQueryResultColumn), "dbCol");
            var valueVar = Expression.Parameter(typeof(string), "value");
            var endLabel = Expression.Label(typeof(string), "returnLabel");

            var columnName = queryContext.QueryNamingHelpers.GetVariableName(null, column);

            List<Expression> expressions = new List<Expression>
            {
                Expression.Assign(dbColVar,
                    Expression.Call(row, "GetColumn", new Type[0], Expression.Constant(columnName))),
                Expression.Assign(valueVar, Expression.Property(dbColVar, "StringValue")),
                Expression.IfThen(Expression.Equal(valueVar, Expression.Constant(null, typeof (string))),
                    Expression.Return(endLabel, Expression.Constant(null, typeof (string)))),
                escape
                    ? Expression.Label(endLabel,
                        Expression.Call(typeof (MappingHelper), "UrlEncode", new Type[0], valueVar))
                    : Expression.Label(endLabel, valueVar)
            };


            return Expression.Block(typeof(string), new[] { dbColVar, valueVar }, expressions);
        }

        /// <summary>
        /// Generates the load node function from constant.
        /// </summary>
        /// <exception cref="System.Exception">
        /// Object map's value must be IRI or literal.
        /// or
        /// Constant must be uri valued or an object map
        /// </exception>
        private Expression<Func<INodeFactory, IQueryResultRow, IQueryContext, INode>> GenerateLoadNodeFuncFromConstant()
        {
            if (TermMap is IUriValuedTermMap uriValuedTermMap)
            {
                var uri = uriValuedTermMap.URI;
                return (fact, row, context) => fact.CreateUriNode(uri);
            }
            else if (TermMap is IObjectMap objectMap)
            {
                if (objectMap.URI != null)
                    return (fact, row, context) => fact.CreateUriNode(objectMap.URI);
                else if (objectMap.Literal != null)
                {
                    var parsedParts = objectMap.Parsed();
                    var value = parsedParts.Value;
                    return (fact, row, context) => ((ILiteralValueType)Type).CreateLiteralNode(fact, value);
                }
                else
                    throw new Exception("Object map's value must be IRI or literal.");
            }
            else
            {
                throw new Exception("Constant must be uri valued or an object map");
            }
        }

        /// <summary>
        /// Generates the term for value function.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="value">The value.</param>
        /// <param name="context">The query context.</param>
        /// <exception cref="System.Exception"></exception>
        private Expression GenerateTermForValueFunc(ParameterExpression factory, ParameterExpression value, ParameterExpression context)
        {
            var endLabel = Expression.Label(typeof(INode), "returnLabel");
            List<Expression> expressions = new List<Expression>();
            ParameterExpression nodeVar = Expression.Parameter(typeof(INode), "node");

            expressions.Add(Expression.Assign(nodeVar, Expression.Constant(null, typeof(INode))));

            expressions.Add(Expression.IfThen(Expression.Equal(value, Expression.Constant(null, typeof(object))),
                Expression.Return(endLabel, Expression.Constant(null, typeof(INode)))));

            var termType = TermMap.TermType;

            if (termType.IsURI)
            {
                expressions.Add(Expression.Assign(nodeVar,
                    Expression.Call(typeof(BaseValueBinder), nameof(GenerateUriTermForValue), new Type[0],
                        value,
                        factory,
                        context,
                        Expression.Constant(TermMap.BaseUri, typeof(Uri)))));
            }
            else if (termType.IsBlankNode)
            {
                expressions.Add(Expression.Assign(nodeVar, GenerateBlankNodeForValueFunc(factory, value, context)));
            }
            else if (termType.IsLiteral)
            {
                expressions.Add(Expression.Assign(nodeVar, GenerateTermForLiteralFunc(factory, value)));
            }
            else
            {
                throw new Exception("Unhandled term type");
            }

            expressions.Add(Expression.Label(endLabel, nodeVar));

            return Expression.Block(typeof(INode), new[] { nodeVar }, expressions);
        }

        /// <summary>
        /// Generates the blank node for value function.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="value">The value.</param>
        /// <param name="context">The query context.</param>
        private Expression GenerateBlankNodeForValueFunc(ParameterExpression factory, ParameterExpression value, ParameterExpression context)
        {
            if (TermMap is ISubjectMap)
                return Expression.Call(context, "GetBlankNodeSubjectForValue", new Type[0], factory, value);
            else
                return Expression.Call(context, "GetBlankNodeObjectForValue", new Type[0], factory, value);
        }

        /// <summary>
        /// Generates the term for literal function.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.Exception">
        /// Term map cannot be of term type literal
        /// or
        /// Literal term map cannot have both language tag and datatype set
        /// </exception>
        private Expression GenerateTermForLiteralFunc(ParameterExpression factory, ParameterExpression value)
        {
            if (value == null)
                return null;

            if (!(Type is ILiteralValueType))
                throw new InvalidOperationException("It is not possible to generate literal for non literal type");

            var literalValueType = (ILiteralValueType) Type;

            return Expression.Call(Expression.Constant(literalValueType, typeof(ILiteralValueType)), "CreateLiteralNode", new Type[0], factory, value);
        }

        /// <summary>
        /// Generates the URI term for value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="factory">The factory.</param>
        /// <param name="context">The context.</param>
        /// <param name="baseUri">The base URI.</param>
        /// <exception cref="System.Exception">
        /// Now the uri must be absolute
        /// or
        /// </exception>
        private static INode GenerateUriTermForValue(string value, INodeFactory factory, IQueryContext context, Uri baseUri)
        {
            try
            {
                var uri = new Uri(value, UriKind.RelativeOrAbsolute);

                if (!uri.IsAbsoluteUri)
                {
                    uri = ConstructAbsoluteUri(value, baseUri);
                }

                if (uri.IsAbsoluteUri)
                {
                    uri.LeaveDotsAndSlashesEscaped();
                    return factory.CreateUriNode(uri);
                }
                else
                {
                    throw new Exception("Now the uri must be absolute");
                }
            }
            catch (Exception)
            {
                throw new Exception($"Value {value} is invalid uri");
            }
        }

        /// <summary>
        /// Constructs the absolute URI.
        /// </summary>
        /// <param name="relativePart">The relative part.</param>
        /// <param name="baseUri">The base URI.</param>
        /// <returns>Uri.</returns>
        /// <exception cref="System.Exception">The relative IRI cannot contain any . or .. parts</exception>
        private static Uri ConstructAbsoluteUri(string relativePart, Uri baseUri)
        {
            if (relativePart.Split('/').Any(seg => seg == "." || seg == ".."))
                throw new Exception("The relative IRI cannot contain any . or .. parts");

            return new Uri(baseUri + relativePart);
        }

        /// <summary>
        /// Asserts the no illegal characters.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="System.Exception"></exception>
        private static void AssertNoIllegalCharacters(Uri value)
        {
            IEnumerable<char> disallowedChars = string.Empty;
            IEnumerable<string> segments = value.IsAbsoluteUri ? value.Segments : new[] { value.OriginalString };

            foreach (var segment in segments)
            {
                if (segment.Any(chara => !MappingHelper.IsIUnreserved(chara)))
                {
                    disallowedChars =
                        disallowedChars.Union(
                            segment.Where(chara => chara != '/' && !MappingHelper.IsIUnreserved(chara)));
                }
            }

            var joinedChars = string.Join(",", disallowedChars.Select(c => $"'{c}'"));
            if (joinedChars.Any())
            {
                const string format = "Column value is not escaped and thus cannot contain these disallowed characters: {0}";
                var reason = string.Format(format, joinedChars);
                throw new Exception(reason);
            }
        }

        /// <summary>
        /// Generates the load node function from column.
        /// </summary>
        /// <param name="queryContext">The query context</param>
        private Expression<Func<INodeFactory, IQueryResultRow, IQueryContext, INode>> GenerateLoadNodeFuncFromColumn(IQueryContext queryContext)
        {
            ParameterExpression nodeFactory = Expression.Parameter(typeof(INodeFactory), "nodeFactory");
            ParameterExpression row = Expression.Parameter(typeof(IQueryResultRow), "row");
            ParameterExpression context = Expression.Parameter(typeof(IQueryContext), "context");

            var column = NeededCalculusVariables.Single();
            var columnName = queryContext.QueryNamingHelpers.GetVariableName(null, column);

            ParameterExpression dbColVar = Expression.Parameter(typeof(IQueryResultColumn), "dbCol");
            ParameterExpression valVar = Expression.Parameter(typeof(string), "value");

            List<Expression> expressions = new List<Expression>
            {
                Expression.Assign(dbColVar,
                    Expression.Call(row, "GetColumn", new Type[0], Expression.Constant(columnName, typeof (string)))),
                Expression.Assign(valVar, Expression.Property(dbColVar, "StringValue"))
            };

            if (TermMap.TermType.IsLiteral)
            {
                expressions.Add(GenerateTermForLiteralFunc(nodeFactory, valVar));
            }
            else
            {
                expressions.Add(Expression.Call(typeof(BaseValueBinder), nameof(AssertNoIllegalCharacters), new Type[0],
                    Expression.New(typeof(Uri).GetConstructor(new[] { typeof(string), typeof(UriKind) }),
                        valVar,
                        Expression.Constant(UriKind.RelativeOrAbsolute, typeof(UriKind)))));
                expressions.Add(GenerateTermForValueFunc(nodeFactory, valVar, context));
            }

            var block = Expression.Block(typeof(INode), new[] { dbColVar, valVar }, expressions);
            return Expression.Lambda<Func<INodeFactory, IQueryResultRow, IQueryContext, INode>>(block, nodeFactory, row, context);
        }
        #endregion
    }
}