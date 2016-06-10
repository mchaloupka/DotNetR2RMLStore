﻿using System.Collections.Generic;
using Slp.Evi.Storage.Sparql.Algebra.Expressions;
using Slp.Evi.Storage.Utils;

namespace Slp.Evi.Storage.Sparql.Algebra
{
    /// <summary>
    /// Public interface for all sparql expressions
    /// </summary>
    public interface ISparqlExpression
        : IVisitable<ISparqlExpressionVisitor>
    {
        /// <summary>
        /// Gets the needed variables to evaluate the expression.
        /// </summary>
        IEnumerable<string> NeededVariables { get; }
    }
}
