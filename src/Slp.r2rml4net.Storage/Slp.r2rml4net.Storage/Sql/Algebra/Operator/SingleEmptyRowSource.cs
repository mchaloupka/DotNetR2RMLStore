﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sql.Binders;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    /// <summary>
    /// SQL source that returns a single empty row
    /// </summary>
    public class SingleEmptyRowSource : INotSqlOriginalDbSource
    {
        /// <summary>
        /// The value binders
        /// </summary>
        private List<IBaseValueBinder> valueBinders;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleEmptyRowSource"/> class.
        /// </summary>
        public SingleEmptyRowSource()
        {
            valueBinders = new List<IBaseValueBinder>();
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>The columns.</value>
        public IEnumerable<ISqlColumn> Columns
        {
            get { yield break; }
        }

        /// <summary>
        /// Adds the value binder.
        /// </summary>
        /// <param name="valueBinder">The value binder.</param>
        public void AddValueBinder(IBaseValueBinder valueBinder)
        {
            this.valueBinders.Add(valueBinder);
        }

        /// <summary>
        /// Gets the value binders.
        /// </summary>
        /// <value>The value binders.</value>
        public IEnumerable<IBaseValueBinder> ValueBinders
        {
            get { return valueBinders; }
        }

        /// <summary>
        /// Replaces the value binder.
        /// </summary>
        /// <param name="oldBinder">The old binder.</param>
        /// <param name="newBinder">The new binder.</param>
        public void ReplaceValueBinder(IBaseValueBinder oldBinder, IBaseValueBinder newBinder)
        {
            var index = this.valueBinders.IndexOf(oldBinder);

            if (index > -1)
                this.valueBinders[index] = newBinder;
        }

        /// <summary>
        /// Removes the value binder.
        /// </summary>
        /// <param name="valueBinder">The value binder.</param>
        public void RemoveValueBinder(IBaseValueBinder valueBinder)
        {
            var index = this.valueBinders.IndexOf(valueBinder);

            if (index > -1)
                this.valueBinders.RemoveAt(index);
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(ISqlSourceVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
