﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Relational.Query.Sources;

namespace Slp.r2rml4net.Storage.Relational.Query
{
    /// <summary>
    /// Relational query
    /// </summary>
    public class RelationalQuery
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationalQuery"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="valueBinders">The value binders.</param>
        public RelationalQuery(CalculusModel model, IEnumerable<IValueBinder> valueBinders)
        {
            Model = model;
            ValueBinders = valueBinders;
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>The model.</value>
        public CalculusModel Model { get; private set; }

        /// <summary>
        /// Gets the value binders.
        /// </summary>
        /// <value>The value binders.</value>
        public IEnumerable<IValueBinder> ValueBinders { get; private set; }
    }
}
