﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Database;
using VDS.RDF;

namespace Slp.r2rml4net.Storage.Relational.Query
{
    /// <summary>
    /// Value binder
    /// </summary>
    public interface IValueBinder
    {
        /// <summary>
        /// Loads the node.
        /// </summary>
        /// <param name="nodeFactory">The node factory.</param>
        /// <param name="rowData">The row data.</param>
        /// <param name="context">The context.</param>
        INode LoadNode(INodeFactory nodeFactory, IQueryResultRow rowData, QueryContext context);

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>The name of the variable.</value>
        string VariableName { get; }
    }
}
