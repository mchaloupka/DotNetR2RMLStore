﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Patterns
{
    /// <summary>
    /// Union pattern
    /// </summary>
    public class UnionPattern
        : IGraphPattern
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JoinPattern"/> class.
        /// </summary>
        /// <param name="unionedGraphPatterns">The unioned graph patterns.</param>
        public UnionPattern(IEnumerable<IGraphPattern> unionedGraphPatterns)
        {
            this.UnionedGraphPatterns = unionedGraphPatterns;
            this.Variables = this.UnionedGraphPatterns.SelectMany(x => x.Variables)
                .Distinct().ToList();
        }

        /// <summary>
        /// Gets the unioned graph patterns.
        /// </summary>
        /// <value>The unioned graph patterns.</value>
        public IEnumerable<IGraphPattern> UnionedGraphPatterns { get; private set; }

        /// <summary>
        /// Gets the SPARQL variables.
        /// </summary>
        /// <value>The variables.</value>
        public IEnumerable<string> Variables { get; private set; }
    }
}
