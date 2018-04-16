﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.Evi.Storage.Sparql.Algebra.Expressions
{
    /// <summary>
    /// The LangMatches expression
    /// </summary>
    /// <seealso cref="Slp.Evi.Storage.Sparql.Algebra.ISparqlCondition" />
    public class LangMatchesExpression
        : ISparqlCondition
    {
        /// <summary>
        /// Gets the language expression (left operand).
        /// </summary>
        public ISparqlExpression LanguageExpression { get; }

        /// <summary>
        /// Gets the language range expression (right operand).
        /// </summary>
        public ISparqlExpression LanguageRangeExpression { get; }

        public LangMatchesExpression(ISparqlExpression languageExpression, ISparqlExpression languageRangeExpression)
        {
            LanguageExpression = languageExpression;
            LanguageRangeExpression = languageRangeExpression;
            NeededVariables = LanguageExpression.NeededVariables.Union(LanguageRangeExpression.NeededVariables).Distinct().ToArray();
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        public object Accept(ISparqlExpressionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <inheritdoc />
        public IEnumerable<string> NeededVariables { get; }
    }
}
