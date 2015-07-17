﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Relational.Query.Condition
{
    /// <summary>
    /// Class representing condition: assigning a tuple from a source
    /// </summary>
    public class TupleFromSourceCondition
        : ICondition
    {
        /// <summary>
        /// Gets or sets the calculus variables.
        /// </summary>
        /// <value>The calculus variables.</value>
        public IEnumerable<ICalculusVariable> CalculusVariables { get; set; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>The source.</value>
        public ICalculusSource Source { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TupleFromSourceCondition"/> class.
        /// </summary>
        /// <param name="calculusVariables">The calculus variables.</param>
        /// <param name="source">The source.</param>
        public TupleFromSourceCondition(IEnumerable<ICalculusVariable> calculusVariables, ICalculusSource source)
        {
            CalculusVariables = calculusVariables;
            Source = source;
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IConditionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
