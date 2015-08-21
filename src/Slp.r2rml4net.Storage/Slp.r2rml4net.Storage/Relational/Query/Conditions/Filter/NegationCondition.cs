﻿using System.Diagnostics;

namespace Slp.r2rml4net.Storage.Relational.Query.Conditions.Filter
{
    /// <summary>
    /// Class NegationCondition.
    /// </summary>
    public class NegationCondition
        : IFilterCondition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NegationCondition"/> class.
        /// </summary>
        /// <param name="condition">The inner condition.</param>
        public NegationCondition(IFilterCondition condition)
        {
            InnerCondition = condition;
        }

        /// <summary>
        /// Gets the inner condition.
        /// </summary>
        /// <value>The inner condition.</value>
        public IFilterCondition InnerCondition { get; private set; }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IFilterConditionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
