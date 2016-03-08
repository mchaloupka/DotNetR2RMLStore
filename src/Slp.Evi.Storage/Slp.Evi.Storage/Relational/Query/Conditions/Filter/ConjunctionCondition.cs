﻿using System.Collections.Generic;
using System.Diagnostics;

namespace Slp.Evi.Storage.Relational.Query.Conditions.Filter
{
    /// <summary>
    /// The disjunction of conditions
    /// </summary>
    public class ConjunctionCondition
        : IFilterCondition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConjunctionCondition"/> class.
        /// </summary>
        /// <param name="conditions">The inner conditions.</param>
        public ConjunctionCondition(IEnumerable<IFilterCondition> conditions)
        {
            this.InnerConditions = conditions;
        }

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

        /// <summary>
        /// Gets the inner conditions.
        /// </summary>
        /// <value>The inner conditions.</value>
        public IEnumerable<IFilterCondition> InnerConditions { get; private set; }
    }
}
