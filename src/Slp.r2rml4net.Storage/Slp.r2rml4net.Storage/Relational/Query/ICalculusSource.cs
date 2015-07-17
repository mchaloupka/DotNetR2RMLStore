﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Relational.Query.Source;
using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Relational.Query
{
    /// <summary>
    /// Calculus source
    /// </summary>
    public interface ICalculusSource
        : IVisitable<ICalculusSourceVisitor>
    {
        /// <summary>
        /// Gets the provided variables.
        /// </summary>
        /// <value>The variables.</value>
        IEnumerable<ICalculusVariable> Variables { get; }
    }

    /// <summary>
    /// SQL Calculus source (representing directly some SQL table or query)
    /// </summary>
    public interface ISqlCalculusSource
        : ICalculusSource
    {
        /// <summary>
        /// Gets the variable.
        /// </summary>
        /// <param name="name">The SQL name.</param>
        ICalculusVariable GetVariable(string name);
    }
}
